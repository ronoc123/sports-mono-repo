---
stepsCompleted: ['step-01-validate-prerequisites', 'step-02-design-epics', 'step-03-create-stories', 'step-04-final-validation']
status: 'complete'
completedAt: '2026-08-19'
inputDocuments:
  - '_bmad-output/planning-artifacts/prd-social-media-ai.md'
  - '_bmad-output/planning-artifacts/architecture-social-media-ai.md'
---

# Social Media AI Video Tool - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for the Social Media AI Video Tool, decomposing the requirements from the PRD and Architecture into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: Operator can create a channel with a name, description, and style/tone context
FR2: Operator can edit a channel's name, description, and style/tone context
FR3: Operator can delete a channel
FR4: Operator can view a list of all channels with last post date and most recent title
FR5: Operator can navigate into a channel to view its full details and post history
FR6: Operator can link a YouTube account to a channel via OAuth
FR7: Operator can unlink a platform account from a channel
FR8: Operator can view which platform accounts are currently linked to a channel
FR9: The system refreshes an expired OAuth access token using a stored refresh token
FR10: Operator can re-authenticate a platform account when its refresh token is revoked or expired
FR11: Operator can start a new post cycle from within a channel
FR12: Operator can upload a video file as the source for a post cycle
FR13: Operator can preview the uploaded video before submitting metadata
FR14: Operator can enter or edit a title for the post
FR15: Operator can enter or edit a description for the post
FR16: Operator can enter or edit hashtags for the post
FR17: The system pre-populates suggested title, description, and hashtag values derived from the channel's post history
FR18: Operator can initiate an AI video generation request for a channel
FR19: The system passes channel history and style context to the video generation adapter as structured input
FR20: Operator can provide an additional free-text prompt when initiating generation
FR21: Operator can submit the reviewed post to all linked platform accounts simultaneously
FR22: The system posts the video and metadata to each linked platform independently
FR23: Operator can view real-time posting status per platform (pending / uploading / published / failed)
FR24: The system continues posting to remaining platforms if one platform fails
FR25: Operator can retry a failed platform post without restarting the cycle or re-uploading the video
FR26: Operator can view the published video link for each successfully posted platform
FR27: The system records a post record at the channel level for each completed post cycle
FR28: Each post record captures title, description, hashtags, video reference, and timestamp
FR29: Each post record captures a per-platform result sub-list (platform name, status, platform post ID or error message)
FR30: Operator can view the full post history for a channel ordered by date
FR31: Operator can view the per-platform result detail for any individual post record
FR32: The system makes channel post history available as structured context input to the video generation adapter
FR33: The system exposes an IVideoGenerationAdapter interface that accepts channel context and returns a video reference and suggested metadata
FR34: The system exposes an ISocialMediaAdapter interface that accepts a video and metadata and returns a post result with a platform post ID
FR35: The YouTube ISocialMediaAdapter implementation posts video to YouTube and returns the published video URL and YouTube video ID
FR36: A new ISocialMediaAdapter can be registered without changes to Channel, post cycle, or history logic
FR37: A new IVideoGenerationAdapter can be registered without changes to Channel, post cycle, or history logic

### NonFunctional Requirements

NFR1: Channel dashboard loads within 1 second under normal conditions
NFR2: Post history for a channel loads within 2 seconds (paginated, 20 records per page)
NFR3: Per-platform posting status updates are reflected in the UI within 3 seconds of the platform responding
NFR4: Video upload progress is visible to the operator during upload
NFR5: Status polling does not exceed one request per 3 seconds per active post cycle
NFR6: All OAuth refresh tokens are encrypted at rest using AES-256 before storage in MongoDB
NFR7: OAuth access tokens are never persisted — only refresh tokens are stored; access tokens are fetched on-demand at time of use
NFR8: All API communication between frontend and backend occurs over HTTPS
NFR9: All communication between the backend and external platform APIs occurs over HTTPS
NFR10: Temporary video files are deleted from the server within 1 hour of post cycle completion or failure
NFR11: A failure posting to one platform does not affect or cancel posting to other platforms in the same cycle
NFR12: Post history records are written durably to MongoDB before the post cycle result is returned to the frontend
NFR13: If the posting polling timeout is reached (10 minutes), timed-out platforms are recorded as failed in post history with a timeout error
NFR14: YouTube API errors are captured per post cycle and surfaced to the operator with a human-readable message (not a raw API error code)
NFR15: OAuth token refresh failures are distinguished from posting failures and trigger a re-authentication prompt rather than a generic error
NFR16: YouTube API rate limit responses are treated as retryable failures — the platform result is marked failed with a retry action available

