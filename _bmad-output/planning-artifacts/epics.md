---
stepsCompleted: ['step-01-validate-prerequisites', 'step-02-design-epics', 'step-03-create-stories', 'step-04-validate']
status: 'complete'
completedAt: '2026-03-01'
inputDocuments:
  - "_bmad-output/planning-artifacts/prd.md"
  - "_bmad-output/planning-artifacts/architecture.md"
---

# sports-ui Fan Economy - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for the sports-ui Fan Economy, decomposing the requirements from the PRD and Architecture into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: A fan can view their current points balance displayed under the platform's branded currency name
FR2: A fan's points balance reflects all transactions (purchases, escrow holds, releases, settlements, H2H payouts) without requiring a manual refresh
FR3: The system records every point movement with timestamp, user ID, action type, and associated entity (pack ID, listing ID, match ID)
FR4: Points cannot be converted to real money, gift cards, or any external value
FR5: A GM can create a CardPlayer entry with name, position, league, and overall rating
FR6: A GM can edit an existing CardPlayer's name, position, and overall rating
FR7: The system automatically assigns a rarity tier to a CardPlayer based on configured overall rating thresholds
FR8: CardPlayer entries are scoped to a specific league and are only available in packs for that league
FR9: A GM can view all CardPlayer entries for their league
FR10: Historical player CardPlayer entries exist independently of the Active Roster system and persist across seasons
FR11: A fan can purchase a card pack by spending points
FR12: Each pack purchase produces exactly 5 cards drawn from the fan's league CardPlayer catalog
FR13: Card rarity distribution per pack is governed by configured probability weights per rarity tier
FR14: A fan receives their 5 pulled cards immediately upon successful pack purchase
FR15: Points are deducted from the fan's balance atomically at the moment of pack purchase
FR16: A fan cannot view the configured rarity pull probabilities for any pack type
FR17: A fan can view all cards they currently own
FR18: A fan can view individual card details including player name, overall rating, rarity tier, and league
FR19: A fan can list a card they own for auction with a starting bid price, an optional buy now price, and a chosen auction duration
FR20: A listed card is removed from the seller's usable collection for the duration of the auction
FR21: A fan can browse active auction listings in the marketplace
FR22: A fan can place a bid on an active auction listing
FR23: Points equal to the bid amount are escrowed atomically when a fan places a bid
FR24: A fan's escrowed points are released immediately when they are outbid by another user
FR25: A fan can purchase a listed card immediately at the buy now price, ending the auction
FR26: The winning bidder at auction expiry receives the card; the seller receives the final bid amount in points
FR27: A fan cannot bid on an auction listing they created
FR28: A fan cannot place a bid if their available (non-escrowed) points balance is insufficient to cover the bid
FR29: A fan receives an in-app notification when they have been outbid on an active auction
FR30: A fan viewing an active listing sees the current highest bid updated in real-time without refreshing
FR31: Auction state (current bid, time remaining) is consistent across all clients viewing the same listing simultaneously
FR32: A fan can select up to 5 cards from their collection to form an H2H squad
FR33: The squad builder displays the team's overall rating calculated from the selected cards
FR34: A fan can set a points wager amount before confirming an H2H match
FR35: Wagered points are escrowed when the fan confirms the match
FR36: The system generates a bot opponent team assembled randomly from the real CardPlayer catalog using the same rarity weights available to all users
FR37: The match outcome is determined by weighted probability based on the overall rating differential between the two teams
FR38: The fan experiences a live match resolution sequence before the outcome is revealed
FR39: The winning side's wagered points are transferred to the winner; the loser forfeits their wagered amount
FR40: A fan's points balance is updated immediately upon match resolution
FR41: A GM can view a dashboard showing cards in circulation, marketplace listing volume by card, and average sale price trends
FR42: A GM can identify which cards are currently active on the marketplace and how frequently each card has appeared in pack pulls

### NonFunctional Requirements

NFR-P1: Outbid notifications delivered to affected clients within 500ms of a competing bid being placed, under normal operating load
NFR-P2: Card pull results returned to the fan within 2 seconds of pack purchase confirmation
NFR-P3: Marketplace listing pages reach interactive state within 3 seconds on standard broadband
NFR-P4: H2H match resolution animation runs entirely client-side — no server round-trip after wager is confirmed and match outcome is received
NFR-S1: All point transaction operations (escrow, release, debit, credit) validated and executed server-side; client cannot assert or modify balance or escrow state
NFR-S2: Self-bid prevention enforced server-side — API rejects any bid where bidder's user ID matches listing owner's user ID
NFR-S3: Rarity probability weights are server-side configuration only and not exposed in any API response or client-accessible resource
NFR-S4: All Fan Economy endpoints (packs, marketplace, H2H, admin) require a valid authenticated session
NFR-R1: Zero escrow integrity failures — no auction may settle with a losing bidder's points remaining locked, and no auction may complete without transferring the card and crediting the seller
NFR-R2: Card pull accuracy must not deviate more than 0.1% from configured rarity rates across statistically significant sample sizes
NFR-R3: Concurrent active auctions must not share or interfere with each other's escrow state or settlement logic
NFR-R4: If a client's SignalR connection drops during an active auction, reconnection automatically re-fetches current auction state — client never displays stale bid data
NFR-SC1: Escrow and notification systems support multiple simultaneously active auctions without degradation in per-auction response time or event delivery
NFR-SC2: Match and wager resolution logic accommodates real-player H2H (Phase 2) without re-architecting the core settlement flow
NFR-I1: Existing points balance data structure extended to support escrow tracking — escrow is a field/record on the existing account model
NFR-I2: SignalR hub connections scoped per auction listing — a client connected to listing A does not receive events for listing B
NFR-I3: CardPlayer entity has no foreign-key dependency on the existing Player Active Roster entity — independent catalog systems

### Additional Requirements

