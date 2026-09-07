# Self-Hosted AI Video Generation — Epics and Stories

**Author:** Kampe
**Date:** 2026-09-06
**Input Documents:**
- `_bmad-output/planning-artifacts/prd-selfhosted-video-generation.md`
- `_bmad-output/planning-artifacts/architecture-selfhosted-video-generation.md`

---

## Overview

Stories are ordered so the system can be built and validated incrementally. Epics 1–4 can be completed without GPU hardware. Epics 5–7 require a real GPU (or local GPU for model validation). Epic 8 focuses on hardening after end-to-end validation.

**Phased delivery:**

| Phase | Epics | Goal |
|-------|-------|------|
| 1 — Worker Infrastructure | 1, 2, 3, 4, 5 | Full pipeline validated with stub generator; no GPU required |
| 2 — Local Model Integration | 6 | WanGP/LTX integration validated locally |
| 3 — Docker | 7 | Containerized worker validated locally |
| 4 — GPU VM | 8 | End-to-end on RunPod GPU |
| 5 — Hardening | 9 | Stale job recovery, retry policy, observability |

---

## Epic 1 — R2 Asset Storage Integration (SocialMediaAPI)

Add Cloudflare R2 object storage to SocialMediaAPI. The backend must be able to upload reference images to R2 and store object keys on generation jobs.

---

### Story 1.1 — Add R2 Storage Client Configuration

**As the** SocialMediaAPI,
**I want** an `IR2StorageService` interface and `CloudflareR2StorageService` implementation registered via DI,
**so that** any command handler can upload or download binary assets to Cloudflare R2 using a clean interface.

**Technical Context:**
- Add `AWSSDK.S3` NuGet package to `SocialMediaAPI/Infrastructure`
- R2 is S3-compatible: configure `ServiceURL = https://{AccountId}.r2.cloudflarestorage.com`, `ForcePathStyle = true`, `AuthenticationRegion = "auto"`
- Add `IR2StorageService` interface to `Application/Common/Interfaces/`
- Add `CloudflareR2StorageService` implementation to `Infrastructure/Storage/`
- Register in `Infrastructure/DependencyInjection.cs`
- Add `CloudflareR2` section to `appsettings.json` (empty values, documented)
- Add `CloudflareR2` section to `appsettings.Development.json` (pointed at a local MinIO or test R2 bucket)

**Interface Contract:**
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

**Acceptance Criteria:**
- [ ] `AWSSDK.S3` is referenced in `Infrastructure.csproj`
- [ ] `IR2StorageService` is defined in Application layer with the three methods above
- [ ] `CloudflareR2StorageService` implements `IR2StorageService` using the AWS S3 client pointed at the R2 endpoint
- [ ] R2 credentials are loaded from `IConfiguration` — never hardcoded
- [ ] `IR2StorageService` is registered as `Scoped` in DI
- [ ] `appsettings.json` contains an empty `CloudflareR2` section with documented placeholder values
- [ ] The service compiles and is injectable

---

### Story 1.2 — Upload Generation Asset Endpoint

**As** an API caller,
**I want** to upload a reference image to R2 and receive an object key in response,
**so that** I can include that key when creating a video generation job.

**Technical Context:**
- New command: `UploadGenerationAssetCommand` with `IFormFile Image` and `string JobContext` (optional label)
- Handler streams image to R2 under key: `generation/assets/{guid}.{ext}`
- Returns the object key
- New endpoint: `POST /api/video-generation/assets` (multipart form)
- Request size limit: 20MB (same as existing image upload pattern)
- Validate: file not null, content type in `[image/jpeg, image/png, image/webp]`, size ≤ 20MB
- Do not store the binary in MongoDB — only the returned key is persisted by the caller

**Acceptance Criteria:**
- [ ] `POST /api/video-generation/assets` accepts a multipart form with a single image file
- [ ] Handler validates content type and size; returns `400` with descriptive message on violation
- [ ] Image is streamed (not buffered in full) to R2
- [ ] Response body: `{ "data": { "objectKey": "generation/assets/{guid}.{ext}" }, "success": true }`
- [ ] `[Authorize]` attribute is present on the endpoint
- [ ] Image binary is not written to MongoDB

---

## Epic 2 — VideoGenerationJob Schema Redesign (SocialMediaAPI)

Redesign the `VideoGenerationJob` MongoDB document to support the self-hosted R2-based pipeline. The new schema replaces temp-file paths with R2 object keys and adds worker tracking, retry, and model parameter fields.

---

### Story 2.1 — Redesign VideoGenerationJob Domain Model

**As the** system,
**I want** a `VideoGenerationJob` document that stores R2 object keys, generation parameters, and worker assignment — never local file paths,
**so that** the worker service can process jobs using only the job document and R2.

**Technical Context:**
Redesign `Domain/VideoGenerationJob/VideoGenerationJob.cs` with the new schema (see architecture Decision 1):

