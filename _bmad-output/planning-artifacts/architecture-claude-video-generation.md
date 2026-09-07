---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments:
  - '_bmad-output/planning-artifacts/prd-claude-video-generation.md'
  - '_bmad-output/planning-artifacts/architecture-social-media-ai.md'
workflowType: 'architecture'
status: 'complete'
project_name: 'claude-higgsfield-video-generation'
user_name: 'Kampe'
date: '2026-08-29'
---

# Architecture Decision Document — Claude + Higgsfield AI Video Generation Feature

**Parent Architecture:** architecture-social-media-ai.md
**Feature PRD:** prd-claude-video-generation.md

---

## Project Context Analysis

### Scope of Change

This document covers the architectural decisions for replacing `StubVideoGenerationAdapter` with a real `HiggsFieldClaudeAdapter` and wiring the full video generation pipeline into the existing Social Media AI Tool. All decisions below are **additive** — no existing contracts, collections, or frontend flows are broken.

| Change Type | Components Affected |
|-------------|-------------------|
| New domain model | `VideoGenerationJob` aggregate |
| Interface extension | `VideoGenerationRequest` + `VideoGenerationResult` |
| New Infrastructure adapter | `HiggsFieldClaudeAdapter` (replaces `StubVideoGenerationAdapter`) |
| New Infrastructure service | `VideoGenerationOrchestrationService` |
| Channel model extension | Add `PromptTemplate` field |
| New commands + queries | `StartVideoGenerationCommand`, `GetVideoGenerationJobQuery`, `UpdatePromptTemplateCommand` |
| New controller | `VideoGenerationController` |
| Existing command extension | `StartPostCycleCommand` — accept `generationJobId` as alternative to file upload |
| Frontend — new store | `VideoGenerationStore` |
| Frontend — new components | `video-source-selector`, `image-upload`, `prompt-editor`, `generation-status` |
| Frontend — channel settings | Prompt template editor added to `channel-form.component` |
| New NuGet dependency | `Anthropic.SDK` |
| New config sections | `VideoGeneration:Anthropic`, `VideoGeneration:Higgsfield` |
| New MongoDB collection | `video_generation_jobs` |

### Requirements Overview

**Functional Requirements (from PRD):**

| FR | Description | Architectural Implication |
|----|-------------|--------------------------|
| FR-V1–V3 | Image upload (JPG/PNG/WebP ≤ 20MB), preview, replace | Multipart endpoint; image streamed to temp path; same streaming pattern as video upload |
| FR-V4–V8 | Per-channel prompt template; rendered with context + history; editable per-generation | `PromptTemplate` field on `Channel`; server-side rendering service; `UpdatePromptTemplateCommand` |
| FR-V9–V12 | Claude API call with image + rendered prompt + channel history | `Anthropic.SDK` in Infrastructure; image as base64 block; `IVideoGenerationAdapter` interface extended |
| FR-V13–V17 | Higgsfield hosted MCP; Claude auto-selects model; video downloaded to temp | MCP config passed per-request to Anthropic SDK; HTTP download of Higgsfield video URL |
| FR-V18–V21 | Polling (3s); status display; error + retry; 5-minute timeout | `VideoGenerationJob` document; lightweight polling endpoint; `IHostedService` timeout promotion |
| FR-V22–V23 | Post history records generation method + model + prompt | `PostRecord` extended with `GenerationMetadata?` sub-document |

**Non-Functional Requirements:**

| NFR | Impact |
|-----|--------|
| NFR-V1: Anthropic SDK retry (2x, exponential backoff) | `HiggsFieldClaudeAdapter` wraps SDK calls in Polly retry policy |
| NFR-V2: Image not persisted beyond cycle | Image temp file deleted in finally block after generation job completes or fails |
| NFR-V3: API keys never in code | `Anthropic:ApiKey` + `VideoGeneration:Higgsfield:AuthToken` from env/app settings |
| NFR-V4: Higgsfield endpoint configurable | `VideoGenerationSettings` model loaded from `appsettings.json` |
| NFR-V5: Polling ≤ 1 req / 3s | Angular `VideoGenerationStore` enforces 3s interval with `setInterval` |
| NFR-V6: Higgsfield API changes isolated | Only `HiggsFieldClaudeAdapter` changes; Claude integration layer + frontend unaffected |
| NFR-V7: Generated video temp file deleted after review | Tied to `PostCycleJob` terminal state (existing cleanup mechanism covers this) |

---

## Technical Foundation

### Stack Additions

All existing stack decisions from `architecture-social-media-ai.md` remain in force. The following are **net-new** additions:

**Backend:**

| Addition | Package / Technology | Purpose |
|----------|---------------------|---------|
| `Anthropic.SDK` | NuGet — `Anthropic.SDK` | Official C# client for Claude API — messages, tool use, MCP server configuration |
| `Microsoft.Extensions.Http.Resilience` | NuGet (already likely present) | Polly retry policy on Anthropic HTTP client |

**Frontend:**

No new npm packages required. Angular `HttpClient` handles image upload; polling uses existing `setInterval` pattern from `PostCycleStore`.

### Existing Patterns That Apply Unchanged

- `Entity<string>` / `Aggregate<string>` for new domain model
- `IRepository<T, TId>` as default repository contract
- `Task.Run` fire-and-forget for async orchestration; new `VideoGenerationOrchestrationService`
- `ServiceResponse<T>` response envelope on all new endpoints
- `IHostedService` for background cleanup + timeout promotion
- `[Authorize]` on all new controller endpoints
- Signal store `status: 'idle' | 'loading' | 'success' | 'error'` shape
- 3-second polling with terminal-state stop condition
- Temp file stream-to-disk (never buffer in memory)

---

## Core Architectural Decisions

