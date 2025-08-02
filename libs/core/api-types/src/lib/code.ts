// Code interfaces matching C# domain models

export interface Code {
  id: string;
  codeValue: string;
  organizationId: string;
  userId?: string;
  votes: number;
  isRedeemed: boolean;
  redeemedAt?: string;
  expiresAt?: string;
  createdAt: string;
  updatedAt?: string;
  
  // Computed properties
  isExpired: boolean;
  isActive: boolean;
  
  // Related entities (optional populated data)
  organization?: Organization;
  user?: User;
}

// Request/Response DTOs for Code operations
export interface GenerateCodeRequest {
  organizationId: string;
  votes: number;
  expiresAt?: string;
}

export interface RedeemCodeRequest {
  codeValue: string;
  userId: string;
}

export interface CodeListResponse {
  codes: Code[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CodeRedemptionResponse {
  success: boolean;
  message: string;
  votesAdded?: number;
  organizationName?: string;
}

// Import types from other files
import { Organization } from './organization';
import { User } from './user';
