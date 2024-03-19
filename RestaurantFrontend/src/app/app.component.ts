import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ReservationFormComponent } from './reservation-form/reservation-form.component'

@Component({
  selector: 'app-root',
  standalone: true,
  styleUrl: './app.component.css',
  template: `<app-reservation-form></app-reservation-form>`,
  imports: [ReservationFormComponent],
})
export class AppComponent {
  title = 'RestaurantFrontend';
}
