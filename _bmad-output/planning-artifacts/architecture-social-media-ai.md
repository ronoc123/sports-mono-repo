---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments:
  - '_bmad-output/planning-artifacts/prd-social-media-ai.md'
  - 'docs/architecture.md'
  - 'docs/integration-architecture.md'
  - 'docs/api-contracts-backend.md'
  - 'docs/data-models-backend.md'
  - 'docs/state-management-frontend.md'
  - 'docs/source-tree-analysis.md'
workflowType: 'architecture'
lastStep: 8
status: 'complete'
project_name: 'social-media-ai-video-tool'
user_name: 'Kampe'
date: '2026-08-19'
completedAt: '2026-08-19'
---

# Architecture Decision Document — Social Media AI Video Tool

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**

37 FRs across 7 capability areas drive the following net-new architectural components:

| Capability Area | FR Count | Architectural Implication |
|---|---|---|
| Channel Management | FR1–FR5 | New `Channel` aggregate root; CRUD API + Angular feature module |
| Platform Account Linking | FR6–FR10 | `LinkedAccount` sub-document per channel; OAuth 2.0 flow; AES-256 encrypted refresh token storage |
| Post Cycle — Video & Metadata | FR11–FR17 | Multipart video upload endpoint; temp file management; history-seeded metadata suggestion query |
| AI Generation (Interface & Stub) | FR18–FR20 | `IVideoGenerationAdapter` contract; stub DI registration; channel context payload assembly |
| Posting & Distribution | FR21–FR26 | Async fan-out job per post cycle; per-platform status tracking; polling endpoint; per-platform retry |
| Post History | FR27–FR32 | `PostRecord` document with per-platform result sub-list; history query for channel context |
| Adapter Contracts | FR33–FR37 | `ISocialMediaAdapter` interface; YouTube implementation; plug-in DI registration pattern |

**Non-Functional Requirements:**

| NFR | Architectural Impact |
|---|---|
| Dashboard load <1s | Channel list endpoint must avoid N+1 MongoDB queries — project only last-post summary fields |
| Post history load <2s | Paginated query (20/page) on `PostRecord` collection, indexed by `channelId + timestamp DESC` |
| Status updates <3s | Polling endpoint must be a lightweight read on `PostCycleJob` document — no heavy aggregation |
| AES-256 at rest | Encryption/decryption service injected into LinkedAccount repository; key from app settings |
| Temp file deleted within 1hr | File lifecycle tied to PostCycleJob terminal state; background cleanup fallback required |
| Per-platform isolation | Fan-out must run each platform adapter independently; one failure must not cancel others |
| Post history written before response | `PostRecord` persisted atomically before returning job terminal result to frontend |
| Retryable platform failures | PostCycleJob per-platform state must be mutable after initial completion |

**Scale & Complexity:**

- Primary domain: Full-stack brownfield — new `SocialMediaAPI` microservice (.NET 8 + MongoDB) + new `social-media` Angular feature module
- Complexity level: Medium — new microservice from scratch with MongoDB, async multi-platform fan-out, OAuth 2.0 credential management, pluggable adapter pattern
- Net-new backend domain documents: 3 core (`Channel`, `PostRecord`, `PostCycleJob`)
- Net-new backend sub-documents: `LinkedAccount` (per channel), `PlatformResult` (per post record)
- Net-new API controllers: 3 (`ChannelController`, `PostCycleController`, `HealthController`)
- Net-new Angular feature modules: 1 (`social-media`) containing channel, post cycle, and history views

### Technical Constraints & Dependencies

- **Existing Angular SPA**: New `social-media` module added to the existing Nx workspace; follows NgRx Signals Store pattern, zoneless change detection, `ApiService` HTTP base
- **SocialMediaAPI already scaffolded**: Domain, Application, Infrastructure, WebAPI layers exist with MongoDB connection infrastructure; `SportifyCore.Domain` and `SportifyCore.Persistence` shared libraries available
- **MongoDB (not EF Core)**: No migrations; schema is document-based. Collection naming, BSON serialization, and index strategy must be explicitly decided
- **YouTube Data API v3**: OAuth 2.0 authorization code flow for account linking; resumable upload API for video upload to YouTube
- **No shared auth infrastructure**: SocialMediaAPI is a personal single-user tool; JWT from IdentityService used for API authentication (same RSA-256 public key pattern as other services)
- **Streaming upload constraint**: Kestrel MaxRequestBodySize set to 500MB; video must be streamed to disk, not buffered in memory
- **Clean Architecture dependency direction**: Domain → Application → Infrastructure → WebAPI; adapters registered in Infrastructure DI, never leaked into Application layer

### Cross-Cutting Concerns Identified

1. **Adapter plug-in registration**: Both `IVideoGenerationAdapter` and `ISocialMediaAdapter` must be resolvable from DI; Infrastructure owns all registrations; no adapter code in Application layer
2. **AES-256 encryption**: Refresh token encrypted before MongoDB write, decrypted before use — encapsulated in a dedicated `IEncryptionService` to avoid scattered crypto logic
3. **Async post cycle job lifecycle**: Post cycle fan-out is inherently async (YouTube upload can take time); a `PostCycleJob` document tracks per-platform state; the polling endpoint reads it; job transitions to terminal state when all platforms complete or fail
4. **Temp file management**: Video file written to a temp path; must be cleaned up on success, failure, and timeout — requires explicit lifecycle ownership (post cycle completion + background fallback)
5. **MongoDB index strategy**: `Channel` by `_id`; `PostRecord` indexed on `(channelId, createdAt DESC)`; `PostCycleJob` indexed on `channelId` — all queries must avoid collection scans
6. **Error isolation in fan-out**: Each platform adapter runs in its own try/catch; failures update only that platform's `PlatformResult` sub-document; other platforms are unaffected
7. **YouTube OAuth flow**: Auth code flow in browser popup → callback to API → exchange for tokens → encrypt and persist; access token refresh on each use before expiry

