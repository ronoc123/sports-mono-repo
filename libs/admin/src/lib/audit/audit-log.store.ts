import { computed } from '@angular/core';
import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from '@ngrx/signals';
import { TransactionAuditDto } from './audit-log.model';

export type AuditStatus = 'idle' | 'loading' | 'success' | 'error';

export interface AuditLogState {
  transactions: TransactionAuditDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  filterUserId: string;
  filterReason: string;
  status: AuditStatus;
  error: string | null;
}

const initialState: AuditLogState = {
  transactions: [],
  totalCount: 0,
  page: 1,
  pageSize: 50,
  filterUserId: '',
  filterReason: '',
  status: 'idle',
  error: null,
};

export const AuditLogStore = signalStore(
  { providedIn: 'root' },
  withState<AuditLogState>(initialState),

  withComputed((state) => ({
    isLoading: computed(() => state.status() === 'loading'),
    totalPages: computed(() => Math.max(1, Math.ceil(state.totalCount() / state.pageSize()))),
    hasPrev: computed(() => state.page() > 1),
    hasNext: computed(() => state.page() < Math.ceil(state.totalCount() / state.pageSize())),
  })),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: 'loading', error: null });
    },
    setTransactions(transactions: TransactionAuditDto[], totalCount: number, page: number) {
      patchState(store, { transactions, totalCount, page, status: 'success' });
    },
    setError(error: string) {
      patchState(store, { error, status: 'error' });
    },
    setFilterUserId(filterUserId: string) {
      patchState(store, { filterUserId, page: 1 });
    },
    setFilterReason(filterReason: string) {
      patchState(store, { filterReason, page: 1 });
    },
    setPage(page: number) {
      patchState(store, { page });
    },
  }))
);
