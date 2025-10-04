// libs/auth/store/src/lib/auth.store.ts
import { computed } from "@angular/core";
import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withState,
} from "@ngrx/signals";
import { AuthState, initialAuthState, AuthTokens } from "./auth.model";

const STORAGE_KEY = "sports-auth";

/* ---------- persistence helpers ---------- */
function readStorage(remember: boolean): AuthState | null {
  const raw = (remember ? localStorage : sessionStorage).getItem(STORAGE_KEY);
  try {
    return raw ? (JSON.parse(raw) as AuthState) : null;
  } catch {
    return null;
  }
}
function clearStorage() {
  try {
    localStorage.removeItem(STORAGE_KEY);
    // eslint-disable-next-line no-empty
  } catch {}
  try {
    sessionStorage.removeItem(STORAGE_KEY);
    // eslint-disable-next-line no-empty
  } catch {}
}

/* ---------- the store ---------- */
export const AuthStore = signalStore(
  withState<AuthState>(initialAuthState),
  withComputed((state) => ({
    isExpired: computed(() => {
      const exp = state.tokens()?.expiresAt;
      return !exp || new Date(exp).getTime() <= Date.now();
    }),
    bearer: computed(() => state.tokens()?.accessToken ?? null),
    userEmail: computed(() => state.user()?.email ?? null),
    roles: computed(() => state.user()?.roles ?? []),
  })),

  // Commands
  withMethods((store) => ({
    /** Load previously saved session from local/session storage */
    hydrate() {
      const fromLocal = readStorage(true);
      const fromSession = readStorage(false);
      const loaded = fromLocal ?? fromSession;
      if (loaded) patchState(store, loaded);
    },

    /** Flip the "authenticating" spinner and optionally clear error */
    setAuthenticating(on: boolean) {
      patchState(store, {
        authenticating: on,
        error: on ? null : store.error(), // keep current error when turning off
      });
    },

    /** Set/clear error message */
    setError(message: string | null) {
      patchState(store, { error: message });
    },

    /** On successful login/refresh — write full session + persist */
    loginSuccess(user: any, tokens: AuthTokens) {
      const next: AuthState = {
        loggedIn: true,
        user,
        tokens,
        authenticating: false,
        error: null,
      };
      patchState(store, next);
    },

    /** Clear session but keep rememberMe preference */
    logout() {
      const next: AuthState = {
        ...initialAuthState,
      };
      patchState(store, next);
      clearStorage();
    },

    /** Update tokens (e.g., refresh) and persist */
    updateTokens(tokens: AuthTokens) {
      const next: AuthState = {
        loggedIn: store.loggedIn(),
        user: store.user(),
        tokens,
        authenticating: store.authenticating(),
        error: store.error(),
      };
      patchState(store, next);
    },

    /** Optional: update just the user profile (e.g., after edit-profile) */
    updateUser(user: any) {
      const next: AuthState = {
        loggedIn: store.loggedIn(),
        user,
        tokens: store.tokens(),
        authenticating: store.authenticating(),
        error: store.error(),
      };
      patchState(store, next);
    },
  }))
);
