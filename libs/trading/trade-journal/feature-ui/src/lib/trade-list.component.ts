import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TradeJournalStore } from '@trading/trade-journal-data-access';
import { BacktestStore } from '@trading/backtest-data-access';
import { Trade } from '@trading/trade-journal-data-access';

@Component({
  selector: 'lib-trade-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host { display: block; }

    .page { max-width: 900px; margin: 0 auto; padding: 2rem; }

    .page-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      margin-bottom: 2rem;
      flex-wrap: wrap;
      gap: 1rem;
    }

    .page-header h1 {
      font-size: 1.75rem;
      font-weight: 700;
      color: #f0f0f0;
      margin: 0 0 0.35rem;
    }

    .page-header p { color: #8b8fa8; margin: 0; font-size: 0.9rem; }

    .btn-primary {
      padding: 0.75rem 1.5rem;
      background: linear-gradient(135deg, #00d4aa, #00b894);
      color: #0f1117;
      font-weight: 700;
      font-size: 0.9rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      white-space: nowrap;
    }

    .btn-primary:disabled { opacity: 0.4; cursor: not-allowed; }

    .gate-warning {
      background: rgba(255, 77, 109, 0.08);
      border: 1px solid rgba(255, 77, 109, 0.25);
      border-radius: 10px;
      padding: 1.25rem 1.5rem;
      margin-bottom: 1.5rem;
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .gate-warning .icon { font-size: 1.5rem; }

    .gate-warning .text { flex: 1; }
    .gate-warning .text strong { color: #ff4d6d; display: block; margin-bottom: 0.2rem; }
    .gate-warning .text span { color: #8b8fa8; font-size: 0.88rem; }

    .gate-warning .btn-link {
      background: transparent;
      border: 1px solid #ff4d6d;
      color: #ff4d6d;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      font-size: 0.85rem;
      cursor: pointer;
      font-weight: 600;
      white-space: nowrap;
    }

    .modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.7);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 100;
    }

    .modal {
      background: #1a1d2e;
      border: 1px solid #2a2d3e;
      border-radius: 14px;
      padding: 2rem;
      width: 400px;
      max-width: calc(100vw - 2rem);
    }

    .modal h3 { font-size: 1.1rem; font-weight: 700; color: #f0f0f0; margin: 0 0 1.25rem; }

    .modal .form-group { margin-bottom: 1rem; }
    .modal .form-group label {
      display: block;
      font-size: 0.8rem;
      color: #8b8fa8;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: 0.4rem;
    }

    .modal .form-group input,
    .modal .form-group textarea {
      width: 100%;
      background: #0f1117;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      padding: 0.65rem 0.9rem;
      color: #f0f0f0;
      font-size: 0.95rem;
      box-sizing: border-box;
    }

    .modal .form-group input:focus,
    .modal .form-group textarea:focus {
      outline: none;
      border-color: #00d4aa;
    }

    .modal .form-group textarea { min-height: 80px; resize: vertical; }

    .modal-actions { display: flex; gap: 0.75rem; margin-top: 1.5rem; }
    .modal-actions .btn-primary { flex: 1; padding: 0.75rem; text-align: center; }
    .modal-actions .btn-cancel {
      padding: 0.75rem 1.25rem;
      background: transparent;
      border: 1px solid #2a2d3e;
      color: #8b8fa8;
      border-radius: 8px;
      cursor: pointer;
      font-size: 0.9rem;
    }

    .trades-table {
      width: 100%;
      border-collapse: collapse;
    }

    .trades-table th {
      text-align: left;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: #8b8fa8;
      padding: 0.75rem 1rem;
      border-bottom: 1px solid #2a2d3e;
    }

    .trade-row {
      cursor: pointer;
      transition: background 0.15s;
    }

    .trade-row:hover td { background: #1a1d2e; }

    .trade-row td {
      padding: 1rem;
      border-bottom: 1px solid #161926;
      font-size: 0.9rem;
      color: #d0d3e8;
    }

    .symbol-cell {
      font-weight: 700;
      font-size: 1rem;
      color: #f0f0f0;
      letter-spacing: 0.05em;
    }

    .phases-progress {
      display: flex;
      gap: 4px;
    }

    .phase-dot {
      width: 10px;
      height: 10px;
      border-radius: 50%;
      background: #2a2d3e;
    }

    .phase-dot.done { background: #00d4aa; }

    .date-cell { color: #8b8fa8; font-size: 0.85rem; }

    .arrow-cell { color: #2a2d3e; text-align: right; }

    .empty-state {
      text-align: center;
      padding: 4rem 2rem;
      color: #8b8fa8;
    }

    .empty-state .icon { font-size: 3rem; margin-bottom: 1rem; }
    .empty-state h3 { font-size: 1.1rem; color: #d0d3e8; margin-bottom: 0.5rem; }
    .empty-state p { font-size: 0.9rem; line-height: 1.6; }

    .loading { text-align: center; padding: 3rem; color: #8b8fa8; }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Trade Journal</h1>
          <p>{{ store.tradeCount() }} trade{{ store.tradeCount() !== 1 ? 's' : '' }} recorded</p>
        </div>
        <button class="btn-primary"
          [disabled]="!backtestStore.isGateUnlocked()"
          (click)="openCreateModal()">
          + New Trade
        </button>
      </div>

      @if (!backtestStore.isGateUnlocked()) {
        <div class="gate-warning">
          <span class="icon">🔒</span>
          <div class="text">
            <strong>Backtest gate required</strong>
            <span>Complete your backtest integrity check before journaling live trades.</span>
          </div>
          <button class="btn-link" (click)="goToBacktest()">Complete Gate →</button>
        </div>
      }

      @if (store.loading()) {
        <div class="loading">Loading trades…</div>
      } @else if (store.trades().length === 0) {
        <div class="empty-state">
          <div class="icon">📋</div>
          <h3>No trades yet</h3>
          <p>Create your first trade to start journaling your emotional state across all four phases.</p>
        </div>
      } @else {
        <table class="trades-table">
          <thead>
            <tr>
              <th>Symbol</th>
              <th>Phases</th>
              <th>Date</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (trade of store.trades(); track trade.id) {
              <tr class="trade-row" (click)="openTrade(trade)">
                <td class="symbol-cell">{{ trade.symbol }}</td>
                <td>
                  <div class="phases-progress">
                    @for (phase of ['ChartAnalysis', 'Execution', 'DuringTrade', 'PostTrade']; track phase) {
                      <div class="phase-dot" [class.done]="hasPhase(trade, phase)"></div>
                    }
                  </div>
                </td>
                <td class="date-cell">{{ trade.createdAt | date:'MMM d, y' }}</td>
                <td class="arrow-cell">›</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>

    @if (showCreateModal()) {
      <div class="modal-backdrop" (click)="closeModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>New Trade</h3>
          <div class="form-group">
            <label>Symbol</label>
            <input type="text" [(ngModel)]="newSymbol" placeholder="e.g. AAPL, ES, NQ" maxlength="20"
              style="text-transform: uppercase" />
          </div>
          <div class="form-group">
            <label>Notes (optional)</label>
            <textarea [(ngModel)]="newNotes" placeholder="Setup description, context…"></textarea>
          </div>
          <div class="modal-actions">
            <button class="btn-cancel" (click)="closeModal()">Cancel</button>
            <button class="btn-primary"
              [disabled]="!newSymbol.trim() || store.saving()"
              (click)="createTrade()">
              {{ store.saving() ? 'Creating…' : 'Create Trade' }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
})
export class TradeListComponent implements OnInit {
  readonly store = inject(TradeJournalStore);
  readonly backtestStore = inject(BacktestStore);
  private readonly router = inject(Router);

  showCreateModal = signal(false);
  newSymbol = '';
  newNotes = '';

  async ngOnInit() {
    await Promise.all([
      this.store.loadTrades(),
      this.backtestStore.loadStatus(),
    ]);
  }

  hasPhase(trade: Trade, phase: string): boolean {
    return trade.journalEntries.some(e => e.phase === phase);
  }

  openTrade(trade: Trade) {
    this.router.navigate(['/journal', trade.id]);
  }

  openCreateModal() {
    this.newSymbol = '';
    this.newNotes = '';
    this.showCreateModal.set(true);
  }

  closeModal() {
    this.showCreateModal.set(false);
  }

  async createTrade() {
    const trade = await this.store.createTrade(
      this.newSymbol.trim().toUpperCase(),
      this.newNotes.trim() || undefined
    );
    if (trade) {
      this.closeModal();
      this.router.navigate(['/journal', trade.id]);
    }
  }

  goToBacktest() {
    this.router.navigate(['/backtest']);
  }
}
