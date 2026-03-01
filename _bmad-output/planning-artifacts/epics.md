---
stepsCompleted: ['step-01-validate-prerequisites', 'step-02-design-epics', 'step-03-create-stories', 'step-04-final-validation']
status: complete
completedAt: 2026-02-28
inputDocuments:
  - "_bmad-output/planning-artifacts/prd.md"
  - "_bmad-output/planning-artifacts/architecture.md"
---

# sports-ui - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for sports-ui Fan Store, decomposing the requirements from the PRD and Architecture into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: Fan can view the vote bundle catalog scoped to their current organization
FR2: Fan can see the vote quantity and price for each available bundle tier
FR3: Fan can select a bundle tier to purchase
FR4: Platform admin can configure bundle tiers (quantity and price) per organization
FR5: Fan can initiate a purchase for a selected bundle
FR6: Fan can enter payment details via an embedded card form without leaving the store page
FR7: Fan receives a purchase confirmation displaying votes credited and the organization they apply to after a successful payment
FR8: Fan can retry a failed or cancelled payment without restarting the purchase flow from the beginning
FR9: Fan is shown a clear, human-readable error message when a payment fails
FR10: Fan is explicitly informed no charge was made when a payment fails or is cancelled
FR11: The system processes payments through an abstracted payment provider interface
FR12: The system supports Stripe as the payment provider at launch
FR13: The system supports adding new payment providers without modifying the purchase domain or application logic
FR14: Card payment data is handled entirely by the payment provider — no card numbers, CVVs, or expiry dates are stored on the platform
FR15: The system creates a purchase record in Pending state before initiating a payment session
FR16: The system credits the purchased vote quantity to the fan's VoteAccount upon confirmed payment
FR17: Vote credits are applied scoped to the fan's current organization
FR18: Updated vote balance is visible to the fan without requiring a manual page refresh
FR19: The system receives and processes payment provider webhook events to update purchase state
FR20: The system verifies the authenticity of all incoming webhook events before processing them
FR21: The system processes duplicate webhook events without crediting votes more than once
FR22: The system transitions a purchase to Completed state and credits votes when a payment success event is received
FR23: The system transitions a purchase to Failed state when a payment failure or cancellation event is received
FR24: The system maintains a complete audit record for every purchase attempt including user ID, organization ID, amount, payment provider reference, timestamps, and status
FR25: Platform admin can view all purchase records across organizations
FR26: Platform admin can filter purchase records by status (Pending, Completed, Failed)
FR27: Platform admin can view full detail of an individual purchase including payment provider reference ID and state history
FR28: Purchase records are retained permanently and cannot be deleted
FR29: Only authenticated users can access the store
FR30: The store displays only the bundle catalog for the fan's currently active organization
FR31: All purchases are associated with the fan's user account and active organization

### NonFunctional Requirements

NFR1: Store page loads within 2 seconds for authenticated fans on a standard connection
NFR2: Stripe Elements payment form mounts and is interactive within 1 second of the store page loading
NFR3: Vote credits appear in the fan's balance within 5 seconds of payment confirmation
NFR4: The store page and checkout flow are fully functional on mobile viewports (320px and above)
NFR5: All store API endpoints require a valid JWT Bearer token — unauthenticated requests are rejected with 401
NFR6: All client-server communication occurs over HTTPS
NFR7: Stripe API secret keys and webhook signing secrets are stored as environment secrets — never committed to source control
NFR8: Incoming Stripe webhook events are rejected if the Stripe-Signature header cannot be verified
NFR9: No card numbers, CVVs, or expiry dates are stored, logged, or transmitted through the platform's backend at any point
NFR10: Only platform admins can access purchase records in the admin application
NFR11: Webhook handlers are idempotent — processing the same Stripe event ID multiple times produces the same outcome as processing it once
NFR12: A purchase record is created before the payment session is initiated — no payment can occur without a corresponding purchase record
NFR13: Purchase state transitions (Pending → Completed / Failed) are atomic — partial state updates are not possible
NFR14: Purchases stuck in Pending state for more than 24 hours are surfaced in the admin purchase view for manual review
NFR15: The Stripe integration uses the official Stripe .NET SDK — no direct HTTP calls to the Stripe API
NFR16: The IPaymentProvider interface is the only integration point between the purchase domain and any payment provider — no provider-specific code exists outside of the concrete implementation
NFR17: Vote crediting uses the existing POST /api/VoteAccount/reward-for-user endpoint — no new vote-crediting mechanism is introduced
NFR18: The frontend calls only platform API endpoints for purchase operations — it never calls Stripe directly except to load Stripe.js for Elements

