import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ReservationModel } from '../models/reservation.model';
import { ReservationService } from '../services/reservation.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-edit-reservation',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './edit-reservation.component.html',
  styleUrl: './edit-reservation.component.css'
})

export class EditReservationComponent implements OnInit {
  editForm: FormGroup;
  reservationId!: number;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private reservationService: ReservationService,
    private router: Router
  ) {
    this.editForm = this.fb.group({
      reservationTime: ['', Validators.required],
      numberOfGuests: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.reservationId = parseInt(this.route.snapshot.paramMap.get('id')!, 10);
    this.reservationService.getReservation(this.reservationId).subscribe({
      next: (reservation: ReservationModel) => {
        this.editForm.patchValue({
          reservationTime: reservation.reservationDateTime,
          numberOfGuests: reservation.numberOfGuests
        });
      },
      error: error => console.error('Error fetching reservation:', error)
    });
  }

  onSubmit(): void {
    if (this.editForm.valid) {
      this.reservationService.updateReservation(this.reservationId, this.editForm.value).subscribe({
        next: () => {
          alert('Reservation updated successfully.');
          this.router.navigate(['/reservation-overview']);
        },
        error: error => {
          console.error('Failed to update reservation:', error);
          alert('Failed to update the reservation.');
        }
      });
    }
  }
}