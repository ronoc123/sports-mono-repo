import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TradeJournalStore } from '@trading/trade-journal-data-access';
import { AIChatComponent } from '@trading/ai-conversation-feature';
import {
  TRADE_PHASES,
  EMOTIONAL_STATES,
  EmotionalState,
  TradePhase,
  JournalEntry,
} from '@trading/trade-journal-data-access';

@Component({
  selector: 'lib-trade-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, AIChatComponent],
  styles: [`
    :host { display: block; }

    .page { max-width: 900px; margin: 0 auto; padding: 2rem; }

    .back-btn {
      background: transparent;
      border: none;
      color: #8b8fa8;
      font-size: 0.9rem;
      cursor: pointer;
      padding: 0;
      margin-bottom: 1.5rem;
      display: flex;
      align-items: center;
      gap: 0.4rem;
    }

    .back-btn:hover { color: #f0f0f0; }

    .trade-header {
      margin-bottom: 2rem;
    }

    .trade-header h1 {
      font-size: 2rem;
      font-weight: 800;
      color: #f0f0f0;
      margin: 0 0 0.35rem;
      letter-spacing: 0.08em;
    }

    .trade-header .date { color: #8b8fa8; font-size: 0.88rem; }

    .phases-grid {
      display: grid;
      gap: 1rem;
    }

    .phase-card {
      background: #1a1d2e;
      border: 1px solid #2a2d3e;
      border-radius: 12px;
      overflow: hidden;
      transition: border-color 0.2s;
    }

    .phase-card.active-phase { border-color: #00d4aa; }

    .phase-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1.1rem 1.5rem;
      cursor: pointer;
      background: transparent;
      border: none;
      width: 100%;
      text-align: left;
    }

    .phase-header:hover { background: rgba(255,255,255,0.02); }

    .phase-header-left {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .phase-icon { font-size: 1.25rem; }

    .phase-title {
      font-size: 1rem;
      font-weight: 600;
      color: #f0f0f0;
    }

    .phase-subtitle { font-size: 0.8rem; color: #8b8fa8; margin-top: 0.15rem; }

    .phase-status {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: #2a2d3e;
    }

    .status-dot.done { background: #00d4aa; }

    .chevron {
      color: #2a2d3e;
      font-size: 1rem;
      transition: transform 0.2s;
    }

    .chevron.open { transform: rotate(90deg); }

    .phase-body {
      padding: 0 1.5rem 1.5rem;
      border-top: 1px solid #2a2d3e;
    }

    .field-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
      margin-top: 1.25rem;
    }

    .field-group label {
      display: block;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: #8b8fa8;
      margin-bottom: 0.5rem;
    }

    .emotion-select {
      width: 100%;
      background: #0f1117;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      padding: 0.65rem 0.9rem;
      color: #f0f0f0;
      font-size: 0.95rem;
    }

    .emotion-select:focus { outline: none; border-color: #8b6eff; }

    .toggle-row {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-top: 1.25rem;
    }

    .toggle-label { color: #d0d3e8; font-size: 0.9rem; flex: 1; }

    .toggle {
      position: relative;
      width: 44px;
      height: 24px;
      background: #2a2d3e;
      border-radius: 12px;
      cursor: pointer;
      transition: background 0.2s;
      flex-shrink: 0;
    }

    .toggle.on { background: #00d4aa; }

    .toggle-knob {
      position: absolute;
      top: 3px;
      left: 3px;
      width: 18px;
      height: 18px;
      background: #fff;
      border-radius: 50%;
      transition: transform 0.2s;
    }

    .toggle.on .toggle-knob { transform: translateX(20px); }

    .notes-area {
      margin-top: 1.25rem;
    }

    .notes-area label {
      display: block;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: #8b8fa8;
      margin-bottom: 0.5rem;
    }

    .notes-area textarea {
      width: 100%;
      background: #0f1117;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      padding: 0.75rem 1rem;
      color: #f0f0f0;
      font-size: 0.9rem;
      resize: vertical;
      min-height: 90px;
      box-sizing: border-box;
      line-height: 1.5;
    }

    .notes-area textarea:focus { outline: none; border-color: #8b6eff; }

    .phase-actions {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-top: 1.25rem;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .btn-save {
      padding: 0.65rem 1.5rem;
      background: linear-gradient(135deg, #00d4aa, #00b894);
      color: #0f1117;
      font-weight: 700;
      font-size: 0.88rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
    }

    .btn-save:disabled { opacity: 0.4; cursor: not-allowed; }

    .btn-ai {
      padding: 0.65rem 1.25rem;
      background: transparent;
      border: 1px solid #8b6eff;
      color: #8b6eff;
      font-weight: 600;
      font-size: 0.88rem;
      border-radius: 8px;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 0.4rem;
    }

    .btn-ai:hover { background: rgba(139, 110, 255, 0.1); }

    .saved-badge {
      color: #00d4aa;
      font-size: 0.85rem;
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 0.3rem;
    }

    .ai-panel {
      margin-top: 1rem;
      border-top: 1px solid #2a2d3e;
      padding-top: 1rem;
    }

    .loading { text-align: center; padding: 3rem; color: #8b8fa8; }

    .emotion-badge {
      display: inline-flex;
      align-items: center;
      padding: 0.25rem 0.75rem;
      border-radius: 12px;
      font-size: 0.78rem;
      font-weight: 600;
      background: rgba(139, 110, 255, 0.15);
      color: #8b6eff;
      border: 1px solid rgba(139, 110, 255, 0.3);
    }
  `],
  template: `
    <div class="page">
      <button class="back-btn" (click)="back()">← All Trades</button>

      @if (store.loading()) {
        <div class="loading">Loading trade…</div>
      } @else {
      @if (store.selectedTrade(); as trade) {
        <div class="trade-header">
          <h1>{{ trade.symbol }}</h1>
          <div class="date">{{ trade.createdAt | date:'EEEE, MMMM d, y' }}</div>
        </div>

        <div class="phases-grid">
          @for (phase of phases; track phase.key) {
            <div class="phase-card" [class.active-phase]="activePhase() === phase.key">
              <button class="phase-header" (click)="togglePhase(phase.key)">
                <div class="phase-header-left">
                  <span class="phase-icon">{{ phase.icon }}</span>
                  <div>
                    <div class="phase-title">{{ phase.label }}</div>
                    @if (getEntry(trade, phase.key); as entry) {
                      <div class="phase-subtitle">
                        <span class="emotion-badge">{{ entry.emotionalState }}</span>
                        {{ entry.isEffortless ? ' · Effortless' : '' }}
                      </div>
                    } @else {
                      <div class="phase-subtitle">Not journaled yet</div>
                    }
                  </div>
                </div>
                <div class="phase-status">
                  <div class="status-dot" [class.done]="getEntry(trade, phase.key)"></div>
                  <span class="chevron" [class.open]="activePhase() === phase.key">›</span>
                </div>
              </button>

              @if (activePhase() === phase.key) {
                <div class="phase-body">
                  <div class="field-row">
                    <div class="field-group">
                      <label>Emotional State</label>
                      <select class="emotion-select" [(ngModel)]="draft[phase.key].emotionalState">
                        @for (e of emotionalStates; track e) {
                          <option [value]="e">{{ e }}</option>
                        }
                      </select>
                    </div>
                  </div>

                  <div class="toggle-row">
                    <span class="toggle-label">Did this phase feel effortless?</span>
                    <div class="toggle" [class.on]="draft[phase.key].isEffortless"
                      (click)="draft[phase.key].isEffortless = !draft[phase.key].isEffortless">
                      <div class="toggle-knob"></div>
                    </div>
                  </div>

                  <div class="notes-area">
                    <label>Notes</label>
                    <textarea [(ngModel)]="draft[phase.key].notes"
                      placeholder="What were you thinking? What did you notice?"></textarea>
                  </div>

                  <div class="phase-actions">
                    <button class="btn-save"
                      [disabled]="store.saving()"
                      (click)="saveEntry(trade.id, phase.key)">
                      {{ store.saving() ? 'Saving…' : 'Save Entry' }}
                    </button>

                    @if (getEntry(trade, phase.key); as entry) {
                      <button class="btn-ai" (click)="toggleAI(phase.key, entry)">
                        🤖 {{ showAI() === phase.key ? 'Close AI' : 'Talk to AI' }}
                      </button>
                    }

                    @if (justSaved() === phase.key) {
                      <span class="saved-badge">✓ Saved</span>
                    }
                  </div>

                  @if (showAI() === phase.key && getEntry(trade, phase.key); as entry) {
                    <div class="ai-panel">
                      <lib-ai-chat
                        [linkedEntityId]="entry.id"
                        [context]="'JournalEntry'"
                        [phaseHint]="phase.key"
                        [emotionalStateHint]="entry.emotionalState"
                        [isEffortlessHint]="entry.isEffortless"
                        [notesHint]="entry.freeFormNotes ?? ''"
                      />
                    </div>
                  }
                </div>
              }
            </div>
          }
        </div>
      }
      }
    </div>
  `,
})
export class TradeDetailComponent implements OnInit {
  readonly store = inject(TradeJournalStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  phases = TRADE_PHASES;
  emotionalStates = EMOTIONAL_STATES;

  activePhase = signal<TradePhase | null>(null);
  showAI = signal<TradePhase | null>(null);
  justSaved = signal<TradePhase | null>(null);

  draft: Record<string, { emotionalState: EmotionalState; isEffortless: boolean; notes: string }> = {
    ChartAnalysis: { emotionalState: 'Calm', isEffortless: true, notes: '' },
    Execution:     { emotionalState: 'Calm', isEffortless: true, notes: '' },
    DuringTrade:   { emotionalState: 'Calm', isEffortless: true, notes: '' },
    PostTrade:     { emotionalState: 'Calm', isEffortless: true, notes: '' },
  };

  async ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    await this.store.loadTrade(id);
    this.syncDraftFromTrade();
  }

