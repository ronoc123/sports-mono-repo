import { Injectable, inject } from "@angular/core";
import { firstValueFrom } from "rxjs";
import { H2HStore } from "./h2h.store";
import { H2HApi } from "./h2h.api";
import { CardsFacade } from "@sports-ui/cards-data-access";

@Injectable({ providedIn: "root" })
export class H2HFacade {
  private readonly store = inject(H2HStore);
  private readonly api = inject(H2HApi);
  private readonly cardsFacade = inject(CardsFacade);

  // ── Exposed signals ────────────────────────────────────────────────────────
  readonly collection = this.store.collection;
  readonly selectedCardIds = this.store.selectedCardIds;
  readonly selectedCards = this.store.selectedCards;
  readonly wagerAmount = this.store.wagerAmount;
  readonly matchResult = this.store.matchResult;
  readonly matchHistory = this.store.matchHistory;
  readonly squadOverall = this.store.squadOverall;
  readonly canPlayMatch = this.store.canPlayMatch;
  readonly isSquadFull = this.store.isSquadFull;
  readonly isLoadingCollection = this.store.isLoadingCollection;
  readonly isPlayingMatch = this.store.isPlayingMatch;
  readonly isLoadingHistory = this.store.isLoadingHistory;
  readonly error = this.store.error;
  readonly matchError = this.store.matchError;
  readonly matchStatus = this.store.matchStatus;
  readonly totalHistoryCount = this.store.totalHistoryCount;
  readonly historyPage = this.store.historyPage;

  // ── Actions ────────────────────────────────────────────────────────────────

  async loadCollection(userId: string): Promise<void> {
    this.store.setCollectionLoading();
    try {
      await this.cardsFacade.loadCollection(userId);
      this.store.setCollection(this.cardsFacade.collection());
    } catch {
      this.store.setCollectionError("Failed to load collection");
    }
  }

  toggleCard(cardId: string): void {
    this.store.toggleCard(cardId);
  }

  setWagerAmount(amount: number | null): void {
    this.store.setWagerAmount(amount);
  }

  async playMatch(fanUserId: string, orgId: string): Promise<boolean> {
    const userCardIds = this.store.selectedCardIds();
    const wagerAmount = this.store.wagerAmount();

    if (!wagerAmount || userCardIds.length === 0) return false;

    this.store.setMatchPlaying();
    try {
      const res = await firstValueFrom(
        this.api.createMatch({ fanUserId, orgId, userCardIds, wagerAmount })
      );
      if (res.data) {
        this.store.setMatchResult(res.data);
        return true;
      }
      this.store.setMatchError(res.message ?? "Match failed");
      return false;
    } catch (err: any) {
      this.store.setMatchError(
        err?.error?.message ?? err?.message ?? "Failed to play match"
      );
      return false;
    }
  }

  resetSquad(): void {
    this.store.resetSquad();
  }

  async loadMatchHistory(
    fanUserId: string,
    orgId: string,
    page = 1
  ): Promise<void> {
    this.store.setHistoryLoading();
    try {
      const result = await firstValueFrom(
        this.api.getMatchHistory(fanUserId, orgId, page)
      );
      this.store.setMatchHistory(result.items, result.totalCount, result.page);
    } catch {
      this.store.setHistoryError("Failed to load match history");
    }
  }
}
