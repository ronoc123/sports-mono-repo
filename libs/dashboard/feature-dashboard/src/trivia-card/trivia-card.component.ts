import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActiveTriviaQuestionDto, SubmitTriviaAnswerResult } from '@sports-ui/dashboard-data-access';

@Component({
  selector: 'lib-trivia-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './trivia-card.component.html',
  styleUrl: './trivia-card.component.css',
})
export class TriviaCardComponent {
  question = input.required<ActiveTriviaQuestionDto>();
  submitting = input<boolean>(false);

  answerSubmitted = output<{ questionId: string; selectedAnswer: string }>();

  readonly selectedOption = signal<string | null>(null);
  readonly result = signal<SubmitTriviaAnswerResult | null>(null);

  get isAnswered(): boolean {
    return this.question().answeredByMe || this.result() !== null;
  }

  get answeredOption(): string | null {
    return this.result()?.correctAnswer !== undefined
      ? this.question().selectedAnswer ?? this.selectedOption()
      : this.question().selectedAnswer;
  }

  selectOption(option: string): void {
    if (this.isAnswered || this.submitting()) return;
    this.selectedOption.set(option);
  }

  submit(): void {
    const opt = this.selectedOption();
    if (!opt || this.isAnswered || this.submitting()) return;
    this.answerSubmitted.emit({ questionId: this.question().questionId, selectedAnswer: opt });
  }

  /** Called by parent after successful API response */
  applyResult(res: SubmitTriviaAnswerResult): void {
    this.result.set(res);
  }
}
