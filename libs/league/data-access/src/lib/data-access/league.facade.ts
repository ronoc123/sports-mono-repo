import { Injectable, effect, inject } from "@angular/core";
import { firstValueFrom } from "rxjs";
import { LeagueStore } from "./league.store";
import { LeaguesApi } from "./league.api";
import { CreateLeagueRequest } from "./league.model";

@Injectable({ providedIn: "root" })
export class LeaguesFacade {
  private readonly store = inject(LeagueStore);
  private readonly api = inject(LeaguesApi);
  readonly selectedLeagueId = this.store.selectedLeagueId;
  // ─── Exposed Signals ─────────────────────
  readonly leagues = this.store.leagues;
  readonly isLoading = this.store.isLoading;
  readonly isSaving = this.store.isSaving;
  readonly error = this.store.error;
  readonly saveError = this.store.saveError;

  resetSaveState() {
    this.store.resetSaveState();
  }

  async loadLeagues(
    pageNumber = 1,
    pageSize = 10,
    searchTerm?: string,
    sortBy = "Name",
    sortDescending = false
  ): Promise<void> {
    this.store.setLoading();

    try {
      const res = await firstValueFrom(
        this.api.getAllLeagues(
          pageNumber,
          pageSize,
          searchTerm,
          sortBy,
          sortDescending
        )
      );

      this.store.setLeagues(res.data ?? []);
    } catch {
      this.store.setError("Failed to load leagues");
    }
  }

  async createLeague(request: CreateLeagueRequest): Promise<boolean> {
    this.store.setSaving();

    try {
      const res = await firstValueFrom(this.api.createLeague(request));

      if (res.data) {
        // backend returns LeagueId (Guid)
        this.store.addLeague({
          id: res.data,
          name: request.name,
          createdAt: new Date().toISOString(),
        });

        return true;
      }

      this.store.setSaveError(res.message ?? "Failed to create league");
      return false;
    } catch (err: any) {
      this.store.setSaveError(
        err?.error?.message ?? err?.message ?? "Failed to create league"
      );
      return false;
    }
  }

  setSelectedLeague(id: string) {
    this.store.setSelectedLeague(id);
  }
}
