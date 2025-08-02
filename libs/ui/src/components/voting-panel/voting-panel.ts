import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { PlayerOption } from '@sports-ui/api-types';

@Component({
  selector: 'lib-voting-panel',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatChipsModule,
    MatTooltipModule,
  ],
  templateUrl: './voting-panel.html',
  styleUrl: './voting-panel.css',
})
export class VotingPanelComponent {
  playerOption = input.required<PlayerOption>();
  userVotesAvailable = input<number>(0);
  hasUserVoted = input<boolean>(false);
  canVote = input<boolean>(true);
  loading = input<boolean>(false);
  
  // Outputs
  vote = output<PlayerOption>();
  removeVote = output<PlayerOption>();
  viewDetails = output<PlayerOption>();
  
  // Local state
  isExpanded = signal<boolean>(false);

  onVote() {
    if (this.canVote() && !this.loading()) {
      this.vote.emit(this.playerOption());
    }
  }

  onRemoveVote() {
    if (this.hasUserVoted() && !this.loading()) {
      this.removeVote.emit(this.playerOption());
    }
  }

  onViewDetails() {
    this.viewDetails.emit(this.playerOption());
  }

  toggleExpanded() {
    this.isExpanded.set(!this.isExpanded());
  }

  getVotePercentage(): number {
    const option = this.playerOption();
    // This would need to be calculated based on total votes across all options
    // For now, we'll use a simple calculation
    const maxVotes = Math.max(option.votes, 100);
    return (option.votes / maxVotes) * 100;
  }

  getTimeRemainingText(): string {
    const option = this.playerOption();
    if (option.isExpired) return 'Expired';
    if (option.daysRemaining <= 0) return 'Expires today';
    if (option.daysRemaining === 1) return '1 day remaining';
    return `${option.daysRemaining} days remaining`;
  }

  getStatusColor(): string {
    const option = this.playerOption();
    if (option.isExpired) return 'warn';
    if (option.isTrending) return 'primary';
    if (option.isPopular) return 'accent';
    return '';
  }

  getStatusIcon(): string {
    const option = this.playerOption();
    if (option.isExpired) return 'schedule';
    if (option.isTrending) return 'trending_up';
    if (option.isPopular) return 'star';
    return 'how_to_vote';
  }

  getVoteButtonText(): string {
    if (this.loading()) return 'Voting...';
    if (this.hasUserVoted()) return 'Remove Vote';
    return 'Vote';
  }

  getVoteButtonIcon(): string {
    if (this.hasUserVoted()) return 'thumb_down';
    return 'thumb_up';
  }

  canUserVote(): boolean {
    return this.canVote() && 
           !this.loading() && 
           !this.playerOption().isExpired && 
           this.userVotesAvailable() > 0;
  }
}
