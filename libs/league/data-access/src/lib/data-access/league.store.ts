import { computed } from "@angular/core";
import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from "@ngrx/signals";
import { LeagueDto } from "./league.model";

export type LeagueStoreStatus = "idle" | "loading" | "success" | "error";

export interface LeagueState {
  leagues: LeagueDto[];
  status: LeagueStoreStatus;
  saveStatus: LeagueStoreStatus;
  error: string | null;
  saveError: string | null;
  selectedLeagueId: string | null;
}

const initialState: LeagueState = {
  leagues: [],
  status: "idle",
  saveStatus: "idle",
  error: null,
  saveError: null,
  selectedLeagueId: localStorage.getItem("selectedLeague") || null,
};

export const LeagueStore = signalStore(
  { providedIn: "root" },
  withState<LeagueState>(initialState),

  withComputed((state) => ({
    hasLeagues: computed(() => state.leagues().length > 0),
    isLoading: computed(() => state.status() === "loading"),
    isSaving: computed(() => state.saveStatus() === "loading"),
  })),

  withMethods((store) => ({
    // ───── Load ─────
    setLoading() {
      patchState(store, { status: "loading", error: null });
    },
    setLeagues(leagues: LeagueDto[]) {
      patchState(store, { leagues, status: "success" });
    },
    setError(error: string) {
      patchState(store, { error, status: "error" });
    },

    // ───── Create ─────
    setSaving() {
      patchState(store, { saveStatus: "loading", saveError: null });
    },
    addLeague(league: LeagueDto) {
      patchState(store, {
        leagues: [...store.leagues(), league],
        saveStatus: "success",
      });
    },
    setSaveError(saveError: string) {
      patchState(store, { saveError, saveStatus: "error" });
    },
    resetSaveState() {
      patchState(store, { saveStatus: "idle", saveError: null });
    },
    setSelectedLeague(id: string) {
      patchState(store, { selectedLeagueId: id });
    },
  }))
);
