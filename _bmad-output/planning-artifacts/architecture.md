---
stepsCompleted: ['step-01-init', 'step-02-context', 'step-03-starter', 'step-04-decisions', 'step-05-patterns', 'step-06-structure', 'step-07-validation', 'step-08-complete']
lastStep: 8
status: 'complete'
completedAt: '2026-03-01'
inputDocuments:
  - "_bmad-output/planning-artifacts/prd.md"
  - "docs/architecture.md"
  - "docs/integration-architecture.md"
  - "docs/api-contracts-backend.md"
  - "docs/data-models-backend.md"
  - "docs/component-inventory-frontend.md"
  - "docs/state-management-frontend.md"
  - "docs/source-tree-analysis.md"
workflowType: 'architecture'
project_name: 'sports-ui'
user_name: 'Kampe'
date: '2026-03-01'
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

---

## Project Context Analysis

### Requirements Overview

**42 Functional Requirements across 8 capability areas:**

| Capability Area | FRs | Architectural Implication |
|---|---|---|
| Currency & Points Management | FR1–4 | Extend existing `VoteAccount` with escrow tracking |
| Card Catalog Administration | FR5–10 | New `CardPlayer` entity — independent of existing `Player` |
| Card Pack System | FR11–16 | Server-side probabilistic pull engine (rarity weights) |
| Card Collection | FR17–18 | `UserCard` inventory per user/league |
| Auction Marketplace | FR19–28 | Full auction lifecycle with atomic bid/escrow state machine |
| Real-Time Notifications | FR29–31 | SignalR hub — new infrastructure for the platform |
| H2H Competition | FR32–40 | Bot match resolution engine, wager escrow |
| Economy Admin & Monitoring | FR41–42 | GM dashboard — read-side aggregation queries |

**Non-Functional Requirements driving architecture:**

| NFR | Architectural Forcing Function |
|---|---|
| NFR-R1: Zero escrow failures | Database transactions for all escrow ops — no optimistic updates |
| NFR-P1: 500ms outbid notification | SignalR push (not polling); hub scoped per listing |
| NFR-R2: 0.1% pull accuracy | Deterministic, auditable RNG; every pull logged |
| NFR-S1: Server-side escrow validation | Client never touches balance state directly |
| NFR-S2: Self-bid prevention | Server-side bidder ≠ owner check |
| NFR-I3: CardPlayer ≠ Player AR | New entity — no FK dependency on existing `Player` |
| NFR-SC2: H2H extensible to real players | Match/wager resolution designed for future participant types |

**Scale:** High complexity. 3 new bounded contexts, 1 new real-time infrastructure layer, 1 background service for auction expiry, multiple new aggregate roots.

### Existing Stack (Inherited — Non-Negotiable)

| Layer | Technology |
|---|---|
| Frontend | Angular 20, standalone components, NgRx Signals Store (`signalStore()`), Nx monorepo |
| State | `withState / withMethods / withComputed / rxMethod` pattern — all stores follow this |
| API Client | NSwag auto-generated TypeScript client (`libs/core/clients/sports-api.client.ts`) |
| Backend | .NET 8, Clean Architecture, CQRS/MediatR, FluentValidation pipeline |
| Data Access | `IRepository` interface — all handlers inject this; zero `_db.[Table]` direct access |
| Entities | All aggregates extend `Aggregate<Guid>` or `Entity` base |
| DB | SQL Server / EF Core 8 code-first migrations via `SportsDbAppContext` |
| Auth | JWT (IdentityService), `authGuard`, `apiBaseUrlInterceptor` |
| Messaging | RabbitMQ / MassTransit (existing notification pipeline) |
| Apps | `sports-ui` (fans), `sports-admin` (admin), `sports-gm` (GMs) |

### New Infrastructure Required

**SignalR** — Not currently in the stack. Required for FR29–31 (real-time outbid notifications). Needs: ASP.NET Core SignalR hub in `sportsAPI/WebAPI/Hubs/`, Angular `@microsoft/signalr` client library, hub registration in `Program.cs`.

**Auction Expiry Background Service** — Auctions have a duration (FR19). When they expire, settlement must trigger automatically: winner gets card, seller gets points, all escrowed bids release. Requires a .NET `BackgroundService` polling for `AuctionListing.ExpiresAt <= now`.

**RNG/Pull Engine** — Probabilistic card draw (FR13). Must be deterministic-per-pull (auditable) and configuration-driven per rarity tier. Server-side only (NFR-S3).

### New Domain Entities Required

| Entity | Type | Notes |
|---|---|---|
| `CardPlayer` | New Aggregate | Rating + auto-assigned rarity tier; league-scoped; no FK to existing `Player` |
| `UserCard` | New Entity | Instance of a pulled card owned by a user |
| `CardPack` | New Aggregate | Pack purchase record; 5 pulls per pack |
| `RarityTier` | Value Object / Config | Threshold ranges → rarity name + pull weight |
| `AuctionListing` | New Aggregate | Start bid, buy now, expiry; full state machine |
| `Bid` | New Entity | Bid amount + bidder; child of `AuctionListing` |
| `PointsEscrow` | New Entity / Extension | Track escrowed points per user per auction |
| `H2HMatch` | New Aggregate | Wager, teams, outcome, settlement |

### Cross-Cutting Concerns

- **Org scoping** — All new entities scoped to `organizationId` (existing pattern — mandatory)
- **Points escrow atomicity** — EF Core transactions required for bid placement, outbid release, auction settlement, H2H wager — database-level, not application-level
- **Transaction audit trail** — FR3 requires every point movement logged with timestamp/user/action/entity. The existing `VoteTransaction` pattern is the foundation — extend or mirror it
- **Auth boundary** — All Fan Economy endpoints require `[Authorize]` (NFR-S4); no anonymous endpoints in this feature
- **NSwag regeneration** — Every new controller endpoint requires NSwag client regeneration before frontend work begins

### Technical Complexity Assessment

- **Complexity:** High
- **Primary domain:** Full-stack brownfield extension
- **New bounded contexts:** 3 (Cards, Marketplace, H2H)
- **New Angular library groups:** 3 (`libs/cards/`, `libs/marketplace/`, `libs/h2h/`)
- **New infrastructure:** SignalR hub, auction expiry background service, pull engine
- **Architectural risk areas:** Escrow atomicity, SignalR connection lifecycle, auction expiry race conditions

---

## Starter Template Evaluation

### Primary Technology Domain

**Brownfield Full-Stack Extension** — No new project scaffolding. The Fan Economy extends the existing Nx + Angular 20 + .NET 10 monorepo using established conventions.

