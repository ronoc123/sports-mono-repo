---
stepsCompleted: ['step-01-init', 'step-02-discovery', 'step-02b-vision', 'step-02c-executive-summary', 'step-03-success', 'step-04-journeys', 'step-05-domain', 'step-06-innovation', 'step-07-project-type', 'step-08-scoping', 'step-09-functional', 'step-10-nonfunctional', 'step-11-polish']
inputDocuments:
  - "_bmad-output/brainstorming/brainstorming-session-2026-02-21.md"
  - "docs/index.md"
  - "docs/architecture.md"
  - "docs/integration-architecture.md"
  - "docs/api-contracts-backend.md"
  - "docs/data-models-backend.md"
  - "docs/component-inventory-frontend.md"
  - "docs/state-management-frontend.md"
  - "docs/source-tree-analysis.md"
documentCounts:
  briefs: 0
  research: 0
  brainstorming: 1
  projectDocs: 8
classification:
  projectType: web_app
  domain: general
  complexity: high
  projectContext: brownfield
workflowType: 'prd'
---

# Product Requirements Document - sports-ui Fan Economy

**Author:** Kampe
**Date:** 2026-02-28

## Executive Summary

The sports-ui Fan Economy transforms the platform's passive points currency (currently "votes," to be rebranded) from a GM-dispensed voting mechanism into the fuel of a three-system fan engagement economy. Fans spend points to open player card packs, trade cards on a real-time auction marketplace, and wager points in a Head-to-Head squad battle mode — creating a durable engagement loop that deepens platform stickiness well beyond the base voting feature.

**Target users:** Existing sports-ui fans who have accumulated points and have no additional spend vector beyond voting. The system gives high-engagement fans meaningful new utility for their currency and a daily reason to return.

**Problem solved:** The current platform has an engagement ceiling — once a fan casts their votes, there is nothing else to do until the next GM distribution. The Fan Economy removes that ceiling by making points a living currency with multiple spend and earn mechanics, creating persistent engagement between league events.

**The loop architecture is the product.** Each system feeds the others: points fund pack purchases → card pulls power the marketplace and H2H squads → marketplace trading lets fans optimize their squads → strong squads win H2H wagers → H2H wins return points. No system is isolated; depth in one rewards depth in all three.

The rating-to-rarity model is the connective tissue. A 95-overall player card is rare to pull (1% probability) *and* a direct competitive advantage in H2H due to its contribution to team overall rating. Scarcity in the pack system creates real market value and real stakes in competition.

Historical player cards — a new entity separate from the existing Active Roster system — extend the collectible catalog beyond any single season, giving the platform evergreen content that remains engaging during off-seasons.

The H2H MVP uses bot opponents with randomly-generated teams, delivering a live match experience without real-time multiplayer infrastructure. The architecture accommodates real player challenges in a future iteration.

