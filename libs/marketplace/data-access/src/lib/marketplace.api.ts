import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@sports-ui/http-client';
import { ServiceResponse } from '@sports-ui/api-types';
import { environment } from '@sports-ui/api-types';
import {
  ListingDto,
  ListingDetailDto,
  CreateListingRequest,
  PlaceBidRequest,
  BuyNowRequest,
} from './marketplace.model';

@Injectable({ providedIn: 'root' })
export class MarketplaceApi {
  private readonly http = inject(ApiService);

  getListings(leagueId: string, page = 1, pageSize = 20): Observable<ServiceResponse<ListingDto[]>> {
    return this.http.get<ServiceResponse<ListingDto[]>>(
      `${environment.sportsApi}cards/listings?leagueId=${leagueId}&page=${page}&pageSize=${pageSize}`
    );
  }

  getListingDetail(listingId: string): Observable<ServiceResponse<ListingDetailDto>> {
    return this.http.get<ServiceResponse<ListingDetailDto>>(
      `${environment.sportsApi}cards/listings/${listingId}`
    );
  }

  createListing(request: CreateListingRequest): Observable<ServiceResponse<string>> {
    return this.http.post<ServiceResponse<string>, CreateListingRequest>(
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

  getAvailableBalance(userId: string, leagueId: string): Observable<ServiceResponse<number>> {
    return this.http.get<ServiceResponse<number>>(
      `${environment.sportsApi}cards/listings/available-balance?userId=${userId}&leagueId=${leagueId}`
    );
  }
}
