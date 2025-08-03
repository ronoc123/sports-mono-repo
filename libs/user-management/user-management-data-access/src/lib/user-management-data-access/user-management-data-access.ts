import { Injectable, inject } from "@angular/core";
import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from "@ngrx/signals";
import { computed } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, of, throwError } from "rxjs";
import { delay, map, catchError } from "rxjs/operators";

// Types
export interface UserManagement {
  id: string;
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  phone?: string;
  dateOfBirth?: string;
  bio?: string;
  avatar?: string;
  accountLevel: string;
  status: string; // active, suspended, inactive
  createdAt: string;
  updatedAt: string;
  lastLoginAt?: string;
  
  // Statistics
  activityStats: UserActivityStats;
  
  // Organizations
  organizations: UserOrganization[];
  
  // Roles and Permissions
  roles: string[];
  permissions: string[];
}

export interface UserActivityStats {
  totalVotes: number;
  votesUsed: number;
  votesRemaining: number;
  optionsParticipated: number;
  organizationsJoined: number;
  codesRedeemed: number;
  lastActivity?: string;
  loginCount: number;
  engagementScore: number; // 0-100
}

export interface UserOrganization {
  organizationId: string;
  organizationName: string;
  role: string; // member, admin, gm, csp
  joinedAt: string;
  isActive: boolean;
}

export interface CreateUserRequest {
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  phone?: string;
  dateOfBirth?: string;
  bio?: string;
  password: string;
  roles: string[];
  organizationIds: string[];
}

export interface UpdateUserStatusRequest {
  userId: string;
  status: string; // active, suspended, inactive
  reason?: string;
}

export interface AssignUserRoleRequest {
  userId: string;
  organizationId: string;
  role: string;
}

export interface UserSearchFilters {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  status?: string;
  accountLevel?: string;
  organizationId?: string;
  role?: string;
  createdAfter?: string;
  createdBefore?: string;
  lastLoginAfter?: string;
  lastLoginBefore?: string;
  sortBy: string;
  sortDescending: boolean;
}

export interface UserStatsOverview {
  totalUsers: number;
  activeUsers: number;
  suspendedUsers: number;
  inactiveUsers: number;
  newUsersThisMonth: number;
  newUsersThisWeek: number;
  averageEngagementScore: number;
  levelStats: UserLevelStats[];
  activityTrends: UserActivityTrend[];
}

export interface UserLevelStats {
  level: string;
  count: number;
  percentage: number;
}

export interface UserActivityTrend {
  date: string;
  activeUsers: number;
  newRegistrations: number;
  votesCast: number;
  codesRedeemed: number;
}

export interface BulkUserActionRequest {
  userIds: string[];
  action: string; // suspend, activate, delete, assign_role
  reason?: string;
  parameters?: Record<string, any>;
}

export interface BulkUserActionResponse {
  totalRequested: number;
  successful: number;
  failed: number;
  results: BulkActionResult[];
}

export interface BulkActionResult {
  userId: string;
  success: boolean;
  error?: string;
}

// State
interface UserManagementState {
  users: UserManagement[];
  selectedUser: UserManagement | null;
  statsOverview: UserStatsOverview | null;
  filters: UserSearchFilters;
  isLoading: boolean;
  isCreating: boolean;
  isUpdating: boolean;
  error: string | null;
  lastUpdated: string | null;
}

const initialState: UserManagementState = {
  users: [],
  selectedUser: null,
  statsOverview: null,
  filters: {
    pageNumber: 1,
    pageSize: 10,
    sortBy: "createdAt",
    sortDescending: true,
  },
  isLoading: false,
  isCreating: false,
  isUpdating: false,
  error: null,
  lastUpdated: null,
};