### Additional Requirements

- No starter template initialization required — SocialMediaAPI service is already scaffolded with Domain, Application, Infrastructure, and WebAPI layers; MongoDB connection infrastructure exists
- MongoDB collections: `channels`, `post_records`, `post_cycle_jobs` (snake_case plural); indexes: `post_records` on `(channelId, createdAt DESC)`, `post_cycle_jobs` on `channelId`
- BSON ObjectId generation in repository `AddAsync` only — never in Domain or Application layers; IDs stored as `string` with `[BsonRepresentation(BsonType.ObjectId)]`
- `IEncryptionService` / `AesEncryptionService` must exist before any LinkedAccount can be persisted; random IV per encryption stored alongside ciphertext; key from `Encryption:Key` app setting
- Fan-out: fire-and-forget `Task.Run` from `StartPostCycleCommandHandler`; `Task.WhenAll` for parallel platform execution; each platform in isolated try/catch
- `ISocialMediaAdapter.Platform { get; }` discriminator required — `PostCycleOrchestrationService` resolves adapter from `IEnumerable<ISocialMediaAdapter>` by matching `linkedAccount.Platform`
- `TempFileCleanupService` (IHostedService, 30-min interval): cleans orphaned temp files older than 1 hour; marks stalled `Running` PostCycleJobs older than 10 minutes as `TimedOut`
- OAuth popup flow: browser popup → `OAuthController /authorize` → Google redirect → `/callback` exchanges auth code → encrypts refresh token → embeds `LinkedAccount` in Channel; frontend polls `getChannel` to detect success
- Angular `PostCycleStore` polls job status every 3 seconds; stops at terminal states (`"Completed"`, `"PartialFailure"`, `"TimedOut"`); enforces 10-minute maximum polling duration
- Video streamed to configurable `TempStorage:Path` (`appsettings.json`); Kestrel `MaxRequestBodySize` = 500MB; upload progress via `HttpClient reportProgress: true` + `observe: 'events'`
- All API responses use `ServiceResponse<T>` envelope: `{ "data": {...}, "success": true, "message": null }`
- `PostCycleJob.Status`: `"Running"` | `"Completed"` | `"PartialFailure"` | `"TimedOut"`; `PlatformJob.Status`: `"Pending"` | `"Uploading"` | `"Published"` | `"Failed"`
- JWT Bearer auth on all endpoints (RSA-256 from IdentityService); exceptions: `GET /api/health`, `GET /api/oauth/callback`
- `IRepository<Channel, string>` base interface sufficient for Channel; `IPostRecordRepository` and `IPostCycleJobRepository` extend base for domain-specific queries
- `Infrastructure/DependencyInjection.cs` is the ONLY registration point for adapters
- Every command requires a FluentValidation validator registered in the pipeline
- `PostRecord` written to MongoDB before `PostCycleJob.Status` transitions to any terminal state
- `PlatformJob.Status` mutable post-completion to support per-platform retry
- Metadata suggestions from most recent 10 `PostRecord` documents: most recent title + description as defaults; top hashtags by frequency

### FR Coverage Map

