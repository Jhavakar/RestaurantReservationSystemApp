import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { LoginModel } from '../models/login.model';
import { SetPasswordModel } from '../models/set-password.model';

@Injectable({
  providedIn: 'root'
})

export class AuthService {
  private apiUrl = 'http://localhost:5068/api'; 
  private currentUserEmail = new BehaviorSubject<string | null>(null);
  
  constructor(private http: HttpClient, private router: Router) {}

  verifyAndFetchReservation(email: string, token: string): Observable<any> {
    const params = { email, token };
    return this.http.get<any>(`${this.apiUrl}/Users/verify-and-fetch-reservation`, { params }).pipe(
      tap(response => {
        if (response.email) {
          this.currentUserEmail.next(response.email);
        } else {
          console.error('Email not found in response:', response);
        }
      }),
      catchError(error => {
        console.error('Error during verification:', error);
        return throwError(() => new Error('Verification failed'));
      })
    );
  }

  getCurrentUserEmail(): Observable<string | null> {
    return this.currentUserEmail.asObservable();
  }

  setPassword(data: SetPasswordModel): Observable<any> {
    return this.http.post(`${this.apiUrl}/Users/reset-password`, data).pipe(
      catchError(error => {
        console.error('Error making request:', error);
        return throwError(() => new Error(`Request failed with status ${error.status}: ${error.statusText}`));
      })
    );
  }
  
  login(email: string, password: string): Observable<any> {
    const loginData: LoginModel = { email: email, password: password };
    return this.http.post<{ token: string }>(`${this.apiUrl}/Users/login`, loginData).pipe(
      tap(response => {
        localStorage.setItem('auth_token', response.token);
        this.currentUserEmail.next(email);
        this.router.navigate(['/reservation-overview']);
      }),
      catchError(error => {
        console.error('Login failed:', error);
        return throwError(() => new Error('Login failed'));
      })
    );
  }

  updatePassword(passwords: { currentPassword: string, newPassword: string }): Observable<any> {
    return this.http.post<void>(`${this.apiUrl}/Users/update-password`, passwords).pipe(
    catchError(error => {
      console.error('Error making request:', error);
      return throwError(() => new Error(`Request failed with status ${error.status}: ${error.statusText}`));
    })
    );
  }

  forgotPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Users/forgot-password`, data).pipe(
      catchError(error => {
        console.error('Error making request:', error);
        return throwError(() => new Error(`Request failed with status ${error.status}: ${error.statusText}`));
      })
    );
  }

  resetPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Users/reset-password`, data).pipe(
      catchError(error => {
        console.error('Error making request:', error);
        return throwError(() => new Error(`Request failed with status ${error.status}: ${error.statusText}`));
      })
    );
  }  

  validatePassword(email: string, password: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/validate-password`, { email, password });
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
    this.currentUserEmail.next(null);
    this.router.navigate(['/login']);  // Add this line to redirect to the login page after logout
  }
}
