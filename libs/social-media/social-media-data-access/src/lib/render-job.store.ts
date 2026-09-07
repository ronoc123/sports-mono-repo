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

  withMethods((store, api = inject(RenderJobApiService)) => ({
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

    /** Single fetch — updates currentJob in state. */
    async fetchJob(jobId: string): Promise<void> {
      try {
        const res = await firstValueFrom(api.getJob(jobId));
        patchState(store, { currentJob: res.data });
      } catch {
        // ignore transient errors
      }
    },

    async loadChannelJobs(channelId: string): Promise<void> {
      patchState(store, { channelJobsStatus: 'loading' });
      try {
        const res = await firstValueFrom(api.listByChannel(channelId));
        patchState(store, { channelJobs: res.data, channelJobsStatus: 'success' });
      } catch {
        patchState(store, { channelJobsStatus: 'error' });
      }
    },

    async deleteJob(jobId: string): Promise<boolean> {
      try {
        await firstValueFrom(api.deleteJob(jobId));
        patchState(store, {
          channelJobs: store.channelJobs().filter(j => j.id !== jobId),
        });
        return true;
      } catch {
        return false;
      }
    },

    async getVideoUrl(jobId: string): Promise<string | null> {
      try {
        const res = await firstValueFrom(api.getVideoUrl(jobId));
        patchState(store, { videoUrl: res.data.url });
        return res.data.url;
      } catch {
        return null;
      }
    },

    /** Resets per-job state (upload/create/currentJob/videoUrl) without clearing channelJobs. */
    resetJob(): void {
      patchState(store, {
        uploadStatus: 'idle',
        createStatus: 'idle',
        currentJob: null,
        videoUrl: null,
        error: null,
      });
    },
  }))
);
