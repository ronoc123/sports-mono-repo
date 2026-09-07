import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@sports-ui/api-types';
import { ServiceResponse } from './channel.api';
import {
  StartVideoGenerationResponse,
  VideoGenerationJob,
  StartPostCycleFromGenerationRequest,
} from './video-generation.models';

@Injectable({ providedIn: 'root' })
export class VideoGenerationApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}${environment.socialMediaApi}`;

  startGeneration(formData: FormData): Observable<ServiceResponse<StartVideoGenerationResponse>> {
    return this.http.post<ServiceResponse<StartVideoGenerationResponse>>(
      `${this.base}video-generation/start`,
      formData
    );
  }

  getJob(jobId: string): Observable<ServiceResponse<VideoGenerationJob>> {
    return this.http.get<ServiceResponse<VideoGenerationJob>>(
      `${this.base}video-generation/${jobId}`
    );
  }

  startFromGeneration(req: StartPostCycleFromGenerationRequest): Observable<ServiceResponse<{ jobId: string }>> {
    return this.http.post<ServiceResponse<{ jobId: string }>>(
      `${this.base}post-cycle/start-from-generation`,
      req
    );
  }
}
