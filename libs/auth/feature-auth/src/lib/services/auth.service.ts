import { Injectable, signal, computed, inject } from "@angular/core";
import { Router } from "@angular/router";
import { BehaviorSubject, Observable } from "rxjs";
import { tap } from "rxjs/operators";

import { throwError } from "rxjs";
import { map, catchError, finalize } from "rxjs/operators";
import { ServiceResponse } from "@sports-ui/api-types";
import { ApiService } from "@sports-ui/http-client";

export interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  userName?: string;
  role?: string;
  permissions?: string[];
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface LoginResponse {
  user: User;
  token: string;
  refreshToken?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  email: string;
  userName: string;
  password: string;
  confirmPassword: string;
}
export interface AuthResponse {
  success: boolean;
  message?: string;
  accessToken: string;
  refreshToken?: string;
  expiresAt: string;
  user: User;
}

@Injectable({
  providedIn: "root",
})
export class AuthService {
  private readonly router = inject(Router);

  // Signals for reactive state management
  private readonly _currentUser = signal<User | null>(null);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public readonly signals
  readonly currentUser = this._currentUser.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly isAuthenticated = computed(() => !!this._currentUser());

  private readonly apiService = inject(ApiService);

  // Local state
  private readonly isAuthenticating = signal<boolean>(false);
  private readonly authError = signal<string | null>(null);

  // Expose read-only signals
  readonly authenticating = this.isAuthenticating.asReadonly();
  // Observable for guards and other reactive needs
  readonly isAuthenticated$ = new BehaviorSubject<boolean>(false);

  constructor() {
    // Initialize authentication state on service creation
    this.initializeAuth();
  }

  /**
   * Logout user
   */
  logout(): void {
    this._currentUser.set(null);
    this._error.set(null);
    this.clearAuthData();

    // Update the observable
    this.isAuthenticated$.next(this.isAuthenticated());

    this.router.navigate(["/login"]);
  }

  /**
   * Check if user has specific permission
   */
  hasPermission(permission: string): boolean {
    const user = this._currentUser();
    return user?.permissions?.includes(permission) ?? false;
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: string): boolean {
    const user = this._currentUser();
    return user?.role === role;
  }

  /**
   * Check if user has any of the specified roles
   */
  hasAnyRole(roles: string[]): boolean {
    const user = this._currentUser();
    return user?.role ? roles.includes(user.role) : false;
  }

  private clearAuthData(): void {
    localStorage.removeItem("authToken");
    localStorage.removeItem("currentUser");
    localStorage.removeItem("refreshToken");
  }

  /**
   * Initialize authentication state from stored token
   */
  private initializeAuth(): void {
    const token = this.getStoredToken();
    if (token && this.isTokenValid(token)) {
      // Load current user if token exists and is valid
    } else {
      // Clear invalid token
      this.clearStoredToken();
    }

    // Update the observable with initial value
    this.isAuthenticated$.next(this.isAuthenticated());
  }

  /** 🔐 NEW: Login with Google ID token */
  loginWithGoogle(
    googleToken: string,
    rememberMe = true
  ): Observable<AuthResponse> {
    this.isAuthenticating.set(true);
    this.authError.set(null);

    return (
      this.apiService
        // Route this to Identity: prefix with 'identity/'
        .post<AuthResponse, { googleToken: any }>(
          `:5001/api/auth/google`, // NOT "/auth/google"
          { googleToken } // NOT { googleToken }
        )
        .pipe(
          tap((res) => {
            console.log(res);
            this.handleAuthSuccess(res, rememberMe);
          }),
          map((res) => {
            if (res?.success && res) return res;
            throw new Error(res?.message || "Google login failed");
          }),
          catchError((err) => {
            this.authError.set(err?.message || "Google login failed");
            return throwError(() => err);
          }),
          finalize(() => this.isAuthenticating.set(false))
        )
    );
  }

