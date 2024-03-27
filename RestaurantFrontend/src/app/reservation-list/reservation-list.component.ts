import { Component, OnInit } from '@angular/core';
import { ReservationService } from '../services/reservation.service';
import { Reservation } from '../models/reservation.model';

@Component({
  selector: 'app-reservation-list',
  templateUrl: './reservation-list.component.html'
})
export class ReservationListComponent implements OnInit {
  reservations: Reservation[] = [];

  constructor(private reservationService: ReservationService) { }

  ngOnInit() {
    this.reservationService.getReservations().subscribe({
      next: (reservations) => this.reservations = reservations,
      error: (err) => console.error('Error fetching reservations', err)
    });
  }
}
