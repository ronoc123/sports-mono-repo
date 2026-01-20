import { inject, Injectable, computed } from "@angular/core";
import { PlayerStore } from "./player.store";
import { PlayerApi } from "./player.api";
import { firstValueFrom } from "rxjs";
import { ToastService } from "@sports-ui/toast";

@Injectable({ providedIn: "root" })
export class PlayerFacade {
  private store = inject(PlayerStore);
  private api = inject(PlayerApi);
  private toast = inject(ToastService);

  // ----------------
  // Exposed state
  // ----------------
  readonly players = this.store.players;
  readonly status = this.store.status;
  readonly error = this.store.error;

  // ----------------
  // Actions
  // ----------------
  async loadPlayers(leagueId?: string) {
    this.store.setLoading();

    try {
      const players = await firstValueFrom(this.api.getAll(leagueId));

      this.store.setPlayers(players ?? []);
    } catch (e: any) {
      const message = e?.error?.message || "Unable to load players";

      this.store.setError(message);
      this.toast.error(message);
    }
  }

  selectPlayer(id: string) {
    this.store.selectPlayer(id);
  }
}
