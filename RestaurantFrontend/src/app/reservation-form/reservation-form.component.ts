import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { ReservationService } from '../services/reservation.service';
import { ReservationSuccessModalComponent } from '../components/reservation-success-modal/reservation-success-modal.component';
import { NotificationDialogComponent } from '../components/notification-dialog/notification-dialog.component';
import { TermsAndConditionsModalComponent } from '../components/terms-and-conditions-modal/terms-and-conditions-modal.component';
import { NavbarComponent } from '../components/navbar/navbar.component';
import { SharedModule } from '../shared/shared.module';

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  templateUrl: './reservation-form.component.html',
  styleUrls: ['./reservation-form.component.css'],
  imports: [ReactiveFormsModule, CommonModule, ReservationSuccessModalComponent, TermsAndConditionsModalComponent, NavbarComponent, SharedModule],
  providers: [DatePipe]
})

export class ReservationFormComponent implements OnInit {
  reservationForm: FormGroup;
  showConfirmation = false;
  minDate: string;
  timeSlots: { time: string, disabled: boolean }[] = [];
  reservedSlots: string[] = [];
  showModal = false;
  showTermsModal = false;
  termsAccepted = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder, 
    private reservationService: ReservationService,
    private dialog: MatDialog,
    private datePipe: DatePipe
  ) {
    this.reservationForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/)]],
      phoneNumber: [''],
      reservationDate: ['', Validators.required],
      reservationTime: ['', Validators.required],
      numberOfGuests: ['', [Validators.required, Validators.min(1)]],
      specialRequests: ['']
    });

    this.reservationForm.get('reservationDate')!.valueChanges.subscribe(date => {
      if (date) {
        this.fetchReservedSlots(date);
        this.generateTimeSlots(date);
      }
    });

    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];
    this.generateTimeSlots(this.minDate); 
  }

  ngOnInit(): void {
    this.generateTimeSlots(this.minDate);
  }

  generateTimeSlots(date: string): void {
    this.timeSlots = [];
    const now = new Date();
    const selectedDate = new Date(date);
    const currentDate = this.datePipe.transform(selectedDate, 'yyyy-MM-dd');

    for (let hour = 12; hour < 23; hour++) {
      const hourString = hour.toString().padStart(2, '0');
      const times = [`${hourString}:00`, `${hourString}:30`];

      times.forEach(time => {
        const slotDateTime = new Date(`${currentDate}T${time}:00`);
        const isPast = selectedDate.toDateString() === now.toDateString() && now > slotDateTime;
        this.timeSlots.push({ time, disabled: isPast });
      });
    }
  }

  fetchReservedSlots(date: string): void {
    const formattedDate = this.formatDateForAPI(date);
    this.reservationService.getAvailableSlots(formattedDate).subscribe({
      next: (reservedSlots: string[]) => {
        this.reservedSlots = reservedSlots;
      },
      error: (error) => {
        console.error('Error fetching reserved slots:', error);
      }
    });
  }

  formatDateForAPI(date: string): string {
    const parsedDate = new Date(date);
    const year = parsedDate.getFullYear();
    const month = String(parsedDate.getMonth() + 1).padStart(2, '0');
    const day = String(parsedDate.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  isTimeSlotAvailable(timeSlot: string): boolean {
    return !this.reservedSlots.includes(timeSlot);
  }

  selectTimeSlot(timeSlot: string): void {
    if (this.isTimeSlotAvailable(timeSlot) && !this.timeSlots.find(slot => slot.time === timeSlot)?.disabled) {
      this.reservationForm.patchValue({ reservationTime: timeSlot });
    }
  }

  onSubmit(): void {
    if (this.reservationForm.valid) {
      this.errorMessage = null;
      this.showConfirmation = true;
    } else {
      this.errorMessage = 'Form is not valid';
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
    const reservationDate = this.formatDateForAPI(formValues.reservationDate);
    const reservationTime = formValues.reservationTime;
    const reservationDateTime = `${reservationDate}T${reservationTime}:00`;

    if (!this.isValidTimeSlot(new Date(reservationDateTime))) {
      this.errorMessage = "Invalid time slot selected.";
      return;
    }

    if (!this.isTimeSlotAvailable(reservationTime)) {
      this.errorMessage = `The time slot ${reservationTime} is unavailable for ${reservationDate}.`;
      return;
    }

    const reservationData = {
      ...formValues,
      reservationDate,
      reservationDateTime
    };

    this.reservationService.createReservation(reservationData).subscribe({
      next: (response) => {
        console.log('Reservation confirmed:', response);
        this.dialog.open(NotificationDialogComponent, {
          data: {
            title: 'Reservation Confirmed',
            message: 'Thank you! Your reservation has been confirmed successfully.',
          }
        });
      },
      error: (error) => {
        console.error('Error response from server:', error);
        if (error && error.status === 400 && error.error) {
          this.errorMessage = `Error: ${error.error.message || 'Bad Request'}`;
        } else {
          this.errorMessage = 'There was an error creating the reservation.';
        }
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

  openDialog(message: string, afterClosedCallback?: () => void): void {
    const dialogRef = this.dialog.open(NotificationDialogComponent, {
      data: { message }
    });

    dialogRef.afterClosed().subscribe(() => {
      if (afterClosedCallback) {
        afterClosedCallback();
      }
    });
  }

  getFormattedDate(date: string): string {
    return this.datePipe.transform(date, 'dd/MM/yyyy')!;
  }
}