## Technical Foundation

### Primary Technology Domain

Brownfield full-stack extension — no starter template initialization required. Both the backend service and frontend workspace are already scaffolded.

### Existing Stack (Established — Non-Negotiable)

**Backend (SocialMediaAPI — already scaffolded):**

| Layer | Technology | Version |
|---|---|---|
| Runtime | .NET / C# | 8 |
| Web framework | ASP.NET Core | 8 |
| Database | MongoDB | 7.x |
| MongoDB driver | MongoDB.Driver | 2.28.0 |
| CQRS | MediatR | 12.2.0 |
| Validation | FluentValidation | 11.9.0 |
| Auth | JWT Bearer (RSA-256, shared public key) | — |
| Shared libs | SportifyCore.Domain, SportifyCore.Persistence | — |

**Frontend (existing Nx Angular workspace):**

| Layer | Technology | Version |
|---|---|---|
| Framework | Angular | 20 |
| State management | NgRx Signals Store | 19 |
| Monorepo tooling | Nx | 21 |
| Language | TypeScript | 5.8 |
| Change detection | Zoneless | — |
| HTTP base | `ApiService` from `@sports-ui/http-client` | — |

### Architectural Patterns New Features Must Follow

**Backend:**
- Domain entities extend `Entity<TId>` or `Aggregate<TId>` from `SportifyCore.Domain`
- Repository pattern: use `IRepository<T, TId>` as the default contract for all aggregate roots
- If a query method is generically useful to all aggregate roots, add it to `MongoRepository<T, TId>` in `SportifyCore.Persistence` (benefits the whole platform)
- Only create a domain-specific repository interface (e.g., `IPostRecordRepository`) when the query is domain-specific and cannot be generalized
- All business logic through MediatR command/query handlers
- Controllers inject `IMediator` only
- Infrastructure layer owns all adapter registrations via DI

**Frontend:**
- New feature added as `social-media` feature module in existing Nx workspace
- Signal stores use `status: 'idle' | 'loading' | 'success' | 'error'` shape
- Async methods use `rxMethod` + `switchMap` + `tapResponse`
- HTTP services use `ApiService` base from `@sports-ui/http-client`
- API URL pattern: `${environment.socialMediaApi}/<controller>/<action>`

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (block implementation):**
- Document model: LinkedAccounts embedded in Channel
- PostCycleJob vs PostRecord: separate collections with distinct lifecycles
- Fan-out execution: fire-and-forget Task with PostCycleJob tracking
- BSON ID strategy: `string` with `[BsonRepresentation(BsonType.ObjectId)]`

**Important Decisions (shape architecture):**
- YouTube OAuth callback: popup + polling
- Temp file location: configurable via app settings
- API response envelope: `ServiceResponse<T>` from SportifyCore.Contracts

**Deferred (Post-MVP):**
- Queue-based job processing (Hangfire or similar) if reliability requirements grow
- Azure Blob Storage for video if file sizes exceed direct upload limits
- postMessage OAuth flow refinement

### Data Architecture

**Document Model — Channel**

`LinkedAccount` documents are embedded as a sub-array inside the `Channel` document. A channel holds at most one linked account per platform — the list is small and bounded. No separate collection needed; reads are single-document.

```
Channel {
  _id: ObjectId (string in C#)
  name: string
  description: string
  styleToneContext: string
  linkedAccounts: LinkedAccount[]    ← embedded sub-documents
  createdAt: DateTime
}

LinkedAccount {
  platform: string           // "YouTube" | "TikTok" | "Instagram"
  accountDisplayName: string
  encryptedRefreshToken: string    ← AES-256 encrypted
  tokenIv: string                  ← IV for AES decryption
  linkedAt: DateTime
}
```

**Document Model — PostRecord (permanent history)**

One document per completed post cycle. Written atomically before the job result is returned to the frontend. Indexed on `(channelId, createdAt DESC)` for paginated history queries.

```
PostRecord {
  _id: ObjectId (string in C#)
  channelId: string
  title: string
  description: string
  hashtags: string[]
  videoReference: string       ← local path or URL used during the cycle
  createdAt: DateTime
  platformResults: PlatformResult[]
}

PlatformResult {
  platform: string
  status: string               // "Published" | "Failed" | "TimedOut"
  platformPostId: string?      ← null if failed
  platformPostUrl: string?     ← null if failed
  errorMessage: string?        ← null if published
  attemptedAt: DateTime
}
```

**Document Model — PostCycleJob (transient)**

Separate collection `post_cycle_jobs`. Created when a post cycle begins; updated per-platform as the fan-out progresses; cleaned up (or archived) after a PostRecord is written at terminal state. Indexed on `channelId`.

```
PostCycleJob {
  _id: ObjectId (string in C#)
  channelId: string
  videoTempPath: string        ← path to uploaded temp file
  title: string
  description: string
  hashtags: string[]
  status: string               // "Running" | "Completed" | "PartialFailure" | "TimedOut"
  platformJobs: PlatformJob[]
  startedAt: DateTime
  completedAt: DateTime?
}

PlatformJob {
  platform: string
  status: string               // "Pending" | "Uploading" | "Published" | "Failed"
  platformPostId: string?
  platformPostUrl: string?
  errorMessage: string?
  updatedAt: DateTime
}
```

