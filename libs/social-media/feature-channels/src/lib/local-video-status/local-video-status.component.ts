import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RenderJobStore } from '@sports-ui/social-media-data-access';

@Component({
  selector: 'lib-local-video-status',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <div class="page-header">
        <button class="btn-back" (click)="backToChannel()">← Back to Channel</button>
        <h1>Local Video Status</h1>
        <div class="sync-area">
          @if (lastChecked()) {
            <span class="last-checked">Checked {{ lastChecked() | date:'HH:mm:ss' }}</span>
          }
          <button class="btn-sync" (click)="syncJob()" [disabled]="syncing()">
            {{ syncing() ? 'Syncing…' : '&#8635; Sync' }}
          </button>
        </div>
      </div>

      @let job = store.currentJob();

      @if (!job && syncing()) {
        <div class="loading-state">
          <span class="spinner spinner--green"></span>
          <p>Loading job status...</p>
        </div>
      } @else if (!job) {
        <div class="loading-state">
          <p>Job not found. <button class="btn-link" (click)="syncJob()">Try syncing</button>.</p>
        </div>
      } @else {

        @if (job.status === 'Pending' || job.status === 'Processing') {
          <div class="status-panel status-panel--in-progress">
            <span class="spinner spinner--large spinner--green"></span>
            <p class="generating-label">Your video is being generated on the local GPU worker…</p>
            <span class="status-badge status-badge--{{ job.status.toLowerCase() }}">{{ job.status }}</span>
            <p class="sync-hint">The status does not update automatically. Click <strong>Sync</strong> to check the latest progress.</p>
          </div>
        }

        @if (job.status === 'Failed' || job.status === 'TimedOut') {
          <div class="status-panel error-panel">
            <h2>Generation {{ job.status === 'TimedOut' ? 'Timed Out' : 'Failed' }}</h2>
            @if (job.errorMessage) {
              <p class="error-detail">{{ job.errorMessage }}</p>
            } @else if (job.status === 'TimedOut') {
              <p class="error-detail">The generation request timed out after 30 minutes. Please try again.</p>
            } @else {
              <p class="error-detail">An unexpected error occurred during video generation.</p>
            }
            <button class="btn-secondary" (click)="tryAgain()">Try Again</button>
          </div>
        }

        @if (job.status === 'Completed') {
          <div class="status-panel success-panel">
            <div class="success-header">
              <span class="success-icon">&#10003;</span>
              <h2>Generation Complete</h2>
            </div>

            <!-- Video player -->
            <div class="video-section">
              @if (videoUrl()) {
                <video class="video-player" controls [src]="videoUrl()">
                  Your browser does not support video playback.
                </video>
              } @else if (videoUrlLoading()) {
                <div class="video-loading">
                  <span class="spinner spinner--green"></span>
                  <span>Loading video...</span>
                </div>
              } @else {
                <div class="video-placeholder">
                  <button class="btn-local" (click)="loadVideoUrl()">&#9654; Play Video</button>
                  <p class="video-hint">Click to load the video from R2 storage (link valid for 1 hour).</p>
                </div>
              }
              @if (videoUrlError()) {
                <p class="error-msg">{{ videoUrlError() }}</p>
              }
            </div>

            <div class="job-meta">
              <div class="meta-row">
                <span class="meta-label">Model</span>
                <span class="meta-value">{{ job.model }}</span>
              </div>
              <div class="meta-row">
                <span class="meta-label">Duration</span>
                <span class="meta-value">{{ job.durationSeconds }}s</span>
              </div>
              <div class="meta-row">
                <span class="meta-label">Resolution</span>
                <span class="meta-value">{{ job.resolution }}</span>
              </div>
              <div class="meta-row">
                <span class="meta-label">Aspect Ratio</span>
                <span class="meta-value">{{ job.aspectRatio }}</span>
              </div>
              @if (job.completedAt) {
                <div class="meta-row">
                  <span class="meta-label">Completed</span>
                  <span class="meta-value">{{ job.completedAt | date:'medium' }}</span>
                </div>
              }
            </div>

            <!-- Post to Social Media -->
            <div class="post-section">
              <h3 class="post-section-title">Post to Social Media</h3>
              <p class="post-section-desc">Fill in the details below to publish this video to your linked accounts.</p>

              <div class="form-group">
                <label class="form-label">Title <span class="required">*</span></label>
                <input type="text"
                       class="form-control"
                       [(ngModel)]="postTitle"
                       placeholder="Enter a title for your video"
                       [disabled]="store.isStartingPost()" />
              </div>

              <div class="form-group">
                <label class="form-label">Description</label>
                <textarea class="form-control"
                          rows="3"
                          [(ngModel)]="postDescription"
                          placeholder="Optional description"
                          [disabled]="store.isStartingPost()"></textarea>
              </div>

              <div class="form-group">
                <label class="form-label">Hashtags <span class="form-hint">(comma-separated, without #)</span></label>
                <input type="text"
                       class="form-control"
                       [(ngModel)]="postHashtags"
                       placeholder="sports, highlights, fitness"
                       [disabled]="store.isStartingPost()" />
              </div>

              @if (store.postError()) {
                <p class="error-msg">{{ store.postError() }}</p>
              }

              <div class="post-actions">
                <button class="btn-secondary" (click)="backToChannel()" [disabled]="store.isStartingPost()">
                  Back to Channel
                </button>
                <button class="btn-post"
                        (click)="submitPost()"
                        [disabled]="!postTitle.trim() || store.isStartingPost()">
                  {{ store.isStartingPost() ? 'Starting…' : 'Post to Social Media' }}
                </button>
              </div>
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .container { padding: 24px; max-width: 760px; margin: 0 auto; }
    .page-header { display: flex; align-items: center; gap: 16px; margin-bottom: 32px; flex-wrap: wrap; }
    .page-header h1 { flex: 1; margin: 0; font-size: 24px; min-width: 0; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; white-space: nowrap; }
    .sync-area { display: flex; align-items: center; gap: 10px; flex-shrink: 0; }
    .last-checked { font-size: 12px; color: #999; white-space: nowrap; }
    .btn-sync { background: white; color: #2e7d32; border: 1px solid #2e7d32; padding: 7px 16px; border-radius: 6px; cursor: pointer; font-size: 13px; }
    .btn-sync:hover { background: #f1f8e9; }
    .btn-sync:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-link { background: none; border: none; color: #1976d2; cursor: pointer; font-size: inherit; padding: 0; text-decoration: underline; }
    .loading-state { display: flex; flex-direction: column; align-items: center; gap: 16px; padding: 64px 24px; color: #666; }
    .status-panel { border-radius: 10px; padding: 28px; margin-bottom: 20px; }
    .status-panel--in-progress { background: #f1f8e9; border: 1px solid #c8e6c9; display: flex; flex-direction: column; align-items: center; gap: 16px; padding: 48px 28px; }
    .generating-label { font-size: 16px; color: #2e7d32; font-weight: 500; text-align: center; margin: 0; }
    .sync-hint { font-size: 13px; color: #777; text-align: center; margin: 0; }
    .spinner { display: inline-block; width: 24px; height: 24px; border: 3px solid #c5b8f8; border-top-color: #7c4dff; border-radius: 50%; animation: spin 0.8s linear infinite; }
    .spinner--large { width: 48px; height: 48px; border-width: 4px; }
    .spinner--green { border-color: #a5d6a7; border-top-color: #2e7d32; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .status-badge { padding: 4px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; }
    .status-badge--pending { background: #fff8e1; color: #f57f17; }
    .status-badge--processing { background: #e8f5e9; color: #2e7d32; }
    .error-panel { background: #ffebee; border: 1px solid #ef9a9a; }
    .error-panel h2 { margin: 0 0 12px; color: #c62828; font-size: 20px; }
    .error-detail { color: #c62828; font-size: 14px; margin: 0 0 20px; line-height: 1.5; }
    .error-msg { color: #d32f2f; font-size: 13px; margin: 8px 0 0; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
    .btn-secondary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-local { background: #2e7d32; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-local:hover { background: #1b5e20; }
    .btn-post { background: #1976d2; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-post:hover { background: #1565c0; }
    .btn-post:disabled { opacity: 0.6; cursor: not-allowed; }
    .success-panel { background: #f1f8e9; border: 1px solid #c8e6c9; }
    .success-header { display: flex; align-items: center; gap: 12px; margin-bottom: 20px; }
    .success-icon { display: inline-flex; align-items: center; justify-content: center; width: 36px; height: 36px; border-radius: 50%; background: #c8e6c9; color: #2e7d32; font-size: 18px; font-weight: bold; flex-shrink: 0; }
    .success-header h2 { margin: 0; font-size: 22px; color: #2e7d32; }
    .video-section { margin-bottom: 20px; }
    .video-player { width: 100%; max-height: 480px; border-radius: 8px; background: #000; display: block; }
    .video-loading { display: flex; align-items: center; gap: 10px; padding: 20px; color: #555; font-size: 14px; }
    .video-placeholder { display: flex; flex-direction: column; align-items: center; gap: 10px; padding: 24px; background: white; border: 1px solid #c8e6c9; border-radius: 8px; }
    .video-hint { margin: 0; font-size: 12px; color: #999; text-align: center; }
    .job-meta { background: white; border: 1px solid #c8e6c9; border-radius: 8px; padding: 14px 16px; margin-bottom: 24px; }
    .meta-row { display: flex; gap: 12px; padding: 5px 0; border-bottom: 1px solid #f0f0f0; flex-wrap: wrap; }
    .meta-row:last-child { border-bottom: none; }
    .meta-label { font-size: 13px; color: #777; min-width: 100px; }
    .meta-value { font-size: 13px; color: #333; font-weight: 500; }
    .post-section { background: white; border: 1px solid #c8e6c9; border-radius: 8px; padding: 20px; }
    .post-section-title { margin: 0 0 6px; font-size: 17px; color: #1a1a1a; }
    .post-section-desc { margin: 0 0 20px; font-size: 13px; color: #777; }
    .form-group { margin-bottom: 16px; }
    .form-label { display: block; font-size: 13px; font-weight: 600; color: #444; margin-bottom: 6px; }
    .required { color: #d32f2f; }
    .form-hint { font-weight: normal; color: #999; font-size: 12px; }
    .form-control { width: 100%; box-sizing: border-box; border: 1px solid #ddd; border-radius: 6px; padding: 8px 12px; font-size: 14px; font-family: inherit; }
    .form-control:focus { outline: none; border-color: #1976d2; }
    .form-control:disabled { background: #f5f5f5; color: #999; cursor: not-allowed; }
    textarea.form-control { resize: vertical; }
    .post-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; }
  `]
})
export class LocalVideoStatusComponent implements OnInit {
  readonly store = inject(RenderJobStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly syncing = signal(false);
  readonly lastChecked = signal<Date | null>(null);
  readonly videoUrl = signal<string | null>(null);
  readonly videoUrlLoading = signal(false);
  readonly videoUrlError = signal<string | null>(null);

  postTitle = '';
  postDescription = '';
  postHashtags = '';

  private jobId = '';
  private channelId = '';

  async ngOnInit(): Promise<void> {
    this.jobId = this.route.snapshot.paramMap.get('jobId')!;
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    await this.syncJob();
  }

  async syncJob(): Promise<void> {
    if (this.syncing()) return;
    this.syncing.set(true);
    try {
      await this.store.fetchJob(this.jobId);
      this.lastChecked.set(new Date());
      // Auto-load video URL when job is completed (only if not already loaded)
      if (this.store.currentJob()?.status === 'Completed' && !this.videoUrl()) {
        await this.loadVideoUrl();
      }
    } finally {
      this.syncing.set(false);
    }
  }

  async loadVideoUrl(): Promise<void> {
    this.videoUrlLoading.set(true);
    this.videoUrlError.set(null);
    try {
      const url = await this.store.getVideoUrl(this.jobId);
      if (url) {
        this.videoUrl.set(url);
      } else {
        this.videoUrlError.set('Could not load video URL. The job may not be complete yet.');
      }
    } finally {
      this.videoUrlLoading.set(false);
    }
  }

  async submitPost(): Promise<void> {
    if (!this.postTitle.trim()) return;

    const hashtags = this.postHashtags
      .split(',')
      .map(h => h.trim().replace(/^#/, ''))
      .filter(h => h.length > 0);

    const postCycleJobId = await this.store.startPostFromRenderJob(this.jobId, {
      channelId: this.channelId,
      title: this.postTitle.trim(),
      description: this.postDescription.trim(),
      hashtags,
    });

    if (postCycleJobId) {
      this.router.navigate(
        ['..', '..', 'post-cycle', postCycleJobId],
        { relativeTo: this.route }
      );
    }
  }

  backToChannel(): void {
    this.router.navigate(['..', '..'], { relativeTo: this.route });
  }

  tryAgain(): void {
    this.router.navigate(['..'], { relativeTo: this.route });
  }
}
