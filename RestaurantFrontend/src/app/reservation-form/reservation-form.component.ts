import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ReservationService } from '../services/reservation.service'; // Adjust path as needed

@Component({
  selector: 'app-reservation-form',
  standalone: true, 
  templateUrl: './reservation-form.component.html',
  styleUrls: ['./reservation-form.component.css'],
  imports: [
    ReactiveFormsModule, // Include ReactiveFormsModule, FormsModule, or any other dependencies
    // other modules this component depends on
  ],
})
export class ReservationFormComponent {
  reservationForm: FormGroup;

  constructor(private fb: FormBuilder, private reservationService: ReservationService) {
    this.reservationForm = this.fb.group({
      // form controls
    });
  }

  onSubmit() {
    if (this.reservationForm.valid) {
      this.reservationService.createReservation(this.reservationForm.value).subscribe({
        next: (reservation) => {
          console.log('Reservation created', reservation);
          // handle response
        },
        error: (error) => {
          console.error('There was an error!', error);
        }
      });
    }
  }
}