  /**
   * Login with email and password
   */
  login(credentials: LoginRequest): Observable<AuthResponse> {
    this.isAuthenticating.set(true);
    this.authError.set(null);

    return this.apiService
      .post<ServiceResponse<AuthResponse>, LoginRequest>(
        "api/auth/login",
        credentials
      )
      .pipe(
        map((response) => {
          if (response.success && response.data) {
            return response.data;
          } else {
            throw new Error(response.message || "Login failed");
          }
        }),
        tap((authResponse) => {
          this.handleAuthSuccess(authResponse, credentials.rememberMe);
        }),
        catchError((error) => {
          this.authError.set(error.message || "Login failed");
          this.isAuthenticating.set(false);
          return throwError(() => error);
        })
      );
  }

  /**
   * Register new user
   */
  register(userData: RegisterRequest): Observable<AuthResponse> {
    this.isAuthenticating.set(true);
    this.authError.set(null);

    return this.apiService
      .post<ServiceResponse<AuthResponse>, RegisterRequest>(
        "api/auth/register",
        userData
      )
      .pipe(
        map((response) => {
          if (response.success && response.data) {
            return response.data;
          } else {
            throw new Error(response.message || "Registration failed");
          }
        }),
        tap((authResponse) => {
          this.handleAuthSuccess(authResponse, false);
        }),
        catchError((error) => {
          this.authError.set(error.message || "Registration failed");
          this.isAuthenticating.set(false);
          return throwError(() => error);
        })
      );
  }

  /**
   * Refresh authentication token
   */
  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getStoredRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error("No refresh token available"));
    }

    return this.apiService
      .post<ServiceResponse<AuthResponse>, { refreshToken: string }>(
        "api/auth/refresh",
        { refreshToken }
      )
      .pipe(
        map((response) => {
          if (response.success && response.data) {
            return response.data;
          } else {
            throw new Error(response.message || "Token refresh failed");
          }
        }),
        tap((authResponse) => {
          this.handleAuthSuccess(authResponse, true);
        }),
        catchError((error) => {
          this.handleLogout();
          return throwError(() => error);
        })
      );
  }

  /**
   * Get current auth token
   */
  getToken(): string | null {
    return this.getStoredToken();
  }

  /**
   * Clear authentication error
   */
  clearError(): void {
    this.authError.set(null);
  }

  /**
   * Handle successful authentication
   */
  private handleAuthSuccess(
    authResponse: AuthResponse,
    rememberMe = false
  ): void {
    // Store tokens
    this.storeToken(authResponse.accessToken, rememberMe);
    if (authResponse.refreshToken) {
      this.storeRefreshToken(authResponse.refreshToken, rememberMe);
    }

    // Update user store
    // this.userStore.setCurrentUser(null);

    // Clear loading state
    this.isAuthenticating.set(false);

    this.isAuthenticated$.next(true);

    this.router.navigate(["/"]);
  }

  /**
   * Handle logout
   */
  private handleLogout(): void {
    this.clearStoredToken();
    this.clearStoredRefreshToken();
    // this.userStore.setCurrentUser(null);
    this.router.navigate(["/auth/login"]);
  }

  /**
   * Token storage methods
   */
  private storeToken(token: string, persistent = false): void {
    if (typeof window !== "undefined") {
      const storage = persistent ? localStorage : sessionStorage;
      storage.setItem("auth_token", token);
    }
  }

  private getStoredToken(): string | null {
    if (typeof window !== "undefined") {
      return (
        localStorage.getItem("auth_token") ||
        sessionStorage.getItem("auth_token")
      );
    }
    return null;
  }

  private clearStoredToken(): void {
    if (typeof window !== "undefined") {
      localStorage.removeItem("auth_token");
      sessionStorage.removeItem("auth_token");
    }
  }

  private storeRefreshToken(token: string, persistent = false): void {
    if (typeof window !== "undefined") {
      const storage = persistent ? localStorage : sessionStorage;
      storage.setItem("refresh_token", token);
    }
  }

  private getStoredRefreshToken(): string | null {
    if (typeof window !== "undefined") {
      return (
        localStorage.getItem("refresh_token") ||
        sessionStorage.getItem("refresh_token")
      );
    }
    return null;
  }

  private clearStoredRefreshToken(): void {
    if (typeof window !== "undefined") {
      localStorage.removeItem("refresh_token");
      sessionStorage.removeItem("refresh_token");
    }
  }

  /**
   * Token validation
   */
  private isTokenValid(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp > currentTime;
    } catch {
      return false;
    }
  }
}
