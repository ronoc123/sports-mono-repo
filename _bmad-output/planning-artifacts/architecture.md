---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments:
  - '_bmad-output/planning-artifacts/prd.md'
  - 'docs/architecture.md'
  - 'docs/api-contracts-backend.md'
  - 'docs/data-models-backend.md'
  - 'docs/state-management-frontend.md'
  - 'docs/component-inventory-frontend.md'
  - 'docs/integration-architecture.md'
  - 'docs/source-tree-analysis.md'
  - 'docs/development-guide.md'
workflowType: 'architecture'
lastStep: 8
status: 'complete'
completedAt: '2026-03-06'
project_name: 'sports-ui'
user_name: 'Kampe'
date: '2026-03-06'
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**

32 FRs across 6 capability areas drive the following net-new architectural components:

| Capability Area | FR Count | Architectural Implication |
|---|---|---|
| Dashboard (fan) | 5 | New query: PlayerOptions sorted by vote count; dashboard aggregation endpoint |
| Trivia — fan experience | 6 | New read model + answer submission + idempotency guard |
| Polls — fan experience | 3 | New read model + vote submission + idempotency guard |
| GM trivia management | 9 | New domain aggregate: `TriviaSeries` + `TriviaQuestion` with status state machine |
| GM poll management | 4 | New domain aggregate: `Poll` + `PollOption` |
| Vote economy — trivia earning | 3 | Integration with existing `VoteAccount`/`VoteTransaction`; new credit trigger |
| Player options feed | 2 | Sort extension on existing `PlayerOption` query |

**Non-Functional Requirements:**

| NFR | Architectural Impact |
|---|---|
| Dashboard load <2s | Dashboard endpoint must be a single aggregated query, not N+1 fetches |
| Trivia/poll submission <500ms | Answer and vote commands must be lightweight (no heavy domain graph loading) |
| Trivia idempotency | `TriviaAnswer` table keyed on `(UserId, TriviaQuestionId)` — unique constraint + upsert-safe |
| Poll idempotency | `PollVote` table keyed on `(UserId, PollId)` — unique constraint |
| Vote credit server-side only | Credit amount stored on `TriviaQuestion`; client cannot influence reward value |
| GMOnly policy enforcement | New controllers use `[Authorize(Policy = "GMOnly")]`; fan endpoints use `[Authorize]` |
| DB scalability | New tables need composite indexes on `(OrgId, Status)` for dashboard queries and `(UserId, QuestionId/PollId)` for idempotency checks |

**Scale & Complexity:**

- Primary domain: Full-stack brownfield (Angular 20 SPA + .NET 8 Clean Architecture)
- Complexity level: Medium — new domain aggregates, vote economy integration, status state machine
- Net-new backend entities: 6 (TriviaSeries, TriviaQuestion, TriviaAnswer, Poll, PollOption, PollVote)
- Net-new API controllers: 2–3 (Dashboard, Trivia, Poll)
- Net-new Angular libs: 2–3 (trivia-data-access, poll-data-access, dashboard rework)

### Technical Constraints & Dependencies

- **Existing vote economy**: Trivia credits must flow through `VoteTransaction.ForRewardCredit()` factory method — no direct balance mutation
- **Existing org-scoping pattern**: All new entities scoped to `OrganizationId`; routes follow `/:organizationId` convention
- **EF Core code-first**: New entities require a migration (`AddDashboardEngagementFeatures` or similar)
- **Clean Architecture layers**: Domain → Application → Infrastructure → WebAPI; no layer skipping
- **Angular Nx monorepo**: New features go in `libs/<domain>/` following `data-access` / `feature-*` / `ui` slice pattern
- **SignalR not required for MVP**: Dashboard features poll on load; live push deferred to Phase 2

### Cross-Cutting Concerns Identified

1. **Auth/Authorization**: GM management endpoints (`GMOnly`), fan submission endpoints (`[Authorize]` + org membership), read-only dashboard endpoints (`[Authorize]`)
2. **Idempotency**: Both trivia answers and poll votes require server-side deduplication tables with unique constraints — critical for vote economy integrity
3. **Vote economy integration**: `TriviaQuestion` carries `VoteRewardAmount`; answer command handler reads it and credits via existing `VoteAccount` aggregate
4. **Org scoping**: Every new query and command carries `OrganizationId`; repositories filter by it
5. **Status state machine**: `TriviaQuestion` has three states (Pending → Active → Archived); only Active questions surface in fan dashboard queries
6. **Dashboard aggregation**: Fan dashboard needs one efficient endpoint returning trending player options + active trivia questions + active poll — avoid N+1 and chatty API patterns

## Technical Foundation

### Primary Technology Domain

Full-stack brownfield extension — Angular 20 SPA (frontend) + .NET 8 Clean Architecture (backend). All new features extend the existing codebase; no new project initialization required.

