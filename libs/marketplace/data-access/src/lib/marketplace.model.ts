import { RarityTier } from '@sports-ui/cards-data-access';

// ── Listing models ───────────────────────────────────────────────────────────

export interface ListingDto {
  id: string;
  cardOwnerId: string;
  sellerId: string;
  orgId: string;
  playerName: string;
  position: string;
  overallRating: number;
  rarityTier: RarityTier;
  startingBid: number;
  currentBid: number;
  buyNowPrice: number | null;
  expiresAt: string;
  bidCount: number;
}

export interface BidDto {
  bidderId: string;
  amount: number;
  timestamp: string;
}

export interface ListingDetailDto extends ListingDto {
  bidHistory: BidDto[];
}

// ── Request models ────────────────────────────────────────────────────────────

export interface CreateListingRequest {
  cardOwnerId: string;
  sellerId: string;
  orgId: string;
  startingBid: number;
  buyNowPrice?: number;
  durationHours: number;
}

export interface PlaceBidRequest {
  bidderId: string;
  orgId: string;
  amount: number;
}

export interface BuyNowRequest {
  buyerId: string;
  orgId: string;
}

// ── Paginated response ───────────────────────────────────────────────────────

export interface PaginatedListings {
  items: ListingDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// ── SignalR event payloads ───────────────────────────────────────────────────

export interface BidPlacedEvent {
  listingId: string;
  currentBid: number;
  bidderId: string;
  timestamp: string;
}

export interface OutbidNotificationEvent {
  listingId: string;
  releasedAmount: number;
}

export interface AuctionSettledEvent {
  listingId: string;
  winnerId: string | null;
  finalBid: number;
}