**BSON ID Strategy**

All aggregate roots use `string` as `TId` in C#, decorated with `[BsonRepresentation(BsonType.ObjectId)]` on the `Id` property. ObjectIds are stored natively in MongoDB; the Domain and Application layers see clean strings. New IDs are generated via `ObjectId.GenerateNewId().ToString()` in the repository `AddAsync` method.

**MongoDB Collections**

| Collection | Index |
|---|---|
| `channels` | `_id` (default) |
| `post_records` | `(channelId, createdAt DESC)` |
| `post_cycle_jobs` | `channelId` |

**Background Temp File Cleanup**

A lightweight `IHostedService` runs on a configurable interval (e.g., every 30 minutes) and deletes any temp files whose corresponding `PostCycleJob` has reached a terminal state for more than 1 hour, providing a safety net for files that weren't cleaned up by the job handler itself.

### Authentication & Security

**JWT Authentication**
SocialMediaAPI validates JWT tokens using the shared RSA-256 public key from `Keys/public.pem`, identical to all other services in the monorepo. All controller endpoints require `[Authorize]` except the health and OAuth callback endpoints.

**OAuth Credential Encryption**
`IEncryptionService` encapsulates AES-256-CBC encryption/decryption. Implemented in the Infrastructure layer; injected into the LinkedAccount persistence logic.

```
IEncryptionService {
  Encrypt(plaintext: string) → (ciphertext: string, iv: string)
  Decrypt(ciphertext: string, iv: string) → string
}
```

Key sourced from `appsettings.json` → `Encryption:Key` (32-byte base64 string). IV is randomly generated per encryption operation and stored alongside the ciphertext in the `LinkedAccount` sub-document.

**YouTube OAuth Flow**

1. Frontend opens a popup to `GET /api/oauth/youtube/authorize?channelId={id}`
2. API redirects to Google OAuth consent screen
3. After consent, Google redirects to `GET /api/oauth/youtube/callback?code=...&state=...`
4. API exchanges auth code for tokens, encrypts the refresh token, embeds the `LinkedAccount` in the Channel document
5. Frontend polls `GET /api/channels/{id}/linked-accounts` until the YouTube account appears (consistent with the polling pattern used elsewhere in the post cycle)

### API & Communication

**Response Envelope**
All controllers return `ServiceResponse<T>` from `SportifyCore.Contracts` — consistent with all other services in the monorepo.

```json
{ "data": { ... }, "success": true, "message": null }
{ "data": null, "success": false, "message": "Error description" }
```

**Controller Routing**

| Controller | Base Route | Auth | Responsibility |
|---|---|---|---|
| `HealthController` | `/api/health` | None | Liveness + DB connectivity check |
| `ChannelController` | `/api/channels` | `[Authorize]` | Channel CRUD + linked account management |
| `PostCycleController` | `/api/post-cycles` | `[Authorize]` | Start cycle, upload video, poll status, retry |
| `PostRecordController` | `/api/post-records` | `[Authorize]` | Channel post history queries |
| `OAuthController` | `/api/oauth` | Mixed | YouTube OAuth authorize + callback |

**Fan-Out Execution**

`StartPostCycleCommandHandler` creates a `PostCycleJob` document, persists it (returns the job ID), then fires a background `Task.Run` that:
1. Reads the channel's linked accounts (decrypting refresh tokens)
2. Iterates each platform — runs each `ISocialMediaAdapter` in a `try/catch`
3. Updates the `PlatformJob.Status` in MongoDB after each platform completes or fails
4. When all platforms reach terminal state: writes the `PostRecord`, deletes the temp file, marks `PostCycleJob.Status` as `Completed` or `PartialFailure`

The polling endpoint `GET /api/post-cycles/{jobId}` reads the `PostCycleJob` document — lightweight indexed lookup, no aggregation.

**Retry Flow**
`POST /api/post-cycles/{jobId}/retry/{platform}` re-runs the adapter for the specified platform only. The `PostCycleJob` must still exist and the platform's status must be `Failed`. On success, re-evaluates overall job status and writes/updates the `PostRecord` accordingly.

### Frontend Architecture

**Feature Module Structure**

New `social-media` feature module in the existing Nx workspace under `libs/social-media/`. Follows the existing `data-access` / `feature-*` / `ui` slice pattern.

| Lib | Path | Contents |
|---|---|---|
| `social-media-data-access` | `libs/social-media/social-media-data-access/` | Stores, API services, TS models |
| `feature-channels` | `libs/social-media/feature-channels/` | Channel list, detail, create/edit |
| `feature-post-cycle` | `libs/social-media/feature-post-cycle/` | Upload, review, posting status |
| `feature-post-history` | `libs/social-media/feature-post-history/` | Channel history list + detail |

**Store Shape**

```typescript
ChannelStore {
  status: 'idle' | 'loading' | 'success' | 'error'
  channels: ChannelSummary[]
  selectedChannel: ChannelDetail | null
}

PostCycleStore {
  status: 'idle' | 'uploading' | 'posting' | 'success' | 'error'
  activeJob: PostCycleJob | null      // polled every 3s while posting
  pollingActive: boolean
}
```

### Infrastructure & Deployment

No new infrastructure required for MVP. SocialMediaAPI runs as an additional Kestrel service alongside existing services. Added to `docker-compose.yml` with MongoDB connection string pointing to the existing MongoDB instance (or a dedicated DB within it).

**Temp file storage**: Configurable via `appsettings.json` → `"TempStorage:Path"`. Defaults to a `temp/` subdirectory relative to the application root in development.

