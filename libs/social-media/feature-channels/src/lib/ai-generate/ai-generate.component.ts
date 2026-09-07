import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ChannelStore, VideoGenerationStore } from '@sports-ui/social-media-data-access';
import { environment } from '@sports-ui/api-types';

@Component({
  selector: 'lib-ai-generate',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <div class="page-header">
        <button class="btn-back" (click)="cancel()">← Back to Channel</button>
        <h1>Generate AI Video</h1>
      </div>

      @if (channelStore.isLoading()) {
        <div class="loading-state">Loading channel...</div>
      }

      @if (channelStore.selectedChannel(); as channel) {

        @if (channel.characterImageUrl) {

          <!-- Character reference preview -->
          <div class="char-ref-card">
            <div class="char-ref-header">
              <span class="char-ref-label">Character Reference</span>
              <span class="char-ref-note">Sent to Higgsfield as visual context for every generation</span>
            </div>
            <img [src]="apiOrigin + channel.characterImageUrl" alt="Character reference" class="char-ref-image">
          </div>

          <!-- Prompt template info -->
          @if (channel.promptTemplate) {
            <div class="info-banner">
              <span class="info-label">Prompt template:</span>
              <span class="info-excerpt">{{ channel.promptTemplate | slice:0:140 }}{{ (channel.promptTemplate.length > 140) ? '…' : '' }}</span>
            </div>
          } @else {
            <div class="info-banner info-banner--default">
              No prompt template set — channel uses the default prompt.
            </div>
          }

          <!-- Optional prompt override -->
          <div class="form-group">
            <label>Prompt Override <span class="optional">(optional)</span></label>
            <textarea class="form-control" rows="3"
                      [value]="promptOverride()"
                      (input)="promptOverride.set($any($event.target).value)"
                      placeholder="Leave blank to use the channel's default prompt template"></textarea>
          </div>

          @if (store.uploadStatus() === 'error') {
            <p class="error-msg">{{ store.error() }}</p>
          }

          <div class="actions">
            <button class="btn-secondary" (click)="cancel()">Cancel</button>
            <button class="btn-ai" (click)="generate()" [disabled]="store.isUploading()">
              {{ store.isUploading() ? 'Starting…' : '&#10024; Generate Video' }}
            </button>
          </div>

        } @else {

          <!-- No character image set -->
          <div class="no-image-warning">
            <div class="warning-icon">&#9888;</div>
            <div class="warning-body">
              <p class="warning-title">Character reference image required</p>
              <p class="warning-desc">
                Upload a character reference image on the channel settings page before generating AI videos.
                This image gives Higgsfield the visual context it needs to animate your character correctly.
              </p>
              <button class="btn-secondary" (click)="cancel()">Go to Channel Settings</button>
            </div>
          </div>

        }
      }
    </div>
  `,
  styles: [`
    .container { padding: 24px; max-width: 760px; margin: 0 auto; }
    .page-header { display: flex; align-items: center; gap: 16px; margin-bottom: 24px; }
    .page-header h1 { flex: 1; margin: 0; font-size: 24px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; white-space: nowrap; }
    .loading-state { text-align: center; padding: 48px; color: #666; }
    .char-ref-card { border: 1px solid #c5b8f8; border-radius: 10px; overflow: hidden; margin-bottom: 20px; }
    .char-ref-header { display: flex; align-items: baseline; gap: 10px; padding: 10px 14px; background: #f3f0ff; border-bottom: 1px solid #c5b8f8; flex-wrap: wrap; }
    .char-ref-label { font-weight: 600; font-size: 14px; color: #5c35c9; }
    .char-ref-note { font-size: 13px; color: #7c5cbf; }
    .char-ref-image { width: 100%; max-height: 360px; object-fit: contain; display: block; background: #fafafa; }
    .info-banner { background: #f3f0ff; border: 1px solid #c5b8f8; border-radius: 8px; padding: 12px 16px; margin-bottom: 20px; font-size: 14px; display: flex; flex-wrap: wrap; gap: 6px; align-items: baseline; }
    .info-banner--default { background: #f8f9fa; border-color: #ddd; color: #777; }
    .info-label { font-weight: 600; color: #5c35c9; white-space: nowrap; }
    .info-excerpt { color: #333; }
    .form-group { margin-bottom: 20px; }
    .form-group label { display: block; font-weight: 500; font-size: 14px; margin-bottom: 6px; }
    .optional { font-weight: 400; color: #999; }
    .form-control { width: 100%; box-sizing: border-box; border: 1px solid #ddd; border-radius: 6px; padding: 8px 12px; font-size: 14px; font-family: inherit; resize: vertical; }
    .form-control:focus { outline: none; border-color: #7c4dff; }
    .error-msg { color: #d32f2f; font-size: 13px; margin: 0 0 12px; }
    .actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
    .btn-ai { background: #7c4dff; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-ai:hover { background: #6200ea; }
    .btn-ai:disabled { opacity: 0.6; cursor: not-allowed; }
    .no-image-warning { display: flex; gap: 16px; background: #fff8e1; border: 1px solid #ffe082; border-radius: 10px; padding: 24px; align-items: flex-start; }
    .warning-icon { font-size: 28px; flex-shrink: 0; color: #f57f17; }
    .warning-body { flex: 1; }
    .warning-title { margin: 0 0 8px; font-size: 16px; font-weight: 600; color: #5d4037; }
    .warning-desc { margin: 0 0 16px; font-size: 14px; color: #6d4c41; line-height: 1.6; }
  `]
})
export class AiGenerateComponent implements OnInit {
  readonly store = inject(VideoGenerationStore);
  readonly channelStore = inject(ChannelStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly promptOverride = signal('');
  readonly apiOrigin = environment.apiUrl + environment.socialMediaApi.split('/api')[0];

  private channelId = '';

  ngOnInit(): void {
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    this.store.resetGeneration();
    if (!this.channelStore.selectedChannel()) {
      this.channelStore.loadChannel(this.channelId);
    }
  }

  cancel(): void {
    this.router.navigate(['..'], { relativeTo: this.route });
  }

  async generate(): Promise<void> {
    const formData = new FormData();
    formData.append('channelId', this.channelId);
    const override = this.promptOverride().trim();
    if (override) {
      formData.append('promptOverride', override);
    }

    const jobId = await this.store.uploadGeneration(formData);
    if (jobId) {
      this.router.navigate([jobId], { relativeTo: this.route });
    }
  }
}
