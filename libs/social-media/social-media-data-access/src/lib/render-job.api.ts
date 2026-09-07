import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@sports-ui/api-types';
import { ServiceResponse } from './channel.api';
import {
  UploadAssetResponse,
  CreateRenderJobRequest,
  CreateRenderJobResponse,
  RenderJob,
} from './render-job.models';

@Injectable({ providedIn: 'root' })
export class RenderJobApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}${environment.socialMediaApi}`;

  uploadAsset(formData: FormData): Observable<ServiceResponse<UploadAssetResponse>> {
    return this.http.post<ServiceResponse<UploadAssetResponse>>(
      `${this.base}render-jobs/assets`,
      formData
    );
  }

  createJob(req: CreateRenderJobRequest): Observable<ServiceResponse<CreateRenderJobResponse>> {
    return this.http.post<ServiceResponse<CreateRenderJobResponse>>(
      `${this.base}render-jobs`,
      req
    );
  }

  getJob(jobId: string): Observable<ServiceResponse<RenderJob>> {
    return this.http.get<ServiceResponse<RenderJob>>(
      `${this.base}render-jobs/${jobId}`
    );
  }
}
