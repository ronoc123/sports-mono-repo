---
stepsCompleted: ['step-01-init', 'step-02-discovery', 'step-02b-vision', 'step-02c-executive-summary', 'step-03-success', 'step-04-journeys', 'step-05-domain', 'step-06-innovation', 'step-07-project-type', 'step-08-scoping', 'step-09-functional', 'step-10-nonfunctional', 'step-11-polish']
inputDocuments: []
workflowType: 'prd'
classification:
  projectType: 'full-stack-web-app'
  domain: 'content-creation-social-media-automation'
  complexity: 'medium'
  projectContext: 'brownfield'
---

# Product Requirements Document — Social Media AI Video Tool

**Author:** Kampe
**Date:** 2026-08-19

## Executive Summary

A personal content automation cockpit for managing multiple social media personas at scale. The product centers on the **Channel** — a named identity that owns one linked account per social platform, a full history of published content (titles, descriptions, hashtags, video metadata), and a persistent AI generation context. That accumulated history feeds into a pluggable AI video generation adapter to produce a new on-brand video "with a new spin," the output passes through a human review and metadata editing step, then publishes simultaneously to all linked platforms via pluggable social media adapters. The workflow is a closed loop: each post enriches the channel's history, which improves the next generation.

The target user is a single operator managing a high volume of social media personas — the tool eliminates the per-post overhead of context-setting, platform-switching, and manual distribution.

### What Makes This Special

Three compounding advantages define this product:

1. **Context-aware generation, not cold-start.** Every AI video generation request is seeded with the channel's full content history — past titles, descriptions, hashtags, and style signals. The AI produces content that feels like a natural continuation of the channel's voice, not a generic output.

2. **Dual plugin architecture.** Both the AI generation layer (adapters for Sora, RunwayML, Kling, etc.) and the social posting layer (adapters per platform: TikTok, Instagram, YouTube, X, etc.) are independently extensible. New tools and new platforms plug in without touching core business logic.

3. **Channel as a first-class persona.** A Channel is not a playlist — it is a managed identity with its own linked platform accounts, generation history, and context. Managing ten personas is the same UX as managing one.

The review step is productive rather than passive: the operator can edit title, description, and hashtags inline, or re-prompt the AI with additional instructions and regenerate before committing to publish.

## Project Classification

| Dimension | Value |
|-----------|-------|
| **Project Type** | Full-Stack Web App — Angular SPA + .NET 8 REST API |
| **Domain** | Content Creation / Social Media Automation |
| **Complexity** | Medium — no regulatory burden; integration complexity from multi-provider adapters and async media workflows |
| **Project Context** | Brownfield — new `SocialMediaAPI` service + new frontend feature module added to `sports-mono-repo` |
| **Tech Stack** | Angular 20 + NgRx Signals Store (frontend) · ASP.NET Core 8 + MongoDB + Clean Architecture (backend) |

## Success Criteria

### User Success

- A complete post cycle — from opening a channel to a video published on YouTube — takes measurably less time than the manual workflow of logging into platforms, copying metadata, and posting individually
- Managing 10 channels feels operationally identical to managing 1: per-channel overhead is eliminated by the unified workflow
- When a real AI video generation tool is connected, zero architectural changes are required — the adapter contract is in place and channel history is already structured to feed it

### Business Success

- A channel's full post history (titles, descriptions, hashtags) is captured automatically and available to seed AI generation the moment a tool is connected
- Each new social media platform requires only a new `ISocialMediaAdapter` implementation — no changes to core Channel, workflow, or generation logic
- Adding a new channel costs minutes, not a new workflow

### Technical Success

- `IVideoGenerationAdapter` interface is well-defined and proven extensible — a real AI tool can be wired up by implementing one interface with no core changes
- `ISocialMediaAdapter` interface is well-defined — YouTube is the first implementation; TikTok is second; adding either requires no changes to Channel or posting orchestration logic
- Channel post history is durably stored in MongoDB and structured to serve as AI context (titles, descriptions, hashtags, timestamps, style notes)
- Per-platform posting failures are isolated and reported without affecting other platforms

### Measurable Outcomes

