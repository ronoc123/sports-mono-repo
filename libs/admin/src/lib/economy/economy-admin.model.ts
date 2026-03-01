// ── DTOs ─────────────────────────────────────────────────────────────────────

export interface RarityTierConfigDto {
  id: string;
  rarityName: string;
  ratingMin: number;
  ratingMax: number;
  pullWeightBps: number;
}

export interface PackConfigDto {
  orgId: string;
  packPointCost: number;
}

// ── Request bodies ────────────────────────────────────────────────────────────

export interface UpdateRarityTierRequest {
  ratingMin: number;
  ratingMax: number;
  pullWeightBps: number;
}

export interface UpdatePackConfigRequest {
  packPointCost: number;
}
