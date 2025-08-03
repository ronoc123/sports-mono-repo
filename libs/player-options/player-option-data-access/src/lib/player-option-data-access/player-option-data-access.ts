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
export interface PlayerOption {
  id: string;
  title: string;
  description: string;
  category: string; // trade, draft, lineup, strategy
  status: string; // active, expired, completed, cancelled
  createdAt: string;
  expiresAt?: string;
  completedAt?: string;
  
  // Organization and Player info
  organizationId: string;
  organizationName: string;
  playerId?: string;
  playerName?: string;
  playerPosition?: string;
  
  // Voting info
  totalVotes: number;
  votesRequired: number;
  hasUserVoted: boolean;
  userVoteChoice?: string;
  
  // Options/Choices
  choices: PlayerOptionChoice[];
  
  // Metadata
  createdBy: string;
  priority: number; // 1-5, 5 being highest
  tags: string[];
}

export interface PlayerOptionChoice {
  id: string;
  title: string;
  description: string;
  voteCount: number;
  votePercentage: number;
  isSelected: boolean;
  impactDescription?: string;
}

export interface PlayerOptionStats {
  totalOptions: number;
  activeOptions: number;
  completedOptions: number;
  expiredOptions: number;
  userParticipatedOptions: number;
  userVotesUsed: number;
  userVotesRemaining: number;
  categoryStats: CategoryStats[];
}

export interface CategoryStats {
  category: string;
  count: number;
  userParticipated: number;
}

export interface VoteRequest {
  playerOptionId: string;
  choiceId: string;
  userId: string;
}

export interface VoteResponse {
  success: boolean;
  message: string;
  updatedPlayerOption?: PlayerOption;
  remainingVotes: number;
}

export interface PlayerOptionFilters {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  organizationId?: string;
  playerId?: string;
  category?: string;
  status?: string;
  hasUserVoted?: boolean;
  priority?: number;
  sortBy: string;
  sortDescending: boolean;
}

// State
interface PlayerOptionState {
  playerOptions: PlayerOption[];
  selectedPlayerOption: PlayerOption | null;
  stats: PlayerOptionStats | null;
  filters: PlayerOptionFilters;
  isLoading: boolean;
  isVoting: boolean;
  error: string | null;
  lastUpdated: string | null;
}

const initialState: PlayerOptionState = {
  playerOptions: [],
  selectedPlayerOption: null,
  stats: null,
  filters: {
    pageNumber: 1,
    pageSize: 10,
    sortBy: "createdAt",
    sortDescending: true,
  },
  isLoading: false,
  isVoting: false,
  error: null,
  lastUpdated: null,
};

