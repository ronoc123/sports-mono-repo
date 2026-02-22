# Component Inventory — Frontend

Generated: 2026-02-22 | Framework: Angular 20 (Standalone components)

---

## Overview

All components are **standalone** (no NgModules). Components are organized across feature libraries and the shared `ui` library. The smart/dumb pattern is followed:
- **Feature libraries** contain smart components (inject stores/services)
- **`libs/ui`** contains dumb/presentational components (inputs/outputs only)

---

## Shared UI Components (`libs/ui`)

| Component | Selector | Description |
|-----------|----------|-------------|
| `NavbarComponent` | — | Top navigation bar |
| `SidebarComponent` | — | Side navigation panel |

---

## Auth Feature (`libs/auth/feature-auth`)

| Component | Type | Description |
|-----------|------|-------------|
| `LoginComponent` | Smart | Login page — injects `AuthStore`, handles form submit |
| `GoogleSigninComponent` | Smart | Google OAuth sign-in button — triggers Google auth flow |

---

## Feature Library Components (smart, route-level)

Each feature library below contains route-level smart components loaded via `loadComponent()` or `loadChildren()`:

| Library / Path | Feature | Entry Route |
|---------------|---------|-------------|
| `libs/dashboard/feature-dashboard` | Dashboard overview | `/:orgId/dashboard` |
| `libs/roaster/feature-roaster` | Active roster management | `/:orgId/active-roaster` |
| `libs/player-options/feature-player-option` | View player options / voting | `/:orgId/player-option` |
| `libs/player-options/create-player-option-feature` | Create new player option | `/:orgId/create-player-option` |
| `libs/redeem/feature-redeem` | Reward redemption flow | `/:orgId/redeem` |
| `libs/profile/feature-profile` | User profile | `/:orgId/profile` |
| `libs/layout/feature-layout` | App shell layout wrapper | Root layout component |
| `libs/notification/notification-feature` | Notification panel | — |
| `libs/organization/feature-organization` | Organization management | — |

---

## App Shell (`apps/sports-ui/src/app/shell`)

| Component | Description |
|-----------|-------------|
| `ShellComponent` | Root layout container loaded at `/:organizationId`. Houses navbar, sidebar, and `<router-outlet>`. Protected by `authGuard`. |

---

## Inline Components (app.routes.ts)

| Component | Route | Description |
|-----------|-------|-------------|
| `UnauthorizedComponent` | `/unauthorized` | Simple 403 message with link to dashboard |
| `NotFoundComponent` | `/not-found` | Simple 404 message with link to dashboard |

---

## Multi-App Inventory

| App | Purpose | Key Routes |
|-----|---------|-----------|
| `sports-ui` | Main consumer-facing sports platform | login, `/:orgId/dashboard`, roster, player options, redeem, profile |
| `sports-admin` | Admin management interface | TBD — separate app config |
| `sports-gm` | General Manager interface | TBD — separate app config |

---

## Naming Conventions

- Smart components: `*feature*`, `*page*`, `*container*`
- Dumb/presentational: live in `libs/ui/` or `*/ui/` subdirectories
- Route-level components: exported from library `index.ts` as named exports
