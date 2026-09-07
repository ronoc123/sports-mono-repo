import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { RenderJobApiService } from './render-job.api';
import {
  RenderJobState,
  initialRenderJobState,
  CreateRenderJobRequest,
} from './render-job.models';

export const RenderJobStore = signalStore(
  { providedIn: 'root' },
  withState<RenderJobState>(initialRenderJobState),

  withComputed((state) => ({
    isUploading: computed(() => state.uploadStatus() === 'loading'),
    isCreating: computed(() => state.createStatus() === 'loading'),
    isJobTerminal: computed(() => {
      const status = state.currentJob()?.status;
      return status !== undefined &&
        ['Completed', 'Failed', 'TimedOut'].includes(status);
    }),
  })),

  withMethods((store, api = inject(RenderJobApiService)) => {
    let pollTimer: ReturnType<typeof setInterval> | null = null;
    let pollCount = 0;
    const maxPollCount = 360; // 360 × 5s = 30 minutes

    function stopPolling(): void {
      if (pollTimer !== null) {
        clearInterval(pollTimer);
        pollTimer = null;
        pollCount = 0;
      }
    }

    return {
      async uploadAsset(formData: FormData): Promise<string | null> {
        patchState(store, { uploadStatus: 'loading', error: null });
        try {
          const res = await firstValueFrom(api.uploadAsset(formData));
          patchState(store, { uploadStatus: 'success' });
          return res.data.objectKey;
        } catch (err: any) {
          patchState(store, {
            uploadStatus: 'error',
            error: err?.error?.message ?? 'Failed to upload asset.',
          });
          return null;
        }
      },

      async createJob(req: CreateRenderJobRequest): Promise<string | null> {
        patchState(store, { createStatus: 'loading', error: null });
        try {
          const res = await firstValueFrom(api.createJob(req));
          patchState(store, { createStatus: 'success' });
          return res.data.jobId;
        } catch (err: any) {
          patchState(store, {
            createStatus: 'error',
            error: err?.error?.message ?? 'Failed to create render job.',
          });
          return null;
        }
      },

      startPolling(jobId: string): void {
        stopPolling();
        pollCount = 0;

        pollTimer = setInterval(async () => {
          pollCount++;

          if (pollCount >= maxPollCount) {
            stopPolling();
            const current = store.currentJob();
            patchState(store, {
              currentJob: current
                ? { ...current, status: 'TimedOut' as const }
                : null,
            });
            return;
          }

          try {
            const res = await firstValueFrom(api.getJob(jobId));
            patchState(store, { currentJob: res.data });

            const terminal = ['Completed', 'Failed', 'TimedOut'];
            if (terminal.includes(res.data.status)) {
              stopPolling();
            }
          } catch {
            // ignore transient errors during polling
          }
        }, 5000);
      },

      stopPolling,

      resetJob(): void {
        stopPolling();
        patchState(store, initialRenderJobState);
      },
    };
  })
);
