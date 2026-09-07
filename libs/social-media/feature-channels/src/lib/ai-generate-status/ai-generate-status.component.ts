import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ChannelStore, VideoGenerationStore } from '@sports-ui/social-media-data-access';

@Component({
  selector: 'lib-ai-generate-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <div class="page-header">
        <button class="btn-back" (click)="backToChannel()">← Back to Channel</button>
        <h1>AI Video Generation</h1>
      </div>

      @let job = store.currentJob();

      @if (!job) {
        <div class="loading-state">
          <span class="spinner"></span>
          <p>Starting generation...</p>
        </div>
      } @else {

        @if (job.status === 'Queued' || job.status === 'Generating') {
          <div class="generating-state">
            <span class="spinner spinner--large"></span>
            <p class="generating-label">Claude is generating your video with Higgsfield...</p>
            <span class="status-badge status-badge--{{ job.status.toLowerCase() }}">{{ job.status }}</span>
          </div>
        }

        @if (job.status === 'Failed' || job.status === 'TimedOut') {
          <div class="error-panel">
            <h2>Generation {{ job.status === 'TimedOut' ? 'Timed Out' : 'Failed' }}</h2>
            @if (job.errorMessage) {
              <p class="error-detail">{{ job.errorMessage }}</p>
            } @else if (job.status === 'TimedOut') {
              <p class="error-detail">The generation request timed out. Please try again.</p>
            }
            <button class="btn-secondary" (click)="tryAgain()">Try Again</button>
          </div>
        }

        @if (job.status === 'Ready' || job.status === 'Consumed') {
          <div class="ready-panel">
            <div class="ready-header">
              <span class="ready-icon">&#10003;</span>
              <h2>Video Ready!</h2>
            </div>
            <p class="ready-desc">Your AI-generated video is ready to publish. Fill in the post details below.</p>

            @if (noActiveAccounts()) {
              <div class="no-accounts-banner">
                <strong>No connected accounts.</strong>
                @if (hasLinkedAccounts()) {
                  All linked accounts have expired tokens — reconnect them in channel settings.
                } @else {
                  This channel has no linked social media accounts. Connect one before posting.
                }
                <button class="btn-settings" (click)="backToChannel()">Go to Channel Settings</button>
              </div>
            }

            <div class="post-form" [class.form-disabled]="noActiveAccounts()">
              <div class="form-group">
                <label>Title <span class="required">*</span></label>
                <input type="text" class="form-control"
                       [value]="title()"
                       (input)="title.set($any($event.target).value)"
                       placeholder="Enter a title for this post">
              </div>

              <div class="form-group">
                <label>Description</label>
                <textarea class="form-control" rows="4"
                          [value]="description()"
                          (input)="description.set($any($event.target).value)"
                          placeholder="Enter a description for this post"></textarea>
              </div>

              <div class="form-group">
                <label>Hashtags</label>
                <div class="hashtag-chips">
                  @for (tag of hashtags(); track tag) {
                    <span class="hashtag-chip">
                      #{{ tag }}
                      <button class="chip-remove" (click)="removeHashtag(tag)">&#10005;</button>
                    </span>
                  }
                </div>
                <input type="text" class="form-control hashtag-input"
                       [value]="hashtagInput()"
                       (input)="hashtagInput.set($any($event.target).value)"
                       (keydown)="onHashtagKeydown($event)"
                       placeholder="Type a hashtag and press Enter or comma">
              </div>

              @if (store.startPostStatus() === 'error') {
                <p class="error-msg">{{ store.error() }}</p>
              }

              <div class="form-actions">
                <button class="btn-secondary" (click)="backToChannel()">Cancel</button>
                <button class="btn-primary"
                        (click)="submitPost()"
                        [disabled]="!title().trim() || store.isStartingPost() || noActiveAccounts()">
                  {{ store.isStartingPost() ? 'Starting…' : 'Post to Channel' }}
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
    .page-header { display: flex; align-items: center; gap: 16px; margin-bottom: 32px; }
    .page-header h1 { flex: 1; margin: 0; font-size: 24px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; white-space: nowrap; }
    .loading-state { display: flex; flex-direction: column; align-items: center; gap: 16px; padding: 64px 24px; color: #666; }
    .generating-state { display: flex; flex-direction: column; align-items: center; gap: 16px; padding: 64px 24px; }
    .generating-label { font-size: 16px; color: #5c35c9; font-weight: 500; text-align: center; }
    .spinner { display: inline-block; width: 24px; height: 24px; border: 3px solid #c5b8f8; border-top-color: #7c4dff; border-radius: 50%; animation: spin 0.8s linear infinite; }
    .spinner--large { width: 48px; height: 48px; border-width: 4px; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .status-badge { padding: 4px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; }
    .status-badge--queued { background: #fff8e1; color: #f57f17; }
    .status-badge--generating { background: #f3f0ff; color: #5c35c9; }
    .error-panel { background: #ffebee; border: 1px solid #ef9a9a; border-radius: 10px; padding: 24px; }
    .error-panel h2 { margin: 0 0 12px; color: #c62828; font-size: 20px; }
    .error-detail { color: #c62828; font-size: 14px; margin: 0 0 20px; line-height: 1.5; }
    .ready-panel { }
    .ready-header { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
    .ready-icon { width: 36px; height: 36px; border-radius: 50%; background: #e8f5e9; color: #2e7d32; display: flex; align-items: center; justify-content: center; font-size: 18px; font-weight: bold; flex-shrink: 0; }
    .ready-header h2 { margin: 0; font-size: 22px; color: #2e7d32; }
    .ready-desc { color: #555; font-size: 14px; margin: 0 0 24px; }
    .post-form { background: #fafafa; border: 1px solid #e0e0e0; border-radius: 10px; padding: 24px; }
    .form-group { margin-bottom: 20px; }
    .form-group label { display: block; font-weight: 500; font-size: 14px; margin-bottom: 6px; }
    .required { color: #d32f2f; }
    .form-control { width: 100%; box-sizing: border-box; border: 1px solid #ddd; border-radius: 6px; padding: 8px 12px; font-size: 14px; font-family: inherit; }
    .form-control:focus { outline: none; border-color: #1976d2; }
    textarea.form-control { resize: vertical; }
    .hashtag-chips { display: flex; flex-wrap: wrap; gap: 6px; margin-bottom: 8px; }
    .hashtag-chip { display: flex; align-items: center; gap: 4px; background: #e3f2fd; color: #1565c0; padding: 4px 10px; border-radius: 16px; font-size: 13px; }
    .chip-remove { background: none; border: none; cursor: pointer; color: #1565c0; padding: 0 0 0 4px; font-size: 12px; line-height: 1; }
    .chip-remove:hover { color: #d32f2f; }
    .hashtag-input { margin-top: 4px; }
    .error-msg { color: #d32f2f; font-size: 13px; margin: 0 0 12px; }
    .form-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-primary:hover { background: #1565c0; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 8px 18px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
    .no-accounts-banner { display: flex; flex-wrap: wrap; align-items: center; gap: 12px; background: #fff3e0; border: 1px solid #ffb74d; border-radius: 8px; padding: 14px 16px; margin-bottom: 16px; font-size: 14px; color: #e65100; }
    .no-accounts-banner strong { color: #bf360c; }
    .btn-settings { margin-left: auto; background: #e65100; color: white; border: none; padding: 7px 16px; border-radius: 6px; cursor: pointer; font-size: 13px; white-space: nowrap; }
    .btn-settings:hover { background: #bf360c; }
    .form-disabled { opacity: 0.5; pointer-events: none; }
  `]
})
export class AiGenerateStatusComponent implements OnInit, OnDestroy {
  readonly store = inject(VideoGenerationStore);
  private readonly channelStore = inject(ChannelStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly title = signal('');
  readonly description = signal('');
  readonly hashtags = signal<string[]>([]);
  readonly hashtagInput = signal('');

  readonly hasLinkedAccounts = computed(() =>
    (this.channelStore.selectedChannel()?.linkedAccounts.length ?? 0) > 0
  );
  readonly noActiveAccounts = computed(() => {
    const accounts = this.channelStore.selectedChannel()?.linkedAccounts ?? [];
    return accounts.length === 0 || accounts.every((a) => a.tokenStatus === 'invalid');
  });

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
    // From channels/:id/ai-generate/:jobId → channels/:id
    this.router.navigate(['..', '..'], { relativeTo: this.route });
  }

  tryAgain(): void {
    // Navigate back to upload page
    this.router.navigate(['..'], { relativeTo: this.route });
  }

  onHashtagKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === ',') {
      event.preventDefault();
      this.addHashtag();
    }
  }

  private addHashtag(): void {
    const raw = this.hashtagInput().trim().replace(/^#/, '').replace(/,$/, '');
    if (raw && !this.hashtags().includes(raw)) {
      this.hashtags.update((tags) => [...tags, raw]);
    }
    this.hashtagInput.set('');
  }

  removeHashtag(tag: string): void {
    this.hashtags.update((tags) => tags.filter((t) => t !== tag));
  }

  async submitPost(): Promise<void> {
    if (!this.title().trim()) return;

    const postCycleJobId = await this.store.startPostFromGeneration({
      channelId: this.channelId,
      videoGenerationJobId: this.jobId,
      title: this.title(),
      description: this.description(),
      hashtags: this.hashtags(),
    });

    if (postCycleJobId) {
      // Navigate to channels/:id/post-cycle/:postCycleJobId
      this.router.navigate(['..', '..', 'post-cycle', postCycleJobId], { relativeTo: this.route });
    }
  }
}
