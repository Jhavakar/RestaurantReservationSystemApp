import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReservationModel } from '../models/reservation.model'; 

@Injectable({
  providedIn: 'root'
})

export class ReservationService {
  private apiUrl = 'http://localhost:5068/api/reservations'; 

  constructor(private http: HttpClient) { }

  getReservations(): Observable<ReservationModel[]> {
    return this.http.get<ReservationModel[]>(this.apiUrl);
  }

  getReservation(id: number): Observable<ReservationModel> {
    return this.http.get<ReservationModel>(`${this.apiUrl}/${id}`);
  }

  getReservationsByEmail(email: string): Observable<ReservationModel[]> {
    return this.http.get<ReservationModel[]>(`${this.apiUrl}/user-reservations?email=${encodeURIComponent(email)}`);
  }

  createReservation(reservation: ReservationModel): Observable<ReservationModel> {
    return this.http.post<ReservationModel>(this.apiUrl, reservation);
  }

  updateReservation(id: number, reservation: ReservationModel): Observable<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json' // Ensures correct content type header
    });

    return this.http.put(`${this.apiUrl}/${id}`, reservation, { headers });
  }
  

  deleteReservation(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
