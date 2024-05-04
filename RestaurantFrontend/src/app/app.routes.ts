// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { CustomersComponent } from './customers/customers.component'; // Uncommented as you might still need it
import { ReservationFormComponent } from './reservation-form/reservation-form.component';
import { LoginComponent } from './pages/login/login.component';
import { UpdatePasswordComponent } from './pages/update-password/update-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';


export const routes: Routes = [
  // Uncommenting this route for redirecting to '/customers' by default
  // { path: '', redirectTo: '/customers', pathMatch: 'full' },
  // Re-adding the CustomersComponent route
  { path: 'customers', component: CustomersComponent, title: 'Customers' },
  // Your ReservationFormComponent route
  { path: 'reserve', component: ReservationFormComponent, title: 'Make a Reservation' },
  { path: 'login', component: LoginComponent, title: 'Login to Account' },
  { path: 'update-password', component: UpdatePasswordComponent, title: 'Update Password' },
  { path: 'reset-password', component: ResetPasswordComponent, title: 'Reset Password'},

  // Define more routes as needed
];