### Foundation: Existing Monorepo

**Selected Foundation:** Existing Nx monorepo (brownfield extension — no `create` command needed)

**Rationale:** The project provides Angular 20 standalone components, NgRx Signals Store, NSwag API client generation, .NET 10 Clean Architecture with MediatR/CQRS, EF Core with SQL Server, JWT auth middleware, and RabbitMQ messaging. All Fan Economy components slot directly into these established patterns.

**Note:** Project docs reference .NET 8; actual runtime is **.NET 10.0.102** — SignalR performance and improvements are better in .NET 10.

### New Packages Required

**Frontend:**

```bash
npm install @microsoft/signalr@10.0.0
```

| Package | Version | Purpose |
|---|---|---|
| `@microsoft/signalr` | 10.0.0 | Angular SignalR client — real-time auction outbid notifications |

**Backend:** SignalR is built into ASP.NET Core — enabled via `builder.Services.AddSignalR()` in `Program.cs`. No additional NuGet package required.

### New Nx Library Structure

```
libs/
  cards/
    data-access/          ← CardStore, card HTTP service
    feature-cards/        ← Pack purchase + card reveal UI
    feature-collection/   ← Card inventory view
  marketplace/
    data-access/          ← MarketplaceStore, auction HTTP service, SignalR service
    feature-marketplace/  ← Browse, list, bid UI
  h2h/
    data-access/          ← H2HStore, match HTTP service
    feature-h2h/          ← Squad builder + match resolution UI
```

**Scaffold commands:**
```bash
nx g @nx/angular:library cards/data-access --standalone
nx g @nx/angular:library cards/feature-cards --standalone
nx g @nx/angular:library cards/feature-collection --standalone
nx g @nx/angular:library marketplace/data-access --standalone
nx g @nx/angular:library marketplace/feature-marketplace --standalone
nx g @nx/angular:library h2h/data-access --standalone
nx g @nx/angular:library h2h/feature-h2h --standalone
```

### New Backend Bounded Context Structure

```
services/sportsAPI/
  Domain/Cards/              ← CardPlayer, UserCard, CardPack entities
  Domain/Marketplace/        ← AuctionListing, Bid, PointsEscrow entities
  Domain/H2H/                ← H2HMatch, H2HSquad entities
  Application/Cards/         ← Commands/queries for pack purchase, card pull
  Application/Marketplace/   ← Commands/queries for listing, bidding, settlement
  Application/H2H/           ← Commands/queries for match creation, resolution
  Infrastructure/Cards/      ← RarityEngine, EF configs
  Infrastructure/Marketplace/← AuctionExpiryService (BackgroundService), EF configs
  Infrastructure/SignalR/    ← AuctionHub
  WebAPI/Hubs/               ← AuctionHub endpoint registration
  WebAPI/Controllers/        ← CardsController, MarketplaceController, H2HController
```

### Architectural Decisions Provided by Foundation

- **Language & Runtime:** TypeScript (frontend), C# / .NET 10 (backend) — unchanged
- **Styling:** Component-scoped CSS — matches all existing `libs/ui` components
- **Build Tooling:** Nx 21 with Angular CLI; existing `project.json` targets apply to new libraries
- **Testing:** Jest (frontend unit), Playwright (e2e), xUnit (backend) — inherited
- **Code Organization:** Feature-sliced Nx libraries; Clean Architecture backend layers

---

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**
- Escrow atomicity: EF Core `IDbContextTransaction` per operation
- SignalR auth: JWT via `?access_token=` query string
- SignalR hub topology: Per-listing groups

**Important Decisions (Shape Architecture):**
- Pull engine: `System.Random` (seeded, seed persisted) + weighted rarity config table
- Rarity config: Database-driven `RarityTierConfig` table
- Store composition: Separate `signalStore()` per domain
- Auction expiry: .NET `BackgroundService` with polling

**Deferred Decisions (Post-MVP):**
- SignalR Redis backplane (scale-out path when multi-server deployment needed)
- Hangfire/Quartz for advanced job scheduling

### Data Architecture

**Escrow Atomicity**
- Decision: EF Core `IDbContextTransaction` wraps all escrow operations
- Rationale: NFR-R1 zero-failure guarantee; existing `SportsDbAppContext` is the transaction boundary
- Affects: `PlaceBidCommandHandler`, `SettleAuctionCommandHandler`, `H2HWagerCommandHandler`

**Pull Engine**
- Decision: `System.Random` seeded per pull (seed persisted in pull log) + Fisher-Yates weighted selection over `RarityTierConfig`
- Rationale: Auditable (seed + outcome stored), deterministic-per-pull, sufficient entropy for entertainment use
- Every pull log record: `(userId, cardPlayerId, rarityTier, seed, timestamp, packId)`
- Affects: `Infrastructure/Cards/RarityEngine.cs`, `Domain/Cards/CardPack.cs`

**Rarity Tier Configuration**
- Decision: Database-driven `RarityTierConfig` table (`ratingMin`, `ratingMax`, `rarityName`, `pullWeightBps`)
- Rationale: FR6 requires GM control over thresholds without redeploys; ties into FR41/42 GM dashboard
- Provided by Starter: No — new entity requiring migration
- Affects: `CardsController` (admin endpoints), `Infrastructure/Cards/` EF config

### Authentication & Security

**SignalR Authentication**
- Decision: JWT via `?access_token=` query string on hub connection (ASP.NET Core built-in pattern)
- Hub reads: `context.GetHttpContext().Request.Query["access_token"]`
- Rationale: Standard SPA SignalR auth pattern; WebSocket connections cannot carry `Authorization` header
- Affects: `Infrastructure/SignalR/AuctionHub.cs`, Angular `AuctionSignalRService`

**Self-Bid Prevention & Server-Side Escrow Validation**
- Decision: Command handler layer — `PlaceBidCommandHandler` validates `bidderId ≠ listing.sellerId` before touching DB
- Rationale: NFR-S1/S2; Clean Architecture — validation at application layer produces meaningful error response
- Affects: `Application/Marketplace/Commands/PlaceBidCommandHandler.cs`

**Fan Economy Auth Boundary**
- Decision: All `CardsController`, `MarketplaceController`, and `H2HController` endpoints require `[Authorize]`
- Rationale: NFR-S4 — no anonymous endpoints in Fan Economy
- Provided by Starter: No — must be applied explicitly to every new controller

### API & Communication Patterns

