# Architecture Decision Document — Self-Hosted AI Video Generation Pipeline

**Author:** Kampe
**Date:** 2026-09-06
**Parent Architecture:** architecture-social-media-ai.md
**Feature PRD:** prd-selfhosted-video-generation.md

---

## Project Context

This document covers architectural decisions for the self-hosted video generation pipeline. The pipeline adds:

1. **Cloudflare R2** asset storage to `SocialMediaAPI`
2. A redesigned **`VideoGenerationJob`** MongoDB document that uses R2 keys instead of local temp paths
3. A new **`.NET Worker Service`** (`VideoWorker`) that runs on a GPU VM and executes generation jobs
4. A **`IVideoGenerator`** abstraction in the worker that hides WanGP/LTX implementation details
5. An initial **`WanGpVideoGenerator`** implementation (after a required technical spike)

### What Changes in SocialMediaAPI

| Change | Type |
|--------|------|
| `VideoGenerationJob` domain model | Redesign — new schema, new statuses |
| `IVideoGenerationJobRepository` | Update — add atomic claim method |
| `IR2StorageService` + `R2StorageService` | New — R2 upload/download/presigned URL |
| Job creation and status query endpoints | Update — new schema |
| Remove `HiggsFieldClaudeAdapter` dependency from GPU rendering path | Removal (HiggsField still used for existing Higgsfield-Claude flow; new self-hosted path is separate) |

### What is Net-New

| Artifact | Location |
|----------|----------|
| `VideoWorker` project | `services/VideoWorker/` |
| `IVideoGenerator` abstraction | `VideoWorker` |
| `StubVideoGenerator` | `VideoWorker` |
| `WanGpVideoGenerator` (post-spike) | `VideoWorker` |
| Worker Dockerfile | `services/VideoWorker/Dockerfile` |
| Generation container (WanGP/Python) | `services/generation-container/` |

---

## Architecture Principles (Recap from PRD)

1. **MongoDB = structured metadata only.** No binary data. Job state, R2 keys, parameters, timestamps.
2. **R2 = binary assets only.** Reference images, keyframes, output MP4s. No job metadata.
3. **GPU VM local disk = transient + persistent.** Temp files for active jobs (cleaned up after). Model/checkpoint files (persistent via Docker volume).
4. **The job is the contract.** The worker reads one MongoDB document and has everything it needs to execute. It does not call back to SocialMediaAPI or query R2 for discovery.
5. **IVideoGenerator hides the model.** WanGP/LTX details are confined to a single implementation class.
6. **MongoDB polling at MVP.** No message broker. Architecture leaves room to replace polling with a queue transport without redesigning the job document.

---

## Decision 1 — VideoGenerationJob Document Redesign

### Problem

The existing `VideoGenerationJob` is designed around the Higgsfield flow: it stores temp file paths on the application server (`imageTempPath`, `videoTempPath`), has Higgsfield-specific status values (`Queued`, `Generating`, `Ready`, `Consumed`), and carries a `higgsFieldModel` field. This schema does not support the self-hosted pipeline where assets live in R2 and a separate worker process on a remote VM performs generation.

### Decision

Redesign the `VideoGenerationJob` document. The new schema is a clean break from the Higgsfield design. The migration strategy is to treat jobs from the old schema as a separate concern — the new worker only processes jobs it creates; stale Higgsfield-pattern jobs can be expired by the existing `TempFileCleanupService`.

**New Document Schema:**

```
VideoGenerationJob {
  _id                : ObjectId (string in C#)
  channelId          : string              — owning channel (for context)
  status             : string              — "Pending" | "Processing" | "Completed" | "Failed"
  prompt             : string              — full text prompt for the generator
  model              : string              — model identifier e.g. "ltx-2.3", "wan"
  durationSeconds    : int                 — target video duration (5–30s)
  resolution         : string              — e.g. "1280x720"
  aspectRatio        : string              — e.g. "16:9", "9:16"
  referenceImageKeys : List<string>        — R2 object keys for reference images
  keyframeKeys       : List<string>        — R2 object keys for keyframes (may be empty)
  outputVideoKey     : string              — R2 object key where worker will write result MP4
  modelOptions       : Dictionary<string,string> — model-specific options (seed, steps, guidance, etc.)
  workerId           : string?             — set when status = Processing
  createdAt          : DateTime
  startedAt          : DateTime?           — set when status = Processing
  completedAt        : DateTime?           — set when status = Completed or Failed
  retryCount         : int                 — incremented on failure
  errorMessage       : string?             — last error (if Failed)
}
```

