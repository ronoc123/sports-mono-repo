import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  computed,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { MarketplaceFacade } from '@sports-ui/marketplace-data-access';
import { AuthFacade } from '@sports-ui/auth-data-access';
import { LeaguesFacade } from '@sports-ui/league-data-access';
import { ListingDto } from '@sports-ui/marketplace-data-access';

@Component({
  selector: 'lib-marketplace',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './marketplace.component.html',
  styleUrl: './marketplace.component.css',
})
export class MarketplaceComponent implements OnInit, OnDestroy {
  private readonly facade = inject(MarketplaceFacade);
  private readonly authFacade = inject(AuthFacade);
  private readonly leaguesFacade = inject(LeaguesFacade);

  private signalRSubscriptions: Subscription[] = [];

  // ── Facade signals ────────────────────────────────────────────────────────
  readonly listings = this.facade.listings;
  readonly selectedListing = this.facade.selectedListing;
  readonly availableBalance = this.facade.availableBalance;
  readonly isLoadingListings = this.facade.isLoadingListings;
  readonly isLoadingDetail = this.facade.isLoadingDetail;
  readonly isBidding = this.facade.isBidding;
  readonly error = this.facade.error;
  readonly bidError = this.facade.bidError;
  readonly lastOutbid = this.facade.lastOutbid;
  readonly lastSettled = this.facade.lastSettled;

  // ── Local UI state ────────────────────────────────────────────────────────
  readonly bidAmount = signal<number | null>(null);
  readonly showBuyNowConfirm = signal(false);
  readonly bidValidationError = signal<string | null>(null);
  readonly searchQuery = signal<string>('');
  readonly timeFilter = signal<'all' | '1h' | '6h' | '24h'>('all');

  readonly timeFilterOptions = [
    { value: 'all' as const, label: 'All' },
    { value: '1h' as const, label: '<1h' },
    { value: '6h' as const, label: '<6h' },
    { value: '24h' as const, label: '<24h' },
  ];

  readonly minBid = computed(() => {
    const listing = this.selectedListing();
    if (!listing) return 0;
    return Math.ceil(listing.currentBid * 1.05);
  });

  readonly isOwnListing = computed(() => {
    const listing = this.selectedListing();
    const userId = this.authFacade.user()?.id;
    return listing?.sellerId === userId;
  });

  readonly timeRemaining = computed(() => {
    const listing = this.selectedListing();
    if (!listing) return '';
    return formatTimeRemaining(listing.expiresAt);
  });

  readonly filteredListings = computed(() => {
    let result = this.listings();
    const q = this.searchQuery().toLowerCase();
    if (q) result = result.filter(l => l.cardName.toLowerCase().includes(q));
    const tf = this.timeFilter();
    if (tf !== 'all') {
      const hours = tf === '1h' ? 1 : tf === '6h' ? 6 : 24;
      const cutoff = Date.now() + hours * 3600000;
      result = result.filter(l => new Date(l.expiresAt).getTime() <= cutoff);
    }
    return result;
  });

  constructor() {
    // Auto-clear notifications after 5s
    effect(() => {
      if (this.lastOutbid() || this.lastSettled()) {
        setTimeout(() => this.facade.clearNotifications(), 5000);
      }
    });
  }

  async ngOnInit(): Promise<void> {
    const leagueId = this.leaguesFacade.selectedLeagueId();
    const userId = this.authFacade.user()?.id;

    if (leagueId) await this.facade.loadListings(leagueId);
    if (userId && leagueId) await this.facade.loadAvailableBalance(userId, leagueId);

    await this.facade.connectSignalR();
  }

  ngOnDestroy(): void {
    const listing = this.selectedListing();
    if (listing) this.facade.leaveListing(listing.id);
    this.facade.disconnectSignalR();
    this.signalRSubscriptions.forEach((s) => s.unsubscribe());
  }

  async selectListing(listing: ListingDto): Promise<void> {
    const current = this.selectedListing();
    if (current) await this.facade.leaveListing(current.id);

    this.bidAmount.set(null);
    this.bidValidationError.set(null);
    this.showBuyNowConfirm.set(false);
    this.facade.resetBidState();

    await this.facade.loadListingDetail(listing.id);
    await this.facade.joinListing(listing.id);
  }

