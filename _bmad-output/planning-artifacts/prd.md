---
stepsCompleted: ['step-01-init', 'step-02-discovery', 'step-02b-vision', 'step-02c-executive-summary', 'step-03-success', 'step-04-journeys', 'step-05-domain', 'step-06-innovation', 'step-07-project-type', 'step-08-scoping', 'step-09-functional', 'step-10-nonfunctional', 'step-11-polish', 'step-12-complete']
classification:
  projectType: web_app
  domain: fintech
  complexity: high
  projectContext: brownfield
inputDocuments:
  - "_bmad-output/brainstorming/brainstorming-session-2026-02-21.md"
  - "docs/index.md"
  - "docs/architecture.md"
  - "docs/integration-architecture.md"
  - "docs/api-contracts-backend.md"
  - "docs/data-models-backend.md"
documentCounts:
  briefs: 0
  research: 0
  brainstorming: 1
  projectDocs: 8
workflowType: 'prd'
---

# Product Requirements Document — sports-ui Fan Store

**Author:** Kampe
**Date:** 2026-02-28

## Executive Summary

The sports-ui Fan Store is a micro-transaction system added to the existing sports-ui fan engagement platform, enabling fans to purchase vote credits — and eventually cosmetic/tradeable virtual items — using real money. The store addresses a structural limitation in the current platform: vote distribution is entirely GM-controlled, creating an engagement ceiling for fans who want to do more. The store removes that ceiling — fans who are most invested can deepen their participation on their own terms.

The initial release is scoped to vote bundle purchases only via Stripe. All items are virtual/digital; physical fulfillment is permanently out of scope. The payment layer is architected for multi-provider support (PayPal, Apple Pay, Google Pay) from day one via an `IPaymentProvider` abstraction — Stripe is the sole implementation at launch. The store is per-organization scoped, consistent with the existing platform architecture.

The data model must be extensible to support future item types — cosmetics (skins, profile items) and eventually a peer-to-peer trading layer — without requiring structural rework.

### What Makes This Special

Most in-app purchase systems sell vanity. Every vote credit purchased on sports-ui connects directly to a real franchise outcome — actual GM roster decisions. Fans aren't buying points; they're buying a voice in decisions that matter to them. That emotional hook, unique to this platform's design, differentiates this store from generic micro-transaction implementations.

The complementary free vote-earning loop (daily check-ins, trivia, GM polls) ensures the store enhances engagement for power users without creating pay-to-win pressure on casual fans.

## Project Classification

| Property | Value |
|---|---|
| **Project Type** | Web App — Angular 20 SPA + .NET 8 API (brownfield addition) |
| **Domain** | Fintech — multi-provider payment processing, PCI DSS territory |
| **Complexity** | High — provider abstraction layer, compliance considerations, extensible item catalog, existing architecture constraints |
| **Project Context** | Brownfield — partial Purchase domain model already exists (`PaymentProvider`, `PurchaseStatus`, `StripePaymentIntentId`, `PurchaseItem`) |

## Success Criteria

### User Success

- Fan completes a vote bundle purchase in under 60 seconds from store open to votes credited
- Purchased votes appear in the fan's VoteAccount immediately upon payment confirmation — no manual refresh required
- Purchase confirmation clearly surfaces which organization the votes apply to
- Failed or cancelled payments surface a clear, non-technical error message with a retry path
- Purchase history is accessible so fans can track what they've spent

### Business Success

- Stripe payment flow operates with zero critical failures at launch (no unrecoverable failed charges, no double-billing)
- The store is live and transacting within the first sprint cycle after implementation
- Adding PayPal, Apple Pay, or Google Pay requires only a new `IPaymentProvider` implementation — no changes to core purchase domain logic
- The item catalog supports new virtual item types without requiring schema migrations to the store's core tables

### Technical Success

