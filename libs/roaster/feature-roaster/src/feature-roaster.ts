import { Component, computed, inject, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RoasterFacade } from "@sports-ui/roaster-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { RosterPlayer } from "@sports-ui/roaster-data-access";

type RosterGroup = "All" | "Offense" | "Defense" | "Special Teams";

const OFFENSE_POSITIONS  = ["QB","RB","FB","WR","TE","OT","OG","OL","C","G"];
const SPECIAL_POSITIONS  = ["K","P","LS","KR","PR"];

@Component({
  selector: "lib-feature-roaster",
  imports: [CommonModule],
  standalone: true,
  templateUrl: "./feature-roaster.html",
  styleUrl: "./feature-roaster.css",
})
export class FeatureRoaster implements OnInit {
  private facade = inject(RoasterFacade);
  private organizationFacade = inject(OrganizationFeatureService);

  readonly loading = this.facade.loading;
  readonly error = this.facade.error;

  readonly tabs: RosterGroup[] = ["All", "Offense", "Defense", "Special Teams"];
  activeTab = signal<RosterGroup>("All");
  searchTerm = signal("");

  // Hardcoded header stats
  readonly headerStats = [
    { label: "Total Roster", value: "53",    suffix: "/53", icon: "groups",       color: "blue"   },
    { label: "Active Cap",   value: "$224.8M", suffix: "",  icon: "trending_up",  color: "green"  },
    { label: "Avg Age",      value: "25.4",  suffix: "",    icon: "emoji_events", color: "purple" },
    { label: "Injured",      value: "3",     suffix: "",    icon: "healing",      color: "red"    },
  ];

  filteredPlayers = computed(() => {
    const tab    = this.activeTab();
    const term   = this.searchTerm().trim().toLowerCase();
    const players = this.facade.players();

    return players.filter((p) => {
      const matchesTab = tab === "All" || this.getGroup(p) === tab;
      const matchesTerm =
        !term ||
        p.name.toLowerCase().includes(term) ||
        p.position.toLowerCase().includes(term);
      return matchesTab && matchesTerm;
    });
  });

  ngOnInit(): void {
    const orgId = this.organizationFacade.selectedOrganization()?.id;
    if (orgId) this.facade.loadRoster(orgId);
  }

  setTab(tab: RosterGroup) {
    this.activeTab.set(tab);
  }

  onSearch(event: Event) {
    this.searchTerm.set((event.target as HTMLInputElement).value ?? "");
  }

  getGroup(player: RosterPlayer): RosterGroup {
    const pos = player.position?.toUpperCase() ?? "";
    if (OFFENSE_POSITIONS.includes(pos)) return "Offense";
    if (SPECIAL_POSITIONS.includes(pos))  return "Special Teams";
    return "Defense";
  }

  getStatus(player: RosterPlayer): string {
    if (player.isActive)   return "Active";
    return "Practice Squad";
  }

  getStatusClass(player: RosterPlayer): string {
    if (player.isActive)   return "status--active";
    return "status--practice";
  }

  posAbbr(position: string): string {
    if (!position) return '?';
    const trimmed = position.trim();
    // Already an abbreviation (≤3 chars) — return as-is uppercased
    if (trimmed.length <= 3) return trimmed.toUpperCase();
    // Full name — take first letter of each word: "Offensive Tackle" → "OT"
    return trimmed
      .split(/\s+/)
      .map(w => w[0])
      .join('')
      .toUpperCase();
  }

  getGroupIcon(tab: RosterGroup): string {
    switch (tab) {
      case "Offense":       return "sports_football";
      case "Defense":       return "security";
      case "Special Teams": return "bolt";
      default:              return "";
    }
  }
}
