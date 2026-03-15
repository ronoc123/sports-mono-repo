import { Component, OnInit, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditLogFacade } from './audit-log.facade';
import { LeaguesFacade } from '@sports-ui/league-data-access';
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
  private readonly leaguesFacade = inject(LeaguesFacade);

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

  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    const pages: number[] = [1];
    const start = Math.max(2, current - 2);
    const end = Math.min(total - 1, current + 2);
    if (start > 2) pages.push(-1);
    for (let i = start; i <= end; i++) pages.push(i);
    if (end < total - 1) pages.push(-1);
    pages.push(total);
    return pages;
  });

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  private get leagueId(): string | undefined {
    return this.leaguesFacade.selectedLeagueId() ?? undefined;
  }

  async load(): Promise<void> {
    const leagueId = this.leagueId;
    if (leagueId) await this.facade.loadTransactions(leagueId);
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
    const leagueId = this.leagueId;
    if (!leagueId) return '';
    return this.facade.getExportUrl(leagueId);
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
