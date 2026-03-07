import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PollManagementApi } from './poll-management.api';
import { PollManagementStore } from './poll-management.store';
import { CreatePollRequest, PollDto } from './poll-management.model';

@Injectable({ providedIn: 'root' })
export class PollManagementFacade {
  private readonly api = inject(PollManagementApi);
  private readonly store = inject(PollManagementStore);

  // ── Selectors ─────────────────────────────────────────────────────────────
  readonly polls = this.store.polls;
  readonly status = this.store.status;
  readonly isLoading = this.store.isLoading;
  readonly isSaving = this.store.isSaving;
  readonly error = this.store.error;
  readonly saveError = this.store.saveError;

  // ── Load ──────────────────────────────────────────────────────────────────
  async loadPolls(organizationId: string): Promise<void> {
    this.store.setLoading();
    try {
      const res = await firstValueFrom(this.api.getPolls(organizationId));
      this.store.setPolls(res.data ?? []);
    } catch (err: unknown) {
      this.store.setError(extractMessage(err));
    }
  }

  // ── Create ────────────────────────────────────────────────────────────────
  async createPoll(organizationId: string, questionText: string, options: string[]): Promise<void> {
    this.store.setSaving();
    const body: CreatePollRequest = { organizationId, questionText, options };
    try {
      const res = await firstValueFrom(this.api.createPoll(body));
      if (res.data) {
        const newPoll: PollDto = {
          pollId: res.data.pollId,
          questionText,
          status: 'Active',
          totalVotes: 0,
          createdAt: new Date().toISOString(),
          options: options.map((text, i) => ({
            pollOptionId: res.data!.optionIds[i] ?? '',
            optionText: text,
            voteCount: 0,
            sortOrder: i,
          })),
        };
        this.store.prependPoll(newPoll);
      }
    } catch (err: unknown) {
      this.store.setSaveError(extractMessage(err));
    }
  }

  // ── Archive ───────────────────────────────────────────────────────────────
  async archivePoll(pollId: string): Promise<void> {
    this.store.setSaving();
    try {
      await firstValueFrom(this.api.archivePoll(pollId));
      this.store.applyArchive(pollId);
    } catch (err: unknown) {
      this.store.setSaveError(extractMessage(err));
    }
  }
}

function extractMessage(err: unknown): string {
  if (err instanceof Error) return err.message;
  if (typeof err === 'string') return err;
  return 'An unexpected error occurred.';
}
