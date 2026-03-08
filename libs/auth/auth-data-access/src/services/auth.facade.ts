// libs/auth/store/src/lib/auth.facade.ts
import { Injectable, inject } from "@angular/core";
import { AuthApi } from "./auth.api";
import { AuthStore } from "../auth.store";
import { firstValueFrom } from "rxjs";
import { Router } from "@angular/router";

@Injectable({ providedIn: "root" })
export class AuthFacade {
  private store = inject(AuthStore);
  private api = inject(AuthApi);
  readonly router = inject(Router);
  readonly user = this.store.user;
  readonly loggedIn = this.store.loggedIn;
  readonly authenticating = this.store.authenticating;
  readonly error = this.store.error;

  hydrate() {
    this.store.hydrate();
  }

  logout() {
    this.store.logout();
    this.router.navigate(["/login"]);
  }

  handleOAuthCallback(params: {
    accessToken: string;
    refreshToken: string | null;
    expiresAt: string;
    userId: string;
    email: string;
    firstName?: string;
    lastName?: string;
    picture?: string;
    roles: string[];
  }) {
    this.store.loginSuccess(
      {
        id: params.userId,
        email: params.email,
        firstName: params.firstName,
        lastName: params.lastName,
        profilePictureUrl: params.picture,
        isGoogleUser: true,
        roles: params.roles,
      },
      {
        accessToken: params.accessToken,
        refreshToken: params.refreshToken,
        expiresAt: params.expiresAt,
      }
    );
  }

  async login(email: string, password: string) {
    this.store.setAuthenticating(true);
    this.store.setError(null);
    try {
      const res = await firstValueFrom(this.api.login({ email, password }));
      if (!res.success) throw new Error(res.message ?? "Login failed");
      this.store.loginSuccess(res.user!, {
        accessToken: res.accessToken,
        refreshToken: res.refreshToken ?? null,
        expiresAt: res.expiresAt,
      });
    } catch (e: any) {
      this.store.setError(e?.message ?? "Login failed");
      throw e;
    } finally {
      this.store.setAuthenticating(false);
    }
  }

  async register(email: string, password: string, confirmPassword: string, firstName?: string, lastName?: string) {
    this.store.setAuthenticating(true);
    this.store.setError(null);
    try {
      const res = await firstValueFrom(
        this.api.register({ email, password, confirmPassword, firstName, lastName })
      );
      if (!res.success) throw new Error(res.message ?? "Registration failed");
      this.store.loginSuccess(res.user!, {
        accessToken: res.accessToken,
        refreshToken: res.refreshToken ?? null,
        expiresAt: res.expiresAt,
      });
    } catch (e: any) {
      this.store.setError(e?.message ?? "Registration failed");
      throw e;
    } finally {
      this.store.setAuthenticating(false);
    }
  }

  async signInWithGoogle(googleToken: string) {
    this.store.setAuthenticating(true);
    this.store.setError(null);

    try {
      const res = await firstValueFrom(this.api.loginWithGoogle(googleToken));
      const { user, accessToken, refreshToken, expiresAt } = res;

      this.store.loginSuccess(user, {
        accessToken,
        refreshToken: refreshToken ?? null,
        expiresAt,
      });
    } catch (e: any) {
      this.store.setError(e?.message ?? "Google login failed");
      throw e;
    } finally {
      this.store.setAuthenticating(false);
    }
  }
}