**Status Lifecycle:**

```
Pending ──(worker claims)──► Processing ──(success)──► Completed
                                        ──(error)───► Failed
                 ▲                                        │
                 └────────── (re-queue after timeout) ────┘
```

**MongoDB Index:**

```
video_generation_jobs: index on (status, createdAt ASC)
```

This allows the worker's polling query (`status = "Pending" order by createdAt ASC limit 1`) to run efficiently.

**Rationale:** The old schema is tightly coupled to the Higgsfield path. A clean redesign is preferable to patching the existing model. The new schema is complete enough for the worker to operate without additional queries.

---

## Decision 2 — Atomic Job Claiming via MongoDB findOneAndUpdate

### Problem

Multiple workers (even if there is only one now) must not claim the same job. Standard read-then-write patterns create a race condition.

### Decision

The worker claims a job with a single `FindOneAndUpdate` operation:

```csharp
// MongoJobQueue.cs in VideoWorker
public async Task<VideoGenerationJob?> TryClaimNextJobAsync(
    string workerId,
    CancellationToken cancellationToken)
{
    var filter = Builders<VideoGenerationJob>.Filter.Eq(j => j.Status, "Pending");
    var sort   = Builders<VideoGenerationJob>.Sort.Ascending(j => j.CreatedAt);
    var update = Builders<VideoGenerationJob>.Update
        .Set(j => j.Status,    "Processing")
        .Set(j => j.WorkerId,  workerId)
        .Set(j => j.StartedAt, DateTime.UtcNow);

    var options = new FindOneAndUpdateOptions<VideoGenerationJob>
    {
        Sort           = sort,
        ReturnDocument = ReturnDocument.After,
        IsUpsert       = false
    };

    return await _collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
}
```

This is a single atomic operation on MongoDB. If null is returned, no job was available.

**Stale job recovery (Phase 5):** A future background check queries for `status = "Processing"` where `startedAt < (now - configurable timeout)` and resets them to `Pending`, incrementing `retryCount`. This is built on top of the same collection without changing the claiming logic.

**Rationale:** `findOneAndUpdate` is the standard MongoDB pattern for building a queue. It requires no transactions, is atomic at the document level, and is already supported by the `MongoDB.Driver` already in use.

---

## Decision 3 — Cloudflare R2 via AWS S3-Compatible Client

### Problem

R2 needs to be integrated into both SocialMediaAPI (for uploading reference images) and the VideoWorker (for downloading assets and uploading the result video). No object storage client currently exists in the project.

### Decision

Use `AWSSDK.S3` (`Amazon.S3`) NuGet package, configured with:

- `ServiceURL` set to the R2 endpoint: `https://<accountId>.r2.cloudflarestorage.com`
- `ForcePathStyle = true` (required for R2 compatibility)
- Standard AWS access key / secret key credentials (R2 API tokens)
- `AuthenticationRegion = "auto"` (R2 does not require a region)

**Interface (shared contract used by both SocialMediaAPI and VideoWorker):**

```csharp
public interface IR2StorageService
{
    Task UploadAsync(string objectKey, Stream content, string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(string objectKey,
        CancellationToken cancellationToken = default);

    Task<string> GetPresignedUrlAsync(string objectKey, TimeSpan expiry,
        CancellationToken cancellationToken = default);
}
```

**R2 Object Key Conventions:**

```
generation/{jobId}/ref-0.png          ← reference image 0 (uploaded by backend)
generation/{jobId}/ref-1.png          ← reference image 1
generation/{jobId}/frame-0.png        ← keyframe 0 (future)
generation/{jobId}/output.mp4         ← generated video (uploaded by worker)
```

The backend computes the expected output key before creating the job (e.g. `generation/{jobId}/output.mp4`) and writes it into `outputVideoKey` on the job document. The worker uses this key as its upload target — no further coordination required.

**Configuration (both projects):**