### Existing Stack (Established — Non-Negotiable)

**Backend:**

| Layer | Technology | Version |
|---|---|---|
| Runtime | .NET / C# | 8 |
| Web framework | ASP.NET Core | 8 |
| ORM | EF Core code-first | 8 |
| CQRS | MediatR | latest |
| Validation | FluentValidation | latest |
| Message broker | MassTransit + RabbitMQ | latest |
| Database | SQL Server | 2022 |
| Auth | JWT Bearer (IdentityService) | — |
| Real-time | SignalR (existing hub) | — |

**Frontend:**

| Layer | Technology | Version |
|---|---|---|
| Framework | Angular | 20 |
| State management | NgRx Signals Store | 19 |
| Component library | Angular Material | — |
| Monorepo tooling | Nx | 21 |
| Language | TypeScript | 5.8 |
| Testing | Jest + Playwright | — |
| Change detection | Zoneless | — |

### Architectural Patterns New Features Must Follow

**Backend:**
- New domain entities extend `Entity` base class; aggregates implement `IAggregate`
- All business logic through MediatR `ICommand` / `IQuery` handlers
- Repository pattern via `IRepository<TAgg, TId>` — `GetByIdAsync`, `Query()`, `AddAsync()`, `SaveChangesAsync()`
- Controllers inject `IMediator` only; return `ServiceResponse<T>`
- Authorization via policy attributes: `[Authorize(Policy = "GMOnly")]` / `[Authorize]`
- EF Core migration for every schema change

**Frontend:**
- New feature libs at `libs/<domain>/<type>/` following `data-access` / `feature-*` / `ui` slices
- Signal stores use `status: 'idle' | 'loading' | 'success' | 'error'` shape
- Async methods use `rxMethod` + `switchMap` + `tapResponse`
- HTTP services extend/use `ApiService` from `@sports-ui/http-client`
- API URL pattern: `${environment.sportsApi}controller/action`

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (block implementation):**
- Domain aggregate boundaries for Trivia and Poll
- Dashboard API design (aggregated vs. separate endpoints)
- Idempotency enforcement mechanism
- Vote credit integration point

**Important Decisions (shape architecture):**
- Controller routing structure
- Frontend lib organization
- TriviaQuestion status state machine placement

**Deferred (Post-MVP):**
- SignalR push for live vote counts and poll results
- Dashboard caching strategy

### Data Architecture

**Domain Aggregate Boundaries**

| Aggregate | Root | Child Entities | Standalone Tables |
|---|---|---|---|
| `TriviaSeries` | `TriviaSeriesId` | `TriviaQuestion` (list, each with status + reward) | — |
| `Poll` | `PollId` | `PollOption` (list) | — |
| — | — | — | `TriviaAnswer` (userId + questionId + isCorrect) |
| — | — | — | `PollVote` (userId + pollId + optionId) |

`TriviaSeries` owns its questions (same lifecycle, GM manages them together). `TriviaAnswer` and `PollVote` are standalone tables — they cross aggregate boundaries and exist purely for idempotency and participation counting.

**Idempotency Enforcement**

Unique constraints at the DB level:
- `TriviaAnswer`: `UNIQUE (UserId, TriviaQuestionId)` — first insert wins; command handler checks existence before crediting votes
- `PollVote`: `UNIQUE (UserId, PollId)` — first insert wins

**Vote Credit Integration**

`SubmitTriviaAnswerCommand` handler flow:
1. Load `TriviaQuestion` via `TriviaSeries` aggregate
2. Check if `TriviaAnswer` already exists for `(userId, questionId)` → reject if exists
3. Evaluate answer correctness
4. Persist `TriviaAnswer` (captures result regardless of correctness)
5. If correct: load `VoteAccount` by `(userId, orgId)` → call `VoteTransaction.ForRewardCredit(question.VoteRewardAmount)` → `SaveChangesAsync()`

**EF Core Migration**

Single migration: `AddDashboardEngagementFeatures`
Adds: `TriviaSeries`, `TriviaQuestions`, `TriviaAnswers`, `Polls`, `PollOptions`, `PollVotes` tables with all indexes and unique constraints.

### API & Communication

**Dashboard Endpoint Design**

Single aggregated endpoint: `GET /api/dashboard/{organizationId}`

Returns one DTO containing:
- `TrendingPlayerOptions[]` — top N active options sorted by vote count descending
- `ActiveTriviaQuestions[]` — all Active questions with series label, answered state per user
- `ActivePoll` — current active poll with options, voted state per user

Single roundtrip satisfies <2s NFR; fan answered/voted state joined server-side against `userId` from JWT.

**Controller Routing**

| Controller | Base Route | Auth | Responsibility |
|---|---|---|---|
| `DashboardController` | `/api/dashboard` | `[Authorize]` | Fan dashboard aggregation |
| `TriviaController` | `/api/trivia` | Mixed | Fan answer submission + GM management |
| `PollController` | `/api/poll` | Mixed | Fan vote submission + GM management |

