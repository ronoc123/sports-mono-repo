import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@sports-ui/api-types';
import {
  ChannelSummary,
  ChannelDetail,
  CreateChannelRequest,
  UpdateChannelRequest,
} from './channel.models';

export interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ChannelApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}${environment.socialMediaApi}channels`;
  private readonly oauthBase = `${environment.apiUrl}${environment.socialMediaApi}oauth`;

  getChannels(): Observable<ServiceResponse<ChannelSummary[]>> {
    return this.http.get<ServiceResponse<ChannelSummary[]>>(this.base);
  }

  getChannel(id: string): Observable<ServiceResponse<ChannelDetail>> {
    return this.http.get<ServiceResponse<ChannelDetail>>(`${this.base}/${id}`);
  }

  createChannel(request: CreateChannelRequest): Observable<ServiceResponse<ChannelDetail>> {
    return this.http.post<ServiceResponse<ChannelDetail>>(this.base, request);
  }

  updateChannel(id: string, request: UpdateChannelRequest): Observable<ServiceResponse<ChannelDetail>> {
    return this.http.put<ServiceResponse<ChannelDetail>>(`${this.base}/${id}`, request);
  }

  deleteChannel(id: string): Observable<ServiceResponse<boolean>> {
    return this.http.delete<ServiceResponse<boolean>>(`${this.base}/${id}`);
  }

  unlinkAccount(channelId: string, platform: string): Observable<ServiceResponse<ChannelDetail>> {
    return this.http.delete<ServiceResponse<ChannelDetail>>(
      `${this.base}/${channelId}/accounts/${encodeURIComponent(platform)}`
    );
  }

  getOAuthUrl(channelId: string): Observable<ServiceResponse<{ url: string }>> {
    return this.http.get<ServiceResponse<{ url: string }>>(
      `${this.oauthBase}/youtube/authorize?channelId=${channelId}`
    );
  }

  updatePromptTemplate(id: string, template: string): Observable<ServiceResponse<ChannelDetail>> {
    return this.http.patch<ServiceResponse<ChannelDetail>>(
      `${this.base}/${id}/prompt-template`,
      { promptTemplate: template }
    );
  }

  uploadCharacterImage(id: string, formData: FormData): Observable<ServiceResponse<ChannelDetail>> {
    return this.http.post<ServiceResponse<ChannelDetail>>(
      `${this.base}/${id}/image`,
      formData
    );
  }
}
