import { inject, Injectable } from "@angular/core";
import { AuthApi } from "./auth.api";
import { AuthStore } from "../auth.store";
import { firstValueFrom } from "rxjs";

@Injectable({ providedIn: "root" })
export class AuthService {
  // acts as Facade
  private store = inject(AuthStore);
  private api = inject(AuthApi);

  // expose store signals
  readonly user = this.store.user;
  readonly authenticating = this.store.authenticating;
  readonly error = this.store.error;
  readonly loggedIn = this.store.loggedIn;

  hydrate() {
    this.store.hydrate();
  }
  logout() {
    this.store.logout();
  }

  async loginWithGoogle(googleToken: string) {
    this.store.setAuthenticating(true);
    this.store.setError(null);

    try {
      const res = await firstValueFrom(this.api.loginWithGoogle(googleToken));

      const { user, accessToken, refreshToken, expiresAt } = res;

      // runtime guard (optional but helpful)
      if (!accessToken || !expiresAt) {
        throw new Error("Invalid auth payload");
      }

      this.store.loginSuccess(user, {
        accessToken, // string (narrowed)
        refreshToken: refreshToken ?? null,
        expiresAt, // string (narrowed)
      });
    } catch (e: any) {
      this.store.setError(e?.message ?? "Google login failed");
      throw e;
    } finally {
      this.store.setAuthenticating(false);
    }
  }
}