| Outcome | Target |
|---------|--------|
| Post cycle time | Measurably faster than manual workflow |
| New platform adapter effort | Implement `ISocialMediaAdapter`, no core changes |
| New AI tool adapter effort | Implement `IVideoGenerationAdapter`, no core changes |
| Channel history accuracy | 100% of published posts captured in channel history |

## Product Scope

### MVP — Minimum Viable Product

**Goal:** The core loop works end-to-end with YouTube. Architecture is adapter-ready for AI generation and additional platforms.

- **Channel management** — Create, edit, delete channels; each channel stores name, description, style/tone context notes, and one linked account per platform
- **`IVideoGenerationAdapter` interface** — Fully defined contract (input: channel context + history + optional prompt; output: video URL/file + suggested metadata). Stub implementation in MVP — the "Generate" button exists in the UI but invokes the stub
- **`ISocialMediaAdapter` interface** — Fully defined contract (input: video + metadata; output: post result + platform post ID). YouTube is the first real implementation
- **Video submission** — Operator manually uploads a video file for the current post cycle; interim path until a real AI adapter is wired up
- **Review step** — View the video, edit title, description, and hashtags inline; option to re-enter the generation step with an additional prompt (stub for now)
- **Post to channel** — Posts the reviewed video + metadata to all linked platform accounts simultaneously in a single action
- **Post history** — Every published post recorded at the channel level (title, description, hashtags, timestamp; per-platform: status, post ID or error)
- **Channel dashboard** — List of channels with last post summary; entry point to start a new post cycle

### Growth Features (Post-MVP)

- **TikTok adapter** — Implement `ISocialMediaAdapter` for TikTok; plug into existing architecture
- **First real AI generation adapter** — Implement `IVideoGenerationAdapter` for the chosen AI video tool (TBD); channel history feeds in automatically
- **Additional platforms** — Instagram Reels, X, etc. as adapter implementations
- **Scheduling** — Queue a post for a future publish time per platform
- **Regenerate with prompt** — Full working re-prompt + regenerate loop once AI adapter is live

### Vision (Future)

- AI-suggested prompts based on channel history + trending context
- Bulk generation — queue multiple videos for a channel in one session
- Engagement analytics pulled from platforms, fed into next generation context
- Multiple AI providers switchable per channel
- Cross-channel repurposing (reformat a YouTube video for TikTok)

## User Journeys

### Journey 1: The Core Post Cycle (Primary — Happy Path)

**Persona:** Kampe, managing 8 social personas. It's content day.

**Opening scene:** Kampe opens the dashboard and sees his channels listed — TechReviews, FitLife, GameClips, and five others. Each shows the last post date and a snippet of the most recent title. He clicks into TechReviews.

**Rising action:** The channel view shows the full post history — 24 entries, each with title, description, hashtags, and publish date. He clicks "New Post." The system opens the post cycle. Since no AI adapter is connected yet, he uploads a video file he recorded earlier. The review screen loads: the video player on the left, editable metadata fields on the right. The description and hashtag fields show suggestions pulled from TechReviews' history — pre-populated from what this channel typically uses. He adjusts the title and tweaks two hashtags.

**Climax:** He clicks "Post to Channel." The system posts simultaneously to all linked platform accounts for TechReviews — YouTube in MVP, expanding to TikTok and others as adapters are added. A progress view shows each platform in real time: uploading → processing → published.

**Resolution:** Live platform links appear. The post is logged in TechReviews' history. Kampe moves to the next channel. What previously took 20 minutes of tab-switching, copy-pasting, and manual uploading per channel is now a single focused flow.

**Capabilities revealed:** Channel dashboard, post history view, video upload, context-seeded metadata suggestions, multi-platform simultaneous posting, post history recording, per-platform status feedback.

---

### Journey 2: Partial Post Failure — Edge Case

**Persona:** Kampe posting to GameClips, which has YouTube and TikTok linked.

**Opening scene:** Post cycle runs normally. YouTube publishes successfully. TikTok fails — OAuth token expired.

**Rising action:** The result screen clearly separates successes from failures. YouTube shows a green row with a video link; TikTok shows a red row with the error type and a "Re-authenticate & Retry" action. Kampe clicks it, completes the OAuth flow in a pop-up, and returns to the result screen.

