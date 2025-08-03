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
export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  username?: string;
  phone?: string;
  dateOfBirth?: string;
  bio?: string;
  avatar?: string;
  preferences: UserPreferences;
  stats: UserStats;
  createdAt: string;
  updatedAt: string;
}

export interface UserPreferences {
  emailNotifications: boolean;
  pushNotifications: boolean;
  theme: "light" | "dark" | "auto";
  language: string;
  timezone: string;
  privacy: {
    profileVisibility: "public" | "private" | "friends";
    showEmail: boolean;
    showPhone: boolean;
  };
}

export interface UserStats {
  totalVotes: number;
  votesUsed: number;
  votesRemaining: number;
  optionsParticipated: number;
  organizationsJoined: number;
  accountLevel: "bronze" | "silver" | "gold" | "platinum";
  joinDate: string;
}

export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  username?: string;
  phone?: string;
  dateOfBirth?: string;
  bio?: string;
  avatar?: string;
}

export interface UpdatePreferencesRequest {
  emailNotifications?: boolean;
  pushNotifications?: boolean;
  theme?: "light" | "dark" | "auto";
  language?: string;
  timezone?: string;
  privacy?: {
    profileVisibility?: "public" | "private" | "friends";
    showEmail?: boolean;
    showPhone?: boolean;
  };
}

// Initial state
interface ProfileState {
  profile: UserProfile | null;
  isLoading: boolean;
  isUpdating: boolean;
  error: string | null;
  lastUpdated: string | null;
}

const initialState: ProfileState = {
  profile: null,
  isLoading: false,
  isUpdating: false,
  error: null,
  lastUpdated: null,
};

// Profile Store
export const ProfileStore = signalStore(
  { providedIn: "root" },
  withState(initialState),
  withComputed((store) => ({
    hasProfile: computed(() => !!store.profile()),
    fullName: computed(() => {
      const profile = store.profile();
      return profile ? `${profile.firstName} ${profile.lastName}` : "";
    }),
    votesPercentageUsed: computed(() => {
      const profile = store.profile();
      if (!profile?.stats.totalVotes) return 0;
      return Math.round(
        (profile.stats.votesUsed / profile.stats.totalVotes) * 100
      );
    }),
    accountLevelColor: computed(() => {
      const profile = store.profile();
      switch (profile?.stats.accountLevel) {
        case "bronze":
          return "#CD7F32";
        case "silver":
          return "#C0C0C0";
        case "gold":
          return "#FFD700";
        case "platinum":
          return "#E5E4E2";
        default:
          return "#CD7F32";
      }
    }),
  })),
  withMethods((store, http = inject(HttpClient)) => {
    const API_BASE_URL = "http://localhost:5000/api"; // Update this to match your API URL

    return {
      // Load user profile
      loadProfile(userId: string = "user-123"): void {
        patchState(store, { isLoading: true, error: null });

        // Try real API call first, fallback to mock data
        const mockProfile: UserProfile = {
          id: "user-123",
          email: "john.doe@example.com",
          firstName: "John",
          lastName: "Doe",
          username: "johndoe",
          phone: "+1 (555) 123-4567",
          dateOfBirth: "1990-05-15",
          bio: "Sports enthusiast and team player. Love making strategic decisions for my favorite teams!",
          avatar: "/assets/default-avatar.png",
          preferences: {
            emailNotifications: true,
            pushNotifications: false,
            theme: "auto",
            language: "en",
            timezone: "America/New_York",
            privacy: {
              profileVisibility: "public",
              showEmail: false,
              showPhone: false,
            },
          },
          stats: {
            totalVotes: 1000,
            votesUsed: 750,
            votesRemaining: 250,
            optionsParticipated: 45,
            organizationsJoined: 3,
            accountLevel: "gold",
            joinDate: "2023-01-15",
          },
          createdAt: "2023-01-15T10:00:00Z",
          updatedAt: "2024-01-15T14:30:00Z",
        };

        // Try real API call first
        http
          .get<{ success: boolean; data: UserProfile; message: string }>(
            `${API_BASE_URL}/User/profile/${userId}`
          )
          .pipe(
            map((response) => response.data),
            catchError((error) => {
              console.warn("API call failed, using mock data:", error);
              // Fallback to mock data for development
              return of(mockProfile).pipe(delay(500));
            })
          )
          .subscribe((profile) => {
            patchState(store, {
              profile,
              isLoading: false,
              error: null,
              lastUpdated: new Date().toISOString(),
            });
          });
      },

      // Update profile information
      updateProfile(updates: UpdateProfileRequest): void {
        const currentProfile = store.profile();
        if (!currentProfile) return;

        patchState(store, { isUpdating: true, error: null });

        // Mock API call - replace with actual API
        of({
          ...currentProfile,
          ...updates,
          updatedAt: new Date().toISOString(),
        })
          .pipe(
            delay(800),
            catchError((error) => {
              patchState(store, {
                isUpdating: false,
                error: "Failed to update profile. Please try again.",
              });
              return throwError(() => error);
            })
          )
          .subscribe((updatedProfile) => {
            patchState(store, {
              profile: updatedProfile,
              isUpdating: false,
              error: null,
              lastUpdated: new Date().toISOString(),
            });
          });
      },

      // Update user preferences
      updatePreferences(updates: UpdatePreferencesRequest): void {
        const currentProfile = store.profile();
        if (!currentProfile) return;

        patchState(store, { isUpdating: true, error: null });

        const updatedPreferences = {
          ...currentProfile.preferences,
          ...updates,
          privacy: {
            ...currentProfile.preferences.privacy,
            ...(updates.privacy || {}),
          },
        };
        const updatedProfile = {
          ...currentProfile,
          preferences: updatedPreferences,
          updatedAt: new Date().toISOString(),
        };

        // Mock API call - replace with actual API
        of(updatedProfile)
          .pipe(
            delay(500),
            catchError((error) => {
              patchState(store, {
                isUpdating: false,
                error: "Failed to update preferences. Please try again.",
              });
              return throwError(() => error);
            })
          )
          .subscribe((profile) => {
            patchState(store, {
              profile,
              isUpdating: false,
              error: null,
              lastUpdated: new Date().toISOString(),
            });
          });
      },

      // Upload avatar
      uploadAvatar(file: File): void {
        const currentProfile = store.profile();
        if (!currentProfile) return;

        patchState(store, { isUpdating: true, error: null });

        // Mock file upload - replace with actual API
        const mockAvatarUrl = URL.createObjectURL(file);
        const updatedProfile = {
          ...currentProfile,
          avatar: mockAvatarUrl,
          updatedAt: new Date().toISOString(),
        };

        of(updatedProfile)
          .pipe(
            delay(1500),
            catchError((error) => {
              patchState(store, {
                isUpdating: false,
                error: "Failed to upload avatar. Please try again.",
              });
              return throwError(() => error);
            })
          )
          .subscribe((profile) => {
            patchState(store, {
              profile,
              isUpdating: false,
              error: null,
              lastUpdated: new Date().toISOString(),
            });
          });
      },

      // Clear error
      clearError(): void {
        patchState(store, { error: null });
      },

      // Reset store
      reset(): void {
        patchState(store, initialState);
      },
    };
  })
);
