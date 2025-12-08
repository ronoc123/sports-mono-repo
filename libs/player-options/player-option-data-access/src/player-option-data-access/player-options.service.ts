import { inject, Injectable } from "@angular/core";
import { PlayerOptionStore } from "./player-option.store";
import { PlayerOptionApi } from "./player-option-data-access";
import { GetAllPlayerOptionsQuery } from "./player-option.model";
import { firstValueFrom } from "rxjs";
import { OrganizationStore } from "@sports-ui/organization-data-access";

@Injectable({ providedIn: "root" })
export class PlayerOptionFeatureService {
  private store = inject(PlayerOptionStore);
  private api = inject(PlayerOptionApi);
  private organzationStore = inject(OrganizationStore);
  // expose state
  readonly options = this.store.options;
  readonly status = this.store.status;
  readonly error = this.store.error;
  readonly selectedOrganization = this.organzationStore.selectedOrganization;

  async loadPlayerOptions(query: GetAllPlayerOptionsQuery = {}) {
    this.store.setLoading();

    try {
      const res = await firstValueFrom(this.api.getPlayerOptions(query));

      const items = res.data?.items ?? [];
      this.store.setOptions(items);
    } catch (e: any) {
      this.store.setError(e?.message ?? "Failed to load player options");
    }
  }
}
