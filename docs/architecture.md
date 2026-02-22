# Architecture Document

Generated: 2026-02-22 | Project: sports-ui Monorepo

---

## Executive Summary

**sports-ui** is a full-stack sports platform monorepo containing three Angular 20 applications (sports-ui, sports-admin, sports-gm) sharing a rich library ecosystem, backed by a .NET 8 microservices backend with Clean Architecture. The system manages sports organizations, leagues, players, fan engagement voting, and reward redemption.

---

## Repository Structure

| Part | Type | Root | Pattern |
|------|------|------|---------|
| `frontend` | Angular Nx Workspace | `apps/` + `libs/` | Feature-sliced libraries, NgRx Signals |
| `backend` | .NET 8 Microservices | `services/` | Clean Architecture, CQRS/MediatR |

---

## Frontend Architecture

### Pattern: Feature-Sliced Nx Monorepo

The frontend is organized as an **Nx monorepo** with Angular 20. Libraries are sliced by domain and type:

```
libs/<domain>/<type>/
  - data-access  → NgRx Signal Store + HTTP services
  - feature-*    → Smart (container) components, lazy-loaded routes
  - ui           → Presentational (dumb) components
  - api-types    → TypeScript interfaces
```

### Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| State Management | NgRx Signals Store | Angular-native signals, simpler than classic NgRx |
| Components | Standalone (no NgModules) | Better tree-shaking, explicit dependencies |
| Change Detection | Zoneless | Performance (no Zone.js overhead) |
| Routing | Functional lazy-load | `loadComponent` / `loadChildren` per route |
| Dependency Injection | `inject()` function | Cleaner than constructor injection |
| HTTP | Angular HttpClient + interceptor | JWT auto-injection via `apiBaseUrlInterceptor` |
| API Client | NSwag auto-generated | Type-safe client from OpenAPI spec (`nswag.sports.json`) |
| Testing (unit) | Jest + jest-preset-angular | Per-library test isolation |
| Testing (e2e) | Playwright | Per-app E2E suites |

### Route Structure (sports-ui)

```
/login                          → LoginComponent (public)
/:organizationId                → ShellComponent (authGuard protected)
  /dashboard                    → feature-dashboard (lazy)
  /active-roaster               → feature-roaster (lazy)
  /player-option                → feature-player-option (lazy)
  /create-player-option         → create-player-option-feature (lazy)
  /redeem                       → feature-redeem (lazy)
  /profile                      → feature-profile (lazy)
  /organization                 → feature-dashboard (placeholder)
/unauthorized                   → UnauthorizedComponent
/not-found                      → NotFoundComponent
** → redirect to /not-found
```

### State Architecture

Each domain has a dedicated Signal Store registered at the root:

```
AuthStore           → session, JWT tokens, user identity
OrganizationStore   → current org data
PlayerStore         → player roster
PlayerOptionStore   → voting options
VoteAccountStore    → user vote token wallet
RedeemStore         → reward redemption state
NotificationStore   → in-app notifications
DashboardStore      → dashboard aggregates
```

Auth state persists to `localStorage` under key `sports-auth`.

---

## Backend Architecture

### Pattern: Clean Architecture + CQRS

The backend follows **Clean Architecture** with strict dependency direction:
```
WebAPI → Application → Domain
Infrastructure → Application + Domain
```

### Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|---------------|
| Domain | `sportsAPI/Domain` | Entities, value objects, domain services, repository interfaces, domain events |
| Application | `sportsAPI/Application` | CQRS commands/queries (MediatR), DTOs, validation (FluentValidation), interfaces |
| Infrastructure | `sportsAPI/Infrastructure` | EF Core DbContext, repositories, migrations, MassTransit consumers, external HTTP clients |
| WebAPI | `sportsAPI/WebAPI` | ASP.NET Core controllers, DI wiring, Swagger, middleware |

### CQRS with MediatR

All business operations flow through MediatR:
- **Commands**: `CreateOrganizationCommand`, `UpdatePlayerCommand`, `VoteCommand`, etc.
- **Queries**: `GetAllOrganizationsQuery`, `GetAllPlayersQuery`, etc.
- **Validation Pipeline**: `ValidationBehaviour<TRequest, TResponse>` runs FluentValidation before every command

### Domain Model Highlights

- Rich domain model with **value objects** (Address, Money, TeamColors, etc.)
- **Strongly-typed IDs** (`OrganizationId`, `PlayerId`, `VoteAccountId`, etc.) — prevent ID confusion
- **Aggregate pattern**: Organization, VoteAccount are aggregate roots with domain events
- **Domain events** dispatched post-save via `IDomainEventDispatcher` + EF Core interceptor

### Microservice Communication

| From | To | Mechanism |
|------|----|-----------|
| `sportsAPI` | `IdentityService` | HTTP (`UserDirectoryClient.cs`) |
| `sportsAPI` | `NotificationAPI` | RabbitMQ via MassTransit (`RewardReceivedConsumer`) |
| `NotificationAPI` | External Email | SMTP (smtp4dev in dev, real SMTP in prod) |

### Shared Libraries (`SportifyCore`, `Messaging`)

- **SportifyCore**: Cross-service contracts, exceptions, shared web middleware
- **Messaging/BuildingBlocks.Messageing**: MassTransit + RabbitMQ configuration abstracted for all services

---

## Infrastructure / Deployment

### Docker Compose Stack

```
services/docker-compose.yml orchestrates:
  - webapi (sportsAPI)        → :5000
  - identityapi               → :5001
  - notificationapi           → background worker
  - rabbitmq:3-management     → :5672 (AMQP), :15672 (management UI)
  - rnwood/smtp4dev           → :3000 (UI), :2525 (SMTP)
  - mssql/server:2022-latest  → :1433 (persistent volume: sql_data)
```

### Environment Configuration

| Service | Key Config |
|---------|-----------|
| sportsAPI | `ConnectionStrings__DefaultConnection`, `MessageBroker__*`, `Jwt__*`, `IdentityService__BaseUrl` |
| IdentityService | `ConnectionStrings__DefaultConnection`, `MessageBroker__*`, Google OAuth credentials |
| NotificationAPI | `MessageBroker__*`, `Email__SmtpHost/Port/FromEmail` |

### Azure SQL (Production)
Config is present in `appsettings.json` (commented out):
```
Server=tcp:conor-sports-db.database.windows.net,1433;Authentication=Active Directory Interactive
```

---

## Security Architecture

| Concern | Implementation |
|---------|---------------|
| Authentication | JWT Bearer tokens issued by IdentityService |
| Google SSO | Google OAuth 2.0 — token verified server-side in IdentityService |
| Token Refresh | `/api/Auth/refresh` endpoint |
| Frontend Guard | `authGuard` functional guard — redirects to `/login` if no user signal |
| HTTP Authorization | `apiBaseUrlInterceptor` injects `Bearer` token on all requests |
| JWT Config | Key: `YourSuperSecretKeyThatIsAtLeast32CharactersLong!` — **must be changed in production** |

---

## Testing Strategy

| Type | Tool | Scope |
|------|------|-------|
| Frontend Unit | Jest (jest-preset-angular) | Per-library, components and stores |
| Frontend E2E | Playwright | Per-app (sports-ui-e2e, sports-admin-e2e, sports-gm-e2e) |
| Backend Unit | xUnit (Application.Tests) | Application layer commands/queries |

### Run Tests
```bash
# Frontend all tests
nx run-many -t test

# Frontend lint
nx run-many -t lint

# Backend tests
dotnet test services/SportsSystem.sln
```
