import { Route } from '@angular/router';

export const backtestRoutes: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./backtest-gate.component').then(m => m.BacktestGateComponent),
  },
];