### Additional Requirements

- **Brownfield — no starter template**: Existing Nx monorepo is the foundation; no project initialization story needed
- **EF Core configurations**: New `ProductConfiguration.cs`, `PurchaseConfiguration.cs`, and `ProcessedWebhookEventConfiguration.cs` must be created to map existing domain entities to the database
- **New EF migration**: One new migration appended after `20260101_rewarditempromocode` creating `Products`, `Purchases`, and `ProcessedWebhookEvents` tables
- **SeedData.cs update**: Add `ProductType.Votes` Product entries (vote bundle tiers) to existing seed data file
- **New aggregate**: `ProcessedWebhookEvent : Aggregate<Guid>` must be created in `Domain/Purchase/` for webhook idempotency
- **IPaymentProvider interface**: `Application/Common/Interfaces/IPaymentProvider.cs` with `CreatePaymentIntentAsync` and `VerifyWebhookSignature` methods
- **StripePaymentProvider**: `Infrastructure/Store/StripePaymentProvider.cs` — only file that imports Stripe.net
- **DI registration**: `Program.cs` must register `IPaymentProvider → StripePaymentProvider` and bind `StripeOptions`
- **NSwag regeneration**: After `StoreController` and `WebhookController` are added, NSwag client must be regenerated for frontend to consume new endpoints
- **New Nx libraries**: `libs/store/data-access` and `libs/store/feature-store` must be scaffolded with `nx g @nx/angular:library`
- **Stripe.js load**: `@stripe/stripe-js` loaded once at app bootstrap; never bundled server-side
- **Responsive layout**: Bundle cards collapse to single-column on mobile (320px+); Stripe Elements is natively responsive
- **Accessibility**: Best effort — keyboard-navigable elements, sufficient color contrast on bundle cards and CTAs

### FR Coverage Map

| FR | Epic | Description |
|---|---|---|
| FR1 | Epic 1 | Fan views bundle catalog scoped to org |
| FR2 | Epic 1 | Fan sees vote quantity and price per tier |
| FR3 | Epic 1 | Fan selects a bundle tier |
| FR4 | Epic 1 | Admin configures bundle tiers (seeded via SeedData.cs MVP) |
| FR5 | Epic 2 | Fan initiates a purchase |
| FR6 | Epic 2 | Fan enters payment via embedded Stripe Elements form |
| FR7 | Epic 2 | Fan receives confirmation with votes credited and org name |
| FR8 | Epic 2 | Fan retries failed payment without restarting |
| FR9 | Epic 2 | Fan shown human-readable error on failure |
| FR10 | Epic 2 | Fan informed no charge was made on failure/cancel |
| FR11 | Epic 2 | System uses abstracted IPaymentProvider interface |
| FR12 | Epic 2 | Stripe is the payment provider at launch |
| FR13 | Epic 2 | New providers addable without domain changes |
| FR14 | Epic 2 | No card data stored on platform |
| FR15 | Epic 2 | Purchase record created (Pending) before payment session |
| FR31 | Epic 2 | Purchases associated with user account and org |
| FR16 | Epic 3 | System credits votes to fan's VoteAccount on payment success |
| FR17 | Epic 3 | Vote credits scoped to fan's current org |
| FR18 | Epic 3 | Updated vote balance visible without page refresh |
| FR19 | Epic 3 | System receives and processes webhook events |
| FR20 | Epic 3 | System verifies webhook authenticity |
| FR21 | Epic 3 | Duplicate webhooks don't double-credit votes |
| FR22 | Epic 3 | Purchase → Completed + votes credited on success event |
| FR23 | Epic 3 | Purchase → Failed on failure/cancellation event |
| FR24 | Epic 4 | Complete audit record for every purchase attempt |
| FR25 | Epic 4 | Admin views all purchase records across orgs |
| FR26 | Epic 4 | Admin filters by status (Pending/Completed/Failed) |
| FR27 | Epic 4 | Admin views full purchase detail including provider reference |
| FR28 | Epic 4 | Purchase records retained permanently |
| FR29 | Epic 1 | Only authenticated users can access store |
| FR30 | Epic 1 | Store shows only active org's catalog |

