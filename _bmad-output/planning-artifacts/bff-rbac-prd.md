# PRD: BFF Authorization Layer & Role-Based Access Control

## Status: Draft
## Date: 2026-03-07

---

## 1. Overview

The Sportify platform currently has a single sports API that is called directly by the Angular frontend. Authentication is handled by a separate IdentityService, but authorization logic is scattered across individual controller attributes and not enforced at a centralized layer.

This PRD defines requirements for:
1. A **Backend For Frontend (BFF)** service (`sports-bff`) that acts as the single entry point for all non-identity API calls from the Angular UI.
2. A **Role-Based Access Control (RBAC)** model with extensible permissions embedded into the user object returned to the client.
3. **Angular UI helpers** (directives, guards, façade methods) that hide or disable features the current user cannot access, based on their roles and permissions.

---

## 2. Goals

- **Centralize authorization**: All HTTP requests from the Angular UI pass through `sports-bff`, which validates the JWT and enforces coarse-grained authorization before proxying to the sports API.
- **Decouple the UI from internal services**: The Angular app has one API surface (`sports-bff`) for all data requests. The sports API URL is never exposed to the browser.
- **Deliver role & permission data to the client**: The user object returned after login includes `roles` and `permissions` arrays so the UI can make access decisions locally without an extra round-trip.
- **Extensible permissions model**: Roles ship with a default permission set, but individual permissions can be granted or denied per user in the future without a code deploy.
- **Consistent tab/feature visibility**: Navigation items and feature sections are hidden (not just disabled) when the user lacks the required role or permission.

---

## 3. Non-Goals

- Fine-grained per-resource authorization (e.g., "user can only edit their own cards") remains in the sports API domain layer.
- The BFF is not a GraphQL gateway or aggregator — it is a transparent reverse proxy with an auth layer.
- Defining individual permissions per user (beyond default role permissions) is out of scope for the initial implementation. The permission extensibility points are wired but seeded only from roles.
- Multi-tenancy permission overrides (different permissions per organization) are not in scope.

---

## 4. User Roles

| Role | Description |
|------|-------------|
| `Admin` | Super-admin. Full access to all features including administration tools. |
| `GM` | General Manager. Full access to all day-to-day operations but cannot access administration tools. |
| `User` | Standard fan account. Access to fan-facing features only. |

### Role Hierarchy

```
Admin ⊃ GM ⊃ User
```

Each higher role inherits all permissions of lower roles plus its own additions.

---

## 5. Feature Access Matrix

The following table defines which top-level navigation sections are visible per role.

| Navigation Section | Routes | Admin | GM | User |
|---|---|:---:|:---:|:---:|
| Dashboard | `dashboard` | ✅ | ✅ | ✅ |
| Collection | `collection` | ✅ | ✅ | ✅ |
| H2H | `h2h` | ✅ | ✅ | ✅ |
| Franchise | `player-option`, `active-roaster` | ✅ | ✅ | ✅ |
| Store & Marketplace | `card-packs`, `marketplace`, `redeem` | ✅ | ✅ | ✅ |
| Operations | `send-votes`, `create-player-option`, `admin/trivia-management`, `admin/poll-management` | ✅ | ✅ | ❌ |
| Administration | `admin/economy`, `admin/audit-log` | ✅ | ❌ | ❌ |

### Within-Tab Permissions (enforced by sports API, surfaced in UI)

| Action | Admin | GM | User |
|---|:---:|:---:|:---:|
| View any content | ✅ | ✅ | ✅ |
| Vote / submit trivia answers / submit poll votes | ✅ | ✅ | ✅ |
| Purchase card packs | ✅ | ✅ | ✅ |
| Create/manage player options | ✅ | ✅ | ❌ |
| Create/archive polls & trivia | ✅ | ✅ | ❌ |
| Send vote rewards | ✅ | ✅ | ❌ |
| Manage players/roster | ✅ | ✅ | ❌ |
| Economy admin (rarity tiers, pack cost) | ✅ | ❌ | ❌ |
| Audit log | ✅ | ❌ | ❌ |

