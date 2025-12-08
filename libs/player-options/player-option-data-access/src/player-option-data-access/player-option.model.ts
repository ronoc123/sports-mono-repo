import { Player } from "@sports-ui/api-types";
import { PlayerDto } from "@sports-ui/organization-data-access";

export interface PlayerOptionDto {
  id: string;
  title: string;
  description: string;
  votes: number;
  playerId: string;
  organizationId: string;
  isActive: boolean;
  isExpired: boolean;
  isPopular: boolean;
  isTrending: boolean;
  daysRemaining: number;
  player: PlayerDto;
}

export interface GetAllPlayerOptionsQuery {
  organizationId?: string | null;
  playerId?: string | null;
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string | null;
  isActive?: boolean | null;
  isExpired?: boolean | null;
  sortBy?: string | null;
  sortDescending?: boolean | null;
}
