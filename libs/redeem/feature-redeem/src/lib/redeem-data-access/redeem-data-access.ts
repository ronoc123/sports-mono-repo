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
export interface RedeemCode {
  id: string;
  code: string;
  type: string; // votes, premium, bonus, special
  value: number;
  description: string;
  expiresAt?: string;
  isActive: boolean;
  createdAt: string;
}

export interface RedemptionHistory {
  id: string;
  code: string;
  type: string;
  value: number;
  description: string;
  redeemedAt: string;
  status: string; // success, failed, expired
  userId: string;
}

export interface UserBalance {
  votes: number;
  premiumUntil?: string;
  bonusMultiplier: number;
  specialRewards: string[];
}

export interface RedeemRequest {
  code: string;
}

export interface RedeemResponse {
  success: boolean;
  message: string;
  reward?: {
    type: string;
    value: number;
    description: string;
  };
  newBalance?: UserBalance;
}

// State
interface RedeemState {
  availableCodes: RedeemCode[];
  redemptionHistory: RedemptionHistory[];
  userBalance: UserBalance | null;
  isLoading: boolean;
  isRedeeming: boolean;
  error: string | null;
  lastRedemption: RedeemResponse | null;
}

const initialState: RedeemState = {
  availableCodes: [],
  redemptionHistory: [],
  userBalance: null,
  isLoading: false,
  isRedeeming: false,
  error: null,
  lastRedemption: null,
};

// Redeem Store
export const RedeemStore = signalStore(
  { providedIn: "root" },
  withState(initialState),
  withComputed((store) => ({
    hasBalance: computed(() => !!store.userBalance()),
    totalVotes: computed(() => store.userBalance()?.votes || 0),
    isPremium: computed(() => {
      const balance = store.userBalance();
      if (!balance?.premiumUntil) return false;
      return new Date(balance.premiumUntil) > new Date();
    }),
    successfulRedemptions: computed(() => 
      store.redemptionHistory().filter(r => r.status === 'success').length
    ),
  })),
  withMethods((store, http = inject(HttpClient)) => {
    const API_BASE_URL = 'http://localhost:5000/api'; // Update this to match your API URL
    
    return {
    // Load available codes
    loadAvailableCodes(userId: string = "user-123"): void {
      patchState(store, { isLoading: true, error: null });

      http.get<{success: boolean, data: RedeemCode[], message: string}>(`${API_BASE_URL}/Code/available/${userId}`)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock data
            const mockCodes: RedeemCode[] = [
              {
                id: "code-1",
                code: "WELCOME100",
                type: "votes",
                value: 100,
                description: "Welcome bonus - 100 free votes",
                expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
                isActive: true,
                createdAt: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString()
              },
              {
                id: "code-2",
                code: "PREMIUM30",
                type: "premium",
                value: 30,
                description: "30 days premium access",
                expiresAt: new Date(Date.now() + 60 * 24 * 60 * 60 * 1000).toISOString(),
                isActive: true,
                createdAt: new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString()
              }
            ];
            return of(mockCodes).pipe(delay(500));
          })
        )
        .subscribe((codes) => {
          patchState(store, {
            availableCodes: codes,
            isLoading: false,
            error: null,
          });
        });
    },

    // Load user balance
    loadUserBalance(userId: string = "user-123"): void {
      http.get<{success: boolean, data: UserBalance, message: string}>(`${API_BASE_URL}/Code/balance/${userId}`)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock data
            const mockBalance: UserBalance = {
              votes: 250,
              premiumUntil: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
              bonusMultiplier: 1.2,
              specialRewards: ["early_access", "exclusive_content"]
            };
            return of(mockBalance).pipe(delay(300));
          })
        )
        .subscribe((balance) => {
          patchState(store, {
            userBalance: balance,
            error: null,
          });
        });
    },

    // Load redemption history
    loadRedemptionHistory(userId: string = "user-123"): void {
      http.get<{success: boolean, data: RedemptionHistory[], message: string}>(`${API_BASE_URL}/Code/history/${userId}`)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock data:', error);
            // Fallback to mock data
            const mockHistory: RedemptionHistory[] = [
              {
                id: "redemption-1",
                code: "STARTER50",
                type: "votes",
                value: 50,
                description: "Starter pack - 50 votes",
                redeemedAt: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
                status: "success",
                userId: userId
              },
              {
                id: "redemption-2",
                code: "BONUS25",
                type: "votes",
                value: 25,
                description: "Daily bonus - 25 votes",
                redeemedAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
                status: "success",
                userId: userId
              }
            ];
            return of(mockHistory).pipe(delay(300));
          })
        )
        .subscribe((history) => {
          patchState(store, {
            redemptionHistory: history,
            error: null,
          });
        });
    },

    // Redeem a code
    redeemCode(request: RedeemRequest): void {
      patchState(store, { isRedeeming: true, error: null, lastRedemption: null });

      http.post<{success: boolean, data: RedeemResponse, message: string}>(`${API_BASE_URL}/Code/redeem-by-code`, request)
        .pipe(
          map(response => response.data),
          catchError((error) => {
            console.warn('API call failed, using mock response:', error);
            // Mock redemption logic for development
            const mockResponse: RedeemResponse = {
              success: true,
              message: `Successfully redeemed code: ${request.code}!`,
              reward: {
                type: "votes",
                value: 50,
                description: "Bonus votes"
              },
              newBalance: {
                votes: (store.userBalance()?.votes || 0) + 50,
                premiumUntil: store.userBalance()?.premiumUntil,
                bonusMultiplier: store.userBalance()?.bonusMultiplier || 1.0,
                specialRewards: store.userBalance()?.specialRewards || []
              }
            };
            return of(mockResponse).pipe(delay(1000));
          })
        )
        .subscribe((response) => {
          patchState(store, {
            lastRedemption: response,
            userBalance: response.newBalance || store.userBalance(),
            isRedeeming: false,
            error: null,
          });
          
          // Reload history to show new redemption
          this.loadRedemptionHistory();
        });
    },

    // Reset store
    reset(): void {
      patchState(store, initialState);
    },
  };
  })
);
