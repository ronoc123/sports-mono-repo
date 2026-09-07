export interface VideoGenerationJob {
  id: string;
  channelId: string;
  status: 'Queued' | 'Generating' | 'Ready' | 'Failed' | 'TimedOut' | 'Consumed';
  renderedPrompt: string;
  videoTempPath?: string;
  higgsFieldModel?: string;
  errorMessage?: string;
  completedAt?: string;
}

export interface StartVideoGenerationResponse {
  jobId: string;
}

export interface StartPostCycleFromGenerationRequest {
  channelId: string;
  videoGenerationJobId: string;
  title: string;
  description: string;
  hashtags: string[];
}

export type VideoGenerationStatus = 'idle' | 'loading' | 'success' | 'error';

export interface VideoGenerationState {
  uploadStatus: VideoGenerationStatus;
  currentJob: VideoGenerationJob | null;
  startPostStatus: VideoGenerationStatus;
  error: string | null;
}

export const initialVideoGenerationState: VideoGenerationState = {
  uploadStatus: 'idle',
  currentJob: null,
  startPostStatus: 'idle',
  error: null,
};
