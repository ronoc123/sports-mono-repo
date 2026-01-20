import {
  Component,
  computed,
  inject,
  signal,
  TemplateRef,
  ViewChild,
  OnInit,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  CreatePlayerOptionCardComponent,
  SearchbarComponent,
  UiInput,
} from "@sports-ui/ui";
import { PlayerDto, PlayerFacade } from "@sports-ui/player-data-access";
import { ModalService } from "@sports-ui/modal-service";

@Component({
  selector: "lib-create-player-option-feature",
  imports: [
    CommonModule,
    SearchbarComponent,
    CreatePlayerOptionCardComponent,
    UiInput,
  ],
  templateUrl: "./create-player-option-feature.html",
  styleUrl: "./create-player-option-feature.css",
})
export class CreatePlayerOptionFeature implements OnInit {
  private facade = inject(PlayerFacade);
  private modal = inject(ModalService);
  searchTerm = signal("");
  @ViewChild("playerModal") playerModal!: TemplateRef<{ player: PlayerDto }>;

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
  open(player: PlayerDto) {
    this.modal.open({
      title: "Create Player Option",
      width: "md",
      content: this.playerModal,
      context: { player },
    });
  }
}