GM management actions use `[Authorize(Policy = "GMOnly")]`; fan submission actions use `[Authorize]`.

### Frontend Architecture

**Nx Library Structure**

| Lib | Path | Contents |
|---|---|---|
| `trivia-data-access` | `libs/trivia/trivia-data-access/` | `TriviaStore`, `TriviaApiService`, trivia API types |
| `poll-data-access` | `libs/poll/poll-data-access/` | `PollStore`, `PollApiService`, poll API types |
| Dashboard rework | `libs/dashboard/feature-dashboard/` | Rework existing dashboard feature component |
| GM trivia management | `apps/sports-gm/` (new route) | Trivia/poll CRUD pages in GM app |

**Dashboard Store Shape**

```typescript
DashboardStore {
  status: 'idle' | 'loading' | 'success' | 'error'
  trendingPlayerOptions: PlayerOptionSummary[]
  activeTriviaQuestions: TriviaQuestionViewModel[]  // includes answeredByMe, selectedAnswer
  activePoll: PollViewModel | null                  // includes votedByMe, selectedOptionId
}
```

Single store for the dashboard aggregation response. GM management views get their own stores.

### Infrastructure & Deployment

No new infrastructure decisions required. Existing Docker Compose stack, SQL Server, and deployment patterns cover all new features. SignalR push for live updates deferred to Phase 2.

## Implementation Patterns & Consistency Rules

### Naming Conventions

**Backend:**

| Artifact | Convention | Example |
|---|---|---|
| Domain entity class | PascalCase noun | `TriviaQuestion`, `PollVote` |
| Strongly-typed ID | `{Entity}Id` struct | `TriviaSeriesId`, `PollId` |
| Command | `{Verb}{Noun}Command` | `SubmitTriviaAnswerCommand`, `CreatePollCommand` |
| Query | `Get{Noun}Query` | `GetDashboardQuery`, `GetTriviaSereisQuery` |
| Handler | `{CommandOrQuery}Handler` | `SubmitTriviaAnswerCommandHandler` |
| DTO (response) | `{Noun}Response` | `DashboardResponse`, `TriviaQuestionResponse` |
| DTO (request param) | `{Noun}Request` | `CreateTriviaSeriesRequest` |
| Controller | `{Domain}Controller` | `DashboardController`, `TriviaController` |
| Repository interface | `I{Aggregate}Repository` | `ITriviaSeriesRepository`, `IPollRepository` |

**Frontend:**

| Artifact | Convention | Example |
|---|---|---|
| Signal store class | `{Domain}Store` | `TriviaStore`, `PollStore`, `DashboardStore` |
| HTTP service class | `{Domain}ApiService` | `TriviaApiService`, `PollApiService` |
| Feature component | `{Domain}Component` | `TriviaCardComponent`, `PollCardComponent` |
| API type interface | `{Noun}` (no suffix) | `TriviaQuestion`, `Poll`, `DashboardData` |
| Store state interface | `{Domain}State` | `DashboardState` |

### Structure Patterns

**New Backend Entity Checklist:**

1. `Domain/` — entity class extending `Entity<TId>` (or `Aggregate<TId>` for aggregate roots)
2. `Domain/` — strongly-typed ID struct
3. `Domain/Repositories/` — repository interface declaration in `Repositories.cs`
4. `Application/` — command(s) + query(ies) with handlers
5. `Application/` — FluentValidation validators for each command
6. `Application/` — response DTO(s)
7. `Infrastructure/Data/` — `DbSet<T>` in `SportsDbAppContext`
8. `Infrastructure/Data/` — `IApplicationDbContext` interface update
9. `Infrastructure/Repositories/` — concrete repository implementation
10. `Infrastructure/Migrations/` — EF Core migration
11. `WebAPI/Controllers/` — controller action(s)
12. `WebAPI/` — DI registration if needed

**New CQRS Handler Checklist:**

- Implement `ICommandHandler<TCommand, TResponse>` or `IQueryHandler<TQuery, TResponse>`
- Inject only the repository interfaces needed
- Validate preconditions (idempotency check, status check) before mutations
- Load via aggregate root (never bypass via direct table access)
- `SaveChangesAsync()` once at end — no mid-handler saves unless required by transaction semantics

**Angular Signal Store Template:**

```typescript
export const DomainStore = signalStore(
  { providedIn: 'root' },
  withState<DomainState>({
    status: 'idle' as Status,
    // domain state fields
  }),
  withComputed((store) => ({
    // derived signals
  })),
  withMethods((store, apiService = inject(DomainApiService)) => ({
    load: rxMethod<LoadParams>(
      pipe(
        tap(() => patchState(store, { status: 'loading' })),
        switchMap((params) =>
          apiService.get(params).pipe(
            tapResponse({
              next: (data) => patchState(store, { status: 'success', ...data }),
              error: () => patchState(store, { status: 'error' }),
            })
          )
        )
      )
    ),
  }))
);
```

