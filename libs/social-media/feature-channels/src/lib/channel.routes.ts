import { Route } from '@angular/router';
import { ChannelListComponent } from './channel-list/channel-list.component';
import { ChannelFormComponent } from './channel-form/channel-form.component';
import { ChannelDetailComponent } from './channel-detail/channel-detail.component';
import { PostCycleComponent } from './post-cycle/post-cycle.component';
import { PostCycleStatusComponent } from './post-cycle-status/post-cycle-status.component';
import { PostHistoryComponent } from './post-history/post-history.component';
import { PostRecordDetailComponent } from './post-record-detail/post-record-detail.component';
import { AiGenerateComponent } from './ai-generate/ai-generate.component';
import { AiGenerateStatusComponent } from './ai-generate-status/ai-generate-status.component';

export const channelRoutes: Route[] = [
  { path: '', redirectTo: 'channels', pathMatch: 'full' },
  {
    path: 'channels',
    children: [
      { path: '', component: ChannelListComponent },
      { path: 'new', component: ChannelFormComponent },
      { path: ':id', component: ChannelDetailComponent },
      { path: ':id/edit', component: ChannelFormComponent },
      { path: ':id/post-cycle', component: PostCycleComponent },
      { path: ':id/post-cycle/:jobId', component: PostCycleStatusComponent },
      { path: ':id/history', component: PostHistoryComponent },
      { path: ':id/history/:postId', component: PostRecordDetailComponent },
      { path: ':id/ai-generate', component: AiGenerateComponent },
      { path: ':id/ai-generate/:jobId', component: AiGenerateStatusComponent },
    ],
  },
];
