import { Component, OnInit, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Router } from "@angular/router";
import { LeaguesFacade } from "@sports-ui/league-data-access";

@Component({
  selector: "lib-league-feature",
  imports: [CommonModule],
  templateUrl: "./league-feature.html",
  styleUrl: "./league-feature.css",
})
export class LeagueFeature implements OnInit {
  private readonly facade = inject(LeaguesFacade);
  private readonly router = inject(Router);

  // Signals from store
  readonly leagues = this.facade.leagues;
  readonly isLoading = this.facade.isLoading;

  selectedLeagueId = signal<string | null>(null);

  ngOnInit(): void {
    this.facade.loadLeagues();
  }

  selectLeague(id: string) {
    this.selectedLeagueId.set(id);
  }

  async continue() {
    const leagueId = this.selectedLeagueId();

    if (!leagueId) return;
    this.facade.setSelectedLeague(leagueId);
    localStorage.setItem("selectedLeague", leagueId);
    this.router.navigate(["/"]);
  }
}
