export interface RenderJob {
  id: string;
  channelId: string;
  /** Pending | Processing | Completed | Failed | TimedOut (client-side) */
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed' | 'TimedOut';
  prompt: string;
  model: string;
  durationSeconds: number;
  resolution: string;
  aspectRatio: string;
  outputVideoKey?: string | null;
  retryCount: number;
  errorMessage?: string | null;
  createdAt?: string | null;
  startedAt?: string | null;
  completedAt?: string | null;
}

export interface UploadAssetResponse {
  objectKey: string;
}

export interface VideoUrlResponse {
  url: string;
}

export interface CreateRenderJobRequest {
  channelId: string;
  prompt: string;
  model: string;
  durationSeconds: number;
  resolution: string;
  aspectRatio: string;
  referenceImageKeys: string[];
  keyframeKeys: string[];
  modelOptions: Record<string, string>;
}

export interface CreateRenderJobResponse {
  jobId: string;
}

export interface StartPostFromRenderJobRequest {
  channelId: string;
  title: string;
  description: string;
  hashtags: string[];
  /** When set, only this platform is posted to. Omit to post to all linked accounts. */
  targetPlatform?: string;
}

export interface StartPostCycleResponse {
  jobId: string;
}

export type RenderJobStatus = 'idle' | 'loading' | 'success' | 'error';

export interface RenderJobState {
  uploadStatus: RenderJobStatus;
  createStatus: RenderJobStatus;
  currentJob: RenderJob | null;
  channelJobs: RenderJob[];
  channelJobsStatus: RenderJobStatus;
  videoUrl: string | null;
  postStatus: RenderJobStatus;
  postError: string | null;
  error: string | null;
}

export const initialRenderJobState: RenderJobState = {
  uploadStatus: 'idle',
  createStatus: 'idle',
  currentJob: null,
  channelJobs: [],
  channelJobsStatus: 'idle',
  videoUrl: null,
  postStatus: 'idle',
  postError: null,
  error: null,
};
