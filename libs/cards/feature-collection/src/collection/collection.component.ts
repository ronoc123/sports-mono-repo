import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CardsFacade, UserCard } from '@sports-ui/cards-data-access';
import { AuthFacade } from '@sports-ui/auth-data-access';
import { OrganizationFeatureService } from '@sports-ui/organization-data-access';
import { MarketplaceFacade } from '@sports-ui/marketplace-data-access';

type RarityFilter = 'All' | 'Common' | 'Rare' | 'Epic' | 'Legendary';

@Component({
  selector: 'lib-collection',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './collection.component.html',
  styleUrl: './collection.component.css',
})
export class CollectionComponent implements OnInit {
  private readonly facade = inject(CardsFacade);
  private readonly authFacade = inject(AuthFacade);
  private readonly orgFacade = inject(OrganizationFeatureService);
  private readonly marketplaceFacade = inject(MarketplaceFacade);

  // ── Facade signals ────────────────────────────────
  readonly isLoading = this.facade.isLoading;
  readonly error = this.facade.error;
  readonly collection = this.facade.collection;

  // ── Local UI state ────────────────────────────────
  readonly rarityFilter = signal<RarityFilter>('All');
  readonly searchQuery = signal('');

  readonly rarityFilters: RarityFilter[] = ['All', 'Common', 'Rare', 'Epic', 'Legendary'];

  readonly filteredCollection = computed(() => {
    let cards = this.collection();

    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      cards = cards.filter(
        (c) =>
          c.name.toLowerCase().includes(query) ||
          c.position.toLowerCase().includes(query)
      );
    }

    const rarity = this.rarityFilter();
    if (rarity !== 'All') {
      cards = cards.filter((c) => c.rarityTier === rarity);
    }

    return cards;
  });

  readonly totalCount = computed(() => this.collection().length);
  readonly filteredCount = computed(() => this.filteredCollection().length);

  // ── Listing form state ────────────────────────────────────────────────────
  readonly listingCard = signal<UserCard | null>(null);
  readonly listingStartingBid = signal<number | null>(null);
  readonly listingBuyNowPrice = signal<number | null>(null);
  readonly listingDurationHours = signal<number>(24);
  readonly isListing = this.marketplaceFacade.isListing;
  readonly listError = this.marketplaceFacade.listError;
  readonly listStatus = this.marketplaceFacade.listStatus;

  readonly durationOptions = [1, 24, 48, 72];

  readonly rarityColors: Record<string, { bg: string; text: string; border: string }> = {
    Common: { bg: '#f1f5f9', text: '#475569', border: '#cbd5e1' },
    Rare: { bg: '#eff6ff', text: '#1d4ed8', border: '#93c5fd' },
    Epic: { bg: '#f5f3ff', text: '#7c3aed', border: '#c4b5fd' },
    Legendary: { bg: '#fffbeb', text: '#b45309', border: '#fcd34d' },
  };

  async ngOnInit(): Promise<void> {
    const userId = this.authFacade.user()?.id;
    const orgId = this.orgFacade.selectedOrganization()?.id;
    if (userId && orgId) {
      await this.facade.loadCollection(userId, orgId);
    }
  }

  rarityStyle(tier: string): Record<string, string> {
    const c = this.rarityColors[tier] ?? this.rarityColors['Common'];
    return {
      background: c.bg,
      color: c.text,
      'border-color': c.border,
    };
  }

  rarityCount(tier: RarityFilter): number {
    if (tier === 'All') return this.collection().length;
    return this.collection().filter((c) => c.rarityTier === tier).length;
  }

  trackByCard(_: number, card: UserCard): string {
    return card.id;
  }

  openListingForm(card: UserCard): void {
    this.listingCard.set(card);
    this.listingStartingBid.set(null);
    this.listingBuyNowPrice.set(null);
    this.listingDurationHours.set(24);
    this.marketplaceFacade.resetListState();
  }

  closeListingForm(): void {
    this.listingCard.set(null);
    this.marketplaceFacade.resetListState();
  }

  async submitListing(): Promise<void> {
    const card = this.listingCard();
    const userId = this.authFacade.user()?.id;
    const orgId = this.orgFacade.selectedOrganization()?.id;
    const startingBid = this.listingStartingBid();

    if (!card || !userId || !orgId || !startingBid || startingBid <= 0) return;

    const ok = await this.marketplaceFacade.createListing({
      cardOwnerId: card.id,
      sellerId: userId,
      orgId,
      startingBid,
      buyNowPrice: this.listingBuyNowPrice() ?? undefined,
      durationHours: this.listingDurationHours(),
    });

    if (ok) {
      this.closeListingForm();
      await this.facade.loadCollection(userId, orgId);
    }
  }
}
