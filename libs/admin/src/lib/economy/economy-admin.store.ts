import { computed } from '@angular/core';
import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from '@ngrx/signals';
import { PackConfigDto, RarityTierConfigDto } from './economy-admin.model';

export type EconomyAdminStatus = 'idle' | 'loading' | 'success' | 'error';

export interface EconomyAdminState {
  rarityTiers: RarityTierConfigDto[];
  packConfig: PackConfigDto | null;
  tiersStatus: EconomyAdminStatus;
  packStatus: EconomyAdminStatus;
  saveStatus: EconomyAdminStatus;
  tiersError: string | null;
  packError: string | null;
  saveError: string | null;
}

const initialState: EconomyAdminState = {
  rarityTiers: [],
  packConfig: null,
  tiersStatus: 'idle',
  packStatus: 'idle',
  saveStatus: 'idle',
  tiersError: null,
  packError: null,
  saveError: null,
};

export const EconomyAdminStore = signalStore(
  { providedIn: 'root' },
  withState<EconomyAdminState>(initialState),

  withComputed((state) => ({
    isLoadingTiers: computed(() => state.tiersStatus() === 'loading'),
    isLoadingPack: computed(() => state.packStatus() === 'loading'),
    isSaving: computed(() => state.saveStatus() === 'loading'),
    totalPullWeight: computed(() =>
      state.rarityTiers().reduce((sum, t) => sum + t.pullWeightBps, 0)
    ),
  })),

  withMethods((store) => ({
    // ── Rarity tiers ─────────────────────────────────────────────────────
    setTiersLoading() {
      patchState(store, { tiersStatus: 'loading', tiersError: null });
    },
    setRarityTiers(rarityTiers: RarityTierConfigDto[]) {
      patchState(store, { rarityTiers, tiersStatus: 'success' });
    },
    setTiersError(tiersError: string) {
      patchState(store, { tiersError, tiersStatus: 'error' });
    },
    applyTierUpdate(updated: RarityTierConfigDto) {
      patchState(store, {
        rarityTiers: store.rarityTiers().map((t) => (t.id === updated.id ? updated : t)),
        saveStatus: 'success',
        saveError: null,
      });
    },

    // ── Pack config ───────────────────────────────────────────────────────
    setPackLoading() {
      patchState(store, { packStatus: 'loading', packError: null });
    },
    setPackConfig(packConfig: PackConfigDto) {
      patchState(store, { packConfig, packStatus: 'success' });
    },
    setPackError(packError: string) {
      patchState(store, { packError, packStatus: 'error' });
    },

    // ── Save state ────────────────────────────────────────────────────────
    setSaving() {
      patchState(store, { saveStatus: 'loading', saveError: null });
    },
    setSaveSuccess() {
      patchState(store, { saveStatus: 'success', saveError: null });
    },
    setSaveError(saveError: string) {
      patchState(store, { saveError, saveStatus: 'error' });
    },
  }))
);