| FR | Epic | Description |
|---|---|---|
| FR1 | Epic 1 | Create channel |
| FR2 | Epic 1 | Edit channel |
| FR3 | Epic 1 | Delete channel |
| FR4 | Epic 1 | Channel list with last post summary |
| FR5 | Epic 1 | Navigate to channel detail + post history |
| FR6 | Epic 2 | Link YouTube account via OAuth |
| FR7 | Epic 2 | Unlink platform account |
| FR8 | Epic 2 | View linked accounts on channel |
| FR9 | Epic 2 | Auto-refresh expired OAuth access token |
| FR10 | Epic 2 | Re-authenticate revoked/expired refresh token |
| FR11 | Epic 3 | Start new post cycle from channel |
| FR12 | Epic 3 | Upload video file for post cycle |
| FR13 | Epic 3 | Preview uploaded video |
| FR14 | Epic 3 | Edit post title |
| FR15 | Epic 3 | Edit post description |
| FR16 | Epic 3 | Edit post hashtags |
| FR17 | Epic 3 | History-seeded metadata suggestions |
| FR18 | Epic 3 | Initiate AI video generation request |
| FR19 | Epic 3 | System passes channel context to generation adapter |
| FR20 | Epic 3 | Free-text prompt for AI generation |
| FR33 | Epic 3 | IVideoGenerationAdapter interface contract |
| FR37 | Epic 3 | New IVideoGenerationAdapter without core changes |
| FR21 | Epic 4 | Submit post to all linked platform accounts |
| FR22 | Epic 4 | Post to each platform independently |
| FR23 | Epic 4 | Real-time per-platform status view |
| FR24 | Epic 4 | Continue posting if one platform fails |
| FR25 | Epic 4 | Retry failed platform without restarting cycle |
| FR26 | Epic 4 | View published video link per platform |
| FR27 | Epic 4 | Record post record on completed cycle |
| FR28 | Epic 4 | Post record captures title, description, hashtags, video ref, timestamp |
| FR29 | Epic 4 | Post record captures per-platform result sub-list |
| FR34 | Epic 4 | ISocialMediaAdapter interface contract |
| FR35 | Epic 4 | YouTube ISocialMediaAdapter implementation |
| FR36 | Epic 4 | New ISocialMediaAdapter without core changes |
| FR30 | Epic 5 | View full post history for a channel ordered by date |
| FR31 | Epic 5 | View per-platform result detail for any post record |
| FR32 | Epic 5 | Post history available as structured AI context input |

## Epic List

### Epic 1: Channel Management
Users can create, manage, and navigate their channel personas from a unified dashboard. After this epic, the operator has a working channel list and detail view — the foundation for all subsequent workflows.
**FRs covered:** FR1, FR2, FR3, FR4, FR5

### Epic 2: Platform Account Linking
Users can securely link and manage their YouTube accounts per channel via OAuth. After this epic, channels have connected platform credentials stored safely and the system can authenticate with YouTube.
**FRs covered:** FR6, FR7, FR8, FR9, FR10

### Epic 3: Post Creation & AI Generation Interface
Users can start a post cycle, upload a video, and prepare metadata with history-seeded suggestions. The AI generation interface (stub for MVP) is fully defined and accessible from the review step. After this epic, an operator can fully prepare a post — video + metadata — ready to submit.
**FRs covered:** FR11, FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR33, FR37

### Epic 4: Multi-Platform Publishing & Post Recording
Users can publish to all linked platform accounts simultaneously, monitor real-time per-platform status, and retry individual platform failures. Each completed post cycle is durably recorded with per-platform results. After this epic, the core loop is complete end-to-end.
**FRs covered:** FR21, FR22, FR23, FR24, FR25, FR26, FR27, FR28, FR29, FR34, FR35, FR36

### Epic 5: Post History & Context Accumulation
Users can view their complete post history per channel with per-platform detail. The system makes accumulated history available as structured context for future AI generation requests — closing the improvement loop.
**FRs covered:** FR30, FR31, FR32

---

## Epic 1: Channel Management

Users can create, manage, and navigate their channel personas from a unified dashboard. After this epic, the operator has a working channel list and detail view — the foundation for all subsequent workflows.

**FRs covered:** FR1, FR2, FR3, FR4, FR5
**NFRs addressed:** NFR1 (dashboard < 1s)

---

### Story 1.1: Channel List Dashboard

As an operator,
I want to view a dashboard listing all my channels with name, last post date, and most recent post title,
So that I can see all my content personas at a glance and navigate to any channel.

**Acceptance Criteria:**

**Given** I am authenticated and navigate to the social media section
**When** the channel list loads
**Then** I see all channels listed with channel name, description snippet, last post date, and most recent post title
**And** the list loads within 1 second

**Given** I have no channels yet
**When** the channel list loads
**Then** I see an empty state with a prompt to create my first channel

**Given** the API returns an error
**When** the channel list attempts to load
**Then** the store transitions to `status: 'error'` and a user-friendly error message is displayed

---

### Story 1.2: Create Channel

