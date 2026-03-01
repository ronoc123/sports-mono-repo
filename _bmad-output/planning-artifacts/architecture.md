---
stepsCompleted: ['step-01-init', 'step-02-context', 'step-03-starter', 'step-04-decisions', 'step-05-patterns', 'step-06-structure', 'step-07-validation', 'step-08-complete']
lastStep: 8
status: 'complete'
completedAt: '2026-02-28'
inputDocuments:
  - "_bmad-output/planning-artifacts/prd.md"
  - "docs/architecture.md"
  - "docs/integration-architecture.md"
  - "docs/api-contracts-backend.md"
  - "docs/data-models-backend.md"
  - "docs/state-management-frontend.md"
  - "docs/component-inventory-frontend.md"
workflowType: 'architecture'
project_name: 'sports-ui'
user_name: 'Kampe'
date: '2026-02-28'
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

---

## Project Context Analysis

### Project Classification

- **Type**: Brownfield — extending an existing production monorepo
- **Complexity**: High — full-stack feature spanning frontend SPA, backend API, third-party payment integration, and webhook infrastructure
- **Scope**: Fan Store micro-transaction system (Phase 1: vote bundle purchases via Stripe)

### Existing Stack (Inherited Constraints)

| Layer | Technology |
|---|---|
| Frontend | Angular 20, standalone components, NgRx Signals Store, Nx monorepo |
| Backend | .NET 8, Clean Architecture, CQRS/MediatR, EF Core |
| Database | SQL Server (Azure) |
| Auth | JWT bearer tokens (existing middleware) |
| API Client | NSwag auto-generated TypeScript client |
| Hosting | Azure App Service (backend), Azure Static Web Apps (frontend) |

### PRD Capability Areas & Architectural Implications

**1. Bundle Catalog** (FR1–FR5) — Read-only; cached; admin-managed. Implies a simple `BundleController` with GET endpoints, seeded data initially.

**2. Stripe Checkout Flow** (FR6–FR14) — Stripe Elements (embedded, no redirect), client-side tokenization → no card data on server (PCI DSS). Requires `POST /api/store/purchase` to create PaymentIntent → confirm on client → webhook verifies.

**3. Idempotent Webhook Handling** (FR15–FR18) — `stripe_event_id` deduplication, JWT-exempt endpoint, atomic `Purchase` state transitions (Pending → Completed/Failed). Event replay safe.

**4. Vote Credit Grant** (FR19–FR22) — After webhook confirms payment, credit via existing `POST /api/VoteAccount/reward-for-user`. Transactional with purchase completion.

**5. Purchase History** (FR23–FR26) — Fan-facing order history. Simple read via `GET /api/store/purchases`.

**6. Admin Remediation** (FR27–FR31) — Admin app handles failed payment investigation; manual credit grants via existing admin endpoints.

### NFRs Driving Architecture

- **IPaymentProvider abstraction** — Interface in Application layer; StripePaymentProvider in Infrastructure. Enables future PayPal/Apple Pay addition without touching domain logic.
- **No card data on server** — PCI DSS compliance; all sensitive data handled client-side by Stripe.js
- **Idempotent webhooks** — Deduplication table or unique constraint on `stripe_event_id`
- **Atomic purchase state** — EF Core transactions; never partial credit grants
- **Audit trail** — `PurchaseTransaction` table records all state changes with timestamps

### Cross-Cutting Concerns

- **Org scoping** — All purchases scoped to `organizationId` (existing pattern from roaster/vote features)
- **Auth boundary** — Webhook endpoint exempted from JWT middleware; all other store endpoints require auth
- **Three-app structure** — `sports-ui` (fans): catalog + checkout + history; `sports-admin` (admin): failed payment remediation
- **NSwag** — New endpoints must be reflected in OpenAPI spec for auto-generated client regeneration

---

## Starter Template Evaluation

### Primary Technology Domain

**Brownfield Full-Stack Extension** — No new project initialization required. The Fan Store feature extends the existing Nx monorepo with new libraries and backend bounded context following established conventions.

### Foundation: Existing Monorepo

**Selected Foundation**: Existing Nx + Angular 20 + .NET 8 monorepo (brownfield extension)

**Rationale**: The project already provides Angular 20 standalone components, NgRx Signals Store, NSwag API client generation, .NET 8 Clean Architecture with MediatR/CQRS, EF Core with SQL Server, and JWT auth middleware. All Fan Store components slot directly into these established patterns.

