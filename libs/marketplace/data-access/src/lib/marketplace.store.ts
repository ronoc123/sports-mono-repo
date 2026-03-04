import { computed } from '@angular/core';
import {
  signalStore,
  withState,
  withMethods,
  withComputed,
  patchState,
} from '@ngrx/signals';
import { ListingDto, ListingDetailDto, BidPlacedEvent, OutbidNotificationEvent, AuctionSettledEvent } from './marketplace.model';

export type MarketplaceStatus = 'idle' | 'loading' | 'success' | 'error';

export interface MarketplaceState {
  listings: ListingDto[];
  selectedListing: ListingDetailDto | null;
  availableBalance: number;
  listingsStatus: MarketplaceStatus;
  detailStatus: MarketplaceStatus;
  bidStatus: MarketplaceStatus;
  listStatus: MarketplaceStatus;
  error: string | null;
  bidError: string | null;
  listError: string | null;
  totalCount: number;
  page: number;
  pageSize: number;
  lastOutbid: OutbidNotificationEvent | null;
  lastSettled: AuctionSettledEvent | null;
}

const initialState: MarketplaceState = {
  listings: [],
  selectedListing: null,
  availableBalance: 0,
  listingsStatus: 'idle',
  detailStatus: 'idle',
  bidStatus: 'idle',
  listStatus: 'idle',
  error: null,
  bidError: null,
  listError: null,
  totalCount: 0,
  page: 1,
  pageSize: 20,
  lastOutbid: null,
  lastSettled: null,
};

export const MarketplaceStore = signalStore(
  { providedIn: 'root' },
  withState<MarketplaceState>(initialState),

  withComputed((state) => ({
    isLoadingListings: computed(() => state.listingsStatus() === 'loading'),
    isLoadingDetail: computed(() => state.detailStatus() === 'loading'),
    isBidding: computed(() => state.bidStatus() === 'loading'),
    isListing: computed(() => state.listStatus() === 'loading'),
    hasListings: computed(() => state.listings().length > 0),
    totalPages: computed(() => Math.ceil(state.totalCount() / state.pageSize())),
  })),

  withMethods((store) => ({
    // ── Listings ────────────────────────────────────────────────────────────
    setListingsLoading() {
      patchState(store, { listingsStatus: 'loading', error: null });
    },
    setListings(listings: ListingDto[], totalCount: number, page: number) {
      patchState(store, { listings, totalCount, page, listingsStatus: 'success' });
    },
    setListingsError(error: string) {
      patchState(store, { error, listingsStatus: 'error' });
    },

    // ── Detail ──────────────────────────────────────────────────────────────
    setDetailLoading() {
      patchState(store, { detailStatus: 'loading', error: null });
    },
    setSelectedListing(listing: ListingDetailDto) {
      patchState(store, { selectedListing: listing, detailStatus: 'success' });
    },
    setDetailError(error: string) {
      patchState(store, { error, detailStatus: 'error' });
    },
    clearSelectedListing() {
      patchState(store, { selectedListing: null, detailStatus: 'idle' });
    },

    // ── Bid ─────────────────────────────────────────────────────────────────
    setBidding() {
      patchState(store, { bidStatus: 'loading', bidError: null });
    },
    setBidSuccess() {
      patchState(store, { bidStatus: 'success' });
    },
    setBidError(bidError: string) {
      patchState(store, { bidError, bidStatus: 'error' });
    },
    resetBidState() {
      patchState(store, { bidStatus: 'idle', bidError: null });
    },

    // ── Create listing ──────────────────────────────────────────────────────
    setListing() {
      patchState(store, { listStatus: 'loading', listError: null });
    },
    setListSuccess() {
      patchState(store, { listStatus: 'success' });
    },
    setListError(listError: string) {
      patchState(store, { listError, listStatus: 'error' });
    },
    resetListState() {
      patchState(store, { listStatus: 'idle', listError: null });
    },

    // ── Balance ─────────────────────────────────────────────────────────────
    setAvailableBalance(availableBalance: number) {
      patchState(store, { availableBalance });
    },

    // ── SignalR live updates ─────────────────────────────────────────────────
    applyBidPlaced(event: BidPlacedEvent) {
      // Update current bid in the listings list
      const updatedListings = store.listings().map((l) =>
        l.id === event.listingId ? { ...l, currentBid: event.currentBid } : l
      );
      patchState(store, { listings: updatedListings });

      // Update current bid in the selected listing detail if open
      const selected = store.selectedListing();
      if (selected && selected.id === event.listingId) {
        const newBid = {
          bidderId: event.bidderId,
          amount: event.currentBid,
          timestamp: event.timestamp,
        };
        patchState(store, {
          selectedListing: {
            ...selected,
            currentBid: event.currentBid,
            bidHistory: [newBid, ...selected.bidHistory],
          },
        });
      }
    },

    applyOutbid(event: OutbidNotificationEvent) {
      patchState(store, { lastOutbid: event });
    },

    applyAuctionSettled(event: AuctionSettledEvent) {
      patchState(store, {
        lastSettled: event,
        listings: store.listings().filter((l) => l.id !== event.listingId),
      });
    },

    clearNotifications() {
      patchState(store, { lastOutbid: null, lastSettled: null });
    },
  }))
);
