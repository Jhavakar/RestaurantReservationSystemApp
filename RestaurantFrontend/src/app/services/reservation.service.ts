import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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

  createReservation(reservation: ReservationModel): Observable<ReservationModel> {
    return this.http.post<ReservationModel>(this.apiUrl, reservation);
  }

  // Implement update and delete methods similarly
}