### Format Patterns

**API Response Envelope:**

All controller actions return `ServiceResponse<T>`:
```json
{ "data": { ... }, "success": true, "message": null }
{ "data": null, "success": false, "message": "Error description" }
```

- JSON property names: `camelCase` (configured globally)
- Enums: serialized as strings (e.g., `"Active"`, `"Pending"`, `"Archived"`)
- Dates: ISO 8601 strings

### Process Patterns

**Error Handling:**
- Domain violations: throw `DomainException` (caught by global middleware → 400)
- Not found: return `ServiceResponse.Failure("not found")` or throw `NotFoundException`
- Auth failures: handled by ASP.NET Core policy middleware (403/401)
- Frontend: `tapResponse` `error` branch sets `status: 'error'`; `ErrorHandlerStore` handles global HTTP errors

**Idempotency Check-First Pattern:**

```csharp
// In command handler
var existing = await _triviaAnswerRepository.Query()
    .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.TriviaQuestionId == request.TriviaQuestionId);
if (existing is not null)
    return ServiceResponse<SubmitAnswerResponse>.Failure("Already answered");
```

Always check-then-insert at the application layer; DB unique constraint is the safety net, not the primary guard.

**TriviaQuestion Status Transitions:**

```
Pending → Active    (GM publishes)
Active  → Archived  (GM archives)
Pending → Archived  (GM discards without publishing)
```

No reverse transitions. State machine lives in the `TriviaQuestion` entity as domain methods: `Publish()`, `Archive()`.

### Enforcement Guidelines

**Mandatory rules (hard violations):**
- Never mutate `VoteAccount.Balance` directly — always via `VoteTransaction` factory methods
- Never load a child entity outside its aggregate root in command handlers
- Never skip FluentValidation — every command has a validator
- Never access `HttpContext` inside application layer — pass userId/orgId via command properties
- Never trust client-supplied reward amounts — read `VoteRewardAmount` from `TriviaQuestion` domain entity

**Anti-patterns to avoid:**
- `DbContext` injection in controllers (use repositories)
- `status: loading` boolean pattern in new stores (use string union)
- Multiple `SaveChangesAsync()` calls in a single handler
- N+1 queries in dashboard aggregation — use `.Include()` or projection queries
- Hardcoded organization IDs in queries — always filter by `OrganizationId` from route/claim

## Project Structure & Boundaries

### Complete Project Directory Structure — Net-New Files

This section shows only the **new and modified** files the Dashboard Engagement features introduce. Existing files are shown with `★ MODIFY` annotations; new files are unmarked.

