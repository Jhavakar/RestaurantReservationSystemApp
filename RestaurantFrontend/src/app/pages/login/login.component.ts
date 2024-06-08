import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LoginModel } from '../../models/login.model';

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  imports: [ReactiveFormsModule],
})

export class LoginComponent {
  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required])
  });

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
  ) { }

  login(): void {
    if (this.loginForm.valid) {
      const credentials: LoginModel = {
        email: this.loginForm.get('email')?.value || '',
        password: this.loginForm.get('password')?.value || ''
      };

      this.authService.login(credentials.email, credentials.password).subscribe({
        next: (response) => {
          this.router.navigate(['/reservation-overview']);
          console.log('Navigated to:', response);
        },
        error: (error) => {
          console.error('Login failed:', error);
        }
      });
    }
  }

  navigateToForgotPassword(): void {
    this.router.navigate(['/password-reset']);
  }
}
