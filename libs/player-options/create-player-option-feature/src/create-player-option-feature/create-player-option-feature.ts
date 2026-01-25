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
import {
  CreatePlayerOptionCommand,
  PlayerOptionFeatureService,
} from "@sports-ui/player-options-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { FormsModule } from "@angular/forms";

@Component({
  selector: "lib-create-player-option-feature",
  imports: [
    CommonModule,
    SearchbarComponent,
    CreatePlayerOptionCardComponent,
    UiInput,
    FormsModule,
  ],
  templateUrl: "./create-player-option-feature.html",
  styleUrl: "./create-player-option-feature.css",
})
export class CreatePlayerOptionFeature implements OnInit {
  private facade = inject(PlayerFacade);
  private playerOptionFeature = inject(PlayerOptionFeatureService);
  private organizationFacade = inject(OrganizationFeatureService);
  protected modal = inject(ModalService);
  searchTerm = signal("");
  // eslint-disable-next-line @typescript-eslint/no-unsafe-function-type
  @ViewChild("playerModal") playerModal!: TemplateRef<{
    player: PlayerDto;
    submit: (p: any) => void;
  }>;

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
      context: {
        player,
        submit: async (payload: any) => {
          try {
            console.log("payload", payload);
            await this.playerOptionFeature.createPlayerOption({
              title: payload.title,
              description: payload.description,
              playerId: payload.player.id,
              organizationId:
                this.organizationFacade.selectedOrganization()?.id || "",
            });
            this.modal.close();
          } catch {
            // already toasted
          }
        },
      },
    });
  }
}
