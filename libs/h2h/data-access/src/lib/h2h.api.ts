import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@sports-ui/http-client';
import { ServiceResponse } from '@sports-ui/api-types';
import { environment } from '@sports-ui/api-types';
import {
  CreateMatchRequest,
  CreateMatchResult,
  MatchDetailDto,
  MatchHistoryResult,
} from './h2h.model';

@Injectable({ providedIn: 'root' })
export class H2HApi {
  private readonly http = inject(ApiService);

  createMatch(request: CreateMatchRequest): Observable<ServiceResponse<CreateMatchResult>> {
    return this.http.post<ServiceResponse<CreateMatchResult>, CreateMatchRequest>(
      `${environment.sportsApi}h2h/matches`,
      request
    );
  }

  getMatchDetail(matchId: string): Observable<MatchDetailDto> {
    return this.http.get<MatchDetailDto>(
      `${environment.sportsApi}h2h/matches/${matchId}`
    );
  }

  getMatchHistory(
    fanUserId: string,
    orgId: string,
    page = 1,
    pageSize = 20
  ): Observable<MatchHistoryResult> {
    return this.http.get<MatchHistoryResult>(
      `${environment.sportsApi}h2h/matches?fanUserId=${fanUserId}&orgId=${orgId}&page=${page}&pageSize=${pageSize}`
    );
  }
}
