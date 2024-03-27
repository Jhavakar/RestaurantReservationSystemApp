// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { CustomersComponent } from './customers/customers.component'; // Uncommented as you might still need it
import { ReservationFormComponent } from './reservation-form/reservation-form.component';

export const routes: Routes = [
  // Uncommenting this route for redirecting to '/customers' by default
  // { path: '', redirectTo: '/customers', pathMatch: 'full' },
  // Re-adding the CustomersComponent route
  { path: 'customers', component: CustomersComponent, title: 'Customers' },
  // Your ReservationFormComponent route
  { path: 'reserve', component: ReservationFormComponent, title: 'Make a Reservation' },

  // Define more routes as needed
];
