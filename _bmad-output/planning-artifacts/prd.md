---
stepsCompleted: ['step-01-init', 'step-02-discovery', 'step-02b-vision', 'step-02c-executive-summary', 'step-03-success', 'step-04-journeys', 'step-05-domain', 'step-06-innovation', 'step-07-project-type', 'step-08-scoping', 'step-09-functional', 'step-10-nonfunctional', 'step-11-polish', 'step-12-complete']
workflowStatus: complete
completedDate: '2026-03-06'
inputDocuments:
  - '_bmad-output/brainstorming/brainstorming-session-2026-02-21.md'
  - 'docs/index.md'
  - 'docs/architecture.md'
  - 'docs/integration-architecture.md'
  - 'docs/api-contracts-backend.md'
  - 'docs/data-models-backend.md'
  - 'docs/state-management-frontend.md'
  - 'docs/component-inventory-frontend.md'
  - 'docs/development-guide.md'
documentCounts:
  briefs: 0
  research: 0
  brainstorming: 1
  projectDocs: 8
classification:
  projectType: web_app
  domain: sports_entertainment
  complexity: medium
  projectContext: brownfield
workflowType: 'prd'
---

# Product Requirements Document - sports-ui

**Author:** Kampe
**Date:** 2026-03-06

## Executive Summary

Sports-ui is a fan-driven sports franchise engagement platform where fans earn voting influence and use it to shape real roster decisions — trades, NFL draft picks, and player acquisitions — submitted by their team's GM. The platform operates a virtual vote economy: fans earn tokens through engagement, spend them on player option votes, and redeem rewards. The system is live with auth, GM content pipeline, fan voting, marketplace, and reward redemption in production.

This PRD defines a **dashboard rework** delivering the first phase of the Daily Engagement Loop: a trending player options feed, GM-managed trivia with vote rewards, and GM polls. The target users are franchise fans who want earned influence and daily reasons to return, and GMs who need lightweight content tools to sustain fan engagement between major voting events.

### What Makes This Special

Most sports apps treat fans as observers. Sports-ui treats them as stakeholders — the vote economy gives engagement real weight. The dashboard rework makes this tangible on every visit: fans see what's trending, earn votes by answering trivia, and voice opinions through polls — all without spending a token. The vote balance grows through participation, not waiting for a GM to send tokens.

## Project Classification

| Property | Value |
|---|---|
| **Project Type** | Web Application (Angular 20 SPA) |
| **Domain** | Sports / Fan Engagement |
| **Complexity** | Medium — virtual economy, social mechanics, GM content tooling |
| **Project Context** | Brownfield — active system; this PRD adds new features to the existing dashboard |

## Success Criteria

### User Success

- Fan opens the dashboard and completes the engagement loop (trending feed → trivia → poll) within a single session
- Fan earns votes through trivia without waiting for GM action — the vote economy feels self-sustaining
- Fan receives correct/incorrect feedback on trivia submission and sees their updated balance within 500ms
- Fan sees poll results immediately after voting — the dashboard is a status view on return visits, not just an action view

### Business Success

- **DAU/MAU ratio** reaches ≥40% daily active within 60 days of launch (up from estimated weekly cadence)
- **Vote economy health**: ≥30% of vote tokens in circulation come from trivia-earned sources vs. GM-granted within 90 days
- **GM content adoption**: ≥70% of active GMs create at least one trivia question or poll per week within 30 days of launch

### Technical Success

- Trivia answer submission is idempotent — no double vote credits, no missed credits
- All new dashboard queries execute without degrading existing endpoint performance
- Dashboard content state (answered/voted) is always derived from server state

### Measurable Outcomes

| Outcome | Target | Timeframe |
|---|---|---|
| DAU/MAU ratio | ≥40% | 60 days post-launch |
| Trivia-earned votes share | ≥30% | 90 days post-launch |
| GM trivia/poll creation rate | ≥70% of active GMs weekly | 30 days post-launch |

## User Journeys

### Journey 1: The Fan — Daily Dashboard Loop (Success Path)

**Meet Marcus.** He's a die-hard Eagles fan who checks sports news every morning over coffee. He found sports-ui two weeks ago when his buddy sent him a code. He's voted on a couple of player options but mostly uses the app to check what's happening.

**Opening Scene:** Marcus opens the app and lands on the dashboard. The first thing he sees is the trending player options feed — cards showing the hottest active options ranked by fan vote count. He can immediately feel the pulse of his franchise without navigating anywhere.

**Rising Action:** Below the feed, he spots a trivia question from his GM — part of a series called "2026 Draft Class." The question asks which prospect ran the fastest 40-yard dash at the combine. He takes a guess, submits, and gets it right. A toast confirms: "+3 votes added to your balance." He earned votes without spending anything.

