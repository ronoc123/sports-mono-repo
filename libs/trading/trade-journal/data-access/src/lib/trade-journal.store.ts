import { inject } from '@angular/core';
import { signalStore, withState, withMethods, withComputed, patchState } from '@ngrx/signals';
import { computed } from '@angular/core';
import { TradeJournalApi } from './trade-journal.api';
import { Trade, UpsertJournalEntryRequest } from './trade-journal.models';

interface TradeJournalState {
  trades: Trade[];
  selectedTrade: Trade | null;
  loading: boolean;
  saving: boolean;
  error: string | null;
}

const initialState: TradeJournalState = {
  trades: [],
  selectedTrade: null,
  loading: false,
  saving: false,
  error: null,
};

export const TradeJournalStore = signalStore(
  { providedIn: 'root' },
  withState<TradeJournalState>(initialState),
  withComputed(({ trades, selectedTrade }) => ({
    tradeCount: computed(() => trades().length),
    completedPhases: computed(() => selectedTrade()?.journalEntries.length ?? 0),
  })),
  withMethods((store, api = inject(TradeJournalApi)) => ({
    async loadTrades() {
      patchState(store, { loading: true, error: null });
      try {
        const trades = await api.getMyTrades().toPromise();
        patchState(store, { trades: trades ?? [], loading: false });
      } catch {
        patchState(store, { error: 'Failed to load trades', loading: false });
      }
    },

    async loadTrade(id: string) {
      patchState(store, { loading: true, error: null });
      try {
        const trade = await api.getTrade(id).toPromise();
        patchState(store, { selectedTrade: trade ?? null, loading: false });
      } catch {
        patchState(store, { error: 'Failed to load trade', loading: false });
      }
    },

    async createTrade(symbol: string, notes?: string): Promise<Trade | undefined> {
      patchState(store, { saving: true, error: null });
      try {
        const trade = await api.createTrade({ symbol, notes }).toPromise();
        if (trade) {
          patchState(store, (s) => ({
            trades: [trade, ...s.trades],
            saving: false,
          }));
        }
        return trade;
      } catch {
        patchState(store, { error: 'Failed to create trade', saving: false });
        return undefined;
      }
    },

    async upsertEntry(tradeId: string, req: UpsertJournalEntryRequest) {
      patchState(store, { saving: true, error: null });
      try {
        const entry = await api.upsertEntry(tradeId, req).toPromise();
        // Refresh selected trade
        const trade = await api.getTrade(tradeId).toPromise();
        patchState(store, { selectedTrade: trade ?? null, saving: false });
        return entry;
      } catch {
        patchState(store, { error: 'Failed to save journal entry', saving: false });
        return undefined;
      }
    },
  }))
);
