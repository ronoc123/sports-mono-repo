import { Routes } from '@angular/router';

export const h2hRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./h2h/h2h.component').then((m) => m.H2HComponent),
  },
];