```
sports-ui/
│
├── libs/
│   │
│   ├── trivia/                                         ← NEW domain
│   │   ├── trivia-data-access/
│   │   │   └── src/
│   │   │       ├── lib/
│   │   │       │   ├── trivia.store.ts                 # TriviaStore (GM management)
│   │   │       │   ├── trivia.api.ts                   # TriviaApiService
│   │   │       │   └── trivia.models.ts                # TS interfaces (TriviaSeries, TriviaQuestion, etc.)
│   │   │       └── index.ts
│   │   └── feature-trivia-management/
│   │       └── src/
│   │           ├── lib/
│   │           │   ├── trivia-management.component.ts  # GM series list + question mgmt
│   │           │   ├── trivia-series-form.component.ts # Create series modal/page
│   │           │   └── trivia-question-list.component.ts
│   │           └── index.ts
│   │
│   ├── poll/                                           ← NEW domain
│   │   ├── poll-data-access/
│   │   │   └── src/
│   │   │       ├── lib/
│   │   │       │   ├── poll.store.ts                   # PollStore (GM management)
│   │   │       │   ├── poll.api.ts                     # PollApiService
│   │   │       │   └── poll.models.ts                  # TS interfaces (Poll, PollOption, etc.)
│   │   │       └── index.ts
│   │   └── feature-poll-management/
│   │       └── src/
│   │           ├── lib/
│   │           │   ├── poll-management.component.ts    # GM poll list
│   │           │   └── poll-form.component.ts          # Create/edit poll modal
│   │           └── index.ts
│   │
│   ├── dashboard/
│   │   ├── dashboard-data-access/src/
│   │   │   ├── dashboard.store.ts                      # ★ MODIFY — rework state shape
│   │   │   └── service/dashboard.service.ts            # ★ MODIFY — call new aggregated endpoint
│   │   └── feature-dashboard/src/
│   │       ├── dashboard.component.ts                  # ★ MODIFY — rework layout
│   │       └── components/                             ← NEW sub-components
│   │           ├── trending-feed/
│   │           │   └── trending-feed.component.ts
│   │           ├── trivia-card/
│   │           │   └── trivia-card.component.ts        # Fan-facing: answer submission + answered-state
│   │           └── poll-card/
│   │               └── poll-card.component.ts          # Fan-facing: vote + result display
│   │
│   └── core/
│       └── api-types/src/lib/
│           ├── trivia.types.ts                         ← NEW
│           ├── poll.types.ts                           ← NEW
│           └── dashboard.types.ts                      # ★ MODIFY — extend DashboardData interface
│
├── apps/
│   └── sports-gm/src/app/
│       ├── app.routes.ts                               # ★ MODIFY — add trivia + poll management routes
│       └── shell/shell.component.ts                    # ★ MODIFY — add nav links
│
└── services/
    └── sportsAPI/
        │
        ├── Domain/
        │   ├── Trivia/                                 ← NEW subdirectory
        │   │   ├── TriviaSeries.cs                     # Aggregate root — owns TriviaQuestions list
        │   │   ├── TriviaQuestion.cs                   # Child entity — Publish() / Archive() methods
        │   │   ├── TriviaAnswer.cs                     # Standalone idempotency entity
        │   │   ├── TriviaSeriesId.cs
        │   │   ├── TriviaQuestionId.cs
        │   │   ├── TriviaAnswerId.cs
        │   │   └── TriviaQuestionStatus.cs             # Enum: Pending | Active | Archived
        │   ├── Poll/                                   ← NEW subdirectory
        │   │   ├── Poll.cs                             # Aggregate root — owns PollOptions list
        │   │   ├── PollOption.cs                       # Child entity
        │   │   ├── PollVote.cs                         # Standalone idempotency entity
        │   │   ├── PollId.cs
        │   │   ├── PollOptionId.cs
        │   │   ├── PollVoteId.cs
        │   │   └── PollStatus.cs                       # Enum: Active | Archived
        │   └── Repositories/
        │       └── Repositories.cs                     # ★ MODIFY — add ITriviaSeriesRepository, IPollRepository
        │
        ├── Application/
        │   ├── Trivia/                                 ← NEW
        │   │   ├── Commands/
        │   │   │   ├── CreateTriviaSeriesCommand.cs    # + Handler + Validator
        │   │   │   ├── AddTriviaQuestionCommand.cs     # + Handler + Validator
        │   │   │   ├── PublishTriviaQuestionCommand.cs # + Handler
        │   │   │   ├── ArchiveTriviaQuestionCommand.cs # + Handler
        │   │   │   └── SubmitTriviaAnswerCommand.cs    # + Handler + Validator (idempotency + credit)
        │   │   ├── Queries/
        │   │   │   ├── GetTriviaSeriesQuery.cs         # + Handler (GM management view)
        │   │   │   └── GetActiveTriviaQuestionsQuery.cs # + Handler (called by dashboard handler)
        │   │   └── Dto/
        │   │       ├── TriviaSeriesResponse.cs
        │   │       ├── TriviaQuestionResponse.cs
        │   │       └── SubmitTriviaAnswerResponse.cs
        │   ├── Poll/                                   ← NEW
        │   │   ├── Commands/
        │   │   │   ├── CreatePollCommand.cs            # + Handler + Validator
        │   │   │   ├── PublishPollCommand.cs           # + Handler
        │   │   │   ├── ArchivePollCommand.cs           # + Handler
        │   │   │   └── SubmitPollVoteCommand.cs        # + Handler + Validator (idempotency)
        │   │   ├── Queries/
        │   │   │   ├── GetPollsQuery.cs                # + Handler (GM view)
        │   │   │   └── GetActivePollQuery.cs           # + Handler (called by dashboard handler)
        │   │   └── Dto/
        │   │       ├── PollResponse.cs
        │   │       └── SubmitPollVoteResponse.cs
        │   ├── Dashboard/                              ← NEW
        │   │   ├── Queries/
        │   │   │   └── GetDashboardQuery.cs            # + Handler (aggregates all 3 data sources)
        │   │   └── Dto/
        │   │       ├── DashboardResponse.cs
        │   │       ├── TrendingPlayerOptionDto.cs
        │   │       ├── TriviaQuestionViewModel.cs      # includes answeredByMe, selectedAnswer
        │   │       └── PollViewModel.cs                # includes votedByMe, selectedOptionId
        │   └── Common/Interfaces/
        │       └── IApplicationDbContext.cs            # ★ MODIFY — add new DbSet declarations
        │
        ├── Infrastructure/
        │   ├── Data/
        │   │   ├── SportsDbAppContext.cs               # ★ MODIFY — add 6 new DbSets
        │   │   ├── Configurations/
        │   │   │   ├── TriviaSeriesConfiguration.cs    ← NEW
        │   │   │   ├── TriviaAnswerConfiguration.cs    ← NEW (UNIQUE UserId+TriviaQuestionId)
        │   │   │   ├── PollConfiguration.cs            ← NEW
        │   │   │   └── PollVoteConfiguration.cs        ← NEW (UNIQUE UserId+PollId)
        │   │   └── Migrations/
        │   │       └── [timestamp]_AddDashboardEngagementFeatures.cs  ← NEW
        │   └── Repositories/
        │       ├── TriviaSeriesRepository.cs           ← NEW
        │       └── PollRepository.cs                   ← NEW
        │
        └── WebAPI/
            ├── Controllers/
            │   ├── DashboardController.cs              ← NEW
            │   ├── TriviaController.cs                 ← NEW
            │   └── PollController.cs                   ← NEW
            └── Program.cs                              # ★ MODIFY — register new repositories
```

