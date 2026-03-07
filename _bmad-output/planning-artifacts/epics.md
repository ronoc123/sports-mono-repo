---
stepsCompleted: [1, 2, 3, 4]
workflowStatus: complete
completedAt: '2026-03-07'
inputDocuments:
  - '_bmad-output/planning-artifacts/prd.md'
  - '_bmad-output/planning-artifacts/architecture.md'
workflowType: 'create-epics-and-stories'
project_name: 'sports-ui'
user_name: 'Kampe'
date: '2026-03-06'
---

# sports-ui - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for sports-ui, decomposing the requirements from the PRD and Architecture documents into implementable stories.

## Requirements Inventory

### Functional Requirements

- FR1: Fan can view trending player options on the dashboard, sorted by vote count descending
- FR2: Fan can navigate from a trending player option card to the full player option detail
- FR3: Fan can view all currently active trivia questions on the dashboard
- FR4: Fan can view the currently active GM poll on the dashboard
- FR5: Fan sees a graceful empty state when no trivia or poll content is active
- FR6: Fan can submit an answer to an active trivia question
- FR7: Fan receives vote credits added to their balance upon submitting a correct trivia answer
- FR8: Fan can view the series label associated with a trivia question
- FR9: Fan can view a read-only answered state for a trivia question they have already answered
- FR10: Fan cannot submit an answer to a trivia question they have already answered
- FR11: Fan can see whether their submitted answer was correct or incorrect after submission
- FR12: Fan can vote on an active GM poll at no vote-token cost
- FR13: Fan can see poll results (vote distribution across options) after casting their vote
- FR14: Fan cannot vote on the same poll more than once
- FR15: GM can create a trivia series with a name/label
- FR16: GM can add multiple trivia questions to a series
- FR17: GM can specify the correct answer for each trivia question
- FR18: GM can configure the vote reward amount awarded for a correct answer per question
- FR19: GM can set a question's status to Active, Pending, or Archived
- FR20: GM can publish a Pending question to Active, making it visible on the fan dashboard
- FR21: GM can archive an Active question, removing it from the fan dashboard immediately
- FR22: GM can view all questions across their series with status indicators and participation counts
- FR23: GM can view archived questions in their trivia management view (historical record)
- FR24: GM can create a poll with a question text and two or more custom answer options
- FR25: GM can publish a poll to the org dashboard
- FR26: GM can close/archive a poll, removing it from the fan dashboard
- FR27: GM can view total participation count and per-option vote breakdown for a poll
- FR28: The system credits votes to a fan's balance when a correct trivia answer is submitted
- FR29: The system prevents duplicate vote credits for the same trivia question per user
- FR30: Fan can view their updated vote balance after earning votes from trivia
- FR31: The system ranks active player options by total vote count for the trending feed
- FR32: The trending feed reflects current vote counts on dashboard load

### NonFunctional Requirements

- NFR1: Dashboard initial load completes within 2 seconds on broadband
- NFR2: Trivia answer submission → vote credit confirmation delivered within 500ms
- NFR3: Poll vote submission → result display delivered within 500ms
- NFR4: GM content management views load within 2 seconds for orgs with up to 500 archived questions
- NFR5: Only GMOnly policy users can create, publish, archive, or modify trivia/polls
- NFR6: Fans can only submit trivia answers and poll votes for orgs they are members of
- NFR7: Vote reward amount is determined and applied server-side; client cannot influence credit value
- NFR8: All trivia and poll mutation endpoints require a valid JWT bearer token
- NFR9: Trivia answer processing is idempotent — same answer twice produces exactly one vote credit event
- NFR10: Poll vote processing is idempotent — duplicate submissions rejected without error
- NFR11: Vote credits use the existing VoteTransaction mechanism — no partial states
- NFR12: Dashboard content state (answered/voted) derived from server state, not client storage
- NFR13: New dashboard queries must not degrade existing endpoint response times
- NFR14: Trivia answer and poll vote tables indexed by orgId/userId/questionId — no full-table scans

