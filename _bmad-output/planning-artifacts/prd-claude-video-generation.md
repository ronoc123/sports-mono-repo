# PRD — Claude-Powered AI Video Generation Feature

**Author:** Kampe
**Date:** 2026-08-29
**Parent PRD:** prd-social-media-ai.md
**Type:** Feature Addition (Brownfield)

---

## Overview

Replaces the stub `IVideoGenerationAdapter` in the Social Media AI Tool with a real Claude-backed implementation. The operator uploads a reference image and provides a prompt against a standardized channel template; Claude receives the image + prompt via the Anthropic API, invokes the **Higgsfield MCP server** (`https://mcp.higgsfield.ai/mcp`) to generate a 10–20 second short-form video, and returns the result. Everything downstream of video generation (review step, metadata editing, multi-platform posting, history recording) is unchanged.

**Resolved decisions:**
- Video generation tool: **Higgsfield AI** via their hosted MCP server
- MCP server: **Higgsfield-hosted** — no self-hosted infrastructure required
- Target video duration: **10–20 seconds** (short-form social content)
- Auth: **Higgsfield account OAuth** (Claude.ai); **Higgsfield API key/token** stored in backend app settings for Anthropic API calls
- Model selection: Claude auto-selects the best Higgsfield model for the task (pool includes Kling 3.0, Veo 3.1, Sora 2, Seedance 2.0, Higgsfield Soul series)

---

## Problem Statement

The existing architecture has a placeholder for AI video generation. The operator currently uploads a pre-recorded video manually. The goal is to replace that manual upload step with a single image + structured prompt that Claude converts into a short, on-brand video via the connected AI video generation tool.

---

## Scope

**In scope:**
- Image upload UI in the post cycle (replaces or supplements manual video upload)
- Standardized prompt template per channel (editable, stored on the channel)
- Claude API integration (Anthropic SDK) in `SocialMediaAPI`
- MCP server integration wired into Claude for the AI video generation tool
- Claude-to-video pipeline: image + rendered prompt → Claude + MCP → video returned
- Polling/status UI while video is generating
- Handoff of generated video into the existing review step

**Out of scope:**
- Changes to the review step, metadata editing, posting, or history recording
- New social platform adapters
- Scheduling or bulk generation

---

## User Journey — AI Video Generation (Happy Path)

**Opening scene:** Kampe opens a channel and clicks "New Post." Instead of uploading a video, he selects "Generate with AI."

**Step 1 — Image upload:** A file picker accepts a single image (JPG/PNG/WebP, ≤ 20MB). Kampe uploads a reference photo — the visual concept he wants the video built around.

**Step 2 — Prompt review:** The channel's standardized prompt template is shown, pre-populated with the channel's style/tone context and recent history signals. Kampe can edit the free-text prompt section or leave the template as-is. He clicks "Generate."

**Step 3 — Generation in progress:** The UI shows a status indicator ("Generating your video..."). The backend sends the image + rendered prompt to Claude, which calls the AI video generation tool via MCP. Polling updates the UI every 3 seconds.

**Step 4 — Video ready:** When Claude returns the video, the post cycle advances automatically to the existing review step — video player on the left, editable metadata on the right. From here the flow is identical to the manual upload path.

**Climax → Resolution:** Kampe reviews, tweaks metadata, and posts. The channel history records this as a generated post with the prompt and image reference stored alongside the post record.

---

## Functional Requirements

### Image Upload
- FR-V1: Operator can upload a single reference image (JPG, PNG, WebP; max 20MB) as input to AI generation
- FR-V2: Operator can preview the uploaded image before submitting the generation request
- FR-V3: Operator can replace the image before submitting

### Standardized Prompt Template
- FR-V4: Each channel stores a reusable prompt template (plain text, up to 2,000 characters)
- FR-V5: The template is pre-populated with channel name, style/tone context, and a free-text section for per-generation instructions
- FR-V6: Operator can edit the rendered prompt inline before each generation request
- FR-V7: Channel admin can edit and save the base template from channel settings
- FR-V8: The system renders the final prompt by combining: channel name + style context + recent post history signals + operator free-text

### Claude Integration
- FR-V9: The backend sends the image (base64-encoded or as a file reference) and rendered prompt to Claude via the Anthropic SDK
- FR-V10: Claude's system prompt instructs it to use the MCP-connected video generation tool to produce a short video from the supplied image and prompt
- FR-V11: The backend passes the Claude response (video URL or file reference returned by the MCP tool) into the post cycle as the video source
- FR-V12: Claude's generation request includes the channel's last N post titles/descriptions as context (consistent with existing channel history structure)