### Decision Impact Analysis

**Implementation Sequence (dependency order):**
1. `IEncryptionService` — required before any LinkedAccount can be persisted
2. Channel aggregate + repository — foundation for everything else
3. YouTube OAuth flow — required before any real posting can be tested
4. PostCycleJob + PostRecord documents + repositories — fan-out lifecycle
5. `ISocialMediaAdapter` + YouTube implementation — first real posting capability
6. `IVideoGenerationAdapter` interface + stub — contracts defined alongside YouTube
7. Angular feature module — consumes all of the above APIs

**Cross-Component Dependencies:**
- `PostCycleController` depends on `IEncryptionService` (via adapter — to decrypt tokens at post time)
- `ISocialMediaAdapter` (YouTube) depends on decrypted refresh token → access token exchange
- `PostRecord` write depends on `PostCycleJob` reaching terminal state
- Temp file cleanup (both job-level and IHostedService) depends on `PostCycleJob.Status`
- Metadata suggestions (FR17) depend on `PostRecord` history query on `channelId`

## Implementation Patterns & Consistency Rules

### Naming Patterns

**Backend — C# Artifacts:**

| Artifact | Convention | Example |
|---|---|---|
| Domain entity | PascalCase noun | `Channel`, `PostRecord`, `PostCycleJob` |
| Embedded sub-document | PascalCase noun | `LinkedAccount`, `PlatformResult`, `PlatformJob` |
| Command | `{Verb}{Noun}Command` | `CreateChannelCommand`, `StartPostCycleCommand` |
| Query | `Get{Noun}Query` | `GetChannelQuery`, `GetPostHistoryQuery` |
| Handler | `{CommandOrQuery}Handler` | `CreateChannelCommandHandler` |
| DTO (response) | `{Noun}Response` | `ChannelResponse`, `PostCycleJobResponse` |
| DTO (request body) | `{Noun}Request` | `CreateChannelRequest` |
| Controller | `{Domain}Controller` | `ChannelController`, `PostCycleController` |
| Repository (specific) | `I{Aggregate}Repository` | `IPostRecordRepository` |
| Adapter interface | `I{Capability}Adapter` | `IVideoGenerationAdapter`, `ISocialMediaAdapter` |
| Adapter implementation | `{Platform}{Capability}Adapter` | `YouTubeSocialMediaAdapter`, `StubVideoGenerationAdapter` |

**Backend — MongoDB Collections:**

| Document | Collection Name |
|---|---|
| `Channel` | `channels` |
| `PostRecord` | `post_records` |
| `PostCycleJob` | `post_cycle_jobs` |

**Backend — API Routes:** Plural `kebab-case` nouns (`/api/channels`, `/api/post-cycles`, `/api/post-records`, `/api/oauth`, `/api/health`)

**Frontend — Angular Artifacts:**

| Artifact | Convention | Example |
|---|---|---|
| Signal store class | `{Domain}Store` | `ChannelStore`, `PostCycleStore` |
| HTTP service class | `{Domain}ApiService` | `ChannelApiService`, `PostCycleApiService` |
| API model interface | `{Noun}` (no suffix) | `Channel`, `PostCycleJob`, `PostRecord` |
| Feature component | `{domain}-{view}.component.ts` | `channel-list.component.ts` |
| Store state interface | `{Domain}State` | `ChannelState`, `PostCycleState` |

### Structure Patterns

**New Backend Entity Checklist (in order):**

1. `Domain/{Aggregate}/` — entity class extending `Entity<string>` or `Aggregate<string>`
2. `Domain/{Aggregate}/` — any embedded sub-document classes (not entities, no ID)
3. `Application/{Aggregate}/Commands/` — command + handler + FluentValidation validator
4. `Application/{Aggregate}/Queries/` — query + handler
5. `Application/{Aggregate}/Dto/` — response DTOs
6. `Infrastructure/Repositories/` — concrete repository extending `MongoRepository<T, string>`
7. `Infrastructure/DependencyInjection.cs` — register repository + any new services
8. `WebAPI/Controllers/` — controller action(s)

**Adapter Implementation Checklist:**

1. Define interface in `Application/Adapters/` (Application layer — no infrastructure dependency)
2. Implement in `Infrastructure/Adapters/`
3. Register in `Infrastructure/DependencyInjection.cs`
4. Never leak adapter types into Domain or Application logic

### Format Patterns

**API Response:** Always `ServiceResponse<T>` — `{ "data": {...}, "success": true, "message": null }`

**JSON Serialization:** `camelCase` property names · enums as strings · dates as ISO 8601 · null fields included (not omitted)

**Status Fields:** Always string unions, never boolean flags:
- `PostCycleJob.Status`: `"Running"` | `"Completed"` | `"PartialFailure"` | `"TimedOut"`
- `PlatformJob.Status`: `"Pending"` | `"Uploading"` | `"Published"` | `"Failed"`
- Angular store `status`: `"idle"` | `"loading"` | `"success"` | `"error"`

**BSON ID Generation:** New ObjectIds generated in repository `AddAsync` before insert — never in Domain or Application layers:
```csharp
if (string.IsNullOrEmpty(entity.Id))
    entity.Id = ObjectId.GenerateNewId().ToString();
```

### Communication Patterns

**Angular Signal Store Template:**