### Decision 1 — IVideoGenerationAdapter Interface Evolution

**Problem:** The existing `IVideoGenerationAdapter` returns only metadata (title, description, hashtags). The real implementation must also return a video file path. The stub must continue to work unchanged.

**Decision:** Extend `VideoGenerationResult` with a nullable `VideoPath` property. The stub returns `null`. The real adapter returns the local temp path of the downloaded video. Callers check `VideoPath != null` to determine if a video was produced.

```csharp
// BEFORE (stub returns this, unchanged)
public class VideoGenerationResult
{
    public string Title { get; set; }
    public string Description { get; set; }
    public List<string> Hashtags { get; set; }
}

// AFTER — additive change, backward compatible
public class VideoGenerationResult
{
    public string Title { get; set; }
    public string Description { get; set; }
    public List<string> Hashtags { get; set; }
    public string? VideoPath { get; set; }         // ← NEW: null = stub path, non-null = real video
    public string? HiggsFieldModel { get; set; }   // ← NEW: model Claude selected (for history record)
}
```

**Extend VideoGenerationRequest with image input:**

```csharp
public class VideoGenerationRequest
{
    // Existing — unchanged
    public string ChannelName { get; set; }
    public string StyleToneContext { get; set; }
    public List<PostRecordSnapshot> RecentHistory { get; set; }
    public string UserPrompt { get; set; }

    // NEW — image input for real adapter; null for stub path
    public string? ImageTempPath { get; set; }     // local path to uploaded image
    public string? RenderedPrompt { get; set; }    // full rendered prompt (template + context + override)
    public int TargetDurationSeconds { get; set; } = 15; // default 15s (10–20s range)
}
```

**Rationale:** Avoids a second interface. Stub ignores the new fields. Real adapter uses them. The `null` check on `VideoPath` is the only branching needed in the orchestration layer.

---

### Decision 2 — VideoGenerationJob Document (Separate Collection)

**Problem:** Video generation via Claude + Higgsfield is async and takes 1–3 minutes. The frontend must be able to poll status. We need a document to track generation state.

**Decision:** New `VideoGenerationJob` aggregate in a separate `video_generation_jobs` collection. Mirrors the `PostCycleJob` pattern.

```
VideoGenerationJob {
  _id: ObjectId (string in C#)
  channelId: string
  status: string               // "Queued" | "Generating" | "Ready" | "Failed" | "TimedOut"
  imageTempPath: string        // path to uploaded reference image
  videoTempPath: string?       // path to downloaded generated video (set when Ready)
  renderedPrompt: string       // full prompt sent to Claude (stored for history)
  higgsFieldModel: string?     // model Claude selected (returned in tool result)
  errorMessage: string?        // set on failure
  createdAt: DateTime
  completedAt: DateTime?
}
```

**MongoDB Collections update:**

| Collection | Index |
|------------|-------|
| `channels` | `_id` (default) |
| `post_records` | `(channelId, createdAt DESC)` |
| `post_cycle_jobs` | `channelId` |
| `video_generation_jobs` | `channelId` ← NEW |

**Rationale:** Reusing `PostCycleJob` would conflate two distinct lifecycles. `VideoGenerationJob` is pre-posting; `PostCycleJob` is posting. Separation keeps each document's status machine clean.

---

### Decision 3 — Channel PromptTemplate Field

**Decision:** Add `PromptTemplate` (nullable string) to the `Channel` aggregate. When null, the system default template is used. When set, it overrides the default.

```csharp
// Channel.cs — add one field
public string? PromptTemplate { get; private set; }  // null = use system default
```

**System default template** (compiled into `PromptTemplateRenderer`):

```
You are creating a short social media video for the channel "{ChannelName}".

Channel style: {StyleToneContext}

Recent post context (last 5 posts):
{RecentPostHistory}

Video request: {UserPrompt}

Using the attached reference image and the Higgsfield video generation tool, create a {TargetDurationSeconds}-second short-form video that matches this channel's style and the request above. Select the best available Higgsfield model for the content type.
```

**New command:** `UpdatePromptTemplateCommand` — sets or clears the channel's custom template. Routed through `ChannelController` (existing base route `/api/channels`).

---

### Decision 4 — HiggsFieldClaudeAdapter Implementation

**Decision:** `HiggsFieldClaudeAdapter` implements `IVideoGenerationAdapter`. It is registered in `Infrastructure/DependencyInjection.cs`, replacing `StubVideoGenerationAdapter` when `VideoGeneration:Provider = "HiggsFieldClaude"` in app settings. The stub remains available for local dev by changing config.

**Provider switching via DI (app settings controlled):**

```csharp
// Infrastructure/DependencyInjection.cs
var provider = configuration["VideoGeneration:Provider"];
if (provider == "HiggsFieldClaude")
    services.AddScoped<IVideoGenerationAdapter, HiggsFieldClaudeAdapter>();
else
    services.AddScoped<IVideoGenerationAdapter, StubVideoGenerationAdapter>();
```

**Adapter internal flow:**

```
HiggsFieldClaudeAdapter.GenerateAsync(request)

1. Read image bytes from request.ImageTempPath
2. Base64-encode image bytes

3. Build Anthropic API request:
   - model: "claude-opus-4-5" (from config)
   - messages:
       role: user
       content:
         - image_block: { type: base64, media_type: image/*, data: <base64> }
         - text_block: request.RenderedPrompt
   - mcp_servers:
       - type: "url"
         url: <Higgsfield:McpEndpoint from config>
         name: "higgsfield"
         authorization_token: <Higgsfield:AuthToken from config>

4. Call AnthropicClient.Messages.CreateAsync(request)
   - Claude receives image + prompt
   - Claude calls Higgsfield MCP tool with image ref + prompt + duration
   - Higgsfield generates video, returns URL in tool_result
   - Claude returns tool_result with video URL to backend

5. Extract video URL from message.Content (ToolResultBlock)

6. Download video from URL to local temp path:
   HttpClient.GetStreamAsync(videoUrl) → File.WriteAllBytesAsync(tempPath)

7. Return VideoGenerationResult:
   - VideoPath: tempPath
   - HiggsFieldModel: extracted from tool_result metadata (if available)
   - Title/Description/Hashtags: empty strings (metadata suggestions come from existing GetMetadataSuggestionsQuery)
```

