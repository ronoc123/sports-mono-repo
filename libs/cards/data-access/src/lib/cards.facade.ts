import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CardStore } from './cards.store';
import { CardsApi } from './cards.api';
import {
  CreateCardPlayerRequest,
  UpdateCardPlayerRequest,
  PackPurchaseRequest,
  PackPurchaseResult,
  UserCard,
} from './cards.model';

@Injectable({ providedIn: 'root' })
export class CardsFacade {
  private readonly store = inject(CardStore);
  private readonly api = inject(CardsApi);

  // ── Exposed signals ───────────────────────────────
  readonly cardPlayers = this.store.cardPlayers;
  readonly collection = this.store.collection;
  readonly isLoading = this.store.isLoading;
  readonly isSaving = this.store.isSaving;
  readonly isPurchasing = this.store.isPurchasing;
  readonly error = this.store.error;
  readonly saveError = this.store.saveError;
  readonly saveStatus = this.store.saveStatus;
  readonly packStatus = this.store.packStatus;
  readonly packError = this.store.packError;
  readonly lastPackResult = this.store.lastPackResult;

  resetSaveState() {
    this.store.resetSaveState();
  }

  async loadCardPlayers(leagueId: string): Promise<void> {
    this.store.setLoading();
    try {
      const res = await firstValueFrom(this.api.getCardPlayers(leagueId));
      this.store.setCardPlayers(res.data ?? []);
    } catch {
      this.store.setError('Failed to load card players');
    }
  }

  async createCardPlayer(request: CreateCardPlayerRequest): Promise<boolean> {
    this.store.setSaving();
    try {
      const res = await firstValueFrom(this.api.createCardPlayer(request));
      if (res.data) this.store.addCardPlayer(res.data);
      return true;
    } catch (err: any) {
      this.store.setSaveError(
        err?.error?.message ?? err?.message ?? 'Failed to create card player'
      );
      return false;
    }
  }

  async updateCardPlayer(id: string, request: UpdateCardPlayerRequest): Promise<boolean> {
    this.store.setSaving();
    try {
      const res = await firstValueFrom(this.api.updateCardPlayer(id, request));
      if (res.data) this.store.replaceCardPlayer(res.data);
      return true;
    } catch (err: any) {
      this.store.setSaveError(
        err?.error?.message ?? err?.message ?? 'Failed to update card player'
      );
      return false;
    }
  }

  async purchasePack(request: PackPurchaseRequest): Promise<PackPurchaseResult | null> {
    this.store.setPackPurchasing();
    try {
      const res = await firstValueFrom(this.api.purchasePack(request));
      if (res.data) {
        this.store.setPackResult(res.data);
        return res.data;
      }
      this.store.setPackError(res.message ?? 'Purchase failed.');
      return null;
    } catch (err: any) {
      this.store.setPackError(
        err?.error?.message ?? err?.message ?? 'Failed to purchase pack.'
      );
      return null;
    }
  }

  resetPackState() {
    this.store.resetPackState();
  }

  async loadCollection(userId: string, orgId: string): Promise<void> {
    this.store.setLoading();
    try {
      const res = await firstValueFrom(this.api.getCollection(userId, orgId));
      this.store.setCollection(res.data ?? []);
    } catch {
      this.store.setError('Failed to load collection');
    }
  }
}
