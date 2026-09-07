import { Route } from '@angular/router';
import { channelRoutes } from '@sports-ui/feature-channels';
import { ShellComponent } from './shell/shell.component';

export const appRoutes: Route[] = [
  {
    path: '',
    component: ShellComponent,
    children: channelRoutes,
  },
  { path: '**', redirectTo: '' },
];