```
status              : "Pending" | "Processing" | "Completed" | "Failed"
prompt              : string
model               : string (e.g. "ltx-2.3")
durationSeconds     : int
resolution          : string (e.g. "1280x720")
aspectRatio         : string (e.g. "16:9")
referenceImageKeys  : List<string>  — R2 keys
keyframeKeys        : List<string>  — R2 keys (may be empty)
outputVideoKey      : string        — R2 key where worker writes result
modelOptions        : Dictionary<string, string>
workerId            : string?
startedAt           : DateTime?
completedAt         : DateTime?
retryCount          : int
errorMessage        : string?
```

Remove: `imageTempPath`, `imageFileName`, `videoTempPath`, `higgsFieldModel`, `renderedPrompt` (Higgsfield-specific fields).

Note: `channelId` and `createdAt` are retained from the base `Entity<string>`.

**Acceptance Criteria:**
- [ ] `VideoGenerationJob.cs` contains the fields listed above with BSON attributes
- [ ] Removed fields (`imageTempPath`, `imageFileName`, `videoTempPath`, `higgsFieldModel`, `renderedPrompt`) are gone
- [ ] `referenceImageKeys` and `keyframeKeys` are `List<string>` (may be empty, not null)
- [ ] `modelOptions` is `Dictionary<string, string>` (may be empty, not null)
- [ ] Default status is `"Pending"`
- [ ] `retryCount` defaults to `0`
- [ ] Domain model compiles cleanly; no references to old temp-path fields remain

---

### Story 2.2 — Update IVideoGenerationJobRepository with Claim Operations

**As the** VideoWorker,
**I want** atomic claim, completion, and failure update methods on `IVideoGenerationJobRepository`,
**so that** the worker can transition job state in a single safe MongoDB operation.

**Technical Context:**
Update `Application/Common/Interfaces/IVideoGenerationJobRepository.cs`:

```csharp
public interface IVideoGenerationJobRepository : IRepository<VideoGenerationJob, string>
{
    // Atomically claims the oldest Pending job; returns null if none available
    Task<VideoGenerationJob?> TryClaimNextJobAsync(
        string workerId, CancellationToken cancellationToken = default);

    Task MarkCompletedAsync(
        string jobId, string outputVideoKey, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        string jobId, string errorMessage, CancellationToken cancellationToken = default);

    // For stale job recovery (Phase 5)
    Task<List<VideoGenerationJob>> GetStaleProcessingJobsAsync(
        DateTime olderThan, CancellationToken cancellationToken = default);

    Task RequeueAsync(
        string jobId, CancellationToken cancellationToken = default);
}
```

Update `Infrastructure/Repositories/VideoGenerationJobRepository.cs` to implement the new methods using `FindOneAndUpdateAsync` for claim (see architecture Decision 2). Add MongoDB index on `(status, createdAt)`.

**Acceptance Criteria:**
- [ ] `TryClaimNextJobAsync` uses a single `FindOneAndUpdateAsync`: filter `status = Pending`, sort by `createdAt ASC`, update sets `status = Processing`, `workerId`, `startedAt = UtcNow`; returns updated document or null
- [ ] `MarkCompletedAsync` sets `status = Completed`, `outputVideoKey`, `completedAt = UtcNow`
- [ ] `MarkFailedAsync` sets `status = Failed`, `errorMessage`, `completedAt = UtcNow`, increments `retryCount`
- [ ] `GetStaleProcessingJobsAsync` returns jobs where `status = Processing` and `startedAt < olderThan`
- [ ] `RequeueAsync` sets `status = Pending`, clears `workerId`, clears `startedAt`, increments `retryCount`
- [ ] MongoDB index on `(status, createdAt)` is created at startup (via `IMongoCollection.Indexes.CreateOne`)

---

### Story 2.3 — Create Video Generation Job Endpoint

**As** an API caller,
**I want** to create a video generation job via a POST endpoint,
**so that** the worker can discover and execute it.

**Technical Context:**
- New command: `CreateVideoGenerationJobCommand`
- Request body: `channelId`, `prompt`, `model`, `durationSeconds`, `resolution`, `aspectRatio`, `referenceImageKeys` (List<string>), `keyframeKeys` (List<string>?), `modelOptions` (Dictionary<string,string>?)
- Handler computes `outputVideoKey = generation/{jobId}/output.mp4` before persisting
- Handler creates job with `status = Pending`
- Returns job ID
- Validation: `prompt` not empty, `referenceImageKeys` not empty, `durationSeconds` in 1–120, `model` not empty
- New endpoint: `POST /api/video-generation/jobs`

**Acceptance Criteria:**
- [ ] `POST /api/video-generation/jobs` creates a `Pending` job in MongoDB
- [ ] `outputVideoKey` is set to `generation/{jobId}/output.mp4` before persisting (backend computes it, not the worker)
- [ ] `status` is always `Pending` on creation; caller cannot set status
- [ ] Handler validates required fields; returns `400` with descriptive message on failure
- [ ] Response: `{ "data": { "jobId": "..." }, "success": true }`
- [ ] `[Authorize]` is present

---

### Story 2.4 — Update Job Status Query Endpoint

**As** an API caller,
**I want** to query a video generation job by ID and receive its current status, output key, and error details,
**so that** I can poll progress and retrieve the result.

