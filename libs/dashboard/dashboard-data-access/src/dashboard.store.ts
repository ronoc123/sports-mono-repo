import { computed } from '@angular/core';
import { signalStore, withState, withMethods, withComputed, patchState } from '@ngrx/signals';
import {
  ActivePollDto,
  ActivePollOptionDto,
  ActiveTriviaQuestionDto,
  DashboardResponse,
  TrendingPlayerOptionDto,
} from './dashboard.model';

export type DashboardStatus = 'idle' | 'loading' | 'success' | 'error';

export interface DashboardState {
  trendingPlayerOptions: TrendingPlayerOptionDto[];
  activeTriviaQuestions: ActiveTriviaQuestionDto[];
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
  activeTriviaQuestions: [],
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
        state.activeTriviaQuestions().length > 0 ||
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
        activeTriviaQuestions: data.activeTriviaQuestions,
        activePoll: data.activePoll,
        status: 'success',
        error: null,
      });
    },
    setError(error: string) {
      patchState(store, { status: 'error', error });
    },

    // Trivia answer submission state
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
        activeTriviaQuestions: store.activeTriviaQuestions().map((q) =>
          q.questionId === questionId
            ? { ...q, answeredByMe: true, selectedAnswer }
            : q
        ),
      });
    },

    // Poll vote submission state
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