As an operator,
I want to create a new channel by providing a name, description, and style/tone context,
So that I can establish a new content persona ready for account linking and posting.

**Acceptance Criteria:**

**Given** I am on the channel list dashboard
**When** I click "New Channel," complete the form with a valid name, description, and style/tone context, and submit
**Then** the channel is created and I am navigated to the new channel's detail view
**And** the new channel appears in the channel list

**Given** I submit the channel creation form with an empty name
**When** the form is submitted
**Then** a validation error is displayed and the channel is not created

**Given** I submit a valid channel creation form
**When** the API call completes
**Then** the response uses the `ServiceResponse<ChannelDetailResponse>` envelope with `success: true`

---

### Story 1.3: Channel Detail View

As an operator,
I want to navigate into a channel and see its full details including name, description, style/tone context, and linked account summary,
So that I can review the channel's configuration and access its management options.

**Acceptance Criteria:**

**Given** I click on a channel from the dashboard
**When** the channel detail view loads
**Then** I see the channel's name, description, and style/tone context
**And** I see a linked accounts section (empty placeholder if no accounts linked)
**And** I see a post history section (empty placeholder if no posts exist)
**And** "Edit Channel," "Delete Channel," and "New Post" actions are visible

**Given** I navigate to a channel ID that does not exist
**When** the detail view attempts to load
**Then** I see a not-found message and a link back to the channel list

---

### Story 1.4: Edit Channel

As an operator,
I want to edit a channel's name, description, and style/tone context,
So that I can update the channel's persona as my content strategy evolves.

**Acceptance Criteria:**

**Given** I am on a channel's detail view
**When** I click "Edit Channel"
**Then** the edit form opens pre-populated with the channel's current name, description, and style/tone context

**Given** I update one or more fields and save
**When** the form is submitted with valid data
**Then** the channel is updated and I am returned to the channel detail view showing the updated values

**Given** I clear the channel name and attempt to save
**When** the form is submitted
**Then** a validation error is displayed and the channel is not updated

---

### Story 1.5: Delete Channel

As an operator,
I want to delete a channel,
So that I can remove channels I no longer need.

**Acceptance Criteria:**

**Given** I am on a channel's detail view
**When** I click "Delete Channel"
**Then** a confirmation dialog appears asking me to confirm the deletion

**Given** the confirmation dialog is open
**When** I confirm deletion
**Then** the channel is permanently deleted and I am redirected to the channel list
**And** the deleted channel no longer appears in the list

**Given** the confirmation dialog is open
**When** I click Cancel
**Then** the dialog closes and the channel is unchanged

---

## Epic 2: Platform Account Linking

Users can securely link and manage their YouTube accounts per channel via OAuth. After this epic, channels have connected platform credentials stored safely encrypted and the system can authenticate with YouTube.

**FRs covered:** FR6, FR7, FR8, FR9 (auto-refresh — addressed in Epic 4 YouTube adapter), FR10
**NFRs addressed:** NFR6 (AES-256 at rest), NFR7 (access tokens never persisted), NFR15 (re-auth distinguished from posting failure)

---

### Story 2.1: Link YouTube Account via OAuth

As an operator,
I want to link my YouTube account to a channel via OAuth,
So that the channel can publish videos to YouTube on my behalf.

**Acceptance Criteria:**

**Given** I am on the channel detail view with no linked YouTube account
**When** I click "Link YouTube Account"
**Then** a browser popup opens the Google OAuth authorization page

**Given** I complete the Google OAuth authorization in the popup
**When** the popup closes and the backend exchanges the auth code for tokens
**Then** my YouTube account (display name) appears in the channel's linked accounts section
**And** the refresh token is stored encrypted (AES-256) in the channel document — never as plaintext

**Given** I deny the Google OAuth authorization
**When** the popup closes
**Then** no account is linked and the channel detail view shows no change

**Given** the OAuth callback completes successfully
**When** the channel detail view polls for the updated channel
**Then** the new `linkedAccount` entry is visible with platform name and account display name

---

### Story 2.2: Unlink Platform Account

As an operator,
I want to unlink a platform account from a channel,
So that the channel stops publishing to that platform.

**Acceptance Criteria:**

