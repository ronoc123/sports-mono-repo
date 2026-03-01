import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { MarketplaceStore } from './marketplace.store';
import { MarketplaceApi } from './marketplace.api';
import { MarketplaceSignalRService } from './marketplace.signalr.service';
import { CreateListingRequest, PlaceBidRequest, BuyNowRequest } from './marketplace.model';

@Injectable({ providedIn: 'root' })
export class MarketplaceFacade {
  private readonly store = inject(MarketplaceStore);
  private readonly api = inject(MarketplaceApi);
  private readonly signalR = inject(MarketplaceSignalRService);

  // ── Exposed signals ────────────────────────────────────────────────────────
  readonly listings = this.store.listings;
  readonly selectedListing = this.store.selectedListing;
  readonly availableBalance = this.store.availableBalance;
  readonly isLoadingListings = this.store.isLoadingListings;
  readonly isLoadingDetail = this.store.isLoadingDetail;
  readonly isBidding = this.store.isBidding;
  readonly isListing = this.store.isListing;
  readonly error = this.store.error;
  readonly bidError = this.store.bidError;
  readonly listError = this.store.listError;
  readonly bidStatus = this.store.bidStatus;
  readonly listStatus = this.store.listStatus;
  readonly totalCount = this.store.totalCount;
  readonly page = this.store.page;
  readonly totalPages = this.store.totalPages;
  readonly lastOutbid = this.store.lastOutbid;
  readonly lastSettled = this.store.lastSettled;

  // ── SignalR observables ────────────────────────────────────────────────────
  readonly bidPlaced$ = this.signalR.bidPlaced$;
  readonly outbid$ = this.signalR.outbid$;
  readonly auctionSettled$ = this.signalR.auctionSettled$;

  // ── SignalR lifecycle ──────────────────────────────────────────────────────
  async connectSignalR(): Promise<void> {
    await this.signalR.connect();

    this.signalR.bidPlaced$.subscribe((event) => {
      this.store.applyBidPlaced(event);
    });

    this.signalR.outbid$.subscribe((event) => {
      this.store.applyOutbid(event);
    });

    this.signalR.auctionSettled$.subscribe((event) => {
      this.store.applyAuctionSettled(event);
    });
  }

  async disconnectSignalR(): Promise<void> {
    await this.signalR.disconnect();
  }

  async joinListing(listingId: string): Promise<void> {
    await this.signalR.joinListing(listingId);
  }

  async leaveListing(listingId: string): Promise<void> {
    await this.signalR.leaveListing(listingId);
  }

  // ── Data methods ───────────────────────────────────────────────────────────
  async loadListings(orgId: string, page = 1, pageSize = 20): Promise<void> {
    this.store.setListingsLoading();
    try {
      const res = await firstValueFrom(this.api.getListings(orgId, page, pageSize));
      const data = res.data;
      if (data) {
        this.store.setListings(data.items, data.totalCount, data.page);
      } else {
        this.store.setListings([], 0, 1);
      }
    } catch {
      this.store.setListingsError('Failed to load listings');
    }
  }

  async loadListingDetail(listingId: string): Promise<void> {
    this.store.setDetailLoading();
    try {
      const res = await firstValueFrom(this.api.getListingDetail(listingId));
      if (res.data) this.store.setSelectedListing(res.data);
      else this.store.setDetailError('Listing not found');
    } catch {
      this.store.setDetailError('Failed to load listing detail');
    }
  }

  clearSelectedListing(): void {
    this.store.clearSelectedListing();
  }

  async createListing(request: CreateListingRequest): Promise<boolean> {
    this.store.setListing();
    try {
      const res = await firstValueFrom(this.api.createListing(request));
      if (res.data) {
        this.store.setListSuccess(res.data);
        return true;
      }
      this.store.setListError(res.message ?? 'Failed to create listing');
      return false;
    } catch (err: any) {
      this.store.setListError(err?.error?.message ?? err?.message ?? 'Failed to create listing');
      return false;
    }
  }

  async placeBid(listingId: string, request: PlaceBidRequest): Promise<boolean> {
    this.store.setBidding();
    try {
      await firstValueFrom(this.api.placeBid(listingId, request));
      this.store.setBidSuccess();
      return true;
    } catch (err: any) {
      this.store.setBidError(err?.error?.message ?? err?.message ?? 'Failed to place bid');
      return false;
    }
  }

  async buyNow(listingId: string, request: BuyNowRequest): Promise<boolean> {
    this.store.setBidding();
    try {
      await firstValueFrom(this.api.buyNow(listingId, request));
      this.store.setBidSuccess();
      return true;
    } catch (err: any) {
      this.store.setBidError(err?.error?.message ?? err?.message ?? 'Failed to buy now');
      return false;
    }
  }

  async loadAvailableBalance(userId: string, orgId: string): Promise<void> {
    try {
      const res = await firstValueFrom(this.api.getAvailableBalance(userId, orgId));
      if (res.data != null) this.store.setAvailableBalance(res.data);
    } catch {
      // non-critical — balance will just show 0
    }
  }

  resetBidState(): void {
    this.store.resetBidState();
  }

  resetListState(): void {
    this.store.resetListState();
  }

  clearNotifications(): void {
    this.store.clearNotifications();
  }
}
