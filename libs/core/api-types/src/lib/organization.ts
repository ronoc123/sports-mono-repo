// Organization interfaces matching C# domain models

export interface Organization {
  id: string;
  leagueId: string;
  name: string;
  teamId?: string;
  teamName?: string;
  teamShortName?: string;
  formedYear?: number;
  sport?: string;
  venue: Venue;
  mediaAssets: MediaAssets;
  socialLinks: SocialLinks;
  teamColors: TeamColors;
  description?: string;
  isLocked: boolean;
  createdAt: string;
  updatedAt?: string;
  
  // Computed properties from business logic
  canCreatePlayerOptions: boolean;
  activePlayerOptionsCount: number;
  totalVotes: number;
  hasActivePlayerOptions: boolean;
  averagePlayerAge: number;
  activePlayersCount: number;
  totalMarketValue: number;
  
  // Related entities (optional populated data)
  playerOptions?: PlayerOption[];
  players?: Player[];
  codes?: Code[];
  league?: League;
}

export interface Venue {
  name: string;
  address: Address;
  capacity?: number;
  type?: string;
}

export interface Address {
  street: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  latitude?: number;
  longitude?: number;
}

export interface MediaAssets {
  logoUrl?: string;
  bannerUrl?: string;
  thumbnailUrl?: string;
  galleryUrls?: string[];
}

export interface SocialLinks {
  website?: string;
  twitter?: string;
  facebook?: string;
  instagram?: string;
  youtube?: string;
  linkedin?: string;
}

export interface TeamColors {
  primary: string;
  secondary?: string;
  accent?: string;
  text?: string;
}

// Request/Response DTOs for Organization operations
export interface CreateOrganizationRequest {
  leagueId: string;
  name: string;
  teamId?: string;
  teamName?: string;
  teamShortName?: string;
  formedYear?: number;
  sport?: string;
  venue: Venue;
  mediaAssets: MediaAssets;
  socialLinks: SocialLinks;
  teamColors: TeamColors;
  description?: string;
}

export interface UpdateOrganizationRequest {
  id: string;
  name?: string;
  teamId?: string;
  teamName?: string;
  teamShortName?: string;
  formedYear?: number;
  sport?: string;
  venue?: Venue;
  mediaAssets?: MediaAssets;
  socialLinks?: SocialLinks;
  teamColors?: TeamColors;
  description?: string;
}

export interface OrganizationListResponse {
  organizations: Organization[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Import types from other files
import { PlayerOption } from './player';
import { Player } from './player';
import { Code } from './code';
import { League } from './league';
