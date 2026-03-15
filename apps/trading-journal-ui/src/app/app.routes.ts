import { Route } from '@angular/router';
import { authGuard } from '@sports-ui/auth-data-access';

export const appRoutes: Route[] = [
  {
    path: 'login',
    loadComponent: () =>
      import('./login/login.component').then((m) => m.TradingLoginComponent),
  },

  {
    path: 'auth/callback',
    loadComponent: () =>
      import('./auth-callback/auth-callback.component').then(
        (m) => m.TradingAuthCallbackComponent
      ),
  },

  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./shell/shell.component').then((m) => m.ShellComponent),
    children: [
      {
        path: 'backtest',
        loadChildren: () =>
          import('@trading/backtest-feature').then((m) => m.backtestRoutes),
      },
      {
        path: 'journal',
        loadChildren: () =>
          import('@trading/trade-journal-feature').then(
            (m) => m.tradeJournalRoutes
          ),
      },
      {
        path: 'review',
        loadChildren: () =>
          import('@trading/review-feature').then((m) => m.reviewRoutes),
      },
      {
        path: '',
        redirectTo: 'backtest',
        pathMatch: 'full',
      },
    ],
  },

  {
    path: '**',
    redirectTo: 'login',
  },
];