**Climax:** He scrolls further and sees a GM poll: "Who should we target in round 2 — speed or size?" Two options, free to vote. He taps "Speed." No stakes, just voice.

**Resolution:** Marcus spent four minutes on the app and left feeling informed, rewarded, and heard. He'll be back tomorrow to see if the trivia series has a new question.

---

### Journey 2: The Fan — Return Visit / Already Answered (Edge Case)

**Marcus comes back later that evening.** He already answered the trivia and voted on the poll this morning.

**Opening Scene:** The trivia card shows "Answered ✓" with his result — read-only, no submit button. The poll now displays live results: 61% speed, 39% size.

**Resolution:** The dashboard is still useful on re-visit. He checks the trending feed and notices a player option has climbed in votes since this morning. He taps through to vote on it using his balance.

---

### Journey 3: The GM — Creating Trivia & a Poll (Content Creator Path)

**Meet DeShawn.** He's the GM of the Philadelphia Eagles org. He posts player options weekly but engagement between events is low. He wants fans coming back on quiet weeks.

**Opening Scene:** DeShawn opens the GM portal and navigates to dashboard content management. He creates a new series: "2026 Draft Class" and adds three questions with a 3-vote reward each. He publishes Q1 immediately and sets Q2 and Q3 to Pending. He can see all three with status indicators (Active / Pending / Archived).

**Rising Action:** He creates a poll: "Round 2 target — Speed or Size?" Two options. Posts it to the dashboard.

**Resolution:** Within an hour, 40 fans have answered the trivia. DeShawn sees participation counts on each question. He archives Q1 the next day and activates Q2 — the series continues.

---

### Journey 4: The GM — Archiving & Rotating Content

**A week later, DeShawn's draft series is wrapping up.** The "2026 Draft Class" series shows Q1 with 80 answers, Q2 with 55, Q3 with 40.

**Rising Action:** He archives all three questions. The dashboard for fans immediately stops showing them. He creates a new series: "Eagles Season Opener" and drafts two new questions.

**Resolution:** Fans see the fresh content on their next dashboard load. DeShawn's org stays current without friction.

---

### Journey Requirements Summary

| Capability Area | Revealed By |
|---|---|
| Trending player options feed (vote-count sorted) | Journeys 1, 2 |
| Trivia card with answer submission + vote credit | Journey 1 |
| Answered-state trivia card (read-only on return) | Journey 2 |
| Poll display with free voting + result view post-vote | Journeys 1, 2 |
| GM trivia creation: series, multi-question, status management | Journeys 3, 4 |
| Configurable vote reward per trivia question | Journey 3 |
| GM participation counts per question/poll | Journey 3 |
| GM poll creation with custom options | Journey 3 |
| Archive action + immediate dashboard refresh | Journey 4 |
| Archived question history (GM-only) | Journey 4 |

## Web App Technical Considerations

Sports-ui is an Angular 20 SPA — fully authenticated, feature-sliced Nx monorepo, zoneless change detection. All dashboard routes are behind `authGuard`; org-scoped data follows the existing `/:organizationId` pattern. SignalR infrastructure exists (marketplace hub) but is not required for MVP dashboard features — trending feed and poll results fetch on load.

New stores follow the `status: 'idle' | 'loading' | 'success' | 'error'` Signal Store pattern. Fan-facing dashboard views are mobile-primary; GM content management views are desktop-primary. Browser support targets last 2 versions of Chrome, Edge, and Firefox (Safari best-effort). No SEO requirements — all routes are auth-gated.

## Project Scoping & Phased Development

### MVP Strategy

This is a brownfield enhancement to a live product. The MVP delivers enough of the Daily Engagement Loop to shift fan behavior from weekly to daily, and gives GMs the content tools to sustain that loop.

**MVP is complete when:** A fan can open the dashboard, see trending player options, answer an active trivia question and earn votes, and vote on a GM poll — without leaving the dashboard. A GM can create, publish, and archive trivia questions and polls from the GM portal.

### Phase 1 — MVP

| Capability | Justification |
|---|---|
| Dashboard — trending player options feed (vote-count sorted) | Core hook; immediate value on every visit |
| Dashboard — active trivia card with answer submission | Primary vote-earning mechanic |
| Trivia answered-state (read-only on return) | Data integrity; prevents double-credit |
| Vote credit on correct trivia answer | Core incentive |
| Dashboard — GM poll with free fan voting | Secondary engagement; zero friction |
| Poll result display after voting | Closes feedback loop |
| GM portal — trivia series creation and labeling | Content organization |
| GM portal — multi-question creation per series | Content depth |
| GM portal — question status management (Active / Pending / Archived) | Content rotation |
| GM portal — configurable vote reward per question | Economy control |
| GM portal — participation counts per question/poll | GM feedback loop |
| GM portal — poll creation with custom options | Poll posting |

### Phase 2 — Growth

