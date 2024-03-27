import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiBaseUrl = 'https://localhost:5068/api'; // Replace with your actual API base URL

  constructor(private http: HttpClient) {}

  login(username: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiBaseUrl}/login`, { username, password })
        .pipe(tap(response => this.storeJwtToken(response.access_token)));
  }

  private storeJwtToken(jwtToken: string): void {
    localStorage.setItem('access_token', jwtToken);
  }

  getJwtToken(): string | null {
    return localStorage.getItem('access_token');
  }

  logout(): void {
    localStorage.removeItem('access_token');
    // Additional logout operations here, such as redirecting the user
  }
  }