**Anthropic SDK usage pattern:**

```csharp
var response = await _anthropicClient.Messages.CreateAsync(new MessageRequest
{
    Model = _settings.Anthropic.Model,
    MaxTokens = 1024,
    McpServers = new List<McpServerConfig>
    {
        new McpServerConfig
        {
            Type = "url",
            Url = _settings.Higgsfield.McpEndpoint,
            Name = "higgsfield",
            AuthorizationToken = _settings.Higgsfield.AuthToken
        }
    },
    Messages = new List<Message>
    {
        new Message
        {
            Role = "user",
            Content = new List<ContentBlock>
            {
                new ImageContentBlock
                {
                    Type = "base64",
                    MediaType = imageMediaType,   // "image/jpeg" | "image/png" | "image/webp"
                    Data = base64ImageData
                },
                new TextContentBlock { Text = request.RenderedPrompt }
            }
        }
    }
});

// Extract video URL from tool_result in response content
var videoUrl = ExtractVideoUrlFromResponse(response);
```

**Retry policy (NFR-V1):** Wrap SDK call in a `ResiliencePipeline` with 2 retries, exponential backoff (2s → 4s), on `HttpRequestException` and `AnthropicException` (transient status codes). Non-retriable errors (4xx client errors) propagate immediately.

---

### Decision 5 — VideoGenerationOrchestrationService

**Decision:** New `IVideoGenerationOrchestrationService` + `VideoGenerationOrchestrationService` in Infrastructure. Follows the same fire-and-forget `Task.Run` pattern as `PostCycleOrchestrationService`.

**Orchestration flow:**

```
VideoGenerationOrchestrationService.RunAsync(jobId, cancellationToken)

1. Load VideoGenerationJob from repository
2. Update status → Generating (persist)
3. Load Channel (get context for request construction)
4. Load recent PostRecord history (last 5 for context)
5. Render full prompt via PromptTemplateRenderer:
     channel.PromptTemplate ?? systemDefault
     + inject: ChannelName, StyleToneContext, RecentHistory, UserPrompt
6. Build VideoGenerationRequest
7. Call IVideoGenerationAdapter.GenerateAsync(request) [wrapped in try/catch]
8a. On success:
    - Update VideoGenerationJob: status → Ready, videoTempPath, higgsFieldModel
    - Delete image temp file (in finally)
8b. On failure:
    - Update VideoGenerationJob: status → Failed, errorMessage
    - Delete image temp file (in finally)
```

**StartVideoGenerationCommandHandler** (Application layer):

```
StartVideoGenerationCommandHandler.Handle(command)

1. Validate: channelId exists, image file not null, image ≤ 20MB, type in [jpg, png, webp]
2. Stream image to TempStorage:Path/{guid}.{ext}
3. Create VideoGenerationJob (status: Queued), persist
4. Task.Run(() => VideoGenerationOrchestrationService.RunAsync(jobId))
5. Return jobId  ← controller returns immediately
```

---

### Decision 6 — Post Cycle Integration (Generated Video Path)

**Problem:** After generation completes, the frontend must trigger a post cycle using the generated video, not a file upload.

**Decision:** Extend `StartPostCycleCommand` to accept a `VideoGenerationJobId` as an alternative source to a file upload. Exactly one of (`VideoFile`, `VideoGenerationJobId`) must be provided (validated via FluentValidation).

```csharp
public class StartPostCycleCommand : IRequest<ServiceResponse<StartPostCycleResult>>
{
    public string ChannelId { get; set; }
    public IFormFile? VideoFile { get; set; }           // existing — manual upload path
    public string? VideoGenerationJobId { get; set; }   // NEW — AI generation path
    public string Title { get; set; }
    public string Description { get; set; }
    public List<string> Hashtags { get; set; }
}
```

**Handler extension:**

```csharp
// In StartPostCycleCommandHandler
string videoPath;
if (command.VideoGenerationJobId != null)
{
    // AI generation path
    var generationJob = await _videoGenerationJobRepo.GetByIdAsync(command.VideoGenerationJobId);
    if (generationJob?.Status != "Ready")
        return ServiceResponse.Failure("Generation job is not ready");
    videoPath = generationJob.VideoTempPath!;
    // Mark generation job as consumed (status: Consumed) so cleanup knows it's in use
    await _videoGenerationJobRepo.MarkConsumedAsync(command.VideoGenerationJobId);
}
else
{
    // Manual upload path — existing logic unchanged
    videoPath = await StreamVideoToDiskAsync(command.VideoFile);
}
// ... rest of existing StartPostCycleCommand handler unchanged
```

**Rationale:** The post cycle from this point is identical regardless of video source. No duplication in orchestration, review step, posting, or history recording.

---

### Decision 7 — PostRecord GenerationMetadata Extension

**Decision:** Add optional `GenerationMetadata` sub-document to `PostRecord` for AI-generated posts (FR-V22–V23).

```csharp
// PostRecord.cs — add one nullable field
public GenerationMetadata? GenerationMetadata { get; private set; }
```

