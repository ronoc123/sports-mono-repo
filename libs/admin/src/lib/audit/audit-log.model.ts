// ── DTOs ─────────────────────────────────────────────────────────────────────

export interface TransactionAuditDto {
  id: number;
  orgId: string;
  userId: string;
  amount: number;
  reason: string;
  refId: string | null;
  createdAt: string;
}

export interface TransactionAuditResult {
  items: TransactionAuditDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const TRANSACTION_REASONS = [
  'vote_spend',
  'reward_redeem',
  'adjust',
  'pack_purchase',
  'bid_escrow',
  'bid_release',
  'auction_sale',
  'buy_now',
  'h2h_wager',
  'h2h_win',
] as const;

export type TransactionReason = (typeof TRANSACTION_REASONS)[number];