### Architectural Boundaries

**API Boundaries:**

| Boundary | Detail |
|---|---|
| Public fan endpoints | `GET /api/dashboard/{orgId}`, `POST /api/trivia/answer`, `POST /api/poll/vote` — `[Authorize]` |
| GM management endpoints | `POST/PUT/DELETE /api/trivia/series/*`, `POST/PUT/DELETE /api/poll/*` — `[Authorize(Policy="GMOnly")]` |
| Vote economy boundary | `SubmitTriviaAnswerCommandHandler` is the ONLY entry point to `VoteTransaction.ForRewardCredit()` for trivia credits |
| Idempotency boundary | DB unique constraints are the final gate; application layer checks first to return clean errors |

**Component Boundaries:**

| Boundary | Rule |
|---|---|
| Dashboard feature ↔ Trivia/Poll data | `DashboardStore` consumes the aggregated `/api/dashboard/{orgId}` response — does NOT call trivia or poll stores directly |
| Fan-facing trivia card | Answer submission calls `TriviaApiService.submitAnswer()` directly (not via `TriviaStore`, which is GM-focused) |
| Fan-facing poll card | Vote submission calls `PollApiService.submitVote()` directly |
| GM trivia management | `TriviaStore` + `TriviaApiService` — isolated from fan dashboard state |
| GM poll management | `PollStore` + `PollApiService` — isolated from fan dashboard state |

**Data Boundaries:**

| Boundary | Rule |
|---|---|
| `TriviaSeries` aggregate | Load via `ITriviaSeriesRepository` — includes `TriviaQuestions` via `.Include()` |
| `TriviaAnswer` | Queried directly by application handlers — NOT part of the `TriviaSeries` aggregate |
| `Poll` aggregate | Load via `IPollRepository` — includes `PollOptions` via `.Include()` |
| `PollVote` | Queried directly by application handlers — NOT part of the `Poll` aggregate |
| Dashboard aggregation | `GetDashboardQuery` handler joins three data sources in one DB roundtrip via projection queries |

### Requirements to Structure Mapping

| FR Category | FRs | Frontend Location | Backend Location |
|---|---|---|---|
| Dashboard (fan) | FR1–FR5 | `libs/dashboard/feature-dashboard/` + sub-components | `Application/Dashboard/`, `DashboardController.cs` |
| Trivia fan experience | FR6–FR11 | `trivia-card.component.ts` in dashboard | `Application/Trivia/Commands/SubmitTriviaAnswerCommand.cs` |
| Polls fan experience | FR12–FR14 | `poll-card.component.ts` in dashboard | `Application/Poll/Commands/SubmitPollVoteCommand.cs` |
| GM trivia management | FR15–FR23 | `libs/trivia/feature-trivia-management/` in sports-gm | `Application/Trivia/Commands/*` + `Queries/*` |
| GM poll management | FR24–FR27 | `libs/poll/feature-poll-management/` in sports-gm | `Application/Poll/Commands/*` + `Queries/*` |
| Vote economy — trivia earning | FR28–FR30 | `VoteAccountStore` balance refresh after answer | `SubmitTriviaAnswerCommandHandler` vote credit logic |
| Player options feed | FR31–FR32 | `trending-feed.component.ts` | `GetDashboardQuery` handler (sort PlayerOptions by VoteCount DESC) |

**Cross-Cutting Concern → Location:**

| Concern | Location |
|---|---|
| Idempotency enforcement | `TriviaAnswerConfiguration.cs` + `PollVoteConfiguration.cs` (DB unique constraints); checked in command handlers |
| Org scoping | All commands/queries carry `OrganizationId`; all repositories filter by it |
| Status state machine | `TriviaQuestion.cs` domain entity — `Publish()` and `Archive()` domain methods |
| GM auth policy | `[Authorize(Policy="GMOnly")]` on management actions in `TriviaController` and `PollController` |
| API type sharing | `libs/core/api-types/src/lib/trivia.types.ts` + `poll.types.ts` — imported by all Angular apps |

### Integration Points

**Data Flow — Dashboard Load:**

