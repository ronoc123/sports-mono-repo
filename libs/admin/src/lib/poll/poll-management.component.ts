import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PollManagementFacade } from './poll-management.facade';
import { OrganizationFeatureService } from '@sports-ui/organization-data-access';

@Component({
  selector: 'lib-poll-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './poll-management.component.html',
  styleUrl: './poll-management.component.css',
})
export class PollManagementComponent implements OnInit {
  private readonly facade = inject(PollManagementFacade);
  private readonly orgService = inject(OrganizationFeatureService);

  // ── Facade signals ────────────────────────────────────────────────────────
  readonly polls = this.facade.polls;
  readonly isLoading = this.facade.isLoading;
  readonly isSaving = this.facade.isSaving;
  readonly error = this.facade.error;
  readonly saveError = this.facade.saveError;

  // ── Create poll form ──────────────────────────────────────────────────────
  readonly showCreateForm = signal(false);
  readonly newQuestionText = signal('');
  readonly newOptions = signal(['', '']);

  async ngOnInit(): Promise<void> {
    const orgId = this.orgService.selectedOrganization()?.id;
    if (orgId) await this.facade.loadPolls(orgId);
  }

  get orgId(): string | undefined {
    return this.orgService.selectedOrganization()?.id;
  }

  async submitCreatePoll(): Promise<void> {
    const orgId = this.orgId;
    const question = this.newQuestionText().trim();
    const opts = this.newOptions().map((o) => o.trim()).filter((o) => o.length > 0);
    if (!orgId || !question || opts.length < 2) return;
    await this.facade.createPoll(orgId, question, opts);
    this.resetForm();
  }

  async archivePoll(pollId: string): Promise<void> {
    await this.facade.archivePoll(pollId);
  }

  addOption(): void {
    if (this.newOptions().length >= 6) return;
    this.newOptions.set([...this.newOptions(), '']);
  }

  removeOption(index: number): void {
    if (this.newOptions().length <= 2) return;
    const opts = [...this.newOptions()];
    opts.splice(index, 1);
    this.newOptions.set(opts);
  }

  updateOption(index: number, value: string): void {
    const opts = [...this.newOptions()];
    opts[index] = value;
    this.newOptions.set(opts);
  }

  resetForm(): void {
    this.newQuestionText.set('');
    this.newOptions.set(['', '']);
    this.showCreateForm.set(false);
  }

  votePercent(voteCount: number, totalVotes: number): number {
    if (totalVotes === 0) return 0;
    return Math.round((voteCount / totalVotes) * 100);
  }

  trackByPollId(_: number, item: { pollId: string }): string {
    return item.pollId;
  }
}