- `IPaymentProvider` isolates all payment provider code — swapping or adding a provider requires only a new concrete implementation
- No raw card data touches the server — PCI DSS scope minimized via Stripe Elements
- Webhook handling is idempotent — duplicate Stripe events do not result in duplicate vote credits
- All purchase state transitions (Pending → Completed / Failed) are durable and recoverable
- Existing VoteAccount/VoteTransaction domain remains unchanged — store credits votes through the existing `reward-for-user` pathway

### Measurable Outcomes

- 0 double-charge incidents at launch
- Vote credit latency < 5 seconds post-payment confirmation
- Provider swap (add PayPal/Apple Pay) achievable with no domain layer changes

## Product Scope

### MVP Strategy

**Approach:** Revenue MVP — minimum production-grade payment flow to open a real revenue channel and validate fan willingness to pay for votes. Narrow in scope, not narrow in quality.

**Phase 1 — MVP Feature Set:**

| Capability | Notes |
|---|---|
| Vote bundle catalog (per org) | Predefined tiers (e.g. 10 / 50 / 100 votes) — platform-configured |
| Stripe Elements checkout | Embedded card form, no redirect off-site |
| `IPaymentProvider` abstraction | Stripe as sole implementation — interface required from day one |
| Purchase record (Pending → Completed / Failed) | Durable state with full audit fields |
| Stripe webhook handler | Signature-verified, idempotent, keyed on `stripe_event_id` |
| Instant VoteAccount credit | Via existing `reward-for-user` endpoint on payment success |
| Payment confirmation screen | Org-scoped, vote count displayed, clear success messaging |
| Failed payment error + retry | Human-readable error, no-charge message, retry CTA |
| Admin purchase view (`sports-admin`) | Filterable by status, shows provider ID, user, org, amount |
| `/:organizationId/store` route | Lazy-loaded, auth-guarded, fits existing shell layout |

**Phase 2 — Growth (Post-MVP):**

| Feature | Notes |
|---|---|
| PayPal integration | Second `IPaymentProvider` implementation — no domain changes |
| Apple Pay integration | Stripe Payment Request Button — minimal additional work |
| Google Pay integration | Same as Apple Pay via Stripe |
| Additional bundle tiers | Config change, no code changes |
| Purchase history in fan profile | UI addition using existing purchase records |
| Promotional / discount pricing | Stripe coupon integration |

**Phase 3 — Expansion (Vision):**

| Feature | Notes |
|---|---|
| Cosmetic item catalog | New `StoreItem` type, extends existing product model |
| Skins / profile badges | Virtual cosmetics, org-themed |
| Peer-to-peer item trading | Marketplace layer — significant new domain work |
| GM-created limited item drops | GM app feature, scoped per org |
| GM store analytics view | Revenue, volume, top purchasers per org |

### Risk Mitigation

| Risk | Mitigation |
|---|---|
| Webhook reliability | Idempotent handlers + admin dashboard surfaces stuck Pending purchases |
| Market adoption | Fans already engage deeply with free votes — store removes ceiling, not adds new behaviour |
| Scope reduction | Admin purchase view deferrable to Phase 2; Stripe dashboard serves as manual fallback |

## User Journeys

### Journey 1: The War Room Junkie — First Purchase (Fan Happy Path)

**Marcus** is a 28-year-old die-hard fan who opens the app every morning before work. He's burned through his free votes from the GM's weekly allocation and there's a player option he *needs* to influence — his team is this close to pulling off a trade he's been hoping for all season.

- **Opening Scene:** Marcus opens the fan app, navigates to the player option, and sees he has 0 votes remaining. A "Get More Votes" prompt surfaces below the cast button.
- **Rising Action:** He taps through to the store — a clean screen scoped to his org showing three vote bundle tiers (10, 50, 100 votes). He picks 50 votes, taps "Buy," and Stripe Elements loads inline. He enters his card details without leaving the page.
- **Climax:** Payment confirms in seconds. He sees a clear confirmation: *"50 votes added to [Org Name]. Ready to use."* His VoteAccount balance updates immediately — no refresh.
- **Resolution:** Marcus returns to the player option, casts his 50 votes, and feels the direct connection between his money and his team's future. He screenshots the confirmation and sends it to his group chat.

