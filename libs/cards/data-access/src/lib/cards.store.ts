import { computed } from '@angular/core';
import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from '@ngrx/signals';
import { CardPlayer, UserCard, PackPurchaseResult } from './cards.model';

export type CardStoreStatus = 'idle' | 'loading' | 'success' | 'error';

export interface CardState {
  cardPlayers: CardPlayer[];
  collection: UserCard[];
  status: CardStoreStatus;
  saveStatus: CardStoreStatus;
  packStatus: CardStoreStatus;
  error: string | null;
  saveError: string | null;
  packError: string | null;
  lastPackResult: PackPurchaseResult | null;
}

const initialState: CardState = {
  cardPlayers: [],
  collection: [],
  status: 'idle',
  saveStatus: 'idle',
  packStatus: 'idle',
  error: null,
  saveError: null,
  packError: null,
  lastPackResult: null,
};

export const CardStore = signalStore(
  { providedIn: 'root' },
  withState<CardState>(initialState),

  withComputed((state) => ({
    hasCards: computed(() => state.cardPlayers().length > 0),
    hasCollection: computed(() => state.collection().length > 0),
    isLoading: computed(() => state.status() === 'loading'),
    isSaving: computed(() => state.saveStatus() === 'loading'),
    isPurchasing: computed(() => state.packStatus() === 'loading'),
  })),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: 'loading', error: null });
    },
    setCardPlayers(cardPlayers: CardPlayer[]) {
      patchState(store, { cardPlayers, status: 'success' });
    },
    setError(error: string) {
      patchState(store, { error, status: 'error' });
    },

    setSaving() {
      patchState(store, { saveStatus: 'loading', saveError: null });
    },
    addCardPlayer(card: CardPlayer) {
      patchState(store, {
        cardPlayers: [...store.cardPlayers(), card],
        saveStatus: 'success',
      });
    },
    replaceCardPlayer(card: CardPlayer) {
      patchState(store, {
        cardPlayers: store.cardPlayers().map((c) => (c.id === card.id ? card : c)),
        saveStatus: 'success',
      });
    },
    setSaveError(saveError: string) {
      patchState(store, { saveError, saveStatus: 'error' });
    },
    resetSaveState() {
      patchState(store, { saveStatus: 'idle', saveError: null });
    },

    setPackPurchasing() {
      patchState(store, { packStatus: 'loading', packError: null, lastPackResult: null });
    },
    setPackResult(result: PackPurchaseResult) {
      patchState(store, { packStatus: 'success', lastPackResult: result });
    },
    setPackError(packError: string) {
      patchState(store, { packStatus: 'error', packError });
    },
    resetPackState() {
      patchState(store, { packStatus: 'idle', packError: null, lastPackResult: null });
    },

    setCollection(collection: UserCard[]) {
      patchState(store, { collection, status: 'success' });
    },
  }))
);
