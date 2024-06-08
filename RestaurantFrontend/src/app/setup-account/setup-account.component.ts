import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-setup-account',
  templateUrl: './setup-account.component.html',
  styleUrls: ['./setup-account.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterModule]
})

export class SetupAccountComponent implements OnInit {
  loginForm: FormGroup;
  signUpForm: FormGroup;
  isExistingUser = false;
  email = '';
  token = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]], 
      password: ['', [Validators.required]]
    });

    this.signUpForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]], 
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.email = params.get('email') || '';
      this.token = params.get('token') || '';

      if (this.email) {
        this.signUpForm.patchValue({ email: this.email });
      }

      if (this.email && this.token) {
        this.checkExistingUser();
      } else {
        console.error('Missing email or token parameters.');
        if (!this.email) {
          alert('Invalid reservation link. Please use a valid link.');
        }
      }
    });
  }

  checkExistingUser(): void {
    if (this.email && this.token) {
      this.authService.verifyAndFetchReservation(this.email, this.token).subscribe({
        next: response => {
          this.isExistingUser = response.hasPassword || false;
          if (this.isExistingUser) {
            this.router.navigate(['/login'], { queryParams: { email: this.email, token: this.token } });
          } else {
            this.signUpForm.patchValue({ email: this.email });
          }
        },
        error: error => {
          console.error('Verification failed:', error);
          alert('Verification failed. Please check the link or try again.');
        }
      });
    }
  }

  onSignUpSubmit(): void {
    if (this.signUpForm.invalid) {
      alert('Please correct the errors on the form.');
      return;
    }

    if (this.signUpForm.value.newPassword !== this.signUpForm.value.confirmPassword) {
      alert('Passwords do not match.');
      return;
    }
  
    const formData = {
      email: this.signUpForm.value.email,
      newPassword: this.signUpForm.value.newPassword,
      confirmPassword: this.signUpForm.value.confirmPassword,
      token: this.token
    };
  
    this.authService.setPassword(formData).subscribe({
      next: (response) => {
        if (response.success) {
          alert('Password set successfully. Please log in.');
          this.router.navigate(['/login'], { queryParams: { email: this.email, token: this.token } });
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
  

  onLoginSubmit(): void {
    if (this.loginForm.invalid) {
      alert('Please correct the errors on the login form.');
      return;
    }

    const credentials = {
      email: this.loginForm.get('email')?.value || '', 
      password: this.loginForm.get('password')?.value || ''
    };

    this.authService.login(credentials.email, credentials.password).subscribe({
      next: (response) => {
        console.log('Logged in successfully:', response);
      },
      error: (error) => {
        console.error('Login failed:', error);
        alert('Login failed. Please try again.');
      }
    });
  }
}
