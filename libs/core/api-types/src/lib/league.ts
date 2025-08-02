// League interfaces matching C# domain models

export interface League {
  id: string;
  name: string;
  sport: string;
  country: string;
  description?: string;
  logoUrl?: string;
  website?: string;
  foundedYear?: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  
  // Related entities (optional populated data)
  organizations?: Organization[];
  players?: Player[];
}

// Request/Response DTOs for League operations
export interface CreateLeagueRequest {
  name: string;
  sport: string;
  country: string;
  description?: string;
  logoUrl?: string;
  website?: string;
  foundedYear?: number;
}

export interface UpdateLeagueRequest {
  id: string;
  name?: string;
  sport?: string;
  country?: string;
  description?: string;
  logoUrl?: string;
  website?: string;
  foundedYear?: number;
  isActive?: boolean;
}

export interface LeagueListResponse {
  leagues: League[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Import types from other files
import { Organization } from './organization';
import { Player } from './player';
