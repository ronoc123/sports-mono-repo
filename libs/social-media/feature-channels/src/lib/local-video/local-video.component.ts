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
  // MiniMax H3 — supports start+end frame guidance, PDD 8-step
  'h3-fl2va': [
    { value: '480x832',  label: '480×832  — 480p 9:16 (recommended)' },
    { value: '832x480',  label: '832×480  — 480p 16:9' },
    { value: '720x1280', label: '720×1280 — 720p 9:16' },
    { value: '1280x720', label: '1280×720 — 720p 16:9' },
  ],
  'h3-ref2va': [
    { value: '480x832',  label: '480×832  — 480p 9:16 (recommended)' },
    { value: '832x480',  label: '832×480  — 480p 16:9' },
    { value: '720x1280', label: '720×1280 — 720p 9:16' },
    { value: '1280x720', label: '1280×720 — 720p 16:9' },
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
  'ltx-2.5': [
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
  'h3-fl2va':     '480x832',
  'h3-ref2va':    '480x832',
  'ltx-2':        '448x256',
  'ltx-2.3':      '448x256',
  'ltx-2.5':      '448x256',
};

interface StagedKeyframe {
  file: File;
  previewUrl: string;
}

interface Clip {
  prompt: string;
  /** Start frame / anchor image (image_start). Overrides the global channel reference image for this clip. */
  stagedStartFrame: StagedKeyframe | null;
  startFrameKey: string | null;
  startFrameUploadStatus: 'idle' | 'uploading' | 'done' | 'error';
  /** End frame image (image_end / keyframe). */
  stagedKeyframe: StagedKeyframe | null;
  keyframeKey: string | null;
  keyframeUploadStatus: 'idle' | 'uploading' | 'done' | 'error';
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

      <!-- Generation Settings (shared across all clips) -->
      <div class="step-section">
        <h2 class="step-title">
          <span class="step-num">1</span>
          Generation Settings
        </h2>

        <div class="form-row">
          <div class="form-group">
            <label>Model</label>
            <select class="form-control"
                    [value]="model()"
                    (change)="onModelChange($any($event.target).value)">
              <optgroup label="MiniMax H3 (Best for transformation / before-after)">
                <option value="h3-fl2va">H3 FL2VA Pruned 20B — 8-step PDD</option>
                <option value="h3-ref2va">H3 Ref2VA Pruned 20B — 8-step PDD</option>
              </optgroup>
              <optgroup label="LTX Video (Fastest)">
                <option value="ltx-2.5">LTX 2.5 22B distilled</option>
                <option value="ltx-2.3">LTX 2.3 22B distilled</option>
                <option value="ltx-2">LTX-2 19B</option>
              </optgroup>
              <optgroup label="Wan 2.1">
                <option value="wan-i2v">Wan 2.1 i2v 480p — image-to-video 14B</option>
                <option value="wan-i2v-720p">Wan 2.1 i2v 720p — image-to-video 14B</option>
                <option value="wan-t2v">Wan 2.1 t2v — text-to-video 14B</option>
                <option value="wan-t2v-1.3b">Wan 2.1 t2v 1.3B — fast / low VRAM</option>
              </optgroup>
            </select>
          </div>

          <div class="form-group">
            <label>Duration per clip (seconds)</label>
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
      </div>

      <!-- Clips -->
      <div class="step-section">
        <h2 class="step-title">
          <span class="step-num">2</span>
          Clips
        </h2>
        <p class="step-desc">
          All clips are submitted as one job and concatenated into a single output video.
          Use 4–5 s clips to keep generation fast and quality high.
          Add an end-frame image to guide how each clip should end.
        </p>

        @for (clip of clips(); track $index; let i = $index) {
          <div class="clip-card">
            <div class="clip-header">
              <span class="clip-label">Clip {{ i + 1 }}</span>
              @if (clips().length > 1) {
                <button class="clip-remove" (click)="removeClip(i)" title="Remove clip">&#10005;</button>
              }
            </div>

            <textarea class="form-control clip-prompt" rows="3"
                      [value]="clip.prompt"
                      (input)="updateClipPrompt(i, $any($event.target).value)"
                      placeholder="Describe what happens in this clip…"></textarea>

            <!-- Anchor images row -->
            <div class="clip-frames-row">
              <!-- Start frame (anchor / image_start) -->
              <div class="clip-frame-slot">
                <div class="clip-frame-slot-label">Start frame <span class="optional-label">(optional)</span></div>
                @if (clip.startFrameUploadStatus === 'done' && clip.stagedStartFrame) {
                  <div class="clip-frame-preview">
                    <img [src]="clip.stagedStartFrame.previewUrl" class="clip-frame-img" alt="Start frame">
                    <button class="clip-frame-clear" (click)="clearClipStartFrame(i)">&#10005;</button>
                  </div>
                } @else if (clip.startFrameUploadStatus === 'uploading') {
                  <span class="clip-endframe-hint">Uploading…</span>
                } @else {
                  <label class="clip-frame-add" [class.clip-frame-add--error]="clip.startFrameUploadStatus === 'error'">
                    <input type="file" accept="image/jpeg,image/png,image/webp" style="display:none"
                           (change)="onClipStartFrameSelected(i, $event)">
                    <span class="clip-frame-add-icon">&#8680;</span>
                    <span>{{ clip.startFrameUploadStatus === 'error' ? 'Retry' : 'Add' }}</span>
                  </label>
                }
              </div>

              <div class="clip-frames-arrow">&#8594;</div>

              <!-- End frame (keyframe / image_end) -->
              <div class="clip-frame-slot">
                <div class="clip-frame-slot-label">End frame <span class="optional-label">(optional)</span></div>
                @if (clip.keyframeUploadStatus === 'done' && clip.stagedKeyframe) {
                  <div class="clip-frame-preview">
                    <img [src]="clip.stagedKeyframe.previewUrl" class="clip-frame-img" alt="End frame">
                    <button class="clip-frame-clear" (click)="clearClipKeyframe(i)">&#10005;</button>
                  </div>
                } @else if (clip.keyframeUploadStatus === 'uploading') {
                  <span class="clip-endframe-hint">Uploading…</span>
                } @else {
                  <label class="clip-frame-add" [class.clip-frame-add--error]="clip.keyframeUploadStatus === 'error'">
                    <input type="file" accept="image/jpeg,image/png,image/webp" style="display:none"
                           (change)="onClipKeyframeSelected(i, $event)">
                    <span class="clip-frame-add-icon">&#8680;</span>
                    <span>{{ clip.keyframeUploadStatus === 'error' ? 'Retry' : 'Add' }}</span>
                  </label>
                }
              </div>
            </div>
          </div>
        }

        <button class="btn-add-clip" (click)="addClip()">&#43; Add Clip</button>

        @if (generationError()) {
          <p class="error-msg" style="margin-top:12px">{{ generationError() }}</p>
        }
        @if (store.createStatus() === 'error') {
          <p class="error-msg" style="margin-top:12px">{{ store.error() }}</p>
        }
      </div>

      <div class="actions">
        <button class="btn-secondary" (click)="cancel()">Cancel</button>
        <button class="btn-local"
                (click)="startGeneration()"
                [disabled]="!canSubmit() || isGenerating()">
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
    /* Clips */
    .clip-card { border: 1px solid #e0e0e0; border-radius: 8px; padding: 14px 16px; margin-bottom: 12px; background: white; }
    .clip-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 10px; }
    .clip-label { font-weight: 600; font-size: 13px; color: #444; }
    .clip-remove { background: none; border: none; color: #999; cursor: pointer; font-size: 16px; padding: 0 4px; line-height: 1; }
    .clip-remove:hover { color: #d32f2f; }
    .clip-prompt { margin-bottom: 10px; }
    .clip-frames-row { display: flex; align-items: center; gap: 8px; margin-top: 6px; }
    .clip-frame-slot { display: flex; flex-direction: column; gap: 4px; flex: 1; min-width: 0; }
    .clip-frame-slot-label { font-size: 11px; font-weight: 600; color: #666; text-transform: uppercase; letter-spacing: 0.04em; }
    .clip-frames-arrow { color: #bdbdbd; font-size: 18px; flex-shrink: 0; margin-top: 18px; }
    .clip-frame-preview { position: relative; width: 64px; height: 64px; }
    .clip-frame-img { width: 64px; height: 64px; object-fit: cover; border-radius: 6px; border: 1px solid #c8e6c9; display: block; }
    .clip-frame-clear { position: absolute; top: 2px; right: 2px; background: rgba(0,0,0,0.55); color: white; border: none; border-radius: 50%; width: 18px; height: 18px; cursor: pointer; font-size: 10px; display: flex; align-items: center; justify-content: center; padding: 0; line-height: 1; }
    .clip-frame-clear:hover { background: rgba(211,47,47,0.85); }
    .clip-frame-add { display: inline-flex; flex-direction: column; align-items: center; justify-content: center; gap: 2px; cursor: pointer; font-size: 11px; color: #1976d2; border: 1px dashed #90caf9; border-radius: 6px; padding: 6px 8px; user-select: none; width: 64px; height: 64px; box-sizing: border-box; text-align: center; }
    .clip-frame-add:hover { background: #e3f2fd; }
    .clip-frame-add--error { border-color: #ef9a9a; color: #d32f2f; }
    .clip-frame-add-icon { font-size: 16px; }
    .clip-endframe-hint { font-size: 12px; color: #999; font-style: italic; }
    .clip-endframe-clear:hover { color: #d32f2f; }
    .btn-add-clip { background: none; border: 1px dashed #a5d6a7; color: #2e7d32; padding: 8px 18px; border-radius: 6px; cursor: pointer; font-size: 13px; width: 100%; margin-top: 4px; }
    .btn-add-clip:hover { background: #f1f8e9; }
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

  readonly model = signal('h3-fl2va');
  readonly durationSeconds = signal(2);
  readonly resolution = signal(MODEL_DEFAULT_RESOLUTION['h3-fl2va']);

  /** Resolution options valid for the currently selected model. */
  readonly availableResolutions = computed(() =>
    MODEL_RESOLUTIONS[this.model()] ?? MODEL_RESOLUTIONS['wan-i2v']
  );

  readonly clips = signal<Clip[]>([this.emptyClip()]);
  readonly isGenerating = signal(false);
  readonly useReferenceImage = signal(true);
  readonly generationError = signal<string | null>(null);

  ideaInput = '';
  private channelId = '';

  private emptyClip(): Clip {
    return {
      prompt: '',
      stagedStartFrame: null, startFrameKey: null, startFrameUploadStatus: 'idle',
      stagedKeyframe: null,   keyframeKey: null,   keyframeUploadStatus: 'idle',
    };
  }

  ngOnInit(): void {
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    this.store.resetJob();
    const existing = this.channelStore.selectedChannel();
    if (!existing || existing.id !== this.channelId) {
      this.channelStore.loadChannel(this.channelId);
    }
  }

  buttonLabel(): string {
    if (this.isGenerating()) return 'Creating job…';
    const n = this.clips().length;
    return n > 1 ? `Generate ${n} Clips` : 'Generate';
  }

  canSubmit(): boolean {
    return this.clips().every(c => c.prompt.trim()) &&
      !this.isGenerating() &&
      !this.clips().some(c =>
        c.keyframeUploadStatus === 'uploading' ||
        c.startFrameUploadStatus === 'uploading'
      );
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
    const scenes = result.scenes ?? [];
    if (scenes.length > 0) {
      // One clip per keyframe scene — replaces existing clips entirely
      this.clips.set((scenes as string[]).map(s => ({ ...this.emptyClip(), prompt: s })));
    } else {
      this.updateClipPrompt(0, result.prompt);
    }
  }

  // --- Clips ---

  private updateClipField(index: number, partial: Partial<Clip>): void {
    this.clips.update(prev => {
      const copy = [...prev];
      copy[index] = { ...copy[index], ...partial };
      return copy;
    });
  }

  updateClipPrompt(index: number, value: string): void {
    this.updateClipField(index, { prompt: value });
  }

  addClip(): void {
    this.clips.update(prev => [...prev, this.emptyClip()]);
  }

  removeClip(index: number): void {
    const clip = this.clips()[index];
    if (clip.stagedStartFrame) URL.revokeObjectURL(clip.stagedStartFrame.previewUrl);
    if (clip.stagedKeyframe)   URL.revokeObjectURL(clip.stagedKeyframe.previewUrl);
    this.clips.update(prev => prev.filter((_, i) => i !== index));
  }

  onClipKeyframeSelected(clipIndex: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    if (!file) return;

    if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) return;
    if (file.size > MAX_IMAGE_BYTES) return;

    const old = this.clips()[clipIndex].stagedKeyframe;
    if (old) URL.revokeObjectURL(old.previewUrl);

    this.updateClipField(clipIndex, {
      stagedKeyframe: { file, previewUrl: URL.createObjectURL(file) },
      keyframeKey: null,
      keyframeUploadStatus: 'idle',
    });

    this.uploadClipKeyframe(clipIndex, file);
  }

  private async uploadClipKeyframe(clipIndex: number, file: File): Promise<void> {
    this.updateClipField(clipIndex, { keyframeUploadStatus: 'uploading' });
    const formData = new FormData();
    formData.append('file', file, file.name);
    const key = await this.store.uploadAsset(formData);
    this.updateClipField(clipIndex, {
      keyframeKey: key ?? null,
      keyframeUploadStatus: key ? 'done' : 'error',
    });
  }

  clearClipKeyframe(clipIndex: number): void {
    const kf = this.clips()[clipIndex].stagedKeyframe;
    if (kf) URL.revokeObjectURL(kf.previewUrl);
    this.updateClipField(clipIndex, {
      stagedKeyframe: null,
      keyframeKey: null,
      keyframeUploadStatus: 'idle',
    });
  }

  onClipStartFrameSelected(clipIndex: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    if (!file) return;
    if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) return;
    if (file.size > MAX_IMAGE_BYTES) return;

    const old = this.clips()[clipIndex].stagedStartFrame;
    if (old) URL.revokeObjectURL(old.previewUrl);

    this.updateClipField(clipIndex, {
      stagedStartFrame: { file, previewUrl: URL.createObjectURL(file) },
      startFrameKey: null,
      startFrameUploadStatus: 'idle',
    });
    this.uploadClipStartFrame(clipIndex, file);
  }

  private async uploadClipStartFrame(clipIndex: number, file: File): Promise<void> {
    this.updateClipField(clipIndex, { startFrameUploadStatus: 'uploading' });
    const formData = new FormData();
    formData.append('file', file, file.name);
    const key = await this.store.uploadAsset(formData);
    this.updateClipField(clipIndex, {
      startFrameKey: key ?? null,
      startFrameUploadStatus: key ? 'done' : 'error',
    });
  }

  clearClipStartFrame(clipIndex: number): void {
    const sf = this.clips()[clipIndex].stagedStartFrame;
    if (sf) URL.revokeObjectURL(sf.previewUrl);
    this.updateClipField(clipIndex, {
      stagedStartFrame: null,
      startFrameKey: null,
      startFrameUploadStatus: 'idle',
    });
  }

  // --- Generate ---

  async startGeneration(): Promise<void> {
    if (!this.canSubmit()) return;

    this.generationError.set(null);
    this.isGenerating.set(true);

    const jobId = await this.store.createJob({
      channelId: this.channelId,
      model: this.model(),
      durationSeconds: this.durationSeconds(),
      resolution: this.resolution(),
      aspectRatio: this.aspectRatioFromResolution(this.resolution()),
      useChannelImage: this.useReferenceImage(),
      clips: this.clips().map(c => ({
        prompt: c.prompt.trim(),
        startImageKey: c.startFrameKey ?? null,
        endImageKey: c.keyframeKey ?? null,
      })),
    });

    this.isGenerating.set(false);

    if (!jobId) {
      this.generationError.set('Failed to create job.');
      return;
    }

    this.router.navigate([jobId], { relativeTo: this.route });
  }
}
