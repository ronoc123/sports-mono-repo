import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PostCycleStore } from '@sports-ui/social-media-data-access';

@Component({
  selector: 'lib-post-history',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="history-container">
      <div class="history-header">
        <button class="btn-back" (click)="backToChannel()">← Back to Channel</button>
        <h1>Post History</h1>
      </div>

      @if (store.isLoadingHistory()) {
        <div class="loading-state">Loading post history...</div>
      } @else if (store.historyStatus() === 'error') {
        <div class="error-state">{{ store.error() }}</div>
      } @else if (!store.historyPage() || store.historyPage()!.records.length === 0) {
        <div class="empty-state">
          <p>No posts have been published yet.</p>
          <button class="btn-primary" (click)="startPost()">Start a Post</button>
        </div>
      } @else {
        @let page = store.historyPage()!;

        <p class="record-count">{{ page.totalCount }} post{{ page.totalCount !== 1 ? 's' : '' }} total</p>

        <div class="records-list">
          @for (record of page.records; track record.id) {
            <div class="record-card" (click)="viewRecord(record.id)" role="button" tabindex="0"
                 (keydown.enter)="viewRecord(record.id)">
              <div class="record-main">
                <h3 class="record-title">{{ record.title }}</h3>
                @if (record.descriptionSnippet) {
                  <p class="record-snippet">{{ record.descriptionSnippet }}</p>
                }
                <span class="record-date">{{ record.postedAt | date:'mediumDate' }}</span>
              </div>

              <div class="record-platforms">
                @for (result of record.platformResults; track result.platform) {
                  <span class="platform-tag"
                        [class.platform-tag--success]="result.status === 'success'"
                        [class.platform-tag--failed]="result.status === 'failed'">
                    {{ result.platform }}
                    @if (result.status === 'success') { ✓ }
                    @if (result.status === 'failed') { ✗ }
                  </span>
                }
              </div>

              <span class="record-arrow">›</span>
            </div>
          }
        </div>

        @if (page.totalPages > 1) {
          <div class="pagination">
            <button class="btn-page" [disabled]="page.page === 1" (click)="loadPage(page.page - 1)">
              ‹ Previous
            </button>
            <span class="page-info">Page {{ page.page }} of {{ page.totalPages }}</span>
            <button class="btn-page" [disabled]="page.page >= page.totalPages" (click)="loadPage(page.page + 1)">
              Next ›
            </button>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .history-container { padding: 24px; max-width: 820px; margin: 0 auto; }
    .history-header { display: flex; align-items: center; gap: 16px; margin-bottom: 28px; }
    .history-header h1 { flex: 1; margin: 0; font-size: 24px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; white-space: nowrap; }
    .loading-state, .error-state { text-align: center; padding: 48px; color: #666; }
    .error-state { color: #d32f2f; }
    .empty-state { text-align: center; padding: 48px; }
    .empty-state p { color: #777; margin: 0 0 16px; }
    .record-count { color: #888; font-size: 14px; margin: 0 0 16px; }
    .records-list { display: flex; flex-direction: column; gap: 10px; }
    .record-card { display: flex; align-items: center; gap: 12px; padding: 16px 20px; border: 1px solid #e0e0e0; border-radius: 10px; cursor: pointer; transition: border-color 0.15s, background 0.15s; background: white; }
    .record-card:hover { border-color: #1976d2; background: #fafcff; }
    .record-main { flex: 1; min-width: 0; }
    .record-title { margin: 0 0 4px; font-size: 15px; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .record-snippet { margin: 0 0 6px; font-size: 13px; color: #666; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .record-date { font-size: 12px; color: #999; }
    .record-platforms { display: flex; gap: 6px; flex-shrink: 0; }
    .platform-tag { padding: 3px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; background: #f5f5f5; color: #555; }
    .platform-tag--success { background: #e8f5e9; color: #2e7d32; }
    .platform-tag--failed { background: #ffebee; color: #c62828; }
    .record-arrow { color: #bbb; font-size: 20px; flex-shrink: 0; }
    .pagination { display: flex; align-items: center; justify-content: center; gap: 16px; margin-top: 28px; }
    .btn-page { background: white; border: 1px solid #ddd; padding: 8px 18px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-page:hover:not(:disabled) { border-color: #1976d2; color: #1976d2; }
    .btn-page:disabled { opacity: 0.4; cursor: not-allowed; }
    .page-info { font-size: 14px; color: #666; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-primary:hover { background: #1565c0; }
  `]
})
export class PostHistoryComponent implements OnInit {
  readonly store = inject(PostCycleStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private channelId = '';

  ngOnInit(): void {
    this.channelId = this.route.snapshot.paramMap.get('id')!;
    this.store.loadPostHistory(this.channelId, 1);
  }

  backToChannel(): void {
    // channels/:id/history → channels/:id
    this.router.navigate(['..', this.channelId], { relativeTo: this.route });
  }

  startPost(): void {
    this.router.navigate(['..', this.channelId, 'post-cycle'], { relativeTo: this.route });
  }

  viewRecord(postId: string): void {
    // channels/:id/history → channels/:id/history/:postId
    this.router.navigate([postId], { relativeTo: this.route });
  }

  loadPage(page: number): void {
    this.store.loadPostHistory(this.channelId, page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
