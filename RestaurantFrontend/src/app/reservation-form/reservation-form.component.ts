import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { ReservationService } from '../services/reservation.service';

@Component({
  selector: 'app-reservation-form',
  standalone: true, 
  templateUrl: './reservation-form.component.html',
  styleUrls: ['./reservation-form.component.css'],
  imports: [ReactiveFormsModule], 
})
export class ReservationFormComponent {
  reservationForm: FormGroup;

  constructor(private fb: FormBuilder, private reservationService: ReservationService) {
    this.reservationForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      reservationTime: ['', Validators.required],
      numberOfGuests: ['', [Validators.required, Validators.min(1)]],
    });
  }

  onSubmit() {
    if (this.reservationForm.valid) {
      this.reservationService.createReservation(this.reservationForm.value).subscribe({
        next: (reservation) => {
          console.log('Reservation created', reservation);
          this.reservationForm.reset();
        },
        error: (error) => console.error('There was an error!', error)
      });
    } else {
      console.error('Form is not valid');
    }
  }
}
