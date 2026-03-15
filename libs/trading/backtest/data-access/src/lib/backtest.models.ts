export interface BacktestMetrics {
  totalTrades: number;
  winRate: number;
  avgRR: number;
  expectancy: number;
}

export interface IntegrityAnswer {
  id: string;
  questionIndex: number;
  answer: boolean;
  journaledReason: string | null;
}

export interface BacktestSession {
  id: string;
  userId: string;
  isGateUnlocked: boolean;
  metrics: BacktestMetrics | null;
  answers: IntegrityAnswer[];
  createdAt: string | null;
}

export interface BacktestStatus {
  isGateUnlocked: boolean;
  expectancy: number | null;
  answeredCount: number;
  metricsSubmitted: boolean;
  answers: IntegrityAnswer[];
}

export interface SubmitMetricsRequest {
  totalTrades: number;
  winRate: number;
  avgRR: number;
}

export interface AnswerInput {
  questionIndex: number;
  answer: boolean;
  reason: string | null;
}

export const INTEGRITY_QUESTIONS: string[] = [
  'Did you follow your entry rules on every trade in this backtest?',
  'Did you respect your stop-loss rules without exception?',
  'Did you avoid entering trades outside your defined setup criteria?',
  'Did you record every trade taken during the backtest period?',
  'Did you avoid revenge trading or sizing up after losses?',
  'Do you believe this backtest accurately reflects how you will trade live?',
];
