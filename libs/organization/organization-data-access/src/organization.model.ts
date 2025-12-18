// -------------------------------------------------------
// ORGANIZATION QUERY MODELS (Angular frontend)
// -------------------------------------------------------

export interface GetAllOrganizationsQuery {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  leagueId?: string;
  sport?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

// -------------------------------------------------------
// ORGANIZATION DTOs (mirror backend DTOs exactly)
// -------------------------------------------------------

export interface OrganizationDto {
  id: string;
  name: string;
  teamId?: string | null;
  teamName?: string | null;
  teamShortName?: string | null;
  formedYear?: number | null;
  sport?: string | null;
  stadium?: string | null;
  location?: string | null;
  stadiumCapacity?: number | null;
  website?: string | null;
  facebook?: string | null;
  twitter?: string | null;
  instagram?: string | null;
  description?: string | null;
  color1?: string | null;
  color2?: string | null;
  color3?: string | null;
  badgeUrl?: string | null;
  logoUrl?: string | null;
  fanart1Url?: string | null;
  fanart2Url?: string | null;
  fanart3Url?: string | null;
}

// -------------------------------------------------------
// ORGANIZATION DETAILS DTO (used on /organizationDetails)
// -------------------------------------------------------

// -------------------------------------------------------
// NESTED DTOs
// -------------------------------------------------------

export interface PlayerDto {
  id: string; // Guid in C# → string in TS
  leagueId: string; // Guid
  organizationId?: string | null; // Nullable Guid → optional string
  name: string;
  position: string;
  imageUrl: string;
  age: number;
  isActive: boolean;
  isVeteran: boolean;
  isYoungPlayer: boolean;
}

export interface ThemeDto {
  name: string;
  colorPrimary: string;
  colorSecondary: string;
  colorTertiary: string;
  logo: string;
}

// -------------------------------------------------------
// COMMAND MODELS (mirror backend Create / Update Requests)
// -------------------------------------------------------

export interface CreateOrganizationCommand {
  name: string;
  leagueId: string;

  teamId?: string | null;
  teamName?: string | null;
  teamShortName?: string | null;
  formedYear?: number | null;

  sport?: string | null;
  stadium?: string | null;
  location?: string | null;
  stadiumCapacity?: number | null;

  website?: string | null;
  facebook?: string | null;
  twitter?: string | null;
  instagram?: string | null;

  description?: string | null;

  color1?: string | null;
  color2?: string | null;
  color3?: string | null;

  badgeUrl?: string | null;
  logoUrl?: string | null;
  fanart1Url?: string | null;
  fanart2Url?: string | null;
  fanart3Url?: string | null;
}

export interface UpdateOrganizationCommand {
  organizationId: string;

  name: string;
  teamId?: string | null;
  teamName?: string | null;
  teamShortName?: string | null;
  formedYear?: number | null;

  sport?: string | null;
  description?: string | null;

  stadium?: string | null;
  location?: string | null;
  stadiumCapacity?: number | null;

  badgeUrl?: string | null;
  logoUrl?: string | null;
  fanart1Url?: string | null;
  fanart2Url?: string | null;
  fanart3Url?: string | null;

  website?: string | null;
  facebook?: string | null;
  twitter?: string | null;
  instagram?: string | null;

  color1?: string | null;
  color2?: string | null;
  color3?: string | null;
}

export interface DeleteOrganizationCommand {
  organizationId: string;
}