**From Architecture — Infrastructure Setup (must precede feature work):**
- Install `@microsoft/signalr@10.0.0` npm package before any marketplace frontend work
- Register SignalR in `Program.cs`: `builder.Services.AddSignalR()` + `app.MapHub<AuctionHub>("/hubs/auction")`
- Register auction expiry background service: `builder.Services.AddHostedService<AuctionExpiryService>()`
- Scaffold 7 new Nx libraries before any feature lib implementation (cards/data-access, cards/feature-cards, cards/feature-collection, marketplace/data-access, marketplace/feature-marketplace, h2h/data-access, h2h/feature-h2h)
- Add 7 tsconfig path aliases to `tsconfig.base.json` for new libraries

**From Architecture — Data Setup (must precede business logic):**
- 4 EF Core migrations in dependency order: RarityTierConfig → Cards → Marketplace → H2H
- `RarityTierConfig` table must be seeded by GM before any pack purchases go live
- All new entities must include `organizationId` scope column

**From Architecture — Integration Gates:**
- NSwag client regeneration required after each new controller endpoint — hard gate before Angular HTTP service work begins
- JWT via `?access_token=` query string pattern required for SignalR hub authentication

**From PRD — Platform/UX:**
- Responsive design required — marketplace bidding and H2H squad builder must be usable on mobile screen sizes
- Modern evergreen browsers only (Chrome, Firefox, Safari, Edge — latest two major versions)
- All Fan Economy content is auth-gated; no public/anonymous pages

### FR Coverage Map

FR1: Epic 1 — Fan views points balance under new currency name
FR2: Epic 1 — Balance reflects all transactions in real-time
FR3: Epic 1 — Transaction audit trail (timestamp, user ID, action, entity)
FR4: Epic 1 — Points cannot be converted to real money
FR5: Epic 1 — GM creates CardPlayer entry
FR6: Epic 1 — GM edits CardPlayer
FR7: Epic 1 — Auto rarity tier assignment from rating thresholds
FR8: Epic 1 — CardPlayer entries scoped to league
FR9: Epic 1 — GM views all CardPlayers for their league
FR10: Epic 1 — Historical CardPlayers persist independently of Active Roster
FR11: Epic 2 — Fan purchases card pack (spends points)
FR12: Epic 2 — Pack produces exactly 5 cards from league catalog
FR13: Epic 2 — Rarity distribution governed by probability weights
FR14: Epic 2 — Fan receives 5 cards immediately upon purchase
FR15: Epic 2 — Points deducted atomically at pack purchase
FR16: Epic 2 — Fan cannot view rarity pull probabilities
FR17: Epic 2 — Fan views all owned cards
FR18: Epic 2 — Fan views individual card details
FR19: Epic 3 — Fan lists card for auction (start bid, buy now, duration)
FR20: Epic 3 — Listed card removed from seller's usable collection
FR21: Epic 3 — Fan browses active auction listings
FR22: Epic 3 — Fan places bid on active listing
FR23: Epic 3 — Points escrowed atomically on bid
FR24: Epic 3 — Escrowed points released immediately on outbid
FR25: Epic 3 — Fan purchases at buy now price, ending auction
FR26: Epic 3 — Winner receives card; seller receives final bid in points
FR27: Epic 3 — Fan cannot bid on their own listing
FR28: Epic 3 — Fan cannot bid with insufficient available balance
FR29: Epic 3 — Fan receives in-app outbid notification
FR30: Epic 3 — Current highest bid updates in real-time (no refresh)
FR31: Epic 3 — Auction state consistent across all viewing clients
FR32: Epic 4 — Fan selects up to 5 cards for H2H squad
FR33: Epic 4 — Squad builder displays live team overall rating
FR34: Epic 4 — Fan sets wager amount before confirming match
FR35: Epic 4 — Wagered points escrowed on match confirmation
FR36: Epic 4 — Bot opponent assembled randomly from real CardPlayer catalog
FR37: Epic 4 — Match outcome determined by weighted probability (overall delta)
FR38: Epic 4 — Fan experiences live match resolution sequence
FR39: Epic 4 — Winner receives wagered points; loser forfeits
FR40: Epic 4 — Balance updated immediately upon match resolution
FR41: Epic 5 — GM views economy dashboard (circulation, volume, price trends)
FR42: Epic 5 — GM identifies marketplace activity and pull frequency per card

## Epic List

### Epic 1: Currency Identity & Card Catalog Foundation
GMs can build the card pool with automatic rarity tier assignment, and fans see their points displayed under the platform's new currency identity. This foundational epic makes all three economy systems possible — no packs can be opened, no cards traded, and no matches played until the card catalog exists and the currency is live.

**FRs covered:** FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, FR10
**NFRs addressed:** NFR-S4, NFR-I3, NFR-I1

### Epic 2: Card Pack Opening & Collection
Fans can purchase packs by spending points, receive exactly 5 rarity-weighted player cards drawn from their league's catalog, and browse their full card collection. This is the economy's primary entry point — the moment a fan first participates in the Fan Economy loop.

**FRs covered:** FR11, FR12, FR13, FR14, FR15, FR16, FR17, FR18
**NFRs addressed:** NFR-P2, NFR-R2, NFR-S1, NFR-S3

### Epic 3: Auction Marketplace with Real-Time Bidding
Fans can list owned cards for auction, browse the marketplace, place bids with atomic points escrow, receive real-time outbid push notifications via SignalR, execute buy now purchases, and settle completed auctions — receiving cards or points as appropriate.

**FRs covered:** FR19, FR20, FR21, FR22, FR23, FR24, FR25, FR26, FR27, FR28, FR29, FR30, FR31
**NFRs addressed:** NFR-P1, NFR-P3, NFR-S2, NFR-R1, NFR-R3, NFR-R4, NFR-SC1, NFR-I2

### Epic 4: Head-to-Head Squad Competition
Fans can assemble a 5-card H2H squad, set a points wager, compete against a bot opponent drawn from the real card catalog, experience a live match resolution sequence, and receive win/loss point settlement immediately — closing the economy loop.

**FRs covered:** FR32, FR33, FR34, FR35, FR36, FR37, FR38, FR39, FR40
**NFRs addressed:** NFR-P4, NFR-S1, NFR-R1, NFR-SC2