```json
{
  "CloudflareR2": {
    "AccountId": "",
    "AccessKey": "",
    "SecretKey": "",
    "BucketName": "sports-media-assets"
  }
}
```

**Rationale:** R2's S3 compatibility means we use a well-supported, battle-tested client (`AWSSDK.S3`) rather than a Cloudflare-specific SDK. The `IR2StorageService` interface hides the AWS SDK from the rest of the application, maintaining the existing project's pattern of interface-backed infrastructure.

---

## Decision 4 — .NET Worker Service Structure

### Problem

The rendering workload must run independently of the ASP.NET request lifecycle, on a separate VM, and must be deployable as a Docker container.

### Decision

Create a new `.NET 8 Worker Service` project at `services/VideoWorker/`. This is a standard `IHostedService`-based background application — not an ASP.NET Web API.

**Project layout:**

```
services/VideoWorker/
├── VideoWorker.csproj
├── Program.cs                          — Host setup, DI, configuration
├── Worker.cs                           — IHostedService: polling loop
├── JobProcessor.cs                     — orchestrates one job end-to-end
│
├── Abstractions/
│   ├── IVideoGenerator.cs              — core interface
│   ├── VideoGenerationRequest.cs       — generator input model
│   └── VideoGenerationResult.cs        — generator output model
│
├── Generators/
│   ├── StubVideoGenerator.cs           — Phase 1: copies sample MP4
│   └── WanGpVideoGenerator.cs          — Phase 2+: real LTX/WanGP integration
│
├── Infrastructure/
│   ├── MongoJobQueue.cs                — TryClaimNextJobAsync, UpdateJobAsync
│   ├── R2AssetService.cs               — DownloadToLocalAsync, UploadFromLocalAsync
│   └── TempFileManager.cs              — creates/cleans temp directories per job
│
├── appsettings.json
├── appsettings.Development.json
└── Dockerfile
```

**Worker.cs — polling loop:**

```csharp
public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await _jobQueue.TryClaimNextJobAsync(_workerId, stoppingToken);

            if (job == null)
            {
                await Task.Delay(_pollInterval, stoppingToken);
                continue;
            }

            await _jobProcessor.ProcessAsync(job, stoppingToken);
        }
    }
}
```

**JobProcessor.cs — job execution lifecycle:**

```
ProcessAsync(job, cancellationToken)

1.  Log: job claimed
2.  tempDir = TempFileManager.CreateJobDirectory(job.Id)
3.  try
4.    refImagePaths = await R2AssetService.DownloadJobAssetsAsync(job, tempDir)
5.    Log: assets downloaded
6.    request = BuildGenerationRequest(job, refImagePaths, tempDir)
7.    Log: generation starting
8.    result = await IVideoGenerator.GenerateAsync(request, cancellationToken)
9.    if result.Success:
10.     Log: generation complete
11.     await R2AssetService.UploadVideoAsync(result.VideoPath, job.OutputVideoKey)
12.     Log: video uploaded
13.     await MongoJobQueue.MarkCompletedAsync(job.Id, job.OutputVideoKey)
14.     Log: job completed
15.   else:
16.     Log: generation failed
17.     await MongoJobQueue.MarkFailedAsync(job.Id, result.ErrorMessage)
18. catch Exception ex:
19.   Log: unexpected error
20.   await MongoJobQueue.MarkFailedAsync(job.Id, ex.Message)
21. finally:
22.   TempFileManager.Cleanup(tempDir)
23.   Log: cleanup complete
```

**Rationale:** The `JobProcessor` owns the complete lifecycle of one job. It calls infrastructure services and the generator through interfaces. This makes it testable and replaceable. The `Worker` is a simple polling loop that delegates to `JobProcessor` — it does not understand job semantics.

---

## Decision 5 — IVideoGenerator Abstraction

### Problem

The worker must not be coupled to WanGP/LTX internals. The interface must support replacing LTX 2.3 with Wan, HunyuanVideo, or future models, or even an external API, without redesigning the worker.

### Decision