### New Packages Required

| Package | Version | Layer | Purpose |
|---|---|---|---|
| `Stripe.net` | 50.3.0 | .NET Infrastructure | PaymentIntent creation, webhook signature verification |
| `@stripe/stripe-js` | 8.8.0 | Angular frontend | Stripe Elements embedded card form, client-side tokenization |

```bash
# Backend
dotnet add package Stripe.net --version 50.3.0

# Frontend
npm install @stripe/stripe-js@8.8.0
```

### Architectural Decisions Provided by Foundation

**Language & Runtime**: TypeScript (frontend), C# 12 / .NET 8 (backend) — unchanged

**Styling Solution**: Component-scoped CSS — matches all existing `libs/ui` components; no utility-class framework

**Build Tooling**: Nx 21 with Angular CLI under the hood; existing `project.json` targets apply to new libraries

**Testing Framework**: Jest (frontend unit), Playwright (e2e) — existing configuration inherited by new libraries

**Code Organization — New Nx Libraries:**
```
libs/
  store/
    data-access/          ← NgRx Signals Store (StoreStore), NSwag models, HTTP service
    feature-store/        ← Fan-facing bundle catalog + Stripe checkout page
```

**Code Organization — New Backend Bounded Context:**
```
services/sportsAPI/
  Domain/Store/           ← Bundle, Purchase, PurchaseStatus entities
  Application/Store/      ← IPaymentProvider interface, CreatePurchase command, GetBundles query
  Infrastructure/Store/   ← StripePaymentProvider, WebhookProcessor
  WebAPI/Controllers/     ← StoreController (auth-required), WebhookController (JWT-exempt)
```

**Development Experience**: Existing Nx serve/build targets, hot reload, NSwag regeneration on build — no new tooling required

**Note:** New Nx libraries should be scaffolded with `nx g @nx/angular:library` following existing library conventions before feature implementation begins.

---

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**
- Payment flow timing: PaymentIntent created on "Buy Now" click
- Webhook deduplication: `ProcessedWebhookEvents` table with unique constraint on `stripe_event_id`
- IPaymentProvider abstraction: Interface in Application layer, StripePaymentProvider in Infrastructure

**Important Decisions (Shape Architecture):**
- Bundle catalog management: Database-seeded via EF Core migration for MVP
- Stripe key storage: Azure App Service Environment Variables (migrate to Key Vault post-MVP)
- Bundle catalog caching: No cache for MVP; add `IMemoryCache` when perf demands it

**Deferred Decisions (Post-MVP):**
- Additional payment providers (PayPal, Apple Pay, Google Pay) via IPaymentProvider
- Admin UI for bundle management (Phase 2)
- Azure Key Vault for Stripe secrets (Phase 2 hardening)
- Bundle catalog caching (when read volume warrants it)

### Data Architecture

**Aggregate Roots — Store Domain**
All Store entities extend `Aggregate<Guid>` (domain event support via `AddDomainEvent` / `ClearDomainEvents`):
- `Purchase : Aggregate<Guid>` — ✓ already exists
- `Product : Aggregate<Guid>` — ✓ already exists (`ProductType.Votes` = vote bundle)
- `ProcessedWebhookEvent : Aggregate<Guid>` — 🆕 new

All data access goes exclusively through injected `IRepository` — never `_db.[Table]` directly.

**Purchase State Machine**
- States: `Pending` → `Paid` → `Fulfilled` | `Failed` (existing `PurchaseStatus` + `FulfillmentStatus` enums)
- `Purchase.MarkPaid(ExternalPaymentId)` — called on webhook `payment_intent.succeeded`
- `Purchase.MarkFulfilled()` — called after votes credited; throws if not `Paid`
- `Purchase.MarkFailed()` — called on webhook `payment_intent.payment_failed`
- Transitions atomic via EF Core transactions; no partial state

**Webhook Deduplication**
- `ProcessedWebhookEvent` aggregate: `StripeEventId` (unique constraint), `EventType`, `ProcessedAt`
- On webhook receipt: check via `IRepository.ExistsAsync` → skip if exists → process → insert via `IRepository.AddAsync` (atomic)
- Rationale: Clean audit log, separates idempotency from purchase record, safe for Stripe's 3-day retry window

**Bundle Catalog — `Product` entity (existing)**
- Fields: `Id`, `Name`, `Description`, `Type=Votes`, `Quantity` (vote count), `Money Price`, `IsActive`
- `PaymentProvider` enum: `Stripe=0, PayPal=1, Square=2` — already defined
- Seeded via existing `SeedData.cs` (add `ProductType.Votes` Products)
- No caching for MVP — small table, infrequent reads

