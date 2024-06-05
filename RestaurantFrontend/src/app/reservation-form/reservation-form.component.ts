import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReservationService } from '../services/reservation.service';
import { ReservationSuccessModalComponent } from '../components/reservation-success-modal/reservation-success-modal.component';
import { TermsAndConditionsModalComponent } from '../components/terms-and-conditions-modal/terms-and-conditions-modal.component';
import { NavbarComponent } from '../components/navbar/navbar.component';

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  templateUrl: './reservation-form.component.html',
  styleUrls: ['./reservation-form.component.css'],
  imports: [ReactiveFormsModule, CommonModule, ReservationSuccessModalComponent, TermsAndConditionsModalComponent, NavbarComponent],
})

export class ReservationFormComponent implements OnInit {
  reservationForm: FormGroup;
  showConfirmation = false;
  timeSlots: string[] = [];
  reservedSlots: string[] = [];
  showModal = false;
  showTermsModal = false;
  termsAccepted = false;
  errorMessage: string | null = null;

  constructor(private fb: FormBuilder, private reservationService: ReservationService) {
    this.reservationForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      emailAddress: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      reservationDate: ['', Validators.required],
      reservationTime: ['', Validators.required],
      numberOfGuests: ['', [Validators.required, Validators.min(1)]],
      specialRequests: ['']
    });

    this.reservationForm.get('reservationDate')!.valueChanges.subscribe(date => {
      if (date) {
        this.fetchReservedSlots(date);
      }
    });
  }

  ngOnInit(): void {
    this.generateTimeSlots();
  }

  generateTimeSlots(): void {
    for (let hour = 12; hour < 22; hour++) {
      const hourString = hour.toString().padStart(2, '0');
      this.timeSlots.push(`${hourString}:00`);
      this.timeSlots.push(`${hourString}:30`);
    }
  }

  fetchReservedSlots(date: string): void {
    this.reservationService.getAvailableSlots(date).subscribe({
      next: (reservedSlots: string[]) => {
        this.reservedSlots = reservedSlots;
        console.log('Reserved slots:', this.reservedSlots);
      },
      error: (error) => {
        console.error('Error fetching reserved slots:', error);
      }
    });
  }

  isTimeSlotAvailable(timeSlot: string): boolean {
    const [hours, minutes] = timeSlot.split(':').map(Number);
    return !this.reservedSlots.includes(timeSlot);
  }

  selectTimeSlot(timeSlot: string): void {
    if (this.isTimeSlotAvailable(timeSlot)) {
      this.reservationForm.patchValue({ reservationTime: timeSlot });
    } else {
      console.log(`Time slot ${timeSlot} is not available.`);
    }
  }

  onSubmit(): void {
    if (this.reservationForm.valid) {
      this.errorMessage = null;
      this.showConfirmation = true;
    } else {
      console.error('Form is not valid');
    }
  }

  onEdit(): void {
    this.showConfirmation = false;
  }

  onConfirm(): void {
    this.errorMessage = null;

    if (!this.termsAccepted) {
      alert('You must accept the terms and conditions');
      return;
    }

    const formValues = this.reservationForm.value;
    const reservationDate = formValues.reservationDate;
    const reservationTime = formValues.reservationTime;
    const reservationDateTime = new Date(`${reservationDate}T${reservationTime}:00`);

    if (!this.isValidTimeSlot(reservationDateTime)) {
      this.errorMessage = "Invalid time slot selected.";
      return;
    }

    if (!this.isTimeSlotAvailable(reservationTime)) {
      this.errorMessage = `The time slot ${reservationTime} is unavailable for ${reservationDate}.`;
      return;
    }

    const reservationData = {
      ...formValues,
      reservationDateTime: reservationDateTime
    };

    this.reservationService.createReservation(reservationData).subscribe({
      next: (response) => {
        console.log('Reservation confirmed:', response);
        this.showModal = true;
      },
      error: (error) => {
        console.error('Error response from server:', error);
        if (error && error.status === 400 && error.error) {
          if (error.error.message && error.error.message.includes('time slot unavailable')) {
            this.errorMessage = `The time slot ${reservationTime} is unavailable for ${reservationDate}.`;
          } else {
            this.errorMessage = `Error: ${error.error.message || 'Bad Request'}`;
          }
        } else {
          this.errorMessage = 'There was an error creating the reservation.';
        }
        console.error('There was an error!', error);
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
