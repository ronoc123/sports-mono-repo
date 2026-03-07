import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { TriviaManagementApi } from './trivia-management.api';
import { TriviaManagementStore } from './trivia-management.store';
import { AddTriviaQuestionRequest, CreateTriviaSeriesRequest } from './trivia-management.model';

@Injectable({ providedIn: 'root' })
export class TriviaManagementFacade {
  private readonly api = inject(TriviaManagementApi);
  private readonly store = inject(TriviaManagementStore);

  // ── Selectors ─────────────────────────────────────────────────────────────
  readonly series = this.store.series;
  readonly status = this.store.status;
  readonly isLoading = this.store.isLoading;
  readonly isSaving = this.store.isSaving;
  readonly error = this.store.error;
  readonly saveError = this.store.saveError;
  readonly selectedSeries = this.store.selectedSeries;
  readonly selectedSeriesId = this.store.selectedSeriesId;

  // ── Load ──────────────────────────────────────────────────────────────────
  async loadSeries(organizationId: string): Promise<void> {
    this.store.setLoading();
    try {
      const res = await firstValueFrom(this.api.getSeries(organizationId));
      this.store.setSeries(res.data ?? []);
    } catch (err: unknown) {
      this.store.setError(extractMessage(err));
    }
  }

  // ── Actions ───────────────────────────────────────────────────────────────
  async createSeries(organizationId: string, seriesName: string): Promise<void> {
    this.store.setSaving();
    try {
      const body: CreateTriviaSeriesRequest = { organizationId, seriesName };
      const res = await firstValueFrom(this.api.createSeries(body));
      if (res.data) {
        this.store.prependSeries({
          seriesId: res.data.seriesId,
          seriesName: res.data.seriesName,
          createdAt: new Date().toISOString(),
          questions: [],
        });
        this.store.selectSeries(res.data.seriesId);
      }
    } catch (err: unknown) {
      this.store.setSaveError(extractMessage(err));
    }
  }

  async addQuestion(organizationId: string, req: AddTriviaQuestionRequest): Promise<void> {
    this.store.setSaving();
    try {
      await firstValueFrom(this.api.addQuestion(req));
      // Reload to get updated participation counts and question state
      await this.loadSeries(organizationId);
      this.store.setSaveSuccess();
    } catch (err: unknown) {
      this.store.setSaveError(extractMessage(err));
    }
  }

  async publishQuestion(organizationId: string, questionId: string): Promise<void> {
    this.store.setSaving();
    try {
      await firstValueFrom(this.api.publishQuestion(questionId));
      await this.loadSeries(organizationId);
      this.store.setSaveSuccess();
    } catch (err: unknown) {
      this.store.setSaveError(extractMessage(err));
    }
  }

  async archiveQuestion(organizationId: string, questionId: string): Promise<void> {
    this.store.setSaving();
    try {
      await firstValueFrom(this.api.archiveQuestion(questionId));
      await this.loadSeries(organizationId);
      this.store.setSaveSuccess();
    } catch (err: unknown) {
      this.store.setSaveError(extractMessage(err));
    }
  }

  selectSeries(seriesId: string | null): void {
    this.store.selectSeries(seriesId);
  }
}

function extractMessage(err: unknown): string {
  if (err instanceof Error) return err.message;
  if (typeof err === 'string') return err;
  return 'An unexpected error occurred.';
}
