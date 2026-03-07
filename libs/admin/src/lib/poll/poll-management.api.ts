import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@sports-ui/http-client';
import { ServiceResponse } from '@sports-ui/api-types';
import { environment } from '@sports-ui/api-types';
import { CreatePollRequest, PollDto } from './poll-management.model';

@Injectable({ providedIn: 'root' })
export class PollManagementApi {
  private readonly http = inject(ApiService);

  getPolls(organizationId: string): Observable<ServiceResponse<PollDto[]>> {
    return this.http.get<ServiceResponse<PollDto[]>>(
      `${environment.sportsApi}poll?organizationId=${organizationId}`
    );
  }

  createPoll(
    body: CreatePollRequest
  ): Observable<ServiceResponse<{ pollId: string; optionIds: string[] }>> {
    return this.http.post<
      ServiceResponse<{ pollId: string; optionIds: string[] }>,
      CreatePollRequest
    >(`${environment.sportsApi}poll`, body);
  }

  archivePoll(pollId: string): Observable<ServiceResponse<boolean>> {
    return this.http.put<ServiceResponse<boolean>, Record<string, never>>(
      `${environment.sportsApi}poll/${pollId}/archive`,
      {}
    );
  }
}