### Additional Requirements

- Brownfield extension — no starter template; existing codebase only
- Single EF Core migration: `AddDashboardEngagementFeatures` (6 new tables + indexes + unique constraints)
- New domain aggregates: `TriviaSeries` (root → `TriviaQuestion`), `Poll` (root → `PollOption`)
- Standalone idempotency tables: `TriviaAnswer` (UNIQUE UserId+TriviaQuestionId), `PollVote` (UNIQUE UserId+PollId)
- New Angular Nx libs: `libs/trivia/trivia-data-access`, `libs/trivia/feature-trivia-management`, `libs/poll/poll-data-access`, `libs/poll/feature-poll-management`
- Dashboard lib rework: `libs/dashboard/dashboard-data-access` + `libs/dashboard/feature-dashboard`
- New backend controllers: `DashboardController`, `TriviaController`, `PollController`
- Vote credit integration point: `SubmitTriviaAnswerCommandHandler` → `VoteTransaction.ForRewardCredit()` only
- `TriviaQuestion` status state machine: `Pending → Active → Archived` (no reverse transitions); domain methods `Publish()` / `Archive()`
- `GetDashboardQuery` handler uses inline EF projections (not MediatR sub-dispatch) for <2s NFR
- `SubmitPollVoteCommand` response returns updated option vote counts — no re-fetch needed (FR13)
- `trivia-card.component` must call `VoteAccountStore.load()` after successful answer for balance refresh (FR30)

### FR Coverage Map

- FR1: Epic 1 — Trending feed sorted by vote count
- FR2: Epic 1 — Navigate from trending card to player option detail
- FR3: Epic 3 — Active trivia questions displayed on dashboard
- FR4: Epic 5 — Active poll displayed on dashboard
- FR5: Epic 1/3/5 — Empty states (partial per epic, completed in Epic 5)
- FR6: Epic 3 — Fan submits trivia answer
- FR7: Epic 3 — Vote credits awarded on correct answer
- FR8: Epic 3 — Series label shown on trivia question card
- FR9: Epic 3 — Read-only answered state for already-answered questions
- FR10: Epic 3 — Cannot re-submit an already-answered trivia question
- FR11: Epic 3 — Correct/incorrect feedback displayed after submission
- FR12: Epic 5 — Fan votes on poll at no vote-token cost
- FR13: Epic 5 — Poll results displayed after voting
- FR14: Epic 5 — Cannot vote on same poll twice
- FR15: Epic 2 — GM creates trivia series with name/label
- FR16: Epic 2 — GM adds multiple questions to a series
- FR17: Epic 2 — GM specifies correct answer per question
- FR18: Epic 2 — GM configures vote reward amount per question
- FR19: Epic 2 — GM sets question status (Active/Pending/Archived)
- FR20: Epic 2 — GM publishes Pending question to Active
- FR21: Epic 2 — GM archives Active question (immediate dashboard removal)
- FR22: Epic 2 — GM views all questions with status indicators and participation counts
- FR23: Epic 2 — GM views archived question history
- FR24: Epic 4 — GM creates poll with question text and custom options
- FR25: Epic 4 — GM publishes poll to org dashboard
- FR26: Epic 4 — GM archives/closes poll
- FR27: Epic 4 — GM views total participation and per-option vote breakdown
- FR28: Epic 3 — System credits votes to fan balance on correct answer
- FR29: Epic 3 — System prevents duplicate vote credits per question per user
- FR30: Epic 3 — Fan sees updated vote balance after earning votes from trivia
- FR31: Epic 1 — System ranks active player options by total vote count
- FR32: Epic 1 — Trending feed reflects current vote counts on dashboard load

## Epic List

### Epic 1: Trending Player Options Dashboard
Fan opens the reworked dashboard and sees trending player options ranked by vote count, with navigation to player option detail. Includes the full `AddDashboardEngagementFeatures` EF Core migration (all 6 tables), `DashboardController` + `GetDashboardQuery` scaffolded to return trending options (trivia/poll as null/empty), dashboard lib rework, and `trending-feed.component`.
**FRs covered:** FR1, FR2, FR31, FR32, FR5 (partial)