### Epic 5: Economy Administration & Monitoring
GMs can view a live economy dashboard showing cards in circulation, marketplace listing volume by card, average sale price trends, and pack pull frequency per card — enabling informed catalog management and economy health monitoring.

**FRs covered:** FR41, FR42
**NFRs addressed:** NFR-SC1 (read-side aggregation)

---

## Epic 1: Currency Identity & Card Catalog Foundation

GMs can build the card pool with automatic rarity tier assignment, and fans see their points displayed under the platform's new currency identity. This foundational epic makes all three economy systems possible — no packs can be opened, no cards traded, and no matches played until the card catalog exists and the currency is live.

### Story 1.1: Points Currency Rebrand & Balance Display

As a fan,
I want to see my points balance displayed with the platform's new currency name throughout the app,
So that I experience points as a valued platform asset rather than just a voting mechanism.

**Acceptance Criteria:**

**Given** I am a logged-in fan on any page that displays my balance
**When** the page loads
**Then** my points balance is displayed with the new platform currency name (not "Votes")
**And** the existing numeric balance value is preserved exactly

**Given** I complete a points transaction (pack purchase, bid, H2H wager)
**When** the transaction completes
**Then** my displayed balance updates to reflect the new total without requiring a page refresh
**And** the balance never shows a stale value after a completed transaction

---

### Story 1.2: Points Transaction Audit Trail

As a platform operator,
I want every point movement recorded with full context,
So that any user dispute can be resolved with an authoritative, timestamped audit trail.

**Acceptance Criteria:**

**Given** any Fan Economy operation moves points (pack purchase, bid escrow, escrow release, auction settlement, H2H wager, H2H payout)
**When** the transaction executes
**Then** a PointTransaction record is created containing: userId, timestamp (UTC), actionType, amount, and associated entityId (packId, listingId, or matchId)
**And** the record is immutable — no UPDATE or DELETE operations are permitted on PointTransaction records

**Given** I calculate the running sum of all PointTransaction records for a fan
**When** compared against the fan's displayed balance
**Then** the sum matches exactly (audit integrity)

**Given** a fan attempts to spend points they do not have
**When** the transaction is attempted
**Then** the operation is rejected server-side before any PointTransaction record is written
**And** the fan's balance remains unchanged

---

### Story 1.3: CardPlayer Catalog Data Foundation

As the platform,
I want the CardPlayer catalog data model and rarity tier configuration established,
So that GMs can create player cards that are automatically assigned the correct rarity tier based on their overall rating.

**Acceptance Criteria:**

**Given** the RarityTierConfig table is seeded with threshold ranges (e.g., 95–99 = Legendary, 85–94 = Epic, 75–84 = Rare, 60–74 = Common)
**When** a CardPlayer is created with an overall rating of 97
**Then** the system assigns it the rarity tier whose threshold range contains 97
**And** no manual rarity selection is permitted — the assignment is automatic and only editable via RarityTierConfig

**Given** a CardPlayer entry exists for a historical player in League A
**When** the Active Roster system undergoes changes (player removed, season rollover)
**Then** the CardPlayer entry is unaffected — it has no FK dependency on the Player Active Roster table
**And** the CardPlayer remains available in pack pulls for League A

**Given** a CardPlayer is created scoped to League A
**When** a fan in League B opens a pack
**Then** the League A CardPlayer does not appear in the League B pull

> **Technical note:** This story creates the `RarityTierConfigs` and `CardPlayers` EF Core migrations, scaffolds the 3 `libs/cards/` Nx libraries (`data-access`, `feature-cards`, `feature-collection`), and adds their `tsconfig.base.json` path aliases.

---

### Story 1.4: GM CardPlayer Management API

As a GM,
I want API endpoints to create, edit, and view CardPlayer entries for my league,
So that I can build and maintain the card pool that fans pull from.

**Acceptance Criteria:**

**Given** I am an authenticated GM for my league
**When** I POST `/api/cards/players` with name, position, leagueId, and overallRating
**Then** a CardPlayer is created with the auto-assigned rarity tier
**And** the response includes the CardPlayer with its rarityTier
**And** the NSwag TypeScript client is regenerated to include this endpoint

**Given** an existing CardPlayer in my league
**When** I PUT `/api/cards/players/{id}` with an updated overallRating
**Then** the rarity tier is recalculated based on the new rating
**And** a 403 is returned if the requesting GM's leagueId does not match the card's leagueId

**Given** I am an authenticated GM
**When** I GET `/api/cards/players?leagueId={myLeagueId}`
**Then** all CardPlayer entries for that league are returned with name, position, rating, and rarityTier
**And** CardPlayers from other leagues are not included

**Given** a non-GM authenticated user
**When** they attempt POST or PUT on `/api/cards/players`
**Then** the API returns 403 Forbidden

---

### Story 1.5: GM Card Catalog Administration UI

As a GM,
I want a card catalog admin panel in the sports-gm application,
So that I can create, edit, and review CardPlayer entries for my league without touching an API directly.

**Acceptance Criteria:**

**Given** I am a logged-in GM in the sports-gm app
**When** I navigate to the Card Catalog section
**Then** I see a list of all CardPlayer entries for my league including name, position, overall rating, and rarity tier

**Given** I click "Add Card Player"
**When** I enter name, position, league, and overall rating and submit
**Then** the CardPlayer is created, the auto-assigned rarity tier is displayed, and the new entry appears in the list
**And** there is no manual rarity tier input — it is shown as a derived read-only value

**Given** I click Edit on an existing CardPlayer
**When** I change the overall rating and save
**Then** the displayed rarity tier updates to reflect the new rating if it crosses a tier threshold

**Given** the API returns a validation error
**When** I submit the create or edit form
**Then** a clear error message is displayed without clearing my input values

---

## Epic 2: Card Pack Opening & Collection

Fans can purchase packs by spending points, receive exactly 5 rarity-weighted player cards drawn from their league's catalog, and browse their full card collection. This is the economy's primary entry point — the moment a fan first participates in the Fan Economy loop.

