import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PostCycleStore } from '@sports-ui/social-media-data-access';

@Component({
  selector: 'lib-post-cycle-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="status-container">
      <div class="status-header">
        <button class="btn-back" (click)="backToChannel()">← Back to Channel</button>
        <h1>Publishing Status</h1>
      </div>

      @if (!store.currentJob()) {
        <div class="loading-state">Loading job status...</div>
      } @else {
        @let job = store.currentJob()!;

        <div class="job-summary">
          <h2>{{ job.title }}</h2>
          <div class="job-status" [class]="'job-status--' + job.status.toLowerCase()">
            {{ statusLabel(job.status) }}
          </div>
        </div>

        @if (job.status === 'TimedOut') {
          <div class="timeout-banner">
            Publishing timed out after 10 minutes. Please check your platform accounts and retry any failed platforms.
          </div>
        }

        <div class="platforms-list">
          @for (platform of job.platformJobs; track platform.platform) {
            <div class="platform-card" [class]="'platform-card--' + platform.status.toLowerCase()">
              <div class="platform-left">
                <span class="platform-badge platform-badge--{{ platform.platform.toLowerCase() }}">
                  {{ platform.platform }}
                </span>

                <div class="platform-status-info">
                  <span class="platform-status-label">
                    @switch (platform.status) {
                      @case ('Pending') { Waiting to upload... }
                      @case ('Uploading') { Uploading... }
                      @case ('Published') { Published }
                      @case ('Failed') { Failed }
                    }
                  </span>

                  @if (platform.status === 'Uploading') {
                    <span class="uploading-indicator"></span>
                  }

                  @if (platform.status === 'Published' && platform.videoUrl) {
                    <a [href]="platform.videoUrl" target="_blank" rel="noopener" class="video-link">
                      Watch on {{ platform.platform }} ↗
                    </a>
                  }

                  @if (platform.status === 'Failed' && platform.errorMessage) {
                    <span class="error-detail">{{ platform.errorMessage }}</span>
                  }
                </div>
              </div>

              <div class="platform-actions">
                @if (platform.status === 'Failed') {
                  @if (platform.requiresReauth) {
                    <button class="btn-reauth" (click)="goToChannel()">
                      Re-authenticate & Retry
                    </button>
                  } @else {
                    <button class="btn-retry" (click)="retryPlatform(platform.platform)">
                      Retry
                    </button>
                  }
                }

                @if (platform.status === 'Uploading') {
                  <span class="spinner"></span>
                }
              </div>
            </div>
          }
        </div>

        @if (store.isJobTerminal()) {
          <div class="terminal-actions">
            <button class="btn-secondary" (click)="backToChannel()">Back to Channel</button>
            @if (job.status !== 'Completed') {
              <button class="btn-primary" (click)="newPostCycle()">Start New Post</button>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .status-container { padding: 24px; max-width: 760px; margin: 0 auto; }
    .status-header { display: flex; align-items: center; gap: 16px; margin-bottom: 32px; }
    .status-header h1 { flex: 1; margin: 0; font-size: 24px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; white-space: nowrap; }
    .loading-state { text-align: center; padding: 48px; color: #666; }
    .job-summary { display: flex; align-items: center; justify-content: space-between; margin-bottom: 24px; flex-wrap: wrap; gap: 12px; }
    .job-summary h2 { margin: 0; font-size: 20px; }
    .job-status { padding: 6px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; }
    .job-status--running { background: #e3f2fd; color: #1565c0; }
    .job-status--completed { background: #e8f5e9; color: #2e7d32; }
    .job-status--partialfailure { background: #fff3e0; color: #e65100; }
    .job-status--failed { background: #ffebee; color: #c62828; }
    .job-status--timedout { background: #fafafa; color: #616161; }
    .timeout-banner { background: #fff8e1; border: 1px solid #ffe082; border-radius: 6px; padding: 12px 16px; font-size: 14px; color: #5d4037; margin-bottom: 24px; }
    .platforms-list { display: flex; flex-direction: column; gap: 12px; }
    .platform-card { display: flex; align-items: center; justify-content: space-between; padding: 16px 20px; border: 1px solid #e0e0e0; border-radius: 10px; gap: 12px; }
    .platform-card--published { border-color: #a5d6a7; background: #f9fff9; }
    .platform-card--failed { border-color: #ef9a9a; background: #fff9f9; }
    .platform-card--uploading { border-color: #90caf9; background: #f5faff; }
    .platform-left { display: flex; align-items: flex-start; gap: 14px; flex: 1; }
    .platform-badge { padding: 3px 10px; border-radius: 4px; font-size: 12px; font-weight: 600; flex-shrink: 0; margin-top: 2px; }
    .platform-badge--youtube { background: #ff0000; color: white; }
    .platform-status-info { display: flex; flex-direction: column; gap: 4px; }
    .platform-status-label { font-weight: 500; font-size: 14px; }
    .uploading-indicator { display: inline-block; width: 12px; height: 12px; border-radius: 50%; background: #1976d2; animation: pulse 1.2s infinite; }
    @keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.3; } }
    .video-link { color: #1976d2; font-size: 13px; text-decoration: none; }
    .video-link:hover { text-decoration: underline; }
    .error-detail { color: #c62828; font-size: 13px; }
    .platform-actions { display: flex; align-items: center; gap: 8px; flex-shrink: 0; }
    .btn-retry { background: white; color: #1976d2; border: 1px solid #1976d2; padding: 6px 14px; border-radius: 6px; cursor: pointer; font-size: 13px; }
    .btn-retry:hover { background: #e3f2fd; }
    .btn-reauth { background: white; color: #e65100; border: 1px solid #e65100; padding: 6px 14px; border-radius: 6px; cursor: pointer; font-size: 13px; }
    .btn-reauth:hover { background: #fff3e0; }
    .spinner { display: inline-block; width: 18px; height: 18px; border: 2px solid #90caf9; border-top-color: #1976d2; border-radius: 50%; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .terminal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 32px; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-primary:hover { background: #1565c0; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
  `]
})
export class PostCycleStatusComponent implements OnInit, OnDestroy {
  readonly store = inject(PostCycleStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private channelId = '';
  private jobId = '';

  ngOnInit(): void {
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    this.jobId = this.route.snapshot.paramMap.get('jobId')!;
    this.store.startPolling(this.jobId);
  }

  ngOnDestroy(): void {
    this.store.stopPolling();
  }

  backToChannel(): void {
    // From channels/:id/post-cycle/:jobId → channels/:id
    this.router.navigate(['..', this.channelId], { relativeTo: this.route });
  }

  goToChannel(): void {
    this.router.navigate(['..', this.channelId], { relativeTo: this.route });
  }

  newPostCycle(): void {
    // From channels/:id/post-cycle/:jobId → channels/:id/post-cycle
    this.router.navigate(['..', this.channelId, 'post-cycle'], { relativeTo: this.route });
  }

  retryPlatform(platform: string): void {
    this.store.retryPlatform(this.jobId, platform);
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      Running: 'Publishing...',
      Completed: 'All Published',
      PartialFailure: 'Partial Success',
      Failed: 'Failed',
      TimedOut: 'Timed Out',
    };
    return map[status] ?? status;
  }
}
