import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@sports-ui/http-client';
import { ServiceResponse } from '@sports-ui/api-types';
import { environment } from '@sports-ui/api-types';
import {
  DashboardResponse,
  SubmitTriviaAnswerRequest,
  SubmitTriviaAnswerResult,
  SubmitPollVoteRequest,
  SubmitPollVoteResult,
} from '../dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(ApiService);

  getDashboard(
    organizationId: string,
    userId: string
  ): Observable<ServiceResponse<DashboardResponse>> {
    return this.http.get<ServiceResponse<DashboardResponse>>(
      `${environment.sportsApi}dashboard/${organizationId}?userId=${userId}`
    );
  }

  submitTriviaAnswer(
    body: SubmitTriviaAnswerRequest
  ): Observable<ServiceResponse<SubmitTriviaAnswerResult>> {
    return this.http.post<
      ServiceResponse<SubmitTriviaAnswerResult>,
      SubmitTriviaAnswerRequest
    >(`${environment.sportsApi}trivia/answer`, body);
  }

  submitPollVote(
    body: SubmitPollVoteRequest
  ): Observable<ServiceResponse<SubmitPollVoteResult>> {
    return this.http.post<
      ServiceResponse<SubmitPollVoteResult>,
      SubmitPollVoteRequest
    >(`${environment.sportsApi}poll/vote`, body);
  }
}
