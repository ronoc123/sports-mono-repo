import { Component, inject, Input, OnInit, signal, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AIConversationStore } from '@trading/ai-conversation-data-access';
import { AISessionContext, AIMessage } from '@trading/ai-conversation-data-access';

@Component({
  selector: 'lib-ai-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host { display: block; }

    .chat-container { display: flex; flex-direction: column; }

    .messages-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-height: 340px;
      overflow-y: auto;
      padding: 0.5rem 0;
      scroll-behavior: smooth;
    }

    .messages-list::-webkit-scrollbar { width: 4px; }
    .messages-list::-webkit-scrollbar-track { background: transparent; }
    .messages-list::-webkit-scrollbar-thumb { background: #2a2d3e; border-radius: 4px; }

    .empty-state {
      text-align: center;
      padding: 1.5rem;
      color: #8b8fa8;
      font-size: 0.88rem;
    }

    .empty-state .ai-icon { font-size: 2rem; margin-bottom: 0.5rem; }

    .bubble-wrap {
      display: flex;
      gap: 0.6rem;
    }

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
      margin-top: 2px;
    }

    .avatar.ai { background: rgba(139, 110, 255, 0.2); color: #8b6eff; }
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

    .bubble.user {
      background: rgba(0, 212, 170, 0.12);
      border: 1px solid rgba(0, 212, 170, 0.2);
      color: #d0d3e8;
      border-bottom-right-radius: 4px;
    }

    .timestamp {
      font-size: 0.7rem;
      color: #8b8fa8;
      margin-top: 0.3rem;
      text-align: right;
    }

    .user .timestamp { text-align: left; }

    .input-row {
      display: flex;
      gap: 0.6rem;
      margin-top: 1rem;
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
      line-height: 1.4;
      font-family: inherit;
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
      transition: opacity 0.2s;
    }

    .send-btn:disabled { opacity: 0.4; cursor: not-allowed; }

    .typing-indicator {
      display: flex;
      gap: 4px;
      align-items: center;
      padding: 0.5rem 0;
    }

    .typing-dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background: #8b6eff;
      animation: bounce 1.2s infinite;
    }

    .typing-dot:nth-child(2) { animation-delay: 0.2s; }
    .typing-dot:nth-child(3) { animation-delay: 0.4s; }

    @keyframes bounce {
      0%, 60%, 100% { transform: translateY(0); }
      30% { transform: translateY(-6px); }
    }
  `],
  template: `
    <div class="chat-container">
      <div class="messages-list" #messageList>
        @if (!messages().length && !store.sending()) {
          <div class="empty-state">
            <div class="ai-icon">🤖</div>
            <p>Ask the AI anything about this phase of your trade.</p>
          </div>
        }

        @for (msg of messages(); track msg.id) {
          <div class="bubble-wrap" [class.user]="msg.role === 'User'">
            <div class="avatar" [class.ai]="msg.role === 'Assistant'" [class.user-av]="msg.role === 'User'">
              {{ msg.role === 'Assistant' ? '🤖' : 'U' }}
            </div>
            <div>
              <div class="bubble" [class.assistant]="msg.role === 'Assistant'" [class.user]="msg.role === 'User'">
                {{ msg.content }}
              </div>
              <div class="timestamp">{{ msg.timestamp | date:'h:mm a' }}</div>
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
        <textarea
          [(ngModel)]="inputText"
          placeholder="Share your thoughts…"
          (keydown.enter)="onEnter($event)"
          rows="1"></textarea>
        <button class="send-btn"
          [disabled]="!inputText.trim() || store.sending()"
          (click)="send()">
          ➤
        </button>
      </div>
    </div>
  `,
})
export class AIChatComponent implements OnInit, AfterViewChecked {
  @Input() linkedEntityId!: string;
  @Input() context: AISessionContext = 'JournalEntry';
  @Input() phaseHint?: string;
  @Input() emotionalStateHint?: string;
  @Input() isEffortlessHint?: boolean;
  @Input() notesHint?: string;

  @ViewChild('messageList') messageListRef!: ElementRef<HTMLElement>;

  readonly store = inject(AIConversationStore);

  inputText = '';
  private shouldScroll = false;

  messages = signal<AIMessage[]>([]);

  async ngOnInit() {
    await this.store.loadSession(this.linkedEntityId);
    this.syncMessages();
  }

  ngAfterViewChecked() {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  private syncMessages() {
    const session = this.store.getSession(this.linkedEntityId);
    this.messages.set(session?.messages ?? []);
  }

  async send() {
    const text = this.inputText.trim();
    if (!text) return;
    this.inputText = '';
    this.shouldScroll = true;

    await this.store.sendMessage({
      context: this.context,
      linkedEntityId: this.linkedEntityId,
      userMessage: text,
      phaseHint: this.phaseHint,
      emotionalStateHint: this.emotionalStateHint,
      isEffortlessHint: this.isEffortlessHint,
      notesHint: this.notesHint,
    });

    this.syncMessages();
    this.shouldScroll = true;
  }

  onEnter(event: Event) {
    const ke = event as KeyboardEvent;
    if (!ke.shiftKey) {
      ke.preventDefault();
      this.send();
    }
  }

  private scrollToBottom() {
    const el = this.messageListRef?.nativeElement;
    if (el) el.scrollTop = el.scrollHeight;
  }
}
