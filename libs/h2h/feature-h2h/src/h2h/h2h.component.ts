import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { H2HFacade } from '@sports-ui/h2h-data-access';
import { BotSquadCard } from '@sports-ui/h2h-data-access';
import { AuthFacade } from '@sports-ui/auth-data-access';
import { OrganizationFeatureService } from '@sports-ui/organization-data-access';
import { UserCard } from '@sports-ui/cards-data-access';

type H2HView = 'builder' | 'history';

@Component({
  selector: 'lib-h2h',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './h2h.component.html',
  styleUrl: './h2h.component.css',
})
export class H2HComponent implements OnInit {
  private readonly facade = inject(H2HFacade);
  private readonly authFacade = inject(AuthFacade);
  private readonly orgFacade = inject(OrganizationFeatureService);

  // ── Facade signals ────────────────────────────────────────────────────────
  readonly collection = this.facade.collection;
  readonly selectedCardIds = this.facade.selectedCardIds;
  readonly selectedCards = this.facade.selectedCards;
  readonly squadOverall = this.facade.squadOverall;
  readonly canPlayMatch = this.facade.canPlayMatch;
  readonly isSquadFull = this.facade.isSquadFull;
  readonly wagerAmount = this.facade.wagerAmount;
  readonly isLoadingCollection = this.facade.isLoadingCollection;
  readonly isPlayingMatch = this.facade.isPlayingMatch;
  readonly isLoadingHistory = this.facade.isLoadingHistory;
  readonly matchResult = this.facade.matchResult;
  readonly matchError = this.facade.matchError;
  readonly matchStatus = this.facade.matchStatus;
  readonly matchHistory = this.facade.matchHistory;
  readonly totalHistoryCount = this.facade.totalHistoryCount;
  readonly error = this.facade.error;

  // ── Local UI state ────────────────────────────────────────────────────────
  readonly activeView = signal<H2HView>('builder');
  readonly showConfirmDialog = signal(false);

  readonly parsedBotSquad = computed<BotSquadCard[]>(() => {
    const result = this.matchResult();
    if (!result) return [];
    try {
      return JSON.parse(result.botSquadSnapshot) as BotSquadCard[];
    } catch {
      return [];
    }
  });

  readonly pointsDelta = computed(() => {
    const result = this.matchResult();
    if (!result) return 0;
    return result.outcome === 'win' ? result.wagerAmount : -result.wagerAmount;
  });

  async ngOnInit(): Promise<void> {
    const userId = this.authFacade.user()?.id;
    const orgId = this.orgFacade.selectedOrganization()?.id;
    if (userId && orgId) {
      await this.facade.loadCollection(userId, orgId);
      await this.facade.loadMatchHistory(userId, orgId);
    }
  }

  toggleCard(card: UserCard): void {
    if (card.isListed) return;
    this.facade.toggleCard(card.id);
  }

  isSelected(cardId: string): boolean {
    return this.selectedCardIds().includes(cardId);
  }

  onWagerInput(value: number | string): void {
    const n = Number(value);
    this.facade.setWagerAmount(isNaN(n) || n <= 0 ? null : n);
  }

  openConfirm(): void {
    this.showConfirmDialog.set(true);
  }

  cancelConfirm(): void {
    this.showConfirmDialog.set(false);
  }

  async confirmPlay(): Promise<void> {
    this.showConfirmDialog.set(false);
    const userId = this.authFacade.user()?.id;
    const orgId = this.orgFacade.selectedOrganization()?.id;
    if (userId && orgId) {
      await this.facade.playMatch(userId, orgId);
    }
  }

  playAgain(): void {
    this.facade.resetSquad();
    const userId = this.authFacade.user()?.id;
    const orgId = this.orgFacade.selectedOrganization()?.id;
    if (userId && orgId) {
      this.facade.loadMatchHistory(userId, orgId);
    }
  }

  switchView(view: H2HView): void {
    this.activeView.set(view);
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

  trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  trackByMatchId(_: number, item: { matchId: string }): string {
    return item.matchId;
  }
}
