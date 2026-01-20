import { Component, computed, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  CreatePlayerOptionCardComponent,
  SearchbarComponent,
} from "@sports-ui/ui";
import { PlayerFacade } from "@sports-ui/player-data-access";

@Component({
  selector: "lib-create-player-option-feature",
  imports: [CommonModule, SearchbarComponent, CreatePlayerOptionCardComponent],
  templateUrl: "./create-player-option-feature.html",
  styleUrl: "./create-player-option-feature.css",
})
export class CreatePlayerOptionFeature {
  private facade = inject(PlayerFacade);

  searchTerm = signal("");

  filteredPlayers = computed(() => {
    const t = this.searchTerm().trim().toLowerCase();
    const players = this.facade.players();
    if (!t) return players;
    return players.filter(
      (p) =>
        p.name?.toLowerCase().includes(t) ||
        (p.position && p.position.toLowerCase().includes(t))
    );
  });

  ngOnInit(): void {
    this.facade.loadPlayers("38ED02D7-CAE6-4A8E-9232-52373BF16338");
  }

  onSearch(term: string) {
    this.searchTerm.set(term ?? "");
  }
}
