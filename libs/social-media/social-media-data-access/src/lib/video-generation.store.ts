import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { VideoGenerationApiService } from './video-generation.api';
import {
  VideoGenerationState,
  initialVideoGenerationState,
  StartPostCycleFromGenerationRequest,
} from './video-generation.models';

export const VideoGenerationStore = signalStore(
  { providedIn: 'root' },
  withState<VideoGenerationState>(initialVideoGenerationState),

  withComputed((state) => ({
    isUploading: computed(() => state.uploadStatus() === 'loading'),
    isStartingPost: computed(() => state.startPostStatus() === 'loading'),
    isJobReady: computed(() => state.currentJob()?.status === 'Ready'),
    isJobTerminal: computed(() => {
      const status = state.currentJob()?.status;
      return status !== undefined &&
        ['Ready', 'Failed', 'TimedOut', 'Consumed'].includes(status);
    }),
  })),

  withMethods((store, api = inject(VideoGenerationApiService)) => {
    let pollTimer: ReturnType<typeof setInterval> | null = null;
    let pollCount = 0;
    const maxPollCount = 200; // 200 × 3s = 10 minutes

    function stopPolling(): void {
      if (pollTimer !== null) {
        clearInterval(pollTimer);
        pollTimer = null;
        pollCount = 0;
      }
    }

    return {
      async uploadGeneration(formData: FormData): Promise<string | null> {
        patchState(store, { uploadStatus: 'loading', error: null });
        try {
          const res = await firstValueFrom(api.startGeneration(formData));
          patchState(store, { uploadStatus: 'success' });
          return res.data.jobId;
        } catch (err: any) {
          patchState(store, {
            uploadStatus: 'error',
            error: err?.error?.message ?? 'Failed to start video generation.',
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

            const terminal = ['Ready', 'Failed', 'TimedOut', 'Consumed'];
            if (terminal.includes(res.data.status)) {
              stopPolling();
            }
          } catch {
            // ignore transient errors during polling
          }
        }, 3000);
      },

      stopPolling,

      async startPostFromGeneration(req: StartPostCycleFromGenerationRequest): Promise<string | null> {
        patchState(store, { startPostStatus: 'loading', error: null });
        try {
          const res = await firstValueFrom(api.startFromGeneration(req));
          patchState(store, { startPostStatus: 'success' });
          return res.data.jobId;
        } catch (err: any) {
          patchState(store, {
            startPostStatus: 'error',
            error: err?.error?.message ?? 'Failed to start post cycle.',
          });
          return null;
        }
      },

      resetGeneration(): void {
        stopPolling();
        patchState(store, initialVideoGenerationState);
      },
    };
  })
);
