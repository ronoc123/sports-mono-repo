// libs/player-option/data-access/src/lib/player-option.contracts.ts

// Simple aliases for readability
export type Guid = string; // e.g. "a3c1e1f2-..."
export type ISODateString = string; // e.g. "2025-10-04T12:34:56Z"

// -----------------------------
// Queries / Requests
// -----------------------------
export interface GetAllPlayerOptionsQuery {
  pageNumber?: number; // default 1 (server-side)
  pageSize?: number; // default 10
  searchTerm?: string | null;
  organizationId?: Guid | null;
  playerId?: Guid | null;
  isActive?: boolean | null;
  isExpired?: boolean | null;
  sortBy?: string | null; // e.g. "CreatedAt" (keep as string to match API)
  sortDescending?: boolean; // default true
}

// -----------------------------
// DTOs
// -----------------------------
export interface PlayerOptionDto {
  id: Guid;
  title: string;
  description: string;
  votes: number;
  expiresAt: ISODateString; // C# DateTime -> JSON ISO string
  createdAt: ISODateString; // "
  playerId: Guid;
  organizationId: Guid;

  // Business logic properties
  isActive: boolean;
  isExpired: boolean;
  isPopular: boolean;
  isTrending: boolean;
  daysRemaining: number;
  popularityLevel: string;
  engagementScore: number;

  // Related data
  playerName?: string | null;
  organizationName?: string | null;
}

// -----------------------------
// Response envelopes
// -----------------------------
export interface ServiceResponse<T> {
  data?: T | null;
  success: boolean; // true on success
  message: string; // optional info message
  // error metadata (present if success === false)
  errorCode?: string | null;
  traceId?: string | null;
  validationErrors?: Record<string, string[]> | null;
  details?: unknown | null;
}

// -----------------------------
// Pagination
// -----------------------------
export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
