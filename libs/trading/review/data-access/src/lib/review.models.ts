export type ReviewMessageRole = 'User' | 'Assistant';

export interface ReviewMessage {
  id: string;
  role: ReviewMessageRole;
  content: string;
  timestamp: string;
}

export interface ReviewSession {
  id: string;
  userId: string;
  periodStart: string;
  periodEnd: string;
  messages: ReviewMessage[];
}

export interface ReviewStatus {
  isReady: boolean;
  tradeCount: number;
  tradesRequired: number;
  daysUntilNext: number;
  sessionId: string | null;
}