## Epic List

### Epic 1: Fan Store & Bundle Catalog
Fan can open the store for their organization and browse available vote bundle tiers with prices and quantities clearly displayed.
**FRs covered:** FR1, FR2, FR3, FR4, FR29, FR30

### Epic 2: Checkout & Payment
Fan can purchase a selected bundle using an embedded Stripe card form without leaving the page, and receives confirmation of their purchase.
**FRs covered:** FR5, FR6, FR7, FR8, FR9, FR10, FR11, FR12, FR13, FR14, FR15, FR31

### Epic 3: Vote Crediting & Webhook Processing
The system reliably credits purchased votes to the fan's VoteAccount via webhook-driven processing, handles duplicate events without double-crediting, and transitions purchase state correctly.
**FRs covered:** FR16, FR17, FR18, FR19, FR20, FR21, FR22, FR23

### Epic 4: Admin Purchase Management
Platform admin can view, filter, and inspect all purchase records across organizations to monitor store activity and investigate issues.
**FRs covered:** FR24, FR25, FR26, FR27, FR28

---

## Epic 1: Fan Store & Bundle Catalog

Fan can open the store for their organization and browse available vote bundle tiers with prices and quantities clearly displayed.

### Story 1.1: Vote Bundle Data Model & EF Configuration

As a fan,
I want vote bundle products to be available in the system,
So that I can purchase vote credits for my organization.

**Acceptance Criteria:**

**Given** the EF Core migration is applied
**When** the system starts
**Then** the `Products`, `Purchases`, and `ProcessedWebhookEvents` tables exist in the database

**Given** the seed data is applied
**When** the application starts
**Then** at least three `Product` records with `ProductType.Votes` exist (e.g. 10 votes / $0.99, 50 votes / $3.99, 100 votes / $6.99) with `IsActive = true`

**Given** a `Product` with `IsActive = false` exists in the database
**When** any catalog query is executed
**Then** the inactive product is excluded from all results

**Given** `ProcessedWebhookEvent.Record(stripeEventId, eventType)` is called
**When** the static factory method executes
**Then** a new `ProcessedWebhookEvent` is returned with a new `Guid` Id, the provided `StripeEventId`, `EventType`, and `ProcessedAt = DateTime.UtcNow`

### Story 1.2: Bundle Catalog API Endpoint

As a fan,
I want the platform to return available vote bundle tiers for my organization via API,
So that the store page can display my purchasing options.

**Acceptance Criteria:**

**Given** an authenticated fan with a valid JWT
**When** they call `GET /api/store/bundles?organizationId={orgId}`
**Then** the API returns HTTP 200 with a list of active `ProductDto` (Id, Name, Description, Quantity, PriceAmount, PriceCurrency) ordered by price ascending

**Given** an unauthenticated request
**When** `GET /api/store/bundles` is called without a JWT
**Then** the API returns HTTP 401 Unauthorized

**Given** no active bundles exist for the organization
**When** `GET /api/store/bundles?organizationId={orgId}` is called
**Then** the API returns HTTP 200 with an empty array