**Given** I am on the channel detail view with a linked YouTube account
**When** I click "Unlink" next to the YouTube account and confirm
**Then** the linked account is removed from the channel
**And** the linked accounts section no longer shows the YouTube account

**Given** I click "Unlink" next to an account
**When** the action is triggered
**Then** a confirmation step is required before the account is removed

**Given** I unlink the last platform account from a channel
**When** the unlink completes
**Then** the channel remains and the linked accounts section shows an empty state

---

### Story 2.3: Re-authenticate Expired Platform Account

As an operator,
I want to re-authenticate a platform account whose refresh token has been revoked or expired,
So that I can restore the channel's ability to publish to that platform without losing my post history.

**Acceptance Criteria:**

**Given** a channel has a linked account whose refresh token is revoked or invalid
**When** I view the channel detail
**Then** the affected linked account is marked with a "Re-authenticate" action

**Given** I click "Re-authenticate" on an invalid linked account
**When** the Google OAuth flow completes in the popup
**Then** the linked account's refresh token is updated with the new encrypted token
**And** the account's status is restored to active

**Given** the re-authentication completes successfully
**When** the channel detail view polls for the updated channel
**Then** the account no longer shows the re-authentication prompt

---

## Epic 3: Post Creation & AI Generation Interface

Users can start a post cycle, upload a video, and prepare metadata with history-seeded suggestions. The AI generation interface (stub for MVP) is fully defined and accessible from the review step. After this epic, an operator can fully prepare a post — video + metadata — ready to publish.

**FRs covered:** FR11, FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR33, FR37
**NFRs addressed:** NFR4 (upload progress visible)

---

### Story 3.1: Start Post Cycle & Video Upload

As an operator,
I want to start a new post cycle from a channel and upload a video file with a visual preview,
So that I have a video ready to review and publish.

**Acceptance Criteria:**

**Given** I am on a channel detail view
**When** I click "New Post"
**Then** the post cycle wizard opens at the video upload step

**Given** I select a video file from my device
**When** the file is selected
**Then** a local video preview is displayed in the browser so I can verify it before continuing

**Given** I select a video file and proceed
**When** I advance to the metadata step
**Then** the selected video file is retained and will be submitted with the final post cycle request

**Given** I select an unsupported file type
**When** the file is selected
**Then** a validation error is displayed indicating only video formats are accepted

---

### Story 3.2: Metadata Review with History Suggestions

As an operator,
I want to review and edit the post title, description, and hashtags — pre-populated with suggestions from my channel's history,
So that I can prepare accurate metadata efficiently without starting from scratch.

_Note: This story creates the `PostRecord` domain entity, `IPostRecordRepository` (read-side only), `GetMetadataSuggestionsQuery`, and `GET /api/post-records/suggestions` endpoint. No post records exist yet at this stage — the suggestions endpoint correctly returns empty results until Epic 4 writes the first records. Story 4.4 adds the write path._

**Acceptance Criteria:**

**Given** I am on the metadata review step of a post cycle
**When** the review screen loads
**Then** I see editable fields for title, description, and hashtags
**And** the fields are pre-populated with suggestions derived from the channel's most recent 10 post records (most recent title/description; top hashtags by frequency)

**Given** the channel has no post history yet
**When** the metadata review step loads
**Then** the fields are empty and I can enter metadata manually with no error

**Given** I modify the title, description, or hashtags
**When** I make changes in the form
**Then** my edits are reflected immediately and are retained for submission

**Given** I submit the review step without entering a title
**When** the form is submitted
**Then** a validation error is displayed and the cycle cannot proceed without a title

---

### Story 3.3: AI Generation Interface (Stub)

As an operator,
I want to invoke AI video generation from within the post cycle review step,
So that the system can produce a new on-brand video and suggest metadata — even if the current implementation uses a stub.

**Acceptance Criteria:**

**Given** I am on the metadata review step
**When** I click "Generate with AI"
**Then** a prompt field appears where I can optionally enter additional generation instructions

**Given** I click "Generate" (with or without a custom prompt)
**When** the generation request is submitted
**Then** the system assembles a `VideoGenerationRequest` containing the channel's style/tone context, post history, and optional prompt, and passes it to the `IVideoGenerationAdapter`

