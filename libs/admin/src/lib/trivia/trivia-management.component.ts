import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TriviaManagementFacade } from './trivia-management.facade';
import { OrganizationFeatureService } from '@sports-ui/organization-data-access';
import { AddTriviaQuestionRequest, TriviaQuestionDto } from './trivia-management.model';

@Component({
  selector: 'lib-trivia-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trivia-management.component.html',
  styleUrl: './trivia-management.component.css',
})
export class TriviaManagementComponent implements OnInit {
  private readonly facade = inject(TriviaManagementFacade);
  private readonly orgService = inject(OrganizationFeatureService);

  // ── Facade signals ────────────────────────────────────────────────────────
  readonly series = this.facade.series;
  readonly isLoading = this.facade.isLoading;
  readonly isSaving = this.facade.isSaving;
  readonly error = this.facade.error;
  readonly saveError = this.facade.saveError;
  readonly selectedSeries = this.facade.selectedSeries;
  readonly selectedSeriesId = this.facade.selectedSeriesId;

  // ── Create series form ────────────────────────────────────────────────────
  readonly showCreateSeriesForm = signal(false);
  readonly newSeriesName = signal('');

  // ── Add question form ─────────────────────────────────────────────────────
  readonly showAddQuestionForm = signal(false);
  readonly newQuestionText = signal('');
  readonly newAnswerOptions = signal(['', '', '', '']);
  readonly newCorrectAnswer = signal('');
  readonly newVoteReward = signal(10);

  async ngOnInit(): Promise<void> {
    const orgId = this.orgService.selectedOrganization()?.id;
    if (orgId) await this.facade.loadSeries(orgId);
  }

  get orgId(): string | undefined {
    return this.orgService.selectedOrganization()?.id;
  }

  // ── Series actions ────────────────────────────────────────────────────────
  async submitCreateSeries(): Promise<void> {
    const orgId = this.orgId;
    const name = this.newSeriesName().trim();
    if (!orgId || !name) return;
    await this.facade.createSeries(orgId, name);
    this.newSeriesName.set('');
    this.showCreateSeriesForm.set(false);
  }

  // ── Question actions ──────────────────────────────────────────────────────
  async submitAddQuestion(): Promise<void> {
    const orgId = this.orgId;
    const seriesId = this.selectedSeriesId();
    if (!orgId || !seriesId) return;

    const options = this.newAnswerOptions().filter((o) => o.trim().length > 0);
    const req: AddTriviaQuestionRequest = {
      seriesId,
      organizationId: orgId,
      questionText: this.newQuestionText().trim(),
      answerOptions: options,
      correctAnswer: this.newCorrectAnswer().trim(),
      voteRewardAmount: this.newVoteReward(),
    };
    await this.facade.addQuestion(orgId, req);
    this.resetAddQuestionForm();
  }

  async publishQuestion(questionId: string): Promise<void> {
    const orgId = this.orgId;
    if (!orgId) return;
    await this.facade.publishQuestion(orgId, questionId);
  }

  async archiveQuestion(questionId: string): Promise<void> {
    const orgId = this.orgId;
    if (!orgId) return;
    await this.facade.archiveQuestion(orgId, questionId);
  }

  // ── Helpers ───────────────────────────────────────────────────────────────
  selectSeries(seriesId: string): void {
    this.facade.selectSeries(seriesId === this.selectedSeriesId() ? null : seriesId);
    this.showAddQuestionForm.set(false);
  }

  updateAnswerOption(index: number, value: string): void {
    const opts = [...this.newAnswerOptions()];
    opts[index] = value;
    this.newAnswerOptions.set(opts);
  }

  resetAddQuestionForm(): void {
    this.newQuestionText.set('');
    this.newAnswerOptions.set(['', '', '', '']);
    this.newCorrectAnswer.set('');
    this.newVoteReward.set(10);
    this.showAddQuestionForm.set(false);
  }

  statusLabel(status: TriviaQuestionDto['status']): string {
    return status;
  }

  trackBySeriesId(_: number, item: { seriesId: string }): string {
    return item.seriesId;
  }

  trackByQuestionId(_: number, item: { questionId: string }): string {
    return item.questionId;
  }
}