### Story 2.1: Card Pack Purchase & Pull Engine Backend

As a fan,
I want to purchase a card pack and immediately receive exactly 5 rarity-weighted player cards,
So that I can start building my card collection by spending my points.

**Acceptance Criteria:**

**Given** I am an authenticated fan with sufficient points and my league has CardPlayers in the catalog
**When** I POST `/api/cards/packs/purchase` with my leagueId
**Then** exactly 5 cards are drawn from my league's CardPlayer catalog using configured rarity weights
**And** my points are debited atomically — if the pull fails for any reason, no points are deducted
**And** the response returns the 5 resulting UserCard records with name, rating, rarityTier, and league

**Given** the rarity engine runs a pull
**When** the pull executes
**Then** each card is selected using weighted probability from the `RarityTierConfig` table
**And** the pull seed and result are persisted in the pull log: (userId, cardPlayerId, rarityTier, seed, timestamp, packId)
**And** rarity weights are never included in the API response — the client receives only the resulting cards

**Given** I attempt to purchase a pack with insufficient points
**When** the request is processed
**Then** the API returns 422 with code `INSUFFICIENT_BALANCE`
**And** no CardPack record, no UserCard records, and no PointTransaction records are created

**Given** statistically significant pack pulls are run
**When** pull outcomes are compared to configured rarity weights
**Then** actual pull rates deviate no more than 0.1% from configured rates

> **Technical note:** This story creates `UserCards` and `CardPacks` EF Core migrations, implements `PurchasePackCommand` with `RarityEngine`, adds the pack purchase endpoint to `CardsController`, and regenerates the NSwag TypeScript client.

---

### Story 2.2: Pack Purchase & Card Reveal UI

As a fan,
I want to purchase a pack from the app and see my 5 cards revealed sequentially,
So that the pack opening experience is exciting and the rarity of each card is clearly communicated.

**Acceptance Criteria:**

**Given** I am on the Card Packs page and have sufficient points
**When** I click "Open Pack"
**Then** a loading state is shown while the server processes the purchase
**And** the result arrives within 2 seconds of my confirmation

**Given** the pack purchase succeeds
**When** my 5 cards are returned from the server
**Then** each card is revealed one by one in sequence
**And** each card displays the player name, overall rating, rarity tier, and league
**And** rarity tier is visually distinct (e.g., Legendary styled differently from Common)

**Given** my pack purchase returns
**When** the reveal completes
**Then** my displayed points balance reflects the deducted pack cost without requiring a page refresh

**Given** the pack purchase fails (insufficient balance or API error)
**When** I click "Open Pack"
**Then** a clear error message is shown and no card reveal animation plays
**And** my points balance is unchanged

**Given** I am on the Card Packs page
**When** I view pack information
**Then** rarity pull probabilities are not displayed anywhere on the page

---

### Story 2.3: Fan Card Collection View

As a fan,
I want to browse all cards I currently own with their full details,
So that I can identify duplicates to list on the marketplace and choose cards to add to my H2H squad.

**Acceptance Criteria:**

**Given** I am an authenticated fan
**When** I navigate to My Collection
**Then** all UserCards I own are displayed with player name, overall rating, rarity tier, and league

**Given** I own no cards
**When** I navigate to My Collection
**Then** an empty state is shown encouraging me to open my first pack

**Given** I click on a card in my collection
**When** the card detail view opens
**Then** I can see the player's name, overall rating, rarity tier, and league
**And** I can see whether the card is currently listed on the marketplace or locked in an active H2H match

**Given** I have cards from multiple leagues
**When** I view my collection
**Then** cards from all my leagues are shown with a league label visible on each card

> **Technical note:** This story implements `GetCollectionQuery`, adds GET `/api/cards/collection` to `CardsController`, regenerates NSwag, and builds the `feature-collection` Angular component.

---

## Epic 3: Auction Marketplace with Real-Time Bidding

**Goal:** Enable fans to list cards for auction, place bids with points escrow atomicity, receive real-time outbid notifications via SignalR, and settle auctions automatically at expiry — delivering the core trading economy loop.

**FRs Covered:** FR19, FR20, FR21, FR22, FR23, FR24, FR25, FR26, FR27, FR28, FR29, FR30, FR31

---

### Story 3.1: Card Listing & Marketplace Browse Backend

As a fan,
I want to list a card I own for auction and browse active listings,
So that I can participate in the card marketplace.

> **Technical note (Infrastructure):** This story scaffolds the marketplace Nx libraries (`libs/marketplace/data-access/`, `libs/marketplace/feature-marketplace/`, `libs/marketplace/ui-marketplace/`), creates the `AuctionListing` + `Bid` + `PointsEscrow` migrations, adds `CardOwner.IsListed` migration, installs `@microsoft/signalr@10.0.0`, registers `builder.Services.AddSignalR()` and stubs `app.MapHub<AuctionHub>("/hubs/auction")` in Program.cs (AuctionHub fully wired in Story 3.4).

**Acceptance Criteria:**

**Given** a fan owns a card (`CardOwner.UserId == request.UserId`)
**When** they POST `/api/cards/listings` with `{ cardId, startingBid, buyNowPrice?, durationHours }`
**Then** an `AuctionListing` is created with `Status = "active"`, `CardOwner.IsListed = true`, and 201 is returned with the listing ID

**Given** `startingBid <= 0` or `durationHours` is not in `[1, 24, 48, 72]`
**When** the endpoint is called
**Then** 422 is returned with a validation error

**Given** `request.CardId` does not belong to the requesting user
**When** the endpoint is called
**Then** 403 is returned

**Given** the card is already listed (`CardOwner.IsListed = true`)
**When** the endpoint is called
**Then** 422 is returned with `{ error: "Card is already listed", code: "ALREADY_LISTED" }`

**Given** active listings exist for an org
**When** a fan calls GET `/api/cards/listings?orgId={orgId}`
**Then** a paginated list is returned with listing ID, card name, rarity, overall, current bid, buy now price, expiry time, and seller display name