  private syncDraftFromTrade() {
    const trade = this.store.selectedTrade();
    if (!trade) return;
    for (const entry of trade.journalEntries) {
      this.draft[entry.phase] = {
        emotionalState: entry.emotionalState,
        isEffortless: entry.isEffortless,
        notes: entry.freeFormNotes ?? '',
      };
    }
  }

  getEntry(trade: { journalEntries: JournalEntry[] }, phase: string): JournalEntry | undefined {
    return trade.journalEntries.find(e => e.phase === phase);
  }

  togglePhase(phase: TradePhase) {
    this.activePhase.set(this.activePhase() === phase ? null : phase);
    this.showAI.set(null);
  }

  toggleAI(phase: TradePhase, _entry: JournalEntry) {
    this.showAI.set(this.showAI() === phase ? null : phase);
  }

  async saveEntry(tradeId: string, phase: TradePhase) {
    const d = this.draft[phase];
    await this.store.upsertEntry(tradeId, {
      phase,
      emotionalState: d.emotionalState,
      isEffortless: d.isEffortless,
      freeFormNotes: d.notes || undefined,
    });
    this.justSaved.set(phase);
    setTimeout(() => this.justSaved.set(null), 2000);
  }

  back() {
    this.router.navigate(['/journal']);
  }
}
