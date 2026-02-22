import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { RosterPlayer } from "./roaster.model";

type Status = "idle" | "loading" | "error" | "success";

interface RoasterState {
  status: Status;
  error: string;
  players: RosterPlayer[];
}

export const RoasterStore = signalStore(
  { providedIn: "root" },
  withState<RoasterState>({
    status: "idle",
    error: "",
    players: [],
  }),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: "loading", error: "" });
    },

    setError(msg: string) {
      patchState(store, { status: "error", error: msg });
    },

    setPlayers(players: RosterPlayer[]) {
      patchState(store, { status: "success", players, error: "" });
    },
  }))
);