```csharp
public class GenerationMetadata
{
    public string Method { get; set; }         // "higgsfield-claude-mcp"
    public string? HiggsFieldModel { get; set; }
    public string RenderedPrompt { get; set; }
    public string ImageReference { get; set; } // original filename (not path)
}
```

Written by `StartPostCycleCommandHandler` when `VideoGenerationJobId` is the source. `null` for manual upload posts — no change to existing records.

---

### Decision 8 — PromptTemplateRenderer (Application Service)

**Decision:** A simple `PromptTemplateRenderer` static class in `Application/VideoGeneration/` handles prompt rendering. No interface required — it's pure, stateless string interpolation.

```csharp
public static class PromptTemplateRenderer
{
    private const string DefaultTemplate = """
        You are creating a short social media video for the channel "{ChannelName}".

        Channel style: {StyleToneContext}

        Recent post context (last 5 posts):
        {RecentPostHistory}

        Video request: {UserPrompt}

        Using the attached reference image and the Higgsfield video generation tool,
        create a {TargetDurationSeconds}-second short-form video that matches this
        channel's style and the request above. Select the best available Higgsfield model.
        """;

    public static string Render(
        string? channelTemplate,
        string channelName,
        string styleToneContext,
        IEnumerable<PostRecordSnapshot> recentHistory,
        string userPrompt,
        int targetDurationSeconds = 15)
    {
        var template = string.IsNullOrWhiteSpace(channelTemplate)
            ? DefaultTemplate
            : channelTemplate;

        var historyBlock = BuildHistoryBlock(recentHistory);

        return template
            .Replace("{ChannelName}", channelName)
            .Replace("{StyleToneContext}", styleToneContext)
            .Replace("{RecentPostHistory}", historyBlock)
            .Replace("{UserPrompt}", userPrompt)
            .Replace("{TargetDurationSeconds}", targetDurationSeconds.ToString());
    }

    private static string BuildHistoryBlock(IEnumerable<PostRecordSnapshot> history)
    {
        var entries = history.Take(5).Select((p, i) =>
            $"{i + 1}. \"{p.Title}\" — {p.Description[..Math.Min(80, p.Description.Length)]}...");
        return string.Join("\n", entries);
    }
}
```

---

### Decision 9 — Timeout Promotion (TempFileCleanupService Extension)

**Decision:** Extend the existing `TempFileCleanupService` to also promote stalled `Queued` or `Generating` `VideoGenerationJob` records to `TimedOut` after 5 minutes (NFR-V5 timeout = 5 minutes, shorter than the 10-minute post cycle timeout).

```csharp
// TempFileCleanupService — additional check added to existing ExecuteAsync loop
var stalledGenerationJobs = await _videoGenerationJobRepo
    .GetStalledAsync(olderThanMinutes: 5); // Queued or Generating for > 5 min

foreach (var job in stalledGenerationJobs)
{
    await _videoGenerationJobRepo.UpdateStatusAsync(job.Id, "TimedOut");
    CleanupTempFile(job.ImageTempPath);
    CleanupTempFile(job.VideoTempPath); // null-safe
}
```

**Rationale:** Reuses the existing background service rather than introducing a new one. Single place for all temp file and job lifecycle cleanup.

---

## API & Communication

### New Endpoints

| Method | Route | Auth | Command/Query | Description |
|--------|-------|------|---------------|-------------|
| `POST` | `/api/video-generation/start` | `[Authorize]` | `StartVideoGenerationCommand` | Upload image, start async generation |
| `GET` | `/api/video-generation/{jobId}` | `[Authorize]` | `GetVideoGenerationJobQuery` | Poll generation status |
| `PATCH` | `/api/channels/{id}/prompt-template` | `[Authorize]` | `UpdatePromptTemplateCommand` | Save/clear channel prompt template |

### Updated Endpoint

| Method | Route | Change |
|--------|-------|--------|
| `POST` | `/api/post-cycles/start` | Accepts `videoGenerationJobId` (string) as alternative to file upload via multipart form field |

### VideoGenerationController

```csharp
[ApiController]
[Route("api/video-generation")]
[Authorize]
public class VideoGenerationController : ControllerBase
{
    [HttpPost("start")]
    [RequestSizeLimit(20_971_520)] // 20MB limit for image — separate from video endpoint
    public async Task<IActionResult> Start([FromForm] StartVideoGenerationRequest request) { ... }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetStatus(string jobId) { ... }
}
```

**Note:** Separate `RequestSizeLimit` of 20MB on this controller overrides the 2GB Kestrel limit for video uploads. Image-only endpoint needs a tighter constraint.

### Request/Response Shapes

**POST /api/video-generation/start (multipart form)**
```
channelId: string (required)
image: IFormFile (required, ≤ 20MB, JPG/PNG/WebP)
promptOverride: string? (optional — replaces UserPrompt token in template)
targetDurationSeconds: int? (optional, default: 15, range: 10–20)
```

**Response:**
```json
{
  "data": { "jobId": "66f1a234b5c6d7e8f9012345" },
  "success": true,
  "message": null
}
```

**GET /api/video-generation/{jobId} (polling)**
```json
{
  "data": {
    "id": "66f1a234b5c6d7e8f9012345",
    "channelId": "66e9b123a4b5c6d7e8f90123",
    "status": "Generating",
    "higgsFieldModel": null,
    "renderedPrompt": "You are creating a short social media video...",
    "errorMessage": null,
    "createdAt": "2026-08-29T10:30:00Z",
    "completedAt": null
  },
  "success": true,
  "message": null
}
```

### End-to-End Flow Diagrams

**AI Generation Path — Full Sequence:**

