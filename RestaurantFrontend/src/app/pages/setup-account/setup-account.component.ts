import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { NotificationDialogComponent } from '../../components/notification-dialog/notification-dialog.component';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-setup-account',
  templateUrl: './setup-account.component.html',
  styleUrls: ['./setup-account.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
})

export class SetupAccountComponent implements OnInit {
  signUpForm: FormGroup;
  isExistingUser = false;
  email = '';
  token = '';
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private dialog: MatDialog
  ) {

    this.signUpForm = this.fb.group({
      email: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/)]], 
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
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
    this.authService.verifyAndFetchReservation(this.email, this.token).subscribe({
      next: response => {
        this.isExistingUser = response.hasPassword || false;
        if (this.isExistingUser) {
          this.router.navigate(['/login'], { queryParams: { email: this.email, token: this.token } });
        }
      },
      error: (error) => {
        console.error('Verification failed:', error);
        alert('Verification failed. Please check the link or try again.');
      }
    });
  }

  onSignUpSubmit(): void {
    if (this.signUpForm.invalid) {
      alert('Please correct the errors on the form.');
      return;
    }

    if (this.signUpForm.value.newPassword !== this.signUpForm.value.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
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
          this.dialog.open(NotificationDialogComponent, {
            data: {
              title: 'Success',
              message: 'Your password has been set successfully. Please log in to continue.',
              confirmText: 'OK'
            }
          }).afterClosed().subscribe(() => {
            this.router.navigate(['/login'], { queryParams: { email: this.email, token: this.token } });
          });
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

  openDialog(message: string, afterClosedCallback?: () => void): void {
    const dialogRef = this.dialog.open(NotificationDialogComponent, {
      data: { message }
    });

    dialogRef.afterClosed().subscribe(() => {
      if (afterClosedCallback) {
        afterClosedCallback();
      }
    });
  }
}
