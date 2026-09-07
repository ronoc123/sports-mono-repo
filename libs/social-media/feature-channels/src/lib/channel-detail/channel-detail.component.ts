import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ChannelStore } from '@sports-ui/social-media-data-access';
import { FormsModule } from '@angular/forms';
import { environment } from '@sports-ui/api-types';

const MAX_IMAGE_BYTES = 20 * 1024 * 1024;
const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

@Component({
  selector: 'lib-channel-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="detail-container">
      @if (store.isLoading()) {
        <div class="loading-state">Loading channel...</div>
      }

      @if (store.status() === 'error') {
        <div class="error-state">
          <p>{{ store.error() }}</p>
          <button class="btn-secondary" (click)="backToList()">Back to Channels</button>
        </div>
      }

      @if (store.selectedChannel(); as channel) {
        <div class="detail-header">
          <button class="btn-back" (click)="backToList()">← Channels</button>
          <div class="header-actions">
            <button class="btn-secondary" (click)="editChannel()">Edit Channel</button>
            <button class="btn-danger" (click)="confirmDelete()">Delete Channel</button>
            <button class="btn-ai" (click)="generateAiVideo()">Generate AI Video</button>
            <button class="btn-primary" (click)="newPost()">New Post</button>
          </div>
        </div>

        <div class="channel-info">
          <h1>{{ channel.name }}</h1>
          <p class="description">{{ channel.description }}</p>
          @if (channel.styleToneContext) {
            <div class="tone-context">
              <span class="tone-label">Style / Tone Context:</span>
              <p>{{ channel.styleToneContext }}</p>
            </div>
          }

          <!-- Prompt Template Section -->
          <div class="prompt-template-section">
            <div class="prompt-template-header">
              <span class="tone-label">AI Prompt Template:</span>
              @if (!editingTemplate()) {
                <button class="btn-edit-inline" (click)="startEditTemplate(channel.promptTemplate)">Edit</button>
              }
            </div>

            @if (!editingTemplate()) {
              @if (channel.promptTemplate) {
                <p class="prompt-template-text">{{ channel.promptTemplate }}</p>
              } @else {
                <p class="prompt-template-empty">No template set — channel uses the default prompt.</p>
              }
            } @else {
              <textarea class="form-control template-textarea"
                        rows="5"
                        maxlength="2000"
                        [(ngModel)]="templateDraft"
                        placeholder="Enter a custom prompt template for AI video generation"></textarea>
              <div class="template-char-count">{{ templateDraft.length }}/2000</div>
              @if (store.promptTemplateSaveStatus() === 'error') {
                <p class="error-msg">{{ store.promptTemplateSaveError() }}</p>
              }
              <div class="template-actions">
                <button class="btn-secondary btn-sm" (click)="cancelEditTemplate()"
                        [disabled]="store.promptTemplateSaveStatus() === 'loading'">
                  Cancel
                </button>
                <button class="btn-primary btn-sm" (click)="saveTemplate()"
                        [disabled]="store.promptTemplateSaveStatus() === 'loading'">
                  {{ store.promptTemplateSaveStatus() === 'loading' ? 'Saving…' : 'Save' }}
                </button>
              </div>
            }
          </div>
        </div>

        <!-- Character Reference Image Section -->
        <div class="section">
          <div class="section-header">
            <h2>Character Reference Image</h2>
          </div>
          <p class="section-desc">
            This image is automatically sent to Higgsfield every time you generate an AI video, giving it the character's visual context from all angles.
          </p>

          @if (channel.characterImageUrl) {
            <div class="char-image-container">
              <img [src]="apiOrigin + channel.characterImageUrl" alt="Character reference" class="char-image">
              <div class="char-image-footer">
                <span class="char-image-label">Current character reference</span>
                <button class="btn-secondary btn-sm"
                        (click)="charImageInput.click()"
                        [disabled]="store.isUploadingImage()">
                  {{ store.isUploadingImage() ? 'Uploading…' : 'Replace Image' }}
                </button>
              </div>
            </div>
          } @else {
            <div class="upload-area"
                 (click)="charImageInput.click()"
                 (dragover)="$event.preventDefault()"
                 (drop)="onCharImageDrop($event)">
              <div class="upload-placeholder">
                @if (store.isUploadingImage()) {
                  <span class="uploading-indicator"></span>
                  <p>Uploading...</p>
                } @else {
                  <span class="upload-icon">&#128444;</span>
                  <p>Click or drag an image here</p>
                  <span class="upload-hint">JPG, PNG, WEBP — up to 20 MB</span>
                }
              </div>
            </div>
          }

          <input #charImageInput type="file" accept="image/jpeg,image/png,image/webp" style="display:none"
                 (change)="onCharImageFileSelected($event)">

          @if (charImageValidationError()) {
            <p class="error-msg">{{ charImageValidationError() }}</p>
          }
          @if (store.imageUploadStatus() === 'error') {
            <p class="error-msg">{{ store.imageUploadError() }}</p>
          }
          @if (store.imageUploadStatus() === 'success' && channel.characterImageUrl) {
            <p class="success-msg">Character image updated.</p>
          }
        </div>

        <div class="section">
          <div class="section-header">
            <h2>Linked Accounts</h2>
            @if (oauthLinking()) {
              <span class="linking-indicator">Waiting for authorization...</span>
            }
          </div>

          @if (channel.linkedAccounts.length === 0) {
            <div class="empty-section">
              <p>No platform accounts linked yet.</p>
              <button class="btn-secondary" (click)="startOAuthLink()" [disabled]="oauthLinking()">
                Link YouTube Account
              </button>
            </div>
          } @else {
            <div class="accounts-list">
              @for (account of channel.linkedAccounts; track account.platform) {
                <div class="account-item" [class.account-item--invalid]="account.tokenStatus === 'invalid'">
                  <div class="account-info">
                    <span class="platform-badge platform-badge--youtube">{{ account.platform }}</span>
                    <span class="account-name">{{ account.accountDisplayName }}</span>
                    <span class="linked-date">Linked {{ account.linkedAt | date:'mediumDate' }}</span>
                    @if (account.tokenStatus === 'invalid') {
                      <span class="token-invalid-badge">Re-authentication required</span>
                    }
                  </div>
                  <div class="account-actions">
                    @if (account.tokenStatus === 'invalid') {
                      <button class="btn-warning" (click)="startOAuthLink(account.platform)" [disabled]="oauthLinking()">
                        Re-authenticate
                      </button>
                    }
                    <button class="btn-danger-sm" (click)="confirmUnlink(account.platform)">
                      Unlink
                    </button>
                  </div>
                </div>
              }
              <button class="btn-secondary btn-add-account" (click)="startOAuthLink()" [disabled]="oauthLinking()">
                + Link Another Account
              </button>
            </div>
          }
        </div>

        <div class="section">
          <div class="section-header">
            <h2>Post History</h2>
            <button class="btn-secondary btn-history" (click)="viewHistory()">View All</button>
          </div>
          <div class="empty-section">
            <p>No posts yet. Start a new post cycle to publish content.</p>
          </div>
        </div>
      }

      @if (showDeleteConfirm()) {
        <div class="modal-overlay" (click)="cancelDelete()">
          <div class="modal" (click)="$event.stopPropagation()">
            <h3>Delete Channel</h3>
            <p>Are you sure you want to delete <strong>{{ store.selectedChannel()?.name }}</strong>? This action cannot be undone.</p>
            <div class="modal-actions">
              <button class="btn-secondary" (click)="cancelDelete()">Cancel</button>
              <button class="btn-danger" (click)="executeDelete()" [disabled]="store.isSaving()">
                {{ store.isSaving() ? 'Deleting...' : 'Delete' }}
              </button>
            </div>
          </div>
        </div>
      }

      @if (unlinkTarget()) {
        <div class="modal-overlay" (click)="cancelUnlink()">
          <div class="modal" (click)="$event.stopPropagation()">
            <h3>Unlink Account</h3>
            <p>Remove <strong>{{ unlinkTarget() }}</strong> from this channel? The channel will no longer be able to post to this platform.</p>
            <div class="modal-actions">
              <button class="btn-secondary" (click)="cancelUnlink()">Cancel</button>
              <button class="btn-danger" (click)="executeUnlink()" [disabled]="store.isSaving()">
                {{ store.isSaving() ? 'Unlinking...' : 'Unlink' }}
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .detail-container { padding: 24px; max-width: 900px; margin: 0 auto; }
    .loading-state, .error-state { text-align: center; padding: 48px; color: #666; }
    .error-state { color: #d32f2f; }
    .detail-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; flex-wrap: wrap; gap: 10px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; }
    .header-actions { display: flex; gap: 10px; flex-wrap: wrap; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 8px 18px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-primary:hover { background: #1565c0; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 8px 18px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
    .btn-secondary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-ai { background: #7c4dff; color: white; border: none; padding: 8px 18px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-ai:hover { background: #6200ea; }
    .btn-danger { background: white; color: #d32f2f; border: 1px solid #d32f2f; padding: 8px 18px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-danger:hover { background: #ffebee; }
    .btn-danger:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-danger-sm { background: transparent; color: #d32f2f; border: 1px solid #d32f2f; padding: 4px 12px; border-radius: 4px; cursor: pointer; font-size: 13px; }
    .btn-danger-sm:hover { background: #ffebee; }
    .btn-warning { background: white; color: #e65100; border: 1px solid #e65100; padding: 4px 12px; border-radius: 4px; cursor: pointer; font-size: 13px; }
    .btn-warning:hover { background: #fff3e0; }
    .btn-warning:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-sm { padding: 6px 14px; font-size: 13px; }
    .channel-info { margin-bottom: 32px; }
    .channel-info h1 { margin: 0 0 8px; font-size: 28px; }
    .description { color: #555; font-size: 15px; margin-bottom: 16px; }
    .tone-context { background: #f8f9fa; border-left: 3px solid #1976d2; padding: 12px 16px; border-radius: 0 6px 6px 0; }
    .tone-label { font-weight: 600; font-size: 13px; color: #555; display: block; margin-bottom: 4px; }
    .tone-context p { margin: 0; font-size: 14px; color: #333; line-height: 1.5; }
    .prompt-template-section { margin-top: 16px; background: #f8f9fa; border-left: 3px solid #7c4dff; padding: 12px 16px; border-radius: 0 6px 6px 0; }
    .prompt-template-header { display: flex; align-items: center; gap: 12px; margin-bottom: 6px; }
    .prompt-template-text { margin: 0; font-size: 14px; color: #333; line-height: 1.5; white-space: pre-wrap; }
    .prompt-template-empty { margin: 0; font-size: 14px; color: #999; font-style: italic; }
    .btn-edit-inline { background: none; border: none; color: #7c4dff; font-size: 13px; cursor: pointer; padding: 0; text-decoration: underline; }
    .btn-edit-inline:hover { color: #6200ea; }
    .form-control { width: 100%; box-sizing: border-box; border: 1px solid #ddd; border-radius: 6px; padding: 8px 12px; font-size: 14px; font-family: inherit; }
    .form-control:focus { outline: none; border-color: #7c4dff; }
    textarea.form-control { resize: vertical; }
    .template-textarea { margin-top: 8px; }
    .template-char-count { font-size: 12px; color: #999; text-align: right; margin-top: 4px; }
    .template-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 10px; }
    .section { margin-bottom: 32px; }
    .section-header { display: flex; align-items: center; gap: 16px; margin-bottom: 8px; }
    .section-header h2 { font-size: 18px; border-bottom: 1px solid #eee; padding-bottom: 8px; margin: 0; flex: 1; }
    .section-desc { color: #777; font-size: 13px; margin: 0 0 14px; line-height: 1.5; }
    .upload-area { border: 2px dashed #c5b8f8; border-radius: 12px; padding: 36px 24px; text-align: center; cursor: pointer; transition: border-color 0.2s; background: #faf8ff; }
    .upload-area:hover { border-color: #7c4dff; background: #f3f0ff; }
    .upload-placeholder { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .upload-icon { font-size: 36px; }
    .upload-hint { color: #999; font-size: 12px; }
    .uploading-indicator { display: inline-block; width: 28px; height: 28px; border: 3px solid #c5b8f8; border-top-color: #7c4dff; border-radius: 50%; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .char-image-container { display: flex; flex-direction: column; gap: 0; border: 1px solid #e0e0e0; border-radius: 10px; overflow: hidden; max-width: 480px; }
    .char-image { width: 100%; max-height: 360px; object-fit: contain; background: #f5f5f5; display: block; }
    .char-image-footer { display: flex; align-items: center; justify-content: space-between; padding: 10px 14px; background: #fafafa; border-top: 1px solid #e0e0e0; }
    .char-image-label { font-size: 13px; color: #777; }
    .error-msg { color: #d32f2f; font-size: 13px; margin: 8px 0 0; }
    .success-msg { color: #2e7d32; font-size: 13px; margin: 8px 0 0; }
    .linking-indicator { font-size: 13px; color: #1976d2; animation: pulse 1.5s infinite; }
    @keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }
    .empty-section { color: #777; font-size: 14px; }
    .empty-section p { margin: 0 0 12px; }
    .accounts-list { display: flex; flex-direction: column; gap: 10px; }
    .account-item { display: flex; align-items: center; justify-content: space-between; padding: 12px 16px; border: 1px solid #e0e0e0; border-radius: 8px; }
    .account-item--invalid { border-color: #e65100; background: #fff8f5; }
    .account-info { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; }
    .account-actions { display: flex; gap: 8px; flex-shrink: 0; }
    .platform-badge { padding: 3px 10px; border-radius: 4px; font-size: 12px; font-weight: 600; }
    .platform-badge--youtube { background: #ff0000; color: white; }
    .account-name { font-weight: 500; }
    .linked-date { color: #999; font-size: 13px; }
    .token-invalid-badge { background: #fff3e0; color: #e65100; padding: 2px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .btn-add-account { margin-top: 4px; width: fit-content; font-size: 13px; padding: 6px 14px; }
    .btn-history { font-size: 13px; padding: 6px 14px; }
    .modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .modal { background: white; border-radius: 8px; padding: 28px; max-width: 440px; width: 90%; }
    .modal h3 { margin: 0 0 12px; font-size: 20px; }
    .modal p { margin: 0 0 24px; color: #444; line-height: 1.5; }
    .modal-actions { display: flex; justify-content: flex-end; gap: 12px; }
  `]
})
export class ChannelDetailComponent implements OnInit, OnDestroy {
  readonly store = inject(ChannelStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  // "http://localhost:53016" — the API origin without the /api/ path
  readonly apiOrigin = environment.apiUrl + environment.socialMediaApi.split('/api')[0];

  readonly showDeleteConfirm = signal(false);
  readonly unlinkTarget = signal<string | null>(null);
  readonly oauthLinking = signal(false);
  readonly editingTemplate = signal(false);
  readonly charImageValidationError = signal<string | null>(null);
  templateDraft = '';

  private channelId = '';
  private oauthPollTimer: ReturnType<typeof setInterval> | null = null;
  private oauthPollCount = 0;
  private readonly maxPollCount = 60;

  ngOnInit(): void {
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    this.store.loadChannel(this.channelId);
  }

  ngOnDestroy(): void {
    this.stopOAuthPoll();
  }

  backToList(): void {
    this.router.navigate(['..'], { relativeTo: this.route });
  }

  editChannel(): void {
    this.router.navigate(['edit'], { relativeTo: this.route });
  }

  newPost(): void {
    this.router.navigate(['post-cycle'], { relativeTo: this.route });
  }

  generateAiVideo(): void {
    this.router.navigate(['ai-generate'], { relativeTo: this.route });
  }

  viewHistory(): void {
    this.router.navigate(['history'], { relativeTo: this.route });
  }

  // --- Character image ---

  onCharImageFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = ''; // reset so same file can be re-selected
    if (file) this.uploadCharImage(file);
  }

  onCharImageDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files[0] ?? null;
    if (file) this.uploadCharImage(file);
  }

  private uploadCharImage(file: File): void {
    this.charImageValidationError.set(null);
    if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) {
      this.charImageValidationError.set('Only JPG, PNG, and WEBP images are accepted.');
      return;
    }
    if (file.size > MAX_IMAGE_BYTES) {
      this.charImageValidationError.set('Image must be 20 MB or smaller.');
      return;
    }
    const formData = new FormData();
    formData.append('image', file, file.name);
    this.store.uploadCharacterImage(this.channelId, formData);
  }

  // --- Prompt template ---

  startEditTemplate(current?: string): void {
    this.templateDraft = current ?? '';
    this.editingTemplate.set(true);
  }

  cancelEditTemplate(): void {
    this.editingTemplate.set(false);
  }

  async saveTemplate(): Promise<void> {
    const success = await this.store.updatePromptTemplate(this.channelId, this.templateDraft);
    if (success) {
      this.editingTemplate.set(false);
    }
  }

  // --- OAuth ---

  async startOAuthLink(reAuthPlatform?: string): Promise<void> {
    const url = await this.store.getOAuthUrl(this.channelId);
    if (!url) return;

    this.oauthLinking.set(true);
    const popup = window.open(url, 'youtube-oauth', 'width=520,height=640,left=200,top=100');

    const messageHandler = (event: MessageEvent) => {
      if (event.data === 'oauth-success') {
        window.removeEventListener('message', messageHandler);
        this.stopOAuthPoll();
        this.store.loadChannel(this.channelId).then(() => this.oauthLinking.set(false));
      }
    };
    window.addEventListener('message', messageHandler);

    this.oauthPollCount = 0;
    this.oauthPollTimer = setInterval(() => {
      this.oauthPollCount++;
      if (popup?.closed || this.oauthPollCount >= this.maxPollCount) {
        window.removeEventListener('message', messageHandler);
        this.stopOAuthPoll();
        this.store.loadChannel(this.channelId).then(() => this.oauthLinking.set(false));
      }
    }, 2000);
  }

  private stopOAuthPoll(): void {
    if (this.oauthPollTimer !== null) {
      clearInterval(this.oauthPollTimer);
      this.oauthPollTimer = null;
    }
  }

  // --- Delete ---

  confirmDelete(): void {
    this.showDeleteConfirm.set(true);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
  }

  async executeDelete(): Promise<void> {
    const id = this.store.selectedChannel()?.id;
    if (!id) return;
    const success = await this.store.deleteChannel(id);
    if (success) this.router.navigate(['..'], { relativeTo: this.route });
  }

  // --- Unlink ---

  confirmUnlink(platform: string): void {
    this.unlinkTarget.set(platform);
  }

  cancelUnlink(): void {
    this.unlinkTarget.set(null);
  }

  async executeUnlink(): Promise<void> {
    const platform = this.unlinkTarget();
    if (!platform) return;
    const success = await this.store.unlinkAccount(this.channelId, platform);
    if (success) this.unlinkTarget.set(null);
  }
}