```typescript
export const ChannelStore = signalStore(
  { providedIn: 'root' },
  withState<ChannelState>({
    status: 'idle' as Status,
    channels: [] as ChannelSummary[],
    selectedChannel: null as ChannelDetail | null,
  }),
  withMethods((store, api = inject(ChannelApiService)) => ({
    loadChannels: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { status: 'loading' })),
        switchMap(() =>
          api.getChannels().pipe(
            tapResponse({
              next: (channels) => patchState(store, { status: 'success', channels }),
              error: () => patchState(store, { status: 'error' }),
            })
          )
        )
      )
    ),
  }))
);
```

**Polling Pattern:** Poll every 3 seconds; stop when `PostCycleJob.Status` is `"Completed"`, `"PartialFailure"`, or `"TimedOut"`; enforce 10-minute maximum polling duration in the store.

### Process Patterns

**Backend Error Handling:**

| Scenario | Pattern |
|---|---|
| Domain rule violation | Throw `DomainException` → global middleware → 400 |
| Resource not found | `ServiceResponse.Failure("not found")` → 404 |
| Adapter failure | Adapter returns `PlatformPostResult(IsSuccess: false)` — never throws |
| Auth failure | ASP.NET Core policy middleware → 401/403 |
| Validation failure | FluentValidation pipeline → `ServiceResponse.Failure(errors)` → 400 |

**Adapter Result Pattern:**

```csharp
public record PlatformPostResult(
    bool IsSuccess,
    string? PlatformPostId,
    string? PlatformPostUrl,
    string? ErrorMessage
);
// Adapters always return this — never throw
```

**Fan-Out Concurrency:** All platform adapters run in parallel:
```csharp
var tasks = platformJobs.Select(job => RunAdapterAsync(job, videoPath, metadata));
await Task.WhenAll(tasks);
```

**Temp File Lifecycle:** Always cleaned up in `finally` regardless of outcome; `IHostedService` provides safety-net cleanup for orphaned files older than 1 hour.

**OAuth Token Handling:** Application layer never receives or handles token values. Decryption happens in Infrastructure; Application issues commands with `channelId` only.

### Enforcement Guidelines

**Mandatory Rules (hard violations):**

- Never store a plaintext OAuth token — always encrypt via `IEncryptionService` before MongoDB write
- Never throw from an adapter implementation — return `PlatformPostResult(IsSuccess: false)`
- Never run platform adapters sequentially — use `Task.WhenAll`
- Never buffer video in memory — stream to temp path via `IFormFile.CopyToAsync`
- Never expose decrypted OAuth tokens in Application or Domain layers
- Always write `PostRecord` before marking `PostCycleJob` as terminal
- Always clean up temp files (job `finally` block + IHostedService safety-net)
- Never inject `IMongoDatabase` or `IMongoCollection<T>` into controllers or handlers — use repository interfaces
- Every new command must have a FluentValidation validator

**Anti-Patterns to Avoid:**

- `isLoading: boolean` in Angular stores — use `status` string union
- Multiple save calls in one handler for the same aggregate
- Hardcoded platform name strings in business logic — use constants defined in Domain
- Polling without a 10-minute timeout guard
- Catching `Task.WhenAll` exceptions at the outer level — each task catches its own

## Project Structure & Boundaries

### Complete Project Directory Structure — Net-New Files

`★ MODIFY` = existing file changed; unmarked = new.

