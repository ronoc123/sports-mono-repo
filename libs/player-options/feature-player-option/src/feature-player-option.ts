import { Component, computed, effect, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatChipsModule } from "@angular/material/chips";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatSnackBarModule } from "@angular/material/snack-bar";
import { PlayerOptionFeatureService } from "@sports-ui/player-options-data-access";
import { PlayerOptionCardComponent, SearchbarComponent } from "@sports-ui/ui";
import { VoteAccountFacade } from "@sports-ui/vote-account-data-access";
import { AuthFacade } from "@sports-ui/auth-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { LeaguesFacade } from "@sports-ui/league-data-access";

@Component({
  selector: "lib-feature-player-option",
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatProgressBarModule,
    MatSnackBarModule,
    PlayerOptionCardComponent,
    SearchbarComponent,
  ],
  templateUrl: "./feature-player-option.html",
  styleUrl: "./feature-player-option.css",
})
export class FeaturePlayerOption {
  private orgFacade = inject(OrganizationFeatureService);
  private leagueFacade = inject(LeaguesFacade);
  feature = inject(PlayerOptionFeatureService);
  selected = inject(VoteAccountFacade);
  authFacade = inject(AuthFacade);
  options = this.feature.options;
  loading = this.feature.status;
  error = this.feature.error;
  searchTerm = signal("");

  organization = this.orgFacade.selectedOrganization;

  constructor() {
    effect(() => {
      const org = this.organization();
      if (!org) return;
      this.feature.loadPlayerOptions({ organizationId: org.id });
    });

    effect(() => {
      this.searchTerm();
      this.currentPage.set(1);
    });
  }

  filteredPlayerOptions = computed(() => {
    const t = this.searchTerm().trim().toLowerCase();
    if (!t) return this.options();
    return this.options().filter(
      (p) =>
        p.player.name?.toLowerCase().includes(t) ||
        (p.player.position && p.player.position.toLowerCase().includes(t))
    );
  });

  // ── Pagination ────────────────────────────────────
  readonly PAGE_SIZE = 12;
  readonly currentPage = signal(1);

  readonly paginatedOptions = computed(() => {
    const page = this.currentPage();
    const start = (page - 1) * this.PAGE_SIZE;
    return this.filteredPlayerOptions().slice(start, start + this.PAGE_SIZE);
  });

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.filteredPlayerOptions().length / this.PAGE_SIZE))
  );

  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    const pages: number[] = [1];
    const start = Math.max(2, current - 2);
    const end = Math.min(total - 1, current + 2);
    if (start > 2) pages.push(-1);
    for (let i = start; i <= end; i++) pages.push(i);
    if (end < total - 1) pages.push(-1);
    pages.push(total);
    return pages;
  });

  goToPage(page: number) {
    const clamped = Math.max(1, Math.min(page, this.totalPages()));
    this.currentPage.set(clamped);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  onSelectOption(option: any) {
    this.selected.vote(
      option.option.id,
      this.authFacade.user()?.id ?? "",
      option.amount,
      this.leagueFacade.selectedLeagueId() ?? ""
    );
  }

  onSearch(term: string) {
    this.searchTerm.set(term ?? "");
  }
}
