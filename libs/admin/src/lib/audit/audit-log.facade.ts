import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AuditLogApi } from './audit-log.api';
import { AuditLogStore } from './audit-log.store';

@Injectable({ providedIn: 'root' })
export class AuditLogFacade {
  private readonly api = inject(AuditLogApi);
  private readonly store = inject(AuditLogStore);

  // ── Selectors ─────────────────────────────────────────────────────────────
  readonly transactions = this.store.transactions;
  readonly totalCount = this.store.totalCount;
  readonly page = this.store.page;
  readonly pageSize = this.store.pageSize;
  readonly isLoading = this.store.isLoading;
  readonly totalPages = this.store.totalPages;
  readonly hasPrev = this.store.hasPrev;
  readonly hasNext = this.store.hasNext;
  readonly filterUserId = this.store.filterUserId;
  readonly filterReason = this.store.filterReason;
  readonly error = this.store.error;

  // ── Actions ───────────────────────────────────────────────────────────────
  async loadTransactions(orgId: string): Promise<void> {
    this.store.setLoading();
    try {
      const userId = this.store.filterUserId() || undefined;
      const reason = this.store.filterReason() || undefined;
      const res = await firstValueFrom(
        this.api.getTransactions(orgId, userId, reason, this.store.page(), this.store.pageSize())
      );
      if (res.data) {
        this.store.setTransactions(res.data.items, res.data.totalCount, res.data.page);
      }
    } catch (err: unknown) {
      this.store.setError(err instanceof Error ? err.message : 'Failed to load transactions.');
    }
  }

  setFilterUserId(userId: string): void {
    this.store.setFilterUserId(userId);
  }

  setFilterReason(reason: string): void {
    this.store.setFilterReason(reason);
  }

  setPage(page: number): void {
    this.store.setPage(page);
  }

  getExportUrl(orgId: string): string {
    const userId = this.store.filterUserId() || undefined;
    const reason = this.store.filterReason() || undefined;
    return this.api.getExportUrl(orgId, userId, reason);
  }
}