### Epic 2: GM Trivia Series & Question Management
GM can create trivia series, add multi-question sets, configure vote rewards, manage question status (Pending/Active/Archived), and view participation counts. Full-stack: `TriviaSeries` + `TriviaQuestion` domain entities, repositories, CQRS commands/queries, `TriviaController` GM endpoints, `libs/trivia/*` Angular libs, sports-gm management routes.
**FRs covered:** FR15, FR16, FR17, FR18, FR19, FR20, FR21, FR22, FR23

### Epic 3: Fan Trivia Experience & Vote Earning
Fan sees active trivia questions on the dashboard, submits answers, receives correct/incorrect feedback, earns vote credits, and sees their updated balance. Extends dashboard endpoint to include active trivia. `TriviaAnswer` entity + idempotency + `SubmitTriviaAnswerCommand` (vote credit via `ForRewardCredit`). `trivia-card.component` with answered-state + balance refresh.
**FRs covered:** FR3, FR6, FR7, FR8, FR9, FR10, FR11, FR28, FR29, FR30, FR5 (partial)

### Epic 4: GM Poll Management
GM can create polls with custom options, publish to the org dashboard, view per-option vote breakdowns, and archive polls. Full-stack: `Poll` + `PollOption` domain entities, repositories, CQRS commands/queries, `PollController` GM endpoints, `libs/poll/*` Angular libs, sports-gm poll management routes.
**FRs covered:** FR24, FR25, FR26, FR27

### Epic 5: Fan Poll Experience
Fan sees the active poll on the dashboard, casts a free vote, and immediately sees live results. Dashboard empty states fully complete. Extends dashboard endpoint to include active poll. `PollVote` entity + idempotency + `SubmitPollVoteCommand`. `poll-card.component` with result display. All FR5 empty states finalized.
**FRs covered:** FR4, FR12, FR13, FR14, FR5 (complete)

---

## Epic 1: Trending Player Options Dashboard

Fan opens the reworked dashboard and sees trending player options ranked by vote count, with navigation to player option detail. Establishes the full database schema for all engagement features and the aggregated dashboard endpoint.

### Story 1.1: Dashboard Engagement Database Schema

As a developer building the Dashboard Engagement feature,
I want all six engagement domain tables established in the database via a single EF Core migration,
So that all subsequent stories have the data model they need without incremental schema conflicts.

**Acceptance Criteria:**

**Given** the EF Core migration `AddDashboardEngagementFeatures` is applied
**When** the database is inspected
**Then** the following tables exist: `TriviaSeries`, `TriviaQuestions`, `TriviaAnswers`, `Polls`, `PollOptions`, `PollVotes`
**And** `TriviaAnswers` has a UNIQUE constraint on `(UserId, TriviaQuestionId)`
**And** `PollVotes` has a UNIQUE constraint on `(UserId, PollId)`
**And** `TriviaQuestions` has a composite index on `(TriviaSeriesId, Status)`
**And** `Polls` has an index on `(OrganizationId, Status)`

**Given** the backend solution is compiled after migration
**When** DI is resolved at startup
**Then** all six domain entity classes exist with strongly-typed IDs (`TriviaSeriesId`, `TriviaQuestionId`, `TriviaAnswerId`, `PollId`, `PollOptionId`, `PollVoteId`)
**And** `ITriviaSeriesRepository` and `IPollRepository` are declared in `Domain/Repositories/Repositories.cs`
**And** `DbSet<TriviaSeries>`, `DbSet<TriviaAnswer>`, `DbSet<Poll>`, `DbSet<PollVote>` are added to `SportsDbAppContext` and `IApplicationDbContext`
**And** `TriviaSeriesRepository` and `PollRepository` are registered in `Program.cs`

### Story 1.2: Trending Player Options API Endpoint

As a fan,
I want to call a single dashboard endpoint that returns trending player options ranked by vote count,
So that the dashboard loads efficiently with the most popular options at the top.