**Capabilities revealed:** Store page (per-org), bundle catalog, Stripe Elements checkout, payment confirmation screen, instant VoteAccount credit.

---

### Journey 2: The Weekend Warrior — Failed Payment (Fan Edge Case)

**Jess** is a casual fan who decides to try the store for the first time during the playoff push. She doesn't use this kind of app often and isn't sure her card is up to date.

- **Opening Scene:** Jess opens the store, picks the smallest bundle (10 votes), and enters her card number via Stripe Elements. It's an expired card.
- **Rising Action:** Stripe returns a card decline. The app surfaces a clear, human-readable message: *"Payment didn't go through — your card may be expired or declined. No charge was made."* A "Try Again" button is prominent.
- **Climax:** Jess re-enters a valid card. Payment succeeds on the second attempt.
- **Resolution:** She gets the same clean confirmation Marcus saw. Her vote balance updates. No duplicate charge, no confusion.

**Capabilities revealed:** Failed payment error state (clear messaging), retry flow, idempotent webhook handling (no double-credit), no raw card data stored server-side.

---

### Journey 3: The Platform Admin — Failed Payment Investigation (Admin App)

**Kampe** (platform admin) notices a purchase stuck in `Pending` state — a webhook event may have been missed or a payment failed silently.

- **Opening Scene:** Admin opens `sports-admin`, navigates to Purchases, and filters by status = `Pending`. One stuck purchase is visible: user ID, org ID, amount, and `StripePaymentIntentId`.
- **Rising Action:** Admin cross-references the Stripe dashboard using the `StripePaymentIntentId`. Stripe confirms the charge failed.
- **Climax:** Purchase is updated to `Failed` (via webhook or manual admin action). The fan's VoteAccount was never credited — correct behaviour.
- **Resolution:** Admin optionally contacts the fan. Audit trail is clean. No manual vote adjustments needed.

**Capabilities revealed:** Admin purchase list (filterable by status), purchase detail view (provider ID, timestamps, state), purchase state management.

---

### Journey 4: The GM — Passive Observer (Post-MVP)

The GM does not interact with the store in MVP — catalog and pricing are platform-configured. A GM-facing view showing purchase activity per org (volume, revenue, top purchasers) is a Phase 2 growth feature.

---

### Journey Requirements Summary

| Capability | Revealed By |
|---|---|
| Per-org store page with bundle catalog | Journey 1, 2 |
| Stripe Elements checkout (embedded) | Journey 1, 2 |
| Payment confirmation screen | Journey 1, 2 |
| Instant VoteAccount credit on success | Journey 1, 2 |
| Failed payment error state + retry flow | Journey 2 |
| Idempotent webhook handling | Journey 2 |
| Admin purchase list + detail view | Journey 3 |
| Purchase state management (Pending/Completed/Failed) | Journey 3 |
| Stripe dashboard cross-reference (ExternalPaymentId) | Journey 3 |
| GM store analytics view | Journey 4 (Phase 2) |

## Domain-Specific Requirements

### Compliance & Regulatory

- **PCI DSS:** Card data must never touch the server. Stripe Elements keeps sports-ui out of PCI scope — Stripe handles all card data tokenization server-side.
- **Refund Policy:** All sales are final. No refund or chargeback flow is built into the application. Chargebacks are disputed directly through the Stripe dashboard by the platform admin.
- **Currency:** USD only at MVP launch — no multi-currency support required.
- **Tax:** No tax calculation, collection, or reporting required. Digital goods, US-only, no VAT/GST handling.
- **Age / KYC:** No restrictions beyond the existing authenticated session. Fans are already verified via IdentityService auth flow.