```
[Frontend]                        [Backend]                      [External]
    |                                 |                               |
    |-- POST /api/video-generation/start --------------------------------->|
    |   (image + channelId + promptOverride)                        |
    |                    |-- Stream image to temp/{guid}.jpg        |
    |                    |-- Load Channel (template + context)      |
    |                    |-- Load PostRecord history (last 5)       |
    |                    |-- Render full prompt                     |
    |                    |-- Create VideoGenerationJob (Queued)     |
    |                    |-- Task.Run(orchestration)                |
    |<-- { jobId } -------|                                         |
    |                    |                                          |
    |-- [every 3s] GET /api/video-generation/{jobId} --------------|
    |<-- { status: "Generating" } ----|                            |
    |                    |                                          |
    |                    | [Background Task]                        |
    |                    |-- Update job → Generating               |
    |                    |-- AnthropicClient.Messages.CreateAsync  |
    |                    |   (image + prompt + Higgsfield MCP) --->| Claude API
    |                    |                                     -----| Claude calls Higgsfield MCP
    |                    |                                     -----| Higgsfield generates 15s video
    |                    |                                    <-----| tool_result: { videoUrl }
    |                    |<-- Claude response with video URL --------|
    |                    |-- HttpClient.GetStreamAsync(videoUrl) -->| Higgsfield CDN
    |                    |<-- video stream -----------------------  |
    |                    |-- Write video to temp/{guid}.mp4         |
    |                    |-- Update job → Ready (videoTempPath set) |
    |                    |-- Delete image temp file                 |
    |                    |                                          |
    |-- GET /api/video-generation/{jobId} --------------------------|
    |<-- { status: "Ready" } ---------|                            |
    |                                 |                             |
    |-- POST /api/post-cycles/start --------------------------------|
    |   (videoGenerationJobId + metadata)                          |
    |                    |-- Load VideoGenerationJob (Ready)        |
    |                    |-- videoPath = job.VideoTempPath          |
    |                    |-- Mark job Consumed                      |
    |                    |-- [EXISTING post cycle flow unchanged]   |
    |<-- { jobId } -------|                                         |
    |                                 |                             |
    |-- [existing 3s polling on PostCycleJob] ----------------------|
```

---

## Frontend Architecture

### New Store — VideoGenerationStore

```typescript
// libs/social-media/social-media-data-access/src/lib/
// video-generation.store.ts

interface VideoGenerationJob {
  id: string;
  channelId: string;
  status: 'Queued' | 'Generating' | 'Ready' | 'Failed' | 'TimedOut';
  higgsFieldModel?: string;
  renderedPrompt: string;
  errorMessage?: string;
  createdAt: string;
  completedAt?: string;
}

interface VideoGenerationState {
  generationStatus: 'idle' | 'loading' | 'success' | 'error';
  currentJob: VideoGenerationJob | null;
  error: string | null;
}

export const VideoGenerationStore = signalStore(
  { providedIn: 'root' },
  withState<VideoGenerationState>({
    generationStatus: 'idle',
    currentJob: null,
    error: null,
  }),
  withComputed((state) => ({
    isGenerating: computed(() =>
      ['loading', 'success'].includes(state.generationStatus()) &&
      ['Queued', 'Generating'].includes(state.currentJob()?.status ?? '')),
    isReady: computed(() => state.currentJob()?.status === 'Ready'),
    isFailed: computed(() =>
      ['Failed', 'TimedOut'].includes(state.currentJob()?.status ?? '')),
  })),
  withMethods((store, api = inject(VideoGenerationApiService)) => {
    let pollTimer: ReturnType<typeof setInterval> | null = null;
    let pollCount = 0;
    const maxPollCount = 100; // 100 × 3s = 5 minutes

    return {
      async startGeneration(formData: FormData): Promise<string | null> { ... },
      startPolling(jobId: string): void {
        // Polls every 3s; stops on Ready, Failed, TimedOut, or 5-min timeout
      },
      stopPolling(): void { ... },
      resetGeneration(): void { ... },
    };
  })
);
```

### New API Service — VideoGenerationApiService

```typescript
// libs/social-media/social-media-data-access/src/lib/
// video-generation.api.ts

@Injectable({ providedIn: 'root' })
export class VideoGenerationApiService {
  private readonly base = `${environment.apiUrl}${environment.socialMediaApi}video-generation`;

  startGeneration(formData: FormData): Observable<ServiceResponse<{ jobId: string }>> { ... }
  getGenerationJob(jobId: string): Observable<ServiceResponse<VideoGenerationJob>> { ... }
}
```

### New TypeScript Models

```typescript
// post-cycle.models.ts additions
interface StartPostCycleWithGenerationRequest {
  channelId: string;
  videoGenerationJobId: string;  // used instead of file upload
  title: string;
  description: string;
  hashtags: string[];
}

// channel.models.ts additions
interface ChannelDetail {
  // ...existing fields...
  promptTemplate: string | null;  // NEW: null = using system default
}

interface UpdatePromptTemplateRequest {
  promptTemplate: string | null;  // null to reset to system default
}
```

### New Feature Components

All new components added to `libs/social-media/feature-post-cycle/src/lib/`:

| Component | File | Responsibility |
|-----------|------|---------------|
| `VideoSourceSelectorComponent` | `video-source-selector/` | Toggle: "Upload Video" vs "Generate with AI" tab |
| `ImageUploadComponent` | `image-upload/` | File picker (JPG/PNG/WebP ≤ 20MB), preview, replace |
| `PromptEditorComponent` | `prompt-editor/` | Display rendered template; allow free-text override; show channel template source |
| `GenerationStatusComponent` | `generation-status/` | Status indicator (Queued/Generating/Ready/Failed); progress message; error + retry button |

**Channel settings extension:**
`channel-form.component` — add "Prompt Template" textarea field with placeholder showing the system default. `null` = cleared/using default.

### Frontend State Flow — AI Generation Path