**Acceptance Criteria:**

**Given** I am an authenticated fan for org `{orgId}`
**When** I call `GET /api/dashboard/{orgId}`
**Then** I receive a 200 response with a `DashboardResponse` containing a `trendingPlayerOptions` array sorted by `voteCount` descending
**And** each `TrendingPlayerOptionDto` includes `playerOptionId`, `playerName`, `positionLabel`, and `voteCount`
**And** `activeTriviaQuestions` is an empty array
**And** `activePoll` is null

**Given** the org has no active player options
**When** I call `GET /api/dashboard/{orgId}`
**Then** `trendingPlayerOptions` is an empty array (not null)

**Given** I am unauthenticated
**When** I call `GET /api/dashboard/{orgId}`
**Then** I receive 401 Unauthorized

**Given** the dashboard query runs
**When** profiled
**Then** it uses a single database projection query with no N+1 fetches

### Story 1.3: Dashboard Trending Feed UI

As a fan,
I want to see a trending player options feed on the reworked dashboard,
So that I can quickly see which player options are most popular and navigate to their detail page.

**Acceptance Criteria:**

**Given** I am on the dashboard and the API returns trending player options
**When** the page loads
**Then** I see trending player option cards ranked by vote count with player name and vote count displayed
**And** the `DashboardStore` status transitions from `loading` to `success`

**Given** I tap a trending player option card
**When** I navigate
**Then** I am taken to the player option detail page for that option (existing route)

**Given** the API returns empty `trendingPlayerOptions`, empty `activeTriviaQuestions`, and null `activePoll`
**When** the dashboard loads
**Then** I see graceful empty state messaging with no broken layout

**Given** the `DashboardStore` status is `loading`
**When** rendered
**Then** a loading indicator is displayed

**Given** the API call returns an error
**When** rendered
**Then** the dashboard shows an error state matching the `status: 'error'` signal store pattern

---

## Epic 2: GM Trivia Series & Question Management

GM can create trivia series, add multi-question sets, configure vote rewards, manage question status (Pending/Active/Archived), and view participation counts.

### Story 2.1: GM Trivia Series & Question Creation

As a GM,
I want to create a trivia series with a name and add multiple questions with configurable vote rewards and correct answers,
So that I can build a themed trivia content set for my fans.

**Acceptance Criteria:**

**Given** I am an authenticated GM for my org
**When** I `POST /api/trivia/series` with `{ organizationId, seriesName }`
**Then** a new `TriviaSeries` is created and I receive a 201 response with `seriesId` and `seriesName`

**Given** I have an existing trivia series
**When** I `POST /api/trivia/series/{seriesId}/questions` with `{ organizationId, questionText, correctAnswer, voteRewardAmount }`
**Then** a new `TriviaQuestion` is added to the series with `Status=Pending` and I receive a 201 response with `questionId`

**Given** I submit a create series request with a missing `seriesName`
**When** FluentValidation runs
**Then** I receive a 400 with a descriptive validation error

**Given** I am not authenticated with the GMOnly policy
**When** I attempt to create a series or add a question
**Then** I receive 403 Forbidden

**Given** I am a GM and navigate to `/trivia-management` in the sports-gm app
**When** the page loads
**Then** I see a list of my org's trivia series and a "Create Series" button

**Given** I complete the create series form and submit
**When** the API responds successfully
**Then** the new series appears in the list without a full page reload

**Given** I click "Add Question" on a series and fill in question text, correct answer, and vote reward amount
**When** I submit
**Then** the question appears in the series question list with status badge "Pending"

### Story 2.2: GM Trivia Question Status Management

As a GM,
I want to publish pending trivia questions to make them visible on the fan dashboard and archive active questions to remove them immediately,
So that I can control which content fans see and rotate my trivia series over time.

**Acceptance Criteria:**

**Given** a `TriviaQuestion` with `Status=Pending`
**When** I `POST /api/trivia/questions/{questionId}/publish`
**Then** the question's `Status` becomes `Active`
**And** the question is now eligible to appear in fan dashboard queries

