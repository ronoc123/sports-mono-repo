import { computed } from '@angular/core';
import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from '@ngrx/signals';
import { TriviaSeriesDto } from './trivia-management.model';

export type TriviaStatus = 'idle' | 'loading' | 'success' | 'error';

export interface TriviaManagementState {
  series: TriviaSeriesDto[];
  status: TriviaStatus;
  saveStatus: TriviaStatus;
  error: string | null;
  saveError: string | null;
  selectedSeriesId: string | null;
}

const initialState: TriviaManagementState = {
  series: [],
  status: 'idle',
  saveStatus: 'idle',
  error: null,
  saveError: null,
  selectedSeriesId: null,
};

export const TriviaManagementStore = signalStore(
  { providedIn: 'root' },
  withState<TriviaManagementState>(initialState),

  withComputed((state) => ({
    isLoading: computed(() => state.status() === 'loading'),
    isSaving: computed(() => state.saveStatus() === 'loading'),
    selectedSeries: computed(() =>
      state.series().find((s) => s.seriesId === state.selectedSeriesId()) ?? null
    ),
  })),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: 'loading', error: null });
    },
    setSeries(series: TriviaSeriesDto[]) {
      patchState(store, { series, status: 'success' });
    },
    setError(error: string) {
      patchState(store, { error, status: 'error' });
    },

    setSaving() {
      patchState(store, { saveStatus: 'loading', saveError: null });
    },
    setSaveSuccess() {
      patchState(store, { saveStatus: 'success', saveError: null });
    },
    setSaveError(saveError: string) {
      patchState(store, { saveError, saveStatus: 'error' });
    },

    selectSeries(seriesId: string | null) {
      patchState(store, { selectedSeriesId: seriesId });
    },

    prependSeries(newSeries: TriviaSeriesDto) {
      patchState(store, { series: [newSeries, ...store.series()], saveStatus: 'success' });
    },

    applySeriesUpdate(series: TriviaSeriesDto[]) {
      patchState(store, { series, saveStatus: 'success', saveError: null });
    },
  }))
);
