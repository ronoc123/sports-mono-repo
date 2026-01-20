export interface PlayerDto {
  id: string;
  leagueId: string;
  organizationId?: string | null;
  name: string;
  position?: string | null;
  imageUrl?: string | null;
  age?: number | null;
  isActive?: boolean;
  isVeteran?: boolean;
  isYoungPlayer?: boolean;
}