**Given** the `GetProductsQueryHandler` executes
**When** querying products
**Then** it uses `IRepository.ListAsync<Product>()` — never `_db.Products` directly

### Story 1.3: Fan Store Shell & Bundle Catalog UI

As a fan,
I want to see the vote bundle catalog for my organization in the app,
So that I can browse available vote packages and select one to purchase.

**Acceptance Criteria:**

**Given** an authenticated fan navigates to `/:orgId/store`
**When** the store page loads
**Then** the page is fully displayed within 2 seconds and shows all active bundle tiers for the organization
**And** each bundle card shows the vote quantity, price, and a "Buy" button

**Given** the fan is not authenticated
**When** they navigate to `/:orgId/store`
**Then** they are redirected to the login page by the route auth guard

**Given** the store page is open on a 320px viewport
**When** the bundle cards are rendered
**Then** cards display in a single-column layout without horizontal overflow

**Given** the API returns zero active bundles
**When** the store page renders
**Then** the page displays an empty-state message indicating no bundles are available

**Given** the NSwag client is regenerated after `StoreController` is added
**When** `store.service.ts` is implemented
**Then** it uses only the NSwag-generated `StoreClient` — no raw `HttpClient` calls

---

## Epic 2: Checkout & Payment

Fan can purchase a selected bundle using an embedded Stripe card form without leaving the page, and receives confirmation of their purchase.

### Story 2.1: Purchase Initiation API & IPaymentProvider

As a fan,
I want the system to create a secure payment session when I choose to buy a bundle,
So that I can enter my card details knowing my purchase is tracked before any charge occurs.

**Acceptance Criteria:**

**Given** an authenticated fan with a valid JWT
**When** they call `POST /api/store/purchases` with a valid `productId` and `organizationId`
**Then** a `Purchase` record is saved to the database with `Status = Pending` before any call to Stripe
**And** the API calls `IPaymentProvider.CreatePaymentIntentAsync` and returns `{ purchaseId, clientSecret }`

**Given** `StripePaymentProvider` implements `IPaymentProvider`
**When** `CreatePaymentIntentAsync` is called
**Then** it uses the official `Stripe.net` SDK — no raw HTTP calls to the Stripe API
**And** the `Stripe.net` import exists only in `Infrastructure/Store/StripePaymentProvider.cs`

**Given** an invalid `productId` is submitted
**When** `POST /api/store/purchases` is called
**Then** the API returns HTTP 400 with a `ProblemDetails` error — no purchase record is created

**Given** an unauthenticated request
**When** `POST /api/store/purchases` is called
**Then** the API returns HTTP 401 Unauthorized

**Given** the `CreatePurchaseCommandHandler` runs
**When** saving the purchase
**Then** it uses `IRepository.AddAsync<Purchase>` and `IRepository.SaveChangesAsync` — never `_db.Purchases` directly

### Story 2.2: Stripe Elements Checkout UI

As a fan,
I want to enter my card details in an embedded form on the store page,
So that I can pay for a vote bundle without being redirected to another site.

**Acceptance Criteria:**

**Given** a fan clicks "Buy" on a bundle card
**When** the checkout overlay opens
**Then** the frontend calls `POST /api/store/purchases` to obtain `clientSecret`
**And** Stripe Elements `CardElement` mounts within 1 second of the overlay opening

**Given** Stripe.js is loaded
**When** it is initialized
**Then** it is loaded via `loadStripe(publishableKey)` from `@stripe/stripe-js` — never bundled server-side
**And** the `publishableKey` comes from the Angular environment config

**Given** the fan has entered valid card details and clicks "Pay"
**When** `stripe.confirmCardPayment(clientSecret)` is called
**Then** the signal store transitions from `confirming` to `success` or `error` based on the result

**Given** the checkout is open on a mobile viewport
**When** Stripe Elements renders
**Then** the card form is fully functional and fits within the viewport without horizontal scroll

### Story 2.3: Payment Confirmation & Error Handling UI

