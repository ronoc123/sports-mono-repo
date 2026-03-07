import { computed } from '@angular/core';
import { signalStore, withState, withMethods, withComputed, patchState } from '@ngrx/signals';
import {
  ActivePollDto,
  ActivePollOptionDto,
  ActiveTriviaSeriesDto,
  DashboardResponse,
  TrendingPlayerOptionDto,
} from './dashboard.model';

export type DashboardStatus = 'idle' | 'loading' | 'success' | 'error';

export interface DashboardState {
  trendingPlayerOptions: TrendingPlayerOptionDto[];
  activeTriviaSeries: ActiveTriviaSeriesDto[];
  activePoll: ActivePollDto | null;
  status: DashboardStatus;
  error: string | null;
  triviaSubmitting: boolean;
  triviaSubmitError: string | null;
  pollSubmitting: boolean;
  pollSubmitError: string | null;
}

const initialState: DashboardState = {
  trendingPlayerOptions: [],
  activeTriviaSeries: [],
  activePoll: null,
  status: 'idle',
  error: null,
  triviaSubmitting: false,
  triviaSubmitError: null,
  pollSubmitting: false,
  pollSubmitError: null,
};

export const DashboardStore = signalStore(
  { providedIn: 'root' },
  withState<DashboardState>(initialState),

  withComputed((state) => ({
    isLoading: computed(() => state.status() === 'loading'),
    hasContent: computed(
      () =>
        state.trendingPlayerOptions().length > 0 ||
        state.activeTriviaSeries().some(s => s.questions.length > 0) ||
        state.activePoll() != null
    ),
  })),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: 'loading', error: null });
    },
    setDashboard(data: DashboardResponse) {
      patchState(store, {
        trendingPlayerOptions: data.trendingPlayerOptions,
        activeTriviaSeries: data.activeTriviaSeries,
        activePoll: data.activePoll,
        status: 'success',
        error: null,
      });
    },
    setError(error: string) {
      patchState(store, { status: 'error', error });
    },

    setTriviaSubmitting(triviaSubmitting: boolean) {
      patchState(store, { triviaSubmitting, triviaSubmitError: null });
    },
    setTriviaSubmitError(triviaSubmitError: string) {
      patchState(store, { triviaSubmitting: false, triviaSubmitError });
    },
    markQuestionAnswered(questionId: string, selectedAnswer: string) {
      patchState(store, {
        triviaSubmitting: false,
        triviaSubmitError: null,
        activeTriviaSeries: store.activeTriviaSeries().map(series => ({
          ...series,
          questions: series.questions.map(q =>
            q.questionId === questionId
              ? { ...q, answeredByMe: true, selectedAnswer }
              : q
          ),
        })),
      });
    },

    setPollSubmitting(pollSubmitting: boolean) {
      patchState(store, { pollSubmitting, pollSubmitError: null });
    },
    setPollSubmitError(pollSubmitError: string) {
      patchState(store, { pollSubmitting: false, pollSubmitError });
    },
    markPollVoted(votedOptionId: string, updatedOptions: ActivePollOptionDto[]) {
      const poll = store.activePoll();
      if (!poll) return;
      patchState(store, {
        pollSubmitting: false,
        pollSubmitError: null,
        activePoll: {
          ...poll,
          votedByMe: true,
          selectedOptionId: votedOptionId,
          options: updatedOptions,
        },
      });
    },
  }))
);
