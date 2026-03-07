import { Component, ViewChildren, QueryList, effect, inject } from '@angular/core';
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

  @ViewChildren(TriviaCardComponent)
  triviaCards!: QueryList<TriviaCardComponent>;

  constructor() {
    effect(() => {
      const org = this.organization();
      const user = this.currentUser();
      if (!org || !user) return;
      this.dashboard.load(org.id, user.id);
    });
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
      // Push result into the matching card for immediate inline feedback
      const card = this.triviaCards.find(
        (c) => c.question().questionId === event.questionId
      );
      card?.applyResult(result);
      // Balance refresh is handled automatically by the layout's SignalR connection
    }
  }
}
