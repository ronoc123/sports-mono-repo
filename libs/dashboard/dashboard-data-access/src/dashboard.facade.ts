import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { DashboardStore } from './dashboard.store';
import { DashboardService } from './service/dashboard.service';
import { SubmitPollVoteResult, SubmitTriviaAnswerResult } from './dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardFacade {
  private readonly store = inject(DashboardStore);
  private readonly service = inject(DashboardService);

  // Exposed signals
  readonly trendingPlayerOptions = this.store.trendingPlayerOptions;
  readonly activeTriviaSeries = this.store.activeTriviaSeries;
  readonly activePoll = this.store.activePoll;
  readonly status = this.store.status;
  readonly isLoading = this.store.isLoading;
  readonly hasContent = this.store.hasContent;
  readonly error = this.store.error;
  readonly triviaSubmitting = this.store.triviaSubmitting;
  readonly triviaSubmitError = this.store.triviaSubmitError;
  readonly pollSubmitting = this.store.pollSubmitting;
  readonly pollSubmitError = this.store.pollSubmitError;

  async load(organizationId: string, userId: string): Promise<void> {
    this.store.setLoading();
    try {
      const res = await firstValueFrom(this.service.getDashboard(organizationId, userId));
      if (res.data) {
        this.store.setDashboard(res.data);
      } else {
        this.store.setError(res.message ?? 'Failed to load dashboard');
      }
    } catch {
      this.store.setError('Failed to load dashboard');
    }
  }

  async submitPollVote(
    organizationId: string,
    userId: string,
    pollId: string,
    pollOptionId: string
  ): Promise<SubmitPollVoteResult | null> {
    this.store.setPollSubmitting(true);
    try {
      const res = await firstValueFrom(
        this.service.submitPollVote({ userId, organizationId, pollId, pollOptionId })
      );
      if (res.data) {
        this.store.markPollVoted(res.data.votedOptionId, res.data.options);
        return res.data;
      } else {
        this.store.setPollSubmitError(res.message ?? 'Failed to submit vote');
        return null;
      }
    } catch {
      this.store.setPollSubmitError('Failed to submit vote');
      return null;
    }
  }

  async submitTriviaAnswer(
    organizationId: string,
    userId: string,
    questionId: string,
    selectedAnswer: string
  ): Promise<SubmitTriviaAnswerResult | null> {
    this.store.setTriviaSubmitting(true);
    try {
      const res = await firstValueFrom(
        this.service.submitTriviaAnswer({
          userId,
          organizationId,
          triviaQuestionId: questionId,
          selectedAnswer,
        })
      );
      if (res.data) {
        this.store.markQuestionAnswered(questionId, selectedAnswer);
        return res.data;
      } else {
        this.store.setTriviaSubmitError(res.message ?? 'Failed to submit answer');
        return null;
      }
    } catch {
      this.store.setTriviaSubmitError('Failed to submit answer');
      return null;
    }
  }
}
