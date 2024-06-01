import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { ReservationService } from '../services/reservation.service';
import { ReservationModel } from '../models/reservation.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-reservation-overview',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './reservation-overview.component.html',
  styleUrl: './reservation-overview.component.css'
})

export class ReservationOverviewComponent implements OnInit {
  reservations: ReservationModel[] = [];
  editForms: { [key: number]: FormGroup } = {};
  editingStates: { [key: number]: boolean } = {};
  timeSlots: string[] = [];

  constructor(
    private reservationService: ReservationService,
    private authService: AuthService,
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.getCurrentUserEmail().subscribe({
      next: (email) => {
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
    for (let hour = 0; hour < 24; hour++) {
      const hourString = hour.toString().padStart(2, '0');
      this.timeSlots.push(`${hourString}:00`);
      this.timeSlots.push(`${hourString}:30`);
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
      emailAddress: [reservation.user.emailAddress, [Validators.required, Validators.email]],
      phoneNumber: [reservation.user.phoneNumber],
      reservationDate: [reservation.reservationDateTime.toISOString().split('T')[0], Validators.required],
      reservationTime: [reservation.reservationDateTime.toTimeString().substring(0, 5), Validators.required],
      numberOfGuests: [reservation.numberOfGuests, [Validators.required, Validators.min(1)]]
    });
  }

  editReservation(reservationId: number | undefined): void {
    if (reservationId !== undefined) {
      this.editingStates[reservationId] = true;
    }
  }

  updateReservation(reservationId: number | undefined): void {
    if (reservationId === undefined) {
      console.error('Reservation ID is undefined.');
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

    const reservationDate = updatedData.reservationDate; // 'yyyy-MM-dd' format
    const reservationTime = updatedData.reservationTime; // 'HH:mm' format
    const reservationDateTime = new Date(`${reservationDate}T${reservationTime}:00`);

    console.log(`Formatted Reservation Date: ${reservationDate}`);
    console.log(`Formatted Reservation Time: ${reservationTime}`);
    console.log(`Combined Reservation DateTime: ${reservationDateTime.toISOString()}`);

    if (!this.isValidTimeSlot(reservationDateTime)) {
      console.error("Invalid time slot selected.");
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
        emailAddress: updatedData.emailAddress,
        phoneNumber: updatedData.phoneNumber
      },
      firstName: updatedData.firstName,
      lastName: updatedData.lastName,
      emailAddress: updatedData.emailAddress,
      phoneNumber: updatedData.phoneNumber,
      reservationDate: reservationDate,
      reservationTime: reservationTime,
      isNewAccount: false
    };

    console.log('Payload being sent:', payload);

    this.reservationService.updateReservation(reservationId, payload).subscribe({
      next: () => {
        this.reservations = this.reservations.map(res => res.reservationId === reservationId ? { ...res, ...payload } : res);
        this.editingStates[reservationId] = false;
        alert('Reservation updated successfully!');
      },
      error: (error) => {
        console.error('Failed to update reservation:', error);
        alert(`Failed to update reservation: ${error.error.errors}`);
      }
    });
  }

  isValidTimeSlot(dateTime: Date): boolean {
    const minutes = dateTime.getMinutes();
    return minutes === 0 || minutes === 30;
  }

  deleteReservation(reservationId: number | undefined): void {
    if (reservationId !== undefined && confirm('Are you sure you want to delete this reservation?')) {
      this.reservationService.deleteReservation(reservationId).subscribe({
        next: () => {
          this.reservations = this.reservations.filter(r => r.reservationId !== reservationId);
          delete this.editingStates[reservationId];
        },
        error: (error) => console.error('Error deleting reservation:', error)
      });
    }
  }

  cancelEdit(reservationId: number | undefined): void {
    if (reservationId !== undefined) {
      this.editingStates[reservationId] = false;
    }
  }
}
