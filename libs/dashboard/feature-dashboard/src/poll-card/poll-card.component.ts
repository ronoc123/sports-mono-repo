import {
  Component,
  input,
  output,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivePollDto, ActivePollOptionDto } from '@sports-ui/dashboard-data-access';

@Component({
  selector: 'lib-poll-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './poll-card.component.html',
  styleUrl: './poll-card.component.css',
})
export class PollCardComponent {
  readonly poll = input.required<ActivePollDto>();
  readonly submitting = input<boolean>(false);

  readonly voteSubmitted = output<{ pollId: string; pollOptionId: string }>();

  readonly pendingOptionId = signal<string | null>(null);

  readonly totalVotes = computed(() =>
    this.poll().options.reduce((sum, o) => sum + o.voteCount, 0)
  );

  readonly displayOptions = computed<(ActivePollOptionDto & { pct: number })[]>(() => {
    const total = this.totalVotes();
    return this.poll().options.map((o) => ({
      ...o,
      pct: total > 0 ? Math.round((o.voteCount / total) * 100) : 0,
    }));
  });

  select(optionId: string): void {
    if (this.poll().votedByMe || this.submitting()) return;
    this.pendingOptionId.set(optionId);
  }

  submit(): void {
    const optionId = this.pendingOptionId();
    if (!optionId || this.poll().votedByMe || this.submitting()) return;
    this.voteSubmitted.emit({ pollId: this.poll().pollId, pollOptionId: optionId });
  }

  isSelected(optionId: string): boolean {
    if (this.poll().votedByMe) return this.poll().selectedOptionId === optionId;
    return this.pendingOptionId() === optionId;
  }
}
