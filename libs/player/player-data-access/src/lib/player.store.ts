import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { PlayerDto } from "./models/player.model";

type Status = "idle" | "loading" | "error" | "success";

interface PlayerState {
  status: Status;
  error?: string;
  players: PlayerDto[];
  selectedId?: string;
}

export const PlayerStore = signalStore(
  withState<PlayerState>({
    status: "idle",
    players: [],
    error: undefined,
    selectedId: undefined,
  }),

  withMethods((store) => ({
    // ----------------
    // Status helpers
    // ----------------
    setLoading() {
      patchState(store, {
        status: "loading",
        error: undefined,
      });
    },

    setError(error: string) {
      patchState(store, {
        status: "error",
        error,
      });
    },

    setPlayers(players: PlayerDto[]) {
      patchState(store, {
        players,
        status: "success",
        error: undefined,
      });
    },

    // ----------------
    // Selection
    // ----------------
    selectPlayer(id: string) {
      patchState(store, {
        selectedId: id,
      });
    },

    reset() {
      patchState(store, {
        status: "idle",
        players: [],
        error: undefined,
        selectedId: undefined,
      });
    },
  }))
);
