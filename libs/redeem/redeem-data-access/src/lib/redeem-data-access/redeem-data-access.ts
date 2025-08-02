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
  type: "votes" | "premium" | "bonus" | "special";
  value: number;
  description: string;
  expiresAt?: string;
  isActive: boolean;
  createdAt: string;
}

export interface RedemptionHistory {
  id: string;
  code: string;
  type: "votes" | "premium" | "bonus" | "special";
  value: number;
  description: string;
  redeemedAt: string;
  status: "success" | "failed" | "expired";
}

export interface RedeemRequest {
  code: string;
}

export interface RedeemResponse {
  success: boolean;
  message: string;
  reward?: {
    type: "votes" | "premium" | "bonus" | "special";
    value: number;
    description: string;
  };
  newBalance?: {
    votes: number;
    premium: boolean;
  };
}

export interface UserBalance {
  votes: number;
  premiumUntil?: string;
  bonusMultiplier: number;
  specialRewards: string[];
}

// Initial state
interface RedeemState {
  availableCodes: RedeemCode[];
  redemptionHistory: RedemptionHistory[];
  userBalance: UserBalance | null;
  isLoading: boolean;
  isRedeeming: boolean;
  error: string | null;
  lastRedemption: RedemptionHistory | null;
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
    recentRedemptions: computed(() => store.redemptionHistory().slice(0, 5)),
    successfulRedemptions: computed(() =>
      store.redemptionHistory().filter((r) => r.status === "success")
    ),
    totalRedeemed: computed(() =>
      store
        .redemptionHistory()
        .filter((r) => r.status === "success" && r.type === "votes")
        .reduce((total, r) => total + r.value, 0)
    ),
  })),
  withMethods((store, http = inject(HttpClient)) => ({
    // Load user balance and available codes
    loadData(): void {
      patchState(store, { isLoading: true, error: null });

      // Mock data - replace with actual API calls
      const mockBalance: UserBalance = {
        votes: 250,
        premiumUntil: "2024-12-31T23:59:59Z",
        bonusMultiplier: 1.2,
        specialRewards: ["early_access", "exclusive_content"],
      };

      const mockCodes: RedeemCode[] = [
        {
          id: "1",
          code: "WELCOME100",
          type: "votes",
          value: 100,
          description: "Welcome bonus - 100 free votes",
          expiresAt: "2024-12-31T23:59:59Z",
          isActive: true,
          createdAt: "2024-01-01T00:00:00Z",
        },
        {
          id: "2",
          code: "PREMIUM30",
          type: "premium",
          value: 30,
          description: "30 days premium access",
          expiresAt: "2024-06-30T23:59:59Z",
          isActive: true,
          createdAt: "2024-01-01T00:00:00Z",
        },
      ];

      const mockHistory: RedemptionHistory[] = [
        {
          id: "1",
          code: "STARTER50",
          type: "votes",
          value: 50,
          description: "Starter pack - 50 votes",
          redeemedAt: "2024-01-10T10:00:00Z",
          status: "success",
        },
        {
          id: "2",
          code: "BONUS25",
          type: "votes",
          value: 25,
          description: "Daily bonus - 25 votes",
          redeemedAt: "2024-01-09T15:30:00Z",
          status: "success",
        },
      ];

      of({ balance: mockBalance, codes: mockCodes, history: mockHistory })
        .pipe(
          delay(1000),
          catchError((error) => {
            patchState(store, {
              isLoading: false,
              error: "Failed to load data. Please try again.",
            });
            return throwError(() => error);
          })
        )
        .subscribe(({ balance, codes, history }) => {
          patchState(store, {
            userBalance: balance,
            availableCodes: codes,
            redemptionHistory: history,
            isLoading: false,
            error: null,
          });
        });
    },

    // Redeem a code
    redeemCode(request: RedeemRequest): void {
      patchState(store, { isRedeeming: true, error: null });

      // Mock redemption logic - replace with actual API
      const code = request.code.toUpperCase();
      const availableCodes = store.availableCodes();
      const foundCode = availableCodes.find(
        (c) => c.code === code && c.isActive
      );

      if (!foundCode) {
        patchState(store, {
          isRedeeming: false,
          error: "Invalid or expired code. Please check and try again.",
        });
        return;
      }

      // Check if already redeemed
      const history = store.redemptionHistory();
      const alreadyRedeemed = history.some(
        (h) => h.code === code && h.status === "success"
      );

      if (alreadyRedeemed) {
        patchState(store, {
          isRedeeming: false,
          error: "This code has already been redeemed.",
        });
        return;
      }

      // Simulate API call
      const mockResponse: RedeemResponse = {
        success: true,
        message: `Successfully redeemed ${foundCode.value} ${foundCode.type}!`,
        reward: {
          type: foundCode.type,
          value: foundCode.value,
          description: foundCode.description,
        },
        newBalance: {
          votes:
            (store.userBalance()?.votes || 0) +
            (foundCode.type === "votes" ? foundCode.value : 0),
          premium:
            foundCode.type === "premium" || store.userBalance()?.premiumUntil
              ? true
              : false,
        },
      };

      of(mockResponse)
        .pipe(
          delay(1500),
          catchError((error) => {
            patchState(store, {
              isRedeeming: false,
              error: "Failed to redeem code. Please try again.",
            });
            return throwError(() => error);
          })
        )
        .subscribe((response) => {
          if (response.success) {
            // Create new redemption history entry
            const newRedemption: RedemptionHistory = {
              id: Date.now().toString(),
              code: foundCode.code,
              type: foundCode.type,
              value: foundCode.value,
              description: foundCode.description,
              redeemedAt: new Date().toISOString(),
              status: "success",
            };

            // Update balance
            const currentBalance = store.userBalance();
            let updatedBalance = { ...currentBalance } as UserBalance;

            if (foundCode.type === "votes") {
              updatedBalance.votes =
                (currentBalance?.votes || 0) + foundCode.value;
            } else if (foundCode.type === "premium") {
              const currentDate = new Date();
              const premiumUntil = new Date(
                currentDate.getTime() + foundCode.value * 24 * 60 * 60 * 1000
              );
              updatedBalance.premiumUntil = premiumUntil.toISOString();
            }

            patchState(store, {
              userBalance: updatedBalance,
              redemptionHistory: [newRedemption, ...store.redemptionHistory()],
              lastRedemption: newRedemption,
              isRedeeming: false,
              error: null,
            });
          } else {
            patchState(store, {
              isRedeeming: false,
              error: response.message || "Failed to redeem code.",
            });
          }
        });
    },

    // Clear error
    clearError(): void {
      patchState(store, { error: null });
    },

    // Clear last redemption
    clearLastRedemption(): void {
      patchState(store, { lastRedemption: null });
    },

    // Reset store
    reset(): void {
      patchState(store, initialState);
    },
  }))
);