**Given** a `TriviaQuestion` with `Status=Active`
**When** I `POST /api/trivia/questions/{questionId}/archive`
**Then** the question's `Status` becomes `Archived`
**And** the question no longer appears in fan dashboard queries (effective immediately on next load)

**Given** a `TriviaQuestion` with `Status=Archived`
**When** I attempt to publish or re-activate it
**Then** I receive a 400 — `Archived` is a terminal state with no reverse transitions

**Given** a `TriviaQuestion` with `Status=Pending`
**When** I attempt to archive it directly
**Then** the operation succeeds — `Pending → Archived` is a valid transition

**Given** I am not a GM
**When** I attempt publish or archive actions
**Then** I receive 403 Forbidden

**Given** I am on the trivia management page and a question has `Status=Pending`
**When** rendered
**Then** I see a "Publish" button and no "Archive" button for that question

**Given** I am on the trivia management page and a question has `Status=Active`
**When** rendered
**Then** I see an "Archive" button and no "Publish" button for that question

**Given** I click "Publish" or "Archive"
**When** the action completes
**Then** the question's status badge updates immediately in the UI without a full page reload

### Story 2.3: GM Trivia Question List with Participation Counts

As a GM,
I want to view all questions across my trivia series with their current status and how many fans have answered each one,
So that I can track engagement and make informed decisions about which content to keep active or retire.

**Acceptance Criteria:**

**Given** I am a GM for org `{orgId}`
**When** I `GET /api/trivia/series?organizationId={orgId}`
**Then** I receive all `TriviaSeries` for my org, each containing their `TriviaQuestions`
**And** each `TriviaQuestion` includes `questionText`, `status`, `voteRewardAmount`, and `participationCount`
**And** `participationCount` equals the count of `TriviaAnswer` rows for that `questionId`
**And** questions with `Status=Archived` are included in the response (historical record)

**Given** a question has been answered by 12 fans
**When** I view the GM trivia list
**Then** `participationCount` shows 12

**Given** a question has `Status=Archived`
**When** I view the GM trivia list
**Then** the archived question is visible with a distinct "Archived" status indicator

**Given** I am not a GM
**When** I call the GM trivia series list endpoint
**Then** I receive 403 Forbidden

**Given** I am on the trivia management page
**When** the list loads
**Then** each question row displays its status badge, participation count, and vote reward amount

---

## Epic 3: Fan Trivia Experience & Vote Earning

Fan sees active trivia questions on the dashboard, submits answers, receives correct/incorrect feedback, earns vote credits, and sees their updated balance.

### Story 3.1: Active Trivia Questions on Dashboard

As a fan,
I want to see all active trivia questions on my dashboard with their series label and my answered state,
So that I know what trivia content is available and whether I've already participated.

**Acceptance Criteria:**

**Given** a GM has published one or more `TriviaQuestion` records with `Status=Active` for my org
**When** I call `GET /api/dashboard/{orgId}`
**Then** `activeTriviaQuestions` contains each active question with `questionId`, `questionText`, `seriesLabel`, `answeredByMe`, and `selectedAnswer`
**And** `answeredByMe` is `false` and `selectedAnswer` is `null` for questions the fan has not yet answered
**And** `answeredByMe` is `true` and `selectedAnswer` reflects their submitted answer for previously answered questions

**Given** there are no active trivia questions for the org
**When** I call `GET /api/dashboard/{orgId}`
**Then** `activeTriviaQuestions` is an empty array

**Given** I am on the dashboard and the API returns active trivia questions
**When** the page loads
**Then** I see trivia cards for each active question displaying the question text and the series label

**Given** I previously answered a question correctly
**When** I return to the dashboard
**Then** that trivia card shows a read-only "Answered ✓" state with my selected answer displayed and no submit button

**Given** I previously answered a question incorrectly
**When** I return to the dashboard
**Then** that trivia card shows a read-only "Answered" state indicating I've already participated