**Given** a specific listing ID
**When** GET `/api/cards/listings/{listingId}` is called
**Then** full listing detail is returned including bid history (bidder display name, amount, timestamp)

**Given** the Nx workspace
**When** this story is complete
**Then** `libs/marketplace/data-access/`, `libs/marketplace/feature-marketplace/`, and `libs/marketplace/ui-marketplace/` exist with correct `project.json` and barrel `index.ts` files

**Tasks:**
1. `nx g @nx/angular:library marketplace-data-access --directory=libs/marketplace/data-access`; repeat for `feature-marketplace` and `ui-marketplace`
2. `npm install @microsoft/signalr@10.0.0`
3. Add EF Core migration: `AuctionListing` (Id, CardOwnerId, SellerId, OrgId, StartingBid, BuyNowPrice, CurrentBid, Status, ExpiresAt, CreatedAt), `Bid` (Id, ListingId, BidderId, Amount, Timestamp), `PointsEscrow` (Id, UserId, ListingId, HeldAmount, Status)
4. Add `CardOwner.IsListed bool` column migration
5. Register `builder.Services.AddSignalR()` and stub `app.MapHub<AuctionHub>("/hubs/auction")` in Program.cs; create `AuctionHub` stub class
6. Create `IListingRepository`, `IBidRepository`, `IPointsEscrowRepository` in `Domain/Marketplace/Repositories/`
7. Implement `CreateListingCommand` + handler (validates ownership, sets `IsListed = true`, creates listing)
8. Implement `GetListingsQuery` + handler (paginated, active only, mapped to `ListingDto`)
9. Implement `GetListingDetailQuery` + handler (includes bid history)
10. Create `CardsListingsController` with POST `/api/cards/listings`, GET `/api/cards/listings`, GET `/api/cards/listings/{id}`
11. Regenerate NSwag TypeScript client

---

### Story 3.2: Bid Placement & Points Escrow Backend

As a fan,
I want to place bids on active auction listings with my points automatically escrowed,
So that my bid is securely held and released if I'm outbid.

**Acceptance Criteria:**

**Given** a fan has sufficient available balance
**When** they POST `/api/cards/listings/{listingId}/bids` with `{ amount }`
**Then** within a single `IDbContextTransaction`: bid amount is escrowed (new `PointsEscrow` row with `Status = "held"`), previous high bidder's escrow is released, `AuctionListing.CurrentBid` is updated, `Bid` row is created, and 201 is returned

**Given** `amount < listing.CurrentBid * 1.05` (less than 5% above current bid)
**When** a bid is placed
**Then** 422 is returned with `{ error: "Bid must exceed current bid by at least 5%", code: "OUTBID_AMOUNT_TOO_LOW" }`

**Given** the fan's available balance (VoteAccount.Balance minus all active `PointsEscrow.HeldAmount`) is less than the bid amount
**When** a bid is placed
**Then** 422 is returned with `{ error: "Insufficient balance", code: "INSUFFICIENT_BALANCE" }`

**Given** the fan is the listing seller
**When** they attempt to bid
**Then** 422 is returned with `{ error: "Cannot bid on your own listing", code: "SELF_BID" }`

**Given** the listing `Status != "active"` or `ExpiresAt < now`
**When** a bid is placed
**Then** 422 is returned with `{ error: "Auction has expired", code: "AUCTION_EXPIRED" }`

**Given** a fan calls POST `/api/cards/listings/{listingId}/buy-now`
**When** the buy now price is set on the listing and the fan has sufficient balance
**Then** within a single transaction: buyer's points are debited, seller receives points, `CardOwner.UserId` is transferred, `AuctionListing.Status = "sold"`, existing escrows are released, and 200 is returned

**Given** the buy now price is null on the listing
**When** buy-now is called
**Then** 422 is returned with `{ code: "NO_BUY_NOW_PRICE" }`

**Given** any escrow error (insufficient balance, self-bid, auction expired)
**Then** the entire transaction is rolled back — no partial state changes occur

**Tasks:**
1. Implement `PlaceBidCommand` + handler (full transactional escrow: validate balance, create bid, release previous escrow, update current bid, commit)
2. Implement `BuyNowCommand` + handler (transactional: debit buyer, credit seller, transfer card ownership, settle listing, release all escrows)
3. Implement `GetAvailableBalanceQuery` (VoteAccount.Balance minus sum of active PointsEscrow.HeldAmount for user)
4. Add `POST /api/cards/listings/{listingId}/bids` and `POST /api/cards/listings/{listingId}/buy-now` to `CardsListingsController`
5. Add `PointTransaction` rows for all balance movements (escrow hold, release, credit, debit) — satisfies FR3
6. Regenerate NSwag TypeScript client

---

### Story 3.3: Auction Settlement Backend

As a system,
I want auctions to settle automatically when they expire,
So that winners receive their cards and sellers receive their points without manual intervention.

**Acceptance Criteria:**

**Given** an `AuctionListing` with `Status = "active"` and `ExpiresAt < DateTime.UtcNow` exists
**When** `AuctionExpiryService` polls (every 30 seconds)
**Then** for each expired listing `SettleAuctionCommand` is dispatched via MediatR `ISender`

