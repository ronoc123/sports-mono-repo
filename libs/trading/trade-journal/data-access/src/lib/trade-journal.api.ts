import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_URL } from '@sports-ui/http-client';
import {
  Trade,
  JournalEntry,
  CreateTradeRequest,
  UpsertJournalEntryRequest,
} from './trade-journal.models';

interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class TradeJournalApi {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);

  private get base() { return `${this.apiUrl}/tradejournal`; }

  getMyTrades(): Observable<Trade[]> {
    return this.http
      .get<ServiceResponse<Trade[]>>(this.base)
      .pipe(map(r => r.data));
  }

  getTrade(id: string): Observable<Trade> {
    return this.http
      .get<ServiceResponse<Trade>>(`${this.base}/${id}`)
      .pipe(map(r => r.data));
  }

  createTrade(req: CreateTradeRequest): Observable<Trade> {
    return this.http
      .post<ServiceResponse<Trade>>(this.base, req)
      .pipe(map(r => r.data));
  }

  upsertEntry(tradeId: string, req: UpsertJournalEntryRequest): Observable<JournalEntry> {
    return this.http
      .put<ServiceResponse<JournalEntry>>(`${this.base}/${tradeId}/entries`, req)
      .pipe(map(r => r.data));
  }
}
