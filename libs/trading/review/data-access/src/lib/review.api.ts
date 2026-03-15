import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_URL } from '@sports-ui/http-client';
import { ReviewSession, ReviewStatus } from './review.models';

interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ReviewApi {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);

  private get base() { return `${this.apiUrl}/biweeklyreview`; }

  getStatus(): Observable<ReviewStatus> {
    return this.http
      .get<ServiceResponse<ReviewStatus>>(`${this.base}/status`)
      .pipe(map(r => r.data));
  }

  getSession(sessionId: string): Observable<ReviewSession> {
    return this.http
      .get<ServiceResponse<ReviewSession>>(`${this.base}/${sessionId}`)
      .pipe(map(r => r.data));
  }

  sendMessage(existingSessionId: string | null, userMessage: string): Observable<ReviewSession> {
    return this.http
      .post<ServiceResponse<ReviewSession>>(`${this.base}/message`, {
        existingSessionId,
        userMessage,
      })
      .pipe(map(r => r.data));
  }
}
