import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-reservation-details',
  templateUrl: './reservation-details.component.html',
  styleUrls: ['./reservation-details.component.css'],
  standalone: true,
  imports: [CommonModule, RouterModule]
})

export class ReservationDetailsComponent implements OnInit {
  reservationDetails: any = null;
  isExistingUser = false;
  firstName = '';
  lastName = '';
  email = '';
  token = '';

  constructor(
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.email = params.get('email') || '';
      this.token = params.get('token') || '';

      if (this.email && this.token) {
        this.fetchReservationDetails();
      } else {
        alert('Invalid reservation link. Please use a valid link.');
      }
    });
  }

  fetchReservationDetails(): void {
    this.authService.verifyAndFetchReservation(this.email, this.token).subscribe({
      next: response => {
        console.log('API Response:', response);
        this.reservationDetails = response.reservationDetails || null;
        this.firstName = response.firstName || 'N/A';
        this.lastName = response.lastName || 'N/A';
        this.isExistingUser = response.hasPassword || false;
      },
      error: error => {
        alert('Verification failed. Please check the link or try again.');
      }
    });
  }
}