**Currency identity:** Rebranding "votes" to a platform-owned currency name (TBD, analogous to Fortnite's V-Bucks) is a prerequisite for the economy launch. The rebrand elevates the currency from a voting mechanism to a platform asset fans feel ownership over — critical to marketplace and H2H adoption.

**Platform context:** Angular SPA frontend, .NET backend API, brownfield extension. Integrates with current points system, organization model, and user authentication. Complexity is high: probabilistic card generation, real-time auction bidding via SignalR, atomic points escrow, weighted H2H resolution, new CardPlayer entity system.

## Success Criteria

### User Success

- A fan opens their first pack, receives cards of varying rarities, and immediately has a meaningful next action — listing a duplicate on the marketplace or adding a card to their H2H squad. The system delivers a complete loop from first spend to first outcome within a single session.
- A fan who acquires a Legendary card experiences recognizable scarcity value: the card commands a higher marketplace price and meaningfully improves their H2H team rating, making the pull feel earned rather than cosmetic.
- A fan using the marketplace successfully lists a card, receives a real-time bid notification, and either accepts the final auction outcome or uses the Buy Now mechanism — completing a transaction end-to-end without confusion.
- A fan building an H2H squad understands how their team's overall rating affects their odds and enters a match feeling informed about their chances, not surprised by the outcome.

### Business Success

- **Primary signal (3-month):** Points velocity — the ratio of points actively spent (on packs, marketplace bids) vs. points sitting idle increases measurably after Fan Economy launch. Success means points in circulation, not accumulation.
- **Marketplace health:** Auctions are completing (not expiring with zero bids). A healthy marketplace has a bid-to-listing ratio above 1.0 within 60 days of launch.
- **H2H engagement:** Active users play at least one H2H match per week. H2H becomes a regular return driver, not a one-time curiosity.
- **Return visits:** Fan Economy features drive at least one additional session per week per active user compared to pre-launch baseline.
- **Currency rebrand adoption:** Within 30 days of rebrand, users refer to the currency by its new name in any feedback/support channels.

### Technical Success

- **Auction integrity:** Zero instances of a bid being accepted when the bidder has insufficient points. Points are escrowed atomically at bid placement; escrow releases immediately on outbid or auction expiry.
- **Card pull accuracy:** The probability engine is deterministic and auditable — pull rates match configured rarity weights exactly over statistically significant sample sizes.
- **SignalR reliability:** Outbid notifications delivered within 500ms of a competing bid being placed under normal load. Auction state never diverges between two connected clients viewing the same listing.
- **Concurrent auction safety:** Multiple simultaneous active auctions do not interfere with each other's escrow or settlement logic.

### Measurable Outcomes

| Metric | Target | Timeframe |
|---|---|---|
| Points velocity increase | Measurable lift vs. baseline | 30 days post-launch |
| Marketplace bid-to-listing ratio | ≥ 1.0 | 60 days post-launch |
| H2H sessions per active user | ≥ 1 per week | 60 days post-launch |
| Outbid notification latency | < 500ms | At launch |
| Auction integrity failures | 0 | Ongoing |
| Card pull accuracy deviation | < 0.1% from configured rates | At launch |

## User Journeys

### Journey 1: Marcus — The Collector (Primary Happy Path)

Marcus has been on sports-ui for four months. He has 850 points from voting and nothing left to spend them on. The last GM distribution was two weeks ago and the next player option vote isn't for another ten days. He opens the app out of habit, notices **Card Packs** in the nav, and clicks it mostly out of curiosity.

He spends 200 points on a pack. Five cards flip over one by one. Three Commons — fine. One Rare, a historical receiver he vaguely recognizes. Then the fifth card loads differently, taking a beat longer, and when it reveals it pulses gold: **Legendary — 96 Overall**. Marcus didn't expect that. He screenshots it.

Now he has a problem: he already has this player in his H2H squad. The duplicate Legendary is just sitting there. He navigates to the marketplace, lists it with a starting bid of 400 points and a buy now of 700, sets a 24-hour duration. Six hours later his phone buzzes — someone placed a bid of 425. An hour after that, another notification: outbid, now at 580. The auction closes at 620. Marcus has 620 points he didn't have yesterday and is already deciding which pack to open next.

**Capabilities revealed:** Pack purchase flow, rarity-weighted card reveal, card inventory, marketplace listing creation, real-time outbid notifications via SignalR, auction settlement and point credit.

---

### Journey 2: Jordan — The Competitor (Primary H2H Path)

Jordan doesn't care much about collecting. She cares about winning. Over the past three weeks she's been deliberately trading on the marketplace, flipping duplicates and targeting specific high-rated players to build a squad. Her five-card team sits at a 91 overall — she knows it because the H2H squad builder shows her the average as she slots cards in.

She navigates to H2H, reviews her lineup, and sets her wager: 150 points. The system generates a bot opponent — a randomly assembled five-card team. Jordan's squad is shown at 91 overall vs. the bot's 83 overall. Her win probability: **68%**. She confirms the match.

The match plays out live — stats animate, the margin narrows in the third quarter, and Jordan's squad pulls through. She wins 150 points. Her balance updates immediately. She goes straight to the marketplace to look for an upgrade before her next match.

**Capabilities revealed:** H2H squad builder with live overall calculation, wager input and escrow, bot team generation, odds display, live match resolution UI, win/loss point settlement.

---

### Journey 3: DeShawn — The Trader (Marketplace Arbitrage)

DeShawn doesn't buy packs. He watches the marketplace the way a day trader watches a ticker. This afternoon he spots a Rare historical defensive back listed at a 50-point starting bid with only 90 minutes left on the clock — whoever listed it set the duration too short and priced it too low.

He bids 50 points. His balance drops immediately — points escrowed. Eight minutes later: **You've been outbid — current bid is 75 points.** He counter-bids at 95. Three more notifications arrive in the final ten minutes as two other bidders chase the same card. DeShawn bids 130. The auction closes — he wins.

He lists the card the next morning at a buy now of 280 points. It sells within four hours. He nets 150 points on the flip and goes looking for the next mispriced listing. He's never opened a pack in his life.

**Capabilities revealed:** Marketplace search and browse, bid placement with immediate escrow, real-time outbid notification, counter-bidding, buy now execution, auction settlement record.

---

### Journey 4: Coach Rivera — The GM (Admin Path)

Coach Rivera runs the NFL league on sports-ui. A fan messaged him asking why Jerry Rice wasn't in the card pool — fair point, he's an all-time legend and should have been there from the start.

Rivera opens the Card Player admin panel. He creates a new Historical Player entry — name, position, league (NFL), and sets the overall rating at **97**. The system reads the rating and automatically flags the card as **Legendary** with a 1% pull rate. He saves it. Jerry Rice is now in the pack pool for every NFL league member.

A week later Rivera checks the economy dashboard — Rice has appeared in 12 pack pulls, four of which have been listed on the marketplace. The average sale price is sitting at 680 points. He can see which cards are moving, which are stagnant, and whether anyone is hoarding a single card type. Nothing looks broken, so he moves on.

**Capabilities revealed:** CardPlayer admin — create/edit historical players with rating, automatic rarity assignment from rating tier, league-scoped catalog management, economy monitoring dashboard (cards in circulation, marketplace volume by card, listing price trends).

---

### Journey Requirements Summary

| Capability Area | Revealed By |
|---|---|
| Pack purchase + rarity-weighted reveal | Marcus, Jordan |
| Card inventory / collection management | Marcus, Jordan |
| Marketplace listing (start bid / buy now / duration) | Marcus, DeShawn |
| Real-time outbid notifications (SignalR) | Marcus, DeShawn |
| Points escrow on bid + release on outbid/expiry | DeShawn |
| Auction settlement + point credit | Marcus, DeShawn |
| H2H squad builder with overall calculation | Jordan |
| Bot team generation (random cards, computed overall) | Jordan |
| Odds display (based on team overall delta) | Jordan |
| Live match resolution UI | Jordan |
| Wager escrow + win/loss settlement | Jordan |
| CardPlayer admin (historical + active, rating input) | Coach Rivera |
| Automatic rarity assignment from rating | Coach Rivera |
| League-scoped card catalog | Coach Rivera |
| Economy monitoring dashboard | Coach Rivera |

## Domain-Specific Requirements

### Virtual Economy Constraints

- **Points are platform-internal only.** Points are never convertible to real money, gift cards, or external value. This keeps the system outside money transmission and gambling regulations.
- **Pull rate opacity is intentional.** Pack rarity probabilities are NOT displayed to users. The surprise and discovery mechanic is a deliberate product decision. Internally, rates must be auditable and accurate — the decision not to publish them does not reduce the engineering obligation to implement them correctly.
- **Transaction audit trail required.** Every point movement — pack purchase, bid escrow, bid release, auction settlement, H2H wager, H2H payout — must be recorded with a timestamp, user ID, and action type. This is the primary mechanism for resolving user disputes.

### Risk Mitigations

- **Escrow integrity:** Points must be locked atomically on bid placement. A race condition that allows two bids from the same user simultaneously — or a bid from a user without sufficient balance — is a critical defect.
- **Bot fairness:** The H2H bot's randomly generated team must draw from the same card pool available to real users, using the same rarity weights. The bot cannot be seeded with cards that don't exist in the real catalog.
- **Auction manipulation:** A user must not be able to bid on their own listing. The system must validate that the bidder and the listing owner are different accounts.

## Innovation & Novel Patterns

### Detected Innovation Areas

**1. Closed-Loop Fan Economy Architecture**
Most sports fan platforms treat engagement features (voting, collectibles, games) as isolated modules. The Fan Economy is built around a single closed loop: points fund packs → cards power the marketplace and H2H → marketplace flips and H2H wins return points. Depth in one system is automatically rewarded by the others.

**2. Rating-as-Rarity Connective Tissue**
A player's overall rating directly determines their card's rarity tier, which simultaneously governs: pull probability in packs, market value expectations, and H2H team strength. One numerical attribute drives three separate systems — creating coherence without coupling.

**3. Bot-First Competitive Play**
The MVP delivers a live match experience against a bot opponent seeded from the real card pool using the same rarity weights. This solves cold-start and infrastructure complexity while preserving the full engagement loop. Real player challenges become a drop-in Phase 2 upgrade.

**4. Currency Identity as Platform Asset**
Rebranding "votes" to a named platform currency is not cosmetic — it's an identity shift that makes the currency a platform asset fans feel ownership over, a prerequisite for marketplace and H2H adoption psychology.

### Market Context & Competitive Landscape

Direct competitors (Sorare, NBA Top Shot, Fanatics Collect) operate on real-money markets with licensed IP. The Fan Economy deliberately avoids real-money conversion, keeping the system within platform-internal points — legally simpler while delivering the same psychological loop as FIFA Ultimate Team, applied to a community sports platform.

### Validation Approach

- **Loop closure test (30 days):** Session depth after first pack purchase — does a fan have a meaningful next action within the same session?
- **Rating-to-value correlation (60 days):** Do higher-rated cards command proportionally higher marketplace prices?
- **H2H bot satisfaction (60 days):** Do users who play against the bot return for a second match?

### Innovation Risk Mitigation

| Risk | Mitigation |
|---|---|
| Bot opponent feels trivial | Weighted odds by team overall rating — high-rated squads have real variance, not guaranteed wins |
| Rating-to-rarity mapping feels arbitrary | Publish rarity tier thresholds in-product (e.g., "95+ = Legendary") |
| Currency rebrand rejected | Phase the rebrand — display both labels temporarily, then fully transition |
| Marketplace liquidity fails | GM seeds initial listings; bonus pack drops at launch create early supply |

## Technical Platform Requirements

### Architecture

- Angular standalone components with signals; new feature libraries follow the existing Nx monorepo structure (`libs/cards/`, `libs/marketplace/`, `libs/h2h/`)
- Lazy-loaded routes — initial bundle size unaffected by new features
- All new features delivered within the existing shell — no separate deployment

### Real-Time (SignalR)

- SignalR hub for auction bidding: outbid events, bid accepted events, auction expiry countdown
- Connection scoped per active auction listing — clients only receive events for listings they are viewing or have bid on
- Graceful degradation if SignalR drops: reconnect with last-known state; no stale data displayed

### Browser & Platform

- Modern evergreen browsers only (Chrome, Firefox, Safari, Edge — latest two major versions)
- Responsive design required — marketplace bidding and H2H squad building must be usable on mobile screen sizes
- All Fan Economy content is auth-gated; SEO not applicable in MVP scope

## Project Scoping & Phased Development

### MVP Strategy

**Approach:** Experience MVP — all three systems (Card Packs, Marketplace, H2H) ship simultaneously. The loop is the value proposition; any single system alone fails the engagement test. Ship the full loop at minimum depth rather than one system at maximum depth.

### MVP Feature Set (Phase 1)

**Core journeys supported:** Marcus (Collector), Jordan (Competitor), DeShawn (Trader), Coach Rivera (GM Admin)

| Capability | Rationale |
|---|---|
| Currency rebrand (votes → points) | Prerequisite for economy identity |
| CardPlayer entity (historical + active, with ratings) | Card pool must exist before packs open |
| Automatic rarity assignment from rating tier | Connective tissue — cannot decouple |
| Card Pack purchase (5 cards, rarity-weighted) | Economy entry point |
| Card inventory / collection view | Fans must see what they've pulled |
| Auction Marketplace — list, bid, buy now, duration | Core trading surface |
| Real-time outbid push via SignalR | Auctions unusable without it |
| Points escrow on bid + atomic release | Non-negotiable for integrity |
| H2H squad builder (5-card, live overall calculation) | Core competitive surface |
| H2H bot opponent (random team from real card pool) | Avoids multiplayer infrastructure |
| Wager escrow + win/loss point settlement | Closes the H2H loop |
| Transaction audit log | Dispute resolution mechanism |
| GM admin panel (CardPlayer CRUD, economy dashboard) | Content and health management |

### Phase 2 (Growth)

- Real player H2H challenges — accept/decline flow, live match between two real users
- H2H leaderboard — ranked standings by win rate or points won
- Pack opening animations — visual card reveal (peel, shine effects by rarity)
- Direct card trading — peer-to-peer swaps outside the auction marketplace
- Card collection viewer — full inventory browser with filter by rarity, player, league

### Phase 3 (Expansion)

- Cross-league card trading — marketplace visible across different leagues
- Mobile push notifications — outbid and auction expiry alerts outside the browser
- Card crafting/burning — combine or sacrifice cards for higher-rarity cards
- Pack variety — position-locked packs, guaranteed-rarity packs
- Historical card sets — curated limited-edition drops tied to real-world sports moments

### Scoping Risk Mitigation

**Technical:** SignalR + escrow atomicity under concurrent load is highest risk. Build escrow as a backend transaction (not optimistic UI); load test concurrent auctions before launch. Rarity engine: unit-tested with deterministic seed; audit log captures every pull.

**Market:** Marketplace liquidity risk — GM seeds initial listings; bonus packs at launch create supply. H2H bot satisfaction risk — bot is explicitly MVP; real player H2H ships in Phase 2.

**Resource fallback:** If scope must cut, priority order: (1) keep Packs + Marketplace, defer H2H; (2) Packs only as absolute minimum — note this breaks the loop and significantly reduces launch impact.

## Functional Requirements

### Currency & Points Management

- **FR1:** A fan can view their current points balance displayed under the platform's branded currency name
- **FR2:** A fan's points balance reflects all transactions (purchases, escrow holds, escrow releases, auction settlements, H2H payouts) without requiring a manual refresh
- **FR3:** The system records every point movement with timestamp, user ID, action type, and associated entity (pack ID, listing ID, match ID)
- **FR4:** Points cannot be converted to real money, gift cards, or any external value

### Card Catalog Administration

- **FR5:** A GM can create a CardPlayer entry with name, position, league, and overall rating
- **FR6:** A GM can edit an existing CardPlayer's name, position, and overall rating
- **FR7:** The system automatically assigns a rarity tier to a CardPlayer based on configured overall rating thresholds
- **FR8:** CardPlayer entries are scoped to a specific league and are only available in packs for that league
- **FR9:** A GM can view all CardPlayer entries for their league
- **FR10:** Historical player CardPlayer entries exist independently of the Active Roster system and persist across seasons

### Card Pack System

- **FR11:** A fan can purchase a card pack by spending points
- **FR12:** Each pack purchase produces exactly 5 cards drawn from the fan's league CardPlayer catalog
- **FR13:** Card rarity distribution per pack is governed by configured probability weights per rarity tier
- **FR14:** A fan receives their 5 pulled cards immediately upon successful pack purchase
- **FR15:** Points are deducted from the fan's balance atomically at the moment of pack purchase
- **FR16:** A fan cannot view the configured rarity pull probabilities for any pack type

### Card Collection

- **FR17:** A fan can view all cards they currently own
- **FR18:** A fan can view individual card details including player name, overall rating, rarity tier, and league

### Auction Marketplace

- **FR19:** A fan can list a card they own for auction with a starting bid price, an optional buy now price, and a chosen auction duration
- **FR20:** A listed card is removed from the seller's usable collection for the duration of the auction
- **FR21:** A fan can browse active auction listings in the marketplace
- **FR22:** A fan can place a bid on an active auction listing
- **FR23:** Points equal to the bid amount are escrowed atomically when a fan places a bid
- **FR24:** A fan's escrowed points are released immediately when they are outbid by another user
- **FR25:** A fan can purchase a listed card immediately at the buy now price, ending the auction
- **FR26:** The winning bidder at auction expiry receives the card; the seller receives the final bid amount in points
- **FR27:** A fan cannot bid on an auction listing they created
- **FR28:** A fan cannot place a bid if their available (non-escrowed) points balance is insufficient to cover the bid

### Real-Time Auction Notifications

- **FR29:** A fan receives an in-app notification when they have been outbid on an active auction
- **FR30:** A fan viewing an active listing sees the current highest bid updated in real-time without refreshing
- **FR31:** Auction state (current bid, time remaining) is consistent across all clients viewing the same listing simultaneously

### Head-to-Head Competition

- **FR32:** A fan can select up to 5 cards from their collection to form an H2H squad
- **FR33:** The squad builder displays the team's overall rating calculated from the selected cards
- **FR34:** A fan can set a points wager amount before confirming an H2H match
- **FR35:** Wagered points are escrowed when the fan confirms the match
- **FR36:** The system generates a bot opponent team assembled randomly from the real CardPlayer catalog using the same rarity weights available to all users
- **FR37:** The match outcome is determined by weighted probability based on the overall rating differential between the two teams
- **FR38:** The fan experiences a live match resolution sequence before the outcome is revealed
- **FR39:** The winning side's wagered points are transferred to the winner; the loser forfeits their wagered amount
- **FR40:** A fan's points balance is updated immediately upon match resolution

### Economy Administration & Monitoring

- **FR41:** A GM can view a dashboard showing cards in circulation, marketplace listing volume by card, and average sale price trends
- **FR42:** A GM can identify which cards are currently active on the marketplace and how frequently each card has appeared in pack pulls

## Non-Functional Requirements

### Performance

- **NFR-P1:** Outbid notifications delivered to affected clients within 500ms of a competing bid being placed, under normal operating load
- **NFR-P2:** Card pull results returned to the fan within 2 seconds of pack purchase confirmation
- **NFR-P3:** Marketplace listing pages reach interactive state within 3 seconds on standard broadband
- **NFR-P4:** H2H match resolution animation runs entirely client-side — no server round-trip after wager is confirmed and match outcome is received

### Security

- **NFR-S1:** All point transaction operations (escrow, release, debit, credit) are validated and executed server-side; the client cannot assert or modify balance or escrow state
- **NFR-S2:** Self-bid prevention enforced server-side — the API rejects any bid where the bidder's user ID matches the listing owner's user ID
- **NFR-S3:** Rarity probability weights are server-side configuration only and are not exposed in any API response or client-accessible resource
- **NFR-S4:** All Fan Economy endpoints (packs, marketplace, H2H, admin) require a valid authenticated session

### Reliability

- **NFR-R1:** Zero escrow integrity failures — no auction may settle with a losing bidder's points remaining locked, and no auction may complete without transferring the card and crediting the seller
- **NFR-R2:** Card pull accuracy must not deviate more than 0.1% from configured rarity rates across statistically significant sample sizes
- **NFR-R3:** Concurrent active auctions must not share or interfere with each other's escrow state or settlement logic
- **NFR-R4:** If a client's SignalR connection drops during an active auction, reconnection automatically re-fetches current auction state — client never displays stale bid data

### Scalability

- **NFR-SC1:** Escrow and notification systems support multiple simultaneously active auctions without degradation in per-auction response time or event delivery
- **NFR-SC2:** Match and wager resolution logic accommodates real-player H2H (Phase 2) without re-architecting the core settlement flow

### Integration

- **NFR-I1:** The existing points balance data structure is extended to support escrow tracking — escrow is a field/record on the existing account model, not a separate system
- **NFR-I2:** SignalR hub connections are scoped per auction listing — a client connected to listing A does not receive events for listing B
- **NFR-I3:** The CardPlayer entity has no foreign-key dependency on the existing Player Active Roster entity — independent catalog systems that may reference the same player by name
