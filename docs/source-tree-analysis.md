# Source Tree Analysis

Generated: 2026-02-22 | Type: Monorepo (Angular Nx Frontend + .NET Backend)

---

## Root Structure

```
sports-ui/                          # Monorepo root
├── apps/                           # Angular applications
│   ├── sports-ui/                  # ★ Main consumer sports app
│   ├── sports-ui-e2e/              # Playwright E2E for sports-ui
│   ├── sports-admin/               # Admin management app
│   ├── sports-admin-e2e/           # Playwright E2E for sports-admin
│   ├── sports-gm/                  # General Manager app
│   ├── sports-gm-e2e/              # Playwright E2E for sports-gm
│   └── assets/                     # Shared static assets
│
├── libs/                           # Shared Angular libraries (feature-sliced)
│   ├── auth/                       # Authentication domain
│   ├── core/                       # Cross-cutting concerns
│   ├── dashboard/                  # Dashboard feature + data
│   ├── layout/                     # App shell layout
│   ├── notification/               # Notifications (toast + feature + data)
│   ├── organization/               # Organization management
│   ├── player/                     # Player data + feature
│   ├── player-options/             # Player voting options
│   ├── profile/                    # User profile
│   ├── redeem/                     # Reward redemption
│   ├── roaster/                    # Active roster
│   ├── theme/                      # App theming
│   ├── ui/                         # Shared presentational components
│   ├── user-management/            # User admin
│   └── vote-account/               # Vote wallet / tokens
│
├── services/                       # .NET backend services
│   ├── sportsAPI/                  # ★ Main sports REST API
│   ├── IdentityService/            # Auth/JWT service
│   ├── NotificationAPI/            # Email notification worker
│   ├── MessagingService/           # (legacy/alternate messaging)
│   ├── SportifyCore/               # Shared .NET core libraries
│   ├── Messaging/                  # RabbitMQ / MassTransit building blocks
│   ├── docker-compose.yml          # Full stack Docker orchestration
│   ├── docker-compose.override.yml # Local override config
│   └── SportsSystem.sln            # .NET solution file
│
├── Infrastructure/                 # (empty/placeholder)
├── docs/                           # ★ Project knowledge base (this folder)
├── _bmad/                          # BMAD AI workflow tooling
├── _bmad-output/                   # BMAD generated artifacts
├── nx.json                         # Nx workspace config
├── package.json                    # Root dependencies (Angular 20, NgRx, Nx 21)
├── tsconfig.base.json              # Base TypeScript config + path aliases
├── jest.config.ts                  # Root Jest config
├── eslint.config.mjs               # ESLint config
└── yarn.lock                       # Dependency lockfile
```

---

## Frontend: `apps/sports-ui/` (Main App)

```
apps/sports-ui/
├── src/
│   ├── app/
│   │   ├── app.component.ts        # Root app component
│   │   ├── app.config.ts           # ★ DI providers, store registration, interceptors
│   │   ├── app.routes.ts           # ★ Route definitions (lazy-loaded)
│   │   └── shell/
│   │       └── shell.component.ts  # Main layout shell (auth-protected)
│   ├── environments/               # env.ts / env.prod.ts
│   └── main.ts                     # Bootstrap entry point
└── project.json                    # Nx project config
```

---

## Frontend: `libs/` — Library Structure

Each library follows: `libs/<domain>/<type>/src/`

```
libs/
├── auth/
│   ├── auth-data-access/src/
│   │   ├── auth.store.ts           # ★ NgRx Signal Store for auth state
│   │   ├── auth.model.ts           # AuthState, AuthTokens interfaces
│   │   └── services/
│   │       ├── auth.api.ts         # HTTP calls to IdentityService
│   │       ├── auth.service.ts     # Auth business logic
│   │       ├── auth.facade.ts      # Facade wrapping store for consumers
│   │       └── auth.guard.ts       # Functional route guard
│   └── feature-auth/src/
│       ├── login/login.component.ts
│       └── google-signin/google-signin.component.ts
│
├── core/
│   ├── api-types/src/              # TypeScript interfaces for all API models
│   ├── clients/
│   │   └── sports-api.client.ts    # NSwag-generated HTTP client
│   ├── data-access/src/            # Base HTTP data access
│   ├── http-client/src/
│   │   ├── interceptors/
│   │   │   └── api-base-url.interceptor.ts  # JWT injection
│   │   └── services/
│   │       ├── api-health.service.ts
│   │       └── environment.service.ts
│   └── error-handler/src/
│       ├── error-handler.store.ts
│       └── error-handler-interceptor.service.ts
│
├── dashboard/
│   ├── dashboard-data-access/src/
│   │   ├── dashboard.store.ts
│   │   └── service/dashboard.service.ts
│   └── feature-dashboard/src/      # Dashboard route components
│
├── notification/
│   ├── notification-data-access/src/notification.store.ts
│   ├── notification-feature/src/
│   ├── toast/src/toast.service.ts
│   └── toast-feature/src/
│
├── organization/
│   ├── organization-data-access/src/
│   │   ├── organziation.store.ts
│   │   └── organizaton.service.ts
│   └── feature-organization/src/
│
├── player/
│   ├── player-data-access/src/lib/
│   │   ├── player.store.ts
│   │   └── player.api.ts
│   └── player-feature/ (+ player-search-feature)
│
├── player-options/
│   ├── player-option-data-access/src/
│   │   ├── player-option.store.ts
│   │   └── player-options.service.ts
│   ├── feature-player-option/
│   └── create-player-option-feature/
│
├── redeem/
│   ├── redeem-data-access/src/redeem.store.ts + redeem.api.ts
│   └── feature-redeem/
│
├── roaster/
│   ├── roaster-data-access/src/roaster.store.ts + roaster.service.ts
│   └── feature-roaster/
│
├── theme/
│   ├── theme-data-access/src/services/theme.service.ts
│   └── feature-theme/
│
├── ui/src/components/
│   ├── navbar/navbar.component.ts
│   └── sidebar/sidebar.component.ts
│
├── profile/
│   ├── profile-data-access/
│   └── feature-profile/
│
└── vote-account/
    └── vote-account-data-access/src/vote-account.store.ts
```

