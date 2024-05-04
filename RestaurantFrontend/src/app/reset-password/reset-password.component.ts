import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators, ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { AuthService } from '../services/auth.service'; // Update path as per your project structure
import { Router } from '@angular/router';
import { ResetPasswordModel } from '../models/reset-password.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})

export class ResetPasswordComponent {
  resetPasswordForm: FormGroup;

  constructor(
    private fb: FormBuilder, // Using FormBuilder to create the form group
    private authService: AuthService,
    private router: Router
  ) {
    this.resetPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      token: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  onSubmit(): void {
    if (this.resetPasswordForm.invalid) {
      alert('Please correct the errors on the form.');
      return;
    }

    if (this.resetPasswordForm.value.newPassword !== this.resetPasswordForm.value.confirmPassword) {
      alert('Passwords do not match.');
      return;
    }

    // Extract the data from the form
    const formData: ResetPasswordModel = {
      email: this.resetPasswordForm.value.email,
      token: this.resetPasswordForm.value.token,
      newPassword: this.resetPasswordForm.value.newPassword,
      confirmPassword: this.resetPasswordForm.value.confirmPassword,
    };

    // Immediately submit the form data
    this.authService.resetPassword(formData).subscribe({
      next: (response) => {
        if (response.success) {
          alert('Password reset successful');
          console.log('Reset password response:', response);
          this.router.navigate(['/login']);
        } else {
          alert('Failed to reset password. Please try again.');
        }
      },
      error: (error) => {
        console.error('Error resetting password:', error);
        alert(`Failed to reset password: ${error.message}`);
      }
    });    
  }
}