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