```
sports-mono-repo/
│
├── services/SocialMediaAPI/
│   │
│   ├── Domain/
│   │   ├── Channel/
│   │   │   ├── Channel.cs                          # Aggregate root — embeds LinkedAccount[]
│   │   │   └── LinkedAccount.cs                    # Embedded sub-document (not an Entity)
│   │   ├── PostRecord/
│   │   │   ├── PostRecord.cs                       # Entity — permanent post history
│   │   │   └── PlatformResult.cs                   # Embedded sub-document
│   │   └── PostCycleJob/
│   │       ├── PostCycleJob.cs                     # Entity — transient fan-out job
│   │       ├── PlatformJob.cs                      # Embedded sub-document
│   │       ├── PostCycleStatus.cs                  # Enum: Running|Completed|PartialFailure|TimedOut
│   │       └── PlatformJobStatus.cs                # Enum: Pending|Uploading|Published|Failed
│   │
│   ├── Application/
│   │   ├── Common/Interfaces/
│   │   │   ├── ISocialMediaDbContext.cs             # ★ MODIFY — add collection accessors
│   │   │   └── IEncryptionService.cs               # NEW — AES-256 encrypt/decrypt contract
│   │   ├── Adapters/
│   │   │   ├── IVideoGenerationAdapter.cs          # NEW — AI generation contract
│   │   │   ├── ISocialMediaAdapter.cs              # NEW — social posting contract
│   │   │   ├── VideoGenerationRequest.cs           # NEW — channel context + history payload
│   │   │   ├── VideoGenerationResult.cs            # NEW — video ref + suggested metadata
│   │   │   ├── SocialPostRequest.cs                # NEW — video path + metadata
│   │   │   └── PlatformPostResult.cs               # NEW — IsSuccess, PostId, Url, Error
│   │   ├── Channel/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateChannelCommand.cs         # + Handler + Validator
│   │   │   │   ├── UpdateChannelCommand.cs         # + Handler + Validator
│   │   │   │   ├── DeleteChannelCommand.cs         # + Handler
│   │   │   │   ├── LinkAccountCommand.cs           # + Handler + Validator
│   │   │   │   └── UnlinkAccountCommand.cs         # + Handler
│   │   │   ├── Queries/
│   │   │   │   ├── GetChannelsQuery.cs             # + Handler (summary list)
│   │   │   │   └── GetChannelQuery.cs              # + Handler (detail + linked accounts)
│   │   │   └── Dto/
│   │   │       ├── ChannelSummaryResponse.cs
│   │   │       ├── ChannelDetailResponse.cs
│   │   │       └── LinkedAccountResponse.cs
│   │   ├── PostCycle/
│   │   │   ├── Commands/
│   │   │   │   ├── StartPostCycleCommand.cs        # + Handler + Validator
│   │   │   │   └── RetryPlatformCommand.cs         # + Handler
│   │   │   ├── Queries/
│   │   │   │   └── GetPostCycleJobQuery.cs         # + Handler
│   │   │   └── Dto/
│   │   │       ├── PostCycleJobResponse.cs
│   │   │       └── PlatformJobResponse.cs
│   │   └── PostRecord/
│   │       ├── Queries/
│   │       │   ├── GetPostHistoryQuery.cs          # + Handler (paginated, channelId)
│   │       │   └── GetMetadataSuggestionsQuery.cs  # + Handler
│   │       └── Dto/
│   │           ├── PostRecordResponse.cs
│   │           ├── PlatformResultResponse.cs
│   │           └── MetadataSuggestionsResponse.cs
│   │
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── SocialMediaDbContext.cs             # ★ MODIFY — add collection accessors
│   │   │   └── MongoDbSettings.cs                  # existing — unchanged
│   │   ├── Repositories/
│   │   │   ├── ChannelRepository.cs               # extends MongoRepository<Channel, string>
│   │   │   ├── PostRecordRepository.cs            # extends MongoRepository + IPostRecordRepository
│   │   │   └── PostCycleJobRepository.cs          # extends MongoRepository + IPostCycleJobRepository
│   │   ├── Services/
│   │   │   ├── AesEncryptionService.cs            # implements IEncryptionService
│   │   │   ├── PostCycleOrchestrationService.cs   # fan-out; Task.WhenAll; per-platform try/catch
│   │   │   └── TempFileCleanupService.cs          # IHostedService; 30-min interval
│   │   ├── Adapters/
│   │   │   ├── StubVideoGenerationAdapter.cs      # implements IVideoGenerationAdapter
│   │   │   └── YouTubeSocialMediaAdapter.cs       # implements ISocialMediaAdapter
│   │   └── DependencyInjection.cs                 # ★ MODIFY — register all new services + adapters
│   │
│   └── WebAPI/
│       ├── Controllers/
│       │   ├── HealthController.cs                # existing — unchanged
│       │   ├── ChannelController.cs               # CRUD + linked account management
│       │   ├── PostCycleController.cs             # start, poll status, retry
│       │   ├── PostRecordController.cs            # history list + metadata suggestions
│       │   └── OAuthController.cs                 # /authorize redirect + /callback exchange
│       ├── Program.cs                             # ★ MODIFY — Kestrel size limit, new routes
│       ├── appsettings.json                       # ★ MODIFY — TempStorage:Path, Encryption:Key
│       └── appsettings.Development.json           # ★ MODIFY — local dev overrides
│
└── (frontend) libs/social-media/
    │
    ├── social-media-data-access/src/lib/
    │   ├── models/
    │   │   ├── channel.models.ts
    │   │   ├── post-cycle.models.ts
    │   │   └── post-record.models.ts
    │   ├── channel.store.ts
    │   ├── channel.api.ts
    │   ├── post-cycle.store.ts
    │   ├── post-cycle.api.ts
    │   └── post-record.api.ts
    │
    ├── feature-channels/src/lib/
    │   ├── channel-list/channel-list.component.ts
    │   ├── channel-detail/channel-detail.component.ts
    │   ├── channel-form/channel-form.component.ts
    │   └── link-account/link-account.component.ts
    │
    ├── feature-post-cycle/src/lib/
    │   ├── video-upload/video-upload.component.ts
    │   ├── metadata-review/metadata-review.component.ts
    │   └── posting-status/posting-status.component.ts
    │
    └── feature-post-history/src/lib/
        ├── post-history-list/post-history-list.component.ts
        └── post-record-detail/post-record-detail.component.ts
```

### Architectural Boundaries

**API Boundaries:**

| Boundary | Detail |
|---|---|
| All endpoints | `[Authorize]` — JWT Bearer required except `/api/health` and `/api/oauth/callback` |
| OAuth callback | No auth — receives Google redirect with auth code |
| Video upload | `POST /api/post-cycles/start` — multipart; Kestrel MaxRequestBodySize 500MB |
| Polling endpoint | `GET /api/post-cycles/{jobId}` — lightweight indexed read; no aggregation |
| Retry endpoint | `POST /api/post-cycles/{jobId}/retry/{platform}` — requires `Failed` status on that platform |
| Adapter registration | `Infrastructure/DependencyInjection.cs` is the ONLY place adapters are wired to DI |

**Component Boundaries:**

| Boundary | Rule |
|---|---|
| Channel store ↔ Post cycle | `PostCycleStore` uses `channelId` param — does not consume `ChannelStore` directly |
| Posting status ↔ Retry | `posting-status.component` calls `PostCycleApiService.retry()` — not via store |
| Metadata suggestions | Loaded via `PostRecordApiService.getSuggestions(channelId)` on review screen load |
| OAuth popup ↔ parent | Parent polls `ChannelApiService.getChannel(id)` to detect new `linkedAccount` |

**Data Boundaries:**

| Boundary | Rule |
|---|---|
| `Channel` aggregate | Loaded via `IRepository<Channel, string>`; `LinkedAccount[]` always embedded |
| `PostRecord` | Domain-specific queries via `IPostRecordRepository` |
| `PostCycleJob` | Domain-specific queries via `IPostCycleJobRepository` |
| Token decryption | Infrastructure only — `PostCycleOrchestrationService` decrypts before adapter call |

