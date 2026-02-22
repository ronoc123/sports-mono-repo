export interface RosterPlayer {
  id: string;
  name: string;
  position: string;
  imageUrl?: string | null;
  age?: number | null;
  organizationId?: string | null;
  leagueId: string;
  isActive?: boolean;
  isVeteran?: boolean;
  isYoungPlayer?: boolean;
}
