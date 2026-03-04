import { Component, OnInit, inject, signal, computed } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { MatIconModule } from "@angular/material/icon";
import { CardsFacade, UserCard } from "@sports-ui/cards-data-access";
import { AuthFacade } from "@sports-ui/auth-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { MarketplaceFacade } from "@sports-ui/marketplace-data-access";

type RarityFilter = "All" | "Common" | "Rare" | "Epic" | "Legendary";
type SortField = "name" | "overallRating" | "pulledAt";
type ViewMode = "grid" | "list";

@Component({
  selector: "lib-collection",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatIconModule],
  templateUrl: "./collection.component.html",
  styleUrl: "./collection.component.css",
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
  readonly rarityFilter = signal<RarityFilter>("All");
  readonly searchQuery = signal("");
  readonly viewMode = signal<ViewMode>("grid");
  readonly sortField = signal<SortField>("overallRating");
  readonly sortDirection = signal<"asc" | "desc">("desc");
  readonly selectedCard = signal<UserCard | null>(null);

  readonly rarityFilters: RarityFilter[] = [
    "All",
    "Common",
    "Rare",
    "Epic",
    "Legendary",
  ];

  readonly sortOptions: { label: string; value: SortField }[] = [
    { label: "Overall Rating", value: "overallRating" },
    { label: "Name", value: "name" },
    { label: "Date Pulled", value: "pulledAt" },
  ];

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
    if (rarity !== "All") {
      cards = cards.filter((c) => c.rarityTier === rarity);
    }

    const field = this.sortField();
    const dir = this.sortDirection() === "asc" ? 1 : -1;
    return [...cards].sort((a, b) => {
      if (field === "overallRating")
        return (a.overallRating - b.overallRating) * dir;
      if (field === "name") return a.name.localeCompare(b.name) * dir;
      if (field === "pulledAt") {
        const ta = a.pulledAt ? new Date(a.pulledAt).getTime() : 0;
        const tb = b.pulledAt ? new Date(b.pulledAt).getTime() : 0;
        return (ta - tb) * dir;
      }
      return 0;
    });
  });

  // ── Stats ─────────────────────────────────────────
  readonly totalCount = computed(() => this.collection().length);
  readonly filteredCount = computed(() => this.filteredCollection().length);

  readonly avgOverallRating = computed(() => {
    const cards = this.collection();
    if (!cards.length) return 0;
    return Math.round(
      cards.reduce((sum, c) => sum + c.overallRating, 0) / cards.length
    );
  });

  readonly commonCount = computed(
    () => this.collection().filter((c) => c.rarityTier === "Common").length
  );
  readonly rareCount = computed(
    () => this.collection().filter((c) => c.rarityTier === "Rare").length
  );
  readonly epicCount = computed(
    () => this.collection().filter((c) => c.rarityTier === "Epic").length
  );
  readonly legendaryCount = computed(
    () => this.collection().filter((c) => c.rarityTier === "Legendary").length
  );

  // ── Listing form state ────────────────────────────
  readonly listingCard = signal<UserCard | null>(null);
  readonly listingStartingBid = signal<number | null>(null);
  readonly listingBuyNowPrice = signal<number | null>(null);
  readonly listingDurationHours = signal<number>(24);
  readonly isListing = this.marketplaceFacade.isListing;
  readonly listError = this.marketplaceFacade.listError;
  readonly listStatus = this.marketplaceFacade.listStatus;

  readonly durationOptions = [1, 24, 48, 72];

  async ngOnInit(): Promise<void> {
    const userId = this.authFacade.user()?.id;
    const orgId = this.orgFacade.selectedOrganization()?.id;
    if (userId && orgId) {
      await this.facade.loadCollection(userId);
    }
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

  rarityCount(tier: RarityFilter): number {
    if (tier === "All") return this.collection().length;
    return this.collection().filter((c) => c.rarityTier === tier).length;
  }

  toggleSortDir(): void {
    this.sortDirection.update((d) => (d === "asc" ? "desc" : "asc"));
  }

  openCardDetail(card: UserCard): void {
    this.selectedCard.set(card);
    this.listingCard.set(card);
    this.listingStartingBid.set(null);
    this.listingBuyNowPrice.set(null);
    this.listingDurationHours.set(24);
    this.marketplaceFacade.resetListState();
  }

  closeCardDetail(): void {
    this.selectedCard.set(null);
    this.listingCard.set(null);
    this.marketplaceFacade.resetListState();
  }

  async submitListing(): Promise<void> {
    const card = this.listingCard();
    const userId = this.authFacade.user()?.id;
    const orgId = this.orgFacade.selectedOrganization()?.id;
    const startingBid = this.listingStartingBid();

    if (!card || !userId || !startingBid || startingBid <= 0) return;

    const ok = await this.marketplaceFacade.createListing({
      userCardId: card.id,
      sellerId: userId,
      startingBid,
      buyNowPrice: this.listingBuyNowPrice() ?? undefined,
      durationHours: this.listingDurationHours(),
    });

    if (ok) {
      this.closeCardDetail();
      await this.facade.loadCollection(userId);
    }
  }

  trackByCard(_: number, card: UserCard): string {
    return card.id;
  }
}