### Higgsfield MCP Integration
- FR-V13: The backend configures Anthropic API calls to connect to the Higgsfield hosted MCP server at `https://mcp.higgsfield.ai/mcp` — no self-hosted MCP infrastructure required
- FR-V14: Higgsfield MCP endpoint and auth token are stored in backend app settings; never hardcoded
- FR-V15: The generation request instructs Claude to produce a **10–20 second** short-form video from the supplied reference image and rendered prompt
- FR-V16: Claude auto-selects the appropriate Higgsfield model (Kling 3.0, Veo 3.1, Sora 2, Seedance 2.0, etc.); operator may optionally specify a preferred model via advanced settings
- FR-V17: Higgsfield returns a video URL; the backend downloads the video to a temp file and passes it into the post cycle as the video source

### Generation Status & Error Handling
- FR-V18: The frontend polls the backend for generation status every 3 seconds while generation is in progress
- FR-V19: The UI displays a generation status indicator with the current state (queued / generating / ready / failed)
- FR-V20: If Claude returns an error or Higgsfield fails, the operator sees a human-readable error and can retry or fall back to manual video upload
- FR-V21: Generation timeout is 5 minutes; after timeout the request is marked failed and the operator is notified

### Post Record
- FR-V22: Post history records for AI-generated videos include: generation method ("higgsfield-claude-mcp"), Higgsfield model used, prompt text, and image filename/reference
- FR-V23: The stored prompt and image reference are viewable in post history detail

---

## Non-Functional Requirements

- NFR-V1: Claude API calls use the Anthropic SDK with retry on transient errors (max 2 retries, exponential backoff)
- NFR-V2: The image uploaded by the operator is not persisted beyond the active post cycle; it is deleted after generation completes or fails
- NFR-V3: The Claude API key is stored in app settings / environment variables; never in source code or the database
- NFR-V4: Higgsfield MCP endpoint URL and auth token are configurable via app settings (no hardcoding)
- NFR-V5: Generation status polling does not exceed one request per 3 seconds
- NFR-V6: If Higgsfield changes their MCP tool API, only the adapter changes — Claude integration layer and frontend are unaffected
- NFR-V7: Backend downloads the Higgsfield video to a local temp file before passing to the post cycle; the temp file is deleted after review step completes or fails

---

## Technical Notes

### Prompt Template Structure (Default)

```
You are creating a short social media video for the channel "{channelName}".

Channel style: {styleToneContext}

Recent post context (last 5 posts):
{recentPostHistory}

Video request: {operatorFreeText}

Using the attached reference image and the Higgsfield video generation tool, create a 10–20 second short-form video that matches this channel's style and the request above. Select the best available Higgsfield model for the content type.
```

### Claude + Higgsfield MCP Flow

```
Frontend → POST /api/generation/start  (multipart: image + promptOverride)
  → Backend renders full prompt (template + channel context + history)
  → Anthropic SDK call:
      messages = [{ role: user, content: [image_block, text_block] }]
      mcp_servers = [{ url: "https://mcp.higgsfield.ai/mcp", auth: bearer_token }]
  → Claude selects Higgsfield model, calls generate_video tool
  → Higgsfield generates 10–20 sec video, returns video URL
  → Claude returns tool_result with video URL to backend
  → Backend downloads video to temp file, marks job as ready
Frontend ← polls GET /api/generation/{jobId}/status
  ← status: ready → frontend advances to existing review step with video
```

### Adapter Implementation

The existing `IVideoGenerationAdapter` interface is implemented as `HiggsFieldClaudeAdapter`:

- Input: `VideoGenerationRequest` (channel context, rendered prompt, image bytes)
- Output: `VideoGenerationResult` (local temp video file path, Higgsfield model used)
- Registered in DI via app settings flag; no changes to post cycle, history, or social posting logic

### Configuration (appsettings)

```json
{
  "VideoGeneration": {
    "Provider": "HiggsFieldClaude",
    "Anthropic": {
      "ApiKey": "<from env>",
      "Model": "claude-opus-4-5"
    },
    "Higgsfield": {
      "McpEndpoint": "https://mcp.higgsfield.ai/mcp",
      "AuthToken": "<from env>",
      "TargetDurationSeconds": 15
    }
  }
}
```

---

## Phasing

| Phase | Scope |
|-------|-------|
| **V1 — MVP** | Image upload, prompt template (per channel), Claude API call, MCP integration, polling status UI, handoff to existing review step |
| **V2 — Growth** | Prompt template versioning, generation history (retry with same image), operator-adjustable video duration target |
| **V3 — Vision** | Auto-suggested prompts from channel history, multi-image input, style reference images per channel |

---

## Resolved Decisions

| # | Decision | Resolution |
|---|----------|------------|
| 1 | AI video generation tool | **Higgsfield AI** — hosted MCP server at `https://mcp.higgsfield.ai/mcp` |
| 2 | Target video duration | **10–20 seconds** short-form |
| 3 | MCP server hosting | **Higgsfield-hosted** — no self-hosted infrastructure; backend connects as a client |
