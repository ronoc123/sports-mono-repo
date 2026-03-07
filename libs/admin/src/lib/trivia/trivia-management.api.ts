import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@sports-ui/http-client';
import { ServiceResponse } from '@sports-ui/api-types';
import { environment } from '@sports-ui/api-types';
import {
  AddTriviaQuestionRequest,
  CreateTriviaSeriesRequest,
  TriviaSeriesDto,
} from './trivia-management.model';

@Injectable({ providedIn: 'root' })
export class TriviaManagementApi {
  private readonly http = inject(ApiService);

  getSeries(organizationId: string): Observable<ServiceResponse<TriviaSeriesDto[]>> {
    return this.http.get<ServiceResponse<TriviaSeriesDto[]>>(
      `${environment.sportsApi}trivia/${organizationId}`
    );
  }

  createSeries(
    body: CreateTriviaSeriesRequest
  ): Observable<ServiceResponse<{ seriesId: string; seriesName: string }>> {
    return this.http.post<
      ServiceResponse<{ seriesId: string; seriesName: string }>,
      CreateTriviaSeriesRequest
    >(`${environment.sportsApi}trivia/series`, body);
  }

  addQuestion(
    body: AddTriviaQuestionRequest
  ): Observable<ServiceResponse<{ questionId: string }>> {
    return this.http.post<
      ServiceResponse<{ questionId: string }>,
      AddTriviaQuestionRequest
    >(`${environment.sportsApi}trivia/questions`, body);
  }

  publishQuestion(questionId: string): Observable<ServiceResponse<boolean>> {
    return this.http.put<ServiceResponse<boolean>, Record<string, never>>(
      `${environment.sportsApi}trivia/questions/${questionId}/publish`,
      {}
    );
  }

  archiveQuestion(questionId: string): Observable<ServiceResponse<boolean>> {
    return this.http.put<ServiceResponse<boolean>, Record<string, never>>(
      `${environment.sportsApi}trivia/questions/${questionId}/archive`,
      {}
    );
  }
}
