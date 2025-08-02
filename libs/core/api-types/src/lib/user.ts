// User interfaces matching C# domain models

export interface User {
  id: string;
  email: string;
  userName: string;
  firstName?: string;
  lastName?: string;
  profilePictureUrl?: string;
  isActive: boolean;
  emailVerified?: boolean;
  lastLoginAt?: string;
  provider?: string; // 'google', 'local', etc.
  createdAt: string;
  updatedAt?: string;

  // Related entities (optional populated data)
  votes?: Vote[];
  codes?: Code[];
  votesAvailable?: UserVotes[];
}

export interface UserVotes {
  id: string;
  userId: string;
  organizationId: string;
  votesRemaining: number;
  totalVotesEarned: number;
  createdAt: string;
  updatedAt?: string;

  // Related entities (optional populated data)
  organization?: Organization;
}

export interface Vote {
  id: string;
  userId: string;
  playerOptionId: string;
  organizationId: string;
  createdAt: string;

  // Related entities (optional populated data)
  user?: User;
  playerOption?: PlayerOption;
  organization?: Organization;
}

// Request/Response DTOs for User operations
export interface CreateUserRequest {
  email: string;
  userName: string;
}

export interface UpdateUserRequest {
  id: string;
  email?: string;
  userName?: string;
}

export interface UserListResponse {
  users: User[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface VoteRequest {
  playerOptionId: string;
  organizationId: string;
}

export interface AddVotesRequest {
  userId: string;
  organizationId: string;
  votes: number;
}

// Import types from other files
import { Organization } from "./organization";
import { PlayerOption } from "./player";
import { Code } from "./code";