As a fan,
I want to see a clear outcome screen after my payment attempt,
So that I know whether my votes were credited and what to do if something went wrong.

**Acceptance Criteria:**

**Given** `stripe.confirmCardPayment` returns a success result
**When** the signal store transitions to `success`
**Then** the UI displays a confirmation screen showing the vote quantity credited and the organization name

**Given** `stripe.confirmCardPayment` returns an error result
**When** the signal store transitions to `error`
**Then** the UI displays a human-readable error message (e.g. "Your card was declined")
**And** a prominent "Try Again" button is displayed

**Given** a payment fails or is cancelled
**When** the error state is displayed
**Then** the message explicitly states "No charge was made"

**Given** the fan clicks "Try Again"
**When** the retry flow begins
**Then** the checkout overlay reopens with a fresh Stripe Elements form without requiring the fan to re-select their bundle

**Given** the signal store is in `error` state
**When** the error message is displayed
**Then** it originates from `store.errorMessage()` signal — not a local component variable

---

## Epic 3: Vote Crediting & Webhook Processing

The system reliably credits purchased votes to the fan's VoteAccount via webhook-driven processing, handles duplicate events without double-crediting, and transitions purchase state correctly.

### Story 3.1: Stripe Webhook Receiver

As the platform,
I want to receive and authenticate Stripe webhook events at a dedicated endpoint,
So that payment outcomes can be processed reliably and securely.

**Acceptance Criteria:**

**Given** Stripe sends a POST to `/api/store/webhook`
**When** the `Stripe-Signature` header is valid
**Then** the endpoint accepts the request and dispatches a `ProcessWebhookCommand` via MediatR

**Given** a POST arrives at `/api/store/webhook` with an invalid or missing `Stripe-Signature` header
**When** `IPaymentProvider.VerifyWebhookSignature` is called
**Then** the endpoint returns HTTP 400 and does not dispatch any command

**Given** `WebhookController` is defined
**When** it is registered
**Then** it carries `[AllowAnonymous]` and the JWT middleware does not block it
**And** `STRIPE_WEBHOOK_SECRET` is read from environment config — never hardcoded

**Given** the webhook endpoint receives a request
**When** the raw request body is read
**Then** it is read before signature verification and passed as-is to `VerifyWebhookSignature` — never modified

### Story 3.2: Idempotent Purchase Fulfillment

As the platform,
I want payment success webhooks to credit votes exactly once to the correct fan's VoteAccount,
So that fans receive what they paid for without risk of double-crediting on duplicate events.

**Acceptance Criteria:**

**Given** a `payment_intent.succeeded` webhook event arrives
**When** `ProcessWebhookCommandHandler` runs
**Then** it first calls `IRepository.ExistsAsync<ProcessedWebhookEvent>(e => e.StripeEventId == stripeEventId)`
**And** if the event was already processed, the handler returns immediately without crediting votes

**Given** the event has not been processed before
**When** the handler continues
**Then** it calls `Purchase.MarkPaid(externalPaymentId)` → `POST /api/VoteAccount/reward-for-user` → `Purchase.MarkFulfilled()` → `IRepository.AddAsync<ProcessedWebhookEvent>` → `IRepository.SaveChangesAsync` — all in one atomic operation

**Given** `POST /api/VoteAccount/reward-for-user` fails
**When** the handler catches the exception
**Then** no `ProcessedWebhookEvent` is inserted and `SaveChangesAsync` is not called, leaving the purchase in `Pending` so the next webhook retry can attempt again

**Given** the same `stripe_event_id` is delivered twice
**When** the second delivery is processed
**Then** `ExistsAsync` returns true and the handler exits — votes are credited exactly once

**Given** vote crediting is triggered
**When** `reward-for-user` is called
**Then** the `organizationId` from the `Purchase` record is included — credits are org-scoped

### Story 3.3: Payment Failure Webhook & Fan Balance Refresh

