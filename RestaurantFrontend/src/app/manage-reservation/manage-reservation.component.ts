import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators, ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manage-reservation',
  templateUrl: './manage-reservation.component.html',
  styleUrls: ['./manage-reservation.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})

export class ManageReservationComponent implements OnInit {
  manageReservationForm: FormGroup;
  loginForm: FormGroup;
  isExistingUser = false;
  reservationDetails: any = null;
  firstName = '';
  lastName = '';
  email = '';
  token = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    // Initialize the forms
    this.manageReservationForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });

    this.loginForm = this.fb.group({
      emailAddress: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    // Get email and token from query parameters
    this.route.queryParamMap.subscribe(params => {
      this.email = params.get('email') || '';
      this.token = params.get('token') || '';

      if (this.email && this.token) {
        this.fetchReservationDetails();
      } else {
        console.error('Missing email or token parameters.');
        alert('Invalid reservation link. Please use a valid link.');
      }
    });
  }

  // Fetch reservation details and identify user type
  fetchReservationDetails(): void {
    this.authService.verifyAndFetchReservation(this.email, this.token).subscribe({
      next: response => {
        console.log('API Response:', response); // Log the full response to inspect its structure
  
        // Assign variables with null-checks
        this.isExistingUser = response.hasPassword || false;
        this.reservationDetails = response.reservationDetails || null;
        this.firstName = response.firstName || 'N/A';
        this.lastName = response.lastName || 'N/A';
  
        console.log(`First Name: ${this.firstName}, Last Name: ${this.lastName}`);
        
        if (this.isExistingUser) {
          this.loginForm.patchValue({ emailAddress: this.email });
        } else {
          this.manageReservationForm.patchValue({ email: this.email });
        }
      },
      error: error => {
        console.error('Verification failed:', error);
        alert('Verification failed. Please check the link or try again.');
      }
    });
  }
  

  // Update the `onManageReservationSubmit` method:
  onManageReservationSubmit(): void {
    if (this.manageReservationForm.invalid) {
        alert('Please correct the errors on the form.');
        return;
    }

    if (this.manageReservationForm.value.newPassword !== this.manageReservationForm.value.confirmPassword) {
        alert('Passwords do not match.');
        return;
    }

    const formData = {
        email: this.manageReservationForm.value.email,
        newPassword: this.manageReservationForm.value.newPassword,
        confirmPassword: this.manageReservationForm.value.confirmPassword,
        token: this.token
    };

    this.authService.setPassword(formData).subscribe({
        next: (response) => {
            if (response.success) {
                alert('Password set successfully. Please log in.');

                // Update to show login form
                this.isExistingUser = true;
                this.loginForm.patchValue({ emailAddress: this.email });
            } else {
                alert('Failed to set password. Please try again.');
            }
        },
        error: (error) => {
            console.error('Error setting password:', error);
            alert(`Failed to set password: ${error.message}`);
        }
    });
  }

  // Submit for login form
  onLoginSubmit(): void {
    if (this.loginForm.invalid) {
      alert('Please correct the errors on the login form.');
      return;
    }

    const credentials = {
      emailAddress: this.loginForm.get('emailAddress')?.value || '',
      password: this.loginForm.get('password')?. value || ''
    };

    // Call the login service and stay on the same page
    this.authService.login(credentials.emailAddress, credentials.password).subscribe({
      next: (response) => {
        console.log('Logged in successfully:', response);
      },
      error: (error) => {
        console.error('Login failed:', error);
      }
    });
  }
}