  async closeDetail(): Promise<void> {
    const listing = this.selectedListing();
    if (listing) await this.facade.leaveListing(listing.id);
    this.facade.clearSelectedListing();
    this.bidAmount.set(null);
    this.bidValidationError.set(null);
    this.showBuyNowConfirm.set(false);
    this.facade.resetBidState();
  }

  validateBid(amount: number | null): boolean {
    if (!amount || amount <= 0) {
      this.bidValidationError.set('Please enter a valid bid amount.');
      return false;
    }
    const min = this.minBid();
    if (amount < min) {
      this.bidValidationError.set(
        `Bid must be at least ${min} points (current bid + 5%).`
      );
      return false;
    }
    this.bidValidationError.set(null);
    return true;
  }

  async placeBid(): Promise<void> {
    const listing = this.selectedListing();
    const amount = this.bidAmount();
    const userId = this.authFacade.user()?.id;
    const leagueId = this.leaguesFacade.selectedLeagueId();

    if (!listing || !userId || !leagueId) return;
    if (!this.validateBid(amount)) return;

    const ok = await this.facade.placeBid(listing.id, {
      bidderId: userId,
      leagueId,
      amount: amount!,
    });

    if (ok) {
      this.bidAmount.set(null);
      await this.facade.loadListingDetail(listing.id);
      await this.facade.loadAvailableBalance(userId, leagueId);
    }
  }

  async confirmBuyNow(): Promise<void> {
    const listing = this.selectedListing();
    const userId = this.authFacade.user()?.id;
    const leagueId = this.leaguesFacade.selectedLeagueId();

    if (!listing || !userId || !leagueId) return;

    const ok = await this.facade.buyNow(listing.id, {
      buyerId: userId,
      leagueId,
    });

    if (ok) {
      await this.closeDetail();
      if (leagueId) await this.facade.loadListings(leagueId);
      await this.facade.loadAvailableBalance(userId, leagueId);
    }

    this.showBuyNowConfirm.set(false);
  }

  quickBid(offset: number | 'min'): void {
    const min = this.minBid();
    if (offset === 'min') {
      this.bidAmount.set(min);
    } else {
      this.bidAmount.set((this.bidAmount() ?? min) + offset);
    }
    this.bidValidationError.set(null);
  }

  trackById(_: number, item: ListingDto): string {
    return item.id;
  }

  getRarityColor(rarity: string): string {
    const map: Record<string, string> = {
      Common: '#64748b',
      Rare: '#1d4ed8',
      Epic: '#7c3aed',
      Legendary: '#b45309',
    };
    return map[rarity] ?? '#64748b';
  }

  getRarityBadgeStyle(rarity: string): { background: string; color: string } {
    const map: Record<string, { background: string; color: string }> = {
      Common:    { background: '#f1f5f9', color: '#475569' },
      Rare:      { background: '#dbeafe', color: '#1d4ed8' },
      Epic:      { background: '#ede9fe', color: '#7c3aed' },
      Legendary: { background: '#ffedd5', color: '#b45309' },
    };
    return map[rarity] ?? { background: '#f1f5f9', color: '#475569' };
  }

  getListingTimeLabel(expiresAt: string): string {
    return formatTimeRemaining(expiresAt);
  }

  getCardArtGradient(rarity: string): string {
    const map: Record<string, string> = {
      Common: 'linear-gradient(135deg, #334155 0%, #1e293b 100%)',
      Rare: 'linear-gradient(135deg, #1d4ed8 0%, #1e3a8a 100%)',
      Epic: 'linear-gradient(135deg, #7c3aed 0%, #4c1d95 100%)',
      Legendary: 'linear-gradient(135deg, #d97706 0%, #92400e 100%)',
    };
    return map[rarity] ?? 'linear-gradient(135deg, #334155 0%, #1e293b 100%)';
  }
}

function formatTimeRemaining(expiresAt: string): string {
  const diff = new Date(expiresAt).getTime() - Date.now();
  if (diff <= 0) return 'Expired';
  const h = Math.floor(diff / 3600000);
  const m = Math.floor((diff % 3600000) / 60000);
  if (h > 0) return `${h}h ${m}m`;
  const s = Math.floor((diff % 60000) / 1000);
  return `${m}m ${s}s`;
}
