import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BacktestStore } from '@trading/backtest-data-access';
import { INTEGRITY_QUESTIONS, AnswerInput } from '@trading/backtest-data-access';

type Step = 'metrics' | 'questions' | 'result';

@Component({
  selector: 'lib-backtest-gate',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host { display: block; }

    .gate-container {
      max-width: 720px;
      margin: 0 auto;
      padding: 2rem;
    }

    .gate-header {
      margin-bottom: 2.5rem;
    }

    .gate-header h1 {
      font-size: 1.75rem;
      font-weight: 700;
      color: #f0f0f0;
      margin: 0 0 0.5rem;
    }

    .gate-header p {
      color: #8b8fa8;
      margin: 0;
      font-size: 0.95rem;
      line-height: 1.5;
    }

    .step-indicator {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 2rem;
    }

    .step-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: #2a2d3e;
      transition: background 0.2s;
    }

    .step-dot.active { background: #00d4aa; }
    .step-dot.done { background: #3d8b6f; }

    .card {
      background: #1a1d2e;
      border: 1px solid #2a2d3e;
      border-radius: 12px;
      padding: 2rem;
    }

    .card h2 {
      font-size: 1.1rem;
      font-weight: 600;
      color: #f0f0f0;
      margin: 0 0 1.5rem;
    }

    .form-group {
      margin-bottom: 1.25rem;
    }

    .form-group label {
      display: block;
      font-size: 0.85rem;
      font-weight: 500;
      color: #8b8fa8;
      margin-bottom: 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .form-group input {
      width: 100%;
      background: #0f1117;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      padding: 0.7rem 1rem;
      color: #f0f0f0;
      font-size: 1rem;
      box-sizing: border-box;
      transition: border-color 0.2s;
    }

    .form-group input:focus {
      outline: none;
      border-color: #00d4aa;
    }

    .expectancy-preview {
      background: #0f1117;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      padding: 1rem 1.25rem;
      margin-top: 1.25rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .expectancy-preview .label {
      font-size: 0.85rem;
      color: #8b8fa8;
    }

    .expectancy-preview .value {
      font-size: 1.5rem;
      font-weight: 700;
    }

    .positive { color: #00d4aa; }
    .negative { color: #ff4d6d; }

    .question-item {
      padding: 1.25rem 0;
      border-bottom: 1px solid #2a2d3e;
    }

    .question-item:last-child { border-bottom: none; }

    .question-text {
      font-size: 0.95rem;
      color: #d0d3e8;
      margin-bottom: 1rem;
      line-height: 1.5;
    }

    .question-index {
      font-size: 0.75rem;
      color: #8b8fa8;
      margin-bottom: 0.4rem;
    }

    .answer-toggle {
      display: flex;
      gap: 0.75rem;
      margin-bottom: 0.75rem;
    }

    .answer-btn {
      flex: 1;
      padding: 0.6rem;
      border-radius: 8px;
      border: 1px solid #2a2d3e;
      background: transparent;
      color: #8b8fa8;
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }

    .answer-btn.yes.selected {
      background: rgba(0, 212, 170, 0.15);
      border-color: #00d4aa;
      color: #00d4aa;
    }

    .answer-btn.no.selected {
      background: rgba(255, 77, 109, 0.15);
      border-color: #ff4d6d;
      color: #ff4d6d;
    }

    .answer-btn:hover { opacity: 0.8; }

    .reason-input {
      width: 100%;
      background: #0f1117;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      padding: 0.6rem 0.9rem;
      color: #f0f0f0;
      font-size: 0.9rem;
      resize: vertical;
      min-height: 72px;
      box-sizing: border-box;
    }

    .reason-input:focus {
      outline: none;
      border-color: #8b6eff;
    }

    .actions {
      display: flex;
      gap: 1rem;
      margin-top: 2rem;
    }

    .btn-primary {
      flex: 1;
      padding: 0.85rem 1.5rem;
      background: linear-gradient(135deg, #00d4aa, #00b894);
      color: #0f1117;
      font-weight: 700;
      font-size: 0.95rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      transition: opacity 0.2s;
    }

    .btn-primary:hover { opacity: 0.9; }
    .btn-primary:disabled { opacity: 0.4; cursor: not-allowed; }

    .btn-secondary {
      padding: 0.85rem 1.5rem;
      background: transparent;
      color: #8b8fa8;
      font-weight: 600;
      font-size: 0.95rem;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      cursor: pointer;
    }

    .result-card {
      text-align: center;
      padding: 3rem 2rem;
    }

    .result-icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    .result-card h2 {
      font-size: 1.5rem;
      margin-bottom: 0.75rem;
    }

    .result-card p {
      color: #8b8fa8;
      line-height: 1.6;
      margin-bottom: 2rem;
    }

    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1rem;
      margin: 1.5rem 0;
    }

    .metric-pill {
      background: #0f1117;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      padding: 0.75rem;
      text-align: center;
    }

    .metric-pill .val {
      font-size: 1.25rem;
      font-weight: 700;
      color: #00d4aa;
    }

    .metric-pill .lbl {
      font-size: 0.7rem;
      color: #8b8fa8;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-top: 0.2rem;
    }

    .error-msg {
      color: #ff4d6d;
      font-size: 0.85rem;
      margin-top: 1rem;
      padding: 0.75rem 1rem;
      background: rgba(255, 77, 109, 0.1);
      border-radius: 8px;
      border: 1px solid rgba(255, 77, 109, 0.3);
    }

    .locked-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      background: rgba(255, 77, 109, 0.15);
      border: 1px solid rgba(255, 77, 109, 0.3);
      color: #ff4d6d;
      border-radius: 20px;
      padding: 0.3rem 0.9rem;
      font-size: 0.8rem;
      font-weight: 600;
      margin-bottom: 1.5rem;
    }

    .unlocked-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      background: rgba(0, 212, 170, 0.15);
      border: 1px solid rgba(0, 212, 170, 0.3);
      color: #00d4aa;
      border-radius: 20px;
      padding: 0.3rem 0.9rem;
      font-size: 0.8rem;
      font-weight: 600;
      margin-bottom: 1.5rem;
    }
  `],
  template: `
    <div class="gate-container">
      <div class="gate-header">
        <h1>Backtest Integrity Gate</h1>
        <p>Before you can journal live trades, complete your backtest review.
           This ensures your strategy has a proven edge.</p>
      </div>

      <!-- Step indicator -->
      <div class="step-indicator">
        <div class="step-dot" [class.active]="currentStep() === 'metrics'" [class.done]="stepDone('metrics')"></div>
        <div class="step-dot" [class.active]="currentStep() === 'questions'" [class.done]="stepDone('questions')"></div>
        <div class="step-dot" [class.active]="currentStep() === 'result'"></div>
      </div>

      <!-- Already unlocked -->
      @if (store.isGateUnlocked() && currentStep() === 'result') {
        <div class="card result-card">
          <div class="result-icon">🔓</div>
          <div class="unlocked-badge">✓ Gate Unlocked</div>
          <h2 class="positive">Your edge is verified</h2>
          <p>Your backtest confirms a positive expectancy. You're cleared to journal live trades.</p>
          <div class="metrics-grid">
            <div class="metric-pill">
              <div class="val">{{ (store.expectancy()! * 100 | number:'1.1-1') }}%</div>
              <div class="lbl">Expectancy</div>
            </div>
            <div class="metric-pill">
              <div class="val">{{ store.answeredCount() }}/6</div>
              <div class="lbl">Answered</div>
            </div>
            <div class="metric-pill">
              <div class="val">✓</div>
              <div class="lbl">All Yes</div>
            </div>
          </div>
          <button class="btn-primary" (click)="goToJournal()">Start Trading Journal →</button>
        </div>
      }

      <!-- Step 1: Metrics -->
      @if (currentStep() === 'metrics') {
        <div class="card">
          <h2>Step 1 — Backtest Performance</h2>
          <div class="form-group">
            <label>Total Trades</label>
            <input type="number" [(ngModel)]="totalTrades" placeholder="e.g. 200" min="1" />
          </div>
          <div class="form-group">
            <label>Win Rate (0.0 – 1.0)</label>
            <input type="number" [(ngModel)]="winRate" placeholder="e.g. 0.55" min="0" max="1" step="0.01" />
          </div>
          <div class="form-group">
            <label>Average Risk:Reward</label>
            <input type="number" [(ngModel)]="avgRR" placeholder="e.g. 1.5" min="0.01" step="0.1" />
          </div>

          @if (previewExpectancy !== null) {
            <div class="expectancy-preview">
              <span class="label">Expectancy Preview</span>
              <span class="value" [class.positive]="previewExpectancy > 0" [class.negative]="previewExpectancy <= 0">
                {{ previewExpectancy > 0 ? '+' : '' }}{{ previewExpectancy | number:'1.3-3' }}R
              </span>
            </div>
          }

          @if (store.error()) {
            <div class="error-msg">{{ store.error() }}</div>
          }

          <div class="actions">
            <button class="btn-primary"
              [disabled]="!canSubmitMetrics() || store.submittingMetrics()"
              (click)="submitMetrics()">
              {{ store.submittingMetrics() ? 'Saving…' : 'Submit Metrics →' }}
            </button>
          </div>
        </div>
      }

      <!-- Step 2: Integrity Questions -->
      @if (currentStep() === 'questions') {
        <div class="card">
          <h2>Step 2 — Integrity Check</h2>
          @for (q of questions; track q; let i = $index) {
            <div class="question-item">
              <div class="question-index">Question {{ i + 1 }} of 6</div>
              <div class="question-text">{{ q }}</div>
              <div class="answer-toggle">
                <button class="answer-btn yes"
                  [class.selected]="answers[i].answer === true"
                  (click)="setAnswer(i, true)">✓ Yes</button>
                <button class="answer-btn no"
                  [class.selected]="answers[i].answer === false"
                  (click)="setAnswer(i, false)">✗ No</button>
              </div>
              @if (answers[i].answer === false) {
                <textarea class="reason-input"
                  [(ngModel)]="answers[i].reason"
                  placeholder="Briefly explain why not…"></textarea>
              }
            </div>
          }

          @if (store.error()) {
            <div class="error-msg">{{ store.error() }}</div>
          }

          <div class="actions">
            <button class="btn-secondary" (click)="currentStep.set('metrics')">← Back</button>
            <button class="btn-primary"
              [disabled]="!allAnswered() || store.submittingAnswers()"
              (click)="submitAnswers()">
              {{ store.submittingAnswers() ? 'Submitting…' : 'Submit Answers →' }}
            </button>
          </div>
        </div>
      }

      <!-- Result: Gate locked -->
      @if (currentStep() === 'result' && !store.isGateUnlocked()) {
        <div class="card result-card">
          <div class="result-icon">🔒</div>
          <div class="locked-badge">Gate Locked</div>
          <h2 class="negative">Gate not unlocked yet</h2>
          <p>
            @if (!store.metricsSubmitted()) {
              You need to submit backtest metrics first.
            } @else if ((store.expectancy() ?? 0) <= 0) {
              Your expectancy is negative ({{ store.expectancy() | number:'1.3-3' }}R).
              A positive expectancy is required to proceed.
            } @else {
              You answered No to one or more integrity questions.
              Review your answers and resubmit when ready.
            }
          </p>
          <div class="actions">
            <button class="btn-primary" (click)="restart()">Try Again</button>
          </div>
        </div>
      }
    </div>
  `,
})
export class BacktestGateComponent implements OnInit {
  readonly store = inject(BacktestStore);
  private readonly router = inject(Router);

  currentStep = signal<Step>('metrics');

  totalTrades = 0;
  winRate = 0;
  avgRR = 0;

  questions = INTEGRITY_QUESTIONS;
  answers: { answer: boolean | null; reason: string | null }[] = INTEGRITY_QUESTIONS.map(() => ({
    answer: null,
    reason: null,
  }));

  get previewExpectancy(): number | null {
    if (!this.winRate || !this.avgRR) return null;
    return (this.winRate * this.avgRR) - (1 - this.winRate);
  }

  async ngOnInit() {
    await this.store.loadStatus();
    if (this.store.status()) {
      if (this.store.isGateUnlocked()) {
        this.currentStep.set('result');
      } else if (this.store.metricsSubmitted()) {
        this.currentStep.set('questions');
      }
    } else {
      await this.store.createSession();
    }
  }

  canSubmitMetrics() {
    return this.totalTrades > 0 && this.winRate > 0 && this.winRate <= 1 && this.avgRR > 0;
  }

  allAnswered() {
    return this.answers.every(a => a.answer !== null);
  }

  stepDone(step: Step): boolean {
    const order: Step[] = ['metrics', 'questions', 'result'];
    return order.indexOf(step) < order.indexOf(this.currentStep());
  }

  setAnswer(index: number, value: boolean) {
    this.answers[index] = { ...this.answers[index], answer: value, reason: value ? null : this.answers[index].reason };
  }

  async submitMetrics() {
    await this.store.submitMetrics({
      totalTrades: this.totalTrades,
      winRate: this.winRate,
      avgRR: this.avgRR,
    });
    if (!this.store.error()) {
      this.currentStep.set('questions');
    }
  }

  async submitAnswers() {
    const payload: AnswerInput[] = this.answers.map((a, i) => ({
      questionIndex: i + 1,
      answer: a.answer ?? false,
      reason: a.reason,
    }));
    await this.store.submitAnswers(payload);
    if (!this.store.error()) {
      this.currentStep.set('result');
    }
  }

  restart() {
    this.currentStep.set('metrics');
    this.answers = INTEGRITY_QUESTIONS.map(() => ({ answer: null, reason: null }));
  }

  goToJournal() {
    this.router.navigate(['/journal']);
  }
}