**SignalR Hub Topology**
- Decision: Per-listing hub groups — `Groups.AddToGroupAsync(connectionId, $"auction-{listingId}")`
- Hub sends `OutbidNotification` only to users watching the relevant listing
- Rationale: NFR-P1 (500ms notification SLA); minimal broadcast surface; group lifecycle tied to listing expiry
- Affects: `WebAPI/Hubs/AuctionHub.cs`, Angular `AuctionSignalRService`

**REST API Design**
- Decision: 3 new controllers following existing pattern (`[ApiController]`, `[Route("api/[controller]")]`, MediatR `ISender`):
  - `CardsController` — pack purchase, pull reveal, collection view
  - `MarketplaceController` — list card, place bid, buy now, view listings
  - `H2HController` — create match, set squad, resolve match, history
- NSwag regeneration required after each new endpoint before frontend HTTP service work begins

**Error Handling**
- Decision: Existing FluentValidation pipeline handles 400 responses; custom `DomainException` → 422 for escrow/business rule failures (insufficient balance, self-bid, already sold)
- Rationale: Consistent with existing error handling; escrow failures need distinct HTTP status from input validation failures

### Frontend Architecture

**NgRx Signal Store Composition**
- Decision: 3 separate `signalStore()` instances in their respective `data-access` libraries:
  - `CardStore` (`libs/cards/data-access/`) — `{ cards: UserCard[], packs: CardPack[], pullStatus: 'idle'|'pulling'|'success', pullResult: UserCard | null }`
  - `MarketplaceStore` (`libs/marketplace/data-access/`) — `{ listings: AuctionListing[], myBids: Bid[], watchedListingId: string | null, signalRConnected: boolean }`
  - `H2HStore` (`libs/h2h/data-access/`) — `{ squad: UserCard[], opponents: BotProfile[], matchResult: H2HResult | null, matchStatus: 'idle'|'pending'|'resolved' }`
- Rationale: Matches existing pattern (`VoteAccountFacade`, `StoreService` are separate); Nx library isolation; independently loadable

**SignalR → Store Integration**
- Decision: `AuctionSignalRService` (Angular service in `marketplace/data-access`) connects to hub; feeds events into `MarketplaceStore` via `rxMethod`
- Pattern: `signalR.on('OutbidNotification', msg => this.store.handleOutbid(msg))`
- Rationale: `rxMethod` is the NgRx Signals pattern for async/event-driven state updates

**Routing**
- Decision: New lazy-loaded routes added to shell — `/store/cards`, `/store/marketplace`, `/store/h2h`
- Each route loads the feature library component; auth guard applied at shell level

### Infrastructure & Deployment

**Auction Expiry Background Service**
- Decision: .NET `BackgroundService` polling `AuctionListing WHERE ExpiresAt <= UTC_NOW() AND Status = 'Active'` every 30 seconds; dispatches `SettleAuctionCommand` via MediatR `ISender`
- Polling interval configurable via `appsettings.json` (`AuctionExpiry:PollIntervalSeconds`)
- Rationale: No new dependencies; single-server deployment; straightforward lifecycle via `IHostedService`
- Affects: `Infrastructure/Marketplace/AuctionExpiryService.cs`, `Program.cs` (`builder.Services.AddHostedService<AuctionExpiryService>()`)

**SignalR Backplane (MVP)**
- Decision: Single-server in-memory backplane (ASP.NET Core default)
- Scale-out path: Redis backplane (`StackExchange.Redis` + `Microsoft.AspNetCore.SignalR.StackExchangeRedis`) for multi-server deployment — deferred post-MVP
- Rationale: Current deployment is single-server; Redis adds operational complexity not yet warranted

### Decision Impact Analysis

**Implementation Sequence:**
1. `RarityTierConfig` table + seed migration — unblocks pull engine and GM dashboard
2. `CardPlayer`, `UserCard`, `CardPack` entities + migrations — unblocks card features
3. Pull engine + `CardsController` + NSwag regen — unblocks frontend card work
4. `AuctionListing`, `Bid`, `PointsEscrow` entities + migrations — unblocks marketplace
5. SignalR hub registration + `AuctionHub` — unblocks real-time frontend
6. `AuctionExpiryService` registration — unblocks full auction lifecycle
7. `H2HMatch`, `H2HSquad` entities + `H2HController` — last bounded context
8. Angular stores + feature libs — can parallel with backend after each NSwag regen

**Cross-Component Dependencies:**
- `MarketplaceStore` depends on SignalR hub being live → hub must ship before real-time bids work in UI
- `AuctionExpiryService` depends on `PointsEscrow` entity → migrations must run before service starts
- Pull engine depends on `RarityTierConfig` table being seeded → GM must configure tiers before packs go live
- NSwag regeneration is a hard gate between backend controller work and frontend HTTP service work

---

## Implementation Patterns & Consistency Rules

### Critical Conflict Points Identified

18 areas where AI agents could make different choices — all resolved by existing codebase conventions or Fan Economy-specific decisions below.

### Naming Patterns

**Database Naming Conventions:**
- Table names: PascalCase (EF Core default, matches existing — `VoteTransactions`, `Players`)
- Column names: PascalCase property names (EF Core default — `OrganizationId`, `ExpiresAt`)
- Foreign keys: `{EntityName}Id` — e.g., `AuctionListingId`, `CardPlayerId`
- New Fan Economy tables: `CardPlayers`, `UserCards`, `CardPacks`, `RarityTierConfigs`, `AuctionListings`, `Bids`, `PointsEscrows`, `H2HMatches`
- Anti-pattern: `card_players`, `cardplayers`, `tbl_CardPlayers` — none of these

**API Naming Conventions:**
- Endpoints: plural kebab-case — `/api/cards`, `/api/marketplace`, `/api/h2h`
- Route params: `{id}` format (ASP.NET Core convention)
- Query params: camelCase — `?organizationId=`, `?listingId=`
- Controller actions: verb-noun — `GetCollection`, `PurchasePack`, `PlaceBid`, `CreateMatch`
- Anti-pattern: `/api/card`, `/api/get-cards`, `/api/CardPlayers`