```
DashboardComponent → DashboardStore.load(orgId)
  → GET /api/dashboard/{orgId}
  → GetDashboardQuery handler
      ├── PlayerOption query (Status=Active, sorted by VoteCount DESC, top N)
      ├── TriviaQuestion query (Status=Active, with answered state for userId from JWT)
      └── Poll query (Status=Active, with voted state for userId from JWT)
  → DashboardResponse → store state updated → components render
```

**Data Flow — Vote Credit:**

```
Fan submits correct trivia answer
  → POST /api/trivia/answer
  → SubmitTriviaAnswerCommandHandler
      → Idempotency check (TriviaAnswer table)
      → Load TriviaSeries → TriviaQuestion (VoteRewardAmount)
      → Persist TriviaAnswer
      → If correct: VoteAccount.AddTransaction(ForRewardCredit(amount)) → SaveChangesAsync()
  → Response: { isCorrect, votesEarned }
  → Frontend: VoteAccountStore.load() → balance refreshed
```

## Architecture Validation Results

### Coherence Validation ✅

**Decision Compatibility:**
All technology choices are the existing established stack — no version conflicts possible. New domain aggregates (`TriviaSeries`, `Poll`) follow the same `Aggregate<TId>` base class pattern as `VoteAccount`. Standalone tables (`TriviaAnswer`, `PollVote`) mirror the existing `VoteTransaction` pattern (crosses aggregate boundaries for side-effects). No contradictory decisions found.

**Pattern Consistency:**
Naming conventions are consistent end-to-end: backend `{Verb}{Noun}Command` handlers → controller actions → Angular `{Domain}ApiService` methods → `{Domain}Store` state. The `status: 'idle'|'loading'|'success'|'error'` signal store shape is applied uniformly to all new stores. CQRS handler checklists are comprehensive and match the existing patterns observed in `Application/VoteAccount/`, `Application/Players/`, etc.

**Structure Alignment:**
New Nx libs at `libs/trivia/` and `libs/poll/` follow the established `libs/<domain>/<type>/` slice pattern exactly. Backend layers respect Clean Architecture dependency direction (Domain → Application → Infrastructure → WebAPI). Integration boundaries (fan vs. GM API endpoints, dashboard-only store vs. domain-specific stores) are clearly separated.

**Coherence Note:**
`GetDashboardQuery` handler must build trivia and poll projections **inline** (direct EF queries) rather than dispatching sub-queries via MediatR. MediatR sub-dispatch from within a handler adds overhead and defeats the single-roundtrip goal. Separate query handlers (`GetActiveTriviaQuestionsQuery`, `GetActivePollQuery`) exist for GM views and can share projection logic via private methods — not through MediatR dispatch from the dashboard handler.

### Requirements Coverage Validation ✅

**All 32 Functional Requirements Covered:**

| FR | Coverage |
|---|---|
| FR1–FR2 | `GetDashboardQuery` sorts PlayerOptions by VoteCount DESC; `TrendingPlayerOptionDto` includes ID for navigation |
| FR3 | `GetDashboardQuery` filters `TriviaQuestions` by `Status=Active` for the org |
| FR4 | `GetDashboardQuery` returns single active `Poll` with options |
| FR5 | `DashboardResponse` allows null/empty lists; frontend components handle gracefully |
| FR6–FR7 | `SubmitTriviaAnswerCommand` handler + `ForRewardCredit()` integration |
| FR8 | `TriviaQuestionViewModel` includes `seriesLabel` from parent `TriviaSeries.Name` |
| FR9–FR10 | `TriviaQuestionViewModel.answeredByMe` + `selectedAnswer`; idempotency reject on re-submit |
| FR11 | `SubmitTriviaAnswerResponse.isCorrect` field |
| FR12 | `SubmitPollVoteCommand` — no VoteAccount deduction |
| FR13 | `SubmitPollVoteResponse` returns updated option vote counts — no re-fetch required |
| FR14 | `PollVote` UNIQUE (UserId, PollId) constraint + handler idempotency check |
| FR15–FR16 | `CreateTriviaSeriesCommand` + `AddTriviaQuestionCommand` |
| FR17–FR18 | `TriviaQuestion.CorrectAnswer` field + `VoteRewardAmount` field |
| FR19–FR21 | `PublishTriviaQuestionCommand` + `ArchiveTriviaQuestionCommand`; domain methods `Publish()` / `Archive()` |
| FR22 | `TriviaSeriesResponse` includes `participationCount` per question (COUNT of `TriviaAnswer` rows per `TriviaQuestionId`) |
| FR23 | `GetTriviaSeriesQuery` returns all statuses; GM view filters/displays Archived |
| FR24–FR25 | `CreatePollCommand` + `PublishPollCommand` |
| FR26 | `ArchivePollCommand` |
| FR27 | `PollResponse` includes `PollOptionResponse.voteCount` (COUNT of PollVotes per PollOptionId) |
| FR28–FR30 | `ForRewardCredit()` in handler; idempotency prevents double credit; frontend balance refresh via `VoteAccountStore` |
| FR31–FR32 | PlayerOptions sorted by VoteCount DESC in `GetDashboardQuery`; reflects current counts on dashboard load |