**Technical Context:**
Update `GetVideoGenerationJobQuery` and `VideoGenerationJobResponse` DTO to reflect the new schema. Response should include: `id`, `channelId`, `status`, `prompt`, `model`, `outputVideoKey`, `createdAt`, `startedAt`, `completedAt`, `retryCount`, `errorMessage`.

**Acceptance Criteria:**
- [ ] `GET /api/video-generation/jobs/{jobId}` returns the job document mapped to the updated DTO
- [ ] Response includes `outputVideoKey` (non-null when Completed)
- [ ] Response includes `errorMessage` (non-null when Failed)
- [ ] Returns `404` with descriptive message when job ID not found
- [ ] `[Authorize]` is present
- [ ] Old Higgsfield-specific fields (`higgsFieldModel`, `imageTempPath`, etc.) are absent from the response

---

## Epic 3 — VideoWorker Service Scaffolding

Scaffold the new `.NET 8 Worker Service` project, configure MongoDB and R2 connectivity, and add it to the solution.

---

### Story 3.1 — Scaffold VideoWorker Project

**As a** developer,
**I want** a new `.NET 8 Worker Service` project at `services/VideoWorker/`,
**so that** I have a deployable background application separate from the SocialMediaAPI web process.

**Technical Context:**
- Create `services/VideoWorker/VideoWorker.csproj` (Worker Service template: `Microsoft.NET.Sdk.Worker`)
- Add to `services/SportsSystem.sln`
- Project structure per architecture Decision 4:
  - `Worker.cs` — `BackgroundService` implementation (polling loop stub — just log and sleep initially)
  - `JobProcessor.cs` — stub (logs "processing job {id}")
  - `Abstractions/IVideoGenerator.cs`, `VideoGenerationRequest.cs`, `VideoGenerationResult.cs`
  - `Generators/StubVideoGenerator.cs`
  - `Infrastructure/MongoJobQueue.cs` (stub — implement in Story 4.1)
  - `Infrastructure/R2AssetService.cs` (stub — implement in Story 4.2)
  - `Infrastructure/TempFileManager.cs`
  - `appsettings.json`, `appsettings.Development.json`
- NuGet packages: `MongoDB.Driver`, `AWSSDK.S3`, `Microsoft.Extensions.Hosting`

**Acceptance Criteria:**
- [ ] `services/VideoWorker/VideoWorker.csproj` exists and uses the Worker Service SDK
- [ ] Project is added to `SportsSystem.sln`
- [ ] `dotnet build services/VideoWorker/` succeeds with no errors
- [ ] `dotnet run --project services/VideoWorker/` starts the host, logs "Worker started", and runs without crashing
- [ ] `IVideoGenerator`, `VideoGenerationRequest`, `VideoGenerationResult` interfaces/models are defined in `Abstractions/`
- [ ] `StubVideoGenerator` implements `IVideoGenerator`; its `GenerateAsync` copies a configured sample MP4 to `request.OutputPath` and returns `Success = true`
- [ ] `appsettings.json` contains documented placeholder sections for `MongoDB`, `CloudflareR2`, `VideoGenerator`, `Worker`

---

### Story 3.2 — Configure MongoDB Connectivity in Worker

**As the** VideoWorker,
**I want** a MongoDB connection configured and a `MongoJobQueue` class that can connect to the `video_generation_jobs` collection,
**so that** I can query, claim, and update jobs.

**Technical Context:**
- Register `IMongoClient` and the `video_generation_jobs` `IMongoCollection<VideoGenerationJob>` in `Program.cs`
- Implement `MongoJobQueue` with the full interface described in architecture Decision 2:
  - `TryClaimNextJobAsync` — `FindOneAndUpdateAsync` (atomic claim)
  - `MarkCompletedAsync`
  - `MarkFailedAsync`
  - `GetStaleProcessingJobsAsync`
  - `RequeueAsync`
- `VideoGenerationJob` class in the worker must match the SocialMediaAPI domain model BSON mapping. Consider extracting to a shared `SportifyCore` contracts project or duplicating the document class (document the decision — for MVP, duplication is acceptable to keep the projects independent)

**Acceptance Criteria:**
- [ ] `MongoJobQueue.TryClaimNextJobAsync` executes a single `FindOneAndUpdateAsync` (verify in integration test or log output)
- [ ] `MarkCompletedAsync` updates `status`, `outputVideoKey`, `completedAt`
- [ ] `MarkFailedAsync` updates `status`, `errorMessage`, `completedAt`, increments `retryCount`
- [ ] Worker starts and connects to MongoDB without errors (verify with a real or local MongoDB connection string in dev settings)
- [ ] Worker logs the MongoDB connection database name at startup

---

### Story 3.3 — Configure R2 Connectivity in Worker

**As the** VideoWorker,
**I want** a `R2AssetService` class that can download R2 objects to local paths and upload local files to R2,
**so that** I can retrieve job assets and store generated videos.

