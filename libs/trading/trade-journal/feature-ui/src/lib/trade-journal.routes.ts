import { Route } from '@angular/router';

export const tradeJournalRoutes: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./trade-list.component').then(m => m.TradeListComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./trade-detail.component').then(m => m.TradeDetailComponent),
  },
];
