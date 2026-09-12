import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ChannelStore, RenderJobStore } from '@sports-ui/social-media-data-access';

const MAX_IMAGE_BYTES = 20 * 1024 * 1024;
const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

interface StagedKeyframe {
  file: File;
  previewUrl: string;
}

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

            <!-- Regenerate panel -->
            @if (!showRegenForm()) {
              <div class="regen-bar">
                <button class="btn-regen" (click)="openRegenForm()">&#8635; Regenerate with Changes</button>
                <span class="regen-bar-hint">Refine prompt, resolution, or model — uses this video as the generation starting point.</span>
              </div>
            } @else {
              <div class="regen-panel">
                <div class="regen-header">
                  <span class="regen-title">Regenerate with Changes</span>
                  <button class="btn-link" (click)="showRegenForm.set(false)">Cancel</button>
                </div>

                <div class="regen-info-pill">
                  <span class="regen-info-icon">&#9654;</span>
                  Using this video as the generation starting point (video-to-video)
                </div>

                <!-- Optional keyframe images -->
                <div class="regen-section">
                  <p class="regen-section-label">Keyframe Images <span class="optional-label">(Optional)</span></p>

                  @if (keyframeUploadStatus() === 'done') {
                    <div class="upload-done-banner">
                      &#10003; {{ keyframeObjectKeys().length }} keyframe{{ keyframeObjectKeys().length !== 1 ? 's' : '' }} uploaded
                    </div>
                  } @else {
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

                    <div class="upload-area upload-area--keyframe"
                         [class.upload-area--over]="isKeyframeDragOver()"
                         (click)="regenKeyframeInput.click()"
                         (dragover)="onKeyframeDragOver($event)"
                         (dragleave)="isKeyframeDragOver.set(false)"
                         (drop)="onKeyframeDrop($event)">
                      <div class="upload-placeholder">
                        <span class="upload-icon">&#43;</span>
                        <p>Click or drag images here to add keyframes</p>
                        <span class="upload-hint">JPG, PNG, WEBP — up to 20 MB each</span>
                      </div>
                    </div>

                    <input #regenKeyframeInput type="file" accept="image/jpeg,image/png,image/webp" multiple style="display:none"
                           (change)="onKeyframeFilesSelected($event)">

                    @if (keyframeValidationError()) {
                      <p class="error-msg">{{ keyframeValidationError() }}</p>
                    }

                    @if (stagedKeyframes().length > 0) {
                      <div class="keyframe-actions">
                        <button class="btn-local btn-sm"
                                (click)="uploadKeyframes()"
                                [disabled]="keyframeUploadStatus() === 'uploading'">
                          {{ keyframeUploadStatus() === 'uploading' ? 'Uploading…' : 'Upload Keyframes (' + stagedKeyframes().length + ')' }}
                        </button>
                        @if (keyframeUploadStatus() === 'error') {
                          <p class="error-msg">Failed to upload keyframes. Please try again.</p>
                        }
                      </div>
                    }
                  }
                </div>

                <!-- Prompt -->
                <div class="form-group">
                  <label class="form-label">Prompt <span class="required">*</span></label>
                  <textarea class="form-control" rows="4"
                            [(ngModel)]="regenPrompt"
                            placeholder="Describe the video you want to generate..."
                            [disabled]="isRegenerating()"></textarea>
                </div>

                <!-- Model + Duration -->
                <div class="form-row">
                  <div class="form-group">
                    <label class="form-label">Model</label>
                    <select class="form-control" [(ngModel)]="regenModel" [disabled]="isRegenerating()">
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
                    <label class="form-label">Duration (seconds)</label>
                    <input type="number" class="form-control" min="1" max="30"
                           [(ngModel)]="regenDuration"
                           [disabled]="isRegenerating()">
                  </div>
                </div>

                <!-- Resolution + Aspect Ratio -->
                <div class="form-row">
                  <div class="form-group">
                    <label class="form-label">Resolution</label>
                    <select class="form-control" [(ngModel)]="regenResolution" [disabled]="isRegenerating()">
                      <option value="448x256">448×256 (256p — fastest test)</option>
                      <option value="640x360">640×360 (360p — fast)</option>
                      <option value="854x480">854×480 (SD)</option>
                      <option value="1280x720">1280×720 (HD)</option>
                      <option value="1920x1080">1920×1080 (Full HD)</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label class="form-label">Aspect Ratio</label>
                    <select class="form-control" [(ngModel)]="regenAspectRatio" [disabled]="isRegenerating()">
                      <option value="16:9">16:9 (Landscape)</option>
                      <option value="9:16">9:16 (Portrait)</option>
                      <option value="1:1">1:1 (Square)</option>
                    </select>
                  </div>
                </div>

                @if (regenError()) {
                  <p class="error-msg">{{ regenError() }}</p>
                }

                <div class="regen-actions">
                  <button class="btn-secondary" (click)="showRegenForm.set(false)" [disabled]="isRegenerating()">Cancel</button>
                  <button class="btn-local"
                          (click)="submitRegen()"
                          [disabled]="!regenPrompt.trim() || isRegenerating() || keyframeUploadStatus() === 'uploading'">
                    @if (isRegenerating()) {
                      <span class="spinner spinner--sm spinner--white"></span>
                    }
                    {{ isRegenerating() ? 'Starting…' : 'Generate New Video' }}
                  </button>
                </div>
              </div>
            }

            <!-- Post to Social Media -->
            <div class="post-section">
              <h3 class="post-section-title">Post to Social Media</h3>

              <!-- Form fields -->
              <div class="form-group">
                <label class="form-label">Title <span class="required">*</span></label>
                <input type="text"
                       class="form-control"
                       [(ngModel)]="postTitle"
                       placeholder="Enter a title for your video"
                       [disabled]="isPosting()" />
              </div>

              <div class="form-group">
                <label class="form-label">Description</label>
                <textarea class="form-control"
                          rows="3"
                          [(ngModel)]="postDescription"
                          placeholder="Optional description"
                          [disabled]="isPosting()"></textarea>
              </div>

              <div class="form-group">
                <label class="form-label">Hashtags <span class="form-hint">(comma-separated, without #)</span></label>
                <input type="text"
                       class="form-control"
                       [(ngModel)]="postHashtags"
                       placeholder="sports, highlights, fitness"
                       [disabled]="isPosting()" />
              </div>

              @if (store.postError()) {
                <p class="error-msg">{{ store.postError() }}</p>
              }

              <!-- Linked accounts -->
              @let channel = channelStore.selectedChannel();
              @if (!channel) {
                <div class="accounts-loading">
                  <span class="spinner spinner--green"></span>
                  <span>Loading linked accounts...</span>
                </div>
              } @else if (channel.linkedAccounts.length === 0) {
                <div class="no-accounts-msg">
                  <span class="no-accounts-icon">&#9888;</span>
                  No social media accounts are linked to this channel. Go to channel settings to connect one.
                </div>
              } @else {
                <div class="accounts-section">
                  <p class="accounts-label">Linked accounts</p>
                  <div class="accounts-list">
                    @for (account of channel.linkedAccounts; track account.platform) {
                      <div class="account-row" [class.account-row--invalid]="account.tokenStatus === 'invalid'">
                        <div class="account-info">
                          <span class="platform-badge platform-badge--{{ account.platform.toLowerCase() }}">
                            {{ account.platform }}
                          </span>
                          <span class="account-name">{{ account.accountDisplayName }}</span>
                          @if (account.tokenStatus === 'invalid') {
                            <span class="token-warn">Token expired — reconnect in channel settings</span>
                          }
                        </div>
                        <button class="btn-post-single"
                                (click)="submitPost(account.platform)"
                                [disabled]="!postTitle.trim() || isPosting() || account.tokenStatus === 'invalid'">
                          @if (postingTarget() === account.platform) {
                            <span class="spinner spinner--sm spinner--white"></span>
                          }
                          Post to {{ account.platform }}
                        </button>
                      </div>
                    }
                  </div>

                  <div class="post-all-row">
                    <button class="btn-post-all"
                            (click)="submitPost()"
                            [disabled]="!postTitle.trim() || isPosting()">
                      @if (postingTarget() === 'all') {
                        <span class="spinner spinner--sm spinner--white"></span>
                      }
                      Post to All Accounts
                    </button>
                  </div>
                </div>
              }

              <div class="post-actions">
                <button class="btn-secondary" (click)="backToChannel()" [disabled]="isPosting()">
                  Back to Channel
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
    .spinner { display: inline-block; width: 24px; height: 24px; border: 3px solid rgba(0,0,0,0.12); border-top-color: #2e7d32; border-radius: 50%; animation: spin 0.8s linear infinite; }
    .spinner--large { width: 48px; height: 48px; border-width: 4px; }
    .spinner--green { border-color: #a5d6a7; border-top-color: #2e7d32; }
    .spinner--sm { width: 14px; height: 14px; border-width: 2px; display: inline-block; vertical-align: middle; margin-right: 6px; }
    .spinner--white { border-color: rgba(255,255,255,0.4); border-top-color: white; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .status-badge { padding: 4px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; }
    .status-badge--pending { background: #fff8e1; color: #f57f17; }
    .status-badge--processing { background: #e8f5e9; color: #2e7d32; }
    .error-panel { background: #ffebee; border: 1px solid #ef9a9a; }
    .error-panel h2 { margin: 0 0 12px; color: #c62828; font-size: 20px; }
    .error-detail { color: #c62828; font-size: 14px; margin: 0 0 20px; line-height: 1.5; }
    .error-msg { color: #d32f2f; font-size: 13px; margin: 8px 0 12px; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
    .btn-secondary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-local { background: #2e7d32; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; display: inline-flex; align-items: center; }
    .btn-local:hover { background: #1b5e20; }
    .btn-local:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-sm { padding: 8px 18px; font-size: 13px; }
    .success-panel { background: #f1f8e9; border: 1px solid #c8e6c9; }
    .success-header { display: flex; align-items: center; gap: 12px; margin-bottom: 20px; }
    .success-icon { display: inline-flex; align-items: center; justify-content: center; width: 36px; height: 36px; border-radius: 50%; background: #c8e6c9; color: #2e7d32; font-size: 18px; font-weight: bold; flex-shrink: 0; }
    .success-header h2 { margin: 0; font-size: 22px; color: #2e7d32; }
    .video-section { margin-bottom: 20px; }
    .video-player { width: 100%; max-height: 480px; border-radius: 8px; background: #000; display: block; }
    .video-loading { display: flex; align-items: center; gap: 10px; padding: 20px; color: #555; font-size: 14px; }
    .video-placeholder { display: flex; flex-direction: column; align-items: center; gap: 10px; padding: 24px; background: white; border: 1px solid #c8e6c9; border-radius: 8px; }
    .video-hint { margin: 0; font-size: 12px; color: #999; text-align: center; }
    .job-meta { background: white; border: 1px solid #c8e6c9; border-radius: 8px; padding: 14px 16px; margin-bottom: 16px; }
    .meta-row { display: flex; gap: 12px; padding: 5px 0; border-bottom: 1px solid #f0f0f0; flex-wrap: wrap; }
    .meta-row:last-child { border-bottom: none; }
    .meta-label { font-size: 13px; color: #777; min-width: 100px; }
    .meta-value { font-size: 13px; color: #333; font-weight: 500; }
    /* Regen bar */
    .regen-bar { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; padding: 12px 16px; background: #e8f5e9; border: 1px solid #a5d6a7; border-radius: 8px; flex-wrap: wrap; }
    .regen-bar-hint { font-size: 12px; color: #555; flex: 1; min-width: 0; }
    .btn-regen { background: #1976d2; color: white; border: none; padding: 9px 20px; border-radius: 6px; cursor: pointer; font-size: 14px; white-space: nowrap; flex-shrink: 0; }
    .btn-regen:hover { background: #1565c0; }
    /* Regen panel */
    .regen-panel { background: white; border: 1px solid #90caf9; border-radius: 10px; padding: 20px; margin-bottom: 16px; }
    .regen-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
    .regen-title { font-size: 16px; font-weight: 600; color: #1a237e; }
    .regen-info-pill { display: flex; align-items: center; gap: 8px; background: #e3f2fd; border: 1px solid #90caf9; border-radius: 20px; padding: 7px 14px; font-size: 13px; color: #1565c0; margin-bottom: 16px; width: fit-content; }
    .regen-info-icon { font-size: 11px; }
    .regen-section { margin-bottom: 16px; }
    .regen-section-label { font-size: 13px; font-weight: 600; color: #444; margin: 0 0 8px; }
    .optional-label { font-size: 12px; font-weight: 400; color: #999; }
    .regen-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 16px; padding-top: 16px; border-top: 1px solid #e3f2fd; }
    /* Keyframe upload */
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
    .keyframe-actions { margin-top: 14px; display: flex; align-items: center; gap: 12px; flex-wrap: wrap; }
    .upload-done-banner { display: flex; align-items: center; gap: 8px; background: #e8f5e9; border: 1px solid #c8e6c9; border-radius: 8px; padding: 12px 16px; font-size: 14px; color: #2e7d32; font-weight: 500; }
    /* Post section */
    .post-section { background: white; border: 1px solid #c8e6c9; border-radius: 8px; padding: 20px; }
    .post-section-title { margin: 0 0 16px; font-size: 17px; color: #1a1a1a; }
    .form-group { margin-bottom: 14px; }
    .form-label { display: block; font-size: 13px; font-weight: 600; color: #444; margin-bottom: 5px; }
    .required { color: #d32f2f; }
    .form-hint { font-weight: normal; color: #999; font-size: 12px; }
    .form-control { width: 100%; box-sizing: border-box; border: 1px solid #ddd; border-radius: 6px; padding: 8px 12px; font-size: 14px; font-family: inherit; }
    .form-control:focus { outline: none; border-color: #1976d2; }
    .form-control:disabled { background: #f5f5f5; color: #999; cursor: not-allowed; }
    textarea.form-control { resize: vertical; }
    select.form-control { appearance: auto; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .accounts-loading { display: flex; align-items: center; gap: 10px; padding: 16px 0; color: #666; font-size: 14px; }
    .no-accounts-msg { display: flex; align-items: center; gap: 8px; padding: 14px; background: #fff8e1; border: 1px solid #ffe082; border-radius: 6px; font-size: 14px; color: #5d4037; margin-top: 8px; }
    .no-accounts-icon { font-size: 16px; flex-shrink: 0; }
    .accounts-section { margin-top: 16px; }
    .accounts-label { font-size: 13px; font-weight: 600; color: #555; margin: 0 0 8px; }
    .accounts-list { display: flex; flex-direction: column; gap: 8px; margin-bottom: 12px; }
    .account-row { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 10px 14px; border: 1px solid #e0e0e0; border-radius: 8px; background: #fafafa; flex-wrap: wrap; }
    .account-row--invalid { border-color: #e65100; background: #fff8f5; }
    .account-info { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; flex: 1; min-width: 0; }
    .platform-badge { padding: 3px 10px; border-radius: 4px; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.3px; }
    .platform-badge--youtube { background: #ff0000; color: white; }
    .platform-badge--tiktok { background: #010101; color: white; }
    .platform-badge--instagram { background: #e1306c; color: white; }
    .account-name { font-size: 14px; font-weight: 500; color: #222; }
    .token-warn { font-size: 12px; color: #e65100; font-style: italic; }
    .btn-post-single { background: white; color: #1976d2; border: 1px solid #1976d2; padding: 7px 16px; border-radius: 6px; cursor: pointer; font-size: 13px; white-space: nowrap; display: inline-flex; align-items: center; flex-shrink: 0; }
    .btn-post-single:hover:not(:disabled) { background: #e3f2fd; }
    .btn-post-single:disabled { opacity: 0.5; cursor: not-allowed; }
    .post-all-row { display: flex; justify-content: flex-end; padding-top: 4px; border-top: 1px solid #eee; margin-top: 4px; padding-top: 12px; }
    .btn-post-all { background: #1976d2; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; font-weight: 500; display: inline-flex; align-items: center; }
    .btn-post-all:hover:not(:disabled) { background: #1565c0; }
    .btn-post-all:disabled { opacity: 0.6; cursor: not-allowed; }
    .post-actions { display: flex; justify-content: flex-start; margin-top: 16px; padding-top: 16px; border-top: 1px solid #eee; }
  `]
})
export class LocalVideoStatusComponent implements OnInit {
  readonly store = inject(RenderJobStore);
  readonly channelStore = inject(ChannelStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly syncing = signal(false);
  readonly lastChecked = signal<Date | null>(null);
  readonly videoUrl = signal<string | null>(null);
  readonly videoUrlLoading = signal(false);
  readonly videoUrlError = signal<string | null>(null);
  /** 'all' = posting to all accounts; platform name = posting to that single account; null = idle */
  readonly postingTarget = signal<string | null>(null);

  // Regenerate panel signals
  readonly showRegenForm = signal(false);
  readonly regenError = signal<string | null>(null);
  readonly isRegenerating = signal(false);
  readonly isKeyframeDragOver = signal(false);
  readonly keyframeUploadStatus = signal<'idle' | 'uploading' | 'done' | 'error'>('idle');
  readonly keyframeValidationError = signal<string | null>(null);
  readonly stagedKeyframes = signal<StagedKeyframe[]>([]);
  readonly keyframeObjectKeys = signal<string[]>([]);

  postTitle = '';
  postDescription = '';
  postHashtags = '';

  // Regenerate form plain properties (ngModel)
  regenPrompt = '';
  regenModel = 'wan-i2v';
  regenDuration = 5;
  regenResolution = '1280x720';
  regenAspectRatio = '16:9';

  private jobId = '';
  private channelId = '';

  isPosting(): boolean {
    return this.postingTarget() !== null;
  }

  async ngOnInit(): Promise<void> {
    this.jobId = this.route.snapshot.paramMap.get('jobId')!;
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    await this.syncJob();
    this.channelStore.loadChannel(this.channelId);
  }

  async syncJob(): Promise<void> {
    if (this.syncing()) return;
    this.syncing.set(true);
    try {
      await this.store.fetchJob(this.jobId);
      this.lastChecked.set(new Date());
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

  /**
   * @param targetPlatform  When provided, posts to that platform only.
   *                        When omitted, posts to all linked accounts.
   */
  async submitPost(targetPlatform?: string): Promise<void> {
    if (!this.postTitle.trim() || this.isPosting()) return;

    const target = targetPlatform ?? 'all';
    this.postingTarget.set(target);

    const hashtags = this.postHashtags
      .split(',')
      .map(h => h.trim().replace(/^#/, ''))
      .filter(h => h.length > 0);

    try {
      const postCycleJobId = await this.store.startPostFromRenderJob(this.jobId, {
        channelId: this.channelId,
        title: this.postTitle.trim(),
        description: this.postDescription.trim(),
        hashtags,
        targetPlatform,
      });

      if (postCycleJobId) {
        this.router.navigate(['/channels', this.channelId, 'post-cycle', postCycleJobId]);
      }
    } finally {
      this.postingTarget.set(null);
    }
  }

  // --- Regenerate panel ---

  openRegenForm(): void {
    const job = this.store.currentJob();
    if (!job) return;
    this.regenPrompt = job.prompt;
    this.regenModel = job.model ?? 'ltx-2.3';
    this.regenDuration = job.durationSeconds ?? 5;
    this.regenResolution = job.resolution ?? '1280x720';
    this.regenAspectRatio = job.aspectRatio ?? '16:9';
    // Reset keyframe state for a fresh panel
    this.stagedKeyframes.set([]);
    this.keyframeObjectKeys.set([]);
    this.keyframeUploadStatus.set('idle');
    this.keyframeValidationError.set(null);
    this.regenError.set(null);
    this.showRegenForm.set(true);
  }

  // --- Keyframe upload ---

  onKeyframeDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isKeyframeDragOver.set(true);
  }

  onKeyframeDrop(event: DragEvent): void {
    event.preventDefault();
    this.isKeyframeDragOver.set(false);
    const files = event.dataTransfer?.files ? Array.from(event.dataTransfer.files) : [];
    if (files.length > 0) this.addKeyframeFiles(files);
  }

  onKeyframeFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
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

  async submitRegen(): Promise<void> {
    if (!this.regenPrompt.trim() || this.isRegenerating()) return;

    const job = this.store.currentJob();
    if (!job) return;

    this.regenError.set(null);
    this.isRegenerating.set(true);

    try {
      // Upload any staged keyframes that haven't been uploaded yet
      if (this.stagedKeyframes().length > 0 && this.keyframeUploadStatus() !== 'done') {
        await this.uploadKeyframes();
        if (this.keyframeUploadStatus() === 'error') {
          this.regenError.set('Failed to upload keyframes. Please try again.');
          return;
        }
      }

      const newJobId = await this.store.createJob({
        channelId: this.channelId,
        model: this.regenModel,
        durationSeconds: this.regenDuration,
        resolution: this.regenResolution,
        aspectRatio: this.regenAspectRatio,
        clips: [{
          prompt: this.regenPrompt.trim(),
          endImageKey: this.keyframeObjectKeys()[0] ?? null,
        }],
      });

      if (newJobId) {
        this.router.navigate(['/channels', this.channelId, 'local-video', newJobId]);
      } else {
        this.regenError.set('Failed to create regeneration job. Please try again.');
      }
    } finally {
      this.isRegenerating.set(false);
    }
  }

  backToChannel(): void {
    this.router.navigate(['/channels', this.channelId]);
  }

  tryAgain(): void {
    this.router.navigate(['/channels', this.channelId, 'local-video']);
  }
}
