import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginModel } from '../models/login.model';

interface LoginResponse {
  token: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiBaseUrl = 'https://localhost:5068/api'; 

  constructor(private http: HttpClient) { }

  // Method to log in and store the JWT token
  login(credentials: LoginModel): Observable<LoginResponse> {
    // Adjust the URL to match your backend endpoint
    return this.http.post<LoginResponse>(`${this.apiBaseUrl}/login`, credentials).pipe(
      tap(response => {
        // Assuming the response includes a token, store it for future requests
        if (response.token) {
          this.storeToken(response.token);
        }
      })
    );
  }

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
