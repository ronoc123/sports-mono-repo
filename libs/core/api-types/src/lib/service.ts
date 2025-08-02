export interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

// Pagination interfaces
export interface PaginationRequest {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Common filter interfaces
export interface PlayerFilter extends PaginationRequest {
  leagueId?: string;
  organizationId?: string;
  position?: string;
  minAge?: number;
  maxAge?: number;
  isActive?: boolean;
}

export interface PlayerOptionFilter extends PaginationRequest {
  organizationId?: string;
  playerId?: string;
  isActive?: boolean;
  isExpired?: boolean;
}

export interface OrganizationFilter extends PaginationRequest {
  leagueId?: string;
  sport?: string;
  isLocked?: boolean;
}

export interface UserFilter extends PaginationRequest {
  email?: string;
  userName?: string;
}

export interface CodeFilter extends PaginationRequest {
  organizationId?: string;
  userId?: string;
  isRedeemed?: boolean;
  isExpired?: boolean;
}

export interface ThemeFilter extends PaginationRequest {
  organizationId?: string;
  isActive?: boolean;
  isDefault?: boolean;
}

// Error response interface
export interface ErrorResponse {
  success: false;
  message: string;
  errors?: { [key: string]: string[] };
  statusCode?: number;
}
