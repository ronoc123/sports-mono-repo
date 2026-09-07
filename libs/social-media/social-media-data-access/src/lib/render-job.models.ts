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

export type RenderJobStatus = 'idle' | 'loading' | 'success' | 'error';

export interface RenderJobState {
  uploadStatus: RenderJobStatus;
  createStatus: RenderJobStatus;
  currentJob: RenderJob | null;
  error: string | null;
}

export const initialRenderJobState: RenderJobState = {
  uploadStatus: 'idle',
  createStatus: 'idle',
  currentJob: null,
  error: null,
};