// Player Option Store
export const PlayerOptionStore = signalStore(
  { providedIn: "root" },
  withState(initialState),
  withComputed((store) => ({
    hasPlayerOptions: computed(() => store.playerOptions().length > 0),
    activeOptions: computed(() => 
      store.playerOptions().filter(option => option.status === 'active')
    ),
    userVotedOptions: computed(() => 
      store.playerOptions().filter(option => option.hasUserVoted)
    ),
    urgentOptions: computed(() => 
      store.playerOptions().filter(option => option.priority >= 4)
    ),
    optionsByCategory: computed(() => {
      const options = store.playerOptions();
      const categories: Record<string, PlayerOption[]> = {};
      options.forEach(option => {
        if (!categories[option.category]) {
          categories[option.category] = [];
        }
        categories[option.category].push(option);
      });
      return categories;
    }),
  })),
  withMethods((store, http = inject(HttpClient)) => {
    const API_BASE_URL = 'http://localhost:5000/api'; // Update this to match your API URL
    
    return {
    // Load player options for user
    loadPlayerOptions(userId: string = "user-123", filters?: Partial<PlayerOptionFilters>): void {
      patchState(store, { isLoading: true, error: null });

      const currentFilters = { ...store.filters(), ...filters };
      patchState(store, { filters: currentFilters });

      const queryParams = new URLSearchParams();
      Object.entries(currentFilters).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          queryParams.append(key, value.toString());
        }
      });

      http.get<{success: boolean, data: PlayerOption[], message: string}>(`${API_BASE_URL}/PlayerOption/user/${userId}?${queryParams}`)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock data
            const mockOptions: PlayerOption[] = [
              {
                id: "option-1",
                title: "Trade Quarterback Decision",
                description: "Should we trade our starting quarterback for draft picks?",
                category: "trade",
                status: "active",
                createdAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
                expiresAt: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(),
                organizationId: "org-1",
                organizationName: "Fantasy Football League",
                playerId: "player-1",
                playerName: "Tom Brady",
                playerPosition: "QB",
                totalVotes: 45,
                votesRequired: 50,
                hasUserVoted: false,
                priority: 5,
                tags: ["urgent", "quarterback", "trade"],
                createdBy: "GM Mike",
                choices: [
                  {
                    id: "choice-1",
                    title: "Trade for Draft Picks",
                    description: "Trade QB for 2 first-round picks",
                    voteCount: 28,
                    votePercentage: 62.2,
                    isSelected: false,
                    impactDescription: "Rebuild for future seasons"
                  },
                  {
                    id: "choice-2",
                    title: "Keep Current QB",
                    description: "Maintain current roster",
                    voteCount: 17,
                    votePercentage: 37.8,
                    isSelected: false,
                    impactDescription: "Compete this season"
                  }
                ]
              },
              {
                id: "option-2",
                title: "Draft Strategy Focus",
                description: "What position should we prioritize in the upcoming draft?",
                category: "draft",
                status: "active",
                createdAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
                expiresAt: new Date(Date.now() + 10 * 24 * 60 * 60 * 1000).toISOString(),
                organizationId: "org-2",
                organizationName: "Basketball Dynasty",
                totalVotes: 32,
                votesRequired: 40,
                hasUserVoted: true,
                userVoteChoice: "Center",
                priority: 3,
                tags: ["draft", "strategy"],
                createdBy: "Coach Sarah",
                choices: [
                  {
                    id: "choice-3",
                    title: "Center",
                    description: "Focus on big man presence",
                    voteCount: 18,
                    votePercentage: 56.25,
                    isSelected: true,
                    impactDescription: "Strengthen interior defense"
                  },
                  {
                    id: "choice-4",
                    title: "Point Guard",
                    description: "Get a floor general",
                    voteCount: 14,
                    votePercentage: 43.75,
                    isSelected: false,
                    impactDescription: "Improve ball movement"
                  }
                ]
              }
            ];
            return of(mockOptions).pipe(delay(500));
          })
        )
        .subscribe((options) => {
          patchState(store, {
            playerOptions: options,
            isLoading: false,
            error: null,
            lastUpdated: new Date().toISOString(),
          });
        });
    },

    // Load a specific player option
    loadPlayerOption(playerOptionId: string, userId?: string): void {
      patchState(store, { isLoading: true, error: null });

      const url = userId 
        ? `${API_BASE_URL}/PlayerOption/${playerOptionId}?userId=${userId}`
        : `${API_BASE_URL}/PlayerOption/${playerOptionId}`;

      http.get<{success: boolean, data: PlayerOption, message: string}>(url)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock data
            const mockOption: PlayerOption = {
              id: playerOptionId,
              title: "Trade Quarterback Decision",
              description: "Should we trade our starting quarterback for draft picks?",
              category: "trade",
              status: "active",
              createdAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
              expiresAt: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(),
              organizationId: "org-1",
              organizationName: "Fantasy Football League",
              playerId: "player-1",
              playerName: "Tom Brady",
              playerPosition: "QB",
              totalVotes: 45,
              votesRequired: 50,
              hasUserVoted: false,
              priority: 5,
              tags: ["urgent", "quarterback", "trade"],
              createdBy: "GM Mike",
              choices: [
                {
                  id: "choice-1",
                  title: "Trade for Draft Picks",
                  description: "Trade QB for 2 first-round picks",
                  voteCount: 28,
                  votePercentage: 62.2,
                  isSelected: false,
                  impactDescription: "Rebuild for future seasons"
                },
                {
                  id: "choice-2",
                  title: "Keep Current QB",
                  description: "Maintain current roster",
                  voteCount: 17,
                  votePercentage: 37.8,
                  isSelected: false,
                  impactDescription: "Compete this season"
                }
              ]
            };
            return of(mockOption).pipe(delay(500));
          })
        )
        .subscribe((option) => {
          patchState(store, {
            selectedPlayerOption: option,
            isLoading: false,
            error: null,
          });
        });
    },

    // Vote on a player option
    voteOnOption(voteRequest: VoteRequest): void {
      patchState(store, { isVoting: true, error: null });

      http.post<{success: boolean, data: VoteResponse, message: string}>(`${API_BASE_URL}/PlayerOption/vote`, voteRequest)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock response:', error);
            // Mock vote response
            const mockResponse: VoteResponse = {
              success: true,
              message: "Vote cast successfully!",
              remainingVotes: 249
            };
            return of(mockResponse).pipe(delay(1000));
          })
        )
        .subscribe((response) => {
          patchState(store, {
            isVoting: false,
            error: null,
          });
          
          // Reload options to reflect the vote
          this.loadPlayerOptions(voteRequest.userId);
        });
    },

    // Load player option statistics
    loadStats(userId?: string, organizationId?: string): void {
      const queryParams = new URLSearchParams();
      if (userId) queryParams.append('userId', userId);
      if (organizationId) queryParams.append('organizationId', organizationId);

      http.get<{success: boolean, data: PlayerOptionStats, message: string}>(`${API_BASE_URL}/PlayerOption/stats?${queryParams}`)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock stats
            const mockStats: PlayerOptionStats = {
              totalOptions: 25,
              activeOptions: 8,
              completedOptions: 15,
              expiredOptions: 2,
              userParticipatedOptions: 12,
              userVotesUsed: 18,
              userVotesRemaining: 232,
              categoryStats: [
                { category: "trade", count: 8, userParticipated: 5 },
                { category: "draft", count: 6, userParticipated: 4 },
                { category: "lineup", count: 7, userParticipated: 2 },
                { category: "strategy", count: 4, userParticipated: 1 }
              ]
            };
            return of(mockStats).pipe(delay(300));
          })
        )
        .subscribe((stats) => {
          patchState(store, {
            stats,
            error: null,
          });
        });
    },

    // Update filters
    updateFilters(filters: Partial<PlayerOptionFilters>): void {
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
