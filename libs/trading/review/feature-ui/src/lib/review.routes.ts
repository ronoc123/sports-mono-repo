import { Route } from '@angular/router';

export const reviewRoutes: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./review-session.component').then(m => m.ReviewSessionComponent),
  },
];