---

## Backend: `services/sportsAPI/` (Main API)

```
services/sportsAPI/
├── Domain/                         # ★ Core business entities (no dependencies)
│   ├── Abstractions/               # Entity, Aggregate, IEntity, IDomainEvent
│   ├── Organizations/              # Organization aggregate + Theme child entity
│   ├── Leagues/                    # League entity
│   ├── Player/                     # Player entity
│   ├── PlayerOption/               # PlayerOption entity
│   ├── VoteAccount/                # VoteAccount aggregate
│   ├── Notification/               # Notification entity
│   ├── Rewards/                    # RewardItem entity
│   ├── Product/ + Purchase/        # Commerce domain
│   ├── DomainServices/             # VotingService, RewardRedemptionService
│   ├── ValueObjects/               # Address, Money, TeamColors, etc.
│   ├── Repositories/               # IRepository interface
│   ├── Shared kernel/              # SpendToken, VoteTransaction
│   └── Enums/                      # FulfillmentStatus, PaymentProvider, etc.
│
├── Application/                    # ★ CQRS commands/queries (MediatR)
│   ├── Common/
│   │   ├── Behaviours/ValidationBehaviour.cs  # FluentValidation pipeline
│   │   ├── Interfaces/             # IApplicationDbContext, IQrCodeGenerator, etc.
│   │   └── Models/                 # Result<T>, UserInfo
│   ├── Leagues/Commands + Queries
│   ├── Organizations/Commands + Queries
│   ├── Players/Commands + Queries
│   ├── PlayerOptions/Commands + Queries
│   ├── VoteAccount/Commands + Queries
│   ├── Notification/Commands + Queries
│   ├── Themes/Queries
│   └── Dto/                        # DTOs for each domain
│
├── Infrastructure/                 # ★ EF Core, repos, external integrations
│   ├── Data/
│   │   ├── SportsDbAppContext.cs   # DbContext (10 DbSets)
│   │   ├── Configurations/         # EF Fluent API configs per entity
│   │   └── SeedData.cs
│   ├── Repositories/               # EfRepository, EfReadRepository, OrganizationRepository
│   ├── Migrations/                 # 3 migrations (latest: rewarditempromocode)
│   ├── Consumers/RewardReceivedConsumer  # MassTransit consumer
│   ├── Events/DomainEventDispatcher.cs
│   └── Integrations/Identity/UserDirectoryClient.cs  # HTTP client → IdentityService
│
├── WebAPI/                         # ★ Entry point + controllers
│   ├── Program.cs                  # DI setup, middleware, Swagger, JWT config
│   └── Controllers/
│       ├── LeagueController.cs
│       ├── OrgController.cs
│       ├── PlayerController.cs
│       ├── PlayerOptionController.cs
│       ├── VoteAccount.cs
│       └── NotificationController.cs
│
└── Tests/Application.Tests/        # Unit tests for Application layer
```

---

## Backend: `services/IdentityService/`

```
services/IdentityService/
├── Controllers/AuthController.cs   # 6 endpoints: register, login, google, refresh, logout, get
├── Data/IdentityDbContext.cs        # ASP.NET Core Identity DbContext
├── Models/ApplicationUser.cs       # Extended Identity user
├── DTOs/AuthenticationDTOs.cs      # Request/response DTOs
├── Services/
│   ├── TokenService.cs             # JWT generation + refresh tokens
│   ├── GoogleAuthService.cs        # Google token verification
│   └── GoogleAuthOptions.cs        # Google config binding
├── Migrations/                     # Identity schema migrations
└── Program.cs                      # Service bootstrap
```

---

## Backend: `services/NotificationAPI/`

```
services/NotificationAPI/
├── Program.cs                      # Worker service bootstrap
├── Consumers/                      # MassTransit message consumers
├── Services/                       # Email sending services
├── Emails/                         # Email template/content
├── Messaging/                      # MassTransit config
└── Configuration/                  # Options binding
```

---

## Shared: `services/SportifyCore/`

Shared .NET library referenced by both `sportsAPI` and `IdentityService`:

```
SportifyCore/
├── Contracts/    # Shared DTOs / message contracts between services
├── Exceptions/   # Custom exception types
├── Web/          # ASP.NET Core shared middleware (+ SqlClient, EF Core)
├── Domain/       # Shared domain primitives
└── Application/  # Shared application abstractions
```

---

## Shared: `services/Messaging/`

```
Messaging/BuildingBlocks.Messageing/
└── BuildingBlocks.Messageing.csproj   # MassTransit.RabbitMQ 8.4.1 wrapper
```

Used by `sportsAPI/WebAPI` and `NotificationAPI`.

---

## Docker Infrastructure (`services/docker-compose.yml`)

| Service | Image / Build | Port | Description |
|---------|--------------|------|-------------|
| `webapi` | sportsAPI Dockerfile | 5000:8080 | Main REST API |
| `identityapi` | IdentityService Dockerfile | 5001:8080 | Auth service |
| `notificationapi` | NotificationAPI Dockerfile | — | Email worker |
| `rabbitmq` | rabbitmq:3-management | 5672, 15672 | Message broker |
| `smtp` | rnwood/smtp4dev | 3000, 2525 | Dev email server |
| `sqlserver` | mssql/server:2022-latest | 1433 | SQL Server DB |
