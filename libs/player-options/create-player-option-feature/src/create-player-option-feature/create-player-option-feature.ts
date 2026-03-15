import {
  Component,
  computed,
  effect,
  inject,
  signal,
  TemplateRef,
  ViewChild,
  OnInit,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { CreatePlayerOptionCardComponent, UiInput } from "@sports-ui/ui";
import { PlayerDto, PlayerFacade } from "@sports-ui/player-data-access";
import { ModalService } from "@sports-ui/modal-service";
import {
  CreatePlayerOptionCommand,
  PlayerOptionFeatureService,
} from "@sports-ui/player-options-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { FormsModule } from "@angular/forms";
import { LeaguesFacade } from "@sports-ui/league-data-access";

@Component({
  selector: "lib-create-player-option-feature",
  imports: [
    CommonModule,
    CreatePlayerOptionCardComponent,
    UiInput,
    FormsModule,
  ],
  templateUrl: "./create-player-option-feature.html",
  styleUrl: "./create-player-option-feature.css",
})
export class CreatePlayerOptionFeature implements OnInit {
  private facade = inject(PlayerFacade);
  private leagueFacade = inject(LeaguesFacade);
  private playerOptionFeature = inject(PlayerOptionFeatureService);
  private organizationFacade = inject(OrganizationFeatureService);
  protected modal = inject(ModalService);
  searchTerm = signal("");
  selectedOptionType = signal<string>("Trade");
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

  // ── Pagination ────────────────────────────────────
  readonly PAGE_SIZE = 10;
  readonly currentPage = signal(1);

  readonly paginatedPlayers = computed(() => {
    const page = this.currentPage();
    const start = (page - 1) * this.PAGE_SIZE;
    return this.filteredPlayers().slice(start, start + this.PAGE_SIZE);
  });

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.filteredPlayers().length / this.PAGE_SIZE))
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

  constructor() {
    effect(() => {
      this.searchTerm();
      this.currentPage.set(1);
    });
  }

  goToPage(page: number) {
    this.currentPage.set(Math.max(1, Math.min(page, this.totalPages())));
  }

  ngOnInit(): void {
    const leagueId = this.leagueFacade.selectedLeagueId() || "";
    this.facade.loadPlayers(leagueId || "698200CB-7255-4C57-8475-52D5385860D7");
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
