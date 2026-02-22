import { computed, inject, Injectable } from "@angular/core";
import { firstValueFrom } from "rxjs";
import { RoasterApi } from "./roaster.api";
import { RoasterStore } from "./roaster.store";
import { ToastService } from "@sports-ui/toast";

@Injectable({ providedIn: "root" })
export class RoasterFacade {
  private api = inject(RoasterApi);
  private store = inject(RoasterStore);
  private toast = inject(ToastService);

  readonly players = this.store.players;
  readonly loading = computed(() => this.store.status() === "loading");
  readonly error = computed(() => this.store.error() || null);

  async loadRoster(organizationId: string): Promise<void> {
    this.store.setLoading();
    try {
      const res = await firstValueFrom(this.api.getRosterByOrganization(organizationId));
      this.store.setPlayers(res?.data ?? []);
    } catch (e: any) {
      const msg = e?.error?.message || "Failed to load roster.";
      this.store.setError(msg);
      this.toast.error(msg);
    }
  }
}
