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
    emailAddress: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required])
});

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    ) { }

  login(): void {
    if (this.loginForm.valid) {
        // Explicitly create an object that matches the LoginModel interface
        const credentials: LoginModel = {
          emailAddress: this.loginForm.get('emailAddress')?.value || '',
          password: this.loginForm.get('password')?.value || ''
        };

        this.authService.login(credentials.emailAddress, credentials.password).subscribe({
          next: (response) => {
            this.router.navigate(['/customers']);
            console.log('Navigated to:', response);

          },
          error: (error) => {
            console.error('Login failed:', error);
          }
        });
    }
}

}
