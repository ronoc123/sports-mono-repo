import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  computed,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { CardsFacade, UserCard } from "@sports-ui/cards-data-access";
import { AuthFacade } from "@sports-ui/auth-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { VoteAccountFacade } from "@sports-ui/vote-account-data-access";
import { LeaguesFacade } from "@sports-ui/league-data-access";

const PACK_COST = 500;
const REVEAL_INTERVAL_MS = 700;

@Component({
  selector: "lib-pack-open",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./pack-open.component.html",
  styleUrl: "./pack-open.component.css",
})
export class PackOpenComponent implements OnInit, OnDestroy {
  private readonly facade = inject(CardsFacade);
  protected readonly leagueFacade = inject(LeaguesFacade);
  private readonly authFacade = inject(AuthFacade);
  private readonly orgFacade = inject(OrganizationFeatureService);
  private readonly voteAccountFacade = inject(VoteAccountFacade);

  // ── Facade signals ────────────────────────────────
  readonly isPurchasing = this.facade.isPurchasing;
  readonly packError = this.facade.packError;
  readonly packStatus = this.facade.packStatus;

  // ── Local UI state ────────────────────────────────
  readonly revealedCards = signal<UserCard[]>([]);
  readonly revealComplete = signal(false);
  readonly balance = this.voteAccountFacade.balance;
  readonly leagueId = this.leagueFacade.selectedLeagueId;

  readonly canPurchase = computed(
    () =>
      !!this.leagueId() &&
      this.balance() >= PACK_COST &&
      !this.isPurchasing() &&
      this.packStatus() !== "loading"
  );

  readonly packCost = PACK_COST;

  readonly rarityColors: Record<
    string,
    { bg: string; text: string; border: string }
  > = {
    Common: { bg: "#f1f5f9", text: "#475569", border: "#cbd5e1" },
    Rare: { bg: "#eff6ff", text: "#1d4ed8", border: "#93c5fd" },
    Epic: { bg: "#f5f3ff", text: "#7c3aed", border: "#c4b5fd" },
    Legendary: { bg: "#fffbeb", text: "#b45309", border: "#fcd34d" },
  };

  private _revealTimer: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.facade.resetPackState();
  }

  ngOnDestroy(): void {
    this._clearTimer();
  }

  // ── Actions ───────────────────────────────────────
  async onOpenPack(): Promise<void> {
    const userId = this.authFacade.user()?.id;
    const orgId = this.orgFacade.selectedOrganization()?.id;
    const leagueId = this.leagueFacade.selectedLeagueId();
    if (!userId || !orgId || !leagueId) return;

    this.revealedCards.set([]);
    this.revealComplete.set(false);
    this._clearTimer();

    const result = await this.facade.purchasePack({ userId, orgId, leagueId });

    if (result) {
      // Reload points balance so it updates without page refresh
      await this.voteAccountFacade.load(userId, orgId);
      this._startReveal(result.cards);
    }
  }

  onOpenAnother(): void {
    this.revealedCards.set([]);
    this.revealComplete.set(false);
    this.facade.resetPackState();
  }

  rarityStyle(tier: string): Record<string, string> {
    const c = this.rarityColors[tier] ?? this.rarityColors["Common"];
    return {
      background: c.bg,
      color: c.text,
      "border-color": c.border,
    };
  }

  // ── Reveal animation ──────────────────────────────
  private _startReveal(cards: UserCard[]): void {
    let idx = 0;
    this._revealTimer = setInterval(() => {
      if (idx < cards.length) {
        this.revealedCards.update((prev) => [...prev, cards[idx]]);
        idx++;
      } else {
        this.revealComplete.set(true);
        this._clearTimer();
      }
    }, REVEAL_INTERVAL_MS);
  }

  private _clearTimer(): void {
    if (this._revealTimer !== null) {
      clearInterval(this._revealTimer);
      this._revealTimer = null;
    }
  }
}
