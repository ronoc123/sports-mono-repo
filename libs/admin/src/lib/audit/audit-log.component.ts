import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditLogFacade } from './audit-log.facade';
import { OrganizationFeatureService } from '@sports-ui/organization-data-access';
import { TRANSACTION_REASONS } from './audit-log.model';

@Component({
  selector: 'lib-audit-log',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-log.component.html',
  styleUrl: './audit-log.component.css',
})
export class AuditLogComponent implements OnInit {
  private readonly facade = inject(AuditLogFacade);
  private readonly orgService = inject(OrganizationFeatureService);

  // ── Facade signals ────────────────────────────────────────────────────────
  readonly transactions = this.facade.transactions;
  readonly totalCount = this.facade.totalCount;
  readonly page = this.facade.page;
  readonly pageSize = this.facade.pageSize;
  readonly isLoading = this.facade.isLoading;
  readonly totalPages = this.facade.totalPages;
  readonly hasPrev = this.facade.hasPrev;
  readonly hasNext = this.facade.hasNext;
  readonly filterUserId = this.facade.filterUserId;
  readonly filterReason = this.facade.filterReason;
  readonly error = this.facade.error;

  readonly reasonOptions = TRANSACTION_REASONS;

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  private get orgId(): string | undefined {
    return this.orgService.selectedOrganization()?.id;
  }

  async load(): Promise<void> {
    const orgId = this.orgId;
    if (orgId) await this.facade.loadTransactions(orgId);
  }

  async applyFilters(): Promise<void> {
    await this.load();
  }

  async goToPage(page: number): Promise<void> {
    this.facade.setPage(page);
    await this.load();
  }

  onUserIdChange(value: string): void {
    this.facade.setFilterUserId(value);
  }

  onReasonChange(value: string): void {
    this.facade.setFilterReason(value);
  }

  getExportUrl(): string {
    const orgId = this.orgId;
    if (!orgId) return '';
    return this.facade.getExportUrl(orgId);
  }

  amountClass(amount: number): string {
    return amount >= 0 ? 'amount-positive' : 'amount-negative';
  }

  formatAmount(amount: number): string {
    return amount >= 0 ? `+${amount.toLocaleString()}` : amount.toLocaleString();
  }

  trackById(_: number, item: { id: number }): number {
    return item.id;
  }
}