**Given** `SettleAuctionCommand` is dispatched for a listing with at least one bid
**When** the handler executes
**Then** within a single transaction: winning `PointsEscrow` is settled (HeldAmount credited to seller's VoteAccount.Balance), all other escrows are released, `CardOwner.UserId` is transferred to winner, `AuctionListing.Status = "settled"`, `CardOwner.IsListed = false`, and `PointTransaction` rows are recorded for all movements

**Given** `SettleAuctionCommand` is dispatched for a listing with zero bids
**When** the handler executes
**Then** listing `Status = "expired"`, `CardOwner.IsListed = false`, no points are moved

**Given** `AuctionExpiryService` is running and a settlement throws an exception
**When** the exception occurs
**Then** the transaction rolls back, the error is logged via Serilog, and the service continues polling — it does not crash

**Given** `builder.Services.AddHostedService<AuctionExpiryService>()` is registered
**When** the application starts
**Then** the background service begins its 30-second polling loop

**Tasks:**
1. Create `AuctionExpiryService : BackgroundService` in `Infrastructure/Marketplace/Services/`
2. Implement `SettleAuctionCommand` + handler (transactional settlement per ACs above)
3. Register `builder.Services.AddHostedService<AuctionExpiryService>()` in Program.cs
4. Unit test: `AuctionExpiryService_CallsSettleForExpiredListings` (in-memory repository stub)
5. Unit test: `SettleAuctionCommandHandler_TransfersCardAndPoints_WhenBidsExist`
6. Unit test: `SettleAuctionCommandHandler_ExpiresListingWithNoChanges_WhenNoBids`

---

### Story 3.4: Real-Time SignalR Auction Hub

As a fan,
I want to receive real-time outbid notifications and see live bid updates on the listing I'm viewing,
So that I can react immediately when the auction changes.

**Acceptance Criteria:**

**Given** a fan is connected to the SignalR hub at `/hubs/auction` with a valid JWT
**When** they invoke `JoinListing(listingId)`
**Then** they are added to the `$"auction-{listingId}"` group

**Given** a fan invokes `LeaveListing(listingId)`
**Then** they are removed from the `$"auction-{listingId}"` group

**Given** a bid is successfully placed (Story 3.2 handler)
**When** the handler completes
**Then** `IHubContext<AuctionHub>` sends `BidPlaced { listingId, currentBid, bidderDisplayName, timestamp }` to the `$"auction-{listingId}"` group

**Given** a fan's escrow is released (outbid)
**When** the release occurs
**Then** `IHubContext<AuctionHub>` sends `OutbidNotification { listingId, releasedAmount }` to the specific outbid user's group `$"user-{userId}"`

**Given** an auction is settled by `AuctionExpiryService`
**When** settlement completes
**Then** `AuctionSettled { listingId, winnerDisplayName, finalBid }` is broadcast to the `$"auction-{listingId}"` group

**Given** a client connects to `/hubs/auction` without a valid JWT
**When** the connection is attempted
**Then** the hub rejects the connection with 401

**Given** the Angular MarketplaceStore
**When** a `BidPlaced` event is received
**Then** `rxMethod` updates `currentBid` in state and the listing detail view re-renders without a page refresh

**Given** an `OutbidNotification` is received
**When** it arrives
**Then** an in-app toast is shown: "You have been outbid on [card name]"

**Tasks:**
1. Implement `AuctionHub : Hub` with `JoinListing` and `LeaveListing` methods; join user-specific group `$"user-{userId}"` on `OnConnectedAsync`
2. Configure JWT auth for SignalR in Program.cs: `options.Events.OnMessageReceived` reads `?access_token=` query param
3. Create `AuctionSignalRService` (injected into `PlaceBidCommandHandler` and `SettleAuctionCommandHandler`) wrapping `IHubContext<AuctionHub>`
4. Create `libs/marketplace/data-access/src/lib/marketplace-signalr.service.ts` — Angular service wrapping `@microsoft/signalr` HubConnection; exposes `bid$` and `outbid$` observables
5. Add `joinListing(id)` / `leaveListing(id)` methods to `MarketplaceSignalRService`
6. In `MarketplaceStore`, wire `bid$` via `rxMethod` to update `currentBid` in state
7. Dispatch toast notification in store when `outbid$` emits

---

### Story 3.5: Marketplace Frontend

As a fan,
I want to browse auction listings, view listing details, place bids, and receive live updates in the UI,
So that I can participate in the card marketplace without leaving the app.

**Acceptance Criteria:**

**Given** a fan navigates to the Marketplace page
**When** the page loads
**Then** `MarketplaceStore.loadListings(orgId)` is called and a grid of active listing cards is displayed (player name, rarity badge, current bid, buy now price if set, time remaining)

**Given** `status === 'loading'`
**When** the page is loading
**Then** a skeleton loader or spinner is displayed

**Given** no active listings exist
**When** the page loads
**Then** an empty state message "No active listings yet" is displayed

**Given** a fan clicks a listing card
**When** the listing detail view opens
**Then** the fan joins the SignalR `auction-{listingId}` group and sees the bid history and current bid

**Given** the fan is not the listing seller and has sufficient balance
**When** they enter a bid amount and click "Place Bid"
**Then** `MarketplaceStore.placeBid()` is called and on success the bid history refreshes

**Given** the fan enters a bid below the minimum (current bid + 5%)
**When** they click "Place Bid"
**Then** an inline validation error is shown before any API call is made

**Given** a `BidPlaced` SignalR event arrives for the viewed listing
**When** it is received
**Then** the current bid updates in real-time without a page refresh

**Given** the listing has a buy now price
**When** the fan clicks "Buy Now"
**Then** a confirmation dialog appears; on confirm `MarketplaceStore.buyNow()` is called and the card moves to their collection

**Given** a fan navigates away from the listing detail
**When** they leave
**Then** `LeaveListing(listingId)` is called to clean up the SignalR group

**Given** the fan owns a card in the Collection page (Story 2.3)
**When** they view a card
**Then** a "List for Auction" button is visible; clicking it opens a listing form with `startingBid`, `buyNowPrice` (optional), and `durationHours` fields

**Tasks:**
1. Create `MarketplaceStore` (`signalStore`) with state `{ listings, selectedListing, bidHistory, status, checkoutStatus }`; methods `loadListings`, `loadListingDetail`, `placeBid`, `buyNow`; use `rxMethod` for all async operations
2. Implement `MarketplaceService` wrapping the NSwag-generated `CardsListingsClient`
3. Create `feature-marketplace` component: listing grid, listing detail panel, bid form, buy now button
4. Wire `MarketplaceSignalRService` (Story 3.4) into `MarketplaceStore` — subscribe `bid$` and `outbid$` on store init, unsubscribe on destroy
5. Add listing form to `feature-collection` component — "List for Auction" action per card
6. Add route `/marketplace` to app routing and nav link to layout
7. Add signal-based toast notification queue for outbid notifications

---

## Epic 4: Head-to-Head Squad Competition

**Goal:** Enable fans to assemble a squad of up to 5 cards, wager points on a match against a bot opponent, and receive an outcome determined by a weighted probability engine — closing the economy loop by returning points to winners and creating a consumption sink for losers.

**FRs Covered:** FR32, FR33, FR34, FR35, FR36, FR37, FR38, FR39, FR40

---

### Story 4.1: H2H Domain & Bot Resolution Backend

As a fan,
I want the system to resolve H2H matches using my squad's overall rating against a bot,
So that outcomes are fair and based on card quality.

> **Technical note (Infrastructure):** This story scaffolds the H2H Nx libraries (`libs/h2h/data-access/`, `libs/h2h/feature-h2h/`, `libs/h2h/ui-h2h/`) and creates the `H2HMatch` + `H2HSquadCard` EF Core migrations. These are embedded here as they are required by this story's deliverables.

**Acceptance Criteria:**

**Given** an `H2HMatch` entity is created with `Status = "pending"`
**When** `ResolveMatchCommand` is dispatched
**Then** a bot squad is assembled from real `CardPlayer` entries in the fan's league (max 5 cards, weighted toward the fan's overall rating range ±15 points), the bot's team overall is calculated, and the outcome is determined by `BotResolutionEngine`

