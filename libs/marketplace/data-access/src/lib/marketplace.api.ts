import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@sports-ui/http-client';
import { ServiceResponse } from '@sports-ui/api-types';
import { environment } from '@sports-ui/api-types';
import {
  ListingDto,
  ListingDetailDto,
  PaginatedListings,
  CreateListingRequest,
  PlaceBidRequest,
  BuyNowRequest,
} from './marketplace.model';

@Injectable({ providedIn: 'root' })
export class MarketplaceApi {
  private readonly http = inject(ApiService);

  getListings(orgId: string, page = 1, pageSize = 20): Observable<ServiceResponse<PaginatedListings>> {
    return this.http.get<ServiceResponse<PaginatedListings>>(
      `${environment.sportsApi}cards/listings?orgId=${orgId}&page=${page}&pageSize=${pageSize}`
    );
  }

  getListingDetail(listingId: string): Observable<ServiceResponse<ListingDetailDto>> {
    return this.http.get<ServiceResponse<ListingDetailDto>>(
      `${environment.sportsApi}cards/listings/${listingId}`
    );
  }

  createListing(request: CreateListingRequest): Observable<ServiceResponse<ListingDto>> {
    return this.http.post<ServiceResponse<ListingDto>, CreateListingRequest>(
      `${environment.sportsApi}cards/listings`,
      request
    );
  }

  placeBid(listingId: string, request: PlaceBidRequest): Observable<ServiceResponse<boolean>> {
    return this.http.post<ServiceResponse<boolean>, PlaceBidRequest>(
      `${environment.sportsApi}cards/listings/${listingId}/bids`,
      request
    );
  }

  buyNow(listingId: string, request: BuyNowRequest): Observable<ServiceResponse<boolean>> {
    return this.http.post<ServiceResponse<boolean>, BuyNowRequest>(
      `${environment.sportsApi}cards/listings/${listingId}/buy-now`,
      request
    );
  }

  getAvailableBalance(userId: string, orgId: string): Observable<ServiceResponse<number>> {
    return this.http.get<ServiceResponse<number>>(
      `${environment.sportsApi}cards/listings/available-balance?userId=${userId}&orgId=${orgId}`
    );
  }
}