### Story 3.2: Fan Trivia Answer Submission & Vote Earning

As a fan,
I want to submit an answer to an active trivia question and receive vote credits if I'm correct,
So that I can earn votes through engagement and see immediate feedback on my answer.

**Acceptance Criteria:**

**Given** I am an authenticated fan and a trivia question is active and unanswered by me
**When** I `POST /api/trivia/answer` with `{ organizationId, triviaQuestionId, selectedAnswer }`
**Then** a `TriviaAnswer` record is created for `(userId, triviaQuestionId)`
**And** I receive a 200 response with `{ isCorrect, votesEarned, correctAnswer }`

**Given** my submitted answer matches the question's `correctAnswer`
**When** the command handler processes the submission
**Then** `VoteTransaction.ForRewardCredit(question.VoteRewardAmount)` is called on my `VoteAccount`
**And** `votesEarned` in the response equals `question.VoteRewardAmount`
**And** my vote balance is increased by that amount

**Given** my submitted answer does not match the `correctAnswer`
**When** the command handler processes the submission
**Then** no `VoteTransaction` is created
**And** `votesEarned` in the response is `0`
**And** `isCorrect` is `false`

**Given** I have already answered this trivia question
**When** I attempt to `POST /api/trivia/answer` again for the same `triviaQuestionId`
**Then** I receive a 400 — idempotency guard rejects the duplicate
**And** no additional `VoteTransaction` is created

**Given** I am unauthenticated
**When** I attempt to submit a trivia answer
**Then** I receive 401 Unauthorized

**Given** I select an answer and submit on a trivia card
**When** the API responds with `isCorrect: true`
**Then** I see a success toast: "+{N} votes added to your balance"
**And** the trivia card transitions to the read-only answered state showing my answer
**And** my displayed vote balance updates to reflect the earned votes

**Given** the API responds with `isCorrect: false`
**When** rendered
**Then** I see feedback indicating my answer was incorrect and the correct answer is revealed
**And** the trivia card transitions to the read-only answered state

**Given** the API responds with an error
**When** rendered
**Then** the trivia card returns to its submittable state and an error message is shown

---

## Epic 4: GM Poll Management

GM can create polls with custom options, publish to the org dashboard, view per-option vote breakdowns, and archive polls.

### Story 4.1: GM Poll Creation & Publishing

As a GM,
I want to create a poll with a question and two or more custom answer options and publish it to my org's dashboard,
So that fans can voice their opinions on topics I choose.

**Acceptance Criteria:**

**Given** I am an authenticated GM for my org
**When** I `POST /api/poll` with `{ organizationId, questionText, options: [{ optionText }, ...] }`
**Then** a new `Poll` is created with `Status=Active` and its `PollOptions`, and I receive a 201 response with `pollId` and the option IDs
**And** the poll requires at least 2 options — a request with fewer than 2 options returns a 400 validation error

**Given** I have created a poll
**When** I `POST /api/poll/{pollId}/publish`
**Then** the poll's `Status` becomes `Active` and it is eligible to appear on the fan dashboard

**Given** I am not authenticated with the GMOnly policy
**When** I attempt to create or publish a poll
**Then** I receive 403 Forbidden

**Given** I submit a create poll request missing `questionText`
**When** FluentValidation runs
**Then** I receive a 400 with a descriptive validation error

**Given** I am a GM and navigate to `/poll-management` in the sports-gm app
**When** the page loads
**Then** I see a list of my org's polls and a "Create Poll" button

**Given** I complete the create poll form with a question and at least 2 options and submit
**When** the API responds successfully
**Then** the new poll appears in the list with status "Active"

### Story 4.2: GM Poll Archive & Results View

As a GM,
I want to archive a poll to remove it from the fan dashboard and view total participation with a per-option vote breakdown,
So that I can manage my content lifecycle and understand how fans responded.

**Acceptance Criteria:**

**Given** a `Poll` with `Status=Active`
**When** I `POST /api/poll/{pollId}/archive`
**Then** the poll's `Status` becomes `Archived`
**And** the poll no longer appears on the fan dashboard (effective immediately on next load)