**Given** the fan's team overall is greater than the bot's
**When** `BotResolutionEngine.Resolve()` is called
**Then** the fan wins with probability `0.5 + (delta / 200)` capped at 0.85, where `delta = fanOverall - botOverall`

**Given** the fan's team overall is less than the bot's
**When** `BotResolutionEngine.Resolve()` is called
**Then** the fan wins with probability `0.5 - (Abs(delta) / 200)` floored at 0.15

**Given** the fan's team overall equals the bot's
**When** `BotResolutionEngine.Resolve()` is called
**Then** the fan wins with exactly 50% probability

**Given** a match outcome is determined
**When** the handler completes
**Then** within a single `IDbContextTransaction`: the wager is transferred (winner receives wager × 2, loser loses wager), `PointTransaction` rows are recorded, `H2HMatch.Status = "completed"`, `H2HMatch.Outcome` and `H2HMatch.BotSquadSnapshot` (JSON) are persisted

**Given** the fan has insufficient available balance for the wager
**When** `CreateMatchCommand` is called
**Then** 422 is returned with `{ code: "INSUFFICIENT_BALANCE" }`

**Given** `H2HMatch.Status = "completed"` already
**When** `ResolveMatchCommand` is re-dispatched
**Then** the handler is a no-op — no state changes occur (idempotent)

**Tasks:**
1. `nx g @nx/angular:library h2h-data-access --directory=libs/h2h/data-access`; repeat for `feature-h2h` and `ui-h2h`
2. Add EF Core migration: `H2HMatch` (Id, FanUserId, OrgId, WagerAmount, FanTeamOverall, BotTeamOverall, BotSquadSnapshot JSON, Outcome, Status, CreatedAt, CompletedAt), `H2HSquadCard` (Id, MatchId, CardOwnerId, SlotIndex)
3. Create `BotResolutionEngine.cs` in `Domain/H2H/Services/` with weighted probability formula
4. Implement `CreateMatchCommand` + handler (validates squad, validates wager balance, escrows wager, dispatches `ResolveMatchCommand`)
5. Implement `ResolveMatchCommand` + handler (builds bot squad, calls engine, settles wager transactionally, persists outcome)
6. Create `IH2HMatchRepository` in `Domain/H2H/Repositories/`
7. Unit test: `BotResolutionEngine_FanWinProbability_ScalesWithDelta` (parametrized: delta -50, 0, +50)
8. Unit test: `ResolveMatchCommandHandler_SettlesWager_WhenFanWins`
9. Unit test: `ResolveMatchCommandHandler_SettlesWager_WhenFanLoses`

---

### Story 4.2: H2H Match API & NSwag

As a fan,
I want to create and view H2H matches via API,
So that the frontend can initiate and display match results.

**Acceptance Criteria:**

**Given** a valid authenticated fan
**When** POST `/api/h2h/matches` is called with `{ cardOwnerIds: string[], wagerAmount: number }`
**Then** a match is created, `ResolveMatchCommand` is dispatched synchronously, and the response includes match ID, outcome, fan team overall, bot team overall, bot squad card names, and updated balance

**Given** `cardOwnerIds.length < 1` or `cardOwnerIds.length > 5`
**When** the endpoint is called
**Then** 422 is returned with `{ code: "INVALID_SQUAD_SIZE" }`

**Given** any `cardOwnerId` does not belong to the requesting user or is currently listed on the marketplace
**When** the endpoint is called
**Then** 422 is returned with `{ code: "INVALID_CARD_SELECTION" }`

**Given** `wagerAmount <= 0`
**When** the endpoint is called
**Then** 422 is returned with a validation error

**Given** a match ID
**When** GET `/api/h2h/matches/{matchId}` is called
**Then** full match detail is returned including bot squad snapshot, outcome, fan team cards, and wager result

**Given** a fan calls GET `/api/h2h/matches?orgId={orgId}`
**Then** a paginated match history is returned (most recent first, scoped to requesting user + org)

**Tasks:**
1. Create `H2HController` with POST `/api/h2h/matches`, GET `/api/h2h/matches/{matchId}`, GET `/api/h2h/matches`
2. Implement `GetMatchDetailQuery` + handler
3. Implement `GetMatchHistoryQuery` + handler (paginated, scoped to requesting user + org)
4. Implement `ValidateSquadQuery` (validates ownership + not listed) called from `CreateMatchCommand` handler
5. Regenerate NSwag TypeScript client

---

### Story 4.3: Squad Builder & H2H Frontend

As a fan,
I want to select up to 5 cards for my squad, see my team's overall rating, choose a wager, and play a match,
So that I can compete and earn points through card-based strategy.

**Acceptance Criteria:**

**Given** a fan navigates to the H2H page
**When** the page loads
**Then** their collection is displayed as selectable cards; cards currently listed on the marketplace are shown as unavailable (greyed out, non-selectable)

