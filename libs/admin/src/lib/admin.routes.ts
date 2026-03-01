import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  {
    path: 'economy',
    loadComponent: () =>
      import('./economy/economy-admin.component').then((m) => m.EconomyAdminComponent),
  },
  {
    path: 'audit-log',
    loadComponent: () =>
      import('./audit/audit-log.component').then((m) => m.AuditLogComponent),
  },
  { path: '', redirectTo: 'economy', pathMatch: 'full' },
];
