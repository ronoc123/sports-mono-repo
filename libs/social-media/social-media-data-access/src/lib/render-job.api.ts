import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@sports-ui/api-types';
import { ServiceResponse } from './channel.api';
import {
  UploadAssetResponse,
  VideoUrlResponse,
  CreateRenderJobRequest,
  CreateRenderJobResponse,
  StartPostFromRenderJobRequest,
  StartPostCycleResponse,
  RenderJob,
  IdeateVideoRequest,
  IdeateVideoResponse,
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

  listByChannel(channelId: string): Observable<ServiceResponse<RenderJob[]>> {
    return this.http.get<ServiceResponse<RenderJob[]>>(
      `${this.base}render-jobs`,
      { params: { channelId } }
    );
  }

  deleteJob(jobId: string): Observable<ServiceResponse<boolean>> {
    return this.http.delete<ServiceResponse<boolean>>(
      `${this.base}render-jobs/${jobId}`
    );
  }

  getVideoUrl(jobId: string): Observable<ServiceResponse<VideoUrlResponse>> {
    return this.http.get<ServiceResponse<VideoUrlResponse>>(
      `${this.base}render-jobs/${jobId}/video-url`
    );
  }

  startPostFromRenderJob(jobId: string, req: StartPostFromRenderJobRequest): Observable<ServiceResponse<StartPostCycleResponse>> {
    return this.http.post<ServiceResponse<StartPostCycleResponse>>(
      `${this.base}render-jobs/${jobId}/post`,
      req
    );
  }

  ideate(req: IdeateVideoRequest): Observable<ServiceResponse<IdeateVideoResponse>> {
    return this.http.post<ServiceResponse<IdeateVideoResponse>>(
      `${this.base}render-jobs/ideate`,
      req
    );
  }
}