### Technical Constraints

- **Webhook security:** All incoming Stripe webhooks must be verified using the `Stripe-Signature` header before processing — unverified events are rejected.
- **Idempotency:** Webhook handlers are idempotent using Stripe's `event.id` — duplicate delivery must not result in duplicate vote credits.
- **No card data storage:** The backend stores only Stripe-issued identifiers (`StripePaymentIntentId`, `StripeSessionId`) — never card numbers, CVVs, or expiry dates.
- **Purchase audit trail:** Every purchase record retains full state history (Pending → Completed / Failed), timestamps, org ID, user ID, and provider reference ID.

### Integration Requirements

- **IPaymentProvider abstraction:** Required from day one. Stripe is the sole concrete implementation at launch; additional providers require only a new implementation class.
- **VoteAccount credit:** On payment success, votes are credited via the existing `POST /api/VoteAccount/reward-for-user` endpoint — no new crediting mechanism.
- **Admin visibility:** Purchase records exposed via `sports-admin` for filtering, inspection, and status tracking.

## Web App Specific Requirements

### Architecture Fit

The Fan Store is a new route added to the existing `sports-ui` Angular 20 SPA — `/:organizationId/store` — following the existing shell/layout pattern. It reuses the existing auth guard, navbar, sidebar, and org-scoping architecture. No new Angular app is created.

- **Rendering:** Angular 20 SPA, zoneless, standalone components — consistent with existing feature libraries
- **Route:** `/:organizationId/store` — lazy-loaded via existing `loadComponent` pattern
- **State:** New `StoreStore` (NgRx Signals) for bundle catalog, active purchase state, and confirmation
- **HTTP:** NSwag-generated client extended with new store/payment endpoints
- **Frontend rule:** The frontend calls only platform API endpoints — it never calls Stripe directly (except loading Stripe.js for Elements)
- **Backend rule:** `IPaymentProvider` abstraction lives in the Application layer — frontend is provider-agnostic

### Browser & Device Support

| Target | Support Level |
|---|---|
| Chrome, Edge, Firefox, Safari (latest 2 versions) | Full support |
| Mobile Safari (iOS 16+) | Full support |
| Chrome for Android | Full support |
| Legacy browsers (IE, pre-2022) | Not supported |

### Responsive Design

- Store page and bundle catalog fully usable at 320px and above
- Stripe Elements is fully responsive — no custom card form layout required
- Bundle tier cards collapse to single-column on mobile
- Purchase confirmation screen minimises scrolling on small viewports

### Accessibility

Best effort — no formal WCAG compliance required. Practical minimums: keyboard-navigable interactive elements, sufficient color contrast on bundle cards and CTA buttons. Stripe Elements meets WCAG 2.1 AA natively.

### Implementation Notes

- Follow existing Nx library structure: `libs/store/feature-store` + `libs/store/store-data-access`
- Stripe.js loaded client-side only — not bundled server-side
- No new Angular Material components required beyond existing `@sports-ui/ui` library

## Functional Requirements

### Store Catalog

- **FR1:** Fan can view the vote bundle catalog scoped to their current organization
- **FR2:** Fan can see the vote quantity and price for each available bundle tier
- **FR3:** Fan can select a bundle tier to purchase
- **FR4:** Platform admin can configure bundle tiers (quantity and price) per organization

### Purchase Flow

- **FR5:** Fan can initiate a purchase for a selected bundle
- **FR6:** Fan can enter payment details via an embedded card form without leaving the store page
- **FR7:** Fan receives a purchase confirmation displaying votes credited and the organization they apply to after a successful payment
- **FR8:** Fan can retry a failed or cancelled payment without restarting the purchase flow from the beginning
- **FR9:** Fan is shown a clear, human-readable error message when a payment fails
- **FR10:** Fan is explicitly informed no charge was made when a payment fails or is cancelled

