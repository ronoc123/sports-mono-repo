import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ChannelStore } from '@sports-ui/social-media-data-access';

@Component({
  selector: 'lib-channel-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="channel-list-container">
      <div class="channel-list-header">
        <h1>Channels</h1>
        <button class="btn-primary" (click)="createChannel()">New Channel</button>
      </div>

      @if (store.isLoading()) {
        <div class="loading-state">Loading channels...</div>
      }

      @if (store.status() === 'error') {
        <div class="error-state">{{ store.error() }}</div>
      }

      @if (store.status() === 'success' && !store.hasChannels()) {
        <div class="empty-state">
          <p>No channels yet. Create your first channel to get started.</p>
          <button class="btn-primary" (click)="createChannel()">Create Channel</button>
        </div>
      }

      @if (store.hasChannels()) {
        <div class="channel-grid">
          @for (channel of store.channels(); track channel.id) {
            <div class="channel-card" (click)="viewChannel(channel.id)">
              <div class="channel-card-header">
                <h3>{{ channel.name }}</h3>
                <span class="linked-badge">{{ channel.linkedAccountCount }} linked</span>
              </div>
              <p class="channel-description">{{ channel.description }}</p>
              @if (channel.lastPostTitle) {
                <div class="last-post">
                  <span class="last-post-label">Last post:</span>
                  <span class="last-post-title">{{ channel.lastPostTitle }}</span>
                  <span class="last-post-date">{{ channel.lastPostDate | date:'mediumDate' }}</span>
                </div>
              } @else {
                <div class="no-posts">No posts yet</div>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .channel-list-container { padding: 24px; max-width: 1200px; margin: 0 auto; }
    .channel-list-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .channel-list-header h1 { margin: 0; font-size: 28px; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-primary:hover { background: #1565c0; }
    .loading-state, .error-state, .empty-state { text-align: center; padding: 48px; color: #666; }
    .error-state { color: #d32f2f; }
    .empty-state p { margin-bottom: 16px; }
    .channel-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 16px; }
    .channel-card { background: white; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; cursor: pointer; transition: box-shadow 0.2s; }
    .channel-card:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.1); }
    .channel-card-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 8px; }
    .channel-card-header h3 { margin: 0; font-size: 18px; }
    .linked-badge { background: #e3f2fd; color: #1565c0; padding: 2px 8px; border-radius: 12px; font-size: 12px; white-space: nowrap; }
    .channel-description { color: #555; font-size: 14px; margin: 0 0 12px; line-height: 1.5; }
    .last-post { font-size: 13px; color: #777; }
    .last-post-label { font-weight: 500; margin-right: 4px; }
    .last-post-title { margin-right: 8px; }
    .no-posts { font-size: 13px; color: #aaa; font-style: italic; }
  `]
})
export class ChannelListComponent implements OnInit {
  readonly store = inject(ChannelStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    this.store.loadChannels();
  }

  viewChannel(id: string): void {
    this.router.navigate([id], { relativeTo: this.route });
  }

  createChannel(): void {
    this.router.navigate(['new'], { relativeTo: this.route });
  }
}