**Given** the stub `IVideoGenerationAdapter` is active
**When** the generation completes
**Then** the stub returns a placeholder video reference and suggested metadata
**And** the metadata fields in the review form are updated with the stub's suggested title, description, and hashtags

**Given** a new `IVideoGenerationAdapter` implementation is registered in DI
**When** generation is invoked
**Then** the new adapter is used with no changes to the Channel, post cycle, or history logic (FR37 proven)

---

## Epic 4: Multi-Platform Publishing & Post Recording

Users can publish to all linked platform accounts simultaneously, monitor real-time per-platform status, and retry individual platform failures. Each completed post cycle is durably recorded with per-platform results. After this epic, the core loop is complete end-to-end.

**FRs covered:** FR21, FR22, FR23, FR24, FR25, FR26, FR27, FR28, FR29, FR34, FR35, FR36
**NFRs addressed:** NFR3, NFR5, NFR6, NFR7, NFR9, NFR10, NFR11, NFR12, NFR13, NFR14, NFR15, NFR16

---

### Story 4.1: Post Cycle Submission & YouTube Publishing

As an operator,
I want to submit my reviewed post and have it published to all linked platform accounts simultaneously,
So that my content reaches YouTube in a single action without manual per-platform steps.

**Acceptance Criteria:**

**Given** I have completed video upload and metadata review
**When** I click "Post to Channel"
**Then** a multipart POST is sent to `POST /api/post-cycles/start` containing the video file and metadata
**And** the backend streams the video to `TempStorage:Path` (not buffered in memory)
**And** a `PostCycleJob` is created with status `"Running"` and the endpoint returns the `jobId` immediately

**Given** the `PostCycleJob` is created
**When** the background fan-out begins
**Then** `PostCycleOrchestrationService` retrieves the channel's linked accounts, decrypts each refresh token (Infrastructure only — never in Application layer), and runs all platform adapters via `Task.WhenAll`

**Given** the `YouTubeSocialMediaAdapter` is called
**When** posting to YouTube
**Then** the adapter exchanges the stored refresh token for a fresh access token and uploads the video via the YouTube Data API v3
**And** on success the `PlatformJob.Status` is updated to `"Published"` with the YouTube video ID and URL

**Given** a new `ISocialMediaAdapter` is registered in DI with a unique `Platform` value
**When** a channel has that platform linked
**Then** the new adapter is resolved and invoked with no changes to Channel, PostCycleOrchestrationService, or history logic (FR36 proven)

---

### Story 4.2: Real-Time Publishing Status & Results

As an operator,
I want to monitor the per-platform publishing status in real time and see the published video link once complete,
So that I know exactly which platforms succeeded and can act on any that did not.

**Acceptance Criteria:**

**Given** a `PostCycleJob` is in `"Running"` status
**When** the posting-status view is active
**Then** the `PostCycleStore` polls `GET /api/post-cycles/{jobId}` every 3 seconds
**And** the view displays per-platform status: `Pending` → `Uploading` → `Published` / `Failed`

**Given** all platforms reach a terminal state
**When** the job status is `"Completed"`, `"PartialFailure"`, or `"TimedOut"`
**Then** polling stops and the final status is displayed

**Given** a platform posts successfully
**When** the status view shows that platform's result
**Then** the published video URL is displayed as a clickable link (FR26)

**Given** the operator has the posting status view open for 10 minutes without terminal state
**When** the maximum polling duration is reached
**Then** polling stops and an appropriate timeout message is shown in the UI

---

### Story 4.3: Platform Failure Isolation & Retry

As an operator,
I want failed platform posts to be isolated from successful ones and to be able to retry a failed platform without restarting the entire cycle,
So that a single platform failure does not undo progress on other platforms.

**Acceptance Criteria:**

**Given** one platform fails during fan-out
**When** the posting-status view shows results
**Then** the failed platform is shown with its error message while successful platforms show their published links
**And** other platforms are unaffected by the failure (FR24)

**Given** a platform's `PlatformJob.Status` is `"Failed"`
**When** I click "Retry" on that platform
**Then** `POST /api/post-cycles/{jobId}/retry/{platform}` is called
**And** only that platform's adapter is re-invoked — the video is not re-uploaded and other platforms are not re-triggered

**Given** an OAuth token refresh fails for a platform
**When** the error is returned
**Then** the platform result displays a "Re-authenticate & Retry" action distinct from a generic posting error (NFR15)

