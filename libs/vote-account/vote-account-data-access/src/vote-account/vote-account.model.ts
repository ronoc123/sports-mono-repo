export interface VoteTransaction {
  id: number;
  leagueId: string;
  userId: string;
  amount: number;
  reason: string;
  refId?: string | null;
  playerOptionId?: string | null;
  spendId?: string | null;
  createdAt: string;
}

export interface VoteAccount {
  leagueId: string;
  userId: string;
  balance: number;
  version: number;
  createdAt: string;
  updatedAt: string;
  transactions: VoteTransaction[];
}