**Climax:** The system retries only the failed platform. TikTok publishes.

**Resolution:** Both platforms show green. The post history entry is updated with platform post IDs for both. Kampe never had to re-enter the metadata, re-upload the video, or re-trigger YouTube.

**Capabilities revealed:** Per-platform failure isolation, retry without restarting the cycle, OAuth re-authentication flow, partial success state in post history.

---

### Journey 3: New Channel Setup

**Persona:** Kampe starting a fitness content vertical from scratch.

**Opening scene:** Kampe clicks "New Channel" from the dashboard. A setup form opens.

**Rising action:** He fills in the channel details — name: "FitLife", description: "High-energy fitness challenges and workout motivation, 3–5 minute videos, upbeat tone, targeted at people who work out at home." This description becomes the style/tone context that seeds AI generation later. He links his FitLife YouTube account via OAuth. He saves.

**Climax:** FitLife appears on the dashboard — zero posts, ready to go. Kampe kicks off the first post cycle immediately: uploads a video, fills in metadata manually (no history yet to seed from), and posts.

**Resolution:** FitLife's first post is live. Every future post cycle for this channel uses this and subsequent posts to inform metadata suggestions and, eventually, AI generation context.

**Capabilities revealed:** Channel creation form, style/tone context field, YouTube OAuth account linking, empty-state handling on first post, context that grows over time.

---

### Journey 4: Platform Extension — Adding TikTok

**Persona:** Kampe the developer, adding TikTok support after MVP ships.

**Opening scene:** Kampe implements `ISocialMediaAdapter` for TikTok — a single class that satisfies the interface contract: accepts video + metadata, calls TikTok's API, returns a post result with a platform post ID.

**Rising action:** He registers the TikTok adapter in the Infrastructure DI configuration. No changes to Channel, no changes to the post orchestration logic, no changes to the review step. He opens FitLife's channel settings and sees "Add Account." TikTok now appears as an available platform alongside YouTube. He links his FitLife TikTok account.

**Climax:** Next post cycle for FitLife: he goes through the same review step as always. He clicks "Post to Channel." The system fans out to YouTube AND TikTok simultaneously.

**Resolution:** Post history shows per-platform results for the post. TikTok was added in an afternoon, with zero changes to the core application.

**Capabilities revealed:** `ISocialMediaAdapter` interface contract, adapter registration pattern, per-channel platform account management, multi-platform simultaneous posting.

---

### Journey Requirements Summary

| Journey | Capabilities Required |
|---------|----------------------|
| Core post cycle | Channel list, post history, video upload, context-seeded metadata, multi-platform fan-out, per-platform status, history recording |
| Partial failure | Per-platform failure isolation, retry single platform, OAuth re-auth flow, partial-success post history state |
| New channel setup | Channel CRUD, style/tone context field, OAuth account linking per platform, empty-state first-post handling |
| Platform extension | `ISocialMediaAdapter` contract, adapter registration via DI, per-channel platform account management |

## Innovation & Novel Patterns

### Detected Innovation Areas

**Context-accumulation loop:** Post history is structured to function as AI generation context — not a log, but a continuous feed that grows with every published post. Generation quality compounds over time without extra operator effort.

**Dual adapter symmetry:** AI generation and social distribution are independently extensible via separate adapter contracts. Neither side dictates the other's evolution. A new AI tool requires no platform changes; a new platform requires no generation changes.

**Channel as context container:** A Channel is a managed identity that accumulates semantic context (tone, vocabulary, hashtag patterns, title style). Multi-persona management becomes operationally flat — the overhead per persona is eliminated, not reduced.

### Validation Approach

- Channel history grows correctly and is retrievable as a structured AI context payload
- A stub `IVideoGenerationAdapter` can be replaced with a real implementation with zero core changes
- A second `ISocialMediaAdapter` (TikTok) can be added with zero changes to Channel, workflow, or generation logic

### Risk Mitigation

- If AI generation tools change their APIs, only the adapter implementation changes
- If a platform bans API access, only that adapter is affected — other platforms continue uninterrupted
- Channel history is durable in MongoDB; no history is lost if a generation adapter is swapped

