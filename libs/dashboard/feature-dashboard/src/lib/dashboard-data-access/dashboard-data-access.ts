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
export interface QuickAction {
  label: string;
  icon: string;
  route: string;
  color: string;
}

export interface RecentActivity {
  title: string;
  description: string;
  timestamp: string;
  type: string; // info, success, warning, error
}

export interface DashboardStats {
  activeOrganizations: number;
  totalUsers: number;
  activePlayerOptions: number;
  systemHealth: number;
  systemStatus: string;
  quickActions: QuickAction[];
  recentActivities: RecentActivity[];
}

export interface UserDashboard {
  welcomeMessage: string;
  personalStats: {
    votesRemaining: number;
    optionsParticipated: number;
    organizationsJoined: number;
    accountLevel: string;
  };
  recentActivity: RecentActivity[];
  quickActions: QuickAction[];
}

// State
interface DashboardState {
  stats: DashboardStats | null;
  userDashboard: UserDashboard | null;
  isLoading: boolean;
  error: string | null;
  lastUpdated: string | null;
}

const initialState: DashboardState = {
  stats: null,
  userDashboard: null,
  isLoading: false,
  error: null,
  lastUpdated: null,
};

// Dashboard Store
export const DashboardStore = signalStore(
  { providedIn: "root" },
  withState(initialState),
  withComputed((store) => ({
    hasStats: computed(() => !!store.stats()),
    hasUserDashboard: computed(() => !!store.userDashboard()),
    systemHealthStatus: computed(() => {
      const health = store.stats()?.systemHealth || 0;
      if (health >= 95) return 'excellent';
      if (health >= 85) return 'good';
      if (health >= 70) return 'fair';
      return 'poor';
    }),
    systemHealthColor: computed(() => {
      const health = store.stats()?.systemHealth || 0;
      if (health >= 95) return 'primary';
      if (health >= 85) return 'accent';
      if (health >= 70) return 'warn';
      return 'warn';
    }),
  })),
  withMethods((store, http = inject(HttpClient)) => {
    const API_BASE_URL = 'http://localhost:5000/api'; // Update this to match your API URL
    
    return {
    // Load dashboard statistics
    loadDashboardStats(): void {
      patchState(store, { isLoading: true, error: null });

      http.get<{success: boolean, data: DashboardStats, message: string}>(`${API_BASE_URL}/Dashboard/stats`)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock data
            const mockStats: DashboardStats = {
              activeOrganizations: 3,
              totalUsers: 1247,
              activePlayerOptions: 8,
              systemHealth: 98.5,
              systemStatus: "operational",
              quickActions: [
                {
                  label: "Create Player Option",
                  icon: "add_circle",
                  route: "/player-options/create",
                  color: "primary"
                },
                {
                  label: "Add New User",
                  icon: "person_add",
                  route: "/users/create",
                  color: "accent"
                },
                {
                  label: "View Reports",
                  icon: "assessment",
                  route: "/reports",
                  color: "primary"
                },
                {
                  label: "System Settings",
                  icon: "settings",
                  route: "/system/settings",
                  color: "warn"
                }
              ],
              recentActivities: [
                {
                  title: "New user registered",
                  description: "John Doe joined the platform",
                  timestamp: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
                  type: "success"
                },
                {
                  title: "Player option created",
                  description: "Trade deadline decision for Team Alpha",
                  timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
                  type: "info"
                },
                {
                  title: "System maintenance",
                  description: "Scheduled maintenance completed successfully",
                  timestamp: new Date(Date.now() - 4 * 60 * 60 * 1000).toISOString(),
                  type: "success"
                },
                {
                  title: "High vote activity",
                  description: "Unusual voting pattern detected",
                  timestamp: new Date(Date.now() - 6 * 60 * 60 * 1000).toISOString(),
                  type: "warning"
                }
              ]
            };
            return of(mockStats).pipe(delay(500));
          })
        )
        .subscribe((stats) => {
          patchState(store, {
            stats,
            isLoading: false,
            error: null,
            lastUpdated: new Date().toISOString(),
          });
        });
    },

    // Load user-specific dashboard
    loadUserDashboard(userId: string = "user-123"): void {
      patchState(store, { isLoading: true, error: null });

      http.get<{success: boolean, data: UserDashboard, message: string}>(`${API_BASE_URL}/Dashboard/user/${userId}`)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock data
            const mockUserDashboard: UserDashboard = {
              welcomeMessage: "Welcome back, User!",
              personalStats: {
                votesRemaining: 250,
                optionsParticipated: 12,
                organizationsJoined: 2,
                accountLevel: "gold"
              },
              recentActivity: [
                {
                  title: "Voted on player trade",
                  description: "You voted on the quarterback trade option",
                  timestamp: new Date(Date.now() - 60 * 60 * 1000).toISOString(),
                  type: "success"
                },
                {
                  title: "Joined new organization",
                  description: "You joined the Fantasy Football League",
                  timestamp: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
                  type: "info"
                }
              ],
              quickActions: [
                {
                  label: "Vote on Options",
                  icon: "how_to_vote",
                  route: "/player-options",
                  color: "primary"
                },
                {
                  label: "View Profile",
                  icon: "person",
                  route: "/profile",
                  color: "accent"
                },
                {
                  label: "Redeem Codes",
                  icon: "redeem",
                  route: "/redeem",
                  color: "primary"
                }
              ]
            };
            return of(mockUserDashboard).pipe(delay(500));
          })
        )
        .subscribe((userDashboard) => {
          patchState(store, {
            userDashboard,
            isLoading: false,
            error: null,
            lastUpdated: new Date().toISOString(),
          });
        });
    },

    // Refresh all dashboard data
    refreshDashboard(userId?: string): void {
      this.loadDashboardStats();
      if (userId) {
        this.loadUserDashboard(userId);
      }
    },

    // Reset store
    reset(): void {
      patchState(store, initialState);
    },
  };
  })
);
