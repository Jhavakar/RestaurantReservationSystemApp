// app.component.ts
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { CustomersComponent } from './customers/customers.component'; // Adjust path as necessary
import { ReservationFormComponent } from './reservation-form/reservation-form.component'; // Adjust path as necessary
import { LoginComponent } from './pages/login/login.component';
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
    LoginComponent,
    // Include other standalone components or necessary modules here
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
