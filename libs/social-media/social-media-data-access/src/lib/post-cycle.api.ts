import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@sports-ui/api-types';
import { ServiceResponse } from './channel.api';
import {
  MetadataSuggestions,
  GenerateVideoRequest,
  GeneratedMetadata,
  PostCycleJob,
  PostHistoryPage,
  PostRecordDetail,
} from './post-cycle.models';

@Injectable({ providedIn: 'root' })
export class PostCycleApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}${environment.socialMediaApi}`;

  getSuggestions(channelId: string): Observable<ServiceResponse<MetadataSuggestions>> {
    return this.http.get<ServiceResponse<MetadataSuggestions>>(
      `${this.base}post-records/suggestions?channelId=${encodeURIComponent(channelId)}`
    );
  }

  generateMetadata(request: GenerateVideoRequest): Observable<ServiceResponse<GeneratedMetadata>> {
    return this.http.post<ServiceResponse<GeneratedMetadata>>(
      `${this.base}video-generation/generate`,
      request
    );
  }

  startPostCycle(formData: FormData): Observable<ServiceResponse<{ jobId: string }>> {
    return this.http.post<ServiceResponse<{ jobId: string }>>(
      `${this.base}post-cycle/start`,
      formData
    );
  }

  getPostCycle(jobId: string): Observable<ServiceResponse<PostCycleJob>> {
    return this.http.get<ServiceResponse<PostCycleJob>>(
      `${this.base}post-cycle/${jobId}`
    );
  }

  retryPlatform(jobId: string, platform: string): Observable<ServiceResponse<PostCycleJob>> {
    return this.http.post<ServiceResponse<PostCycleJob>>(
      `${this.base}post-cycle/${jobId}/retry/${encodeURIComponent(platform)}`,
      {}
    );
  }

  getPostHistory(channelId: string, page: number, pageSize: number): Observable<ServiceResponse<PostHistoryPage>> {
    return this.http.get<ServiceResponse<PostHistoryPage>>(
      `${this.base}post-records/history?channelId=${encodeURIComponent(channelId)}&page=${page}&pageSize=${pageSize}`
    );
  }

  getPostRecord(id: string): Observable<ServiceResponse<PostRecordDetail>> {
    return this.http.get<ServiceResponse<PostRecordDetail>>(
      `${this.base}post-records/${id}`
    );
  }
}
