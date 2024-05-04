import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { LoginModel } from '../models/login.model';
import { ResetPasswordModel } from '../models/reset-password.model';

interface LoginResponse {
  token: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5068/api'; 

  constructor(private http: HttpClient, private router: Router) { }

  resetPassword(data: ResetPasswordModel): Observable<any> {
    return this.http.post(`${this.apiUrl}/Users/ResetPassword`, data).pipe(
      catchError(error => {
        console.error('Error making request:', error);
        return throwError(() => new Error(`Request failed with status ${error.status}: ${error.statusText}`));
      })
    );
  }
  
  
  login(email: string, password: string): Observable<any> {
    const loginData: LoginModel = { emailAddress: email, password: password };
    return this.http.post<{ token: string, isTempPassword: boolean }>(
      `${this.apiUrl}/login`, loginData
    ).pipe(
      tap(response => {
        localStorage.setItem('auth_token', response.token);
        if (response.isTempPassword) {
          this.router.navigate(['/update-password']);
        } else {
          this.router.navigate(['/dashboard']);
        }
      }),
        catchError(error => {
            console.error('Login failed:', error);
            return throwError(() => new Error('Login failed'));
        })
    );
    
}

  // Method to log in and store the JWT token
  // login(credentials: LoginModel): Observable<LoginResponse> {
  //   // Adjust the URL to match your backend endpoint
  //   return this.http.post<LoginResponse>(`${this.apiBaseUrl}/login`, credentials).pipe(
  //     tap(response => {
  //       // Assuming the response includes a token, store it for future requests
  //       if (response.token) {
  //         this.storeToken(response.token);
  //       }
  //     })
  //   );
  // }

  // Method to store the JWT token in local storage
  private storeToken(token: string): void {
    localStorage.setItem('auth_token', token);
  }

  // Method to get the JWT token from local storage
  getJwtToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  // Method to check if the user is logged in
  isLoggedIn(): boolean {
    return !!this.getJwtToken();
  }

  // Method to log out the user and clear the stored token
  logout(): void {
    localStorage.removeItem('auth_token');
  }
}