### Payment Processing

- **FR11:** The system processes payments through an abstracted payment provider interface
- **FR12:** The system supports Stripe as the payment provider at launch
- **FR13:** The system supports adding new payment providers without modifying the purchase domain or application logic
- **FR14:** Card payment data is handled entirely by the payment provider — no card numbers, CVVs, or expiry dates are stored on the platform
- **FR15:** The system creates a purchase record in Pending state before initiating a payment session

### Vote Crediting

- **FR16:** The system credits the purchased vote quantity to the fan's VoteAccount upon confirmed payment
- **FR17:** Vote credits are applied scoped to the fan's current organization
- **FR18:** Updated vote balance is visible to the fan without requiring a manual page refresh

### Webhook & Event Handling

- **FR19:** The system receives and processes payment provider webhook events to update purchase state
- **FR20:** The system verifies the authenticity of all incoming webhook events before processing them
- **FR21:** The system processes duplicate webhook events without crediting votes more than once
- **FR22:** The system transitions a purchase to Completed state and credits votes when a payment success event is received
- **FR23:** The system transitions a purchase to Failed state when a payment failure or cancellation event is received

### Purchase Records & Audit

- **FR24:** The system maintains a complete audit record for every purchase attempt including user ID, organization ID, amount, payment provider reference, timestamps, and status
- **FR25:** Platform admin can view all purchase records across organizations
- **FR26:** Platform admin can filter purchase records by status (Pending, Completed, Failed)
- **FR27:** Platform admin can view full detail of an individual purchase including payment provider reference ID and state history
- **FR28:** Purchase records are retained permanently and cannot be deleted

### Account & Access

- **FR29:** Only authenticated users can access the store
- **FR30:** The store displays only the bundle catalog for the fan's currently active organization
- **FR31:** All purchases are associated with the fan's user account and active organization

## Non-Functional Requirements

### Performance

- **NFR1:** Store page loads within 2 seconds for authenticated fans on a standard connection
- **NFR2:** Stripe Elements payment form mounts and is interactive within 1 second of the store page loading
- **NFR3:** Vote credits appear in the fan's balance within 5 seconds of payment confirmation
- **NFR4:** The store page and checkout flow are fully functional on mobile viewports (320px and above)

### Security

- **NFR5:** All store API endpoints require a valid JWT Bearer token — unauthenticated requests are rejected with 401
- **NFR6:** All client-server communication occurs over HTTPS
- **NFR7:** Stripe API secret keys and webhook signing secrets are stored as environment secrets — never committed to source control
- **NFR8:** Incoming Stripe webhook events are rejected if the `Stripe-Signature` header cannot be verified
- **NFR9:** No card numbers, CVVs, or expiry dates are stored, logged, or transmitted through the platform's backend at any point
- **NFR10:** Only platform admins can access purchase records in the admin application

### Reliability

- **NFR11:** Webhook handlers are idempotent — processing the same Stripe event ID multiple times produces the same outcome as processing it once
- **NFR12:** A purchase record is created before the payment session is initiated — no payment can occur without a corresponding purchase record
- **NFR13:** Purchase state transitions (Pending → Completed / Failed) are atomic — partial state updates are not possible
- **NFR14:** Purchases stuck in Pending state for more than 24 hours are surfaced in the admin purchase view for manual review

### Integration

- **NFR15:** The Stripe integration uses the official Stripe .NET SDK — no direct HTTP calls to the Stripe API
- **NFR16:** The `IPaymentProvider` interface is the only integration point between the purchase domain and any payment provider — no provider-specific code exists outside of the concrete implementation
- **NFR17:** Vote crediting uses the existing `POST /api/VoteAccount/reward-for-user` endpoint — no new vote-crediting mechanism is introduced
- **NFR18:** The frontend calls only platform API endpoints for purchase operations — it never calls Stripe directly except to load Stripe.js for Elements
