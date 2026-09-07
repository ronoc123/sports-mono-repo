export interface LinkedAccount {
  platform: string;
  accountDisplayName: string;
  linkedAt: string;
  tokenStatus: 'active' | 'invalid';
}

export interface ChannelSummary {
  id: string;
  name: string;
  description: string;
  linkedAccountCount: number;
  lastPostTitle: string | null;
  lastPostDate: string | null;
  createdAt: string | null;
}

export interface ChannelDetail {
  id: string;
  name: string;
  description: string;
  styleToneContext: string;
  linkedAccounts: LinkedAccount[];
  createdAt: string | null;
  promptTemplate?: string;
  characterImageUrl?: string;
}

export interface CreateChannelRequest {
  name: string;
  description: string;
  styleToneContext: string;
}

export interface UpdateChannelRequest {
  name: string;
  description: string;
  styleToneContext: string;
}

export type ChannelStatus = 'idle' | 'loading' | 'success' | 'error';

export interface ChannelState {
  status: ChannelStatus;
  saveStatus: ChannelStatus;
  channels: ChannelSummary[];
  selectedChannel: ChannelDetail | null;
  error: string | null;
  saveError: string | null;
  promptTemplateSaveStatus: ChannelStatus;
  promptTemplateSaveError: string | null;
  imageUploadStatus: ChannelStatus;
  imageUploadError: string | null;
}

export const initialChannelState: ChannelState = {
  status: 'idle',
  saveStatus: 'idle',
  channels: [],
  selectedChannel: null,
  error: null,
  saveError: null,
  promptTemplateSaveStatus: 'idle',
  promptTemplateSaveError: null,
  imageUploadStatus: 'idle',
  imageUploadError: null,
};
