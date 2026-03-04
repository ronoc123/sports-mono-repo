// ── Request / Response DTOs ────────────────────────────────────────────────

export interface CreateMatchRequest {
  fanUserId: string;
  orgId: string;
  userCardIds: string[];
  wagerAmount: number;
}

export interface CreateMatchResult {
  matchId: string;
  outcome: 'win' | 'loss' | 'pending';
  fanTeamOverall: number;
  botTeamOverall: number;
  botSquadSnapshot: string; // JSON string
  updatedBalance: number;
  wagerAmount: number;
}

export interface BotSquadCard {
  name: string;
  overallRating: number;
  rarityTier: string;
}

export interface SquadCardDto {
  userCardId: string;
  playerName: string;
  position: string;
  overallRating: number;
  rarityTier: string;
  slotIndex: number;
}

export interface MatchDetailDto {
  matchId: string;
  fanUserId: string;
  wagerAmount: number;
  fanTeamOverall: number;
  botTeamOverall: number;
  botSquadSnapshot: string;
  outcome: string;
  status: string;
  createdAt: string;
  completedAt: string | null;
  fanSquadCards: SquadCardDto[];
}

export interface MatchSummaryDto {
  matchId: string;
  wagerAmount: number;
  fanTeamOverall: number;
  botTeamOverall: number;
  outcome: string;
  status: string;
  createdAt: string;
  completedAt: string | null;
}

export interface MatchHistoryResult {
  items: MatchSummaryDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}
