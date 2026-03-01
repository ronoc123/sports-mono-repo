import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { EconomyAdminApi } from './economy-admin.api';
import { EconomyAdminStore } from './economy-admin.store';
import { UpdatePackConfigRequest, UpdateRarityTierRequest } from './economy-admin.model';

@Injectable({ providedIn: 'root' })
export class EconomyAdminFacade {
  private readonly api = inject(EconomyAdminApi);
  private readonly store = inject(EconomyAdminStore);

  // ── Selectors ─────────────────────────────────────────────────────────────
  readonly rarityTiers = this.store.rarityTiers;
  readonly packConfig = this.store.packConfig;
  readonly isLoadingTiers = this.store.isLoadingTiers;
  readonly isLoadingPack = this.store.isLoadingPack;
  readonly isSaving = this.store.isSaving;
  readonly totalPullWeight = this.store.totalPullWeight;
  readonly tiersError = this.store.tiersError;
  readonly packError = this.store.packError;
  readonly saveError = this.store.saveError;

  // ── Load ──────────────────────────────────────────────────────────────────
  async loadRarityTiers(orgId: string): Promise<void> {
    this.store.setTiersLoading();
    try {
      const res = await firstValueFrom(this.api.getRarityTiers(orgId));
      this.store.setRarityTiers(res.data ?? []);
    } catch (err: unknown) {
      this.store.setTiersError(extractMessage(err));
    }
  }

  async loadPackConfig(orgId: string): Promise<void> {
    this.store.setPackLoading();
    try {
      const res = await firstValueFrom(this.api.getPackConfig(orgId));
      if (res.data) this.store.setPackConfig(res.data);
    } catch (err: unknown) {
      this.store.setPackError(extractMessage(err));
    }
  }

  // ── Update ────────────────────────────────────────────────────────────────
  async updateRarityTier(configId: string, body: UpdateRarityTierRequest): Promise<void> {
    this.store.setSaving();
    try {
      const res = await firstValueFrom(this.api.updateRarityTier(configId, body));
      if (res.data) {
        this.store.applyTierUpdate(res.data);
      }
    } catch (err: unknown) {
      this.store.setSaveError(extractMessage(err));
    }
  }

  async updatePackConfig(orgId: string, body: UpdatePackConfigRequest): Promise<void> {
    this.store.setSaving();
    try {
      const res = await firstValueFrom(this.api.updatePackConfig(orgId, body));
      if (res.data) {
        this.store.setPackConfig(res.data);
        this.store.setSaveSuccess();
      }
    } catch (err: unknown) {
      this.store.setSaveError(extractMessage(err));
    }
  }
}

function extractMessage(err: unknown): string {
  if (err instanceof Error) return err.message;
  if (typeof err === 'string') return err;
  return 'An unexpected error occurred.';
}