**Code Naming — Backend (C#):**
- Entities: PascalCase — `CardPlayer`, `AuctionListing`, `PointsEscrow`
- Commands: `{Verb}{Noun}Command` — `PurchasePackCommand`, `PlaceBidCommand`, `SettleAuctionCommand`
- Queries: `Get{Noun}Query` — `GetCollectionQuery`, `GetListingsQuery`
- Handlers: `{CommandOrQuery}Handler` — `PurchasePackCommandHandler`
- Repository methods: `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`

**Code Naming — Frontend (TypeScript/Angular):**
- Components: PascalCase class, kebab-case selector — `CardRevealComponent`, `lib-card-reveal`
- Services: `{Domain}Service` or `{Domain}Store` — `CardService`, `MarketplaceStore`
- Store files: `{domain}.store.ts` — `card.store.ts`, `marketplace.store.ts`, `h2h.store.ts`
- Signal store methods: camelCase verbs — `loadListings`, `placeBid`, `handleOutbid`
- Files: kebab-case — `card-reveal.component.ts`, `auction-signalr.service.ts`
- Anti-pattern: `CardRevealComp`, `cardRevealComponent`, `card_reveal.component.ts`

**SignalR Event Names:**
- PascalCase hub method names — `OutbidNotification`, `AuctionExpired`, `BidPlaced`
- Angular `.on('OutbidNotification', ...)` must exactly match hub `Clients.Group(...).SendAsync("OutbidNotification", ...)`
- Anti-pattern: `outbidNotification`, `outbid_notification`, `OUTBID_NOTIFICATION`

### Structure Patterns

**Backend Project Organization:**
```
Domain/{BoundedContext}/          ← Pure entities, value objects, no dependencies
Application/{BoundedContext}/
  Commands/                       ← {Verb}{Noun}Command.cs + Handler
  Queries/                        ← Get{Noun}Query.cs + Handler
Infrastructure/{BoundedContext}/  ← EF configs, RarityEngine, AuctionExpiryService
WebAPI/Controllers/               ← Thin controllers; only ISender.Send() calls
WebAPI/Hubs/                      ← SignalR hub registration
```
- Rule: Handlers NEVER inject `SportsDbAppContext` directly — always `IRepository<T>`
- Rule: Domain entities have NO reference to Application or Infrastructure namespaces
- Anti-pattern: Putting business logic in controllers or EF config classes

**Frontend Library Organization (Nx):**
```
libs/{domain}/data-access/        ← signalStore(), HTTP service, SignalR service
libs/{domain}/feature-{name}/     ← Routable page components (smart)
libs/{domain}/ui/                 ← Presentational components (dumb), if needed
```
- Rule: Feature components inject stores/services from their `data-access` sibling only
- Rule: `data-access` libraries are never imported by other `data-access` libraries
- Anti-pattern: Feature components calling HTTP directly; stores in feature libraries

**Test File Location:**
- Frontend: co-located `*.spec.ts` alongside source file (Jest)
- Backend: `sportsAPI.Tests/` project, mirroring source namespace structure (xUnit)
- Anti-pattern: `__tests__/` folder, separate `tests/` directory at lib root

### Format Patterns

**API Response Formats:**
- Success: Direct typed response object (no wrapper) — NSwag generates client from OpenAPI schema
- Validation failure (400): FluentValidation default format — `{ errors: { field: ['message'] } }`
- Business rule failure (422): `{ error: string, code: string }` — e.g., `{ error: "Insufficient balance", code: "INSUFFICIENT_BALANCE" }`
- Escrow failure codes: `INSUFFICIENT_BALANCE`, `SELF_BID`, `AUCTION_EXPIRED`, `ALREADY_SOLD`, `OUTBID_AMOUNT_TOO_LOW`
- Anti-pattern: Wrapping all responses in `{ data: ..., success: true }`, returning 200 for errors

**Date/Time Formats:**
- All API timestamps: ISO 8601 UTC — `"2026-03-01T14:30:00Z"`
- EF Core storage: `DateTime` with `DateTimeKind.Utc`
- Frontend display: Angular `DatePipe` with user locale; never raw ISO strings in templates
- Anti-pattern: Unix timestamps, local time in API responses, `DateTime.Now` (use `DateTime.UtcNow`)

**JSON Field Naming:**
- System.Text.Json default in .NET → camelCase serialization (`PropertyNamingPolicy.CamelCase`)
- Matches existing — Angular NSwag client already expects camelCase
- Anti-pattern: PascalCase JSON (`"OrganizationId": ...`), snake_case (`"organization_id": ...`)

### Communication Patterns

**SignalR Hub Patterns:**
- Hub group key: `$"auction-{listingId}"` (lowercase prefix, hyphen separator)
- Client joins group on `WatchListing(listingId)`, leaves on `UnwatchListing(listingId)`
- Hub sends typed DTO records — `record OutbidNotification(Guid ListingId, decimal NewHighBid, Guid NewHighBidderId)`
- Angular service manages connection lifecycle: connect on marketplace route init, disconnect on destroy
- Anti-pattern: Broadcast to all connections, string-only payloads, managing hub state in components

**NgRx Signal Store Patterns:**
- State shape: flat where possible — avoid nested mutable objects
- Status literals: `'idle' | 'loading' | 'success' | 'error'` — matches existing stores
- Method naming: `load{Noun}`, `set{Noun}`, `clear{Noun}`, `handle{Event}` for SignalR events
- Async methods use `rxMethod` — never `async/await` directly in store methods
- Computed signals: descriptive noun — `activeListings`, `myBidTotal`, `canBid`
- Anti-pattern: `isLoading: boolean` (use status literal), `fetchData()`, `setIsLoading(true)`

**MassTransit Event Patterns (inherited):**
- Event class names: past tense noun — `PackPurchased`, `AuctionSettled`, `BidPlaced`
- Publish via MassTransit `IPublishEndpoint` in command handlers (not in domain entities)
- Anti-pattern: Publishing events from domain entities, string-based event routing

### Process Patterns

**Loading State Patterns:**
- All async store operations set `status` to `'loading'` before HTTP call, `'success'` or `'error'` on completion
- Component templates gate on `status() === 'loading'` for spinners, `status() === 'error'` for error UI
- Pull reveal uses dedicated `pullStatus: 'idle' | 'pulling' | 'success'` with `pullResult: UserCard | null`
- Anti-pattern: `isLoading` boolean, component-level loading state, no error state

**Error Recovery Patterns:**
- HTTP errors caught in store `rxMethod` — set `status` to `'error'`, log to console
- Escrow failures (422): display user-facing message from `error.code` lookup in Angular
- SignalR disconnection: `MarketplaceStore.signalRConnected` flips to `false`; UI shows reconnect banner
- Anti-pattern: Swallowing errors silently, `alert()` for user errors

**Auth Flow Patterns:**
- Backend: Every Fan Economy endpoint has `[Authorize]` attribute — no exceptions
- Frontend: Shell-level `authGuard` covers `/store/*` routes; no component-level auth checks
- SignalR: `AuctionSignalRService` reads JWT from `AuthFacade.token()` and passes as `?access_token=`
- Anti-pattern: `[AllowAnonymous]` on escrow endpoints

**Escrow Operation Patterns:**
- ALL point debits/credits occur inside `IDbContextTransaction` scope
- Transaction scope: open → validate balance → debit → record → commit (rollback on any failure)
- Never read balance outside transaction then write inside — TOCTOU race condition
- Pattern: Read + validate + write atomically within a single transaction

### Enforcement Guidelines

**All AI Agents MUST:**
- Follow `IRepository<T>` pattern — zero direct `_db.Table` access in handlers
- Use `DateTime.UtcNow` — never `DateTime.Now`
- Wrap ALL escrow operations in `IDbContextTransaction`
- Log every card pull: `(userId, cardPlayerId, rarityTier, seed, timestamp, packId)`
- Apply `[Authorize]` to every Fan Economy controller action
- Scope every new entity to `organizationId`
- Run NSwag regen after adding any new controller endpoint before writing Angular HTTP service
- Use status literal `'idle' | 'loading' | 'success' | 'error'` — not `isLoading: boolean`
- Name SignalR events in PascalCase — match exactly between hub `SendAsync` and Angular `.on()`

**Pattern Verification:**
- EF Core migrations: review generated migration for correct table/column naming before applying
- Frontend: `nx lint` enforces module boundaries (feature libs cannot import other feature libs)
- SignalR: integration test confirming hub group join/leave + `OutbidNotification` delivery
- Escrow: unit test confirming transaction rollback on balance validation failure

### Pattern Examples

**Correct — Signal store async method:**
```typescript
readonly loadListings = rxMethod<string>(
  pipe(
    tap(() => patchState(this, { status: 'loading' })),
    switchMap(orgId => this.marketplaceService.getListings(orgId).pipe(
      tapResponse({
        next: listings => patchState(this, { listings, status: 'success' }),
        error: () => patchState(this, { status: 'error' })
      })
    ))
  )
);
```

**Anti-pattern — async/await in store (forbidden):**
```typescript
// WRONG — never use async/await in signalStore methods
async loadListings(orgId: string) {
  this.status.set('loading');
  const listings = await this.service.getListings(orgId); // ❌
}
```

**Correct — Escrow command handler (atomic transaction):**
```csharp
await using var tx = await _db.Database.BeginTransactionAsync();
var escrow = await _repo.GetEscrowAsync(request.UserId, request.ListingId);
if (escrow.Balance < request.BidAmount) throw new DomainException("INSUFFICIENT_BALANCE");
escrow.Debit(request.BidAmount);
await _repo.UpdateAsync(escrow);
await tx.CommitAsync();
```

**Anti-pattern — Optimistic escrow (TOCTOU race):**
```csharp
// WRONG — read outside transaction; another request can debit between read and write
var balance = await _repo.GetBalanceAsync(userId); // ❌
if (balance >= amount)
    await _repo.DebitAsync(userId, amount); // ❌ not atomic
```

---

## Project Structure & Boundaries

### Complete Project Directory Structure

Brownfield extension — only new additions and key integration points shown. Existing structure inherited unchanged.

#### New Backend Structure

```
services/sportsAPI/
│
├── Domain/
│   ├── Cards/
│   │   ├── CardPlayer.cs               ← Aggregate<Guid>; rating, rarityTierId, orgId
│   │   ├── UserCard.cs                 ← Entity; owned card instance (userId, cardPlayerId)
│   │   ├── CardPack.cs                 ← Aggregate<Guid>; 5 pulls; pull log entries
│   │   └── PullLogEntry.cs             ← Value object; seed, rarityTier, timestamp
│   ├── Marketplace/
│   │   ├── AuctionListing.cs           ← Aggregate<Guid>; state machine (Active/Sold/Expired)
│   │   ├── Bid.cs                      ← Entity; child of AuctionListing
│   │   └── PointsEscrow.cs             ← Entity; per-user per-auction escrowed balance
│   └── H2H/
│       ├── H2HMatch.cs                 ← Aggregate<Guid>; wager, teams, outcome
│       └── H2HSquad.cs                 ← Entity; 5-card squad per match participant
│
├── Application/
│   ├── Cards/
│   │   ├── Commands/
│   │   │   ├── PurchasePackCommand.cs      ← FR12: buy pack, debit points, trigger 5 pulls
│   │   │   ├── CreateCardPlayerCommand.cs  ← FR5: GM creates card catalog entry
│   │   │   └── UpdateCardPlayerCommand.cs  ← FR7: GM updates rating/availability
│   │   └── Queries/
│   │       ├── GetCollectionQuery.cs       ← FR17: fan views owned cards
│   │       ├── GetCardCatalogQuery.cs      ← FR9: browse available card players
│   │       └── GetEconomySummaryQuery.cs   ← FR41: GM dashboard aggregate stats
│   ├── Marketplace/
│   │   ├── Commands/
│   │   │   ├── ListCardCommand.cs          ← FR19: fan lists UserCard for auction
│   │   │   ├── PlaceBidCommand.cs          ← FR22: fan places bid + escrow debit
│   │   │   ├── BuyNowCommand.cs            ← FR25: instant purchase
│   │   │   └── SettleAuctionCommand.cs     ← FR27: expiry settlement (BackgroundService)
│   │   └── Queries/
│   │       ├── GetListingsQuery.cs         ← FR20: browse active listings
│   │       └── GetMyBidsQuery.cs           ← FR26: fan's active bid history
│   └── H2H/
│       ├── Commands/
│       │   ├── CreateMatchCommand.cs       ← FR32: initiate H2H match + wager escrow
│       │   ├── SetSquadCommand.cs          ← FR35: fan selects 5 cards
│       │   └── ResolveMatchCommand.cs      ← FR36: bot resolution + settlement
│       └── Queries/
│           └── GetMatchHistoryQuery.cs     ← FR40: fan's H2H history
│
├── Infrastructure/
│   ├── Cards/
│   │   ├── RarityEngine.cs                 ← Weighted pull (System.Random + RarityTierConfig)
│   │   ├── CardPlayerConfiguration.cs
│   │   ├── UserCardConfiguration.cs
│   │   ├── CardPackConfiguration.cs
│   │   └── RarityTierConfigConfiguration.cs
│   ├── Marketplace/
│   │   ├── AuctionExpiryService.cs         ← BackgroundService; polls + dispatches SettleAuctionCommand
│   │   ├── AuctionListingConfiguration.cs
│   │   ├── BidConfiguration.cs
│   │   └── PointsEscrowConfiguration.cs
│   ├── H2H/
│   │   ├── BotResolutionEngine.cs          ← FR36: bot team generation + match outcome
│   │   ├── H2HMatchConfiguration.cs
│   │   └── H2HSquadConfiguration.cs
│   └── Data/
│       └── SportsDbAppContext.cs           ← EXISTING — new DbSets added here
│
└── WebAPI/
    ├── Controllers/
    │   ├── CardsController.cs              ← [Authorize] FR5–18 endpoints
    │   ├── MarketplaceController.cs        ← [Authorize] FR19–28 endpoints
    │   └── H2HController.cs               ← [Authorize] FR32–40 endpoints
    ├── Hubs/
    │   └── AuctionHub.cs                  ← SignalR hub; WatchListing/UnwatchListing/OutbidNotification
    └── appsettings.json                   ← EXISTING — add AuctionExpiry:PollIntervalSeconds
```

#### New EF Core Migrations

```
services/sportsAPI/Infrastructure/Data/Migrations/
├── ..._AddFanEconomy_RarityTierConfig.cs
├── ..._AddFanEconomy_Cards.cs          ← CardPlayers, UserCards, CardPacks, PullLogEntries
├── ..._AddFanEconomy_Marketplace.cs    ← AuctionListings, Bids, PointsEscrows
└── ..._AddFanEconomy_H2H.cs            ← H2HMatches, H2HSquads
```

#### New Frontend Library Structure

```
libs/
├── cards/
│   ├── data-access/
│   │   └── src/lib/
│   │       ├── card.store.ts               ← signalStore(); CardStore
│   │       ├── card.service.ts             ← NSwag-typed HTTP calls
│   │       └── index.ts                    ← Public API barrel
│   ├── feature-cards/
│   │   └── src/lib/
│   │       ├── feature-cards.component.ts  ← Pack purchase page (routable)
│   │       ├── feature-cards.component.html
│   │       ├── feature-cards.component.css
│   │       └── card-reveal/
│   │           ├── card-reveal.component.ts
│   │           ├── card-reveal.component.html
│   │           └── card-reveal.component.css
│   └── feature-collection/
│       └── src/lib/
│           ├── feature-collection.component.ts
│           ├── feature-collection.component.html
│           └── feature-collection.component.css
│
├── marketplace/
│   ├── data-access/
│   │   └── src/lib/
│   │       ├── marketplace.store.ts            ← signalStore(); MarketplaceStore
│   │       ├── marketplace.service.ts          ← NSwag-typed HTTP calls
│   │       ├── auction-signalr.service.ts      ← @microsoft/signalr connection + event relay
│   │       └── index.ts
│   └── feature-marketplace/
│       └── src/lib/
│           ├── feature-marketplace.component.ts
│           ├── listing-card/
│           │   └── listing-card.component.ts
│           └── bid-panel/
│               └── bid-panel.component.ts
│
└── h2h/
    ├── data-access/
    │   └── src/lib/
    │       ├── h2h.store.ts                ← signalStore(); H2HStore
    │       ├── h2h.service.ts              ← NSwag-typed HTTP calls
    │       └── index.ts
    └── feature-h2h/
        └── src/lib/
            ├── feature-h2h.component.ts
            ├── squad-builder/
            │   └── squad-builder.component.ts
            └── match-result/
                └── match-result.component.ts
```

#### Shell Routing Integration (Existing File Modified)

```typescript
// apps/sports-ui/src/app/shell/shell.component.ts — ADD these routes
{ path: 'store/cards',       loadComponent: () => import('@sports-ui/cards-feature-cards').then(m => m.FeatureCardsComponent),                  canActivate: [authGuard] },
{ path: 'store/collection',  loadComponent: () => import('@sports-ui/cards-feature-collection').then(m => m.FeatureCollectionComponent),          canActivate: [authGuard] },
{ path: 'store/marketplace', loadComponent: () => import('@sports-ui/marketplace-feature-marketplace').then(m => m.FeatureMarketplaceComponent),  canActivate: [authGuard] },
{ path: 'store/h2h',         loadComponent: () => import('@sports-ui/h2h-feature-h2h').then(m => m.FeatureH2hComponent),                         canActivate: [authGuard] }
```

#### Nx tsconfig Path Aliases (tsconfig.base.json — ADD)

```json
"@sports-ui/cards-data-access":               ["libs/cards/data-access/src/index.ts"],
"@sports-ui/cards-feature-cards":              ["libs/cards/feature-cards/src/index.ts"],
"@sports-ui/cards-feature-collection":         ["libs/cards/feature-collection/src/index.ts"],
"@sports-ui/marketplace-data-access":          ["libs/marketplace/data-access/src/index.ts"],
"@sports-ui/marketplace-feature-marketplace":  ["libs/marketplace/feature-marketplace/src/index.ts"],
"@sports-ui/h2h-data-access":                  ["libs/h2h/data-access/src/index.ts"],
"@sports-ui/h2h-feature-h2h":                 ["libs/h2h/feature-h2h/src/index.ts"]
```

### Architectural Boundaries

**API Boundaries:**
- `/api/cards/*` — `[Authorize]`; fan + GM endpoints; `CardsController`
- `/api/marketplace/*` — `[Authorize]`; fan endpoints; `MarketplaceController`
- `/api/h2h/*` — `[Authorize]`; fan endpoints; `H2HController`
- `/hubs/auction` — `[Authorize]` via JWT query string; SignalR WebSocket
- GM admin actions in `CardsController`: `[Authorize(Roles = "GM")]`

**Component Boundaries:**
- `cards/data-access` → consumed by `feature-cards` and `feature-collection` only
- `marketplace/data-access` → consumed by `feature-marketplace` only
- `h2h/data-access` → consumed by `feature-h2h` only
- `libs/ui` (existing) → consumed by all new feature libraries for shared UI primitives
- Cross-domain `data-access` imports forbidden — enforced by Nx module boundary rules

**Service Boundaries:**
- `RarityEngine` — internal to `Infrastructure/Cards/`; called only by `PurchasePackCommandHandler`
- `BotResolutionEngine` — internal to `Infrastructure/H2H/`; called only by `ResolveMatchCommandHandler`
- `AuctionExpiryService` — `IHostedService`; communicates only via MediatR `ISender`; no direct repo access
- `AuctionHub` — receives client connections; dispatches notifications; no direct domain logic

**Data Boundaries:**
- All new entities registered in `SportsDbAppContext` — single DB context (existing pattern)
- `PointsEscrow` reads `VoteAccount.Balance` — crosses bounded context; must be inside same `IDbContextTransaction`
- `CardPlayer` has zero FK to `Player` table (NFR-I3) — enforced at EF config level

### Requirements to Structure Mapping

| FR Group | Backend Files | Frontend Files |
|---|---|---|
| FR1–4 Currency/Points | `VoteAccount` extension, all escrow commands | `libs/core/vote-account-data-access` (existing) |
| FR5–10 Card Catalog Admin | `CreateCardPlayerCommand`, `CardsController` admin actions | `sports-admin` / `sports-gm` app |
| FR11–16 Card Pack System | `PurchasePackCommand`, `RarityEngine` | `libs/cards/feature-cards/`, card-reveal component |
| FR17–18 Card Collection | `GetCollectionQuery` | `libs/cards/feature-collection/` |
| FR19–28 Marketplace | `ListCard/PlaceBid/BuyNow/SettleAuction` commands | `libs/marketplace/feature-marketplace/` |
| FR29–31 Real-Time | `AuctionHub` | `auction-signalr.service.ts`, `MarketplaceStore.handleOutbid` |
| FR32–40 H2H | `CreateMatch/SetSquad/ResolveMatch`, `BotResolutionEngine` | `libs/h2h/feature-h2h/` |
| FR41–42 Economy Admin | `GetEconomySummaryQuery` | `sports-gm` app GM dashboard |

### Integration Points & Data Flows

**Pack Purchase Flow:**
```
Fan → PurchasePackCommand → open tx → debit VoteAccount → create CardPack
     → RarityEngine × 5 → persist UserCards + PullLogEntries → commit tx
     → Response: 5 UserCard DTOs → CardStore.pullResult → CardReveal animation
```

**Bid Placement Flow:**
```
Fan → PlaceBidCommand → open tx → validate bidderId ≠ sellerId → validate amount > high bid
     → debit PointsEscrow → create Bid → commit tx
     → AuctionHub.SendAsync("OutbidNotification") to group "auction-{listingId}"
     → AuctionSignalRService → MarketplaceStore.handleOutbid() → listings signal updates
```

**Auction Expiry Flow:**
```
AuctionExpiryService (every 30s) → query expired Active listings
→ SettleAuctionCommand per listing → open tx → transfer UserCard to winner
→ credit seller VoteAccount → release losing PointsEscrows → set Status=Settled → commit
→ AuctionHub.SendAsync("AuctionExpired") to group "auction-{listingId}"
```

**H2H Match Flow:**
```
Fan → CreateMatchCommand → open tx → escrow wager → create H2HMatch → commit
→ SetSquadCommand → store fan's 5 UserCards
→ ResolveMatchCommand → BotResolutionEngine generates bot squad → calculate outcome
→ open tx → transfer wager to winner VoteAccount → set match status → commit
```

### Development Workflow

**NSwag Regeneration Gate:**
1. Add/modify endpoint in `WebAPI/Controllers/`
2. `dotnet build services/sportsAPI/WebAPI`
3. NSwag CLI regenerates `libs/core/clients/sports-api.client.ts`
4. Write Angular HTTP service methods against new typed client

**Nx Library Scaffold (one-time setup):**
```bash
nx g @nx/angular:library cards/data-access --standalone
nx g @nx/angular:library cards/feature-cards --standalone
nx g @nx/angular:library cards/feature-collection --standalone
nx g @nx/angular:library marketplace/data-access --standalone
nx g @nx/angular:library marketplace/feature-marketplace --standalone
nx g @nx/angular:library h2h/data-access --standalone
nx g @nx/angular:library h2h/feature-h2h --standalone
```

**EF Core Migration Sequence:**
```bash
dotnet ef migrations add AddFanEconomy_RarityTierConfig --project Infrastructure --startup-project WebAPI
dotnet ef migrations add AddFanEconomy_Cards --project Infrastructure --startup-project WebAPI
dotnet ef migrations add AddFanEconomy_Marketplace --project Infrastructure --startup-project WebAPI
dotnet ef migrations add AddFanEconomy_H2H --project Infrastructure --startup-project WebAPI
dotnet ef database update --project Infrastructure --startup-project WebAPI
```

---

## Architecture Validation Results

### Coherence Validation ✅

**Decision Compatibility:** All technology choices are mutually compatible.
- Angular 20 + `@microsoft/signalr@10.0.0` + Nx 21: no conflicts
- .NET 10 + ASP.NET Core SignalR (built-in) + EF Core 8: no version conflicts
- `IDbContextTransaction` + `IRepository<T>` + `SportsDbAppContext`: valid transactional pattern
- `System.Random` pull engine + `RarityTierConfig` DB table: no dependencies
- `BackgroundService` + MediatR `ISender`: standard .NET DI, no circular references

**Pattern Consistency:** Confirmed uniform across all 3 bounded contexts.
- All stores: `rxMethod` for async, `'idle'|'loading'|'success'|'error'` status literals
- All handlers: `IRepository<T>` — zero direct `_db.Table` access
- All endpoints: `[Authorize]` — no exceptions
- SignalR events: PascalCase, matched exactly between hub `SendAsync` and Angular `.on()`

**Architecture Clarification — VoteAccount vs PointsEscrow:**
FR1–4 specified "extend VoteAccount with escrow tracking." The architecture uses `PointsEscrow` as a separate entity (per-user, per-auction) rather than a field extension on `VoteAccount`. This is intentional: `VoteAccount.Balance` is the canonical balance (debited atomically on bid placement); `PointsEscrow` tracks locked amounts per listing (released to losing bidders on outbid/expiry, credited to seller on settlement). The separation maintains single-responsibility and enables multi-auction bidding without corrupting the primary balance.

### Requirements Coverage Validation ✅

**All 42 Functional Requirements covered:**

| FR Group | Architectural Coverage |
|---|---|
| FR1–4 Currency/Points | `VoteAccount` debited in transactions; `PointsEscrow` tracks locked amounts |
| FR5–10 Card Catalog Admin | `CreateCardPlayerCommand`, `UpdateCardPlayerCommand`, `CardsController` admin actions |
| FR11–16 Card Pack System | `PurchasePackCommand`, `RarityEngine`, `PullLogEntry`; pull weights hidden server-side (NFR-S3) |
| FR17–18 Card Collection | `GetCollectionQuery`, `libs/cards/feature-collection/` |
| FR19–28 Marketplace | `ListCard/PlaceBid/BuyNow/SettleAuction` commands + `GetListings/GetMyBids` queries |
| FR29–31 Real-Time | `AuctionHub`, `AuctionSignalRService`, `MarketplaceStore.handleOutbid` |
| FR32–40 H2H | `CreateMatch/SetSquad/ResolveMatch`, `BotResolutionEngine`, `libs/h2h/feature-h2h/` |
| FR41–42 Economy Admin | `GetEconomySummaryQuery`, `sports-gm` GM dashboard page |

**All 9 NFRs covered:**

| NFR | Covered By |
|---|---|
| NFR-R1: Zero escrow failures | `IDbContextTransaction` on all escrow ops |
| NFR-P1: 500ms outbid notification | SignalR push, per-listing hub groups |
| NFR-R2: 0.1% pull accuracy | Seeded `System.Random` + pull log with seed |
| NFR-S1: Server-side escrow | All balance ops in command handlers only |
| NFR-S2: Self-bid prevention | `PlaceBidCommandHandler` validates `bidderId ≠ sellerId` |
| NFR-S3: Pull weights hidden | `RarityEngine` server-side; weights never in API responses |
| NFR-S4: All endpoints `[Authorize]` | Enforcement guideline + pattern verification |
| NFR-I3: CardPlayer ≠ Player | No FK, enforced at EF config level |
| NFR-SC2: H2H extensible | `H2HMatch`/`H2HSquad` not hardcoded to bot participant type |

### Implementation Readiness Validation ✅

**Decision Completeness:** All critical decisions documented with versions and rationale. Deferred decisions (Redis backplane, Hangfire) explicitly tagged post-MVP.

**Structure Completeness:** File-level directory tree for all 3 bounded contexts (backend + frontend). Shell routing additions, tsconfig aliases, EF migration sequence, and Nx scaffold commands all specified.

**Pattern Completeness:** 18 conflict points identified and resolved. Correct/anti-pattern code examples provided for store methods and escrow handlers.

### Gap Analysis Results

**Critical Gaps: NONE**

**Important Gap Addressed — `Program.cs` Registrations:**
Three mandatory registrations not previously made explicit. Must be added to `WebAPI/Program.cs`:
```csharp
// Builder section:
builder.Services.AddSignalR();
builder.Services.AddHostedService<AuctionExpiryService>();

// App section (after UseAuthorization):
app.MapHub<AuctionHub>("/hubs/auction");
```

**Nice-to-Have (deferred):** `BotResolutionEngine` internal algorithm left to implementation — architecture constrains the interface, not the scoring logic.

### Architecture Completeness Checklist

**✅ Requirements Analysis**
- [x] 42 FRs across 8 capability areas analyzed and mapped
- [x] 9 NFRs mapped to architectural forcing functions
- [x] Existing stack constraints identified as non-negotiable
- [x] New infrastructure scoped (SignalR, BackgroundService, RarityEngine)
- [x] Cross-cutting concerns mapped (org scoping, escrow atomicity, audit trail, NSwag gate)

**✅ Architectural Decisions**
- [x] Escrow atomicity: `IDbContextTransaction`
- [x] Pull engine: seeded `System.Random` + weighted `RarityTierConfig`
- [x] Rarity config: database-driven (GM-configurable without redeploy)
- [x] SignalR auth: JWT via `?access_token=` query string
- [x] Hub topology: per-listing groups
- [x] Auction expiry: .NET `BackgroundService` + MediatR dispatch
- [x] Store composition: 3 separate `signalStore()` instances
- [x] SignalR→store integration: `rxMethod` event relay pattern

**✅ Implementation Patterns**
- [x] 18 conflict points identified and resolved
- [x] Naming conventions: DB tables, API endpoints, C# classes, TypeScript files, SignalR events
- [x] Structure patterns: Clean Architecture layers, Nx module boundary rules
- [x] Format patterns: API responses (422 for escrow failures), UTC ISO 8601 dates, camelCase JSON
- [x] Communication patterns: SignalR hub groups, NgRx `rxMethod`, MassTransit events
- [x] Process patterns: loading states, error recovery, auth flow, escrow atomicity
- [x] Enforcement guidelines + correct/anti-pattern code examples

**✅ Project Structure**
- [x] Backend file-level tree: Domain/Application/Infrastructure/WebAPI × 3 bounded contexts
- [x] Frontend file-level tree: 7 new Nx libraries with component breakdown
- [x] EF migration sequence (4 migrations, dependency-ordered)
- [x] Nx scaffold commands (7 libraries)
- [x] Shell routing additions (4 lazy-loaded routes with `authGuard`)
- [x] tsconfig path aliases (7 new aliases)
- [x] NSwag regen workflow documented as hard gate
- [x] `Program.cs` SignalR + BackgroundService registrations specified

### Architecture Readiness Assessment

**Overall Status: READY FOR IMPLEMENTATION**

**Confidence Level: High**

**Key Strengths:**
- Brownfield extension — all patterns inherited from proven existing stack; zero greenfield scaffolding risk
- Escrow atomicity enforced at pattern level — not left to implementation discretion
- Implementation sequence is dependency-ordered — no bounded context can start out-of-order
- NSwag regen gate explicitly documented — prevents frontend/backend type drift
- SignalR→NgRx integration pattern specified with concrete code shape — eliminates agent variation
- All 18 AI agent conflict points pre-resolved with examples

**Areas for Future Enhancement (Post-MVP):**
- Redis SignalR backplane for multi-server horizontal scaling
- Hangfire or Quartz.NET for advanced auction expiry scheduling with retry policies
- CQRS read-model projection for GM economy dashboard as data volume grows
- `BotResolutionEngine` algorithm tuning based on real H2H engagement data

### Implementation Handoff

**First Implementation Steps (in order):**
```bash
# 1. Install SignalR Angular client
npm install @microsoft/signalr@10.0.0

# 2. Scaffold Nx libraries (see Project Structure section)
nx g @nx/angular:library cards/data-access --standalone
# ... (all 7 libraries)

# 3. Add Program.cs registrations
# builder.Services.AddSignalR()
# builder.Services.AddHostedService<AuctionExpiryService>()
# app.MapHub<AuctionHub>("/hubs/auction")

# 4. First EF migration (unblocks pull engine + GM dashboard)
dotnet ef migrations add AddFanEconomy_RarityTierConfig --project Infrastructure --startup-project WebAPI
dotnet ef database update --project Infrastructure --startup-project WebAPI

# 5. Implement CardsController → NSwag regen → unblocks all frontend card work
```

**AI Agent Guidelines:**
- Follow all architectural decisions exactly as documented — no local optimization without explicit approval
- Use implementation patterns consistently — anti-pattern examples are firm prohibitions
- Respect Nx module boundaries — `nx lint` enforces; violations are build failures
- Run NSwag regen after EVERY new backend endpoint before writing Angular HTTP service code
- Reference this document for all architectural questions before making local decisions
