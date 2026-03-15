import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_URL } from '@sports-ui/http-client';
import {
  BacktestSession,
  BacktestStatus,
  SubmitMetricsRequest,
  AnswerInput,
} from './backtest.models';

interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class BacktestApi {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);

  private get base() { return `${this.apiUrl}/backtest`; }

  createSession(): Observable<BacktestSession> {
    return this.http
      .post<ServiceResponse<BacktestSession>>(`${this.base}/session`, {})
      .pipe(map(r => r.data));
  }

  getStatus(): Observable<BacktestStatus> {
    return this.http
      .get<ServiceResponse<BacktestStatus>>(`${this.base}/status`)
      .pipe(map(r => r.data));
  }

  submitMetrics(req: SubmitMetricsRequest): Observable<BacktestStatus> {
    return this.http
      .post<ServiceResponse<BacktestStatus>>(`${this.base}/metrics`, req)
      .pipe(map(r => r.data));
  }

  submitAnswers(answers: AnswerInput[]): Observable<BacktestStatus> {
    return this.http
      .post<ServiceResponse<BacktestStatus>>(`${this.base}/answers`, { answers })
      .pipe(map(r => r.data));
  }
}
