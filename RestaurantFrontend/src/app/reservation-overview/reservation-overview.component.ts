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
  }

  loadReservationsByEmail(email: string): void {
    this.reservationService.getReservationsByEmail(email).subscribe({
      next: (reservations) => {
        console.log('Reservations loaded:', reservations); 
        this.reservations = reservations;
        this.reservations.forEach(reservation => {
          this.editForms[reservation.reservationId] = this.createFormGroup(reservation);
          this.editingStates[reservation.reservationId] = false;
        });
      },
      error: (error) => console.error('Error loading reservations:', error)
    });
  }

  createFormGroup(reservation: ReservationModel): FormGroup {
    return this.fb.group({
      firstName: [reservation.user.firstName, Validators.required],
      lastName: [reservation.user.lastName, Validators.required],
      email: [reservation.user.email, [Validators.required, Validators.email]],
      reservationTime: [reservation.reservationTime, Validators.required],
      numberOfGuests: [reservation.numberOfGuests, Validators.required]
    });
  }  

  editReservation(reservationId: number): void {
    this.editingStates[reservationId] = true;
  }

  updateReservation(reservationId: number): void {
    if (this.editForms[reservationId].valid) {
      const updatedReservation = this.editForms[reservationId].value;
      this.reservationService.updateReservation(reservationId, updatedReservation).subscribe({
        next: () => {
          this.reservations = this.reservations.map(res => {
            if (res.reservationId === reservationId) {
              return { ...res, ...updatedReservation };
            }
            return res;
          });
          this.editingStates[reservationId] = false;
          alert('Reservation updated successfully!');
        },
        error: (error) => {
          console.error('Failed to update reservation:', error);
          alert('Failed to update reservation.');
        }
      });
    } else {
      alert('Please correct the errors in the form.');
    }
  }  

  deleteReservation(reservationId: number): void {
    if (confirm('Are you sure you want to delete this reservation?')) {
      this.reservationService.deleteReservation(reservationId).subscribe({
        next: () => {
          this.reservations = this.reservations.filter(r => r.reservationId !== reservationId);
          delete this.editingStates[reservationId];  // Clean up editing state
        },
        error: (error) => console.error('Error deleting reservation:', error)
      });
    }
  }

  cancelEdit(reservationId: number): void {
    this.editingStates[reservationId] = false;  // Hide the form without saving changes
  }
}
