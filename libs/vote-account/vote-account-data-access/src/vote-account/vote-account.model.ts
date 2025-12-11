export interface VoteTransaction {
  id: number;
  orgId: string;
  userId: string;
  amount: number;
  reason: string;
  refId?: string | null;
  playerOptionId?: string | null;
  spendId?: string | null;
  createdAt: string;
}

export interface VoteAccount {
  orgId: string;
  userId: string;
  balance: number;
  version: number;
  createdAt: string;
  updatedAt: string;
  transactions: VoteTransaction[];
}
