export type EmotionalState = 'Calm' | 'Confident' | 'Anxious' | 'Frustrated' | 'FOMO' | 'Defeated';
export type TradePhase = 'ChartAnalysis' | 'Execution' | 'DuringTrade' | 'PostTrade';

export const EMOTIONAL_STATES: EmotionalState[] = [
  'Calm', 'Confident', 'Anxious', 'Frustrated', 'FOMO', 'Defeated',
];

export const TRADE_PHASES: { key: TradePhase; label: string; icon: string }[] = [
  { key: 'ChartAnalysis', label: 'Chart Analysis', icon: '📊' },
  { key: 'Execution',     label: 'Execution',      icon: '⚡' },
  { key: 'DuringTrade',  label: 'During Trade',   icon: '⏱️' },
  { key: 'PostTrade',    label: 'Post Trade',      icon: '📝' },
];

export interface JournalEntry {
  id: string;
  phase: TradePhase;
  emotionalState: EmotionalState;
  isEffortless: boolean;
  freeFormNotes: string | null;
  aISessionId: string | null;
}

export interface Trade {
  id: string;
  userId: string;
  symbol: string;
  notes: string | null;
  journalEntries: JournalEntry[];
  createdAt: string | null;
}

export interface CreateTradeRequest {
  symbol: string;
  notes?: string;
}

export interface UpsertJournalEntryRequest {
  phase: TradePhase;
  emotionalState: EmotionalState;
  isEffortless: boolean;
  freeFormNotes?: string;
}