```csharp
public interface IVideoGenerator
{
    Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);
}

public class VideoGenerationRequest
{
    public string Prompt                   { get; set; } = string.Empty;
    public string Model                    { get; set; } = string.Empty;
    public List<string> ReferenceImagePaths { get; set; } = new();
    public List<string> KeyframePaths      { get; set; } = new();
    public int DurationSeconds             { get; set; }
    public string Resolution               { get; set; } = string.Empty;
    public string AspectRatio              { get; set; } = string.Empty;
    public string OutputPath               { get; set; } = string.Empty; // where to write the MP4
    public Dictionary<string, string> ModelOptions { get; set; } = new();
}

public class VideoGenerationResult
{
    public bool Success        { get; set; }
    public string? VideoPath   { get; set; } // equals request.OutputPath on success
    public string? ErrorMessage { get; set; }
}
```

**Generator registration (DI in Program.cs):**

```csharp
var generatorType = builder.Configuration["VideoGenerator:Type"];
if (generatorType == "WanGP")
    builder.Services.AddSingleton<IVideoGenerator, WanGpVideoGenerator>();
else
    builder.Services.AddSingleton<IVideoGenerator, StubVideoGenerator>();
```

**StubVideoGenerator (Phase 1):**

Copies a bundled sample MP4 from a configured path to `request.OutputPath`. Returns `Success = true`. Simulates an optional configurable delay. Allows full pipeline validation without GPU hardware.

**WanGpVideoGenerator (Phase 2+):**

Implementation determined by technical spike (see Decision 6). May call WanGP via CLI subprocess, Gradio HTTP API, or a thin Python wrapper service. All model-specific concerns are contained in this class.

**Rationale:** The interface is intentionally model-agnostic. `ModelOptions` is a string dictionary to allow passing model-specific settings (seed, steps, guidance scale, motion strength) without polluting the interface with model-specific properties. The `OutputPath` is part of the request so the generator writes to the pre-determined path — the `JobProcessor` then knows exactly where to find the output.

---

## Decision 6 — WanGP/LTX Integration Mechanism (Spike Required)

### Problem

WanGP is a Python-based video generation framework. The .NET worker cannot import Python libraries directly. There are multiple possible integration mechanisms, each with different trade-offs.

### Decision

**This is a known uncertainty. A technical spike (Story 8.1) is required before implementing `WanGpVideoGenerator`.**

The spike must evaluate:

| Option | Description | Pro | Con |
|--------|-------------|-----|-----|
| A — CLI subprocess | Worker shells out to `python generate.py --prompt "..." --output /tmp/out.mp4` | Simple, no inter-process HTTP | Output parsing fragile, harder to handle async progress |
| B — Gradio HTTP API | WanGP exposes a Gradio interface; worker calls `http://localhost:7860/api/...` | Clean boundary, queryable | Gradio API surface may change; headless mode support uncertain |
| C — Thin Python HTTP wrapper | Write a minimal FastAPI/Flask service that wraps WanGP and exposes a clean POST endpoint | Full control over API contract | Additional Python service to maintain |

**Expected spike output:** A working proof-of-concept that generates a short video from a reference image and prompt using one of the above mechanisms, with a recommendation for the production approach.

**Architecture impact of each option:**

- **Option A**: Worker and WanGP in the same container (or same host). `WanGpVideoGenerator` uses `System.Diagnostics.Process` to invoke the Python script. No inter-container networking.
- **Option B**: WanGP container exposes port 7860 locally; worker container calls it via `http://localhost:7860` (same network namespace) or internal Docker network. Worker uses `HttpClient`.
- **Option C**: Python wrapper service in its own container; worker calls it via `HttpClient`. Clean Docker separation.

**Guidance for the spike:** Prefer the option that produces the cleanest boundary with the least fragility in the response parsing. Option C is the preferred architectural direction if the spike confirms WanGP is stable enough as a subprocess; Option A is acceptable if WanGP supports reliable headless CLI invocation.

---

## Decision 7 — Docker Deployment Architecture

### Problem

The GPU VM must run both the .NET worker and the WanGP/Python runtime. These have very different dependencies (.NET 8 vs PyTorch/CUDA) and should be separated in Docker.

### Decision

**Two-container architecture on the GPU VM:**

