import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
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
    ) { }

  isLoading = false;

  login(): void {
    if (this.loginForm.valid) {
      this.isLoading = true; 
      this.authService.login(this.loginForm.value as LoginModel).subscribe({
        next: (resp) => {
          this.isLoading = false; 
          this.router.navigate(['/reserve']);
        },
        error: (error) => {
          this.isLoading = false; 
          // Handle error
        }
      });
    }
}

}
