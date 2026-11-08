import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'products' },
  {
    path: 'products',
    title: 'Products · ERP Lite',
    loadComponent: () => import('./products/product-list/product-list').then((m) => m.ProductList),
  },
  {
    path: 'products/new',
    title: 'New product · ERP Lite',
    loadComponent: () => import('./products/product-form/product-form').then((m) => m.ProductForm),
  },
  { path: '**', redirectTo: 'products' },
];