```
GPU VM (RunPod)
├── Worker Container
│   ├── Base: mcr.microsoft.com/dotnet/runtime:8.0
│   ├── Contents: VideoWorker .NET application
│   └── Connects to: MongoDB (external), R2 (external), Generation Container (local)
│
├── Generation Container
│   ├── Base: nvidia/cuda:12.x-cudnn-devel-ubuntu22.04 (or pytorch official image)
│   ├── Contents: Python, PyTorch, WanGP, LTX 2.3 model
│   └── Exposes: Local HTTP API or shared volume (determined by spike)
│
└── Persistent Volumes
    ├── /models           ← LTX 2.3 checkpoint files (do not download on every start)
    └── /generation-work  ← Optional shared working directory if Option A/C

docker-compose.yml (GPU VM)

services:
  worker:
    image: sports-video-worker:latest
    environment:
      - MongoDB__ConnectionString=...
      - CloudflareR2__AccountId=...
      - VideoGenerator__Type=WanGP
      - VideoGenerator__WanGp__ApiUrl=http://generation:8080
    depends_on:
      - generation

  generation:
    image: sports-wangp-generation:latest
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]
    volumes:
      - models:/models
      - generation-work:/tmp/generation

volumes:
  models:
  generation-work:
```

**Worker Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish services/VideoWorker/VideoWorker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VideoWorker.dll"]
```

**Generation Container Dockerfile:**

Determined by spike outcome. Starts from a CUDA + PyTorch base image, installs WanGP and its dependencies, downloads/mounts LTX 2.3 checkpoint files from the persistent `/models` volume.

**Model persistence:** LTX 2.3 checkpoint files are downloaded to `/models` on first start and reused on subsequent starts via the named volume. The generation container checks for model existence at startup and downloads only if not present.

**Rationale:** Separating .NET and Python/CUDA concerns into two containers is simpler than trying to run both in one container. The trade-off is inter-container communication overhead, which is negligible for video generation workloads.

---

## Decision 8 — Stale Job Recovery (Phase 5 Mechanism, Designed Now)

### Problem

If the worker crashes while `Processing` a job, that job stays in `Processing` state indefinitely. A recovery mechanism is needed.

### Decision

**Phase 1-4:** Stale job recovery is operational awareness — manually re-queue by setting `status = "Pending"` in MongoDB if a job is stuck. The `startedAt` and `workerId` fields provide the necessary information.

**Phase 5:** Add a background check to `Worker.cs`:

```csharp
// Every N minutes, check for stale Processing jobs
var staleThreshold = DateTime.UtcNow - _staleJobTimeout; // configurable, e.g. 30 min
var staleJobs = await _jobQueue.GetStaleProcessingJobsAsync(staleThreshold);
foreach (var job in staleJobs)
{
    if (job.RetryCount < _maxRetries)
        await _jobQueue.RequeueAsync(job.Id);
    else
        await _jobQueue.MarkFailedAsync(job.Id, "Max retries exceeded after worker crash");
}
```

The `RequeueAsync` operation: sets `status = "Pending"`, clears `workerId`, clears `startedAt`, increments `retryCount`.

**Retry limit:** Configurable via `VideoWorker:MaxRetries` (default: 3). Jobs that exceed the limit are marked `Failed` permanently.

**Rationale:** The `retryCount` and `startedAt` fields in the job document make this implementable as a pure MongoDB operation without a separate scheduler or heartbeat mechanism. At 10 jobs/day, manual recovery is acceptable at MVP; automated recovery is designed but deferred to Phase 5.

---

## API Additions to SocialMediaAPI

### New Endpoints

| Method | Route | Auth | Command/Query | Description |
|--------|-------|------|---------------|-------------|
| `POST` | `/api/video-generation/jobs` | `[Authorize]` | `CreateVideoGenerationJobCommand` | Create a Pending job |
| `GET` | `/api/video-generation/jobs/{jobId}` | `[Authorize]` | `GetVideoGenerationJobQuery` | Query job status and result |
| `POST` | `/api/video-generation/assets` | `[Authorize]` | `UploadGenerationAssetCommand` | Upload reference image to R2, return key |

### CreateVideoGenerationJobCommand — Request Body

```json
{
  "channelId": "...",
  "prompt": "A cinematic shot of a sports athlete warming up at dawn",
  "model": "ltx-2.3",
  "durationSeconds": 10,
  "resolution": "1280x720",
  "aspectRatio": "16:9",
  "referenceImageKeys": ["generation/temp/ref-abc123.png"],
  "keyframeKeys": [],
  "modelOptions": {
    "seed": "42",
    "steps": "50"
  }
}
```

The backend computes `outputVideoKey = generation/{jobId}/output.mp4` before persisting.

### Response — Job Status

```json
{
  "data": {
    "id": "66f1a234b5c6d7e8f9012345",
    "channelId": "...",
    "status": "Completed",
    "prompt": "...",
    "model": "ltx-2.3",
    "outputVideoKey": "generation/66f1a234b5c6d7e8f9012345/output.mp4",
    "createdAt": "2026-09-06T10:00:00Z",
    "startedAt": "2026-09-06T10:00:05Z",
    "completedAt": "2026-09-06T10:04:32Z",
    "retryCount": 0,
    "errorMessage": null
  },
  "success": true
}
```

---

## Data Flow — End to End

```
[Backend / Caller]                [MongoDB]            [R2]              [GPU VM / Worker]
       │                               │                 │                       │
       │── POST /api/video-generation/assets ──────────► R2.Upload(ref.png)     │
       │◄── { objectKey: "generation/tmp/ref.png" } ────┘                       │
       │                               │                                         │
       │── POST /api/video-generation/jobs ──────────►                           │
       │   { prompt, model, referenceImageKeys, ... }   │                       │
       │                   InsertJob(Pending) ──────────►                        │
       │◄── { jobId }                  │                                         │
       │                               │                                         │
       │                               │         [Worker polls every 5s]         │
       │                               │◄── FindOneAndUpdate(Pending→Processing) ┤
       │                               │                                         │
       │                               │                  ◄── R2.Download(ref.png) → /tmp/{jobId}/ref-0.png
       │                               │                                         │
       │                               │                  ◄── IVideoGenerator.GenerateAsync(...)
       │                               │                       [WanGP/LTX running on GPU]
       │                               │                       → /tmp/{jobId}/output.mp4
       │                               │                                         │
       │                               │                  ◄── R2.Upload(/tmp/{jobId}/output.mp4 → generation/{jobId}/output.mp4)
       │                               │                                         │
       │                    UpdateJob(Completed) ◄────────────────────────────── │
       │                               │                  ◄── Cleanup /tmp/{jobId}/
       │                               │                                         │
       │── GET /api/video-generation/jobs/{jobId} ──────────►                    │
       │◄── { status: "Completed", outputVideoKey: "generation/.../output.mp4" } │