**Given** the fan selects up to 5 cards
**When** cards are selected
**Then** `H2HStore.squadOverall` (computed signal) updates in real-time showing the team's average overall rating

**Given** fewer than 1 card is selected
**When** the fan tries to confirm the squad
**Then** an inline validation message is shown and the "Play Match" button is disabled

**Given** 1–5 cards are selected and a wager is entered
**When** the fan clicks "Play Match"
**Then** a confirmation dialog shows: squad summary, wager amount, and the note "Bot difficulty scales with your squad rating"

**Given** the fan confirms the match
**When** the API call resolves successfully
**Then** the match result screen shows: win/loss outcome, fan team vs bot team overall comparison, bot card names, points won/lost, and updated balance

**Given** the match result is shown
**When** the fan clicks "Play Again"
**Then** the squad builder resets and they can select new cards

**Given** a fan views their H2H match history
**When** the history page/tab loads
**Then** past matches are listed with date, outcome (win/loss), wager amount, fan overall vs bot overall

**Given** `status === 'loading'`
**When** the match is being processed
**Then** an animated loading state "Calculating match outcome..." is displayed

**Tasks:**
1. Create `H2HStore` (`signalStore`) with state `{ collection, selectedCardIds, wagerAmount, matchResult, matchHistory, status }`; computed `squadOverall`; methods `loadCollection`, `toggleCard`, `playMatch`, `loadMatchHistory`; use `rxMethod` for all async
2. Implement `H2HService` wrapping NSwag-generated `H2HClient`
3. Create `feature-h2h` component: card selector grid, squad summary panel, wager input, match result screen, match history list
4. Add route `/h2h` to app routing and nav link to layout
5. Re-use card display components from `libs/cards/ui-cards/` for the card selector — no duplication

---

## Epic 5: Economy Administration & Monitoring

**Goal:** Give GMs the visibility and controls needed to configure and monitor the fan economy — rarity tier thresholds, pack pricing, and transaction audit — without requiring developer intervention.

**FRs Covered:** FR41, FR42

---

### Story 5.1: Economy Configuration UI (GM)

As a GM,
I want to configure rarity tier thresholds and pack prices through a UI,
So that I can tune the economy without touching the database or code.

**Acceptance Criteria:**

**Given** a GM navigates to Economy Settings in the GM portal
**When** the page loads
**Then** all `RarityTierConfig` rows for their league are displayed with columns: rarity name, rating min, rating max, pull weight (bps), and pack price (points)

**Given** a GM edits a `RarityTierConfig` row and clicks Save
**When** the API call succeeds
**Then** the updated thresholds take effect for all subsequent pack pulls and a success confirmation is shown

**Given** `ratingMin >= ratingMax`
**When** the GM saves
**Then** an inline validation error is shown and the API call is not made

**Given** `pullWeightBps` values across all tiers do not sum to 10000 (representing 100.00%)
**When** the GM saves
**Then** a validation error is shown: "Pull weights must total 100%"

**Given** a GM sets a new pack price
**When** a fan subsequently purchases a pack
**Then** the updated price is applied — the old price is not used for any new purchases after the config change

**Tasks:**
1. Implement `UpdateRarityTierConfigCommand` + handler (validates sum of bps == 10000, saves to DB)
2. Implement `GetRarityTierConfigQuery` + handler (returns all tiers for an org/league)
3. Implement `UpdatePackConfigCommand` + handler (saves pack price)
4. Add `GET /api/admin/rarity-config`, `PUT /api/admin/rarity-config`, `GET /api/admin/pack-config`, `PUT /api/admin/pack-config` to a new `AdminEconomyController` (GM role only)
5. Regenerate NSwag TypeScript client
6. Create `EconomyAdminStore` (`signalStore`) with state `{ rarityTiers, packConfig, status }`; methods `load`, `saveRarityTier`, `savePackConfig`
7. Build Economy Settings page in `sports-gm` app with editable table for rarity tiers and pack price input
8. Add client-side validation: `pullWeightBps` sum == 10000 before API call

---

### Story 5.2: Transaction Audit Log UI (GM)

As a GM,
I want to view the full points transaction audit log for my league,
So that I can investigate unusual activity, verify payouts, and ensure economy integrity.

**Acceptance Criteria:**

**Given** a GM navigates to the Audit Log page
**When** the page loads
**Then** all `PointTransaction` rows for their league are displayed, paginated (50 per page), sorted by timestamp descending, with columns: timestamp, user display name, action type, amount (+ or −), associated entity ID, and resulting balance after transaction

**Given** a GM enters a user display name in the search filter and applies it
**When** the filter is active
**Then** only transactions for that user are shown

**Given** a GM selects an action type filter (e.g. "pack_purchase", "auction_win", "h2h_loss")
**When** the filter is applied
**Then** only matching transaction types are shown

**Given** a GM selects a date range filter and applies it
**When** the filter is active
**Then** only transactions within that date range are shown

**Given** no transactions match the current filters
**When** the filters are applied
**Then** an empty state message "No transactions found for this filter" is shown

**Given** the audit log contains data and the GM clicks "Export CSV"
**When** the export is triggered
**Then** a CSV download begins containing all filtered transactions (not just the current page)

**Tasks:**
1. Implement `GetTransactionAuditQuery` + handler (paginated, filterable by userId, actionType, dateRange, orgId)
2. Add `GET /api/admin/transactions` to `AdminEconomyController` with query params `userId`, `actionType`, `from`, `to`, `page`, `pageSize`
3. Implement `ExportTransactionsCsvQuery` + handler (returns all matching rows as CSV stream)
4. Add `GET /api/admin/transactions/export` to `AdminEconomyController`
5. Regenerate NSwag TypeScript client
6. Create `AuditLogStore` (`signalStore`) with state `{ transactions, filters, totalCount, status }`; methods `load`, `applyFilters`, `exportCsv`
7. Build Audit Log page in `sports-gm` app: data table, filter bar (user search, action type dropdown, date range picker), pagination controls, Export CSV button
