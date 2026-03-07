import { computed } from '@angular/core';
import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from '@ngrx/signals';
import { PollDto } from './poll-management.model';

export type PollMgmtStatus = 'idle' | 'loading' | 'success' | 'error';

export interface PollManagementState {
  polls: PollDto[];
  status: PollMgmtStatus;
  saveStatus: PollMgmtStatus;
  error: string | null;
  saveError: string | null;
}

const initialState: PollManagementState = {
  polls: [],
  status: 'idle',
  saveStatus: 'idle',
  error: null,
  saveError: null,
};

export const PollManagementStore = signalStore(
  { providedIn: 'root' },
  withState<PollManagementState>(initialState),

  withComputed((state) => ({
    isLoading: computed(() => state.status() === 'loading'),
    isSaving: computed(() => state.saveStatus() === 'loading'),
  })),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: 'loading', error: null });
    },
    setPolls(polls: PollDto[]) {
      patchState(store, { polls, status: 'success' });
    },
    setError(error: string) {
      patchState(store, { error, status: 'error' });
    },

    setSaving() {
      patchState(store, { saveStatus: 'loading', saveError: null });
    },
    setSaveSuccess() {
      patchState(store, { saveStatus: 'success', saveError: null });
    },
    setSaveError(saveError: string) {
      patchState(store, { saveError, saveStatus: 'error' });
    },

    prependPoll(poll: PollDto) {
      patchState(store, { polls: [poll, ...store.polls()], saveStatus: 'success' });
    },

    applyArchive(pollId: string) {
      patchState(store, {
        polls: store.polls().map((p) =>
          p.pollId === pollId ? { ...p, status: 'Archived' as const } : p
        ),
        saveStatus: 'success',
      });
    },
  }))
);
