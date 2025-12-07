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