## Full-Stack Web App Specific Requirements

### Project-Type Overview

A brownfield Angular 20 SPA feature module added to `sports-mono-repo`, backed by the new `SocialMediaAPI` (.NET 8). The frontend follows the existing NgRx Signals Store pattern. Desktop-first (single-operator personal tool); mobile responsiveness inherits from the existing shell but is not a primary concern.

A Channel holds **one linked account per platform** — one YouTube, one TikTok, one Instagram, etc. Fan-out during posting is across platforms simultaneously.

### Technical Architecture Considerations

**Frontend (Angular 20 + NgRx Signals Store)**
- New `social-media` feature module added to the existing Angular SPA
- State managed via NgRx Signals Store (consistent with existing app pattern)
- No routing changes to the shell beyond adding the new module routes

**Backend (ASP.NET Core 8 / SocialMediaAPI)**
- Clean Architecture: Domain → Application → Infrastructure → WebAPI
- MongoDB for all persistence (channels, linked accounts, post history)
- OAuth refresh tokens stored as encrypted fields in MongoDB (no external secrets store)

### Real-Time Posting Progress

Angular polls the API for posting job status every 2–3 seconds while a fan-out is in progress. No WebSocket or SSE infrastructure required.

- API exposes a job status endpoint returning per-platform posting state
- Frontend displays per-platform progress (pending → uploading → published / failed)
- Polling stops when all platforms reach a terminal state

### Video Upload

Direct upload to the .NET API via multipart form post. No cloud storage dependency in MVP.

- Kestrel `MaxRequestBodySize` configured for up to 500MB (sufficient for a 1-minute video at typical social media quality: 50–300MB)
- Streaming upload — file written to a temp location on disk, not buffered in memory
- Temp file deleted after post cycle completes (success or failure)
- Azure Blob Storage with presigned upload URL is the natural upgrade path if file sizes grow

### OAuth Credential Management

- One refresh token stored per linked platform account per channel
- Tokens encrypted at rest using AES-256; key configured via app settings
- Access tokens fetched on-demand using the stored refresh token; never persisted
- Re-authentication flow triggered when a refresh token is expired or revoked

### Browser & Accessibility

- Target: modern Chromium-based browsers (personal tool — no legacy browser support)
- Accessibility: basic semantic HTML; no WCAG compliance target beyond what the existing Angular app provides
- SEO: not applicable (authenticated personal tool, no public-facing routes)

### Performance Targets

- Channel dashboard load: < 1 second (channel list + last post metadata from MongoDB)
- Post history load per channel: < 2 seconds (paginated, 20 posts per page)
- Polling interval during fan-out: 2–3 seconds; timeout after 10 minutes with error recorded

## Project Scoping & Phased Development

### MVP Strategy & Philosophy

**MVP Approach:** Platform-first — prove the end-to-end loop works on YouTube, with all architectural contracts in place for extension. No external services beyond MongoDB and the YouTube Data API v3.

### Phased Roadmap

| Phase | Focus |
|-------|-------|
| **Phase 1 — MVP** | YouTube end-to-end; adapter contracts proven; stub AI generation |
| **Phase 2 — Growth** | TikTok adapter; Instagram Reels; first real AI generation adapter; re-prompt loop |
| **Phase 3 — Expansion** | Scheduling; AI-suggested prompts; bulk generation; engagement analytics; cross-channel repurposing |

### Risk Mitigation Strategy

**Technical risks:**
- YouTube API auth + upload is the highest-risk piece. Mitigated by implementing and end-to-end testing the full cycle (upload → publish → history recorded) before MVP is considered done.
- OAuth token storage (encrypted MongoDB fields) is second. Mitigated by proving token refresh works before the YouTube adapter is complete.

**Resource risks:** Single developer; MVP scoped to one platform. The adapter pattern is proven on YouTube before any second platform is attempted.

## Functional Requirements

### Channel Management

- FR1: Operator can create a channel with a name, description, and style/tone context
- FR2: Operator can edit a channel's name, description, and style/tone context
- FR3: Operator can delete a channel
- FR4: Operator can view a list of all channels with last post date and most recent title
- FR5: Operator can navigate into a channel to view its full details and post history

