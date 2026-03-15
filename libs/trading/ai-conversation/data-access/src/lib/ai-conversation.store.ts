import { inject } from '@angular/core';
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { AIConversationApi } from './ai-conversation.api';
import { AISession, SendAIMessageRequest } from './ai-conversation.models';

interface AIConversationState {
  sessions: Record<string, AISession>;
  sending: boolean;
  error: string | null;
}

export const AIConversationStore = signalStore(
  { providedIn: 'root' },
  withState<AIConversationState>({ sessions: {}, sending: false, error: null }),
  withMethods((store, api = inject(AIConversationApi)) => ({
    async loadSession(linkedEntityId: string) {
      try {
        const session = await api.getByEntity(linkedEntityId).toPromise();
        if (session) {
          patchState(store, (s) => ({
            sessions: { ...s.sessions, [linkedEntityId]: session },
          }));
        }
      } catch {
        patchState(store, { error: 'Failed to load AI session' });
      }
    },

    async sendMessage(req: SendAIMessageRequest): Promise<AISession | undefined> {
      patchState(store, { sending: true, error: null });
      try {
        const session = await api.sendMessage(req).toPromise();
        if (session) {
          patchState(store, (s) => ({
            sessions: { ...s.sessions, [req.linkedEntityId]: session },
            sending: false,
          }));
        }
        return session;
      } catch {
        patchState(store, { error: 'Failed to send message', sending: false });
        return undefined;
      }
    },

    getSession(linkedEntityId: string): AISession | undefined {
      return store.sessions()[linkedEntityId];
    },
  }))
);
