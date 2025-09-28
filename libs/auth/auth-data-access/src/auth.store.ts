import { computed } from "@angular/core";
import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withState,
} from "@ngrx/signals";
import {
  AuthState,
  initialAuthState,
  SessionUser,
  AuthTokens,
} from "./auth.model";

const STORAGE_KEY = "sports-auth";

function readStorage(remember: boolean): AuthState | null {
  const raw = (remember ? localStorage : sessionStorage).getItem(STORAGE_KEY);
  try {
    return raw ? (JSON.parse(raw) as AuthState) : null;
  } catch {
    return null;
  }
}
function writeStorage(state: AuthState) {
  const box = state.rememberMe ? localStorage : sessionStorage;
  box.setItem(STORAGE_KEY, JSON.stringify(state));
}
function clearStorage() {
  localStorage.removeItem(STORAGE_KEY);
  sessionStorage.removeItem(STORAGE_KEY);
}

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

  withMethods((store) => ({
    hydrate() {
      const fromLocal = readStorage(true);
      const fromSession = readStorage(false);
      const loaded = fromLocal ?? fromSession;
      if (loaded) patchState(store, loaded);
    },

    setRemember(remember: boolean) {
      patchState(store, { rememberMe: remember });
    },

    setAuthenticating(on: boolean) {
      patchState(store, {
        authenticating: on,
        error: on ? null : store.error(),
      });
    },

    setError(message: string | null) {
      patchState(store, { error: message });
    },

    loginSuccess(user: SessionUser, tokens: AuthTokens) {
      const next: AuthState = {
        // read current values from signals:
        loggedIn: true,
        user,
        tokens,
        authenticating: false,
        error: null,
        rememberMe: store.rememberMe(), // keep current preference
      };
      patchState(store, next);
      writeStorage(next);
    },

    logout() {
      patchState(store, {
        ...initialAuthState,
        rememberMe: store.rememberMe(),
      });
      clearStorage();
    },

    updateTokens(tokens: AuthTokens) {
      const next: AuthState = {
        loggedIn: store.loggedIn(),
        user: store.user(),
        tokens,
        authenticating: store.authenticating(),
        error: store.error(),
        rememberMe: store.rememberMe(),
      };
      patchState(store, next);
      writeStorage(next);
    },
  }))
);
