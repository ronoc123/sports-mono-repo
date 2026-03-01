import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@sports-ui/http-client';
import { ServiceResponse } from '@sports-ui/api-types';
import { environment } from '@sports-ui/api-types';
import { TransactionAuditResult } from './audit-log.model';

@Injectable({ providedIn: 'root' })
export class AuditLogApi {
  private readonly http = inject(ApiService);

  getTransactions(
    orgId: string,
    userId?: string,
    reason?: string,
    page = 1,
    pageSize = 50
  ): Observable<ServiceResponse<TransactionAuditResult>> {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (userId) params.set('userId', userId);
    if (reason) params.set('reason', reason);
    return this.http.get<ServiceResponse<TransactionAuditResult>>(
      `${environment.sportsApi}admin/economy/transactions/${orgId}?${params}`
    );
  }

  getExportUrl(orgId: string, userId?: string, reason?: string): string {
    const params = new URLSearchParams();
    if (userId) params.set('userId', userId);
    if (reason) params.set('reason', reason);
    const qs = params.toString();
    return `${environment.sportsApi}admin/economy/transactions/${orgId}/export${qs ? '?' + qs : ''}`;
  }
}
