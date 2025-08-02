import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from '@sports-ui/http-client';
import {
  Vote,
  ServiceResponse,
  PaginatedResponse,
  PaginationRequest,
  VoteRequest,
} from '@sports-ui/api-types';

@Injectable({
  providedIn: 'root'
})
export class VoteService {
  private readonly apiService = inject(ApiService);

  /**
   * Get all votes with pagination and filtering
   */
  getVotes(filter: VoteFilter): Observable<ServiceResponse<PaginatedResponse<Vote>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.searchTerm) {
      params = params.set('searchTerm', filter.searchTerm);
    }
    if (filter.userId) {
      params = params.set('userId', filter.userId);
    }
    if (filter.organizationId) {
      params = params.set('organizationId', filter.organizationId);
    }
    if (filter.playerOptionId) {
      params = params.set('playerOptionId', filter.playerOptionId);
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortDescending !== undefined) {
      params = params.set('sortDescending', filter.sortDescending.toString());
    }

    return this.apiService.get<ServiceResponse<PaginatedResponse<Vote>>>('api/Vote/all', params);
  }

  /**
   * Get a single vote by ID
   */
  getVote(voteId: string): Observable<ServiceResponse<Vote>> {
    return this.apiService.get<ServiceResponse<Vote>>(`api/Vote/${voteId}`);
  }

  /**
   * Cast a vote on a player option
   */
  castVote(request: VoteRequest): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, VoteRequest>('api/Vote/cast', request);
  }

  /**
   * Remove a vote
   */
  removeVote(voteId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.delete<ServiceResponse<boolean>>(`api/Vote/remove/${voteId}`);
  }

  /**
   * Get votes by user
   */
  getVotesByUser(userId: string): Observable<ServiceResponse<Vote[]>> {
    const params = new HttpParams().set('userId', userId);
    return this.apiService.get<ServiceResponse<Vote[]>>('api/Vote/by-user', params);
  }

  /**
   * Get votes by organization
   */
  getVotesByOrganization(organizationId: string): Observable<ServiceResponse<Vote[]>> {
    const params = new HttpParams().set('organizationId', organizationId);
    return this.apiService.get<ServiceResponse<Vote[]>>('api/Vote/by-organization', params);
  }

  /**
   * Get votes by player option
   */
  getVotesByPlayerOption(playerOptionId: string): Observable<ServiceResponse<Vote[]>> {
    const params = new HttpParams().set('playerOptionId', playerOptionId);
    return this.apiService.get<ServiceResponse<Vote[]>>('api/Vote/by-player-option', params);
  }

  /**
   * Get vote statistics
   */
  getVoteStats(): Observable<ServiceResponse<VoteStats>> {
    return this.apiService.get<ServiceResponse<VoteStats>>('api/Vote/stats');
  }

  /**
   * Get organization vote statistics
   */
  getOrganizationVoteStats(organizationId: string): Observable<ServiceResponse<OrganizationVoteStats>> {
    return this.apiService.get<ServiceResponse<OrganizationVoteStats>>(`api/Vote/organization/${organizationId}/stats`);
  }

  /**
   * Get player option vote statistics
   */
  getPlayerOptionVoteStats(playerOptionId: string): Observable<ServiceResponse<PlayerOptionVoteStats>> {
    return this.apiService.get<ServiceResponse<PlayerOptionVoteStats>>(`api/Vote/player-option/${playerOptionId}/stats`);
  }

  /**
   * Get user vote statistics
   */
  getUserVoteStats(userId: string): Observable<ServiceResponse<UserVoteStats>> {
    return this.apiService.get<ServiceResponse<UserVoteStats>>(`api/Vote/user/${userId}/stats`);
  }

  /**
   * Get recent votes
   */
  getRecentVotes(limit: number = 10): Observable<ServiceResponse<Vote[]>> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.apiService.get<ServiceResponse<Vote[]>>('api/Vote/recent', params);
  }

  /**
   * Get trending votes
   */
  getTrendingVotes(): Observable<ServiceResponse<Vote[]>> {
    return this.apiService.get<ServiceResponse<Vote[]>>('api/Vote/trending');
  }

  /**
   * Check if user can vote on player option
   */
  canUserVote(userId: string, playerOptionId: string): Observable<ServiceResponse<boolean>> {
    const params = new HttpParams()
      .set('userId', userId)
      .set('playerOptionId', playerOptionId);
    return this.apiService.get<ServiceResponse<boolean>>('api/Vote/can-vote', params);
  }

  /**
   * Get user's vote on specific player option
   */
  getUserVoteOnPlayerOption(userId: string, playerOptionId: string): Observable<ServiceResponse<Vote | null>> {
    const params = new HttpParams()
      .set('userId', userId)
      .set('playerOptionId', playerOptionId);
    return this.apiService.get<ServiceResponse<Vote | null>>('api/Vote/user-vote-on-option', params);
  }

  /**
   * Bulk cast votes
   */
  bulkCastVotes(requests: VoteRequest[]): Observable<ServiceResponse<string[]>> {
    return this.apiService.post<ServiceResponse<string[]>, VoteRequest[]>('api/Vote/bulk-cast', requests);
  }
}

// Filter interface for vote operations
export interface VoteFilter extends PaginationRequest {
  userId?: string;
  organizationId?: string;
  playerOptionId?: string;
}

// Additional interfaces for vote operations
export interface VoteStats {
  totalVotes: number;
  totalVoters: number;
  totalPlayerOptions: number;
  totalOrganizations: number;
  averageVotesPerOption: number;
  averageVotesPerUser: number;
  mostPopularOption?: any;
  mostActiveUser?: any;
  mostActiveOrganization?: any;
}

export interface OrganizationVoteStats {
  organizationId: string;
  organizationName: string;
  totalVotes: number;
  totalVoters: number;
  totalPlayerOptions: number;
  averageVotesPerOption: number;
  mostPopularOption?: any;
  recentVotes: Vote[];
}

export interface PlayerOptionVoteStats {
  playerOptionId: string;
  playerOptionTitle: string;
  totalVotes: number;
  uniqueVoters: number;
  votesPerDay: { [date: string]: number };
  recentVotes: Vote[];
  topVoters: any[];
}

export interface UserVoteStats {
  userId: string;
  userName: string;
  totalVotes: number;
  totalOrganizations: number;
  totalPlayerOptions: number;
  favoriteOrganization?: any;
  recentVotes: Vote[];
  votingHistory: { [date: string]: number };
}