**Non-Functional Requirements — All Covered:**

| NFR | Coverage |
|---|---|
| Dashboard <2s | Single aggregated endpoint; inline EF projections (not MediatR sub-dispatches) |
| Submissions <500ms | Lightweight commands — idempotency check is a single indexed lookup |
| Trivia idempotency | UNIQUE (UserId, TriviaQuestionId) + check-first in handler |
| Poll idempotency | UNIQUE (UserId, PollId) + check-first in handler |
| Vote credit server-side | `VoteRewardAmount` read from domain entity; client passes only `questionId` and answer |
| GMOnly enforcement | `[Authorize(Policy="GMOnly")]` on all management actions |
| DB scalability | Composite indexes on `(OrgId, Status)` for dashboard; `(UserId, QuestionId/PollId)` for idempotency |

### Implementation Readiness Validation ✅

**Decision Completeness:** All 6 net-new domain entities have defined aggregate boundaries, ID types, and repository interfaces. All controllers have defined routes and auth policies. No ambiguous decisions remain.

**Structure Completeness:** Every file needed to implement the MVP is enumerated in the directory structure with `★ MODIFY` vs. new distinctions. EF Core migration is named and scoped. All Angular library paths and path aliases are specified.

**Pattern Completeness:** The 12-step backend entity checklist and CQRS handler checklist cover the full lifecycle. Angular signal store template is concrete. Error handling, idempotency guard, and status transition patterns are documented with code examples.

### Gap Analysis Results

**Critical Gaps: None**

**Important Gaps (noted for implementation):**

1. **Dashboard handler projection**: `GetDashboardQuery` must use inline EF projections, not MediatR sub-dispatch — noted in coherence section above.
2. **Poll result on vote response**: `SubmitPollVoteCommand` should return updated `PollOptionResponse[]` with vote counts so the frontend can display results without a re-fetch (FR13).
3. **Balance refresh trigger**: `trivia-card.component.ts` must call `VoteAccountStore.load()` on successful answer response to update the displayed balance.

**Nice-to-Have (deferred):**
- Pagination for GM trivia/poll management views (up to 500 archived questions per org)
- `TriviaSeriesResponse` aggregate `totalParticipations` across all questions

### Architecture Completeness Checklist

**✅ Requirements Analysis**
- [x] Project context analyzed (brownfield, 32 FRs, 6 capability areas)
- [x] Scale and complexity assessed (Medium — 6 new entities, 3 controllers, 4 new Angular libs)
- [x] Technical constraints identified (vote economy, org scoping, EF code-first, layer rules)
- [x] Cross-cutting concerns mapped (auth, idempotency, state machine, dashboard aggregation)

**✅ Architectural Decisions**
- [x] Domain aggregate boundaries decided
- [x] Dashboard API design decided (single aggregated endpoint)
- [x] Idempotency mechanism decided (DB unique constraints + check-first)
- [x] Vote credit integration point decided (ForRewardCredit, server-side only)
- [x] Controller routing structure decided
- [x] Frontend lib organization decided

**✅ Implementation Patterns**
- [x] Naming conventions (backend + frontend)
- [x] Backend entity checklist (12 steps)
- [x] CQRS handler checklist
- [x] Angular signal store template
- [x] Error handling, idempotency, and status transition patterns
- [x] Mandatory rules + anti-patterns

**✅ Project Structure**
- [x] Complete directory structure (all new/modified files enumerated)
- [x] Component boundaries (fan dashboard vs. GM management, store isolation)
- [x] Integration points and data flow
- [x] All 32 FRs mapped to specific files/directories

### Architecture Readiness Assessment

**Overall Status: READY FOR IMPLEMENTATION**

**Confidence Level: High** — Brownfield extension of established stack. All patterns proven in existing codebase. Vote economy integration path is single and well-defined.

**Key Strengths:**
- Single-roundtrip dashboard endpoint eliminates N+1 risk
- Two-layer idempotency (application check + DB constraint) — vote economy integrity guaranteed
- Fan dashboard and GM management stores fully isolated
- Status state machine lives in domain entity — clean encapsulation

**Areas for Future Enhancement:**
- SignalR push for live vote counts and poll results (Phase 2)
- Dashboard caching strategy (Phase 2)
- Pagination for large GM trivia archives

### Implementation Handoff

**First Implementation Priority:**
Backend domain layer first: `Domain/Trivia/` entities and IDs → `Domain/Poll/` → EF migration → Application CQRS handlers → Infrastructure repositories → WebAPI controllers → Angular libs.
