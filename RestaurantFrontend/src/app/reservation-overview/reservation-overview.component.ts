import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { ReservationService } from '../services/reservation.service';
import { ReservationModel } from '../models/reservation.model';
import { Router } from '@angular/router';
import { ReservationSuccessModalComponent } from '../components/reservation-success-modal/reservation-success-modal.component';
import { ConfirmationDialogComponent } from '../components/confirmation-dialog/confirmation-dialog.component';
import { NotificationDialogComponent } from '../components/notification-dialog/notification-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { SharedModule } from '../shared/shared.module';

@Component({
  selector: 'app-reservation-overview',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, ReservationSuccessModalComponent, SharedModule],
  templateUrl: './reservation-overview.component.html',
  styleUrls: ['./reservation-overview.component.css'],
  providers: [DatePipe]
})

export class ReservationOverviewComponent implements OnInit {
  reservations: ReservationModel[] = [];
  editForms: { [key: number]: FormGroup } = {};
  editingStates: { [key: number]: boolean } = {};
  minDate: string;
  timeSlots: { time: string, disabled: boolean }[] = [];
  reservedSlots: string[] = [];
  showModal = false;

  constructor(
    private reservationService: ReservationService,
    private authService: AuthService,
    private fb: FormBuilder,
    private router: Router,
    private dialog: MatDialog,
    private datePipe: DatePipe
  ) {

    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];

  }

  ngOnInit(): void {
    this.authService.getCurrentUserEmail().subscribe({
      next: (email) => {
        console.log('Email fetched:', email);
        if (email) {
          this.loadReservationsByEmail(email);
        } else {
          console.error('No email found for current user.');
        }
      },
      error: (err) => console.error('Error retrieving user email:', err)
    });

    this.generateTimeSlots();
  }

  generateTimeSlots(): void {
    const now = new Date();
    const currentDate = this.datePipe.transform(now, 'yyyy-MM-dd');
    
    for (let hour = 12; hour < 23; hour++) {
      const hourString = hour.toString().padStart(2, '0');
      const timeSlots = [`${hourString}:00`, `${hourString}:30`];
      
      timeSlots.forEach(time => {
        const slotDateTime = new Date(`${currentDate}T${time}:00`);
        const isPast = now > slotDateTime;
        this.timeSlots.push({ time, disabled: isPast });
      });
    }
  }

  fetchReservedSlots(date: string): void {
    const formattedDate = this.formatDateForAPI(date);
    this.reservationService.getAvailableSlots(formattedDate).subscribe({
      next: (reservedSlots: any) => {
        this.reservedSlots = reservedSlots.map((time: any) => {
          const dateTime = new Date(time);
          return dateTime.toISOString().substr(11, 5);
        });
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

  selectTimeSlot(timeSlot: string, reservationId: number): void {
    if (this.isTimeSlotAvailable(timeSlot) && !this.timeSlots.find(slot => slot.time === timeSlot)?.disabled) {
      this.editForms[reservationId].patchValue({ reservationTime: timeSlot });
    }
  }

  loadReservationsByEmail(email: string): void {
    this.reservationService.getReservationsByEmail(email).subscribe({
      next: (reservations) => {
        console.log('Reservations loaded:', reservations);
        this.reservations = reservations.map(reservation => {
          return {
            ...reservation,
            reservationDateTime: new Date(reservation.reservationDateTime)
          };
        });
        this.reservations.forEach(reservation => {
          if (reservation.reservationId !== undefined) {
            this.editForms[reservation.reservationId] = this.createFormGroup(reservation);
            this.editingStates[reservation.reservationId] = false;
          }
        });
      },
      error: (error) => console.error('Error loading reservations:', error)
    });
  }

  createFormGroup(reservation: ReservationModel): FormGroup {
    return this.fb.group({
      firstName: [reservation.user.firstName, Validators.required],
      lastName: [reservation.user.lastName, Validators.required],
      email: [reservation.user.email, [Validators.required, Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/)]],
      phoneNumber: [reservation.user.phoneNumber],
      reservationDate: [this.datePipe.transform(reservation.reservationDateTime, 'yyyy-MM-dd'), Validators.required],
      reservationTime: [reservation.reservationDateTime.toTimeString().substring(0, 5), Validators.required],
      numberOfGuests: [reservation.numberOfGuests, [Validators.required, Validators.min(1)]],
      specialRequests: [reservation.specialRequests],
    });
  }

  editReservation(reservationId: number | undefined): void {
    if (reservationId !== undefined) {
      this.editingStates[reservationId] = true;
    }
  }

  updateReservation(reservationId: number | undefined): void {
    if (reservationId === undefined) {
      return;
    }

    if (!this.editForms[reservationId].valid) {
      alert('Please correct the errors in the form.');
      return;
    }

    const updatedData = this.editForms[reservationId].value;
    const existingReservation = this.reservations.find(r => r.reservationId === reservationId);

    if (!existingReservation) {
      console.error(`No reservation found with id ${reservationId}`);
      return;
    }

    const reservationDate = this.formatDateForAPI(updatedData.reservationDate);
    const reservationTime = updatedData.reservationTime;
    const reservationDateTime = new Date(`${reservationDate}T${reservationTime}:00`);

    if (!this.isValidTimeSlot(reservationDateTime)) {
      alert("Invalid time slot selected.");
      return;
    }

    const payload: ReservationModel = {
      reservationId: reservationId,
      reservationDateTime: reservationDateTime,
      numberOfGuests: updatedData.numberOfGuests,
      user: {
        id: existingReservation.user.id,
        firstName: updatedData.firstName,
        lastName: updatedData.lastName,
        email: updatedData.email,
        phoneNumber: updatedData.phoneNumber
      },
      firstName: updatedData.firstName,
      lastName: updatedData.lastName,
      email: updatedData.email,
      phoneNumber: updatedData.phoneNumber,
      reservationDate: reservationDate,
      reservationTime: reservationTime,
      specialRequests: updatedData.specialRequests,
      isNewAccount: false
    };

    this.reservationService.updateReservation(reservationId, payload).subscribe({
      next: () => {
        this.reservations = this.reservations.map(res => res.reservationId === reservationId ? { ...res, ...payload } : res);
        this.editingStates[reservationId] = false;

        // Show modal after successful update
        this.showModal = true;
      },
      error: (error) => {
        alert(`Failed to update reservation: ${error.error.errors}`);
      }
    });
  }

  isValidTimeSlot(dateTime: Date): boolean {
    const minutes = dateTime.getMinutes();
    return minutes === 0 || minutes === 30;
  }

  deleteReservation(reservationId: number | undefined): void {
    if (reservationId !== undefined) {
      const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
        data: {
          title: 'Confirm Delete',
          message: 'Are you sure you want to delete this reservation?'
        }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.reservationService.deleteReservation(reservationId).subscribe({
            next: () => {
              this.reservations = this.reservations.filter(r => r.reservationId !== reservationId);
              delete this.editingStates[reservationId];
            },
            error: (error) => console.error('Error deleting reservation:', error)
          });
        }
      });
    }
  }

  cancelEdit(reservationId: number | undefined): void {
    if (reservationId !== undefined) {
      this.editingStates[reservationId] = false;
    }
  }

  closeModal(): void {
    this.showModal = false;
  }

  openDialog(message: string, afterClosedCallback?: () => void): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: { message }
    });

    dialogRef.afterClosed().subscribe(() => {
      if (afterClosedCallback) {
        afterClosedCallback();
      }
    });
  }

  openNotificationDialog(message: string, afterClosedCallback?: () => void): void {
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