```

---

## Project Structure — Net-New Files

### SocialMediaAPI Changes

```
services/SocialMediaAPI/
│
├── Domain/
│   └── VideoGenerationJob/
│       └── VideoGenerationJob.cs          ★ REDESIGN — new schema (see Decision 1)
│
├── Application/
│   ├── Common/Interfaces/
│   │   ├── IVideoGenerationJobRepository.cs  ★ UPDATE — add ClaimNextAsync, MarkCompletedAsync, MarkFailedAsync
│   │   └── IR2StorageService.cs           NEW — Upload, Download, GetPresignedUrl
│   └── VideoGeneration/
│       ├── Commands/
│       │   ├── CreateVideoGenerationJobCommand.cs  NEW (replaces StartVideoGenerationCommand for self-hosted path)
│       │   └── UploadGenerationAssetCommand.cs     NEW
│       └── Queries/
│           └── GetVideoGenerationJobQuery.cs  ★ UPDATE — new response shape
│
├── Infrastructure/
│   ├── Repositories/
│   │   └── VideoGenerationJobRepository.cs  ★ UPDATE — implement new interface methods
│   ├── Storage/
│   │   └── CloudflareR2StorageService.cs    NEW — implements IR2StorageService
│   └── DependencyInjection.cs               ★ MODIFY — register IR2StorageService, new commands
│
└── WebAPI/
    ├── Controllers/
    │   └── VideoGenerationController.cs     ★ UPDATE — new endpoints
    └── appsettings.json                     ★ MODIFY — add CloudflareR2 section
