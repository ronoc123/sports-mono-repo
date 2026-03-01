import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@sports-ui/http-client';
import { ServiceResponse } from '@sports-ui/api-types';
import { environment } from '@sports-ui/api-types';
import {
  CardPlayer,
  CardOwner,
  UserCard,
  CreateCardPlayerRequest,
  UpdateCardPlayerRequest,
  PackPurchaseRequest,
  PackPurchaseResult,
} from './cards.model';

@Injectable({ providedIn: 'root' })
export class CardsApi {
  private readonly http = inject(ApiService);

  getCardPlayers(leagueId: string): Observable<ServiceResponse<CardPlayer[]>> {
    return this.http.get<ServiceResponse<CardPlayer[]>>(
      `${environment.sportsApi}cards/players?leagueId=${leagueId}`
    );
  }

  createCardPlayer(
    request: CreateCardPlayerRequest
  ): Observable<ServiceResponse<CardPlayer>> {
    return this.http.post<ServiceResponse<CardPlayer>, CreateCardPlayerRequest>(
      `${environment.sportsApi}cards/players`,
      request
    );
  }

  updateCardPlayer(
    id: string,
    request: UpdateCardPlayerRequest
  ): Observable<ServiceResponse<CardPlayer>> {
    return this.http.put<ServiceResponse<CardPlayer>, UpdateCardPlayerRequest>(
      `${environment.sportsApi}cards/players/${id}`,
      request
    );
  }

  purchasePack(
    request: PackPurchaseRequest
  ): Observable<ServiceResponse<PackPurchaseResult>> {
    return this.http.post<ServiceResponse<PackPurchaseResult>, PackPurchaseRequest>(
      `${environment.sportsApi}cards/packs/purchase`,
      request
    );
  }

  getCollection(
    userId: string,
    orgId: string
  ): Observable<ServiceResponse<UserCard[]>> {
    return this.http.get<ServiceResponse<UserCard[]>>(
      `${environment.sportsApi}cards/collection?userId=${userId}&orgId=${orgId}`
    );
  }
}
