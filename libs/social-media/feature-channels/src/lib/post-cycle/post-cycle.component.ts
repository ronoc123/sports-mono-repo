import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ChannelStore, PostCycleStore } from '@sports-ui/social-media-data-access';

@Component({
  selector: 'lib-post-cycle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="wizard-container">
      <div class="wizard-header">
        <button class="btn-back" (click)="cancel()">← Back to Channel</button>
        <h1>New Post Cycle</h1>
        <div class="step-indicator">
          <span class="step" [class.step--active]="step() === 1" [class.step--done]="step() > 1">1</span>
          <span class="step-line"></span>
          <span class="step" [class.step--active]="step() === 2">2</span>
        </div>
      </div>

      <!-- Step 1: Upload Video -->
      @if (step() === 1) {
        <div class="step-panel">
          <h2>Step 1: Select Video</h2>
          <p class="step-desc">Choose the video file you want to publish.</p>

          <div class="upload-area"
               [class.upload-area--has-file]="videoFile()"
               (click)="fileInput.click()"
               (dragover)="$event.preventDefault()"
               (drop)="onDrop($event)">
            @if (!videoFile()) {
              <div class="upload-placeholder">
                <span class="upload-icon">&#127916;</span>
                <p>Click or drag a video file here</p>
                <span class="upload-hint">MP4, MOV, AVI up to 2 GB</span>
              </div>
            } @else {
              <div class="file-selected">
                <span class="file-icon">&#10003;</span>
                <div class="file-info">
                  <span class="file-name">{{ videoFile()!.name }}</span>
                  <span class="file-size">{{ formatSize(videoFile()!.size) }}</span>
                </div>
                <button class="btn-clear" (click)="clearFile($event)">&#10005;</button>
              </div>
            }
          </div>

          <input #fileInput type="file" accept="video/*" style="display:none"
                 (change)="onFileSelected($event)">

          @if (videoPreviewUrl()) {
            <div class="video-preview">
              <video [src]="videoPreviewUrl()!" controls preload="metadata"></video>
            </div>
          }

          <div class="step-actions">
            <button class="btn-secondary" (click)="cancel()">Cancel</button>
            <button class="btn-primary" (click)="goToStep2()" [disabled]="!videoFile()">
              Next: Add Metadata
            </button>
          </div>
        </div>
      }

      <!-- Step 2: Metadata & AI Generation -->
      @if (step() === 2) {
        <div class="step-panel">
          <h2>Step 2: Video Metadata</h2>
          <p class="step-desc">Review and edit the title, description, and hashtags. Use AI to generate suggestions based on your channel history.</p>

          <!-- AI Generation Panel -->
          <div class="ai-panel">
            <div class="ai-panel-header">
              <span class="ai-label">AI Metadata Generator</span>
              @if (postCycleStore.isGenerating()) {
                <span class="ai-status">Generating...</span>
              }
            </div>
            <div class="ai-prompt-row">
              <input
                type="text"
                class="ai-prompt-input"
                placeholder="Optional: describe this video (e.g. 'match highlights from Sunday game')"
                [value]="aiPrompt()"
                (input)="aiPrompt.set($any($event.target).value)">
              <button class="btn-ai" (click)="generateMetadata()" [disabled]="postCycleStore.isGenerating()">
                {{ postCycleStore.isGenerating() ? 'Generating...' : '✦ Generate' }}
              </button>
            </div>
            @if (postCycleStore.generateStatus() === 'error') {
              <p class="error-msg">{{ postCycleStore.error() }}</p>
            }
          </div>

          <!-- Metadata Form -->
          <div class="form-group">
            <label>Title</label>
            <input type="text" class="form-control"
                   [value]="title()"
                   (input)="title.set($any($event.target).value)"
                   placeholder="Enter video title">
          </div>

          <div class="form-group">
            <label>Description</label>
            <textarea class="form-control" rows="5"
                      [value]="description()"
                      (input)="description.set($any($event.target).value)"
                      placeholder="Enter video description"></textarea>
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

          <!-- Suggestions banner -->
          @if (postCycleStore.suggestionsStatus() === 'loading') {
            <p class="suggestions-loading">Loading suggestions from post history...</p>
          }

          <div class="step-actions">
            <button class="btn-secondary" (click)="step.set(1)" [disabled]="postCycleStore.isSubmitting()">Back</button>
            <button class="btn-primary" (click)="submit()" [disabled]="!title().trim() || postCycleStore.isSubmitting()">
              {{ postCycleStore.isSubmitting() ? 'Uploading...' : 'Post to Channel' }}
            </button>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .wizard-container { padding: 24px; max-width: 760px; margin: 0 auto; }
    .wizard-header { display: flex; align-items: center; gap: 16px; margin-bottom: 32px; flex-wrap: wrap; }
    .wizard-header h1 { flex: 1; margin: 0; font-size: 24px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; white-space: nowrap; }
    .step-indicator { display: flex; align-items: center; gap: 8px; }
    .step { width: 28px; height: 28px; border-radius: 50%; border: 2px solid #ddd; display: flex; align-items: center; justify-content: center; font-size: 13px; font-weight: 600; color: #999; background: white; }
    .step--active { border-color: #1976d2; color: #1976d2; }
    .step--done { border-color: #43a047; background: #43a047; color: white; }
    .step-line { width: 32px; height: 2px; background: #ddd; }
    .step-panel { }
    .step-panel h2 { margin: 0 0 4px; font-size: 20px; }
    .step-desc { color: #666; font-size: 14px; margin: 0 0 24px; }
    .upload-area { border: 2px dashed #ccc; border-radius: 12px; padding: 40px 24px; text-align: center; cursor: pointer; transition: border-color 0.2s; }
    .upload-area:hover, .upload-area--has-file { border-color: #1976d2; }
    .upload-placeholder { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .upload-icon { font-size: 40px; }
    .upload-hint { color: #999; font-size: 12px; }
    .file-selected { display: flex; align-items: center; gap: 12px; }
    .file-icon { width: 32px; height: 32px; border-radius: 50%; background: #e8f5e9; color: #43a047; display: flex; align-items: center; justify-content: center; font-weight: bold; flex-shrink: 0; }
    .file-info { flex: 1; text-align: left; }
    .file-name { display: block; font-weight: 500; font-size: 14px; }
    .file-size { font-size: 12px; color: #777; }
    .btn-clear { background: none; border: none; cursor: pointer; color: #999; padding: 4px 8px; font-size: 16px; }
    .btn-clear:hover { color: #d32f2f; }
    .video-preview { margin-top: 16px; border-radius: 8px; overflow: hidden; background: #000; }
    .video-preview video { width: 100%; max-height: 320px; display: block; }
    .ai-panel { background: #f3f0ff; border: 1px solid #c5b8f8; border-radius: 8px; padding: 16px; margin-bottom: 24px; }
    .ai-panel-header { display: flex; align-items: center; gap: 12px; margin-bottom: 10px; }
    .ai-label { font-weight: 600; font-size: 14px; color: #5c35c9; }
    .ai-status { font-size: 13px; color: #5c35c9; animation: pulse 1.5s infinite; }
    @keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }
    .ai-prompt-row { display: flex; gap: 8px; }
    .ai-prompt-input { flex: 1; border: 1px solid #c5b8f8; border-radius: 6px; padding: 8px 12px; font-size: 14px; background: white; }
    .ai-prompt-input:focus { outline: none; border-color: #7c4dff; }
    .btn-ai { background: #7c4dff; color: white; border: none; padding: 8px 18px; border-radius: 6px; cursor: pointer; font-size: 14px; white-space: nowrap; }
    .btn-ai:hover { background: #6200ea; }
    .btn-ai:disabled { opacity: 0.6; cursor: not-allowed; }
    .error-msg { color: #d32f2f; font-size: 13px; margin: 8px 0 0; }
    .form-group { margin-bottom: 20px; }
    .form-group label { display: block; font-weight: 500; font-size: 14px; margin-bottom: 6px; }
    .form-control { width: 100%; box-sizing: border-box; border: 1px solid #ddd; border-radius: 6px; padding: 8px 12px; font-size: 14px; font-family: inherit; }
    .form-control:focus { outline: none; border-color: #1976d2; }
    textarea.form-control { resize: vertical; }
    .hashtag-chips { display: flex; flex-wrap: wrap; gap: 6px; margin-bottom: 8px; }
    .hashtag-chip { display: flex; align-items: center; gap: 4px; background: #e3f2fd; color: #1565c0; padding: 4px 10px; border-radius: 16px; font-size: 13px; }
    .chip-remove { background: none; border: none; cursor: pointer; color: #1565c0; padding: 0 0 0 4px; font-size: 12px; line-height: 1; }
    .chip-remove:hover { color: #d32f2f; }
    .hashtag-input { margin-top: 4px; }
    .suggestions-loading { color: #999; font-size: 13px; font-style: italic; }
    .step-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 32px; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-primary:hover { background: #1565c0; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
  `]
})
export class PostCycleComponent implements OnInit, OnDestroy {
  readonly channelStore = inject(ChannelStore);
  readonly postCycleStore = inject(PostCycleStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly step = signal<1 | 2>(1);
  readonly videoFile = signal<File | null>(null);
  readonly videoPreviewUrl = signal<string | null>(null);
  readonly aiPrompt = signal('');
  readonly title = signal('');
  readonly description = signal('');
  readonly hashtags = signal<string[]>([]);
  readonly hashtagInput = signal('');

  private channelId = '';
  private previewObjectUrl: string | null = null;

  ngOnInit(): void {
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    this.postCycleStore.resetPostCycle();
  }

  ngOnDestroy(): void {
    this.revokePreviewUrl();
  }

  cancel(): void {
    this.router.navigate(['..'], { relativeTo: this.route });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.setVideoFile(file);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files[0] ?? null;
    if (file?.type.startsWith('video/')) {
      this.setVideoFile(file);
    }
  }

  clearFile(event: Event): void {
    event.stopPropagation();
    this.setVideoFile(null);
  }

  private setVideoFile(file: File | null): void {
    this.revokePreviewUrl();
    this.videoFile.set(file);
    if (file) {
      const url = URL.createObjectURL(file);
      this.previewObjectUrl = url;
      this.videoPreviewUrl.set(url);
    } else {
      this.videoPreviewUrl.set(null);
    }
  }

  private revokePreviewUrl(): void {
    if (this.previewObjectUrl) {
      URL.revokeObjectURL(this.previewObjectUrl);
      this.previewObjectUrl = null;
    }
  }

  async goToStep2(): Promise<void> {
    this.step.set(2);
    await this.postCycleStore.loadSuggestions(this.channelId);
    const suggestions = this.postCycleStore.suggestions();
    if (suggestions) {
      if (!this.title()) this.title.set(suggestions.suggestedTitle);
      if (!this.description()) this.description.set(suggestions.suggestedDescription);
      if (!this.hashtags().length) this.hashtags.set([...suggestions.suggestedHashtags]);
    }
  }

  async generateMetadata(): Promise<void> {
    await this.postCycleStore.generateMetadata({
      channelId: this.channelId,
      videoReference: this.videoFile()?.name ?? '',
      userPrompt: this.aiPrompt() || undefined,
    });
    const generated = this.postCycleStore.generatedMetadata();
    if (generated) {
      this.title.set(generated.title);
      this.description.set(generated.description);
      this.hashtags.set([...generated.hashtags]);
    }
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

  async submit(): Promise<void> {
    const file = this.videoFile();
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file, file.name);
    formData.append('channelId', this.channelId);
    formData.append('title', this.title());
    formData.append('description', this.description());
    formData.append('hashtags', this.hashtags().join(','));

    const jobId = await this.postCycleStore.startPostCycle(formData);
    if (jobId) {
      // Navigate from channels/:id/post-cycle → channels/:id/post-cycle/:jobId
      this.router.navigate([jobId], { relativeTo: this.route });
    }
  }

  formatSize(bytes: number): string {
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`;
  }
}
