import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReservationService } from '../services/reservation.service';

@Component({
  selector: 'app-reservation-form',
  standalone: true, 
  templateUrl: './reservation-form.component.html',
  styleUrls: ['./reservation-form.component.css'],
  imports: [ReactiveFormsModule, CommonModule], 
})
export class ReservationFormComponent {
  reservationForm: FormGroup;
  showConfirmation: boolean = false;

  constructor(private fb: FormBuilder, private reservationService: ReservationService) {
    this.reservationForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      reservationTime: ['', Validators.required],
      numberOfGuests: ['', [Validators.required, Validators.min(1)]],
      emailAddress: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit(): void {
    if (this.reservationForm.valid) {
      this.showConfirmation = true; // Move to the confirmation view
    } else {
      console.error('Form is not valid');
    }
  }

  onEdit(): void {
    this.showConfirmation = false; // Go back to the form to allow editing
  }

  onConfirm(): void {
    const reservationData = this.reservationForm.value;
    // Call the ReservationService to submit the data
    this.reservationService.createReservation(reservationData).subscribe({
      next: (response) => {
        console.log('Reservation confirmed:', response);
      },
      error: (error) => console.error('There was an error!', error)
    });
  }
  
}