**Technical Context:**
- Register `IAmazonS3` in `Program.cs` with R2 endpoint configuration
- Implement `R2AssetService`:
  ```csharp
  Task<string> DownloadToLocalAsync(string objectKey, string localDirectory,
      CancellationToken cancellationToken);
  // Returns the local file path where the object was saved

  Task UploadFromLocalAsync(string localPath, string objectKey, string contentType,
      CancellationToken cancellationToken);
  ```
- `DownloadToLocalAsync`: calls `GetObjectAsync`, streams response to `localDirectory/{filename}`
- `UploadFromLocalAsync`: calls `PutObjectAsync` with file stream

**Acceptance Criteria:**
- [ ] `R2AssetService.DownloadToLocalAsync` downloads an R2 object and returns the local file path
- [ ] `R2AssetService.UploadFromLocalAsync` uploads a local file to R2 under the given key
- [ ] R2 credentials are loaded from `IConfiguration` — never hardcoded
- [ ] Integration-level test or manual test: download a test file from a configured R2 bucket, verify it lands locally

---

### Story 3.4 — Implement TempFileManager

**As the** VideoWorker,
**I want** a `TempFileManager` that creates a job-scoped temporary directory and cleans it up after the job completes,
**so that** temporary generation files are always cleaned up and never accumulate on the VM.

**Technical Context:**
```csharp
public class TempFileManager
{
    private readonly string _baseDir; // from config, e.g. "/tmp/video-generation"

    public string CreateJobDirectory(string jobId)
    {
        var dir = Path.Combine(_baseDir, jobId);
        Directory.CreateDirectory(dir);
        return dir;
    }

    public void Cleanup(string directory)
    {
        try { Directory.Delete(directory, recursive: true); }
        catch (Exception ex) { /* log but don't throw */ }
    }
}
```

**Acceptance Criteria:**
- [ ] `TempFileManager.CreateJobDirectory` creates a directory at `{baseDir}/{jobId}` and returns the path
- [ ] `TempFileManager.Cleanup` deletes the directory and all contents recursively
- [ ] `Cleanup` does not throw if the directory does not exist
- [ ] Base directory is configurable via `Worker:TempDirectory` setting (default: `/tmp/video-generation`)

---

## Epic 4 — Worker Core — Job Polling and Processing

Implement the Worker polling loop and JobProcessor that orchestrates the full job lifecycle end-to-end.

---

### Story 4.1 — Implement Worker Polling Loop

**As the** VideoWorker,
**I want** a continuous polling loop that claims Pending jobs and delegates to JobProcessor,
**so that** jobs are picked up and executed without manual intervention.