### Requirements to Structure Mapping

| FR Group | Backend Location | Frontend Location |
|---|---|---|
| FR1–FR5 (Channel CRUD) | `Application/Channel/Commands + Queries` | `feature-channels` |
| FR6–FR10 (Platform Linking) | `Application/Channel/Commands/LinkAccountCommand` + `OAuthController` | `link-account.component` |
| FR11–FR13 (Video Upload) | `Application/PostCycle/Commands/StartPostCycleCommand` | `video-upload.component` |
| FR14–FR17 (Metadata + Suggestions) | `Application/PostRecord/Queries/GetMetadataSuggestionsQuery` | `metadata-review.component` |
| FR18–FR20 (AI Gen Stub) | `Application/Adapters/IVideoGenerationAdapter` + `StubVideoGenerationAdapter` | `metadata-review` (Generate button) |
| FR21–FR26 (Fan-Out + Status + Retry) | `PostCycleOrchestrationService` + `Application/PostCycle` | `posting-status.component` |
| FR27–FR32 (Post History) | `Application/PostRecord/Queries` + `Domain/PostRecord` | `feature-post-history` |
| FR33–FR37 (Adapter Contracts) | `Application/Adapters/` interfaces + `Infrastructure/Adapters/` impls | — |

### Integration Points & Data Flow

**Post Cycle — Start:**
```
VideoUploadComponent → POST /api/post-cycles/start (multipart)
  → StartPostCycleCommandHandler
      → Stream video to TempStorage:Path/{guid}.mp4
      → Create + persist PostCycleJob (status: Running)
      → Task.Run: PostCycleOrchestrationService.ExecuteAsync(jobId)
      → Return jobId immediately
  → PostCycleStore receives jobId → begins 3s polling
```

**Post Cycle — Fan-Out (background Task):**
```
PostCycleOrchestrationService.ExecuteAsync(jobId)
  → Load Channel → decrypt refresh tokens per LinkedAccount
  → Task.WhenAll(linkedAccounts.Select(a => RunPlatformAsync(a, videoPath, metadata)))
      Each platform task (isolated try/catch):
        → Refresh OAuth access token via YouTube API
        → Call YouTubeSocialMediaAdapter.PostAsync(SocialPostRequest)
        → Update PlatformJob.Status in MongoDB
  → When all terminal: write PostRecord → delete temp file → update PostCycleJob.Status
```

**Metadata Suggestions:**
```
MetadataReviewComponent → GET /api/post-records/suggestions?channelId={id}
  → GetMetadataSuggestionsQueryHandler
      → IPostRecordRepository.GetRecentByChannelIdAsync(channelId, limit: 10)
      → Extract: most recent title + description + top hashtags by frequency
  → MetadataSuggestionsResponse → pre-populate review form fields
```

## Architecture Validation

### Functional Requirements Coverage

All 37 FRs verified covered:

| FR Group | FR Count | Architecture Coverage | Status |
|---|---|---|---|
| FR1–FR5 Channel CRUD | 5 | `ChannelController` + `Application/Channel/Commands+Queries` + `ChannelRepository` | ✅ |
| FR6–FR10 Platform Account Linking | 5 | `OAuthController` + `LinkAccountCommand/UnlinkAccountCommand` + `IEncryptionService` + `LinkedAccount` embedded in Channel | ✅ |
| FR11–FR13 Video Upload & Temp Storage | 3 | `StartPostCycleCommand` streams to `TempStorage:Path`; Kestrel 500MB limit; `PostCycleJob` persisted before response | ✅ |
| FR14–FR17 Metadata + Suggestions | 4 | `GetMetadataSuggestionsQuery` + `IPostRecordRepository.GetRecentByChannelIdAsync` + `metadata-review.component` | ✅ |
| FR18–FR20 AI Generation Interface & Stub | 3 | `IVideoGenerationAdapter` interface + `StubVideoGenerationAdapter` in Infrastructure | ✅ |
| FR21–FR26 Posting, Fan-Out, Status, Retry | 6 | `PostCycleOrchestrationService` + `Task.WhenAll` + `PostCycleJobRepository` + `RetryPlatformCommand` + polling endpoint | ✅ |
| FR27–FR32 Post History | 6 | `PostRecord` collection + `IPostRecordRepository` + `PostRecordController` + `feature-post-history` | ✅ |
| FR33–FR37 Adapter Contracts & Extensibility | 5 | `ISocialMediaAdapter.Platform` discriminator + `IEnumerable<ISocialMediaAdapter>` injection + `YouTubeSocialMediaAdapter` | ✅ |

### Non-Functional Requirements Coverage

All 16 NFRs verified covered:

