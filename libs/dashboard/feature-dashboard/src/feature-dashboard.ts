import { Component, ViewChildren, QueryList, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthStore } from '@sports-ui/auth-data-access';
import { OrganizationFeatureService } from '@sports-ui/organization-data-access';
import { DashboardFacade } from '@sports-ui/dashboard-data-access';
import { TrendingFeedComponent } from './trending-feed/trending-feed.component';
import { TriviaCardComponent } from './trivia-card/trivia-card.component';
import { PollCardComponent } from './poll-card/poll-card.component';

@Component({
  selector: 'lib-feature-dashboard',
  standalone: true,
  imports: [CommonModule, TrendingFeedComponent, TriviaCardComponent, PollCardComponent],
  templateUrl: './feature-dashboard.html',
  styleUrl: './feature-dashboard.css',
})
export class FeatureDashboard {
  private readonly authStore = inject(AuthStore);
  private readonly orgService = inject(OrganizationFeatureService);
  readonly dashboard = inject(DashboardFacade);

  readonly currentUser = this.authStore.user;
  readonly organization = this.orgService.selectedOrganization;

  /** Per-series stepper index: { [seriesId]: currentIndex } */
  private readonly _triviaIndices = signal<Record<string, number>>({});

  /** Per-series answered-in-session set: { [seriesId]: Set<questionId> } */
  private readonly _answeredInSession = signal<Record<string, ReadonlySet<string>>>({});

  @ViewChildren(TriviaCardComponent)
  triviaCards!: QueryList<TriviaCardComponent>;

  constructor() {
    effect(() => {
      const org = this.organization();
      const user = this.currentUser();
      if (!org || !user) return;
      this.dashboard.load(org.id, user.id);
      this._triviaIndices.set({});
      this._answeredInSession.set({});
    });
  }

  getTriviaIndex(seriesId: string): number {
    return this._triviaIndices()[seriesId] ?? 0;
  }

  isTriviaAnswered(seriesId: string, index: number): boolean {
    const series = this.dashboard.activeTriviaSeries().find(s => s.seriesId === seriesId);
    const q = series?.questions[index];
    if (!q) return false;
    return q.answeredByMe || (this._answeredInSession()[seriesId]?.has(q.questionId) ?? false);
  }

  goToTrivia(seriesId: string, i: number): void {
    this._triviaIndices.update(map => ({ ...map, [seriesId]: i }));
  }

  prevTrivia(seriesId: string): void {
    const current = this.getTriviaIndex(seriesId);
    if (current > 0) this._triviaIndices.update(map => ({ ...map, [seriesId]: current - 1 }));
  }

  nextTrivia(seriesId: string, total: number): void {
    const current = this.getTriviaIndex(seriesId);
    if (current < total - 1) this._triviaIndices.update(map => ({ ...map, [seriesId]: current + 1 }));
  }

  async onPollVoteSubmitted(
    event: { pollId: string; pollOptionId: string }
  ): Promise<void> {
    const org = this.organization();
    const user = this.currentUser();
    if (!org || !user) return;
    await this.dashboard.submitPollVote(org.id, user.id, event.pollId, event.pollOptionId);
  }

  async onTriviaAnswerSubmitted(
    event: { questionId: string; selectedAnswer: string }
  ): Promise<void> {
    const org = this.organization();
    const user = this.currentUser();
    if (!org || !user) return;

    const result = await this.dashboard.submitTriviaAnswer(
      org.id,
      user.id,
      event.questionId,
      event.selectedAnswer
    );

    if (result) {
      const card = this.triviaCards.find(
        (c) => c.question().questionId === event.questionId
      );
      card?.applyResult(result);

      // Mark answered in session for dot indicator
      const seriesList = this.dashboard.activeTriviaSeries();
      for (const series of seriesList) {
        const qIdx = series.questions.findIndex(q => q.questionId === event.questionId);
        if (qIdx === -1) continue;

        this._answeredInSession.update(map => {
          const prev = map[series.seriesId] ?? new Set<string>();
          const next = new Set(prev);
          next.add(event.questionId);
          return { ...map, [series.seriesId]: next };
        });

        // Auto-advance to next question in this series
        if (qIdx < series.questions.length - 1) {
          setTimeout(() => this.goToTrivia(series.seriesId, qIdx + 1), 900);
        }
        break;
      }
    }
  }
}