```
[PostCycleComponent]
  |
  ├── [User selects "Generate with AI" tab]
  |     VideoSourceSelectorComponent emits mode change
  |
  ├── [ImageUploadComponent] → user uploads reference image
  |
  ├── [PromptEditorComponent] → shows rendered template; user edits prompt override
  |
  ├── [User clicks "Generate"]
  |     VideoGenerationStore.startGeneration(formData)
  |       → POST /api/video-generation/start
  |       → store.generationStatus = 'loading'
  |       → startPolling(jobId)
  |
  ├── [GenerationStatusComponent] polls every 3s
  |     → status: Queued | Generating → show spinner + message
  |     → status: Ready → stop polling
  |       PostCycleStore.startPostCycleFromGeneration(jobId, metadata)
  |         → POST /api/post-cycles/start (videoGenerationJobId)
  |         → existing polling → existing MetadataReviewComponent
  |     → status: Failed | TimedOut → show error + "Retry" + "Upload instead" fallback
```

---

## Project Structure — Net-New Files

`★ MODIFY` = existing file changed; unmarked = new file.

```
services/SocialMediaAPI/
│
├── Domain/
│   └── VideoGenerationJob/
│       ├── VideoGenerationJob.cs                  # Aggregate<string>; status state machine
│       └── VideoGenerationJobStatus.cs            # const strings: Queued|Generating|Ready|Failed|TimedOut
│
├── Application/
│   ├── Common/Interfaces/
│   │   └── IVideoGenerationJobRepository.cs       # domain-specific queries (GetStalledAsync, MarkConsumedAsync)
│   ├── VideoGeneration/
│   │   ├── Commands/
│   │   │   └── StartVideoGenerationCommand.cs     # + Handler + Validator (image ≤20MB, type check)
│   │   ├── Queries/
│   │   │   └── GetVideoGenerationJobQuery.cs      # + Handler
│   │   ├── Dto/
│   │   │   └── VideoGenerationJobResponse.cs
│   │   └── PromptTemplateRenderer.cs              # static; renders template with channel context
│   ├── Channel/
│   │   └── Commands/
│   │       └── UpdatePromptTemplateCommand.cs     # ★ NEW: + Handler + Validator; sets/clears template
│   ├── Adapters/
│   │   ├── VideoGenerationRequest.cs              # ★ MODIFY: add ImageTempPath, RenderedPrompt, TargetDurationSeconds
│   │   └── VideoGenerationResult.cs              # ★ MODIFY: add VideoPath?, HiggsFieldModel?
│   └── PostCycle/
│       └── Commands/
│           └── StartPostCycleCommand.cs           # ★ MODIFY: add VideoGenerationJobId?; validator updated
│
├── Infrastructure/
│   ├── Adapters/
│   │   └── HiggsFieldClaudeAdapter.cs             # implements IVideoGenerationAdapter; Anthropic SDK + MCP
│   ├── Repositories/
│   │   └── VideoGenerationJobRepository.cs        # extends MongoRepository<VideoGenerationJob, string>
│   ├── Services/
│   │   ├── VideoGenerationOrchestrationService.cs # fire-and-forget; runs adapter; updates job status
│   │   ├── TempFileCleanupService.cs              # ★ MODIFY: add stalled VideoGenerationJob → TimedOut
│   │   └── PromptTemplateRenderer.cs              # (static — referenced, not registered)
│   ├── Settings/
│   │   └── VideoGenerationSettings.cs             # config model: Provider, Anthropic{}, Higgsfield{}
│   └── DependencyInjection.cs                     # ★ MODIFY: register new adapter, repo, orchestration svc
│
└── WebAPI/
    ├── Controllers/
    │   └── VideoGenerationController.cs           # POST start, GET {jobId}; 20MB size limit
    ├── appsettings.json                            # ★ MODIFY: add VideoGeneration section
    └── appsettings.Development.json               # ★ MODIFY: dev overrides (stub provider, test keys)
```

```
libs/social-media/
│
├── social-media-data-access/src/lib/
│   ├── models/
│   │   ├── channel.models.ts                      # ★ MODIFY: add promptTemplate to ChannelDetail
│   │   ├── post-cycle.models.ts                   # ★ MODIFY: add StartPostCycleWithGenerationRequest
│   │   └── video-generation.models.ts             # NEW: VideoGenerationJob, VideoGenerationState
│   ├── video-generation.store.ts                  # NEW: VideoGenerationStore (NgRx Signals)
│   └── video-generation.api.ts                    # NEW: VideoGenerationApiService
│
└── feature-post-cycle/src/lib/
    ├── video-source-selector/
    │   └── video-source-selector.component.ts     # NEW: Upload | Generate tab toggle
    ├── image-upload/
    │   └── image-upload.component.ts              # NEW: image picker + preview
    ├── prompt-editor/
    │   └── prompt-editor.component.ts             # NEW: rendered template + override input
    ├── generation-status/
    │   └── generation-status.component.ts         # NEW: status display + polling integration
    └── post-cycle.component.ts                    # ★ MODIFY: orchestrate new sub-components

(channel form update)
└── feature-channels/src/lib/
    └── channel-form/
        └── channel-form.component.ts              # ★ MODIFY: add prompt template textarea
```

---

## Configuration

### appsettings.json additions

```json
{
  "VideoGeneration": {
    "Provider": "HiggsFieldClaude",
    "Anthropic": {
      "ApiKey": "",
      "Model": "claude-opus-4-5"
    },
    "Higgsfield": {
      "McpEndpoint": "https://mcp.higgsfield.ai/mcp",
      "AuthToken": "",
      "TargetDurationSeconds": 15
    }
  }
}
```

### appsettings.Development.json additions

```json
{
  "VideoGeneration": {
    "Provider": "Stub"
  }
}
```

