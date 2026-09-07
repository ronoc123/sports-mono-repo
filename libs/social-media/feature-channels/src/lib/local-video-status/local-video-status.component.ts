import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { RenderJobStore } from '@sports-ui/social-media-data-access';

@Component({
  selector: 'lib-local-video-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <div class="page-header">
        <button class="btn-back" (click)="backToChannel()">← Back to Channel</button>
        <h1>Local Video Generation</h1>
      </div>

      @let job = store.currentJob();

      @if (!job) {
        <div class="loading-state">
          <span class="spinner"></span>
          <p>Connecting to job...</p>
        </div>
      } @else {

        @if (job.status === 'Pending' || job.status === 'Processing') {
          <div class="generating-state">
            <span class="spinner spinner--large spinner--green"></span>
            <p class="generating-label">Your video is being generated on the local GPU worker…</p>
            <span class="status-badge status-badge--{{ job.status.toLowerCase() }}">{{ job.status }}</span>
          </div>
        }

        @if (job.status === 'Failed' || job.status === 'TimedOut') {
          <div class="error-panel">
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
          <div class="success-panel">
            <div class="success-header">
              <span class="success-icon">&#10003;</span>
              <h2>Generation Complete</h2>
            </div>
            <p class="success-desc">
              Your video has been generated and saved to R2 storage.
            </p>

            @if (job.outputVideoKey) {
              <div class="output-key-box">
                <span class="output-key-label">Output video key</span>
                <code class="output-key-value">{{ job.outputVideoKey }}</code>
              </div>
            }

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

            <div class="success-actions">
              <button class="btn-secondary" (click)="backToChannel()">Back to Channel</button>
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .container { padding: 24px; max-width: 760px; margin: 0 auto; }
    .page-header { display: flex; align-items: center; gap: 16px; margin-bottom: 32px; }
    .page-header h1 { flex: 1; margin: 0; font-size: 24px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; white-space: nowrap; }
    .loading-state { display: flex; flex-direction: column; align-items: center; gap: 16px; padding: 64px 24px; color: #666; }
    .generating-state { display: flex; flex-direction: column; align-items: center; gap: 16px; padding: 64px 24px; }
    .generating-label { font-size: 16px; color: #2e7d32; font-weight: 500; text-align: center; }
    .spinner { display: inline-block; width: 24px; height: 24px; border: 3px solid #c5b8f8; border-top-color: #7c4dff; border-radius: 50%; animation: spin 0.8s linear infinite; }
    .spinner--large { width: 48px; height: 48px; border-width: 4px; }
    .spinner--green { border-color: #a5d6a7; border-top-color: #2e7d32; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .status-badge { padding: 4px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; }
    .status-badge--pending { background: #fff8e1; color: #f57f17; }
    .status-badge--processing { background: #e8f5e9; color: #2e7d32; }
    .error-panel { background: #ffebee; border: 1px solid #ef9a9a; border-radius: 10px; padding: 24px; }
    .error-panel h2 { margin: 0 0 12px; color: #c62828; font-size: 20px; }
    .error-detail { color: #c62828; font-size: 14px; margin: 0 0 20px; line-height: 1.5; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
    .success-panel { background: #f1f8e9; border: 1px solid #c8e6c9; border-radius: 10px; padding: 28px; }
    .success-header { display: flex; align-items: center; gap: 12px; margin-bottom: 10px; }
    .success-icon { display: inline-flex; align-items: center; justify-content: center; width: 36px; height: 36px; border-radius: 50%; background: #c8e6c9; color: #2e7d32; font-size: 18px; font-weight: bold; flex-shrink: 0; }
    .success-header h2 { margin: 0; font-size: 22px; color: #2e7d32; }
    .success-desc { color: #555; font-size: 14px; margin: 0 0 20px; }
    .output-key-box { background: white; border: 1px solid #c8e6c9; border-radius: 8px; padding: 14px 16px; margin-bottom: 20px; }
    .output-key-label { display: block; font-size: 12px; font-weight: 600; color: #2e7d32; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 6px; }
    .output-key-value { display: block; font-family: monospace; font-size: 13px; color: #333; word-break: break-all; }
    .job-meta { background: white; border: 1px solid #c8e6c9; border-radius: 8px; padding: 14px 16px; margin-bottom: 20px; }
    .meta-row { display: flex; gap: 12px; padding: 5px 0; border-bottom: 1px solid #f0f0f0; }
    .meta-row:last-child { border-bottom: none; }
    .meta-label { font-size: 13px; color: #777; min-width: 100px; }
    .meta-value { font-size: 13px; color: #333; font-weight: 500; }
    .success-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
  `]
})
export class LocalVideoStatusComponent implements OnInit, OnDestroy {
  readonly store = inject(RenderJobStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private jobId = '';

  ngOnInit(): void {
    this.jobId = this.route.snapshot.paramMap.get('jobId')!;
    this.store.startPolling(this.jobId);
  }

  ngOnDestroy(): void {
    this.store.stopPolling();
  }

  backToChannel(): void {
    this.router.navigate(['..', '..'], { relativeTo: this.route });
  }

  tryAgain(): void {
    this.router.navigate(['..'], { relativeTo: this.route });
  }
}
