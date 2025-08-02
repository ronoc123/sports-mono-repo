import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from '@sports-ui/http-client';
import {
  User,
  ServiceResponse,
  PaginatedResponse,
  UserFilter,
  CreateUserRequest,
  UpdateUserRequest,
  VoteRequest,
  AddVotesRequest,
} from '@sports-ui/api-types';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiService = inject(ApiService);

  /**
   * Get all users with pagination and filtering
   */
  getUsers(filter: UserFilter): Observable<ServiceResponse<PaginatedResponse<User>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.searchTerm) {
      params = params.set('searchTerm', filter.searchTerm);
    }
    if (filter.email) {
      params = params.set('email', filter.email);
    }
    if (filter.userName) {
      params = params.set('userName', filter.userName);
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortDescending !== undefined) {
      params = params.set('sortDescending', filter.sortDescending.toString());
    }

    return this.apiService.get<ServiceResponse<PaginatedResponse<User>>>('api/User/all', params);
  }

  /**
   * Get a single user by ID
   */
  getUser(userId: string): Observable<ServiceResponse<User>> {
    return this.apiService.get<ServiceResponse<User>>(`api/User/${userId}`);
  }

  /**
   * Get current user profile
   */
  getCurrentUser(): Observable<ServiceResponse<User>> {
    return this.apiService.get<ServiceResponse<User>>('api/User/profile');
  }

  /**
   * Create a new user
   */
  createUser(user: CreateUserRequest): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, CreateUserRequest>('api/User/create', user);
  }

  /**
   * Update an existing user
   */
  updateUser(user: UpdateUserRequest): Observable<ServiceResponse<boolean>> {
    return this.apiService.put<ServiceResponse<boolean>, UpdateUserRequest>('api/User/update', user);
  }

  /**
   * Delete a user
   */
  deleteUser(userId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.delete<ServiceResponse<boolean>>(`api/User/delete/${userId}`);
  }

  /**
   * Update user email
   */
  updateEmail(userId: string, email: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.put<ServiceResponse<boolean>, { userId: string; email: string }>(
      'api/User/update-email',
      { userId, email }
    );
  }

  /**
   * Update username
   */
  updateUsername(userId: string, userName: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.put<ServiceResponse<boolean>, { userId: string; userName: string }>(
      'api/User/update-username',
      { userId, userName }
    );
  }

  /**
   * Get user votes
   */
  getUserVotes(userId: string): Observable<ServiceResponse<any[]>> {
    return this.apiService.get<ServiceResponse<any[]>>(`api/User/${userId}/votes`);
  }

  /**
   * Get user available votes by organization
   */
  getUserVotesAvailable(userId: string): Observable<ServiceResponse<any[]>> {
    return this.apiService.get<ServiceResponse<any[]>>(`api/User/${userId}/votes-available`);
  }

  /**
   * Get user redeemed codes
   */
  getUserCodes(userId: string): Observable<ServiceResponse<any[]>> {
    return this.apiService.get<ServiceResponse<any[]>>(`api/User/${userId}/codes`);
  }

  /**
   * Add votes for user in organization
   */
  addVotesForUser(request: AddVotesRequest): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, AddVotesRequest>('api/User/add-votes', request);
  }

  /**
   * Use vote for organization
   */
  useVoteForOrganization(userId: string, organizationId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { userId: string; organizationId: string }>(
      'api/User/use-vote',
      { userId, organizationId }
    );
  }

  /**
   * Get user statistics
   */
  getUserStats(userId: string): Observable<ServiceResponse<UserStats>> {
    return this.apiService.get<ServiceResponse<UserStats>>(`api/User/${userId}/stats`);
  }

  /**
   * Get user activity
   */
  getUserActivity(userId: string): Observable<ServiceResponse<UserActivity[]>> {
    return this.apiService.get<ServiceResponse<UserActivity[]>>(`api/User/${userId}/activity`);
  }

  /**
   * Search users by email or username
   */
  searchUsers(searchTerm: string): Observable<ServiceResponse<User[]>> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.apiService.get<ServiceResponse<User[]>>('api/User/search', params);
  }

  /**
   * Check if email is available
   */
  checkEmailAvailability(email: string): Observable<ServiceResponse<boolean>> {
    const params = new HttpParams().set('email', email);
    return this.apiService.get<ServiceResponse<boolean>>('api/User/check-email', params);
  }

  /**
   * Check if username is available
   */
  checkUsernameAvailability(userName: string): Observable<ServiceResponse<boolean>> {
    const params = new HttpParams().set('userName', userName);
    return this.apiService.get<ServiceResponse<boolean>>('api/User/check-username', params);
  }
}

// Additional interfaces for user operations
export interface UserStats {
  totalVotes: number;
  totalVotesUsed: number;
  totalVotesAvailable: number;
  totalCodesRedeemed: number;
  organizationsWithVotes: number;
  favoriteOrganization?: any;
  recentActivity: UserActivity[];
}

export interface UserActivity {
  id: string;
  type: 'vote' | 'code_redeemed' | 'profile_updated';
  description: string;
  organizationId?: string;
  organizationName?: string;
  playerOptionId?: string;
  playerOptionTitle?: string;
  createdAt: string;
}
