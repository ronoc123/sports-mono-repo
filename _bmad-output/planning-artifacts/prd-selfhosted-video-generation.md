# PRD — Self-Hosted AI Video Generation Pipeline

**Author:** Kampe
**Date:** 2026-09-06
**Type:** Feature Addition (Brownfield)
**Related Artifacts:** architecture-social-media-ai.md, prd-claude-video-generation.md

---

## Overview

This PRD describes adding a **self-hosted AI video generation pipeline** to the existing Social Media platform. The system enables the .NET backend (SocialMediaAPI) to create structured video generation jobs, store required assets in Cloudflare R2, and hand those jobs asynchronously to a dedicated GPU-hosted .NET Worker Service. The worker generates videos using open-source models (initially WanGP + LTX 2.3) running locally on a GPU VM, then stores results back in R2 and updates job status in MongoDB.

**The OpenAI creative/pre-production pipeline** (script generation, shot planning, keyframe generation from character context) is explicitly **out of scope** for this PRD and will be addressed in a separate feature. This PRD focuses on the rendering infrastructure, job lifecycle, R2 asset storage, and the worker service.

---

## Problem Statement

The current video generation path relies on **Higgsfield AI** — a hosted external API — via Claude MCP integration. This introduces:

- Per-video API costs that scale with volume
- Dependency on external service availability and model selection
- Limited control over generation quality, settings, and model version
- Video assets stored only in temporary local files with no durable external storage
- No separation between the web application and the rendering workload — both run in the same ASP.NET process

The self-hosted pipeline addresses each of these by running generation on GPU infrastructure under direct control, storing assets durably in Cloudflare R2, and establishing a clean contract between the application backend and the rendering worker through a MongoDB-backed job queue.

---

## Goals

1. **Self-hosted generation**: Run AI video generation on GPU infrastructure we control, with no per-video external API costs beyond GPU hosting.
2. **Durable asset storage**: Store reference images, keyframes, and generated videos in Cloudflare R2 — not temporary local disk.
3. **Clean architectural separation**: The backend creates and monitors jobs. The worker executes them. Neither knows the internals of the other.
4. **Model flexibility**: Introduce an `IVideoGenerator` abstraction so the initial LTX 2.3 model can be replaced with Wan, HunyuanVideo, or future models without redesigning the worker.
5. **Reliable job processing**: Use atomic MongoDB job claiming so no job is processed twice, even if multiple workers are added later.
6. **Incremental delivery**: Validate the full pipeline end-to-end with a stub generator before introducing real GPU/model complexity.

---

## Non-Goals (Current Scope)

The following are explicitly out of scope for this PRD:

- OpenAI integration for script generation, shot planning, or keyframe creation
- Character definitions, character reference image management, or consistency tooling
- Frontend UI changes — job creation is API-driven; frontend changes are a separate story
- Multiple simultaneous GPU workers or horizontal autoscaling
- Kubernetes, Kafka, RabbitMQ, or dedicated message broker infrastructure
- Automatic GPU VM startup or shutdown based on queue depth
- Job scheduling, priority queuing, or job cancellation
- Bulk or batch video generation
- Changes to the existing post cycle review step, metadata editing, or platform posting flow
- Productionized metrics dashboards or alerting

---

## System Roles

**Backend (SocialMediaAPI)**: The orchestration layer. Creates generation jobs, manages reference asset uploads to R2, exposes job status to callers. Does not perform GPU rendering.

**Video Worker Service**: The rendering layer. A long-running .NET Worker Service running on a GPU VM. Claims jobs from MongoDB, downloads assets from R2, invokes local video generation, uploads results to R2, and updates job status.

**Video Generator (local model)**: The rendering implementation. Initially WanGP + LTX 2.3 running on the GPU VM. Hidden behind `IVideoGenerator` — the worker does not contain model-specific logic outside the implementation class.

**Cloudflare R2**: Object storage for binary assets. Reference images, keyframes (future), and output MP4 files. Never stores structured metadata — that lives in MongoDB.

**MongoDB**: Structured metadata and job queue. Job state, parameters, R2 object keys, worker assignments, timestamps, error information. Never stores binary data.

---

## User Journeys

### Journey 1 — Submit a Video Generation Job

An authorized caller (backend service, admin API, or automated pipeline) submits a video generation job to SocialMediaAPI. The request includes:

- A text prompt describing the desired video
- R2 object keys for pre-uploaded reference images
- Optional R2 object keys for keyframes
- Generation parameters: model, duration, resolution, aspect ratio
- The desired R2 output key for the result video

The backend validates the request, creates a `Pending` job in MongoDB, and returns the job ID. The caller does not wait for generation.

### Journey 2 — Worker Executes a Job

The running Video Worker Service (on the GPU VM) continuously polls MongoDB for `Pending` jobs. When a job is available:

