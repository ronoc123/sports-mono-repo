export type RarityTier = 'Common' | 'Rare' | 'Epic' | 'Legendary';

export interface RarityTierConfig {
  id: string;
  orgId: string;
  rarityName: RarityTier;
  ratingMin: number;
  ratingMax: number;
  pullWeightBps: number; // basis points, sum across tiers = 10000 (100%)
}

export interface CardPlayer {
  id: string;
  leagueId: string;
  name: string;
  position: string;
  overallRating: number;
  rarityTier: RarityTier;
  createdAt: string;
  updatedAt?: string;
}

export interface CardOwner {
  id: string;
  userId: string;
  cardPlayerId: string;
  orgId: string;
  isListed: boolean;
  acquiredAt: string;
  cardPlayer?: CardPlayer;
}

// ── Request / Response DTOs ──

// ── Pack types ──

export interface UserCard {
  id: string;
  cardPackId: string;
  cardPlayerId: string;
  leagueId: string;
  name: string;
  position: string;
  overallRating: number;
  rarityTier: RarityTier;
  isListed: boolean;
  pulledAt?: string;
}

export interface PackPurchaseRequest {
  userId: string;
  orgId: string;
  leagueId: string;
}

export interface PackPurchaseResult {
  packId: string;
  pointsSpent: number;
  remainingBalance: number;
  cards: UserCard[];
}

export interface CreateCardPlayerRequest {
  leagueId: string;
  orgId: string;
  name: string;
  position: string;
  overallRating: number;
}

export interface UpdateCardPlayerRequest {
  cardPlayerId: string;
  orgId: string;
  leagueId: string;
  name: string;
  position: string;
  overallRating: number;
}
