import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PostCycleApiService } from './post-cycle.api';
import { PostCycleState, initialPostCycleState, GenerateVideoRequest } from './post-cycle.models';

export const PostCycleStore = signalStore(
  { providedIn: 'root' },
  withState<PostCycleState>(initialPostCycleState),

  withComputed((state) => ({
    isLoadingSuggestions: computed(() => state.suggestionsStatus() === 'loading'),
    isGenerating: computed(() => state.generateStatus() === 'loading'),
    isSubmitting: computed(() => state.submitStatus() === 'loading'),
    isLoadingHistory: computed(() => state.historyStatus() === 'loading'),
    isLoadingRecord: computed(() => state.recordStatus() === 'loading'),
    isJobTerminal: computed(() => {
      const job = state.currentJob();
      return job !== null &&
        ['Completed', 'PartialFailure', 'Failed', 'TimedOut'].includes(job.status);
    }),
  })),

  withMethods((store, api = inject(PostCycleApiService)) => {
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
      async loadSuggestions(channelId: string): Promise<void> {
        patchState(store, { suggestionsStatus: 'loading', error: null });
        try {
          const res = await firstValueFrom(api.getSuggestions(channelId));
          patchState(store, { suggestions: res.data, suggestionsStatus: 'success' });
        } catch {
          patchState(store, { suggestionsStatus: 'error', error: 'Failed to load suggestions.' });
        }
      },

      async generateMetadata(request: GenerateVideoRequest): Promise<void> {
        patchState(store, { generateStatus: 'loading', error: null });
        try {
          const res = await firstValueFrom(api.generateMetadata(request));
          patchState(store, { generatedMetadata: res.data, generateStatus: 'success' });
        } catch {
          patchState(store, { generateStatus: 'error', error: 'Failed to generate metadata.' });
        }
      },

      async startPostCycle(formData: FormData): Promise<string | null> {
        patchState(store, { submitStatus: 'loading', error: null });
        try {
          const res = await firstValueFrom(api.startPostCycle(formData));
          patchState(store, { submitStatus: 'success' });
          return res.data.jobId;
        } catch {
          patchState(store, { submitStatus: 'error', error: 'Failed to start post cycle.' });
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
            patchState(store, {
              currentJob: store.currentJob()
                ? { ...store.currentJob()!, status: 'TimedOut' }
                : null,
            });
            return;
          }

          try {
            const res = await firstValueFrom(api.getPostCycle(jobId));
            patchState(store, { currentJob: res.data });

            const terminal = ['Completed', 'PartialFailure', 'Failed', 'TimedOut'];
            if (terminal.includes(res.data.status)) {
              stopPolling();
            }
          } catch {
            // ignore transient errors during polling
          }
        }, 3000);
      },

      stopPolling,

      async retryPlatform(jobId: string, platform: string): Promise<void> {
        try {
          const res = await firstValueFrom(api.retryPlatform(jobId, platform));
          patchState(store, { currentJob: res.data });
          this.startPolling(jobId);
        } catch {
          patchState(store, { error: `Failed to retry ${platform}.` });
        }
      },

      async loadPostHistory(channelId: string, page: number, pageSize = 20): Promise<void> {
        patchState(store, { historyStatus: 'loading', error: null });
        try {
          const res = await firstValueFrom(api.getPostHistory(channelId, page, pageSize));
          patchState(store, { historyPage: res.data, historyStatus: 'success' });
        } catch {
          patchState(store, { historyStatus: 'error', error: 'Failed to load post history.' });
        }
      },

      async loadPostRecord(id: string): Promise<void> {
        patchState(store, { recordStatus: 'loading', selectedRecord: null, error: null });
        try {
          const res = await firstValueFrom(api.getPostRecord(id));
          patchState(store, { selectedRecord: res.data, recordStatus: 'success' });
        } catch {
          patchState(store, { recordStatus: 'error', error: 'Failed to load post record.' });
        }
      },

      resetPostCycle(): void {
        stopPolling();
        patchState(store, initialPostCycleState);
      },
    };
  })
);