| NFR | Requirement | Architecture Coverage | Status |
|---|---|---|---|
| NFR1 | Dashboard load <1s | Channel list queries project summary fields only; no N+1 | ✅ |
| NFR2 | Post history load <2s | Paginated 20/page on indexed `(channelId, createdAt DESC)` | ✅ |
| NFR3 | Status updates <3s | Polling endpoint is lightweight indexed read on `PostCycleJob` | ✅ |
| NFR4 | AES-256 at rest | `AesEncryptionService` with random IV per token; key from `Encryption:Key` app setting | ✅ |
| NFR5 | Temp file deleted within 1hr | Job `finally` block + `TempFileCleanupService` IHostedService (30-min interval) | ✅ |
| NFR6 | Per-platform isolation | `Task.WhenAll` + individual try/catch per platform task | ✅ |
| NFR7 | PostRecord before response | `PostRecord` written atomically before `PostCycleJob.Status` set to terminal | ✅ |
| NFR8 | Retryable failures | `PlatformJob.Status` mutable post-completion; `RetryPlatformCommand` re-targets single platform | ✅ |
| NFR9 | Never expose plaintext tokens | Token decryption isolated to Infrastructure; Application layer never receives token value | ✅ |
| NFR10 | Orphaned file cleanup | `TempFileCleanupService.ExecuteAsync` deletes temp files older than 1hr | ✅ |
| NFR11 | Upload progress feedback | Angular `HttpClient` `reportProgress: true` + `observe: 'events'` in `PostCycleApiService` | ✅ |
| NFR12 | Adapter plug-in extensibility | New platform = implement `ISocialMediaAdapter` + register in `DependencyInjection.cs` only | ✅ |
| NFR13 | Job timeout enforcement | `TempFileCleanupService` marks `Running` jobs older than 10 minutes as `TimedOut` | ✅ |
| NFR14 | Polling timeout guard | `PostCycleStore` enforces 10-minute maximum polling duration with `takeUntil` | ✅ |
| NFR15 | No sequential adapter execution | `Task.WhenAll` mandated; sequential execution listed as hard anti-pattern | ✅ |
| NFR16 | JWT Bearer auth on all endpoints | `[Authorize]` on all controllers; exception for `/api/health` and `/api/oauth/callback` | ✅ |

### Gaps Identified and Resolved

**Gap 1 — Adapter Resolution Strategy** _(resolved during validation)_

- **Issue**: Architecture did not specify how `PostCycleOrchestrationService` selects the correct `ISocialMediaAdapter` when multiple platform adapters are registered in DI.
- **Resolution**: `ISocialMediaAdapter` includes `string Platform { get; }` discriminator property. `PostCycleOrchestrationService` receives `IEnumerable<ISocialMediaAdapter>` and selects with `.First(a => a.Platform == linkedAccount.Platform)`. Adding TikTok requires only: implement interface (with `Platform = "TikTok"`), register in DI — zero core changes.

**Gap 2 — Job Timeout Enforcement** _(resolved during structure step)_

- **Issue**: NFR13 (10-minute stalled job timeout) needed an owner.
- **Resolution**: `TempFileCleanupService` (IHostedService, 30-min interval) serves dual role — orphaned file cleanup (NFR10) and stalled `Running` job → `TimedOut` promotion (NFR13).

**Gap 3 — Upload Progress Reporting** _(resolved during validation)_

- **Issue**: NFR11 required upload progress feedback but the Angular pattern wasn't specified.
- **Resolution**: `PostCycleApiService` uses `HttpClient` with `reportProgress: true` + `observe: 'events'` + `filter(e => e.type === HttpEventType.UploadProgress)` to emit progress events to `PostCycleStore`.

### Architecture Completeness Checklist

- [x] All aggregate roots defined with clear ownership boundaries
- [x] All embedded sub-documents identified (LinkedAccount, PlatformResult, PlatformJob)
- [x] All MongoDB collections named and indexed
- [x] BSON ID generation strategy defined (repository layer, never Domain/Application)
- [x] Encryption strategy fully specified (AES-256, random IV, Infrastructure-only decryption)
- [x] OAuth flow fully specified (popup → redirect → callback → embed → polling detection)
- [x] Fan-out concurrency model defined (fire-and-forget Task.Run → Task.WhenAll)
- [x] Temp file lifecycle defined (stream → persist → finally cleanup → IHostedService safety-net)
- [x] Adapter interface contracts defined with result types
- [x] Adapter resolution mechanism specified (Platform discriminator)
- [x] API response envelope consistent (ServiceResponse<T>)
- [x] Angular store pattern specified with signal store template
- [x] Polling mechanism specified (3s interval, terminal states, 10-minute guard)
- [x] Upload progress feedback mechanism specified
- [x] JWT auth boundaries specified
- [x] IRepository<T, TId> as default; domain-specific interfaces only when needed
- [x] All naming conventions tabulated (backend + frontend)
- [x] Mandatory rules and anti-patterns documented
- [x] All FRs mapped to architecture locations
- [x] All NFRs mapped to architecture mechanisms

### Architecture Readiness Assessment

**Status: READY FOR IMPLEMENTATION**

**Confidence Level: High**

All 37 functional requirements and 16 non-functional requirements are covered by named architectural components. Three gaps identified during validation were resolved inline. No open decisions remain.

### Implementation Handoff — Priority Order

| Priority | Component | Reason |
|---|---|---|
| 1 | `IEncryptionService` + `AesEncryptionService` | Required before any LinkedAccount can be persisted |
| 2 | `Channel` aggregate root + `ChannelRepository` | Foundation for all other domain operations |
| 3 | `ChannelController` + Angular `feature-channels` | First user-visible functionality; validates API shape |
| 4 | OAuth flow (`OAuthController` + `LinkAccountCommand`) | Required before real posting can be tested end-to-end |
| 5 | `PostCycleJob` + `PostRecord` documents + repositories | Fan-out lifecycle backbone |
| 6 | `ISocialMediaAdapter` + `YouTubeSocialMediaAdapter` | First real posting capability |
| 7 | `PostCycleOrchestrationService` + `StartPostCycleCommand` | Wires fan-out; requires items 4, 5, 6 |
| 8 | `IVideoGenerationAdapter` + `StubVideoGenerationAdapter` | Stub only for MVP; can be built in parallel with item 7 |
| 9 | Angular `feature-post-cycle` + `feature-post-history` | Consumes all backend APIs; build after backend is stable |
