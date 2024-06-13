import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerDetailsComponent } from '../customer-details/customer-details.component';
import { ReservationOverviewComponent } from '../reservation-overview/reservation-overview.component';
import { AuthService } from '../services/auth.service';
import { CustomerService } from '../services/customer.service';
import { Router } from '@angular/router';
import { CustomerModel } from '../models/customer.model';

@Component({
  selector: 'app-account-dashboard',
  standalone: true,
  imports: [CommonModule, CustomerDetailsComponent, ReservationOverviewComponent],
  templateUrl: './account-dashboard.component.html',
  styleUrls: ['./account-dashboard.component.css'],
})

export class AccountDashboardComponent implements OnInit {
  activeSection: string = 'customer-details';
  userEmail: string | null = null;
  customer: CustomerModel | null = null;
  
  constructor(
    private authService: AuthService,
    private customerService: CustomerService,
    private router: Router

    ) {}

  ngOnInit(): void {
    this.authService.getCurrentUserEmail().subscribe({
      next: (email) => {
        this.userEmail = email;
        if (email) {
          this.loadCustomerByEmail(email); 
        } else {
          console.error('No email found for current user.');
        }
      },
      error: (err) => console.error('Error retrieving user email:', err)
    });

     // Listen for the custom event
     window.addEventListener('customerUpdated', this.reloadCustomerDetails);
  }

  ngOnDestroy(): void {
    // Clean up the event listener
    window.removeEventListener('customerUpdated', this.reloadCustomerDetails);
  }

  loadCustomerByEmail(email: string): void {
    this.customerService.getCustomerByEmail(email).subscribe({
      next: (customer) => {
        this.customer = customer;
      },
      error: (error) => console.error('Failed to load customer', error)
    });
  }

  showSection(section: string) {
    this.activeSection = section;
  }

  logout() {
    console.log('Logging out...');
    this.router.navigate(['/login']);
  }

  reloadCustomerDetails = (): void => {
    if (this.userEmail) {
      this.loadCustomerByEmail(this.userEmail);
    }
  }
}
