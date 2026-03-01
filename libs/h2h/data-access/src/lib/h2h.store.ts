import { computed } from '@angular/core';
import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from '@ngrx/signals';
import { UserCard } from '@sports-ui/cards-data-access';
import { CreateMatchResult, MatchSummaryDto } from './h2h.model';

export type H2HStatus = 'idle' | 'loading' | 'success' | 'error';

export interface H2HState {
  collection: UserCard[];
  selectedCardIds: string[];
  wagerAmount: number | null;
  matchResult: CreateMatchResult | null;
  matchHistory: MatchSummaryDto[];
  collectionStatus: H2HStatus;
  matchStatus: H2HStatus;
  historyStatus: H2HStatus;
  error: string | null;
  matchError: string | null;
  totalHistoryCount: number;
  historyPage: number;
}

const initialState: H2HState = {
  collection: [],
  selectedCardIds: [],
  wagerAmount: null,
  matchResult: null,
  matchHistory: [],
  collectionStatus: 'idle',
  matchStatus: 'idle',
  historyStatus: 'idle',
  error: null,
  matchError: null,
  totalHistoryCount: 0,
  historyPage: 1,
};

export const H2HStore = signalStore(
  { providedIn: 'root' },
  withState<H2HState>(initialState),

  withComputed((state) => ({
    isLoadingCollection: computed(() => state.collectionStatus() === 'loading'),
    isPlayingMatch: computed(() => state.matchStatus() === 'loading'),
    isLoadingHistory: computed(() => state.historyStatus() === 'loading'),

    selectedCards: computed(() =>
      state.collection().filter((c) => state.selectedCardIds().includes(c.id))
    ),

    squadOverall: computed(() => {
      const selected = state.collection().filter((c) =>
        state.selectedCardIds().includes(c.id)
      );
      if (selected.length === 0) return 0;
      return Math.round(
        selected.reduce((sum, c) => sum + c.overallRating, 0) / selected.length
      );
    }),

    canPlayMatch: computed(
      () =>
        state.selectedCardIds().length >= 1 &&
        state.selectedCardIds().length <= 5 &&
        (state.wagerAmount() ?? 0) > 0
    ),

    isSquadFull: computed(() => state.selectedCardIds().length >= 5),
  })),

  withMethods((store) => ({
    // ── Collection ────────────────────────────────────────────────────────
    setCollectionLoading() {
      patchState(store, { collectionStatus: 'loading', error: null });
    },
    setCollection(collection: UserCard[]) {
      patchState(store, { collection, collectionStatus: 'success' });
    },
    setCollectionError(error: string) {
      patchState(store, { error, collectionStatus: 'error' });
    },

    // ── Squad selection ───────────────────────────────────────────────────
    toggleCard(cardId: string) {
      const current = store.selectedCardIds();
      if (current.includes(cardId)) {
        patchState(store, { selectedCardIds: current.filter((id) => id !== cardId) });
      } else if (current.length < 5) {
        patchState(store, { selectedCardIds: [...current, cardId] });
      }
    },

    setWagerAmount(wagerAmount: number | null) {
      patchState(store, { wagerAmount });
    },

    // ── Match ─────────────────────────────────────────────────────────────
    setMatchPlaying() {
      patchState(store, { matchStatus: 'loading', matchError: null });
    },
    setMatchResult(matchResult: CreateMatchResult) {
      patchState(store, { matchResult, matchStatus: 'success' });
    },
    setMatchError(matchError: string) {
      patchState(store, { matchError, matchStatus: 'error' });
    },

    resetSquad() {
      patchState(store, {
        selectedCardIds: [],
        wagerAmount: null,
        matchResult: null,
        matchStatus: 'idle',
        matchError: null,
      });
    },

    // ── History ───────────────────────────────────────────────────────────
    setHistoryLoading() {
      patchState(store, { historyStatus: 'loading' });
    },
    setMatchHistory(matchHistory: MatchSummaryDto[], totalCount: number, page: number) {
      patchState(store, { matchHistory, totalHistoryCount: totalCount, historyPage: page, historyStatus: 'success' });
    },
    setHistoryError(error: string) {
      patchState(store, { error, historyStatus: 'error' });
    },
  }))
);
