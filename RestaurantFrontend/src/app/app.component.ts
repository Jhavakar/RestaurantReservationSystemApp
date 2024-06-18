import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReservationFormComponent } from './pages/reservation-form/reservation-form.component'; 
import { UpdatePasswordComponent } from './pages/update-password/update-password.component';
import { LoginComponent } from './pages/login/login.component';
import { ManageReservationComponent } from './pages/manage-reservation/manage-reservation.component';
import { ReservationOverviewComponent } from './pages/reservation-overview/reservation-overview.component';
import { PasswordResetComponent } from './pages/password-reset/password-reset.component';
import { AuthInterceptor } from './interceptor/auth.interceptors.service';
import { AuthService } from './services/auth.service';
import { ReservationDetailsComponent } from './pages/reservation-details/reservation-details.component';
import { SetupAccountComponent } from './pages/setup-account/setup-account.component';
import { NavbarComponent } from './components/navbar/navbar.component';
import { AccountDashboardComponent } from './pages/account-dashboard/account-dashboard.component';
import { AboutUsComponent } from './pages/about-us/about-us.component';
import { SharedModule } from './shared/shared.module';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, DateAdapter, MAT_DATE_FORMATS } from '@angular/material/core';
import { CustomDateAdapter } from './custom-date-adapter';
import { DatePipe } from '@angular/common';

export const MY_DATE_FORMATS = {
  parse: {
    dateInput: 'DD/MM/YYYY',
  },
  display: {
    dateInput: 'DD/MM/YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'DD/MM/YYYY',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  standalone: true,
  imports: [
    RouterModule, // Use your routes with RouterModule
    HttpClientModule,
    ReservationFormComponent,
    UpdatePasswordComponent,
    LoginComponent,
    ManageReservationComponent,
    ReservationOverviewComponent,
    PasswordResetComponent,
    ReservationDetailsComponent,
    SetupAccountComponent,
    NavbarComponent,
    AccountDashboardComponent,
    AboutUsComponent,
    SharedModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  providers: [
    AuthService,
    DatePipe,
    { provide: DateAdapter, useClass: CustomDateAdapter },
    { provide: MAT_DATE_FORMATS, useValue: MY_DATE_FORMATS },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
    // other services
  ],
})
export class AppComponent {
  title = 'RestaurantFrontend';
}
