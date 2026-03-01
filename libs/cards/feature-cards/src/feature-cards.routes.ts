import { Routes } from '@angular/router';
import { CardCatalogComponent } from './card-catalog/card-catalog.component';
import { PackOpenComponent } from './pack-open/pack-open.component';

export const cardCatalogRoutes: Routes = [
  {
    path: '',
    component: CardCatalogComponent,
  },
];

export const packOpenRoutes: Routes = [
  {
    path: '',
    component: PackOpenComponent,
  },
];
