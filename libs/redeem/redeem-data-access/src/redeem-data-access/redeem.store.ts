import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";

type Status = "idle" | "loading" | "error" | "success";

interface RedeemState {
  status: Status;
  error: string;
  lastVotedId?: string;
}

export const RedeemStore = signalStore(
  withState<RedeemState>({
    status: "idle",
    error: "",
  }),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: "loading", error: "" });
    },

    setError(msg: string) {
      patchState(store, { status: "error", error: msg });
    },

    setSuccess() {
      patchState(store, { status: "success", error: undefined });
    },
  }))
);