**EF Core Migration Approach**
- `ProductConfiguration.cs`, `PurchaseConfiguration.cs` — 🆕 EF mappings for existing domain entities
- `ProcessedWebhookEventConfiguration.cs` — 🆕 new
- One new migration appended after `20260101_rewarditempromocode`
- Tables: `Products`, `Purchases`, `ProcessedWebhookEvents`

### Authentication & Security

**Store Endpoints**: JWT bearer required — inherits existing `[Authorize]` middleware

**Webhook Endpoint**: `[AllowAnonymous]` + Stripe signature verification via `Stripe.net` `ConstructEvent()`
- Stripe-Signature header validated against `STRIPE_WEBHOOK_SECRET` env var
- Reject any request failing signature check with 400

**Stripe Key Storage (MVP)**
- `STRIPE_SECRET_KEY` → Azure App Service Application Settings
- `STRIPE_WEBHOOK_SECRET` → Azure App Service Application Settings
- `STRIPE_PUBLISHABLE_KEY` → Angular environment config (safe to expose client-side)

**PCI DSS Compliance**
- No card data ever touches the server — Stripe Elements handles collection and tokenization
- Server only receives `paymentIntentId` to confirm; Stripe.js sends card data directly to Stripe

### API & Communication Patterns

**Payment Flow (On "Buy Now" Click)**
1. Fan selects bundle → clicks "Buy Now"
2. `POST /api/store/purchases` → backend creates `Purchase` (Pending) + Stripe PaymentIntent → returns `{ purchaseId, clientSecret }`
3. Angular mounts Stripe Elements with `clientSecret` → fan enters card → `stripe.confirmCardPayment()`
4. On client confirmation → poll or show "processing" state
5. Stripe webhook fires `payment_intent.succeeded` → backend: deduplication check → mark Purchase Completed → credit votes via `POST /api/VoteAccount/reward-for-user` → insert `ProcessedWebhookEvents` record

**New REST Endpoints**
| Method | Path | Auth | Purpose |
|---|---|---|---|
| `GET` | `/api/store/bundles` | JWT | List active vote bundles |
| `POST` | `/api/store/purchases` | JWT | Create purchase + PaymentIntent |
| `GET` | `/api/store/purchases` | JWT | Fan's purchase history |
| `POST` | `/api/store/webhook` | None (sig verify) | Stripe webhook receiver |

**Error Handling**: Follow existing `ProblemDetails` format; Stripe errors mapped to appropriate HTTP status codes

### Frontend Architecture

**NgRx Signals Store — `StoreSignalStore`**
```typescript
// libs/store/data-access
{
  bundles: VoteBundleDto[],       // catalog
  selectedBundleId: string | null,
  purchaseStatus: 'idle' | 'creating' | 'confirming' | 'success' | 'error',
  clientSecret: string | null,
  errorMessage: string | null,
  purchaseHistory: PurchaseDto[]
}
```

**Stripe Elements Integration**
- Load `@stripe/stripe-js` once at app bootstrap via `loadStripe(publishableKey)`
- Mount `CardElement` inside the checkout component after `clientSecret` received
- Call `stripe.confirmCardPayment(clientSecret)` on form submit
- No Angular wrapper library needed — direct Stripe.js API

**Routing**
- New route `/store` in `sports-ui` fan app
- Route guard: authenticated fans only
- Child views: `/store` (catalog) → checkout modal/overlay (same route, modal pattern)

### Infrastructure & Deployment

**Stripe Webhook Registration**
- Production: Registered in Stripe Dashboard pointing to `https://api.yourdomain.com/api/store/webhook`
- Local dev: Stripe CLI `stripe listen --forward-to localhost:5000/api/store/webhook`

**Environment Configuration**
- Backend env vars: `STRIPE_SECRET_KEY`, `STRIPE_WEBHOOK_SECRET` in Azure App Service Settings
- Frontend env vars: `STRIPE_PUBLISHABLE_KEY` in Angular `environment.ts` (per existing pattern)

**No new infrastructure required for MVP** — slots into existing Azure App Service + Static Web Apps deployment

### Decision Impact Analysis

