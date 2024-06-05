import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-password-reset',
  standalone: true,
  templateUrl: './password-reset.component.html',
  styleUrls: ['./password-reset.component.css'],
  imports: [CommonModule, ReactiveFormsModule]
})
export class PasswordResetComponent {
  forgotPasswordForm: FormGroup;
  resetPasswordForm: FormGroup;
  token: string | null = null;
  email: string | null = null;
  message: string = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private authService: AuthService
  ) {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.resetPasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.token = params['token'];
      this.email = params['email'];
    });
  }

  onSubmitForgotPassword(): void {
    if (this.forgotPasswordForm.valid) {
      this.authService.forgotPassword(this.forgotPasswordForm.value).subscribe(
        response => {
          this.message = 'Password reset link has been sent to your email.';
        },
        error => {
          this.message = 'Failed to send password reset link.';
        }
      );
    }
  }

  onSubmitResetPassword(): void {
    if (this.resetPasswordForm.valid && this.token && this.email) {
      const data = {
        token: this.token,
        email: this.email,
        newPassword: this.resetPasswordForm.value.newPassword,
        confirmPassword: this.resetPasswordForm.value.confirmPassword
      };
      this.authService.resetPassword(data).subscribe(
        response => {
          this.message = 'Password has been successfully reset.';
        },
        error => {
          this.message = 'Failed to reset password.';
        }
      );
    } else {
      this.message = 'Invalid form submission.';
    }
  }
}