1. The worker atomically claims the job (`Pending` → `Processing`, sets worker ID and start time).
2. The worker reads the job payload — the complete set of R2 asset keys and generation parameters.
3. The worker downloads each R2 asset to a local temporary directory on the VM.
4. The worker calls `IVideoGenerator.GenerateAsync(...)` with the prompt, local asset paths, and generation settings.
5. The generator runs locally on the GPU and produces an MP4 file.
6. The worker uploads the MP4 to the job's designated R2 output key.
7. The worker marks the job `Completed` in MongoDB and records the output key and completion timestamp.
8. Temporary local files are cleaned up.
9. The worker looks for the next job.

### Journey 3 — Query Job Status

The backend or an authorized caller queries a job by ID. The response includes current status, timestamps, the output R2 key (when `Completed`), and error details (when `Failed`).

### Journey 4 — Handle Job Failure

If the worker encounters an error during any phase (R2 download, generation, R2 upload):

- The job is marked `Failed` in MongoDB with a human-readable error message.
- The retry count is incremented.
- Temporary local files are cleaned up in a `finally` block.
- The caller can inspect the error and decide to re-queue the job manually.

### Journey 5 — Handle Worker Crash or Restart

If the worker crashes mid-execution:

- The job remains in `Processing` state with a `StartedAt` timestamp.
- A background process (or future lease mechanism) detects the stale `Processing` job based on elapsed time and re-queues it to `Pending`.
- The worker picks it up again on restart.

---

## Functional Requirements

| ID | Requirement |
|----|------------|
| FR-SHV-01 | The backend can create a video generation job in MongoDB with status `Pending` |
| FR-SHV-02 | A job payload must include: prompt, model identifier, duration (seconds), resolution, aspect ratio, R2 reference image keys (list), optional R2 keyframe keys (list), desired output R2 key |
| FR-SHV-03 | The backend can query a job by ID and receive: status, output R2 key (when Completed), error message (when Failed), all timestamps |
| FR-SHV-04 | The backend can upload a reference image to Cloudflare R2 under a deterministic object key and receive the key in response |
| FR-SHV-05 | The backend does not store binary image or video data in MongoDB — only R2 object keys |
| FR-SHV-06 | The worker atomically claims a `Pending` job: sets status to `Processing`, assigns `WorkerId`, sets `StartedAt` — using a single MongoDB `findOneAndUpdate` operation |
| FR-SHV-07 | No two workers may claim or process the same job simultaneously |
| FR-SHV-08 | The worker reads the complete job payload from MongoDB after claiming; it does not search R2 to determine what it needs |
| FR-SHV-09 | The worker downloads all R2 assets referenced in the job to a local temporary directory before invoking the generator |
| FR-SHV-10 | The worker passes prompt, local asset paths, duration, resolution, aspect ratio, and model options to `IVideoGenerator.GenerateAsync(...)` |
| FR-SHV-11 | The worker uploads the generated MP4 to the job's designated R2 output key |
| FR-SHV-12 | The worker marks the job `Completed` in MongoDB with the output R2 key and `CompletedAt` timestamp |
| FR-SHV-13 | The worker marks the job `Failed` in MongoDB with a descriptive error message when generation or any prerequisite step fails |
| FR-SHV-14 | The worker cleans up all temporary local files after each job, regardless of outcome (success or failure) |
| FR-SHV-15 | The worker polls MongoDB at a configurable interval (default: 5 seconds) when no jobs are available |
| FR-SHV-16 | The system exposes an `IVideoGenerator` interface that accepts a generation request and returns a result (video path or error); it must not expose model-specific types in its contract |
| FR-SHV-17 | A `StubVideoGenerator` implementation exists that copies a sample MP4 to the output path — used for Phase 1 infrastructure validation without a real GPU/model |
| FR-SHV-18 | The worker logs all significant lifecycle events: job claimed, download started/completed, generation started/completed, upload started/completed, job completed/failed |
| FR-SHV-19 | The worker and backend share the same MongoDB job collection but are otherwise independently deployable |

---

## Non-Functional Requirements

