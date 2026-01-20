import { inject, Injectable } from "@angular/core";
import { PlayerOptionStore } from "./player-option.store";
import { PlayerOptionApi } from "./player-option-data-access";
import { GetAllPlayerOptionsQuery } from "./player-option.model";
import { firstValueFrom } from "rxjs";
import { OrganizationStore } from "@sports-ui/organization-data-access";
import { ToastService } from "@sports-ui/toast";

@Injectable({ providedIn: "root" })
export class PlayerOptionFeatureService {
  private store = inject(PlayerOptionStore);
  private api = inject(PlayerOptionApi);
  private organzationStore = inject(OrganizationStore);
  private toast = inject(ToastService);
  // expose state
  readonly options = this.store.options;
  readonly status = this.store.status;
  readonly error = this.store.error;
  readonly selectedOrganization = this.organzationStore.selectedOrganization;

  async loadPlayerOptions(query: GetAllPlayerOptionsQuery = {}) {
    this.store.setLoading();

    try {
      const res = await firstValueFrom(this.api.getPlayerOptions(query));

      const items = res.data ?? [];
      this.store.setOptions(items);
    } catch (e: any) {
      this.toast.error(e?.error?.message || "Unable to load player options");
    }
  }
}
