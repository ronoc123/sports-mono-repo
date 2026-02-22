# Integration Architecture

Generated: 2026-02-22

---

## Overview

The sports-ui platform is a monorepo with two major parts that communicate across well-defined integration points: the Angular frontend communicates with the .NET backend via REST/HTTP, and the backend services communicate with each other via HTTP and RabbitMQ.

---

## Integration Map

```
┌──────────────────────────────────────────────────────────────────┐
│                     FRONTEND (Angular Nx)                        │
│  sports-ui / sports-admin / sports-gm                            │
│                                                                  │
│  apiBaseUrlInterceptor → injects Bearer token on every request   │
│  NSwag auto-generated client (sports-api.client.ts)              │
└──────────┬───────────────────┬───────────────────────────────────┘
           │ REST HTTP          │ REST HTTP
           │ :5000              │ :5001
           ▼                   ▼
┌──────────────────┐   ┌──────────────────────┐
│   sportsAPI      │   │   IdentityService     │
│   (port 5000)    │◄──│   (port 5001)         │
│                  │   │                       │
│  - Leagues       │   │  - /auth/register     │
│  - Organizations │   │  - /auth/login        │
│  - Players       │   │  - /auth/google       │
│  - PlayerOptions │   │  - /auth/refresh      │
│  - VoteAccounts  │   │  - /auth/logout       │
│  - Notifications │   │  - /auth/get          │
└────────┬─────────┘   └──────────┬────────────┘
         │ MassTransit              │ ASP.NET Identity
         │ RabbitMQ                 │ SQL Server (SportsDb)
         ▼                         │
┌──────────────────┐               │
│  NotificationAPI │               │
│  (worker service)│               │
│                  │               │
│  Consumers RMQ   │               │
│  → sends email   │               │
│  via SMTP        │               │
└──────────────────┘               │
         │                         │
         ▼                         ▼
┌──────────────────────────────────────────────┐
│         Infrastructure (Docker)              │
│                                              │
│  rabbitmq:3-management   → :5672 / :15672    │
│  mssql/server:2022       → :1433 (SportsDb)  │
│  rnwood/smtp4dev          → :3000 / :2525    │
└──────────────────────────────────────────────┘
```

---

## Integration Points (Detail)

### 1. Frontend → IdentityService (Auth)

| Property | Value |
|----------|-------|
| Protocol | REST / HTTPS |
| Base URL | `http://localhost:5001` |
| Key Endpoints | `POST /api/Auth/login`, `POST /api/Auth/google`, `POST /api/Auth/refresh` |
| Auth | None required (public endpoints) |
| Response | JWT access token + refresh token |
| Frontend Handler | `auth.api.ts` + `AuthStore.loginSuccess()` → persists to localStorage |

### 2. Frontend → sportsAPI (Data)

| Property | Value |
|----------|-------|
| Protocol | REST / HTTPS |
| Base URL | `http://localhost:5000` |
| Auth | `Authorization: Bearer <JWT>` (injected by `apiBaseUrlInterceptor`) |
| Client | NSwag auto-generated `sports-api.client.ts` |
| Key Domains | Org, League, Player, PlayerOption, VoteAccount, Notification |
| Error Handling | `error-handler-interceptor.service.ts` + `ErrorHandlerStore` |

### 3. sportsAPI → IdentityService (User Directory)

| Property | Value |
|----------|-------|
| Protocol | HTTP |
| Client | `UserDirectoryClient.cs` (HttpClient registered via DI) |
| Config | `IdentityService.BaseUrl` = `http://localhost:5081` |
| Purpose | Look up user info from IdentityService during domain operations |

### 4. sportsAPI → NotificationAPI (Events)

| Property | Value |
|----------|-------|
| Protocol | RabbitMQ / AMQP via MassTransit 8.4.1 |
| Broker | `rabbitmq://sports.rabbitmq` (Docker: `sports.rabbitmq`) |
| Credentials | guest / guest |
| Published Events | `RewardReceivedEvent` (consumed by `RewardReceivedConsumer`) |
| Shared Contracts | `SportifyCore/Contracts` — shared message contract DTOs |

### 5. NotificationAPI → SMTP

| Property | Value |
|----------|-------|
| Protocol | SMTP |
| Dev Server | `rnwood/smtp4dev` — `:25` (SMTP), `:3000` (web UI) |
| Config | `Email__SmtpHost`, `Email__SmtpPort`, `Email__FromEmail` |
| Purpose | Send transactional emails (reward notifications, etc.) |

---

## Data Flow: Fan Voting + Reward Redemption

```
1. User loads /:orgId/player-option
2. Frontend → GET /api/PlayerOption/GetPlayerOptionsByOrganization?orgId=X
3. User casts vote → POST /api/PlayerOption/vote
4. sportsAPI applies vote to VoteAccount via VotingService domain service
5. On threshold: domain event raised → DomainEventDispatcher fires
6. sportsAPI publishes RewardReceivedEvent → RabbitMQ
7. NotificationAPI consumes event → sends email via SMTP

8. User views rewards → GET /api/VoteAccount/get-vote-account/{userId}/organization/{orgId}
9. User redeems → POST /api/VoteAccount/redeem-vote/{userId}/reward/{rewardItemId}
10. RewardRedemptionService generates QR code (QRCoder) or promo code
```

---

## Shared Code Dependencies

| Shared Asset | Used By |
|-------------|---------|
| `SportifyCore/Contracts` | sportsAPI + IdentityService — shared message DTOs |
| `SportifyCore/Exceptions` | sportsAPI + IdentityService + NotificationAPI |
| `SportifyCore/Web` | sportsAPI + IdentityService — shared middleware |
| `Messaging/BuildingBlocks` | sportsAPI/WebAPI + NotificationAPI — MassTransit setup |
| `@sports-ui/api-types` | All 3 Angular apps — shared TypeScript interfaces |
| `@sports-ui/auth` / `@sports-ui/auth-data-access` | All 3 Angular apps — auth guard + store |
| `@sports-ui/ui` | All 3 Angular apps — Navbar + Sidebar components |