As the platform,
I want payment failure webhook events to mark purchases as Failed and the fan's UI to reflect the final outcome,
So that purchase state accurately reflects real payment results and fans see their updated balance.

**Acceptance Criteria:**

**Given** a `payment_intent.payment_failed` event arrives
**When** `ProcessWebhookCommandHandler` handles it
**Then** `Purchase.MarkFailed()` is called and the purchase is saved with `Status = Failed`
**And** a `ProcessedWebhookEvent` is inserted for this event (idempotency applies equally to failures)

**Given** a `payment_intent.payment_failed` event has already been processed
**When** it arrives again
**Then** `ExistsAsync` returns true and the handler exits without changing purchase state again

**Given** a fan's payment succeeds and votes are credited via webhook
**When** the fan views their vote balance
**Then** the updated balance is visible without requiring a manual page refresh
**And** the balance update is driven by the signal store reloading vote account data after detecting `success` state

---

## Epic 4: Admin Purchase Management

Platform admin can view, filter, and inspect all purchase records across organizations to monitor store activity and investigate issues.

### Story 4.1: Purchase History API

As a platform admin,
I want to retrieve all purchase records across organizations with status filtering,
So that I can monitor store activity and identify issues programmatically.

**Acceptance Criteria:**

**Given** an authenticated admin with a valid JWT
**When** they call `GET /api/store/purchases`
**Then** the API returns all purchase records across all organizations as a list of `PurchaseDto` (Id, UserId, OrgId, ProductName, Amount, Currency, Status, StripePaymentIntentId, CreatedAt, PaidAt)

**Given** a status filter is provided
**When** `GET /api/store/purchases?status=Pending` is called
**Then** only purchase records matching that status are returned

**Given** a purchase record exists
**When** any admin calls the endpoint
**Then** the record is always included — purchase records are never deleted or excluded

**Given** the `GetPurchaseHistoryQueryHandler` runs
**When** querying purchases
**Then** it uses `IRepository.Query<Purchase>()` with filters — never `_db.Purchases` directly

**Given** a non-admin authenticated user calls the endpoint
**When** the authorization check runs
**Then** the API returns HTTP 403 Forbidden

### Story 4.2: Admin Purchase List UI

As a platform admin,
I want to view and filter all purchase records in the admin app,
So that I can monitor store health and surface stuck or failed transactions at a glance.

**Acceptance Criteria:**

**Given** a platform admin opens the Purchases section in `sports-admin`
**When** the page loads
**Then** a list of all purchases is displayed showing: fan user ID, org ID, bundle name, amount, status badge, and created timestamp

**Given** the admin selects a status filter (Pending / Completed / Failed)
**When** the filter is applied
**Then** only purchases matching that status are shown

**Given** a purchase has been in `Pending` status for more than 24 hours
**When** it appears in the list
**Then** it is visually highlighted or flagged to draw admin attention

**Given** the admin clears the status filter
**When** the list refreshes
**Then** all purchases across all statuses are shown

### Story 4.3: Admin Purchase Detail View

As a platform admin,
I want to view the full detail of any individual purchase including the Stripe payment reference,
So that I can cross-reference with the Stripe dashboard to investigate payment issues.

**Acceptance Criteria:**

**Given** a platform admin clicks on a purchase in the list
**When** the detail view opens
**Then** it shows the complete purchase record: user ID, org ID, bundle name, vote quantity, amount, currency, `StripePaymentIntentId`, `Status`, `FulfillmentStatus`, `CreatedAt`, and `PaidAt`

**Given** the purchase has a `StripePaymentIntentId`
**When** it is displayed in the detail view
**Then** it is shown as selectable/copyable text so the admin can use it to look up the payment in the Stripe dashboard

**Given** the purchase is in `Failed` state
**When** the detail view renders
**Then** the status is clearly indicated with a Failed badge and the `PaidAt` field shows as null/empty

**Given** an admin navigates to a purchase detail URL directly
**When** the page loads
**Then** the correct purchase data is fetched and displayed — deep linking is supported