- Daily check-in streak (consecutive daily open multipliers, streak reset)
- Trivia leaderboards / Franchise Scholar (weekly + all-time per org, badges and titles)
- Cross-franchise news feed / League Pulse (browsable feed across all orgs)
- Trivia difficulty tiers with scaled vote rewards
- Bulk archive action for GM trivia management

### Phase 3 — Expansion

- Real-time trivia events (GM-hosted live sessions, time-pressured answers)
- Cross-org fan competitions and seasonal challenges
- Public fan profiles with achievement history and reputation scores

### Risk Mitigation

**Technical:** GM content management UI is net-new in the sports-gm app — no existing multi-item management pattern. Keep MVP UI simple (no drag-and-drop, no rich text). Track trivia answered state server-side per `(userId, questionId)`; never trust client for idempotency.

**Market:** GM content dependency — fans have nothing to engage with if GMs don't create content. Make creation fast (≤3 steps); seed initial orgs with example content at launch.

**Scope:** Streak, leaderboard, and League Pulse features are explicitly Phase 2. PRD scope gates them; resist mid-sprint additions.

## Functional Requirements

### Dashboard

- **FR1:** Fan can view trending player options on the dashboard, sorted by vote count descending
- **FR2:** Fan can navigate from a trending player option card to the full player option detail
- **FR3:** Fan can view all currently active trivia questions on the dashboard
- **FR4:** Fan can view the currently active GM poll on the dashboard
- **FR5:** Fan sees a graceful empty state when no trivia or poll content is active

### Trivia — Fan Experience

- **FR6:** Fan can submit an answer to an active trivia question
- **FR7:** Fan receives vote credits added to their balance upon submitting a correct trivia answer
- **FR8:** Fan can view the series label associated with a trivia question
- **FR9:** Fan can view a read-only answered state for a trivia question they have already answered
- **FR10:** Fan cannot submit an answer to a trivia question they have already answered
- **FR11:** Fan can see whether their submitted answer was correct or incorrect after submission

### Polls — Fan Experience

- **FR12:** Fan can vote on an active GM poll at no vote-token cost
- **FR13:** Fan can see poll results (vote distribution across options) after casting their vote
- **FR14:** Fan cannot vote on the same poll more than once

### GM Trivia Management

- **FR15:** GM can create a trivia series with a name/label
- **FR16:** GM can add multiple trivia questions to a series
- **FR17:** GM can specify the correct answer for each trivia question
- **FR18:** GM can configure the vote reward amount awarded for a correct answer per question
- **FR19:** GM can set a question's status to Active, Pending, or Archived
- **FR20:** GM can publish a Pending question to Active, making it visible on the fan dashboard
- **FR21:** GM can archive an Active question, removing it from the fan dashboard immediately
- **FR22:** GM can view all questions across their series with status indicators and participation counts
- **FR23:** GM can view archived questions in their trivia management view (historical record)

### GM Poll Management

- **FR24:** GM can create a poll with a question text and two or more custom answer options
- **FR25:** GM can publish a poll to the org dashboard
- **FR26:** GM can close/archive a poll, removing it from the fan dashboard
- **FR27:** GM can view total participation count and per-option vote breakdown for a poll

### Vote Economy — Trivia Earning

- **FR28:** The system credits votes to a fan's balance when a correct trivia answer is submitted
- **FR29:** The system prevents duplicate vote credits for the same trivia question per user
- **FR30:** Fan can view their updated vote balance after earning votes from trivia

### Player Options Feed

- **FR31:** The system ranks active player options by total vote count for the trending feed
- **FR32:** The trending feed reflects current vote counts on dashboard load

## Non-Functional Requirements

### Performance

- Dashboard initial load (trending feed + active trivia + active poll) completes within 2 seconds on broadband
- Trivia answer submission → vote credit confirmation delivered within 500ms
- Poll vote submission → result display delivered within 500ms
- GM content management views load within 2 seconds for orgs with up to 500 archived questions

### Security

- Only users with the `GMOnly` policy can create, publish, archive, or modify trivia questions, series, and polls
- Fans can only submit trivia answers and poll votes for orgs they are members of
- Vote reward amount is determined and applied server-side; the client does not pass or influence the credit value
- All trivia and poll mutation endpoints require a valid JWT bearer token

### Reliability

- Trivia answer processing is idempotent — submitting the same answer twice produces exactly one vote credit event
- Poll vote processing is idempotent — duplicate submissions for the same poll by the same user are rejected without error
- Vote credits use the existing `VoteTransaction` mechanism — no partial states; credit fully applies or does not apply
- Dashboard content state (answered/voted) is derived from server state, not client storage

### Scalability

- New dashboard queries must not degrade existing endpoint response times under current user load
- Trivia answer and poll vote tables support per-org growth without full-table scans (indexed by `orgId`, `userId`, `questionId`)
