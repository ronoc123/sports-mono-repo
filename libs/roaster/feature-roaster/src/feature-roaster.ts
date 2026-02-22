import { Component, computed, inject, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { PlayerCardComponent, SearchbarComponent } from "@sports-ui/ui";
import { RoasterFacade } from "@sports-ui/roaster-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";

@Component({
  selector: "lib-feature-roaster",
  imports: [CommonModule, SearchbarComponent, PlayerCardComponent],
  standalone: true,
  templateUrl: "./feature-roaster.html",
  styleUrl: "./feature-roaster.css",
})
export class FeatureRoaster implements OnInit {
  private facade = inject(RoasterFacade);
  private organizationFacade = inject(OrganizationFeatureService);

  readonly loading = this.facade.loading;
  readonly error = this.facade.error;

  searchTerm = signal("");

  filteredPlayers = computed(() => {
    const t = this.searchTerm().trim().toLowerCase();
    const players = this.facade.players();
    if (!t) return players;
    return players.filter(
      (p) =>
        p.name.toLowerCase().includes(t) ||
        p.position.toLowerCase().includes(t)
    );
  });

  ngOnInit(): void {
    const orgId = this.organizationFacade.selectedOrganization()?.id;
    if (orgId) {
      this.facade.loadRoster(orgId);
    }
  }

  onSearch(term: string) {
    this.searchTerm.set(term ?? "");
  }
}
