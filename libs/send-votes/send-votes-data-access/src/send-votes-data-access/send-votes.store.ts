import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { SendVotesUserInfo } from "./send-votes.model";

type Status = "idle" | "loading" | "error" | "success";

interface SendVotesState {
  status: Status;
  error: string;
  users: SendVotesUserInfo[];
}

export const SendVotesStore = signalStore(
  withState<SendVotesState>({
    status: "idle",
    error: "",
    users: [],
  }),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: "loading", error: "" });
    },

    setError(msg: string) {
      patchState(store, { status: "error", error: msg });
    },

    setSuccess() {
      patchState(store, { status: "success", error: "" });
    },

    setUsers(users: SendVotesUserInfo[]) {
      patchState(store, { users });
    },
  }))
);
