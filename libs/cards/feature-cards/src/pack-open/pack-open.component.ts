import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  computed,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatIconModule } from "@angular/material/icon";
import { CardsFacade, UserCard } from "@sports-ui/cards-data-access";
import { AuthFacade } from "@sports-ui/auth-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { VoteAccountFacade } from "@sports-ui/vote-account-data-access";
import { LeaguesFacade } from "@sports-ui/league-data-access";

const PACK_COST = 500;
const REVEAL_INTERVAL_MS = 600;

export interface PackTier {
  id: string;
  name: string;
  description: string;
  cost: number;
  cardsPerPack: number;
  gradient: string;
  icon: string;
  dropRates: { tier: string; pct: number }[];
  comingSoon: boolean;
}

const PACK_TIERS: PackTier[] = [
  {
    id: "standard",
    name: "Standard Pack",
    description: "5 cards with a chance at rare players",
    cost: 100,
    cardsPerPack: 5,
    gradient: "linear-gradient(135deg, #475569, #64748b)",
    icon: "inventory_2",
    dropRates: [
      { tier: "Common", pct: 70 },
      { tier: "Rare", pct: 20 },
      { tier: "Epic", pct: 8 },
      { tier: "Legendary", pct: 2 },
    ],
    comingSoon: true,
  },
  {
    id: "premium",
    name: "Premium Pack",
    description: "7 cards with guaranteed rare or better",
    cost: 250,
    cardsPerPack: 7,
    gradient: "linear-gradient(135deg, #1d4ed8, #7c3aed)",
    icon: "stars",
    dropRates: [
      { tier: "Common", pct: 45 },
      { tier: "Rare", pct: 35 },
      { tier: "Epic", pct: 15 },
      { tier: "Legendary", pct: 5 },
    ],
    comingSoon: true,
  },
  {
    id: "elite",
    name: "Elite Pack",
    description: "5 cards with guaranteed epic or better",
    cost: PACK_COST,
    cardsPerPack: 5,
    gradient: "linear-gradient(135deg, #7c3aed, #ec4899)",
    icon: "auto_awesome",
    dropRates: [
      { tier: "Common", pct: 30 },
      { tier: "Rare", pct: 35 },
      { tier: "Epic", pct: 25 },
      { tier: "Legendary", pct: 10 },
    ],
    comingSoon: false,
  },
  {
    id: "legendary",
    name: "Legendary Pack",
    description: "5 cards with guaranteed legendary",
    cost: 1000,
    cardsPerPack: 5,
    gradient: "linear-gradient(135deg, #b45309, #f59e0b)",
    icon: "workspace_premium",
    dropRates: [
      { tier: "Common", pct: 20 },
      { tier: "Rare", pct: 30 },
      { tier: "Epic", pct: 30 },
      { tier: "Legendary", pct: 20 },
    ],
    comingSoon: true,
  },
];

@Component({
  selector: "lib-pack-open",
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule],
  templateUrl: "./pack-open.component.html",
  styleUrl: "./pack-open.component.css",
})
export class PackOpenComponent implements OnInit, OnDestroy {
  private readonly facade = inject(CardsFacade);
  protected readonly leagueFacade = inject(LeaguesFacade);
  private readonly authFacade = inject(AuthFacade);
  private readonly orgFacade = inject(OrganizationFeatureService);
  private readonly voteAccountFacade = inject(VoteAccountFacade);

  // ── Facade signals ─────────────────────────────────
  readonly isPurchasing = this.facade.isPurchasing;
  readonly packError = this.facade.packError;
  readonly packStatus = this.facade.packStatus;
  readonly collection = this.facade.collection;

  // ── Local UI state ─────────────────────────────────
  readonly revealedCards = signal<UserCard[]>([]);
  readonly revealComplete = signal(false);
  readonly balance = this.voteAccountFacade.balance;
  readonly leagueId = this.leagueFacade.selectedLeagueId;

  readonly packTiers = PACK_TIERS;
  readonly packCost = PACK_COST;

  readonly canPurchase = computed(
    () =>
      !!this.leagueId() &&
      this.balance() >= PACK_COST &&
      !this.isPurchasing() &&
      this.packStatus() !== "loading"
  );

  // ── Collection stats ───────────────────────────────
  readonly totalCollectionCards = computed(() => this.collection().length);
  readonly legendaryCount = computed(
    () => this.collection().filter((c) => c.rarityTier === "Legendary").length
  );
  readonly bestOverall = computed(() => {
    const cards = this.collection();
    return cards.length ? Math.max(...cards.map((c) => c.overallRating)) : 0;
  });
  readonly uniquePlayers = computed(
    () => new Set(this.collection().map((c) => c.cardPlayerId)).size
  );

  private _revealTimer: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.facade.resetPackState();
    const userId = this.authFacade.user()?.id;
    const orgId = this.orgFacade.selectedOrganization()?.id;
    if (userId && orgId) {
      this.facade.loadCollection(userId);
    }
  }

  ngOnDestroy(): void {
    this._clearTimer();
  }

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
      await this.voteAccountFacade.load(userId, leagueId);
      this._startReveal(result.cards);
    }
  }

  onOpenAnother(): void {
    this.revealedCards.set([]);
    this.revealComplete.set(false);
    this.facade.resetPackState();
  }

  initials(name: string): string {
    return name
      .split(" ")
      .map((w) => w[0])
      .join("")
      .slice(0, 2)
      .toUpperCase();
  }

  rarityGradient(tier: string): string {
    const map: Record<string, string> = {
      Common: "linear-gradient(135deg, #475569, #64748b)",
      Rare: "linear-gradient(135deg, #1d4ed8, #3b82f6)",
      Epic: "linear-gradient(135deg, #7c3aed, #a855f7)",
      Legendary: "linear-gradient(135deg, #b45309, #f59e0b)",
    };
    return map[tier] ?? map["Common"];
  }

  rarityAccent(tier: string): string {
    const map: Record<string, string> = {
      Common: "#64748b",
      Rare: "#3b82f6",
      Epic: "#a855f7",
      Legendary: "#f59e0b",
    };
    return map[tier] ?? "#64748b";
  }

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