**Convention:** Development defaults to stub. Switching to real requires only setting `Provider = "HiggsFieldClaude"` and populating the API key + auth token in environment variables or user secrets. Never commit credentials.

---

## Implementation Patterns & Consistency Rules

### New Naming Additions

Following conventions from `architecture-social-media-ai.md`:

| Artifact | Name |
|----------|------|
| Domain entity | `VideoGenerationJob` |
| Status constants | `VideoGenerationJobStatus` (string constants, not enum) |
| Repository interface | `IVideoGenerationJobRepository` (domain-specific: `GetStalledAsync`, `MarkConsumedAsync`) |
| Repository implementation | `VideoGenerationJobRepository` |
| Infrastructure adapter | `HiggsFieldClaudeAdapter` |
| Orchestration service | `VideoGenerationOrchestrationService` |
| Application service (static) | `PromptTemplateRenderer` |
| Controller | `VideoGenerationController` |
| Angular store | `VideoGenerationStore` |
| Angular API service | `VideoGenerationApiService` |
| Angular model | `VideoGenerationJob` |
| Angular state interface | `VideoGenerationState` |

### New Mandatory Rules

Following and extending the mandatory rules from the parent architecture doc:

- Never read image file into memory all at once for upload — stream to disk, then read for base64 encoding only at generation time
- Always delete image temp file in `finally` regardless of generation outcome
- Never hardcode the Higgsfield MCP endpoint URL — always from `VideoGenerationSettings`
- Never hardcode the Anthropic API key or Higgsfield auth token — always from environment / app settings
- Always pass `TargetDurationSeconds` in the rendered prompt — never let Claude infer duration
- `StartPostCycleCommand` validator must reject requests where both `VideoFile` and `VideoGenerationJobId` are null, and reject requests where both are provided simultaneously
- `HiggsFieldClaudeAdapter` must never throw — return `VideoGenerationResult` with `VideoPath = null` and log the error; the orchestration service translates null path to job failure
- `VideoGenerationJob` must be marked `Consumed` before `PostCycleJob` is created to prevent double-use of the generated video temp file

### Anti-Patterns to Avoid

- Buffering the reference image in memory — use `IFormFile.CopyToAsync` stream pattern
- Hardcoding the system default prompt template in the adapter — it belongs in `PromptTemplateRenderer`
- Running `HiggsFieldClaudeAdapter` synchronously on the request thread — always `Task.Run` via `VideoGenerationOrchestrationService`
- Storing the Higgsfield video URL as the final video reference — always download to local temp file before handing to post cycle
- Using a new `IHostedService` for VideoGenerationJob cleanup — extend `TempFileCleanupService`

---

## Architecture Validation

### Functional Requirements Coverage

| FR | Description | Architecture Coverage | Status |
|----|-------------|----------------------|--------|
| FR-V1 | Image upload ≤ 20MB, JPG/PNG/WebP | `VideoGenerationController` 20MB limit; `StartVideoGenerationCommandHandler` validates type | ✅ |
| FR-V2 | Preview uploaded image | `ImageUploadComponent` — client-side FileReader preview before submit | ✅ |
| FR-V3 | Replace image before submitting | `ImageUploadComponent` — re-select clears previous selection | ✅ |
| FR-V4 | Per-channel prompt template stored | `Channel.PromptTemplate` nullable field; `UpdatePromptTemplateCommand` | ✅ |
| FR-V5 | Template pre-populated with channel context | `PromptTemplateRenderer.Render()` — injects ChannelName, StyleToneContext | ✅ |
| FR-V6 | Edit rendered prompt inline | `PromptEditorComponent` — displays rendered template; `promptOverride` field in request | ✅ |
| FR-V7 | Edit/save base template from settings | `channel-form.component` template textarea; `UpdatePromptTemplateCommand` | ✅ |
| FR-V8 | Render: channel name + style + history + user text | `PromptTemplateRenderer.Render()` — all four tokens | ✅ |
| FR-V9 | Backend sends image + prompt to Claude | `HiggsFieldClaudeAdapter` — base64 image block + text block in message | ✅ |
| FR-V10 | Claude uses Higgsfield MCP tool | `mcp_servers` config in Anthropic SDK request; Claude auto-invokes Higgsfield tool | ✅ |
| FR-V11 | Claude response (video URL) passed to post cycle | Adapter extracts URL, downloads to temp, returns `VideoPath`; `StartPostCycleCommand` uses it | ✅ |
| FR-V12 | Last N post history as Claude context | `PromptTemplateRenderer` — `{RecentPostHistory}` token from last 5 PostRecords | ✅ |
| FR-V13 | Higgsfield hosted MCP — no self-hosting | `mcp_servers` URL config points to `https://mcp.higgsfield.ai/mcp` | ✅ |
| FR-V14 | MCP endpoint + auth from config | `VideoGenerationSettings.Higgsfield` — never hardcoded | ✅ |
| FR-V15 | 10–20 second target duration | `TargetDurationSeconds` from config (default 15); injected into rendered prompt | ✅ |
| FR-V16 | Claude auto-selects Higgsfield model | No model override in prompt — Claude chooses from Higgsfield MCP tool | ✅ |
| FR-V17 | Video URL downloaded to temp file | `HiggsFieldClaudeAdapter` — `HttpClient.GetStreamAsync` → temp file | ✅ |
| FR-V18 | Frontend polls every 3s | `VideoGenerationStore.startPolling()` — `setInterval(3000)` | ✅ |
| FR-V19 | Status display (Queued/Generating/Ready/Failed) | `GenerationStatusComponent` — maps `VideoGenerationJob.status` | ✅ |
| FR-V20 | Error: human-readable + retry + fallback to upload | `GenerationStatusComponent` — error state + Retry button + "Upload instead" link | ✅ |
| FR-V21 | 5-minute generation timeout | `TempFileCleanupService` promotes stalled jobs > 5 min to `TimedOut` | ✅ |
| FR-V22 | Post history: generation method + model + prompt | `PostRecord.GenerationMetadata?` sub-document | ✅ |
| FR-V23 | Generation details viewable in history | `post-record-detail.component` — render `GenerationMetadata` if present | ✅ |

