import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { UserStore } from '@sports-ui/data-access';
import { User, ServiceResponse } from '@sports-ui/api-types';
import { ApiService } from '@sports-ui/http-client';

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
  user: User;
  token: string;
  refreshToken?: string;
  expiresIn: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiService = inject(ApiService);
  private readonly userStore = inject(UserStore);
  private readonly router = inject(Router);

  // Local state
  private readonly isAuthenticating = signal<boolean>(false);
  private readonly authError = signal<string | null>(null);

  // Expose read-only signals
  readonly authenticating = this.isAuthenticating.asReadonly();
  readonly error = this.authError.asReadonly();

  constructor() {
    // Initialize authentication state on service creation
    this.initializeAuth();
  }

  /**
   * Initialize authentication state from stored token
   */
  private initializeAuth(): void {
    const token = this.getStoredToken();
    if (token && this.isTokenValid(token)) {
      // Load current user if token exists and is valid
      this.userStore.loadCurrentUser();
    } else {
      // Clear invalid token
      this.clearStoredToken();
    }
  }

  /**
   * Login with email and password
   */
  login(credentials: LoginRequest): Observable<AuthResponse> {
    this.isAuthenticating.set(true);
    this.authError.set(null);

    return this.apiService.post<ServiceResponse<AuthResponse>, LoginRequest>('api/auth/login', credentials)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          } else {
            throw new Error(response.message || 'Login failed');
          }
        }),
        tap(authResponse => {
          this.handleAuthSuccess(authResponse, credentials.rememberMe);
        }),
        catchError(error => {
          this.authError.set(error.message || 'Login failed');
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

    return this.apiService.post<ServiceResponse<AuthResponse>, RegisterRequest>('api/auth/register', userData)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          } else {
            throw new Error(response.message || 'Registration failed');
          }
        }),
        tap(authResponse => {
          this.handleAuthSuccess(authResponse, false);
        }),
        catchError(error => {
          this.authError.set(error.message || 'Registration failed');
          this.isAuthenticating.set(false);
          return throwError(() => error);
        })
      );
  }

  /**
   * Logout user
   */
  logout(): Observable<boolean> {
    return this.apiService.post<ServiceResponse<boolean>, {}>('api/auth/logout', {})
      .pipe(
        tap(() => {
          this.handleLogout();
        }),
        map(response => response.success),
        catchError(() => {
          // Even if logout API fails, clear local session
          this.handleLogout();
          return of(true);
        })
      );
  }

  /**
   * Refresh authentication token
   */
  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getStoredRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.apiService.post<ServiceResponse<AuthResponse>, { refreshToken: string }>('api/auth/refresh', { refreshToken })
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          } else {
            throw new Error(response.message || 'Token refresh failed');
          }
        }),
        tap(authResponse => {
          this.handleAuthSuccess(authResponse, true);
        }),
        catchError(error => {
          this.handleLogout();
          return throwError(() => error);
        })
      );
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    const token = this.getStoredToken();
    return token !== null && this.isTokenValid(token);
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
  private handleAuthSuccess(authResponse: AuthResponse, rememberMe: boolean = false): void {
    // Store tokens
    this.storeToken(authResponse.token, rememberMe);
    if (authResponse.refreshToken) {
      this.storeRefreshToken(authResponse.refreshToken, rememberMe);
    }

    // Update user store
    this.userStore.setCurrentUser(authResponse.user);

    // Clear loading state
    this.isAuthenticating.set(false);

    // Navigate to dashboard or return URL
    const returnUrl = this.getReturnUrl();
    this.router.navigate([returnUrl || '/']);
  }

  /**
   * Handle logout
   */
  private handleLogout(): void {
    this.clearStoredToken();
    this.clearStoredRefreshToken();
    this.userStore.setCurrentUser(null);
    this.router.navigate(['/auth/login']);
  }

  /**
   * Token storage methods
   */
  private storeToken(token: string, persistent: boolean = false): void {
    if (typeof window !== 'undefined') {
      const storage = persistent ? localStorage : sessionStorage;
      storage.setItem('auth_token', token);
    }
  }

  private getStoredToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('auth_token') || sessionStorage.getItem('auth_token');
    }
    return null;
  }

  private clearStoredToken(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('auth_token');
      sessionStorage.removeItem('auth_token');
    }
  }

  private storeRefreshToken(token: string, persistent: boolean = false): void {
    if (typeof window !== 'undefined') {
      const storage = persistent ? localStorage : sessionStorage;
      storage.setItem('refresh_token', token);
    }
  }

  private getStoredRefreshToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('refresh_token') || sessionStorage.getItem('refresh_token');
    }
    return null;
  }

  private clearStoredRefreshToken(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('refresh_token');
      sessionStorage.removeItem('refresh_token');
    }
  }

  /**
   * Token validation
   */
  private isTokenValid(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp > currentTime;
    } catch {
      return false;
    }
  }

  /**
   * Get return URL from query params
   */
  private getReturnUrl(): string | null {
    if (typeof window !== 'undefined') {
      const urlParams = new URLSearchParams(window.location.search);
      return urlParams.get('returnUrl');
    }
    return null;
  }
}