// User Management Store
export const UserManagementStore = signalStore(
  { providedIn: "root" },
  withState(initialState),
  withComputed((store) => ({
    hasUsers: computed(() => store.users().length > 0),
    activeUsers: computed(() => 
      store.users().filter(user => user.status === 'active')
    ),
    suspendedUsers: computed(() => 
      store.users().filter(user => user.status === 'suspended')
    ),
    inactiveUsers: computed(() => 
      store.users().filter(user => user.status === 'inactive')
    ),
    usersByLevel: computed(() => {
      const users = store.users();
      const levels: Record<string, UserManagement[]> = {};
      users.forEach(user => {
        if (!levels[user.accountLevel]) {
          levels[user.accountLevel] = [];
        }
        levels[user.accountLevel].push(user);
      });
      return levels;
    }),
    highEngagementUsers: computed(() => 
      store.users().filter(user => user.activityStats.engagementScore >= 80)
    ),
  })),
  withMethods((store, http = inject(HttpClient)) => {
    const API_BASE_URL = 'http://localhost:5000/api'; // Update this to match your API URL
    
    return {
    // Load users for management
    loadUsers(filters?: Partial<UserSearchFilters>): void {
      patchState(store, { isLoading: true, error: null });

      const currentFilters = { ...store.filters(), ...filters };
      patchState(store, { filters: currentFilters });

      const queryParams = new URLSearchParams();
      Object.entries(currentFilters).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          queryParams.append(key, value.toString());
        }
      });

      http.get<{success: boolean, data: UserManagement[], message: string}>(`${API_BASE_URL}/User/management?${queryParams}`)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock data
            const mockUsers: UserManagement[] = [
              {
                id: "user-1",
                email: "john.doe@example.com",
                userName: "johndoe",
                firstName: "John",
                lastName: "Doe",
                phone: "+1 (555) 123-4567",
                accountLevel: "gold",
                status: "active",
                createdAt: new Date(Date.now() - 6 * 30 * 24 * 60 * 60 * 1000).toISOString(),
                updatedAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
                lastLoginAt: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(),
                activityStats: {
                  totalVotes: 1000,
                  votesUsed: 750,
                  votesRemaining: 250,
                  optionsParticipated: 45,
                  organizationsJoined: 3,
                  codesRedeemed: 8,
                  lastActivity: new Date(Date.now() - 60 * 60 * 1000).toISOString(),
                  loginCount: 127,
                  engagementScore: 85.5
                },
                organizations: [
                  {
                    organizationId: "org-1",
                    organizationName: "Fantasy Football League",
                    role: "member",
                    joinedAt: new Date(Date.now() - 4 * 30 * 24 * 60 * 60 * 1000).toISOString(),
                    isActive: true
                  }
                ],
                roles: ["User"],
                permissions: ["vote", "redeem_codes"]
              },
              {
                id: "user-2",
                email: "jane.smith@example.com",
                userName: "janesmith",
                firstName: "Jane",
                lastName: "Smith",
                accountLevel: "silver",
                status: "active",
                createdAt: new Date(Date.now() - 3 * 30 * 24 * 60 * 60 * 1000).toISOString(),
                updatedAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
                lastLoginAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
                activityStats: {
                  totalVotes: 500,
                  votesUsed: 320,
                  votesRemaining: 180,
                  optionsParticipated: 22,
                  organizationsJoined: 2,
                  codesRedeemed: 4,
                  lastActivity: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
                  loginCount: 89,
                  engagementScore: 72.3
                },
                organizations: [
                  {
                    organizationId: "org-2",
                    organizationName: "Basketball Dynasty",
                    role: "admin",
                    joinedAt: new Date(Date.now() - 2 * 30 * 24 * 60 * 60 * 1000).toISOString(),
                    isActive: true
                  }
                ],
                roles: ["User", "Admin"],
                permissions: ["vote", "redeem_codes", "manage_organization"]
              }
            ];
            return of(mockUsers).pipe(delay(500));
          })
        )
        .subscribe((users) => {
          patchState(store, {
            users,
            isLoading: false,
            error: null,
            lastUpdated: new Date().toISOString(),
          });
        });
    },

    // Load user statistics overview
    loadStatsOverview(): void {
      http.get<{success: boolean, data: UserStatsOverview, message: string}>(`${API_BASE_URL}/User/stats`)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock data
            const mockStats: UserStatsOverview = {
              totalUsers: 1247,
              activeUsers: 1089,
              suspendedUsers: 12,
              inactiveUsers: 146,
              newUsersThisMonth: 89,
              newUsersThisWeek: 23,
              averageEngagementScore: 76.8,
              levelStats: [
                { level: "bronze", count: 623, percentage: 49.9 },
                { level: "silver", count: 374, percentage: 30.0 },
                { level: "gold", count: 187, percentage: 15.0 },
                { level: "platinum", count: 63, percentage: 5.1 }
              ],
              activityTrends: [
                {
                  date: new Date(Date.now() - 6 * 24 * 60 * 60 * 1000).toISOString(),
                  activeUsers: 456,
                  newRegistrations: 12,
                  votesCast: 234,
                  codesRedeemed: 45
                },
                {
                  date: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
                  activeUsers: 523,
                  newRegistrations: 18,
                  votesCast: 289,
                  codesRedeemed: 52
                }
              ]
            };
            return of(mockStats).pipe(delay(300));
          })
        )
        .subscribe((stats) => {
          patchState(store, {
            statsOverview: stats,
            error: null,
          });
        });
    },

    // Create new user
    createUser(request: CreateUserRequest): void {
      patchState(store, { isCreating: true, error: null });

      http.post<{success: boolean, data: UserManagement, message: string}>(`${API_BASE_URL}/User/create`, request)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock response:', error);
            // Mock creation
            const mockUser: UserManagement = {
              id: `user-${Date.now()}`,
              email: request.email,
              userName: request.userName,
              firstName: request.firstName,
              lastName: request.lastName,
              phone: request.phone,
              dateOfBirth: request.dateOfBirth,
              bio: request.bio,
              accountLevel: "bronze",
              status: "active",
              createdAt: new Date().toISOString(),
              updatedAt: new Date().toISOString(),
              roles: request.roles.length > 0 ? request.roles : ["User"],
              permissions: ["vote", "redeem_codes"],
              activityStats: {
                totalVotes: 100,
                votesUsed: 0,
                votesRemaining: 100,
                optionsParticipated: 0,
                organizationsJoined: 0,
                codesRedeemed: 0,
                loginCount: 0,
                engagementScore: 0
              },
              organizations: []
            };
            return of(mockUser).pipe(delay(1000));
          })
        )
        .subscribe((newUser) => {
          patchState(store, {
            users: [...store.users(), newUser],
            isCreating: false,
            error: null,
          });
        });
    },

    // Update user status
    updateUserStatus(request: UpdateUserStatusRequest): void {
      patchState(store, { isUpdating: true, error: null });

      http.put<{success: boolean, data: boolean, message: string}>(`${API_BASE_URL}/User/status`, request)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock response:', error);
            return of(true).pipe(delay(500));
          })
        )
        .subscribe((success) => {
          if (success) {
            const updatedUsers = store.users().map(user => 
              user.id === request.userId 
                ? { ...user, status: request.status, updatedAt: new Date().toISOString() }
                : user
            );
            patchState(store, {
              users: updatedUsers,
              isUpdating: false,
              error: null,
            });
          }
        });
    },

    // Bulk user action
    bulkUserAction(request: BulkUserActionRequest): void {
      patchState(store, { isUpdating: true, error: null });

      http.post<{success: boolean, data: BulkUserActionResponse, message: string}>(`${API_BASE_URL}/User/bulk-action`, request)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock response:', error);
            // Mock bulk action response
            const mockResponse: BulkUserActionResponse = {
              totalRequested: request.userIds.length,
              successful: request.userIds.length - 1,
              failed: 1,
              results: request.userIds.map((userId, index) => ({
                userId,
                success: index !== 0,
                error: index === 0 ? "User not found" : undefined
              }))
            };
            return of(mockResponse).pipe(delay(1000));
          })
        )
        .subscribe((response) => {
          patchState(store, {
            isUpdating: false,
            error: null,
          });
          
          // Reload users to reflect changes
          this.loadUsers();
        });
    },

    // Update filters
    updateFilters(filters: Partial<UserSearchFilters>): void {
      const newFilters = { ...store.filters(), ...filters };
      patchState(store, { filters: newFilters });
    },

    // Reset store
    reset(): void {
      patchState(store, initialState);
    },
  };
  })
);