```

### New Project

```
services/VideoWorker/
│
├── VideoWorker.csproj
├── Program.cs                           — Host, DI, configuration
├── Worker.cs                            — IHostedService polling loop
├── JobProcessor.cs                      — job execution lifecycle
│
├── Abstractions/
│   ├── IVideoGenerator.cs
│   ├── VideoGenerationRequest.cs
│   └── VideoGenerationResult.cs
│
├── Generators/
│   ├── StubVideoGenerator.cs
│   └── WanGpVideoGenerator.cs           (post-spike)
│
├── Infrastructure/
│   ├── MongoJobQueue.cs                 — TryClaimNextJobAsync, MarkCompletedAsync, MarkFailedAsync
│   ├── R2AssetService.cs                — DownloadToLocalAsync, UploadFromLocalAsync
│   └── TempFileManager.cs              — CreateJobDirectory, Cleanup
│
├── appsettings.json
├── appsettings.Development.json
└── Dockerfile
```

### Generation Container (post-spike)

```
services/generation-container/
├── Dockerfile
├── requirements.txt
├── server.py                            — HTTP wrapper (if Option C)
└── generate.py                          — CLI entry point (if Option A)
```

---

## Configuration Reference

### SocialMediaAPI — appsettings additions

```json
{
  "CloudflareR2": {
    "AccountId": "",
    "AccessKey": "",
    "SecretKey": "",
    "BucketName": "sports-media-assets"
  }
}
```

### VideoWorker — appsettings.json

```json
{
  "MongoDB": {
    "ConnectionString": "",
    "DatabaseName": "SocialMediaDb"
  },
  "CloudflareR2": {
    "AccountId": "",
    "AccessKey": "",
    "SecretKey": "",
    "BucketName": "sports-media-assets"
  },
  "VideoGenerator": {
    "Type": "Stub",
    "Stub": {
      "SampleVideoPath": "/app/samples/sample.mp4",
      "SimulatedDelaySeconds": 5
    },
    "WanGp": {
      "ApiUrl": "http://generation:8080",
      "TimeoutSeconds": 1800
    }
  },
  "Worker": {
    "Id": "worker-01",
    "PollIntervalSeconds": 5,
    "StaleJobTimeoutMinutes": 30,
    "MaxRetries": 3
  }
}
```

---

## Naming Conventions

Follows existing SocialMediaAPI conventions plus the following additions:

| Artifact | Name | Location |
|----------|------|----------|
| Storage interface | `IR2StorageService` | Application/Common/Interfaces |
| Storage implementation | `CloudflareR2StorageService` | Infrastructure/Storage |
| Job create command | `CreateVideoGenerationJobCommand` | Application/VideoGeneration/Commands |
| Asset upload command | `UploadGenerationAssetCommand` | Application/VideoGeneration/Commands |
| Worker hosted service | `Worker` | VideoWorker |
| Job processor | `JobProcessor` | VideoWorker |
| Job queue (worker) | `MongoJobQueue` | VideoWorker/Infrastructure |
| Asset service (worker) | `R2AssetService` | VideoWorker/Infrastructure |
| Generator interface | `IVideoGenerator` | VideoWorker/Abstractions |
| Stub generator | `StubVideoGenerator` | VideoWorker/Generators |
| WanGP generator | `WanGpVideoGenerator` | VideoWorker/Generators |

---

## Mandatory Rules

- Never store binary image or video data in MongoDB — R2 object keys only
- Never store R2 credentials in source code or committed config files — environment variables or secrets only
- Job claiming must always use `findOneAndUpdate` — never read-then-write
- `JobProcessor` must always call `TempFileManager.Cleanup()` in a `finally` block
- `IVideoGenerator.GenerateAsync` must never throw — return `VideoGenerationResult { Success = false, ErrorMessage = ... }` on all errors; let `JobProcessor` handle the failure
- The worker must log the job ID in every log entry for a given job (use a log scope)
- `outputVideoKey` must be determined by the backend before job creation, not by the worker at upload time
- The worker must not query SocialMediaAPI — it only reads MongoDB and R2

## Anti-Patterns to Avoid

- Storing temp file paths in MongoDB (the Higgsfield pattern) — all storage references are R2 keys
- Making the worker aware of channels, users, or creative context beyond what is in the job document
- Calling SocialMediaAPI from the worker — the worker is a peer, not a dependent
- Running model checkpoints from ephemeral container storage — always use a persistent volume
- Downloading model/checkpoint files on every container start — check for existence first
- Using `Task.Run` fire-and-forget in the worker — generation is already background; do not add another async layer

---

## Architecture Validation

### Functional Requirements Coverage

| FR | Requirement | Architecture Coverage |
|----|------------|----------------------|
| FR-SHV-01 | Backend creates Pending job | `CreateVideoGenerationJobCommand` → `IVideoGenerationJobRepository.AddAsync` |
| FR-SHV-02 | Job includes prompt, model, R2 keys, settings | New `VideoGenerationJob` document schema |
| FR-SHV-03 | Backend queries job by ID | `GetVideoGenerationJobQuery` |
| FR-SHV-04 | Backend uploads reference image to R2 | `UploadGenerationAssetCommand` → `IR2StorageService.UploadAsync` |
| FR-SHV-05 | No binary data in MongoDB | Architecture rule; schema uses string keys only |
| FR-SHV-06 | Worker atomically claims job | `MongoJobQueue.TryClaimNextJobAsync` (findOneAndUpdate) |
| FR-SHV-07 | No two workers process same job | Atomic findOneAndUpdate prevents double-claim |
| FR-SHV-08 | Worker reads complete job payload | `JobProcessor` reads full document after claim |
| FR-SHV-09 | Worker downloads R2 assets to local disk | `R2AssetService.DownloadToLocalAsync` in `JobProcessor` |
| FR-SHV-10 | Worker passes inputs to IVideoGenerator | `JobProcessor.BuildGenerationRequest(job, localPaths)` |
| FR-SHV-11 | Worker uploads MP4 to R2 | `R2AssetService.UploadFromLocalAsync` |
| FR-SHV-12 | Worker marks job Completed | `MongoJobQueue.MarkCompletedAsync` |
| FR-SHV-13 | Worker marks job Failed | `MongoJobQueue.MarkFailedAsync` in catch + else |
| FR-SHV-14 | Worker cleans up temp files | `TempFileManager.Cleanup` in finally |
| FR-SHV-15 | Worker polls at configurable interval | `Worker.cs` with `_pollInterval` from config |
| FR-SHV-16 | IVideoGenerator abstraction | `IVideoGenerator` interface (Decision 5) |
| FR-SHV-17 | StubVideoGenerator | `StubVideoGenerator` in Generators/ |
| FR-SHV-18 | Worker logs lifecycle events | Structured logging with job ID scope throughout `JobProcessor` |
| FR-SHV-19 | Worker and backend share collection | Both use `video_generation_jobs` collection; worker has its own connection |

### Non-Functional Requirements Coverage

| NFR | Requirement | Coverage |
|-----|------------|----------|
| NFR-SHV-01 | Atomic job claiming | `findOneAndUpdate` — Decision 2 |
| NFR-SHV-02 | R2 credentials from config | `CloudflareR2` config section; never hardcoded |
| NFR-SHV-03 | MongoDB connection from config | Standard .NET configuration |
| NFR-SHV-04 | Temp files cleaned up | `finally` block in `JobProcessor` |
| NFR-SHV-05 | Model files on persistent volume | Named Docker volume `/models` |
| NFR-SHV-06 | Worker deployable as Docker container | `VideoWorker/Dockerfile` |
| NFR-SHV-07 | Worker restartable safely | Pending jobs survive in MongoDB; only in-flight job at risk |
| NFR-SHV-08 | Stale job recovery | Designed in Decision 8; operational in Phase 1-4, automated in Phase 5 |
| NFR-SHV-09 | IVideoGenerator hides model details | Interface uses only domain types; no CUDA/PyTorch references |
| NFR-SHV-10 | ~10 videos/day single worker | No horizontal scaling required; single worker architecture |
| NFR-SHV-11 | Replaceable queue transport | Job document contains all data needed by any transport |

### Open Items (Require Spike — Story 8.1)

| Item | Decision |
|------|----------|
| WanGP headless invocation mechanism | CLI subprocess vs Gradio API vs Python wrapper |
| Worker ↔ Generation container communication | Shared volume vs HTTP vs subprocess |
| Generation container base image | CUDA version, PyTorch compatibility with LTX 2.3 |
| LTX 2.3 VRAM requirements on RTX 4090 | Performance validation in Phase 4 |

**Status: READY FOR IMPLEMENTATION — Phase 1 stories require no spike. Phase 2+ stories blocked on Story 8.1 (WanGP spike).**
