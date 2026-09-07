import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ChannelApiService } from './channel.api';
import {
  ChannelState,
  initialChannelState,
  CreateChannelRequest,
  UpdateChannelRequest,
} from './channel.models';

export const ChannelStore = signalStore(
  { providedIn: 'root' },
  withState<ChannelState>(initialChannelState),

  withComputed((state) => ({
    isLoading: computed(() => state.status() === 'loading'),
    isSaving: computed(() => state.saveStatus() === 'loading'),
    hasChannels: computed(() => state.channels().length > 0),
    isUploadingImage: computed(() => state.imageUploadStatus() === 'loading'),
  })),

  withMethods((store, api = inject(ChannelApiService)) => ({
    async loadChannels(): Promise<void> {
      patchState(store, { status: 'loading', error: null });
      try {
        const res = await firstValueFrom(api.getChannels());
        patchState(store, { channels: res.data ?? [], status: 'success' });
      } catch {
        patchState(store, { status: 'error', error: 'Failed to load channels.' });
      }
    },

    async loadChannel(id: string): Promise<void> {
      patchState(store, { status: 'loading', error: null, selectedChannel: null });
      try {
        const res = await firstValueFrom(api.getChannel(id));
        patchState(store, { selectedChannel: res.data, status: 'success' });
      } catch {
        patchState(store, { status: 'error', error: 'Channel not found.' });
      }
    },

    async createChannel(request: CreateChannelRequest): Promise<string | null> {
      patchState(store, { saveStatus: 'loading', saveError: null });
      try {
        const res = await firstValueFrom(api.createChannel(request));
        patchState(store, { saveStatus: 'success', selectedChannel: res.data });
        return res.data.id;
      } catch (err: any) {
        patchState(store, {
          saveStatus: 'error',
          saveError: err?.error?.message ?? 'Failed to create channel.',
        });
        return null;
      }
    },

    async updateChannel(id: string, request: UpdateChannelRequest): Promise<boolean> {
      patchState(store, { saveStatus: 'loading', saveError: null });
      try {
        const res = await firstValueFrom(api.updateChannel(id, request));
        patchState(store, { saveStatus: 'success', selectedChannel: res.data });
        return true;
      } catch (err: any) {
        patchState(store, {
          saveStatus: 'error',
          saveError: err?.error?.message ?? 'Failed to update channel.',
        });
        return false;
      }
    },

    async deleteChannel(id: string): Promise<boolean> {
      patchState(store, { saveStatus: 'loading', saveError: null });
      try {
        await firstValueFrom(api.deleteChannel(id));
        patchState(store, {
          saveStatus: 'success',
          channels: store.channels().filter((c) => c.id !== id),
          selectedChannel: null,
        });
        return true;
      } catch (err: any) {
        patchState(store, {
          saveStatus: 'error',
          saveError: err?.error?.message ?? 'Failed to delete channel.',
        });
        return false;
      }
    },

    async unlinkAccount(channelId: string, platform: string): Promise<boolean> {
      patchState(store, { saveStatus: 'loading', saveError: null });
      try {
        const res = await firstValueFrom(api.unlinkAccount(channelId, platform));
        patchState(store, { saveStatus: 'success', selectedChannel: res.data });
        return true;
      } catch (err: any) {
        patchState(store, {
          saveStatus: 'error',
          saveError: err?.error?.message ?? 'Failed to unlink account.',
        });
        return false;
      }
    },

    async getOAuthUrl(channelId: string): Promise<string | null> {
      try {
        const res = await firstValueFrom(api.getOAuthUrl(channelId));
        return res.data.url;
      } catch {
        return null;
      }
    },

    async uploadCharacterImage(channelId: string, formData: FormData): Promise<boolean> {
      patchState(store, { imageUploadStatus: 'loading', imageUploadError: null });
      try {
        const res = await firstValueFrom(api.uploadCharacterImage(channelId, formData));
        patchState(store, { imageUploadStatus: 'success', selectedChannel: res.data });
        return true;
      } catch (err: any) {
        patchState(store, {
          imageUploadStatus: 'error',
          imageUploadError: err?.error?.message ?? 'Failed to upload image.',
        });
        return false;
      }
    },

    async updatePromptTemplate(channelId: string, template: string): Promise<boolean> {
      patchState(store, { promptTemplateSaveStatus: 'loading', promptTemplateSaveError: null });
      try {
        const res = await firstValueFrom(api.updatePromptTemplate(channelId, template));
        patchState(store, {
          promptTemplateSaveStatus: 'success',
          selectedChannel: res.data,
        });
        return true;
      } catch (err: any) {
        patchState(store, {
          promptTemplateSaveStatus: 'error',
          promptTemplateSaveError: err?.error?.message ?? 'Failed to save prompt template.',
        });
        return false;
      }
    },

    resetSaveState(): void {
      patchState(store, { saveStatus: 'idle', saveError: null });
    },
  }))
);