**Given** YouTube returns an API rate limit error
**When** the platform result is displayed
**Then** the platform is marked `"Failed"` with a retry action available (NFR16)

---

### Story 4.4: Post Record Creation & Lifecycle Cleanup

As an operator,
I want every completed post cycle to be durably recorded with full metadata and per-platform results,
So that my post history is always accurate and temporary files are cleaned up automatically.

_Note: `PostRecord` entity and `IPostRecordRepository` were created in Story 3.2 (read schema). This story adds the write path: `PostRecord` creation inside `PostCycleOrchestrationService` and the `TempFileCleanupService` IHostedService._

**Acceptance Criteria:**

**Given** a `PostCycleJob` reaches a terminal state (Completed, PartialFailure, or TimedOut)
**When** the fan-out completes
**Then** a `PostRecord` is written to MongoDB containing title, description, hashtags, video reference, timestamp, and a per-platform result sub-list before the job status transitions to terminal (NFR12)

**Given** a `PostRecord` is written
**When** the per-platform result sub-list is populated
**Then** each entry includes platform name, status, platform post ID (if succeeded), and error message (if failed) (FR29)

**Given** the post cycle completes (success or failure)
**When** the fan-out finally block executes
**Then** the temporary video file at `TempStorage:Path` is deleted

**Given** a `PostCycleJob` remains in `"Running"` status for more than 10 minutes
**When** the `TempFileCleanupService` IHostedService runs (every 30 minutes)
**Then** the job is marked `"TimedOut"` and its orphaned temp file is deleted (NFR10, NFR13)

---

## Epic 5: Post History & Context Accumulation

Users can view their complete post history per channel with per-platform detail. The system makes accumulated history available as structured context for future AI generation requests — closing the improvement loop.

**FRs covered:** FR30, FR31, FR32
**NFRs addressed:** NFR2 (history load < 2s)

---

### Story 5.1: Post History List View

As an operator,
I want to view the complete post history for a channel ordered by date with pagination,
So that I can review what has been published and track my content over time.

**Acceptance Criteria:**

**Given** I am on a channel detail view that has published posts
**When** I navigate to the post history section
**Then** I see a paginated list of post records ordered by date descending, 20 per page
**And** each entry shows the post title, description snippet, timestamp, and a summary of per-platform statuses

**Given** the channel has no post history
**When** I view the post history section
**Then** I see an empty state indicating no posts have been made yet

**Given** the channel has more than 20 post records
**When** I reach the end of the first page
**Then** pagination controls allow me to load the next page
**And** the page loads within 2 seconds (NFR2)

---

### Story 5.2: Post Record Detail & Per-Platform Results

As an operator,
I want to view the full detail of any individual post record including per-platform publishing results,
So that I can see exactly how each post performed across platforms.

**Acceptance Criteria:**

**Given** I am viewing the post history list
**When** I click on a post record
**Then** I see the post's full title, description, hashtags, video reference, and timestamp

**Given** I am on the post record detail view
**When** per-platform results are displayed
**Then** each platform shows its status, the platform post ID (if published), the published video URL as a clickable link (if published), and the error message (if failed)

**Given** a post record has a mix of successful and failed platform results
**When** the detail view loads
**Then** successes and failures are clearly differentiated visually

---

### Story 5.3: Channel History as AI Generation Context

As an operator,
I want the system to automatically include my channel's post history in AI generation requests,
So that generated content is context-aware and improves with each post — without any extra effort on my part.

**Acceptance Criteria:**

**Given** a channel has one or more post records
**When** I invoke AI generation from the post cycle review step
**Then** the `VideoGenerationRequest` payload sent to `IVideoGenerationAdapter` includes the channel's style/tone context and the most recent post history (titles, descriptions, hashtags, timestamps)

**Given** the stub `IVideoGenerationAdapter` receives a `VideoGenerationRequest` with channel history
**When** the stub responds
**Then** the metadata suggestions in the review form reflect that history was passed (e.g., stub can return the most recent title as context confirmation)

**Given** a channel has no post history
**When** AI generation is invoked
**Then** the `VideoGenerationRequest` includes the channel's style/tone context with an empty history array — generation still proceeds without error
