import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_URL } from '@sports-ui/http-client';
import { AISession, SendAIMessageRequest } from './ai-conversation.models';

interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class AIConversationApi {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);

  private get base() { return `${this.apiUrl}/aiconversation`; }

  getByEntity(linkedEntityId: string): Observable<AISession | null> {
    return this.http
      .get<ServiceResponse<AISession | null>>(`${this.base}/by-entity/${linkedEntityId}`)
      .pipe(map(r => r.data));
  }

  sendMessage(req: SendAIMessageRequest): Observable<AISession> {
    return this.http
      .post<ServiceResponse<AISession>>(`${this.base}/message`, req)
      .pipe(map(r => r.data));
  }
}
