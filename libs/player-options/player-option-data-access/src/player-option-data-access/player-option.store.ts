import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { PlayerOptionDto } from "./player-option.model";

type Status = "idle" | "loading" | "error" | "success";

interface PlayerOptionState {
  status: Status;
  error?: string;
  options: PlayerOptionDto[];
  lastVotedId?: string;
}

export const PlayerOptionStore = signalStore(
  withState<PlayerOptionState>({
    status: "idle",
    options: [],
    error: undefined,
  }),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: "loading", error: undefined });
    },

    setError(msg: string) {
      patchState(store, { status: "error", error: msg });
    },

    setOptions(opts: PlayerOptionDto[]) {
      patchState(store, { options: opts, status: "success" });
    },
  }))
);
