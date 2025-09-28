import { Injectable, inject } from "@angular/core";
import { AuthApi } from "./auth.api";
import { AuthStore } from "../auth.store";
import { map, tap, catchError, throwError } from "rxjs";
import { AuthenticationResponse, ServiceResponse } from "@sports-ui/api-types";
import { SessionUser, AuthTokens } from "../auth.model";

@Injectable({ providedIn: "root" })
export class AuthFacade {
  private readonly api = inject(AuthApi);
  private readonly store = inject(AuthStore);

  // expose signals to dumb components
  readonly loggedIn = this.store.loggedIn;
  readonly authenticating = this.store.authenticating;
  readonly error = this.store.error;
  readonly user = this.store.user;
  readonly bearer = this.store.bearer;

  setRemember(remember: boolean) {
    this.store.setRemember(remember);
  }

  login(email: string, password: string) {
    this.store.setAuthenticating(true);
    return this.api.login({ email, password }).pipe(
      map(this.unwrap),
      tap((resp) => this.applyAuthResponse(resp)),
      catchError((err) => {
        this.store.setAuthenticating(false);
        this.store.setError(this.msg(err));
        return throwError(() => err);
      })
    );
  }

  loginGoogle(idToken: string) {
    this.store.setAuthenticating(true);
    return this.api.loginWithGoogle(idToken).pipe(
      map(this.unwrap),
      tap((resp) => this.applyAuthResponse(resp)),
      catchError((err) => {
        this.store.setAuthenticating(false);
        this.store.setError(this.msg(err));
        return throwError(() => err);
      })
    );
  }

  register(payload: {
    email: string;
    password: string;
    firstName?: string;
    lastName?: string;
  }) {
    this.store.setAuthenticating(true);
    return this.api.register(payload).pipe(
      map(this.unwrap),
      tap((resp) => this.applyAuthResponse(resp)),
      catchError((err) => {
        this.store.setAuthenticating(false);
        this.store.setError(this.msg(err));
        return throwError(() => err);
      })
    );
  }

  refresh() {
    const token = this.store.tokens()?.refreshToken;
    if (!token) return throwError(() => new Error("No refresh token"));
    return this.api.refresh(token).pipe(
      map(this.unwrap),
      tap((resp) => {
        if (resp.accessToken && resp.refreshToken && resp.expiresAt) {
          this.store.updateTokens({
            accessToken: resp.accessToken,
            refreshToken: resp.refreshToken,
            expiresAt: resp.expiresAt,
          });
        }
      })
    );
  }

  async logout() {
    try {
      await this.api.logout().toPromise();
    } catch {
      /* no-op */
    }
    this.store.logout();
  }

  // ----- helpers -----
  private unwrap = (res: ServiceResponse<AuthenticationResponse>) => {
    if (!res.success || !res.data)
      throw new Error(res.message || "Authentication failed");
    return res.data;
  };

  private applyAuthResponse = (data: AuthenticationResponse) => {
    const tokens: AuthTokens | null =
      data.accessToken && data.refreshToken && data.expiresAt
        ? {
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
            expiresAt: data.expiresAt,
          }
        : null;

    const user: SessionUser | null = data.user
      ? {
          id: data.user.id,
          email: data.user.email,
          firstName: data.user.firstName,
          lastName: data.user.lastName,
          fullName: data.user.fullName,
          profilePictureUrl: data.user.profilePictureUrl,
          roles: data.user.roles ?? [],
        }
      : null;

    if (tokens && user) this.store.loginSuccess(user, tokens);
    else this.store.setError("Malformed auth response");
  };

  private msg(err: any) {
    return err?.error?.message || err?.message || "Request failed";
  }
}