**Given** I am not a GM
**When** I attempt to archive a poll
**Then** I receive 403 Forbidden

**Given** I am a GM for org `{orgId}`
**When** I `GET /api/poll?organizationId={orgId}`
**Then** I receive all polls for my org including `Status=Archived` polls
**And** each `PollResponse` includes `questionText`, `status`, `totalVotes`, and an `options` array
**And** each option includes `optionText` and `voteCount` (count of `PollVote` rows for that `pollOptionId`)

**Given** a poll has received 30 total votes (20 for option A, 10 for option B)
**When** I view the GM poll list
**Then** `totalVotes` shows 30, option A shows `voteCount: 20`, option B shows `voteCount: 10`

**Given** I am on the poll management page
**When** the list loads
**Then** each poll row shows its status, total vote count, and per-option breakdown
**And** active polls show an "Archive" button; archived polls show no action buttons

**Given** I click "Archive" on an active poll
**When** the action completes
**Then** the poll's status updates to "Archived" immediately in the UI without a full page reload

---

## Epic 5: Fan Poll Experience

Fan sees the active poll on the dashboard, casts a free vote, and immediately sees live results. Dashboard empty states fully complete.

### Story 5.1: Active Poll on Dashboard

As a fan,
I want to see the active GM poll on my dashboard with options I can vote on,
So that I can voice my opinion without spending any vote tokens.

**Acceptance Criteria:**

**Given** a GM has published an active `Poll` for my org
**When** I call `GET /api/dashboard/{orgId}`
**Then** `activePoll` contains `pollId`, `questionText`, an `options` array with `pollOptionId` and `optionText` per option, `votedByMe`, and `selectedOptionId`
**And** `votedByMe` is `false` and `selectedOptionId` is `null` if I have not yet voted
**And** `votedByMe` is `true` and `selectedOptionId` reflects my submitted vote if I have already voted

**Given** there is no active poll for the org
**When** I call `GET /api/dashboard/{orgId}`
**Then** `activePoll` is `null`

**Given** I am on the dashboard and `activePoll` is not null
**When** the page loads
**Then** I see a poll card displaying the poll question and each voting option as a selectable button

**Given** I have already voted on the poll
**When** I return to the dashboard
**Then** the poll card shows a read-only result view with vote distribution across options and my selected option highlighted

**Given** the dashboard has no trending options, no active trivia, and no active poll
**When** the page loads
**Then** all three sections display graceful empty states with appropriate messaging

### Story 5.2: Fan Poll Vote Submission & Result Display

As a fan,
I want to cast a free vote on the active poll and immediately see the live vote distribution,
So that I can participate in my franchise's conversations and see how others voted.

**Acceptance Criteria:**

**Given** I am an authenticated fan and an active poll has not been voted on by me
**When** I `POST /api/poll/vote` with `{ organizationId, pollId, pollOptionId }`
**Then** a `PollVote` record is created for `(userId, pollId, pollOptionId)`
**And** I receive a 200 response with `{ votedOptionId, options: [{ pollOptionId, optionText, voteCount }] }`
**And** no `VoteTransaction` is created — poll voting is free

**Given** I attempt to vote on the same poll a second time
**When** I `POST /api/poll/vote` again with the same `pollId`
**Then** I receive a 400 — idempotency guard rejects the duplicate
**And** no additional `PollVote` is created

**Given** I am unauthenticated
**When** I attempt to submit a poll vote
**Then** I receive 401 Unauthorized

**Given** I tap a poll option and submit
**When** the API responds successfully
**Then** the poll card transitions from voting mode to results mode immediately
**And** I see each option's vote count and percentage distribution
**And** my selected option is visually highlighted

**Given** I return to the dashboard after having voted
**When** the dashboard loads
**Then** the poll card renders directly in results mode (derived from server state `votedByMe: true`)

**Given** the API responds with an error
**When** rendered
**Then** the poll card returns to its voting state and an error message is shown
