import { Component, inject, OnInit, signal, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReviewStore } from '@trading/review-data-access';
import { ReviewMessage } from '@trading/review-data-access';

@Component({
  selector: 'lib-review-session',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host { display: block; }

    .page { max-width: 780px; margin: 0 auto; padding: 2rem; }

    .page-header { margin-bottom: 2rem; }
    .page-header h1 { font-size: 1.75rem; font-weight: 700; color: #f0f0f0; margin: 0 0 0.35rem; }
    .page-header p { color: #8b8fa8; margin: 0; font-size: 0.9rem; }

    /* Status card */
    .status-card {
      background: #1a1d2e;
      border: 1px solid #2a2d3e;
      border-radius: 12px;
      padding: 2rem;
      text-align: center;
      margin-bottom: 2rem;
    }

    .progress-ring {
      width: 120px;
      height: 120px;
      margin: 0 auto 1.5rem;
      position: relative;
    }

    .progress-ring svg { transform: rotate(-90deg); }

    .progress-text {
      position: absolute;
      inset: 0;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
    }

    .progress-text .count {
      font-size: 2rem;
      font-weight: 800;
      color: #f0f0f0;
      line-height: 1;
    }

    .progress-text .label {
      font-size: 0.7rem;
      color: #8b8fa8;
      text-transform: uppercase;
      letter-spacing: 0.08em;
    }

    .status-title {
      font-size: 1.1rem;
      font-weight: 600;
      color: #f0f0f0;
      margin-bottom: 0.5rem;
    }

    .status-subtitle { color: #8b8fa8; font-size: 0.9rem; margin-bottom: 1.5rem; }

    .btn-primary {
      padding: 0.8rem 2rem;
      background: linear-gradient(135deg, #00d4aa, #00b894);
      color: #0f1117;
      font-weight: 700;
      font-size: 0.95rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
    }

    .btn-primary:disabled { opacity: 0.4; cursor: not-allowed; }

    /* Chat */
    .chat-card {
      background: #1a1d2e;
      border: 1px solid #2a2d3e;
      border-radius: 12px;
      overflow: hidden;
    }

    .chat-header {
      padding: 1rem 1.5rem;
      border-bottom: 1px solid #2a2d3e;
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .chat-header .dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: #00d4aa;
      animation: pulse 2s infinite;
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.4; }
    }

    .chat-header .title { font-weight: 600; color: #f0f0f0; font-size: 0.95rem; }
    .chat-header .period { color: #8b8fa8; font-size: 0.82rem; margin-left: auto; }

    .messages-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      padding: 1.5rem;
      max-height: 420px;
      overflow-y: auto;
      scroll-behavior: smooth;
    }

    .messages-list::-webkit-scrollbar { width: 4px; }
    .messages-list::-webkit-scrollbar-thumb { background: #2a2d3e; border-radius: 4px; }

    .intro-message {
      background: rgba(139, 110, 255, 0.08);
      border: 1px solid rgba(139, 110, 255, 0.2);
      border-radius: 12px;
      border-bottom-left-radius: 4px;
      padding: 1rem 1.25rem;
      color: #d0d3e8;
      font-size: 0.9rem;
      line-height: 1.6;
    }

    .bubble-wrap { display: flex; gap: 0.6rem; }
    .bubble-wrap.user { flex-direction: row-reverse; }

    .avatar {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      font-size: 0.75rem;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .avatar.ai { background: rgba(139, 110, 255, 0.2); }
    .avatar.user-av { background: rgba(0, 212, 170, 0.2); color: #00d4aa; }

    .bubble {
      max-width: 80%;
      padding: 0.75rem 1rem;
      border-radius: 12px;
      font-size: 0.88rem;
      line-height: 1.55;
    }

    .bubble.assistant {
      background: rgba(139, 110, 255, 0.12);
      border: 1px solid rgba(139, 110, 255, 0.2);
      color: #d0d3e8;
      border-bottom-left-radius: 4px;
    }

    .bubble.user-msg {
      background: rgba(0, 212, 170, 0.12);
      border: 1px solid rgba(0, 212, 170, 0.2);
      color: #d0d3e8;
      border-bottom-right-radius: 4px;
    }

    .typing-indicator { display: flex; gap: 4px; padding: 0.5rem 0; }
    .typing-dot { width: 6px; height: 6px; border-radius: 50%; background: #8b6eff; animation: bounce 1.2s infinite; }
    .typing-dot:nth-child(2) { animation-delay: 0.2s; }
    .typing-dot:nth-child(3) { animation-delay: 0.4s; }
    @keyframes bounce {
      0%, 60%, 100% { transform: translateY(0); }
      30% { transform: translateY(-6px); }
    }

    .input-row {
      display: flex;
      gap: 0.6rem;
      padding: 1rem 1.5rem;
      border-top: 1px solid #2a2d3e;
      align-items: flex-end;
    }

    .input-row textarea {
      flex: 1;
      background: #0f1117;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      padding: 0.65rem 0.9rem;
      color: #f0f0f0;
      font-size: 0.9rem;
      resize: none;
      min-height: 42px;
      max-height: 120px;
      font-family: inherit;
      line-height: 1.4;
    }

    .input-row textarea:focus { outline: none; border-color: #8b6eff; }

    .send-btn {
      width: 42px;
      height: 42px;
      border-radius: 8px;
      background: linear-gradient(135deg, #8b6eff, #6a4fff);
      border: none;
      color: #fff;
      font-size: 1rem;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .send-btn:disabled { opacity: 0.4; cursor: not-allowed; }

    .loading { text-align: center; padding: 3rem; color: #8b8fa8; }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h1>Biweekly Review</h1>
        <p>AI-guided performance review. Available after 8 trades in the last 14 days.</p>
      </div>

      @if (store.loading()) {
        <div class="loading">Loading…</div>
      } @else if (!store.isReady() && !store.session()) {
        <div class="status-card">
          <div class="progress-ring">
            <svg width="120" height="120" viewBox="0 0 120 120">
              <circle cx="60" cy="60" r="50" fill="none" stroke="#2a2d3e" stroke-width="8"/>
              <circle cx="60" cy="60" r="50" fill="none" stroke="#00d4aa" stroke-width="8"
                stroke-linecap="round"
                [attr.stroke-dasharray]="314"
                [attr.stroke-dashoffset]="314 - (314 * store.tradeCount() / store.tradesRequired())" />
            </svg>
            <div class="progress-text">
              <div class="count">{{ store.tradeCount() }}</div>
              <div class="label">/ {{ store.tradesRequired() }}</div>
            </div>
          </div>
          <div class="status-title">{{ store.tradesNeeded() }} more trade{{ store.tradesNeeded() !== 1 ? 's' : '' }} needed</div>
          <div class="status-subtitle">Keep journaling. Your review will unlock at {{ store.tradesRequired() }} trades in 14 days.</div>
        </div>
      } @else {
        <div class="chat-card">
          <div class="chat-header">
            <div class="dot"></div>
            <div class="title">AI Review Session</div>
            @if (store.session()) {
              <div class="period">
                {{ store.session()!.periodStart | date:'MMM d' }} –
                {{ store.session()!.periodEnd | date:'MMM d, y' }}
              </div>
            }
          </div>

          <div class="messages-list" #messageList>
            @if (!store.session()?.messages?.length) {
              <div class="intro-message">
                👋 Welcome to your biweekly review. I've analysed your recent trades and journal entries.
                Send me a message to begin — or just say "Let's start" and I'll open with my observations.
              </div>
            }

            @for (msg of store.session()?.messages ?? []; track msg.id) {
              <div class="bubble-wrap" [class.user]="msg.role === 'User'">
                <div class="avatar" [class.ai]="msg.role === 'Assistant'" [class.user-av]="msg.role === 'User'">
                  {{ msg.role === 'Assistant' ? '🤖' : 'U' }}
                </div>
                <div class="bubble" [class.assistant]="msg.role === 'Assistant'" [class.user-msg]="msg.role === 'User'">
                  {{ msg.content }}
                </div>
              </div>
            }

            @if (store.sending()) {
              <div class="bubble-wrap">
                <div class="avatar ai">🤖</div>
                <div class="typing-indicator">
                  <div class="typing-dot"></div>
                  <div class="typing-dot"></div>
                  <div class="typing-dot"></div>
                </div>
              </div>
            }
          </div>

          <div class="input-row">
            <textarea [(ngModel)]="inputText"
              placeholder="Share your thoughts or ask a question…"
              (keydown.enter)="onEnter($event)"
              rows="1"></textarea>
            <button class="send-btn"
              [disabled]="!inputText.trim() || store.sending()"
              (click)="send()">➤</button>
          </div>
        </div>
      }
    </div>
  `,
})
export class ReviewSessionComponent implements OnInit, AfterViewChecked {
  readonly store = inject(ReviewStore);

  @ViewChild('messageList') messageListRef!: ElementRef<HTMLElement>;

  inputText = '';
  private shouldScroll = false;

  async ngOnInit() {
    await this.store.loadStatus();
    if (this.store.status()?.sessionId) {
      await this.store.loadSession(this.store.status()!.sessionId!);
    }
  }

  ngAfterViewChecked() {
    if (this.shouldScroll) {
      const el = this.messageListRef?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
      this.shouldScroll = false;
    }
  }

  async send() {
    const text = this.inputText.trim();
    if (!text) return;
    this.inputText = '';
    this.shouldScroll = true;
    await this.store.sendMessage(text);
    this.shouldScroll = true;
  }

  onEnter(event: Event) {
    const ke = event as KeyboardEvent;
    if (!ke.shiftKey) { ke.preventDefault(); this.send(); }
  }
}