### Platform Account Linking

- FR6: Operator can link a YouTube account to a channel via OAuth
- FR7: Operator can unlink a platform account from a channel
- FR8: Operator can view which platform accounts are currently linked to a channel
- FR9: The system refreshes an expired OAuth access token using a stored refresh token
- FR10: Operator can re-authenticate a platform account when its refresh token is revoked or expired

### Post Cycle — Video & Metadata

- FR11: Operator can start a new post cycle from within a channel
- FR12: Operator can upload a video file as the source for a post cycle
- FR13: Operator can preview the uploaded video before submitting metadata
- FR14: Operator can enter or edit a title for the post
- FR15: Operator can enter or edit a description for the post
- FR16: Operator can enter or edit hashtags for the post
- FR17: The system pre-populates suggested title, description, and hashtag values derived from the channel's post history

### AI Generation (Interface & Stub)

- FR18: Operator can initiate an AI video generation request for a channel
- FR19: The system passes channel history and style context to the video generation adapter as structured input
- FR20: Operator can provide an additional free-text prompt when initiating generation

### Posting & Distribution

- FR21: Operator can submit the reviewed post to all linked platform accounts simultaneously
- FR22: The system posts the video and metadata to each linked platform independently
- FR23: Operator can view real-time posting status per platform (pending / uploading / published / failed)
- FR24: The system continues posting to remaining platforms if one platform fails
- FR25: Operator can retry a failed platform post without restarting the cycle or re-uploading the video
- FR26: Operator can view the published video link for each successfully posted platform

### Post History

- FR27: The system records a post record at the channel level for each completed post cycle
- FR28: Each post record captures title, description, hashtags, video reference, and timestamp
- FR29: Each post record captures a per-platform result sub-list (platform name, status, platform post ID or error message)
- FR30: Operator can view the full post history for a channel ordered by date
- FR31: Operator can view the per-platform result detail for any individual post record
- FR32: The system makes channel post history available as structured context input to the video generation adapter

### Adapter Contracts

- FR33: The system exposes an `IVideoGenerationAdapter` interface that accepts channel context and returns a video reference and suggested metadata
- FR34: The system exposes an `ISocialMediaAdapter` interface that accepts a video and metadata and returns a post result with a platform post ID
- FR35: The YouTube `ISocialMediaAdapter` implementation posts video to YouTube and returns the published video URL and YouTube video ID
- FR36: A new `ISocialMediaAdapter` can be registered without changes to Channel, post cycle, or history logic
- FR37: A new `IVideoGenerationAdapter` can be registered without changes to Channel, post cycle, or history logic

## Non-Functional Requirements

### Performance

- NFR1: Channel dashboard loads within 1 second under normal conditions
- NFR2: Post history for a channel loads within 2 seconds (paginated, 20 records per page)
- NFR3: Per-platform posting status updates are reflected in the UI within 3 seconds of the platform responding
- NFR4: Video upload progress is visible to the operator during upload
- NFR5: Status polling does not exceed one request per 3 seconds per active post cycle

### Security

- NFR6: All OAuth refresh tokens are encrypted at rest using AES-256 before storage in MongoDB
- NFR7: OAuth access tokens are never persisted — only refresh tokens are stored; access tokens are fetched on-demand at time of use
- NFR8: All API communication between frontend and backend occurs over HTTPS
- NFR9: All communication between the backend and external platform APIs occurs over HTTPS
- NFR10: Temporary video files are deleted from the server within 1 hour of post cycle completion or failure

### Reliability

- NFR11: A failure posting to one platform does not affect or cancel posting to other platforms in the same cycle
- NFR12: Post history records are written durably to MongoDB before the post cycle result is returned to the frontend
- NFR13: If the posting polling timeout is reached (10 minutes), timed-out platforms are recorded as failed in post history with a timeout error

### Integration

- NFR14: YouTube API errors are captured per post cycle and surfaced to the operator with a human-readable message (not a raw API error code)
- NFR15: OAuth token refresh failures are distinguished from posting failures and trigger a re-authentication prompt rather than a generic error
- NFR16: YouTube API rate limit responses are treated as retryable failures — the platform result is marked failed with a retry action available
