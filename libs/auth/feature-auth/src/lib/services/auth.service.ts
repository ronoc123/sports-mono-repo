import { Injectable, signal, computed, inject } from "@angular/core";
import { Router } from "@angular/router";
import { BehaviorSubject, Observable, of, throwError } from "rxjs";
import { delay, tap, catchError } from "rxjs/operators";

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

  // Observable for guards and other reactive needs
  readonly isAuthenticated$ = new BehaviorSubject<boolean>(false);

  constructor() {
    // Check for existing session on service initialization
    this.checkExistingSession();

    // Update the observable with initial value
    this.isAuthenticated$.next(this.isAuthenticated());
  }

  /**
   * Login with email and password
   */
  login(credentials: LoginCredentials): Observable<LoginResponse> {
    this._isLoading.set(true);
    this._error.set(null);

    // Mock login - replace with actual API call
    return this.mockLogin(credentials).pipe(
      tap((response) => {
        this.setAuthenticatedUser(response.user, response.token);
        this._isLoading.set(false);
      }),
      catchError((error) => {
        this._error.set(error.message || "Login failed");
        this._isLoading.set(false);
        return throwError(() => error);
      })
    );
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

  /**
   * Refresh authentication token
   */
  refreshToken(): Observable<LoginResponse> {
    const refreshToken = localStorage.getItem("refreshToken");
    if (!refreshToken) {
      return throwError(() => new Error("No refresh token available"));
    }

    // Mock refresh - replace with actual API call
    return this.mockRefreshToken(refreshToken).pipe(
      tap((response) => {
        this.setAuthenticatedUser(response.user, response.token);
      }),
      catchError((error) => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  private checkExistingSession(): void {
    const token = localStorage.getItem("authToken");
    const userJson = localStorage.getItem("currentUser");

    if (token && userJson) {
      try {
        const user = JSON.parse(userJson);
        this._currentUser.set(user);
      } catch (error) {
        console.error("Error parsing stored user data:", error);
        this.clearAuthData();
      }
    }
  }

  private setAuthenticatedUser(
    user: User,
    token: string,
    refreshToken?: string
  ): void {
    this._currentUser.set(user);
    localStorage.setItem("authToken", token);
    localStorage.setItem("currentUser", JSON.stringify(user));

    if (refreshToken) {
      localStorage.setItem("refreshToken", refreshToken);
    }

    // Update the observable
    this.isAuthenticated$.next(this.isAuthenticated());
  }

  private clearAuthData(): void {
    localStorage.removeItem("authToken");
    localStorage.removeItem("currentUser");
    localStorage.removeItem("refreshToken");
  }

  // Mock implementations - replace with actual API calls
  private mockLogin(credentials: LoginCredentials): Observable<LoginResponse> {
    // Simulate API delay
    return of({
      user: {
        id: "1",
        email: credentials.email,
        firstName: "John",
        lastName: "Doe",
        userName: "johndoe",
        role: "admin",
        permissions: ["VIEW_USERS", "EDIT_USERS", "VIEW_ORGANIZATIONS"],
      },
      token: "mock-jwt-token-" + Date.now(),
      refreshToken: "mock-refresh-token-" + Date.now(),
    }).pipe(
      delay(1000), // Simulate network delay
      tap(() => console.log("Mock login successful"))
    );
  }

  private mockRefreshToken(refreshToken: string): Observable<LoginResponse> {
    return of({
      user: this._currentUser()!,
      token: "mock-jwt-token-refreshed-" + Date.now(),
      refreshToken: "mock-refresh-token-refreshed-" + Date.now(),
    }).pipe(delay(500));
  }
}
