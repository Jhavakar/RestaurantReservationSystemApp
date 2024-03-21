// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { CustomersComponent } from './customers/customers.component';

export const routes: Routes = [
  { path: '', redirectTo: '/customers', pathMatch: 'full' },
  { path: 'customers', component: CustomersComponent, title: 'Customers' },
  // Define more routes as needed
];
