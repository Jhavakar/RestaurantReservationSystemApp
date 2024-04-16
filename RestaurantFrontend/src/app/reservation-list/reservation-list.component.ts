import { Component, OnInit } from '@angular/core';
import { ReservationService } from '../services/reservation.service';
import { ReservationModel } from '../models/reservation.model';

@Component({
  selector: 'app-reservation-list',
  templateUrl: './reservation-list.component.html'
})
export class ReservationListComponent implements OnInit {
  reservations: ReservationModel[] = [];

  constructor(private reservationService: ReservationService) { }

  ngOnInit() {
    this.reservationService.getReservations().subscribe({
      next: (reservations) => this.reservations = reservations,
      error: (err) => console.error('Error fetching reservations', err)
    });
  }
}