---

## 6. BFF Service Requirements

### 6.1 Routing

- The BFF must expose the same API surface as the sports API (same paths, same HTTP verbs).
- All requests to `/api/**` (except `/api/auth/**`) are proxied through the BFF to the sports API.
- The Angular UI sends its JWT to the BFF. The BFF validates the token, then forwards the request to the sports API with the same bearer token (sports API still validates independently as defense-in-depth).

### 6.2 Authentication

- The BFF validates incoming JWT tokens using the RSA public key from the IdentityService.
- Requests without a valid token receive a `401 Unauthorized` response.
- The `/api/auth/**` path is **not** proxied — those calls go directly to the IdentityService from the browser.

### 6.3 Authorization

- The BFF enforces coarse-grained authorization (role presence) before proxying.
- Fine-grained rules (ownership, specific command validation) remain in the sports API.
- Specific routes that require elevated roles are listed in BFF policy configuration (not hardcoded per controller — a route-policy map).

### 6.4 Request Forwarding

- The BFF passes the original `Authorization: Bearer <token>` header downstream to the sports API unchanged.
- Other relevant headers (`Content-Type`, `Accept`, `X-Correlation-ID`) are forwarded.
- The sports API base URL is stored in BFF `appsettings.json`, not known to the browser.

---

## 7. User Object — Roles & Permissions

After login or token refresh, the IdentityService returns an `AuthenticationResponse`. The `UserInfo` embedded in this response must be extended to include:

```json
{
  "id": "...",
  "email": "...",
  "firstName": "...",
  "lastName": "...",
  "roles": ["GM"],
  "permissions": [
    "feature:dashboard",
    "feature:collection",
    "feature:h2h",
    "feature:franchise",
    "feature:store",
    "feature:marketplace",
    "feature:operations",
    "action:vote",
    "action:purchase-pack",
    "action:create-player-option",
    "action:manage-polls",
    "action:send-votes"
  ]
}
```

- `roles` is a flat array of role names (e.g., `["Admin"]`, `["GM"]`, `["User"]`).
- `permissions` is a flat array of permission keys derived from the user's roles at login time.
- The permission key format is `<category>:<action>` (e.g., `feature:administration`, `action:vote`).
- The client uses permissions for local UI visibility decisions; the server enforces them independently.

---

## 8. Angular UI Requirements

### 8.1 Navigation Filtering

- `ShellComponent` must filter `navItems` at runtime based on the current user's permissions.
- Hidden nav items are **not rendered** (not just styled as disabled).
- Navigation filtering is reactive — if the user's session changes (e.g., role change after re-login), the nav updates automatically.

### 8.2 Structural Directive

A `*hasPermission` directive must be available globally:

```html
<!-- Show only to users with the given permission -->
<button *hasPermission="'action:create-player-option'">Create Option</button>

<!-- Show only to users with the given role -->
<div *hasRole="'admin'">Admin-only content</div>
```

### 8.3 Route Guards

- A `roleGuard` factory function must protect sensitive routes.
- Routes without the required role redirect to `/unauthorized`.

### 8.4 AuthFacade Helpers

- `hasRole(role: string): boolean` — checks if the current user has a role.
- `hasPermission(permission: string): boolean` — checks if the current user has a specific permission.
- `canAccess(feature: FeatureKey): boolean` — checks the feature access map.

---

## 9. Success Criteria

- Angular app has zero direct calls to `sports-bff` URLs that bypass the BFF (verified by network tab).
- A `User` role account cannot navigate to Operations or Administration routes (redirected to `/unauthorized`).
- A `GM` role account cannot navigate to Administration routes.
- An `Admin` account has full access to all routes.
- Removing a nav item from RBAC config hides it for affected roles within one release cycle.
- The `permissions` array on the user object can be extended with new keys without a schema migration.
