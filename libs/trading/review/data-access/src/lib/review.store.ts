import { inject } from '@angular/core';
import { signalStore, withState, withMethods, withComputed, patchState } from '@ngrx/signals';
import { computed } from '@angular/core';
import { ReviewApi } from './review.api';
import { ReviewSession, ReviewStatus } from './review.models';

interface ReviewState {
  status: ReviewStatus | null;
  session: ReviewSession | null;
  loading: boolean;
  sending: boolean;
  error: string | null;
}

export const ReviewStore = signalStore(
  { providedIn: 'root' },
  withState<ReviewState>({ status: null, session: null, loading: false, sending: false, error: null }),
  withComputed(({ status }) => ({
    isReady: computed(() => status()?.isReady ?? false),
    tradeCount: computed(() => status()?.tradeCount ?? 0),
    tradesRequired: computed(() => status()?.tradesRequired ?? 8),
    tradesNeeded: computed(() => Math.max(0, (status()?.tradesRequired ?? 8) - (status()?.tradeCount ?? 0))),
  })),
  withMethods((store, api = inject(ReviewApi)) => ({
    async loadStatus() {
      patchState(store, { loading: true, error: null });
      try {
        const status = await api.getStatus().toPromise();
        patchState(store, { status: status ?? null, loading: false });
      } catch {
        patchState(store, { error: 'Failed to load review status', loading: false });
      }
    },

    async loadSession(sessionId: string) {
      patchState(store, { loading: true, error: null });
      try {
        const session = await api.getSession(sessionId).toPromise();
        patchState(store, { session: session ?? null, loading: false });
      } catch {
        patchState(store, { error: 'Failed to load review session', loading: false });
      }
    },

    async sendMessage(message: string) {
      patchState(store, { sending: true, error: null });
      try {
        const existingId = store.session()?.id ?? null;
        const session = await api.sendMessage(existingId, message).toPromise();
        patchState(store, { session: session ?? null, sending: false });
      } catch {
        patchState(store, { error: 'Failed to send message', sending: false });
      }
    },
  }))
);
