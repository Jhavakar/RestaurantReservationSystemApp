// app.component.ts
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { CustomersComponent } from './customers/customers.component'; // Adjust path as necessary
import { ReservationFormComponent } from './reservation-form/reservation-form.component'; // Adjust path as necessary

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
    // Include other standalone components or necessary modules here
  ],
})
export class AppComponent {
  title = 'RestaurantFrontend';
}
