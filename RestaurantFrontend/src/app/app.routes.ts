// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { CustomersComponent } from './customers/customers.component'; // Uncommented as you might still need it
import { ReservationFormComponent } from './reservation-form/reservation-form.component';
import { LoginComponent } from './pages/login/login.component';
import { UpdatePasswordComponent } from './pages/update-password/update-password.component';
import { SetPasswordComponent } from './set-password/set-password.component';
import { ManageReservationComponent } from './manage-reservation/manage-reservation.component';
import { ReservationOverviewComponent } from './reservation-overview/reservation-overview.component';
import { EditReservationComponent } from './edit-reservation/edit-reservation.component';
import { PasswordResetComponent } from './pages/password-reset/password-reset.component';
import { ReservationDetailsComponent } from './reservation-details/reservation-details.component';
import { SetupAccountComponent } from './setup-account/setup-account.component';
import { HomeComponent } from './pages/home/home.component';

export const routes: Routes = [
  // Uncommenting this route for redirecting to '/customers' by default
  // { path: '', redirectTo: '/customers', pathMatch: 'full' },
  // Re-adding the CustomersComponent route
  { path: '', redirectTo: '/home', pathMatch: 'full' }, // Default route
  { path: 'home', component: HomeComponent, title: 'Home' },
  { path: 'customers', component: CustomersComponent, title: 'Customers' },
  // Your ReservationFormComponent route
  { path: 'reserve', component: ReservationFormComponent, title: 'Make a Reservation' },
  { path: 'login', component: LoginComponent, title: 'Login to Account' },
  { path: 'update-password', component: UpdatePasswordComponent, title: 'Update Password' },
  { path: 'set-password', component: SetPasswordComponent, title: 'Set Password'},
  { path: 'manage-reservation', component: ManageReservationComponent, title: 'Manage Reservation'},
  { path: 'reservation-overview', component: ReservationOverviewComponent, title: 'Reservation Overview'},
  { path: 'edit-reservation', component: EditReservationComponent, title: 'Edit Reservation'},
  { path: 'password-reset', component: PasswordResetComponent, title: 'Password Reset'},
  { path: 'reservation-details', component: ReservationDetailsComponent, title: 'Reservation Details'},
  { path: 'setup-account', component: SetupAccountComponent, title: 'Setup Account'},

  // Define more routes as needed
];