**Technical Context:**
Implement `Worker.cs` (extends `BackgroundService`):

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("Worker {WorkerId} started", _workerId);

    while (!stoppingToken.IsCancellationRequested)
    {
        VideoGenerationJob? job = null;
        try
        {
            job = await _jobQueue.TryClaimNextJobAsync(_workerId, stoppingToken);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Error polling for jobs");
            await Task.Delay(_pollInterval, stoppingToken);
            continue;
        }

        if (job == null)
        {
            _logger.LogDebug("No pending jobs. Waiting {Interval}s.", _pollInterval.TotalSeconds);
            await Task.Delay(_pollInterval, stoppingToken);
            continue;
        }

        await _jobProcessor.ProcessAsync(job, stoppingToken);
    }

    _logger.LogInformation("Worker {WorkerId} stopping", _workerId);
}
```

**Acceptance Criteria:**
- [ ] Worker polls MongoDB on the configured interval when no jobs are available (default 5s)
- [ ] Worker calls `JobProcessor.ProcessAsync` when a job is claimed
- [ ] Worker logs at `Debug` when waiting, `Information` on startup/shutdown
- [ ] Worker does not crash on MongoDB connection errors — logs and continues polling
- [ ] Worker stops cleanly when `CancellationToken` is triggered (SIGTERM / Ctrl+C)
- [ ] `WorkerId` is loaded from `Worker:Id` configuration (defaults to hostname if not set)

---

### Story 4.2 — Implement JobProcessor — Full Lifecycle

**As the** VideoWorker,
**I want** a `JobProcessor` that orchestrates the complete job lifecycle (download → generate → upload → complete/fail → cleanup),
**so that** each job is executed correctly regardless of the generator implementation.

**Technical Context:**
Implement `JobProcessor.ProcessAsync(job, cancellationToken)` per the pseudocode in architecture Decision 4:

1. Log: job claimed (`jobId`, `model`, `prompt` truncated to 100 chars)
2. `tempDir = TempFileManager.CreateJobDirectory(job.Id)`
3. `try {`
4.   Download each R2 key in `job.ReferenceImageKeys` via `R2AssetService`; collect local paths
5.   Download each R2 key in `job.KeyframeKeys` via `R2AssetService`; collect local paths
6.   Build `VideoGenerationRequest` from job fields + local paths; set `OutputPath = Path.Combine(tempDir, "output.mp4")`
7.   `result = await IVideoGenerator.GenerateAsync(request, cancellationToken)`
8.   `if (result.Success) {`
9.     Upload `result.VideoPath` to `job.OutputVideoKey` via `R2AssetService`
10.    `await _jobQueue.MarkCompletedAsync(job.Id, job.OutputVideoKey)`
11.    Log: job completed
12.  `} else {`
13.    `await _jobQueue.MarkFailedAsync(job.Id, result.ErrorMessage ?? "Unknown error")`
14.    Log: job failed
15.  `}`
16. `} catch (Exception ex) {`
17.   `await _jobQueue.MarkFailedAsync(job.Id, ex.Message)`
18.   Log error
19. `} finally {`
20.   `TempFileManager.Cleanup(tempDir)`
21.   Log: cleanup complete
22. `}`

Use a log scope with `jobId` for all entries within `ProcessAsync`.

**Acceptance Criteria:**
- [ ] `JobProcessor.ProcessAsync` executes all steps in order
- [ ] Download failures cause the job to be marked `Failed` with a descriptive error message
- [ ] Generator returning `Success = false` causes the job to be marked `Failed`
- [ ] Unexpected exceptions cause the job to be marked `Failed`; exception is logged
- [ ] `TempFileManager.Cleanup` is called in `finally` — always runs regardless of outcome
- [ ] All log entries within `ProcessAsync` include the `jobId` in a log scope
- [ ] The `MarkCompletedAsync` and `MarkFailedAsync` calls use the correct arguments

---

## Epic 5 — Phase 1 End-to-End Validation

Validate the complete pipeline end-to-end using the `StubVideoGenerator`, without GPU hardware. This verifies MongoDB job handling, R2 integration, worker lifecycle, error handling, and status updates.

---

### Story 5.1 — Phase 1 Happy Path Validation

**As a** developer,
**I want** to run the full pipeline (create job → worker claims → downloads from R2 → generates stub video → uploads to R2 → marks Completed) locally against a real MongoDB and R2 instance,
**so that** I can confirm all infrastructure integrations work before adding GPU complexity.

**Technical Context:**
This is a validation story, not a code-writing story. It confirms the integration of all Epic 1–4 work.

**Setup:**
1. Configure local `appsettings.Development.json` in VideoWorker with real MongoDB connection string and R2 credentials (test bucket)
2. Configure SocialMediaAPI `appsettings.Development.json` with the same R2 credentials
3. Ensure `VideoGenerator:Type = "Stub"` in worker config
4. Ensure a sample MP4 file exists at the configured `Stub:SampleVideoPath`

**Validation Steps:**
1. Start VideoWorker
2. Call `POST /api/video-generation/assets` — upload a test image → get object key
3. Call `POST /api/video-generation/jobs` — create job with the returned image key
4. Observe worker logs: job claimed, asset downloaded, generation started, upload started, job completed
5. Call `GET /api/video-generation/jobs/{jobId}` — verify `status = Completed`, `outputVideoKey` is populated
6. Verify the output MP4 exists in R2 at the expected key

**Acceptance Criteria:**
- [ ] Image uploaded to R2 via SocialMediaAPI endpoint
- [ ] Job created in MongoDB with status `Pending`
- [ ] Worker claims job (status transitions to `Processing`)
- [ ] Worker downloads reference image from R2 to local temp directory
- [ ] `StubVideoGenerator` writes a valid MP4 to the output path
- [ ] Worker uploads the MP4 to the job's `outputVideoKey` in R2
- [ ] MongoDB job status is `Completed`; `outputVideoKey` and `completedAt` are set
- [ ] Temp directory is cleaned up (no leftover files)

---

### Story 5.2 — Phase 1 Failure Handling Validation

**As a** developer,
**I want** to verify that job failure scenarios are handled correctly — invalid R2 key, generator failure, unexpected exception,
**so that** I can trust the error path before moving on to real model integration.

**Technical Context:**
Test each failure scenario manually or via an integration test:
- **Scenario A**: Job with a non-existent R2 key → worker downloads fail → job marked `Failed` with descriptive error
- **Scenario B**: Configure `StubVideoGenerator` to return `Success = false` → job marked `Failed`
- **Scenario C**: Configure `StubVideoGenerator` to throw an exception → job marked `Failed`; exception logged

After each failure scenario: verify temp directory is cleaned up.

**Acceptance Criteria:**
- [ ] Non-existent R2 key causes job to be marked `Failed` with a descriptive error message
- [ ] Generator returning `Success = false` causes job to be marked `Failed`
- [ ] Generator throwing an exception causes job to be marked `Failed`; exception message is stored in `errorMessage`
- [ ] Temp directory is cleaned up in all failure scenarios
- [ ] Worker continues polling for new jobs after a failure (does not crash)

---

## Epic 6 — WanGP/LTX Integration (Spike + Implementation)

Integrate the WanGP + LTX 2.3 model as the real `IVideoGenerator` implementation. This epic is blocked until the technical spike (Story 6.1) is complete and recommends an integration approach.

---

### Story 6.1 — Technical Spike: WanGP Programmatic Interface

**As a** developer,
**I want** to evaluate and document the best mechanism for a .NET process to invoke WanGP/LTX generation headlessly,
**so that** the `WanGpVideoGenerator` implementation is built on a validated approach rather than assumptions.

**This is a time-boxed investigation. Do not write production code in this story.**

**Investigate:**
1. Does WanGP support headless CLI invocation? Can a Python script be called as: `python generate.py --prompt "..." --image ./ref.png --output ./out.mp4 --model ltx-2.3` and produce an MP4 without a GUI?
2. Does WanGP expose a Gradio API (`/api/predict`) that can be called via HTTP? What is the request/response shape?
3. Is there a clean way to build a thin FastAPI/Flask wrapper around WanGP's generation pipeline?
4. What CUDA and PyTorch versions does LTX 2.3 require?
5. What is the VRAM requirement for LTX 2.3 at 1280x720, 10s duration?
6. Does WanGP support batch = 1 inference with a single reference image?

**Deliverable:** A short decision record (added to this document or a separate `spike-wangp-interface.md`) that:
- Identifies the recommended integration mechanism
- Provides a working proof-of-concept command or script
- Confirms VRAM requirements
- Identifies any constraints or configuration needed

**Acceptance Criteria:**
- [ ] Spike document or decision record committed with findings
- [ ] A working generation command or script is demonstrated (produces an MP4)
- [ ] The recommended integration mechanism is identified (CLI / Gradio API / Python wrapper)
- [ ] VRAM requirements at target resolution/duration are documented
- [ ] Any WanGP configuration required for headless/server mode is documented

---

### Story 6.2 — Implement WanGpVideoGenerator

**As the** VideoWorker,
**I want** a `WanGpVideoGenerator` that implements `IVideoGenerator` using the mechanism identified in the spike,
**so that** the worker can run real LTX 2.3 generation without changing the job processing logic.

**This story is blocked on Story 6.1.**

**Technical Context:**
Implement `Generators/WanGpVideoGenerator.cs` using the approach selected in the spike:

**If CLI subprocess (Option A):**
```csharp
public async Task<VideoGenerationResult> GenerateAsync(
    VideoGenerationRequest request, CancellationToken cancellationToken)
{
    var args = BuildCliArgs(request);
    var process = new Process { StartInfo = new ProcessStartInfo
    {
        FileName = _settings.PythonPath,     // e.g. "python3"
        Arguments = $"{_settings.ScriptPath} {args}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    }};
    process.Start();
    await process.WaitForExitAsync(cancellationToken);
    if (process.ExitCode != 0)
        return new VideoGenerationResult { Success = false, ErrorMessage = await process.StandardError.ReadToEndAsync() };
    return new VideoGenerationResult { Success = true, VideoPath = request.OutputPath };
}
```

**If HTTP API (Option B or C):**
```csharp
// Call configured API endpoint, poll for completion, return result path
```

Implementation details are determined by the spike. Keep all WanGP/Python-specific logic inside this class. The interface contract does not change.

**Acceptance Criteria:**
- [ ] `WanGpVideoGenerator` implements `IVideoGenerator`
- [ ] A generated MP4 is written to `request.OutputPath` on success
- [ ] Generation failure (non-zero exit code / HTTP error / timeout) returns `Success = false` with a descriptive `ErrorMessage` — does not throw
- [ ] `WanGpVideoGenerator` is registered in DI when `VideoGenerator:Type = "WanGP"`
- [ ] Configuration for WanGP (script path / API URL / timeout) is loaded from `appsettings.json`
- [ ] Switching from stub to WanGP requires only a config change, not a code change in `Worker.cs` or `JobProcessor.cs`

---

### Story 6.3 — Local Model Integration Validation

**As a** developer,
**I want** to run the full pipeline with `WanGpVideoGenerator` locally (on a machine with a GPU or WanGP installed),
**so that** I can validate the real generation path before deploying to RunPod.

**Acceptance Criteria:**
- [ ] A real MP4 is generated from a test prompt and reference image using LTX 2.3
- [ ] The generated video is uploaded to R2 and the job is marked `Completed`
- [ ] Generation completes within a reasonable time (establish a baseline for timeout configuration)
- [ ] Worker logs show the full lifecycle without errors

---

## Epic 7 — Docker Containerization

Package the worker and generation runtime into Docker containers for reproducible deployment.

---

### Story 7.1 — Worker Dockerfile

**As a** developer,
**I want** a `Dockerfile` for `VideoWorker` that produces a runnable Docker image,
**so that** the worker can be deployed consistently to any Docker-capable host.

**Technical Context:**
Multi-stage build in `services/VideoWorker/Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY services/VideoWorker/ .
RUN dotnet restore VideoWorker.csproj
RUN dotnet publish VideoWorker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
# Copy sample video for stub generator
COPY services/VideoWorker/samples/ /app/samples/
ENTRYPOINT ["dotnet", "VideoWorker.dll"]
```

**Acceptance Criteria:**
- [ ] `docker build -f services/VideoWorker/Dockerfile .` succeeds
- [ ] `docker run` starts the worker and it polls for jobs
- [ ] Environment variables override `appsettings.json` values (standard .NET config binding)
- [ ] Image does not contain source code or `.pdb` files in the final stage
- [ ] Image uses `mcr.microsoft.com/dotnet/runtime:8.0` (not SDK) in the final stage

---

### Story 7.2 — Generation Container Dockerfile

**As a** developer,
**I want** a Dockerfile for the WanGP/Python generation runtime,
**so that** the generation environment is reproducible and isolated from the .NET worker.

**This story is blocked on Story 6.1 (spike) and Story 6.2 (WanGpVideoGenerator).**

**Technical Context:**
- Location: `services/generation-container/Dockerfile`
- Base: CUDA + PyTorch image compatible with LTX 2.3 (determined by spike)
- Install WanGP and LTX 2.3 dependencies
- Model/checkpoint files loaded from mounted `/models` volume at startup (not baked into image)
- Startup script checks for model existence; downloads if absent (using huggingface-cli or wget)
- Expose HTTP API port if Option B/C; otherwise run as a CLI entrypoint

**Acceptance Criteria:**
- [ ] `docker build -f services/generation-container/Dockerfile .` succeeds
- [ ] Container starts and loads LTX 2.3 from the `/models` volume
- [ ] Container generates a test video from a test prompt (manual verification)
- [ ] Model checkpoint download only runs if `/models/{checkpoint}` does not exist

---

### Story 7.3 — Docker Compose for Local Development

**As a** developer,
**I want** a `docker-compose.yml` that starts the worker container and generation container together with shared configuration,
**so that** I can reproduce the GPU VM environment locally for testing.

**Technical Context:**
- Location: `services/docker-compose.video.yml` (separate from existing `docker-compose.yml` to avoid coupling)
- Services: `worker`, `generation`
- Shared named volumes: `models`, `generation-work`
- GPU device reservation on `generation` service (NVIDIA runtime)
- Environment variable pass-through for MongoDB, R2 credentials

**Acceptance Criteria:**
- [ ] `docker compose -f services/docker-compose.video.yml up` starts both containers
- [ ] Worker connects to MongoDB and R2 using environment variables
- [ ] Worker can communicate with the generation container
- [ ] Phase 1 happy path works inside Docker (stub generator)
- [ ] `models` volume persists across container restarts

---

## Epic 8 — GPU VM Deployment (RunPod)

Deploy the containerized pipeline to a RunPod GPU instance and validate end-to-end video generation.

---

### Story 8.1 — RunPod GPU VM Setup

**As a** developer,
**I want** a RunPod GPU instance configured with Docker, NVIDIA drivers, and persistent volume storage,
**so that** the worker and generation containers can run in a production-like environment.

**Technical Context:**
- Select RunPod GPU: RTX 4090 or equivalent based on VRAM requirements from spike
- Mount a persistent disk volume for the `/models` directory
- Verify Docker and NVIDIA Container Toolkit are available on the pod
- Configure environment variables for MongoDB connection string, R2 credentials

**Acceptance Criteria:**
- [ ] RunPod GPU pod is running with NVIDIA GPU accessible (`nvidia-smi` returns GPU info)
- [ ] Docker is available
- [ ] Persistent volume is mounted at `/models`
- [ ] Environment variables for MongoDB and R2 are configured (not committed to source)

---

### Story 8.2 — Deploy Containers to RunPod

**As a** developer,
**I want** the worker and generation containers running on the RunPod GPU pod,
**so that** the self-hosted pipeline operates on real GPU hardware.

**Technical Context:**
- Pull or transfer Docker images to the RunPod pod
- Run containers with the same `docker-compose.video.yml` used locally, with production environment variables
- Verify model checkpoint download on first start
- Verify persistent volume retains models across container restarts

**Acceptance Criteria:**
- [ ] Worker container starts and connects to MongoDB
- [ ] Generation container starts and loads LTX 2.3 from the persistent volume
- [ ] Model files are not re-downloaded on second container start

---

### Story 8.3 — End-to-End GPU Validation

**As a** developer,
**I want** to validate the complete pipeline on RunPod — from job creation in SocialMediaAPI to completed video in R2,
**so that** I can confirm the self-hosted pipeline works at the MVP level.

**Validation Steps:**
1. Upload a test reference image via SocialMediaAPI
2. Create a generation job
3. Observe worker claiming job on RunPod
4. Observe WanGP/LTX generating video on GPU
5. Verify MP4 uploaded to R2
6. Query job via SocialMediaAPI — verify `Completed` status and `outputVideoKey`

**Acceptance Criteria:**
- [ ] A real AI-generated video is produced from a prompt and reference image on the GPU VM
- [ ] The generated MP4 is accessible in R2 at the expected key
- [ ] Job status is `Completed` in MongoDB
- [ ] Total wall-clock time from job creation to Completed is within a reasonable range (document the observed baseline)
- [ ] Temp files are cleaned up on the GPU VM after completion

---

## Epic 9 — Production Hardening (Post-MVP)

Address operational reliability concerns after the end-to-end pipeline is validated. Do not implement these before the basic flow works.

---

### Story 9.1 — Stale Job Recovery

**As the** VideoWorker,
**I want** a background check that detects and re-queues stale `Processing` jobs,
**so that** a worker crash does not permanently block a job.

**Technical Context:**
Add a periodic stale job check inside `Worker.ExecuteAsync`:
- Every N minutes (configurable, e.g., every 5 poll cycles)
- Query `GetStaleProcessingJobsAsync(DateTime.UtcNow - staleTimeout)` (staleTimeout configurable, default 30 min)
- For each stale job: if `retryCount < maxRetries`, call `RequeueAsync`; else call `MarkFailedAsync("Max retries exceeded")`
- Log all recovery actions

**Acceptance Criteria:**
- [ ] Stale check runs periodically (configurable interval)
- [ ] Jobs stuck in `Processing` beyond the timeout are re-queued with incremented `retryCount`
- [ ] Jobs exceeding `maxRetries` are marked `Failed`
- [ ] Worker logs stale job ID, retry count, and action taken

---

### Story 9.2 — Retry Policy and Backoff

**As the** VideoWorker,
**I want** failed jobs that are within their retry limit to be automatically re-queued with backoff,
**so that** transient failures (model OOM, brief R2 outage) are recovered without manual intervention.

**Acceptance Criteria:**
- [ ] `retryCount` is incremented on each failure
- [ ] Jobs with `retryCount < maxRetries` are requeued to `Pending` with a brief delay (e.g., 60s before next claim)
- [ ] Jobs with `retryCount >= maxRetries` remain `Failed` and are not automatically requeued
- [ ] `maxRetries` is configurable (default: 3)

---

### Story 9.3 — Worker Heartbeat / Lease Mechanism

**As an** operator,
**I want** the worker to write a heartbeat timestamp to the job document while it is `Processing`,
**so that** stale job detection can distinguish a running job from a crashed worker more precisely.

**Technical Context:**
- Add `lastHeartbeatAt : DateTime?` to `VideoGenerationJob` document
- Worker updates `lastHeartbeatAt` every N seconds while processing
- Stale job query uses `lastHeartbeatAt` instead of `startedAt` for recency check

**Acceptance Criteria:**
- [ ] `lastHeartbeatAt` is updated periodically while job is `Processing`
- [ ] Stale job detection uses `lastHeartbeatAt` (not `startedAt`) when available
- [ ] Heartbeat interval is configurable (default: 30s)

---

### Story 9.4 — Structured Logging and Observability

**As an** operator,
**I want** structured log output from the worker with consistent fields (jobId, workerId, status, duration),
**so that** logs are queryable in a log aggregation tool and job-level timing is visible.

**Acceptance Criteria:**
- [ ] All worker log entries include `jobId` when processing a specific job (via `ILogger.BeginScope`)
- [ ] Job completion log includes wall-clock duration from `startedAt` to `completedAt`
- [ ] Log output is structured JSON (configure `Serilog` or equivalent with JSON formatter)
- [ ] Log level is configurable via environment variable

---

## Story Dependency Map

```
Epic 1 (R2 Integration)
  1.1 → 1.2

Epic 2 (Job Schema)
  2.1 → 2.2 → 2.3 → 2.4

Epic 3 (Worker Scaffolding)
  3.1 → 3.2 → 3.3 → 3.4
  (3.2 blocked on 2.2 for shared document schema)

Epic 4 (Worker Core)
  4.1 → 4.2
  (blocked on Epic 3)

Epic 5 (Phase 1 Validation)
  5.1 → 5.2
  (blocked on Epics 1, 2, 3, 4)

Epic 6 (WanGP Integration)
  6.1 (spike — not blocked)
  6.2 → blocked on 6.1
  6.3 → blocked on 6.2

Epic 7 (Docker)
  7.1 → blocked on Epic 3
  7.2 → blocked on 6.2
  7.3 → blocked on 7.1, 7.2

Epic 8 (GPU VM)
  8.1 (parallel)
  8.2 → blocked on 7.1, 7.2, 8.1
  8.3 → blocked on 8.2

Epic 9 (Hardening)
  → blocked on 8.3 (all of Epic 9)
```

---

## Implementation Order Recommendation

Build in this sequence to maximize early feedback:

1. **2.1** — Redesign job domain model (unblocks everything)
2. **1.1** — R2 client configuration in SocialMediaAPI
3. **3.1** — Scaffold VideoWorker project
4. **2.2** — Repository atomic claim methods
5. **3.2** — Worker MongoDB connectivity
6. **3.3** — Worker R2 connectivity
7. **3.4** — TempFileManager
8. **1.2** — Asset upload endpoint
9. **2.3** — Job creation endpoint
10. **2.4** — Job query endpoint update
11. **4.1** — Worker polling loop
12. **4.2** — JobProcessor full lifecycle
13. **5.1** — Phase 1 happy path validation ← **first milestone**
14. **5.2** — Phase 1 failure handling validation
15. **6.1** — WanGP spike (can start in parallel with 5.x)
16. **6.2** — WanGpVideoGenerator (blocked on 6.1)
17. **6.3** — Local model validation
18. **7.1** — Worker Dockerfile
19. **7.2** — Generation container Dockerfile
20. **7.3** — Docker Compose
21. **8.1** — RunPod setup
22. **8.2** — Deploy to RunPod
23. **8.3** — End-to-end GPU validation ← **MVP milestone**
24. **9.x** — Hardening stories (post-MVP)
