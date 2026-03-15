import { inject } from '@angular/core';
import { signalStore, withState, withMethods, withComputed, patchState } from '@ngrx/signals';
import { computed } from '@angular/core';
import { BacktestApi } from './backtest.api';
import { BacktestStatus, AnswerInput, SubmitMetricsRequest } from './backtest.models';

interface BacktestState {
  status: BacktestStatus | null;
  loading: boolean;
  error: string | null;
  submittingMetrics: boolean;
  submittingAnswers: boolean;
}

const initialState: BacktestState = {
  status: null,
  loading: false,
  error: null,
  submittingMetrics: false,
  submittingAnswers: false,
};

export const BacktestStore = signalStore(
  { providedIn: 'root' },
  withState<BacktestState>(initialState),
  withComputed(({ status }) => ({
    isGateUnlocked: computed(() => status()?.isGateUnlocked ?? false),
    expectancy: computed(() => status()?.expectancy ?? null),
    answeredCount: computed(() => status()?.answeredCount ?? 0),
    metricsSubmitted: computed(() => status()?.metricsSubmitted ?? false),
    answers: computed(() => status()?.answers ?? []),
  })),
  withMethods((store, api = inject(BacktestApi)) => ({
    async loadStatus() {
      patchState(store, { loading: true, error: null });
      try {
        const status = await api.getStatus().toPromise();
        patchState(store, { status: status ?? null, loading: false });
      } catch {
        patchState(store, { error: 'Failed to load backtest status', loading: false });
      }
    },

    async createSession() {
      patchState(store, { loading: true, error: null });
      try {
        await api.createSession().toPromise();
        const status = await api.getStatus().toPromise();
        patchState(store, { status: status ?? null, loading: false });
      } catch {
        patchState(store, { error: 'Failed to create backtest session', loading: false });
      }
    },

    async submitMetrics(req: SubmitMetricsRequest) {
      patchState(store, { submittingMetrics: true, error: null });
      try {
        const status = await api.submitMetrics(req).toPromise();
        patchState(store, { status: status ?? null, submittingMetrics: false });
      } catch {
        patchState(store, { error: 'Failed to submit metrics', submittingMetrics: false });
      }
    },

    async submitAnswers(answers: AnswerInput[]) {
      patchState(store, { submittingAnswers: true, error: null });
      try {
        const status = await api.submitAnswers(answers).toPromise();
        patchState(store, { status: status ?? null, submittingAnswers: false });
      } catch {
        patchState(store, { error: 'Failed to submit answers', submittingAnswers: false });
      }
    },
  }))
);