**Implementation Sequence (drives story ordering):**
1. DB migration — `VoteBundles`, `Purchases`, `ProcessedWebhookEvents` tables
2. Backend domain + Application layer — entities, IPaymentProvider, commands/queries
3. Backend Infrastructure — StripePaymentProvider, WebhookProcessor
4. Backend API — StoreController, WebhookController + Stripe sig verification
5. Frontend data-access — NgRx Signals Store, NSwag client regeneration
6. Frontend feature — bundle catalog UI, checkout flow with Stripe Elements
7. End-to-end testing — happy path + webhook replay

**Cross-Component Dependencies:**
- Frontend checkout depends on backend `POST /api/store/purchases` returning `clientSecret`
- Webhook processor depends on `VoteAccount.reward-for-user` endpoint (existing — no change needed)
- NSwag client must be regenerated after backend endpoints added
- Stripe webhook secret must be configured before webhook endpoint is live

---

## Implementation Patterns & Consistency Rules

### Domain & Data Access — MANDATORY

**All Store entities MUST extend `Aggregate<Guid>`**
```csharp
// ✅ Correct
public class Purchase : Aggregate<Guid> { … }
public class VoteBundle : Aggregate<Guid> { … }
public class ProcessedWebhookEvent : Aggregate<Guid> { … }

// ❌ Wrong — plain class bypasses domain event infrastructure
public class Purchase { … }
```

**All data access MUST go through injected `IRepository` — never `_db.[Table]`**
```csharp
// ✅ Correct — handler receives IRepository via constructor injection
public class GetBundlesQueryHandler(IRepository repository)
{
    public async Task<IReadOnlyList<VoteBundle>> Handle(GetBundlesQuery q, CancellationToken ct)
        => await repository.ListAsync<VoteBundle>(
               filter: b => b.IsActive && b.OrganizationId == q.OrganizationId,
               orderBy: q => q.OrderBy(b => b.DisplayOrder),
               ct: ct);
}

// ❌ Wrong — bypasses IRepository entirely
public class GetBundlesQueryHandler(SportsDbAppContext db)
{
    public async Task Handle(…) => await db.VoteBundles.Where(…).ToListAsync();
}
```

**Webhook idempotency guard — `ExistsAsync`, not `_db` directly**
```csharp
// ✅ Correct
if (await _repository.ExistsAsync<ProcessedWebhookEvent>(
        e => e.StripeEventId == stripeEventId, ct))
    return; // silently skip — already processed

// ❌ Wrong
if (await _db.ProcessedWebhookEvents.AnyAsync(e => e.StripeEventId == stripeEventId))
    return;
```

**Adding + saving aggregates**
```csharp
// ✅ Correct
await _repository.AddAsync(purchase, ct);
await _repository.SaveChangesAsync(ct);

// ❌ Wrong
_db.Purchases.Add(purchase);
await _db.SaveChangesAsync();
```

**Composable queries via `Query<TAgg>()`**
```csharp
// ✅ Correct — AsNoTracking by default, fully composable
var history = await _repository
    .Query<Purchase>()
    .Where(p => p.UserId == userId && p.OrganizationId == orgId)
    .OrderByDescending(p => p.CreatedAt)
    .ToListAsync(ct);
```

### Naming Patterns

**Database (EF Core)**
- Tables: `PascalCase` plural — `VoteBundles`, `Purchases`, `ProcessedWebhookEvents`
- Columns: `PascalCase` — `StripePaymentIntentId`, `OrganizationId`, `CreatedAt`
- Foreign keys: `{Entity}Id` — `UserId`, `BundleId`
- Migrations: `{timestamp}_{PascalCaseDescription}` — e.g. `20260228_AddStoreTables`

**API endpoints**
- Plural resource nouns, kebab-case segments: `/api/store/bundles`, `/api/store/purchases`
- Route params: `{id}` C# convention; query params `camelCase`
- JSON fields: `camelCase` (ASP.NET `JsonSerializerOptions` default)

