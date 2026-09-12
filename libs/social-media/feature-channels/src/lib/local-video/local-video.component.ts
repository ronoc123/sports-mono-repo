import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ChannelStore, RenderJobStore } from '@sports-ui/social-media-data-access';
import { environment } from '@sports-ui/api-types';

const MAX_IMAGE_BYTES = 20 * 1024 * 1024;
const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

/** Valid pixel dimensions for each model. Aspect ratio is encoded in the dimensions. */
const MODEL_RESOLUTIONS: Record<string, { value: string; label: string }[]> = {
  'wan-i2v': [
    { value: '832x480',  label: '832×480  — 480p 16:9' },
    { value: '480x832',  label: '480×832  — 480p 9:16' },
    { value: '624x624',  label: '624×624  — 480p 1:1'  },
  ],
  'wan-i2v-720p': [
    { value: '1280x720', label: '1280×720 — 720p 16:9' },
    { value: '720x1280', label: '720×1280 — 720p 9:16' },
    { value: '960x960',  label: '960×960  — 720p 1:1'  },
  ],
  'wan-t2v': [
    { value: '832x480',  label: '832×480  — 480p 16:9' },
    { value: '480x832',  label: '480×832  — 480p 9:16' },
    { value: '624x624',  label: '624×624  — 480p 1:1'  },
    { value: '1280x720', label: '1280×720 — 720p 16:9' },
    { value: '720x1280', label: '720×1280 — 720p 9:16' },
  ],
  'wan-t2v-1.3b': [
    { value: '448x256',  label: '448×256  — 256p 16:9 (fast test)' },
    { value: '256x448',  label: '256×448  — 256p 9:16' },
    { value: '640x360',  label: '640×360  — 360p 16:9' },
    { value: '832x480',  label: '832×480  — 480p 16:9' },
  ],
  'ltx-2': [
    { value: '448x256',  label: '448×256  — 256p 16:9 (fast test)' },
    { value: '256x448',  label: '256×448  — 256p 9:16' },
    { value: '640x360',  label: '640×360  — 360p 16:9' },
    { value: '360x640',  label: '360×640  — 360p 9:16' },
    { value: '854x480',  label: '854×480  — 480p 16:9' },
    { value: '480x854',  label: '480×854  — 480p 9:16' },
    { value: '1280x720', label: '1280×720 — 720p 16:9' },
    { value: '720x1280', label: '720×1280 — 720p 9:16' },
  ],
  'ltx-2.3': [
    { value: '448x256',  label: '448×256  — 256p 16:9 (fast test)' },
    { value: '256x448',  label: '256×448  — 256p 9:16' },
    { value: '640x360',  label: '640×360  — 360p 16:9' },
    { value: '360x640',  label: '360×640  — 360p 9:16' },
    { value: '854x480',  label: '854×480  — 480p 16:9' },
    { value: '480x854',  label: '480×854  — 480p 9:16' },
    { value: '1280x720', label: '1280×720 — 720p 16:9' },
    { value: '720x1280', label: '720×1280 — 720p 9:16' },
  ],
};

const MODEL_DEFAULT_RESOLUTION: Record<string, string> = {
  'wan-i2v':      '832x480',
  'wan-i2v-720p': '1280x720',
  'wan-t2v':      '832x480',
  'wan-t2v-1.3b': '448x256',
  'ltx-2':        '448x256',
  'ltx-2.3':      '448x256',
};

interface StagedKeyframe {
  file: File;
  previewUrl: string;
}