| ID | Requirement |
|----|------------|
| NFR-SHV-01 | Job claiming must be atomic — single `findOneAndUpdate` with filter `Status = Pending` prevents duplicate claims |
| NFR-SHV-02 | R2 credentials (account ID, access key, secret key, bucket name) must never be hardcoded — always sourced from configuration/environment |
| NFR-SHV-03 | MongoDB connection string must never be hardcoded — always from configuration/environment |
| NFR-SHV-04 | Temporary local generation files must be cleaned up after each job regardless of outcome |
| NFR-SHV-05 | Model/checkpoint files must persist across worker container restarts via a named persistent volume on the GPU VM — not ephemeral container storage |
| NFR-SHV-06 | The worker must be deployable as a Docker container |
| NFR-SHV-07 | The worker must be restartable without losing `Pending` jobs; restart only risks re-processing the in-flight `Processing` job |
| NFR-SHV-08 | The system must handle stale `Processing` jobs (worker crash): a background mechanism must detect and re-queue them after a configurable timeout |
| NFR-SHV-09 | The `IVideoGenerator` interface must not expose model-specific types (CUDA, PyTorch, WanGP settings) in its public contract |
| NFR-SHV-10 | Initial target throughput: ~10 videos/day on a single GPU worker. No horizontal scaling required at MVP. |
| NFR-SHV-11 | The job schema must be designed to support adding a message broker (Redis, RabbitMQ) as the queue transport in future without redesigning the job document |

---

## Architecture Summary

```
SocialMediaAPI (.NET)
  ├── Creates Pending jobs in MongoDB
  ├── Uploads reference images to R2
  ├── Exposes job status/result API
  └── Does NOT render video

MongoDB (video_generation_jobs)
  ├── Job queue (Status = Pending → claimed atomically)
  ├── Job metadata (prompt, model, settings, R2 keys)
  └── Job lifecycle (status, timestamps, worker, errors)

Cloudflare R2
  ├── generation/{jobId}/ref-{n}.png    ← reference images (uploaded by backend)
  ├── generation/{jobId}/frame-{n}.png  ← keyframes (future)
  └── generation/{jobId}/output.mp4     ← generated video (uploaded by worker)

GPU VM (RunPod)
  └── VideoWorker (.NET Worker Service)
      ├── Polls MongoDB for Pending jobs
      ├── Claims job atomically
      ├── Downloads R2 assets to /tmp/{jobId}/
      ├── Calls IVideoGenerator.GenerateAsync(...)
      │     └── WanGpVideoGenerator → WanGP/LTX runtime
      ├── Uploads MP4 to R2
      ├── Updates job: Completed / Failed
      └── Cleans up /tmp/{jobId}/
```

---

## Phased Delivery Plan

### Phase 1 — Worker Infrastructure (No GPU Required)
Validate the full pipeline end-to-end using a stub generator:
- Backend creates a `Pending` job with test R2 image keys
- Worker claims job, downloads test image from R2, runs `StubVideoGenerator` (copies a sample MP4), uploads to R2, marks `Completed`
- Validates: MongoDB job handling, R2 integration, worker lifecycle, error handling, status updates

### Phase 2 — Local Model Integration
Replace the stub with real WanGP/LTX integration. First complete a technical spike to determine the correct programmatic interface (CLI, Gradio API, Python subprocess). Then implement `WanGpVideoGenerator` and validate generation locally.

### Phase 3 — Docker
Containerize the worker and validate Phase 1 workflow inside Docker on a development machine.

### Phase 4 — GPU VM
Deploy worker container and WanGP/generation runtime to a RunPod GPU instance. Validate full end-to-end pipeline with real video generation.

### Phase 5 — Production Hardening
After end-to-end works: stale job recovery, worker heartbeat leases, retry policy, timeouts, observability.

---

## Success Criteria (MVP — Phase 4 Complete)

1. SocialMediaAPI creates a `Pending` job in MongoDB and uploads a reference image to R2.
2. The Video Worker on RunPod claims the job atomically.
3. The worker downloads the reference image from R2 to the GPU VM disk.
4. WanGP + LTX 2.3 generates an MP4 from the prompt and reference image.
5. The worker uploads the MP4 to the job's R2 output key.
6. The worker marks the job `Completed` in MongoDB.
7. SocialMediaAPI queries the job and retrieves the output R2 key.
8. The worker runs in a Docker container with persistent model storage on the GPU VM.
9. The workflow survives a worker restart without losing `Pending` jobs.

---

## Open Questions / Risks

| # | Question | Impact | Resolution |
|---|----------|--------|-----------|
| 1 | What is the best programmatic interface to WanGP/LTX? CLI? Gradio API? Python subprocess? | Phase 2 implementation approach | Technical spike (Epic 8, Story 8.1) |
| 2 | Does LTX 2.3 on an RTX 4090 fit within RunPod's VRAM constraints for the target resolution and duration? | GPU cost and model viability | Performance testing in Phase 4 |
| 3 | Does the .NET Worker and WanGP runtime need to run in separate Docker containers, or can they share one? | Docker architecture, shared volume vs HTTP API | Determined by spike outcome (Story 8.1) |
| 4 | What is the RunPod GPU pricing and startup time for an RTX 4090? | Operational cost and job latency | Research in Phase 4 setup |
| 5 | How should R2 presigned URLs be handled — should the frontend access videos directly, or via a signed proxy endpoint? | Frontend integration (future story) | Deferred until frontend UI epic |
