import { inject, Injectable } from "@angular/core";
import { PlayerOptionStore } from "./player-option.store";
import { PlayerOptionApi } from "./player-option-data-access";
import {
  GetAllPlayerOptionsQuery,
  PaginatedList,
  PlayerOptionDto,
  ServiceResponse,
} from "./playey-option.model";
import { firstValueFrom } from "rxjs";

@Injectable({ providedIn: "root" })
export class PlayerOptionFeatureService {
  private store = inject(PlayerOptionStore);
  private api = inject(PlayerOptionApi);

  async loadPlayerOptions(query: GetAllPlayerOptionsQuery = {}) {
    this.store.setLoading();
    try {
      const res: ServiceResponse<PaginatedList<PlayerOptionDto>> =
        await firstValueFrom(this.api.getPlayerOptions(query));

      const items = res.data?.items ?? [];
      this.store.setOptions(items);
    } catch (e: any) {
      this.store.setError(e?.message ?? "Failed to load");
    }
  }
}
