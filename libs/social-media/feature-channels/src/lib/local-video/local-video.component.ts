import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { RenderJobStore } from '@sports-ui/social-media-data-access';

const MAX_IMAGE_BYTES = 20 * 1024 * 1024;
const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

@Component({
  selector: 'lib-local-video',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <div class="page-header">
        <button class="btn-back" (click)="cancel()">← Back to Channel</button>
        <h1>Local Video Generation</h1>
      </div>

      <!-- Step 1: Reference Image Upload -->
      <div class="step-section">
        <h2 class="step-title">
          <span class="step-num">1</span>
          Reference Image <span class="required">*</span>
        </h2>
        <p class="step-desc">Upload a reference image (JPG, PNG, WEBP — up to 20 MB). This is sent to the local GPU worker as visual context.</p>

        @if (!uploadedObjectKey()) {
          <div class="upload-area"
               [class.upload-area--over]="isDragOver()"
               (click)="refImageInput.click()"
               (dragover)="onDragOver($event)"
               (dragleave)="isDragOver.set(false)"
               (drop)="onDrop($event)">
            <div class="upload-placeholder">
              @if (store.isUploading()) {
                <span class="uploading-indicator"></span>
                <p>Uploading...</p>
              } @else {
                <span class="upload-icon">&#128444;</span>
                <p>Click or drag an image here</p>
                <span class="upload-hint">JPG, PNG, WEBP — up to 20 MB</span>
              }
            </div>
          </div>
        } @else {
          <div class="preview-container">
            <img [src]="previewUrl()" alt="Reference image preview" class="preview-image">
            <div class="preview-footer">
              <span class="preview-label">&#10003; Image uploaded</span>
              <button class="btn-secondary btn-sm" (click)="refImageInput.click()" [disabled]="store.isUploading()">
                Replace
              </button>
            </div>
          </div>
        }

        <input #refImageInput type="file" accept="image/jpeg,image/png,image/webp" style="display:none"
               (change)="onFileSelected($event)">

        @if (imageValidationError()) {
          <p class="error-msg">{{ imageValidationError() }}</p>
        }
        @if (store.uploadStatus() === 'error') {
          <p class="error-msg">{{ store.error() }}</p>
        }
      </div>

      <!-- Step 2: Generation Parameters (shown after image uploaded) -->
      @if (uploadedObjectKey()) {
        <div class="step-section">
          <h2 class="step-title">
            <span class="step-num">2</span>
            Generation Parameters
          </h2>

          <div class="form-group">
            <label>Prompt <span class="required">*</span></label>
            <textarea class="form-control" rows="4"
                      [value]="prompt()"
                      (input)="prompt.set($any($event.target).value)"
                      placeholder="Describe the video you want to generate..."></textarea>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Model</label>
              <select class="form-control"
                      [value]="model()"
                      (change)="model.set($any($event.target).value)">
                <option value="ltx-2.3">ltx-2.3</option>
              </select>
            </div>

            <div class="form-group">
              <label>Duration (seconds)</label>
              <input type="number" class="form-control"
                     min="1" max="30"
                     [value]="durationSeconds()"
                     (input)="durationSeconds.set(+$any($event.target).value)">
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Resolution</label>
              <select class="form-control"
                      [value]="resolution()"
                      (change)="resolution.set($any($event.target).value)">
                <option value="1280x720">1280×720 (HD)</option>
                <option value="1920x1080">1920×1080 (Full HD)</option>
                <option value="854x480">854×480 (SD)</option>
              </select>
            </div>

            <div class="form-group">
              <label>Aspect Ratio</label>
              <select class="form-control"
                      [value]="aspectRatio()"
                      (change)="aspectRatio.set($any($event.target).value)">
                <option value="16:9">16:9 (Landscape)</option>
                <option value="9:16">9:16 (Portrait)</option>
                <option value="1:1">1:1 (Square)</option>
              </select>
            </div>
          </div>

          @if (store.createStatus() === 'error') {
            <p class="error-msg">{{ store.error() }}</p>
          }
        </div>
      }

      <div class="actions">
        <button class="btn-secondary" (click)="cancel()">Cancel</button>
        <button class="btn-local"
                (click)="startGeneration()"
                [disabled]="!canSubmit()">
          {{ store.isCreating() ? 'Starting…' : 'Start Generation' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .container { padding: 24px; max-width: 760px; margin: 0 auto; }
    .page-header { display: flex; align-items: center; gap: 16px; margin-bottom: 28px; }
    .page-header h1 { flex: 1; margin: 0; font-size: 24px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; white-space: nowrap; }
    .step-section { margin-bottom: 28px; background: #fafafa; border: 1px solid #e0e0e0; border-radius: 10px; padding: 20px 24px; }
    .step-title { display: flex; align-items: center; gap: 10px; margin: 0 0 6px; font-size: 17px; }
    .step-num { display: inline-flex; align-items: center; justify-content: center; width: 26px; height: 26px; border-radius: 50%; background: #2e7d32; color: white; font-size: 13px; font-weight: 700; flex-shrink: 0; }
    .step-desc { color: #666; font-size: 13px; margin: 0 0 14px; line-height: 1.5; }
    .required { color: #d32f2f; }
    .upload-area { border: 2px dashed #a5d6a7; border-radius: 12px; padding: 36px 24px; text-align: center; cursor: pointer; transition: border-color 0.2s, background 0.2s; background: #f9fbe7; }
    .upload-area:hover { border-color: #2e7d32; background: #f1f8e9; }
    .upload-area--over { border-color: #2e7d32; background: #f1f8e9; }
    .upload-placeholder { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .upload-icon { font-size: 36px; }
    .upload-hint { color: #999; font-size: 12px; }
    .uploading-indicator { display: inline-block; width: 28px; height: 28px; border: 3px solid #a5d6a7; border-top-color: #2e7d32; border-radius: 50%; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .preview-container { display: flex; flex-direction: column; gap: 0; border: 1px solid #c8e6c9; border-radius: 10px; overflow: hidden; max-width: 480px; }
    .preview-image { width: 100%; max-height: 320px; object-fit: contain; background: #f5f5f5; display: block; }
    .preview-footer { display: flex; align-items: center; justify-content: space-between; padding: 10px 14px; background: #f9fbe7; border-top: 1px solid #c8e6c9; }
    .preview-label { font-size: 13px; color: #2e7d32; font-weight: 500; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-weight: 500; font-size: 14px; margin-bottom: 6px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .form-control { width: 100%; box-sizing: border-box; border: 1px solid #ddd; border-radius: 6px; padding: 8px 12px; font-size: 14px; font-family: inherit; }
    .form-control:focus { outline: none; border-color: #43a047; }
    textarea.form-control { resize: vertical; }
    select.form-control { appearance: auto; }
    .error-msg { color: #d32f2f; font-size: 13px; margin: 8px 0 0; }
    .actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
    .btn-secondary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-sm { padding: 6px 14px; font-size: 13px; }
    .btn-local { background: #2e7d32; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-local:hover { background: #1b5e20; }
    .btn-local:disabled { opacity: 0.6; cursor: not-allowed; }
  `]
})
export class LocalVideoComponent implements OnInit {
  readonly store = inject(RenderJobStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly prompt = signal('');
  readonly model = signal('ltx-2.3');
  readonly durationSeconds = signal(6);
  readonly resolution = signal('1280x720');
  readonly aspectRatio = signal('16:9');
  readonly uploadedObjectKey = signal<string | null>(null);
  readonly previewUrl = signal<string | null>(null);
  readonly imageValidationError = signal<string | null>(null);
  readonly isDragOver = signal(false);

  private channelId = '';

  ngOnInit(): void {
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    this.store.resetJob();
  }

  canSubmit(): boolean {
    return !!this.uploadedObjectKey() &&
      !!this.prompt().trim() &&
      !this.store.isUploading() &&
      !this.store.isCreating();
  }

  cancel(): void {
    this.router.navigate(['..'], { relativeTo: this.route });
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
    const file = event.dataTransfer?.files[0] ?? null;
    if (file) this.handleFile(file);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    if (file) this.handleFile(file);
  }

  private async handleFile(file: File): Promise<void> {
    this.imageValidationError.set(null);
    if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) {
      this.imageValidationError.set('Only JPG, PNG, and WEBP images are accepted.');
      return;
    }
    if (file.size > MAX_IMAGE_BYTES) {
      this.imageValidationError.set('Image must be 20 MB or smaller.');
      return;
    }

    // Show local preview immediately
    this.previewUrl.set(URL.createObjectURL(file));

    const formData = new FormData();
    formData.append('file', file, file.name);
    const objectKey = await this.store.uploadAsset(formData);
    if (objectKey) {
      this.uploadedObjectKey.set(objectKey);
    } else {
      this.previewUrl.set(null);
    }
  }

  async startGeneration(): Promise<void> {
    if (!this.canSubmit()) return;

    const jobId = await this.store.createJob({
      channelId: this.channelId,
      prompt: this.prompt().trim(),
      model: this.model(),
      durationSeconds: this.durationSeconds(),
      resolution: this.resolution(),
      aspectRatio: this.aspectRatio(),
      referenceImageKeys: [this.uploadedObjectKey()!],
      keyframeKeys: [],
      modelOptions: {},
    });

    if (jobId) {
      this.router.navigate([jobId], { relativeTo: this.route });
    }
  }
}
