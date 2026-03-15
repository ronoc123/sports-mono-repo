export type MessageRole = 'User' | 'Assistant';
export type AISessionContext = 'JournalEntry' | 'IntegrityAnswer';

export interface AIMessage {
  id: string;
  role: MessageRole;
  content: string;
  timestamp: string;
}

export interface AISession {
  id: string;
  userId: string;
  context: AISessionContext;
  linkedEntityId: string;
  messages: AIMessage[];
}

export interface SendAIMessageRequest {
  existingSessionId?: string;
  context: AISessionContext;
  linkedEntityId: string;
  userMessage: string;
  phaseHint?: string;
  emotionalStateHint?: string;
  isEffortlessHint?: boolean;
  notesHint?: string;
}