### Non-Functional Requirements Coverage

| NFR | Requirement | Coverage | Status |
|-----|-------------|----------|--------|
| NFR-V1 | Anthropic SDK retry (2x, exponential backoff) | `ResiliencePipeline` wrapping SDK call in `HiggsFieldClaudeAdapter` | ✅ |
| NFR-V2 | Image not persisted beyond cycle | `finally` block in `VideoGenerationOrchestrationService.RunAsync` | ✅ |
| NFR-V3 | API keys never in code | `VideoGenerationSettings` from `IConfiguration`; env var / user secrets | ✅ |
| NFR-V4 | Higgsfield endpoint configurable | `VideoGenerationSettings.Higgsfield.McpEndpoint` | ✅ |
| NFR-V5 | Polling ≤ 1 req/3s | `VideoGenerationStore.startPolling()` — `setInterval(3000)` + `clearInterval` on terminal state | ✅ |
| NFR-V6 | Higgsfield API changes isolated | Only `HiggsFieldClaudeAdapter` changes; interface + frontend unaffected | ✅ |
| NFR-V7 | Generated video temp file deleted after review | Tied to `PostCycleJob` terminal cleanup (existing `TempFileCleanupService` covers `videoTempPath`) | ✅ |

### Architecture Completeness Checklist

- [x] `VideoGenerationJob` aggregate defined with full status state machine
- [x] `VideoGenerationJob` MongoDB collection named and indexed
- [x] `IVideoGenerationAdapter` interface extended backward-compatibly
- [x] `Channel.PromptTemplate` field defined and owned by domain
- [x] `PromptTemplateRenderer` stateless service defined
- [x] `HiggsFieldClaudeAdapter` Anthropic SDK call structure defined
- [x] MCP server configuration pattern specified (per-request, not global)
- [x] Retry policy defined (2x, exponential backoff, transient errors only)
- [x] Image upload size limit explicitly bounded at controller level (20MB)
- [x] Image temp file lifecycle defined (stream → disk → base64 at generation time → delete in finally)
- [x] Generated video temp file lifecycle defined (download → hand to post cycle → existing cleanup)
- [x] `StartPostCycleCommand` extension defined (either file OR generationJobId — not both)
- [x] `PostRecord.GenerationMetadata` sub-document defined
- [x] `VideoGenerationOrchestrationService` fire-and-forget pattern defined
- [x] `TempFileCleanupService` extension defined (5-min timeout for generation jobs)
- [x] Config switching (Stub vs HiggsFieldClaude) via `VideoGeneration:Provider`
- [x] All new controller endpoints defined with auth, routes, and request shapes
- [x] `VideoGenerationStore` shape and polling pattern defined
- [x] All new Angular components and their responsibilities defined
- [x] `channel-form.component` modification scoped
- [x] `post-record-detail.component` modification scoped
- [x] Naming conventions followed consistently (backend + frontend)
- [x] All new mandatory rules and anti-patterns documented
- [x] All 23 FRs mapped to architecture components
- [x] All 7 NFRs mapped to architecture mechanisms

### Architecture Readiness Assessment

**Status: READY FOR IMPLEMENTATION**

**Confidence Level: High**

All 23 functional requirements and 7 non-functional requirements are covered. No open decisions remain. The design is additive — no existing contracts, collections, or components are broken. The stub adapter path continues to work unchanged in development.

### Implementation Handoff — Priority Order

| Priority | Component | Reason |
|----------|-----------|--------|
| 1 | `VideoGenerationSettings` model + config wiring | Required before any adapter or orchestration code compiles |
| 2 | `VideoGenerationResult` + `VideoGenerationRequest` extensions | Required before real adapter can be written |
| 3 | `VideoGenerationJob` domain model + repository | Required for async tracking |
| 4 | `PromptTemplateRenderer` static class | Required by orchestration service |
| 5 | `HiggsFieldClaudeAdapter` | Core feature — depends on 1, 2 |
| 6 | `VideoGenerationOrchestrationService` | Depends on 3, 4, 5 |
| 7 | `StartVideoGenerationCommand` + handler + validator | Depends on 3, 6 |
| 8 | `GetVideoGenerationJobQuery` + handler | Depends on 3 |
| 9 | `UpdatePromptTemplateCommand` + handler | Depends on existing Channel aggregate |
| 10 | `Channel.PromptTemplate` field + `StartPostCycleCommand` extension | Depends on 9; parallel with 7 |
| 11 | `VideoGenerationController` | Depends on 7, 8 |
| 12 | `TempFileCleanupService` extension | Depends on 3 |
| 13 | `PostRecord.GenerationMetadata` + handler wiring | Depends on 10 |
| 14 | Angular `VideoGenerationApiService` + `VideoGenerationStore` | Can build alongside backend; mock responses in dev |
| 15 | Angular `ImageUploadComponent` + `PromptEditorComponent` | Depends on 14 |
| 16 | Angular `GenerationStatusComponent` | Depends on 14, 15 |
| 17 | `VideoSourceSelectorComponent` + `PostCycleComponent` wiring | Depends on 15, 16 |
| 18 | `channel-form.component` prompt template editor | Depends on existing component; parallel with 14 |
| 19 | `post-record-detail.component` generation metadata display | Last — depends on full flow being testable |
