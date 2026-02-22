# State Management — Frontend

Generated: 2026-02-22 | Library: NgRx Signals Store 19.2.1

---

## Overview

All state is managed via **NgRx Signal Store** (`@ngrx/signals`). Each domain feature has its own store registered as a root-level provider in `app.config.ts`. There are no NgModules — all stores use `signalStore()` with standalone Angular.

Stores use `withState`, `withMethods`, `withComputed`, and where needed `rxMethod` for async operations.

---

## Registered Stores (app.config.ts)

| Store | Library Path | Domain |
|-------|-------------|--------|
| `AuthStore` | `@sports-ui/auth-data-access` | Authentication & session |
| `PlayerOptionStore` | `@sports-ui/player-options-data-access` | Player voting options |
| `OrganizationStore` | `@sports-ui/organization-data-access` | Organization data |
| `VoteAccountStore` | `@sports-ui/vote-account-data-access` | Vote wallet / tokens |
| `NotificationStore` | `@sports-ui/notification-data-access` | In-app notifications |
| `PlayerStore` | `@sports-ui/player-data-access` | Player roster data |
| `RedeemStore` | `@sports-ui/redeem-data-access` | Reward redemption |
| `DashboardStore` | `@sports-ui/dashboard-data-access` | Dashboard aggregates |

---

## AuthStore (detailed)

**File**: `libs/auth/auth-data-access/src/auth.store.ts`

### State shape (`AuthState`)
```typescript
{
  loggedIn: boolean,
  user: any | null,
  tokens: AuthTokens | null,   // { accessToken, expiresAt }
  authenticating: boolean,
  error: string | null
}
```

### Computed signals
| Signal | Type | Description |
|--------|------|-------------|
| `isExpired` | `boolean` | Whether the current access token is expired |
| `bearer` | `string \| null` | The raw access token string |
| `userEmail` | `string \| null` | Logged-in user's email |
| `roles` | `string[]` | User's role array |

### Methods
| Method | Description |
|--------|-------------|
| `hydrate()` | Load session from localStorage / sessionStorage |
| `setAuthenticating(on)` | Toggle loading spinner |
| `setError(message)` | Set/clear error |
| `loginSuccess(user, tokens)` | Save auth session + persist to localStorage |
| `logout()` | Clear session + remove from storage |
| `updateTokens(tokens)` | Update tokens on refresh |
| `updateUser(user)` | Update user profile data |

### Persistence
- Auth state is persisted to **localStorage** under key `sports-auth`
- On app init, `hydrate()` rehydrates from storage
- Key: `sports-auth`

---

## Auth Guard

**File**: `libs/auth/auth-data-access/src/services/auth.guard.ts`

Functional guard (`CanActivateFn`) using `AuthFacade`:
- If `auth.user()` signal is truthy → allow navigation
- Otherwise → redirect to `/login`

Applied to all routes under `/:organizationId` path.

---

## HTTP Interceptor

**File**: `libs/core/http-client/src/lib/interceptors/api-base-url.interceptor.ts`

Reads `authToken` from `localStorage` and injects:
```
Authorization: Bearer <token>
```
on every outgoing HTTP request.

> Note: There's a slight divergence — `AuthStore` uses key `sports-auth` (JSON object) while the interceptor reads `authToken` (raw string). These may need reconciliation.

---

## Store Pattern (all stores follow this)

```typescript
export const ExampleStore = signalStore(
  withState<ExampleState>(initialState),
  withComputed((state) => ({
    derivedValue: computed(() => state.someField()),
  })),
  withMethods((store, service = inject(ExampleService)) => ({
    loadData: rxMethod<void>(pipe(
      switchMap(() => service.getAll()),
      tap((data) => patchState(store, { items: data }))
    )),
  }))
);
```
