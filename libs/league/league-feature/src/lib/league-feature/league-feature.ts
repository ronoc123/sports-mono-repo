import { Component, OnInit, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Router } from "@angular/router";
import { MatIconModule } from "@angular/material/icon";
import { LeaguesFacade } from "@sports-ui/league-data-access";

interface SportMeta {
  icon: string;
  gradient: string;
  fullName: string;
  season: string;
}

@Component({
  selector: "lib-league-feature",
  imports: [CommonModule, MatIconModule],
  templateUrl: "./league-feature.html",
  styleUrl: "./league-feature.css",
})
export class LeagueFeature implements OnInit {
  private readonly facade = inject(LeaguesFacade);
  private readonly router = inject(Router);

  readonly leagues = this.facade.leagues;
  readonly isLoading = this.facade.isLoading;

  selectedLeagueId = signal<string | null>(null);

  private readonly metaMap: Record<string, SportMeta> = {
    NFL: {
      icon: "🏈",
      gradient: "linear-gradient(to right, #013369, #D50A0A)",
      fullName: "National Football League",
      season: "2025-26 Season",
    },
    NBA: {
      icon: "🏀",
      gradient: "linear-gradient(to right, #17408B, #C9082A)",
      fullName: "National Basketball Association",
      season: "2025-26 Season",
    },
  };

  ngOnInit(): void {
    this.facade.loadLeagues();
  }

  getMeta(name: string): SportMeta {
    return (
      this.metaMap[name] ?? {
        icon: "🏆",
        gradient: "linear-gradient(to right, #1d4ed8, #3b82f6)",
        fullName: name,
        season: "2025-26 Season",
      }
    );
  }

  getSelectedLeagueName(): string {
    const id = this.selectedLeagueId();
    if (!id) return "";
    const league = this.leagues().find((l) => l.id === id);
    return league ? this.getMeta(league.name).fullName : "";
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
