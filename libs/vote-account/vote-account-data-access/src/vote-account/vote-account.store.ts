import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from "@ngrx/signals";
import { computed } from "@angular/core";
import { VoteAccount } from "./vote-account.model";

export interface VoteAccountState {
  loading: boolean;
  error: string | null;
  account: VoteAccount | null;
}

const initialState: VoteAccountState = {
  loading: false,
  error: null,
  account: null,
};

export const VoteAccountStore = signalStore(
  withState<VoteAccountState>(initialState),

  withComputed((state) => ({
    balance: computed(() => state.account()?.balance ?? 0),
    transactions: computed(() => state.account()?.transactions ?? []),
    hasTransactions: computed(
      () => (state.account()?.transactions?.length ?? 0) > 0
    ),
  })),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { loading: true, error: null });
    },

    setAccount(account: VoteAccount | null) {
      patchState(store, { account, loading: false });
    },

    setError(message: string) {
      patchState(store, { error: message, loading: false });
    },

    clear() {
      patchState(store, initialState);
    },
  }))
);
