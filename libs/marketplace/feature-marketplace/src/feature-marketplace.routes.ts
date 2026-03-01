import { Routes } from '@angular/router';

export const marketplaceRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./marketplace/marketplace.component').then(
        (m) => m.MarketplaceComponent
      ),
  },
];
