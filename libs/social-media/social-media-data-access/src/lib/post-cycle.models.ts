export interface MetadataSuggestions {
  suggestedTitle: string;
  suggestedDescription: string;
  suggestedHashtags: string[];
}

export interface GenerateVideoRequest {
  channelId: string;
  videoReference: string;
  userPrompt?: string;
}

export interface GeneratedMetadata {
  title: string;
  description: string;
  hashtags: string[];
}

export interface PlatformJob {
  platform: string;
  status: 'Pending' | 'Uploading' | 'Published' | 'Failed';
  videoUrl?: string;
  externalPostId?: string;
  errorMessage?: string;
  requiresReauth: boolean;
}

export interface PostCycleJob {
  id: string;
  channelId: string;
  status: 'Running' | 'Completed' | 'PartialFailure' | 'Failed' | 'TimedOut';
  title: string;
  platformJobs: PlatformJob[];
  createdAt: string | null;
  completedAt: string | null;
}

// Post History models (Epic 5)

export interface PlatformResultSummary {
  platform: string;
  status: string;
}

export interface PostRecordSummary {
  id: string;
  title: string;
  descriptionSnippet: string;
  videoReference: string;
  postedAt: string | null;
  platformResults: PlatformResultSummary[];
}

export interface PostHistoryPage {
  records: PostRecordSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PlatformResultDetail {
  platform: string;
  status: string;
  publishedUrl?: string;
  errorMessage?: string;
  publishedAt?: string;
}

export interface PostRecordDetail {
  id: string;
  channelId: string;
  title: string;
  description: string;
  hashtags: string[];
  videoReference: string;
  postedAt: string | null;
  platformResults: PlatformResultDetail[];
}

export type PostCycleStatus = 'idle' | 'loading' | 'success' | 'error';

export interface PostCycleState {
  suggestions: MetadataSuggestions | null;
  generatedMetadata: GeneratedMetadata | null;
  suggestionsStatus: PostCycleStatus;
  generateStatus: PostCycleStatus;
  submitStatus: PostCycleStatus;
  currentJob: PostCycleJob | null;
  historyPage: PostHistoryPage | null;
  historyStatus: PostCycleStatus;
  selectedRecord: PostRecordDetail | null;
  recordStatus: PostCycleStatus;
  error: string | null;
}

export const initialPostCycleState: PostCycleState = {
  suggestions: null,
  generatedMetadata: null,
  suggestionsStatus: 'idle',
  generateStatus: 'idle',
  submitStatus: 'idle',
  currentJob: null,
  historyPage: null,
  historyStatus: 'idle',
  selectedRecord: null,
  recordStatus: 'idle',
  error: null,
};
