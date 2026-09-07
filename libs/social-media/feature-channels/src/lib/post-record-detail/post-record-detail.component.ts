import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PostCycleStore } from '@sports-ui/social-media-data-access';

@Component({
  selector: 'lib-post-record-detail',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="detail-container">
      <div class="detail-header">
        <button class="btn-back" (click)="backToHistory()">← Post History</button>
      </div>

      @if (store.isLoadingRecord()) {
        <div class="loading-state">Loading post record...</div>
      } @else if (store.recordStatus() === 'error') {
        <div class="error-state">{{ store.error() }}</div>
      } @else if (store.selectedRecord()) {
        @let record = store.selectedRecord()!;
        <div class="record-meta">
          <h1>{{ record.title }}</h1>
          <span class="posted-date">{{ record.postedAt | date:'longDate' }}</span>
        </div>

        <div class="section">
          <h2>Description</h2>
          <p class="description">{{ record.description }}</p>
        </div>

        @if (record.hashtags.length > 0) {
          <div class="section">
            <h2>Hashtags</h2>
            <div class="hashtag-chips">
              @for (tag of record.hashtags; track tag) {
                <span class="hashtag-chip">#{{ tag }}</span>
              }
            </div>
          </div>
        }

        @if (record.videoReference) {
          <div class="section">
            <h2>Video</h2>
            <span class="video-ref">{{ record.videoReference }}</span>
          </div>
        }

        <div class="section">
          <h2>Platform Results</h2>
          @if (record.platformResults.length === 0) {
            <p class="empty-msg">No platform results recorded.</p>
          } @else {
            <div class="platforms-list">
              @for (pr of record.platformResults; track pr.platform) {
                <div class="platform-row" [class.platform-row--success]="pr.status === 'success'"
                     [class.platform-row--failed]="pr.status === 'failed'">
                  <div class="platform-left">
                    <span class="platform-badge platform-badge--{{ pr.platform.toLowerCase() }}">
                      {{ pr.platform }}
                    </span>
                    <div class="platform-info">
                      <span class="status-label" [class.status-label--success]="pr.status === 'success'"
                            [class.status-label--failed]="pr.status === 'failed'">
                        {{ pr.status === 'success' ? 'Published' : 'Failed' }}
                      </span>
                      @if (pr.publishedUrl) {
                        <a [href]="pr.publishedUrl" target="_blank" rel="noopener" class="video-link">
                          Watch on {{ pr.platform }} ↗
                        </a>
                      }
                      @if (pr.publishedAt) {
                        <span class="published-at">{{ pr.publishedAt | date:'medium' }}</span>
                      }
                      @if (pr.errorMessage) {
                        <span class="error-msg">{{ pr.errorMessage }}</span>
                      }
                    </div>
                  </div>
                </div>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .detail-container { padding: 24px; max-width: 760px; margin: 0 auto; }
    .detail-header { margin-bottom: 24px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; }
    .loading-state, .error-state { text-align: center; padding: 48px; color: #666; }
    .error-state { color: #d32f2f; }
    .record-meta { margin-bottom: 28px; }
    .record-meta h1 { margin: 0 0 6px; font-size: 26px; }
    .posted-date { font-size: 14px; color: #888; }
    .section { margin-bottom: 28px; }
    .section h2 { font-size: 16px; font-weight: 600; border-bottom: 1px solid #eee; padding-bottom: 6px; margin: 0 0 12px; }
    .description { color: #444; font-size: 15px; line-height: 1.6; margin: 0; white-space: pre-wrap; }
    .hashtag-chips { display: flex; flex-wrap: wrap; gap: 8px; }
    .hashtag-chip { background: #e3f2fd; color: #1565c0; padding: 4px 12px; border-radius: 16px; font-size: 13px; }
    .video-ref { background: #f5f5f5; padding: 8px 12px; border-radius: 6px; font-size: 13px; font-family: monospace; color: #555; display: inline-block; }
    .empty-msg { color: #999; font-size: 14px; }
    .platforms-list { display: flex; flex-direction: column; gap: 10px; }
    .platform-row { display: flex; align-items: flex-start; padding: 14px 16px; border: 1px solid #e0e0e0; border-radius: 8px; }
    .platform-row--success { border-color: #a5d6a7; background: #f9fff9; }
    .platform-row--failed { border-color: #ef9a9a; background: #fff9f9; }
    .platform-left { display: flex; align-items: flex-start; gap: 12px; }
    .platform-badge { padding: 3px 10px; border-radius: 4px; font-size: 12px; font-weight: 600; flex-shrink: 0; margin-top: 2px; }
    .platform-badge--youtube { background: #ff0000; color: white; }
    .platform-info { display: flex; flex-direction: column; gap: 4px; }
    .status-label { font-weight: 600; font-size: 14px; }
    .status-label--success { color: #2e7d32; }
    .status-label--failed { color: #c62828; }
    .video-link { color: #1976d2; font-size: 13px; text-decoration: none; }
    .video-link:hover { text-decoration: underline; }
    .published-at { font-size: 12px; color: #999; }
    .error-msg { font-size: 13px; color: #c62828; }
  `]
})
export class PostRecordDetailComponent implements OnInit {
  readonly store = inject(PostCycleStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private channelId = '';

  ngOnInit(): void {
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    const postId = this.route.snapshot.paramMap.get('postId')!;
    this.store.loadPostRecord(postId);
  }

  backToHistory(): void {
    // channels/:id/history/:postId → channels/:id/history
    this.router.navigate(['..', this.channelId, 'history'], { relativeTo: this.route });
  }
}
