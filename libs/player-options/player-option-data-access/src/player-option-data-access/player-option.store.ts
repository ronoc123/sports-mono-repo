import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { PlayerOptionDto } from "./playey-option.model";

type Status = "idle" | "loading" | "error" | "success";

interface PlayerOptionState {
  status: Status;
  error?: string;
  options: PlayerOptionDto[];
  lastVotedId?: string;
}

const initialState: PlayerOptionState = {
  status: "idle",
  options: [],
};

export const PlayerOptionStore = signalStore(
  withState(initialState),
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
