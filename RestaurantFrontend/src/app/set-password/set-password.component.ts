import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators, ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { AuthService } from '../services/auth.service'; // Update path as per your project structure
import { Router } from '@angular/router';
import { SetPasswordModel } from '../models/set-password.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-set-password',
  templateUrl: './set-password.component.html',
  styleUrls: ['./set-password.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})

export class SetPasswordComponent {
  setPasswordForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.setPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      token: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  onSubmit(): void {
    if (this.setPasswordForm.invalid) {
      alert('Please enter the details.');
      return;
    }

    if (this.setPasswordForm.value.newPassword !== this.setPasswordForm.value.confirmPassword) {
      alert('Passwords do not match.');
      return;
    }

    const formData: SetPasswordModel = {
      email: this.setPasswordForm.value.email,
      token: this.setPasswordForm.value.token,
      newPassword: this.setPasswordForm.value.newPassword,
      confirmPassword: this.setPasswordForm.value.confirmPassword,
    };

    this.authService.setPassword(formData).subscribe({
      next: (response) => {
        if (response.success) {
          alert('Password set successfully');
          this.router.navigate(['/login'], { queryParams: { email: formData.email } });
        } else {
          alert('Failed to set password. Please try again.');
        }
      },
      error: (error) => {
        alert(`Failed to set password: ${error.message}`);
      }
    });
  }
}