**Backend (C#)**
- Classes/interfaces: `PascalCase` — `IPaymentProvider`, `StripePaymentProvider`, `CreatePurchaseCommand`
- Methods: `PascalCase` — `CreatePaymentIntentAsync`, `ProcessWebhookAsync`
- Private fields: `_camelCase` — `_repository`, `_stripeClient`

**Frontend (Angular/TypeScript)**
- Component files: `kebab-case.ts` — `feature-store.ts`, `bundle-card.ts`
- Component selectors: `lib-*` — `<lib-bundle-card>`, `<lib-feature-store>`
- Classes/interfaces: `PascalCase` — `StoreSignalStore`, `VoteBundleDto`
- Signal store methods: `camelCase` — `selectBundle()`, `createPurchase()`
- CSS classes: `kebab-case` — `.bundle-card`, `.checkout-overlay`

### Structure Patterns

**Backend — Clean Architecture layer rules**
- `Domain/Store/` — aggregates + value objects only; zero external dependencies; no EF or Stripe imports
- `Application/Store/` — commands, queries, `IPaymentProvider` interface; injects `IRepository`; no Stripe SDK
- `Infrastructure/Store/` — only layer that imports `Stripe.net`; implements `IPaymentProvider`; no business logic
- `WebAPI/Controllers/` — thin; delegates to MediatR; no `IRepository` injection here

**Frontend — Nx library rules**
- `libs/store/data-access/` — signal store + HTTP service; no UI components
- `libs/store/feature-store/` — page components that compose UI; no raw HTTP calls
- `libs/ui/` — shared dumb components; no store dependencies
- Cross-library imports via `index.ts` barrel exports only

**Test placement**
- Angular: `*.spec.ts` co-located with source file
- .NET: `sportsAPI.Tests/Store/` mirroring source structure

### Format Patterns

**API responses** — direct object/array; no wrapper envelope
```json
// ✅ [{ "id": "…", "name": "100 Votes", "priceUsd": 4.99 }]
// ❌ { "data": […], "success": true }
```

**Errors** — RFC 7807 `ProblemDetails`: `{ "title": "…", "status": 400, "detail": "…" }`

**Money** — `decimal` in C#, `number` in TypeScript; Stripe always receives cents (`499` = $4.99); display layer formats

**Dates** — ISO 8601 UTC strings everywhere: `"2026-02-28T12:00:00Z"`

### NgRx Signals Store Patterns

- All state mutations via `withMethods` — never mutate signals outside the store
- Status field: `'idle' | 'creating' | 'confirming' | 'success' | 'error'` string literal union
- Derived state via `withComputed` — never recompute in components
- Purchase flow states must progress sequentially: `idle → creating → confirming → success | error`

### Enforcement — All Agents MUST

- `Purchase`, `Product`, `ProcessedWebhookEvent` extend `Aggregate<Guid>`
- All handlers inject `IRepository`; zero direct `_db.[Table]` access anywhere in Application layer
- Stripe.net imported only inside `Infrastructure/Store/`
- `@stripe/stripe-js` imported only inside `libs/store/data-access/` or `libs/store/feature-store/`
- `OrganizationId` filter applied on every data query returning user-facing data
- Stripe webhook signature verified before accessing any request body data
- `_repository.ExistsAsync<ProcessedWebhookEvent>` checked before crediting votes — idempotency is non-negotiable

---

## Project Structure & Boundaries

### Complete Project Directory Structure

Legend: ✓ = already exists | 🆕 = new file to create

```
services/sportsAPI/
│
├── Domain/
│   ├── Abstractions/               ✓ Aggregate<T>, IAggregate, Entity, IDomainEvent
│   ├── Enums/
│   │   ├── PaymentProvider.cs      ✓ Stripe=0, PayPal=1, Square=2
│   │   ├── ProductType.cs          ✓ Votes=0, Subscription=1, Cosmetic=2
│   │   ├── PurchaseStatus.cs       ✓ Pending=0, Paid=1, Failed=2
│   │   └── FulfillmentStatus.cs    ✓ NotStarted=0, Completed=1
│   ├── Product/
│   │   └── Product.cs              ✓ Aggregate<Guid> — IS the vote bundle
│   ├── Purchase/
│   │   ├── Purchase.cs             ✓ Aggregate<Guid> — full state machine
│   │   └── ProcessedWebhookEvent.cs  🆕 Aggregate<Guid> — idempotency record
│   ├── ValueObjects/
│   │   ├── Money.cs                ✓ (decimal Amount, string Currency)
│   │   ├── PurchaseItem.cs         ✓ (string Type, int Quantity)
│   │   ├── ExternalPaymentId.cs    ✓
│   │   └── ExternalSessionId.cs    ✓
│   └── Repositories/
│       └── IRepository.cs          ✓ generic data access interface
│
├── Application/
│   ├── Common/
│   │   └── Interfaces/
│   │       └── IPaymentProvider.cs   🆕 CreatePaymentIntentAsync, VerifyWebhookSignature
│   ├── Dto/Store/
│   │   ├── ProductDto.cs             🆕
│   │   ├── PurchaseDto.cs            🆕
│   │   ├── CreatePurchaseRequest.cs  🆕 { ProductId, OrganizationId }
│   │   └── CreatePurchaseResponse.cs 🆕 { PurchaseId, ClientSecret }
│   └── Store/
│       ├── Commands/
│       │   ├── CreatePurchase/
│       │   │   ├── CreatePurchaseCommand.cs          🆕
│       │   │   ├── CreatePurchaseCommandHandler.cs   🆕
│       │   │   └── CreatePurchaseCommandValidator.cs 🆕
│       │   └── ProcessWebhook/
│       │       ├── ProcessWebhookCommand.cs          🆕
│       │       └── ProcessWebhookCommandHandler.cs   🆕
│       └── Queries/
│           ├── GetProducts/
│           │   ├── GetProductsQuery.cs               🆕
│           │   └── GetProductsQueryHandler.cs        🆕
│           └── GetPurchaseHistory/
│               ├── GetPurchaseHistoryQuery.cs        🆕
│               └── GetPurchaseHistoryQueryHandler.cs 🆕
│
├── Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   │   ├── ProductConfiguration.cs               🆕 EF mapping for existing entity
│   │   │   ├── PurchaseConfiguration.cs              🆕 EF mapping for existing entity
│   │   │   └── ProcessedWebhookEventConfiguration.cs 🆕
│   │   ├── SportsDbAppContext.cs                     ✓ add 3 new DbSets
│   │   └── SeedData.cs                              ✓ add ProductType.Votes entries
│   ├── Migrations/
│   │   └── {timestamp}_AddStoreTables.cs             🆕 Products, Purchases, ProcessedWebhookEvents
│   └── Store/
│       └── StripePaymentProvider.cs                  🆕 implements IPaymentProvider
│
└── WebAPI/
    ├── Controllers/
    │   ├── StoreController.cs          🆕 [Authorize] GET /bundles, POST /purchases, GET /purchases
    │   └── WebhookController.cs        🆕 [AllowAnonymous] POST /webhook + sig verify
    └── Program.cs                      ✓ register IPaymentProvider → StripePaymentProvider


libs/
│
├── store/                              🆕 new Nx domain
│   ├── data-access/
│   │   └── src/
│   │       ├── lib/
│   │       │   ├── store.signal-store.ts   🆕 NgRx Signals Store
│   │       │   └── store.service.ts        🆕 NSwag client wrapper
│   │       └── index.ts                    🆕 barrel export
│   └── feature-store/
│       └── src/
│           ├── feature-store/
│           │   ├── feature-store.ts        🆕 page component
│           │   ├── feature-store.html      🆕
│           │   └── feature-store.css       🆕
│           └── index.ts                    🆕
│
└── ui/src/components/
    ├── bundle-card/                    🆕 dumb card (accepts ProductDto input)
    │   ├── bundle-card.ts
    │   ├── bundle-card.html
    │   └── bundle-card.css
    └── checkout-overlay/               🆕 Stripe Elements mount point
        ├── checkout-overlay.ts
        ├── checkout-overlay.html
        └── checkout-overlay.css
```

### Requirements → Structure Mapping

| FR Category | Backend Location | Frontend Location |
|---|---|---|
| FR1–5 Bundle Catalog | `GetProductsQueryHandler`, `StoreController`, `ProductConfiguration`, `SeedData` | `store.signal-store` (bundles), `bundle-card` component |
| FR6–14 Stripe Checkout | `CreatePurchaseCommandHandler`, `IPaymentProvider`, `StripePaymentProvider` | `checkout-overlay` (Stripe Elements), signal store purchase flow |
| FR15–18 Webhook | `ProcessWebhookCommandHandler`, `ProcessedWebhookEvent`, `WebhookController` | — backend only |
| FR19–22 Vote Credit | `ProcessWebhookCommandHandler` → existing `reward-for-user` endpoint | Signal store `success` state |
| FR23–26 Purchase History | `GetPurchaseHistoryQueryHandler`, `StoreController` GET /purchases | `feature-store` history section |
| FR27–31 Admin | Existing admin endpoints (no MVP changes) | `sports-admin` app (Phase 2) |

### Integration Points & Data Flow

**Purchase creation flow**
```
Fan clicks "Buy" → POST /api/store/purchases
  → CreatePurchaseCommandHandler
  → IRepository.GetByIdAsync<Product>(productId)
  → IPaymentProvider.CreatePaymentIntentAsync(amount, currency)
  → Purchase.Create(...)  +  Purchase.AttachStripeSession(...)
  → IRepository.AddAsync<Purchase>
  → IRepository.SaveChangesAsync
  → returns { purchaseId, clientSecret }
  → Angular: stripe.confirmCardPayment(clientSecret)
```

**Webhook fulfillment flow**
```
Stripe → POST /api/store/webhook
  → WebhookController verifies Stripe-Signature
  → send ProcessWebhookCommand via MediatR
  → ProcessWebhookCommandHandler:
      IRepository.ExistsAsync<ProcessedWebhookEvent>(stripeEventId)  ← idempotency check
      IRepository.Query<Purchase>().Where(p => p.ExternalSessionId == sessionId)
      Purchase.MarkPaid(paymentIntentId)
      HTTP POST /api/VoteAccount/reward-for-user
      Purchase.MarkFulfilled()
      IRepository.AddAsync<ProcessedWebhookEvent>(...)
      IRepository.SaveChangesAsync()   ← atomic commit
```

**NSwag regeneration boundary**
- After `StoreController` + `WebhookController` added → run NSwag → regenerates TypeScript client
- `store.service.ts` consumes auto-generated `StoreClient` / `ProductDto` / `PurchaseDto` types

---

## Architecture Validation Results

### Coherence Validation ✅

**Decision Compatibility** — All compatible:
- Angular 20 + `@stripe/stripe-js` 8.8.0 — no conflicts
- .NET 8 + `Stripe.net` 50.3.0 — full .NET 8 support confirmed
- `IPaymentProvider` placed in `Application/Common/Interfaces/` — matches existing `IVotingService`, `IQrCodeGenerator`, `IUserDirectory` location pattern
- `[AllowAnonymous]` webhook endpoint pattern — already proven in codebase
- `Aggregate<Guid>` + `IRepository` — zero new abstractions; uses existing infrastructure

**Pattern Consistency** — Clean:
- `Product`, `Purchase`, `ProcessedWebhookEvent` all extend `Aggregate<Guid>` ✅
- All handler examples use `IRepository` injection, no `_db.[Table]` ✅
- `PascalCase` C# / `camelCase` JSON / `kebab-case` Angular — consistent ✅

**Structure Alignment** — Corrected during validation:
- `IPaymentProvider` location corrected from `Application/Store/` → `Application/Common/Interfaces/` to match existing interface conventions ✅

### Requirements Coverage ✅

| FR Range | Architectural Support | Status |
|---|---|---|
| FR1–5 Bundle Catalog | `GetProductsQueryHandler`, `ProductConfiguration`, `SeedData`, `StoreController` | ✅ |
| FR6–14 Stripe Checkout | `CreatePurchaseCommandHandler`, `IPaymentProvider`, `StripePaymentProvider`, Stripe Elements | ✅ |
| FR15–18 Webhook | `WebhookController`, `ProcessWebhookCommandHandler`, `ProcessedWebhookEvent` | ✅ |
| FR19–22 Vote Credit | `MarkPaid()` → `MarkFulfilled()` → existing `reward-for-user` | ✅ |
| FR23–26 Purchase History | `GetPurchaseHistoryQueryHandler`, `StoreController` GET | ✅ |
| FR27–31 Admin Remediation | Deferred Phase 2 — existing admin app handles | ✅ scoped |

**NFR coverage:**
- PCI DSS — Stripe Elements, zero card data on server ✅
- `IPaymentProvider` extensibility — interface in Application layer, Stripe is sole MVP impl ✅
- Idempotency — `ProcessedWebhookEvent` + `ExistsAsync` guard before every credit ✅
- Atomic state — `MarkPaid` → credit → `MarkFulfilled` → `SaveChangesAsync` one transaction ✅
- Org scoping — `OrganizationId` filter enforced on all user-facing queries ✅
- Audit trail — `Purchase` state history + `ProcessedWebhookEvents` table ✅

### Gap Analysis & Resolutions

**Gap 1 — `IPaymentProvider` interface contract** (resolved)

```csharp
// Application/Common/Interfaces/IPaymentProvider.cs
public interface IPaymentProvider
{
    Task<(string ClientSecret, string PaymentIntentId)> CreatePaymentIntentAsync(
        decimal amount, string currency, Guid purchaseId, CancellationToken ct = default);

    bool VerifyWebhookSignature(
        string payload, string signatureHeader, out Stripe.Event stripeEvent);
}
```

**Gap 2 — `ProcessedWebhookEvent` factory pattern** (resolved)

Follows existing `Purchase.Create(...)` static factory convention:

```csharp
// Domain/Purchase/ProcessedWebhookEvent.cs
public sealed class ProcessedWebhookEvent : Aggregate<Guid>
{
    public string StripeEventId { get; private set; } = default!;
    public string EventType { get; private set; } = default!;
    public DateTime ProcessedAt { get; private set; }

    private ProcessedWebhookEvent() { }

    public static ProcessedWebhookEvent Record(string stripeEventId, string eventType)
        => new() { Id = Guid.NewGuid(), StripeEventId = stripeEventId,
                   EventType = eventType, ProcessedAt = DateTime.UtcNow };
}
```

**Gap 3 — DI registration in `Program.cs`** (resolved)

```csharp
// Program.cs additions
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<IPaymentProvider, StripePaymentProvider>();
// StripeOptions: SecretKey, WebhookSecret, PublishableKey
```

Webhook route must be excluded from the global JWT policy — add before `UseAuthorization()`:
```csharp
app.MapControllers();  // WebhookController carries [AllowAnonymous] — no extra config needed
```

### Architecture Completeness Checklist

**✅ Context & Analysis**
- [x] Brownfield project classification confirmed
- [x] Existing domain skeleton discovered and mapped (Purchase, Product, enums all pre-built)
- [x] Technical constraints identified (IRepository, Aggregate<Guid>, NSwag)
- [x] Cross-cutting concerns mapped (org scoping, auth boundary, idempotency)

**✅ Architectural Decisions**
- [x] Payment flow timing: PaymentIntent on "Buy Now" click
- [x] Webhook deduplication: `ProcessedWebhookEvent` table via `IRepository`
- [x] Bundle catalog: `Product` entity (existing), `ProductType.Votes`, seeded via `SeedData.cs`
- [x] Stripe key storage: App Service environment variables (MVP)
- [x] `IPaymentProvider` abstraction: interface in Application, Stripe impl in Infrastructure

**✅ Implementation Patterns**
- [x] `Aggregate<Guid>` mandatory for all entities
- [x] `IRepository` only — zero `_db.[Table]` access
- [x] Naming conventions: PascalCase/camelCase/kebab-case per layer
- [x] Concrete code examples provided for all critical patterns
- [x] Anti-patterns explicitly documented

**✅ Project Structure**
- [x] All files named with ✓ (existing) / 🆕 (new) markers
- [x] FR categories mapped to specific files
- [x] Both purchase creation and webhook fulfillment flows diagrammed
- [x] NSwag regeneration boundary documented

### Architecture Readiness Assessment

**Overall Status: READY FOR IMPLEMENTATION**

**Confidence Level: High**

**Key Strengths:**
- Domain layer (Purchase, Product, enums) is substantially pre-built — reduces implementation risk
- IRepository + Aggregate pattern is already proven across the codebase — no new abstractions to learn
- PCI DSS compliance is architectural (Stripe Elements) not procedural — cannot be accidentally broken
- Idempotency is enforced at architecture level with explicit code examples

**Areas for Future Enhancement (Post-MVP):**
- Azure Key Vault migration for Stripe secrets
- Bundle catalog admin UI (Phase 2)
- Additional `IPaymentProvider` implementations (PayPal, Apple Pay)
- `IMemoryCache` for bundle catalog when read volume warrants

### Implementation Handoff

**First implementation story:** EF Core configurations + migration (`ProductConfiguration`, `PurchaseConfiguration`, `ProcessedWebhookEventConfiguration`, seed data, `SportsDbAppContext` DbSet additions)

**Implementation sequence:**
1. EF configs + migration + seed Products
2. `ProcessedWebhookEvent` aggregate + `IPaymentProvider` interface
3. Application commands/queries + validators
4. `StripePaymentProvider` + DI registration
5. `StoreController` + `WebhookController`
6. NSwag regeneration
7. `libs/store/data-access` (signal store + service)
8. `libs/store/feature-store` + `bundle-card` + `checkout-overlay` UI components

**AI Agent Guidelines:**
- Always consult this document before making structural decisions
- `IRepository` is the only path to data — no exceptions
- `Aggregate<Guid>` is required for every persistable entity
- Stripe.net touches only `Infrastructure/Store/StripePaymentProvider.cs`
- `OrganizationId` scoping is non-negotiable on all data access

---
