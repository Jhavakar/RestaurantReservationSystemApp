import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReservationService } from '../services/reservation.service';
import { ReservationSuccessModalComponent } from '../components/reservation-success-modal/reservation-success-modal.component';
import { TermsAndConditionsModalComponent } from '../components/terms-and-conditions-modal/terms-and-conditions-modal.component';

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  templateUrl: './reservation-form.component.html',
  styleUrls: ['./reservation-form.component.css'],
  imports: [ReactiveFormsModule, CommonModule, ReservationSuccessModalComponent, TermsAndConditionsModalComponent],
})
export class ReservationFormComponent implements OnInit {
  reservationForm: FormGroup;
  showConfirmation: boolean = false;
  timeSlots: string[] = [];
  showModal = false;
  showTermsModal = false;
  termsAccepted = false;

  constructor(private fb: FormBuilder, private reservationService: ReservationService) {
    this.reservationForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      emailAddress: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      reservationDate: ['', Validators.required],
      reservationTime: ['', Validators.required],
      numberOfGuests: ['', [Validators.required, Validators.min(1)]],
    });
  }

  ngOnInit(): void {
    this.generateTimeSlots();
  }

  generateTimeSlots(): void {
    for (let hour = 0; hour < 24; hour++) {
      const hourString = hour.toString().padStart(2, '0');
      this.timeSlots.push(`${hourString}:00`);
      this.timeSlots.push(`${hourString}:30`);
    }
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
    if (!this.termsAccepted) {
      alert('You must accept the terms and conditions');
      return;
    }

    const formValues = this.reservationForm.value;
    const reservationDate = formValues.reservationDate;
    const reservationTime = formValues.reservationTime;
    const reservationDateTime = new Date(`${reservationDate}T${reservationTime}:00`);

    if (!this.isValidTimeSlot(reservationDateTime)) {
      console.error("Invalid time slot selected.");
      alert("Invalid time slot selected.");
      return;
    }

    const reservationData = {
      ...formValues,
      reservationDateTime: reservationDateTime
    };

    // Call the ReservationService to submit the data
    this.reservationService.createReservation(reservationData).subscribe({
      next: (response) => {
        console.log('Reservation confirmed:', response);
        this.showModal = true;
      },
      error: (error) => {
        console.error('There was an error!', error);
        alert('There was an error creating the reservation.');
      }
    });
  }

  isValidTimeSlot(dateTime: Date): boolean {
    const minutes = dateTime.getMinutes();
    return minutes === 0 || minutes === 30;
  }

  openTermsModal(event: Event) {
    event.preventDefault();
    this.showTermsModal = true;
  }
  

  closeModal(): void {
    this.showModal = false;
  }
}
