// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { ReservationFormComponent } from './pages/reservation-form/reservation-form.component';
import { LoginComponent } from './pages/login/login.component';
import { UpdatePasswordComponent } from './pages/update-password/update-password.component';
import { ManageReservationComponent } from './pages/manage-reservation/manage-reservation.component';
import { ReservationOverviewComponent } from './pages/reservation-overview/reservation-overview.component';
import { PasswordResetComponent } from './pages/password-reset/password-reset.component';
import { ReservationDetailsComponent } from './pages/reservation-details/reservation-details.component';
import { SetupAccountComponent } from './pages/setup-account/setup-account.component';
import { HomeComponent } from './pages/home/home.component';
import { AccountDashboardComponent } from './pages/account-dashboard/account-dashboard.component';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' }, // Default route
  { path: 'home', component: HomeComponent, title: 'Home' },
  // Your ReservationFormComponent route
  { path: 'reserve', component: ReservationFormComponent, title: 'Make a Reservation' },
  { path: 'login', component: LoginComponent, title: 'Login to Account' },
  { path: 'update-password', component: UpdatePasswordComponent, title: 'Update Password' },
  { path: 'manage-reservation', component: ManageReservationComponent, title: 'Manage Reservation'},
  { path: 'reservation-overview', component: ReservationOverviewComponent, title: 'Reservation Overview'},
  { path: 'password-reset', component: PasswordResetComponent, title: 'Password Reset'},
  { path: 'reservation-details', component: ReservationDetailsComponent, title: 'Reservation Details'},
  { path: 'setup-account', component: SetupAccountComponent, title: 'Setup Account'},
  { path: 'account-dashboard', component: AccountDashboardComponent, title: 'Account Dashboard'},
  // Define more routes as needed
];
