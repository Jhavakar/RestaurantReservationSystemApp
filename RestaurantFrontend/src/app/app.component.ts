import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { CustomersComponent } from './customers/customers.component'; 
import { ReservationFormComponent } from './reservation-form/reservation-form.component'; 
import { UpdatePasswordComponent } from './pages/update-password/update-password.component';
import { SetPasswordComponent } from './set-password/set-password.component';
import { LoginComponent } from './pages/login/login.component';
import { ManageReservationComponent } from './manage-reservation/manage-reservation.component';
import { ReservationOverviewComponent } from './reservation-overview/reservation-overview.component';
import { EditReservationComponent } from './edit-reservation/edit-reservation.component';
import { AuthInterceptor } from './interceptor/auth.interceptors.service';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  standalone: true,
  imports: [
    RouterModule, // Use your routes with RouterModule
    HttpClientModule,
    CustomersComponent,
    ReservationFormComponent,
    UpdatePasswordComponent,
    SetPasswordComponent,
    LoginComponent,
    ManageReservationComponent,
    ReservationOverviewComponent,
    EditReservationComponent,
  ],
  providers: [
    AuthService,
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