@Component({
  selector: 'lib-local-video',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <div class="page-header">
        <button class="btn-back" (click)="cancel()">← Back to Channel</button>
        <h1>Local Video Generation</h1>
      </div>

      <!-- Channel Reference Image (auto-used) -->
      <div class="step-section">
        <h2 class="step-title">
          <span class="step-num">&#9654;</span>
          Channel Reference Image
        </h2>
        @if (channelStore.selectedChannel(); as channel) {
          @if (channel.characterImageUrl) {
            <p class="step-desc">Your channel's character reference image can be sent to the GPU worker as visual context.</p>
            <div class="ref-image-container" [class.ref-image-container--disabled]="!useReferenceImage()">
              <img [src]="apiOrigin + channel.characterImageUrl" alt="Channel character reference" class="ref-image">
              <div class="ref-image-footer">
                <label class="ref-image-toggle">
                  <input type="checkbox"
                         [checked]="useReferenceImage()"
                         (change)="useReferenceImage.set($any($event.target).checked)">
                  {{ useReferenceImage() ? '&#10003; Send character image to model' : 'Character image disabled for this job' }}
                </label>
              </div>
            </div>
          } @else {
            <div class="no-ref-image-warning">
              <span class="warning-icon">&#9888;</span>
              <div>
                <strong>No character reference image set.</strong>
                <p>Go back to the channel page and upload a character reference image before generating a video.</p>
              </div>
            </div>
          }
        } @else {
          <div class="loading-inline">Loading channel...</div>
        }
      </div>

      <!-- Channel Context Audio (auto-used) -->
      <div class="step-section">
        <h2 class="step-title">
          <span class="step-num">&#9654;</span>
          Channel Context Audio
        </h2>
        @if (channelStore.selectedChannel(); as channel) {
          @if (channel.contextAudioUrl) {
            <p class="step-desc">Your channel's context audio will automatically be mixed into the final video.</p>
            <div class="ref-audio-container">
              <audio controls [src]="apiOrigin + channel.contextAudioUrl" class="ref-audio-player"></audio>
              <div class="ref-image-footer">
                <span class="ref-image-label">&#10003; Auto-mixing channel context audio</span>
              </div>
            </div>
          } @else {
            <p class="step-desc" style="color:#999">No context audio set for this channel — video will be generated without audio. You can add audio on the channel settings page.</p>
          }
        }
      </div>

      <!-- Claude Ideation (optional) -->
      <div class="step-section step-section--ideation">
        <h2 class="step-title">
          <span class="step-num step-num--ai">&#10022;</span>
          Ideate with Claude <span class="optional-label">(Optional)</span>
        </h2>
        <p class="step-desc">Describe a rough idea and Claude will generate a detailed prompt and 6 keyframe scene descriptions using your channel's character and context. You can then apply it directly to the form below.</p>

        <div class="form-group">
          <label>Your rough idea</label>
          <textarea class="form-control" rows="3"
                    [(ngModel)]="ideaInput"
                    placeholder="e.g. 'A day in the life morning routine, energetic and motivational'"></textarea>
        </div>

        <div class="ideation-actions">
          <button class="btn-ai btn-sm"
                  (click)="runIdeation()"
                  [disabled]="!ideaInput.trim() || store.isIdeating()">
            {{ store.isIdeating() ? 'Claude is thinking…' : 'Generate Concept' }}
          </button>
          @if (store.ideateStatus() === 'success' && store.ideateResult()) {
            <button class="btn-apply btn-sm" (click)="applyIdeation()">
              &#10003; Apply to Form
            </button>
          }
        </div>

        @if (store.ideateError()) {
          <p class="error-msg">{{ store.ideateError() }}</p>
        }

        @if (store.ideateStatus() === 'success' && store.ideateResult(); as result) {
          <div class="ideation-result">
            <div class="ideation-result-section">
              <div class="ideation-result-label">Overall Prompt</div>
              <p class="ideation-result-prompt">{{ result.prompt }}</p>
            </div>
            <div class="ideation-result-section">
              <div class="ideation-result-label">Keyframe Scenes</div>
              <ol class="scene-list">
                @for (scene of result.scenes; track $index) {
                  <li class="scene-item">{{ scene }}</li>
                }
              </ol>
            </div>
          </div>
        }
      </div>

      <!-- Keyframe Images (optional) -->
      <div class="step-section">
        <h2 class="step-title">
          <span class="step-num">1</span>
          Keyframe Images <span class="optional-label">(Optional)</span>
        </h2>
        <p class="step-desc">Add one or more keyframe images to guide scene composition. When ready, click "Upload Keyframes" to submit them.</p>

        @if (keyframeUploadStatus() === 'done') {
          <div class="upload-done-banner">
            &#10003; {{ keyframeObjectKeys().length }} keyframe{{ keyframeObjectKeys().length !== 1 ? 's' : '' }} uploaded
          </div>
        } @else {
          <!-- Staged grid -->
          @if (stagedKeyframes().length > 0) {
            <div class="keyframe-grid">
              @for (kf of stagedKeyframes(); track kf.previewUrl; let i = $index) {
                <div class="keyframe-thumb">
                  <img [src]="kf.previewUrl" alt="Keyframe {{ i + 1 }}" class="keyframe-img">
                  <button class="keyframe-remove" (click)="removeKeyframe(i)" title="Remove">&#10005;</button>
                </div>
              }
            </div>
          }

          <!-- Drop / click area -->
          <div class="upload-area upload-area--keyframe"
               [class.upload-area--over]="isKeyframeDragOver()"
               (click)="keyframeInput.click()"
               (dragover)="onKeyframeDragOver($event)"
               (dragleave)="isKeyframeDragOver.set(false)"
               (drop)="onKeyframeDrop($event)">
            <div class="upload-placeholder">
              <span class="upload-icon">&#43;</span>
              <p>Click or drag images here to add keyframes</p>
              <span class="upload-hint">JPG, PNG, WEBP — up to 20 MB each</span>
            </div>
          </div>

          <input #keyframeInput type="file" accept="image/jpeg,image/png,image/webp" multiple style="display:none"
                 (change)="onKeyframeFilesSelected($event)">

          @if (keyframeValidationError()) {
            <p class="error-msg">{{ keyframeValidationError() }}</p>
          }

          <div class="keyframe-actions">
            @if (stagedKeyframes().length > 0) {
              <button class="btn-local btn-sm"
                      (click)="uploadKeyframes()"
                      [disabled]="keyframeUploadStatus() === 'uploading'">
                {{ keyframeUploadStatus() === 'uploading' ? 'Uploading…' : 'Upload Keyframes (' + stagedKeyframes().length + ')' }}
              </button>
            } @else {
              <span class="skip-label">No keyframes — generation uses reference image only.</span>
            }
          </div>

          @if (keyframeUploadStatus() === 'error') {
            <p class="error-msg">Failed to upload one or more keyframes. Please try again.</p>
          }
        }
      </div>

      <!-- Generation Parameters -->
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
                    (change)="onModelChange($any($event.target).value)">
              <optgroup label="Wan 2.1 (Recommended)">
                <option value="wan-i2v">Wan 2.1 i2v 480p — image-to-video 14B</option>
                <option value="wan-i2v-720p">Wan 2.1 i2v 720p — image-to-video 14B</option>
                <option value="wan-t2v">Wan 2.1 t2v — text-to-video 14B</option>
                <option value="wan-t2v-1.3b">Wan 2.1 t2v 1.3B — fast / low VRAM</option>
              </optgroup>
              <optgroup label="LTX Video">
                <option value="ltx-2">LTX-2 19B</option>
                <option value="ltx-2.3">LTX-2.3 22B distilled</option>
              </optgroup>
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

        <div class="form-group">
          <label>Resolution</label>
          <select class="form-control"
                  [value]="resolution()"
                  (change)="resolution.set($any($event.target).value)">
            @for (r of availableResolutions(); track r.value) {
              <option [value]="r.value">{{ r.label }}</option>
            }
          </select>
        </div>

        @if (generationError()) {
          <p class="error-msg">{{ generationError() }}</p>
        }
        @if (store.createStatus() === 'error') {
          <p class="error-msg">{{ store.error() }}</p>
        }
      </div>

      <div class="actions">
        <button class="btn-secondary" (click)="cancel()">Cancel</button>
        <button class="btn-local"
                (click)="startGeneration()"
                [disabled]="!canSubmit()">
          {{ buttonLabel() }}
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
    .optional-label { font-size: 13px; font-weight: 400; color: #999; }
    .required { color: #d32f2f; }
    .loading-inline { color: #999; font-size: 14px; padding: 8px 0; }
    .ref-audio-container { display: flex; flex-direction: column; border: 1px solid #c8e6c9; border-radius: 10px; overflow: hidden; max-width: 420px; }
    .ref-audio-player { width: 100%; display: block; background: #f5f5f5; padding: 12px; box-sizing: border-box; }
    .ref-image-container { display: flex; flex-direction: column; border: 1px solid #c8e6c9; border-radius: 10px; overflow: hidden; max-width: 420px; }
    .ref-image { width: 100%; max-height: 240px; object-fit: contain; background: #f5f5f5; display: block; }
    .ref-image-footer { display: flex; align-items: center; padding: 10px 14px; background: #f1f8e9; border-top: 1px solid #c8e6c9; }
    .ref-image-label { font-size: 13px; color: #2e7d32; font-weight: 500; }
    .ref-image-container--disabled { opacity: 0.45; }
    .ref-image-toggle { display: flex; align-items: center; gap: 8px; cursor: pointer; font-size: 13px; color: #2e7d32; font-weight: 500; user-select: none; }
    .no-ref-image-warning { display: flex; gap: 12px; align-items: flex-start; background: #fff8e1; border: 1px solid #ffe082; border-radius: 8px; padding: 14px 16px; }
    .warning-icon { font-size: 20px; color: #f57f17; flex-shrink: 0; }
    .no-ref-image-warning strong { display: block; font-size: 14px; color: #e65100; margin-bottom: 4px; }
    .no-ref-image-warning p { margin: 0; font-size: 13px; color: #555; }
    .keyframe-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(100px, 1fr)); gap: 10px; margin-bottom: 14px; }
    .keyframe-thumb { position: relative; border-radius: 8px; overflow: hidden; border: 1px solid #c8e6c9; }
    .keyframe-img { width: 100%; height: 90px; object-fit: cover; display: block; background: #f0f0f0; }
    .keyframe-remove { position: absolute; top: 4px; right: 4px; background: rgba(0,0,0,0.55); color: white; border: none; border-radius: 50%; width: 20px; height: 20px; cursor: pointer; font-size: 11px; display: flex; align-items: center; justify-content: center; padding: 0; }
    .keyframe-remove:hover { background: rgba(211,47,47,0.85); }
    .upload-area { border: 2px dashed #a5d6a7; border-radius: 12px; padding: 24px 16px; text-align: center; cursor: pointer; transition: border-color 0.2s, background 0.2s; background: #f9fbe7; }
    .upload-area:hover { border-color: #2e7d32; background: #f1f8e9; }
    .upload-area--over { border-color: #2e7d32; background: #f1f8e9; }
    .upload-area--keyframe { padding: 18px 16px; }
    .upload-placeholder { display: flex; flex-direction: column; align-items: center; gap: 6px; }
    .upload-icon { font-size: 28px; color: #43a047; }
    .upload-placeholder p { margin: 0; font-size: 14px; color: #555; }
    .upload-hint { color: #999; font-size: 12px; }
    .keyframe-actions { margin-top: 14px; display: flex; align-items: center; gap: 12px; }
    .skip-label { font-size: 13px; color: #999; font-style: italic; }
    .upload-done-banner { display: flex; align-items: center; gap: 8px; background: #e8f5e9; border: 1px solid #c8e6c9; border-radius: 8px; padding: 12px 16px; font-size: 14px; color: #2e7d32; font-weight: 500; }
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
    .btn-sm { padding: 8px 18px; font-size: 13px; }
    .btn-local { background: #2e7d32; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-local:hover { background: #1b5e20; }
    .btn-local:disabled { opacity: 0.6; cursor: not-allowed; }
    /* Ideation section */
    .step-section--ideation { border-color: #d8b4fe; background: #faf5ff; }
    .step-num--ai { background: #7c3aed; font-size: 15px; }
    .btn-ai { background: #7c3aed; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-ai:hover { background: #6d28d9; }
    .btn-ai:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-apply { background: #059669; color: white; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-apply:hover { background: #047857; }
    .ideation-actions { display: flex; gap: 10px; align-items: center; margin-top: 4px; flex-wrap: wrap; }
    .ideation-result { margin-top: 18px; border: 1px solid #d8b4fe; border-radius: 8px; overflow: hidden; }
    .ideation-result-section { padding: 14px 16px; }
    .ideation-result-section + .ideation-result-section { border-top: 1px solid #e9d5ff; }
    .ideation-result-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #7c3aed; margin-bottom: 8px; }
    .ideation-result-prompt { margin: 0; font-size: 14px; color: #1e1b4b; line-height: 1.6; }
    .scene-list { margin: 0; padding-left: 20px; display: flex; flex-direction: column; gap: 8px; }
    .scene-item { font-size: 13px; color: #374151; line-height: 1.5; }
  `]
})
export class LocalVideoComponent implements OnInit {
  readonly store = inject(RenderJobStore);
  readonly channelStore = inject(ChannelStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly apiOrigin = environment.apiUrl + environment.socialMediaApi.split('/api')[0];

  readonly prompt = signal('');
  readonly model = signal('wan-i2v');
  readonly durationSeconds = signal(2);
  readonly resolution = signal(MODEL_DEFAULT_RESOLUTION['wan-i2v']);

  /** Resolution options valid for the currently selected model. */
  readonly availableResolutions = computed(() =>
    MODEL_RESOLUTIONS[this.model()] ?? MODEL_RESOLUTIONS['wan-i2v']
  );

  readonly stagedKeyframes = signal<StagedKeyframe[]>([]);
  readonly keyframeObjectKeys = signal<string[]>([]);
  readonly keyframeUploadStatus = signal<'idle' | 'uploading' | 'done' | 'error'>('idle');
  readonly keyframeValidationError = signal<string | null>(null);
  readonly isKeyframeDragOver = signal(false);

  readonly useReferenceImage = signal(true);
  readonly generationError = signal<string | null>(null);

  ideaInput = '';
  private channelId = '';

  ngOnInit(): void {
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    this.store.resetJob();
    const existing = this.channelStore.selectedChannel();
    if (!existing || existing.id !== this.channelId) {
      this.channelStore.loadChannel(this.channelId);
    }
  }

  buttonLabel(): string {
    if (this.store.isUploading()) return 'Uploading…';
    if (this.store.isCreating()) return 'Starting…';
    return 'Start Generation';
  }

  canSubmit(): boolean {
    return !!this.prompt().trim() &&
      !this.store.isUploading() &&
      !this.store.isCreating() &&
      this.keyframeUploadStatus() !== 'uploading';
  }

  cancel(): void {
    this.router.navigate(['/channels', this.channelId]);
  }

  // --- Model / resolution ---

  onModelChange(newModel: string): void {
    this.model.set(newModel);
    const options = MODEL_RESOLUTIONS[newModel] ?? [];
    if (!options.find(r => r.value === this.resolution())) {
      this.resolution.set(MODEL_DEFAULT_RESOLUTION[newModel] ?? options[0]?.value ?? '832x480');
    }
  }

  private aspectRatioFromResolution(resolution: string): string {
    const [w, h] = resolution.split('x').map(Number);
    return w > h ? '16:9' : h > w ? '9:16' : '1:1';
  }

  // --- Claude Ideation ---

  async runIdeation(): Promise<void> {
    if (!this.ideaInput.trim()) return;
    await this.store.ideate({ channelId: this.channelId, userIdea: this.ideaInput.trim() });
  }

  applyIdeation(): void {
    const result = this.store.ideateResult();
    if (!result) return;
    this.prompt.set(result.prompt);
  }

  // --- Keyframes ---

  onKeyframeDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isKeyframeDragOver.set(true);
  }

  onKeyframeDrop(event: DragEvent): void {
    event.preventDefault();
    this.isKeyframeDragOver.set(false);
    // Copy to array immediately — DataTransfer files may be cleared on next tick
    const files = event.dataTransfer?.files ? Array.from(event.dataTransfer.files) : [];
    if (files.length > 0) this.addKeyframeFiles(files);
  }

  onKeyframeFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    // Snapshot to array BEFORE clearing input.value — FileList is a live reference
    // and becomes empty once the input is reset
    const files = input.files ? Array.from(input.files) : [];
    input.value = '';
    if (files.length > 0) this.addKeyframeFiles(files);
  }

  private addKeyframeFiles(files: File[]): void {
    this.keyframeValidationError.set(null);
    const toAdd: StagedKeyframe[] = [];
    for (const file of files) {
      if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) {
        this.keyframeValidationError.set(`"${file.name}" is not an accepted image type (JPG, PNG, WEBP).`);
        continue;
      }
      if (file.size > MAX_IMAGE_BYTES) {
        this.keyframeValidationError.set(`"${file.name}" exceeds the 20 MB limit.`);
        continue;
      }
      toAdd.push({ file, previewUrl: URL.createObjectURL(file) });
    }
    if (toAdd.length > 0) {
      this.stagedKeyframes.update(prev => [...prev, ...toAdd]);
    }
  }

  removeKeyframe(index: number): void {
    this.stagedKeyframes.update(prev => {
      const copy = [...prev];
      URL.revokeObjectURL(copy[index].previewUrl);
      copy.splice(index, 1);
      return copy;
    });
  }

  async uploadKeyframes(): Promise<void> {
    this.keyframeUploadStatus.set('uploading');
    const objectKeys: string[] = [];
    for (const staged of this.stagedKeyframes()) {
      const formData = new FormData();
      formData.append('file', staged.file, staged.file.name);
      const key = await this.store.uploadAsset(formData);
      if (!key) {
        this.keyframeUploadStatus.set('error');
        return;
      }
      objectKeys.push(key);
    }
    this.keyframeObjectKeys.set(objectKeys);
    this.keyframeUploadStatus.set('done');
  }

  // --- Start Generation ---

  async startGeneration(): Promise<void> {
    if (!this.canSubmit()) return;

    this.generationError.set(null);

    // Reference image keys are intentionally empty — the backend auto-uploads
    // the channel's character image (CharacterImagePath) to R2 on job creation.
    const jobId = await this.store.createJob({
      channelId: this.channelId,
      prompt: this.prompt().trim(),
      model: this.model(),
      durationSeconds: this.durationSeconds(),
      resolution: this.resolution(),
      aspectRatio: this.aspectRatioFromResolution(this.resolution()),
      referenceImageKeys: [],
      keyframeKeys: this.keyframeObjectKeys(),
      modelOptions: {},
      useChannelImage: this.useReferenceImage(),
    });

    if (jobId) {
      this.router.navigate([jobId], { relativeTo: this.route });
    }
  }
}
