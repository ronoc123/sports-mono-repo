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

export interface CreateRenderJobClipRequest {
  prompt: string;
  /** R2 key for the clip's start frame (image_start). Omit to use the channel image. */
  startImageKey?: string | null;
  /** R2 key for the clip's end frame (image_end). Omit for no end-frame guidance. */
  endImageKey?: string | null;
}

export interface CreateRenderJobRequest {
  channelId: string;
  model: string;
  durationSeconds: number;
  resolution: string;
  aspectRatio: string;
  clips: CreateRenderJobClipRequest[];
  modelOptions?: Record<string, string>;
  /**
   * When false the backend will NOT auto-upload the channel's character image as the
   * default start frame for clips that have no explicit startImageKey. Defaults to true.
   */
  useChannelImage?: boolean;
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

export interface IdeateVideoRequest {
  channelId: string;
  userIdea: string;
}

export interface IdeateVideoResponse {
  prompt: string;
  scenes: string[];
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
  ideateStatus: RenderJobStatus;
  ideateResult: IdeateVideoResponse | null;
  ideateError: string | null;
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
  ideateStatus: 'idle',
  ideateResult: null,
  ideateError: null,
  error: null,
};
