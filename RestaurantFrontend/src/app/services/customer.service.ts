import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Customer } from '../models/customer.model';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {

  private apiUrl = 'http://localhost:5068/api/customer';

  constructor(private http: HttpClient) {}

  // getCustomers(): Observable<Customer[]> {
  //   return this.http.get<Customer[]>(this.apiUrl);
  // }

  register(customer: Customer): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, customer);
  }

  // updateCustomer(customerId: number, customer: Customer): Observable<Customer> {
  //   return this.http.put<Customer>(`${this.apiUrl}/${customerId}`, customer);
  // }

  // deleteCustomer(customerId: number): Observable<void> {
  //   return this.http.delete<void>(`${this.apiUrl}/${customerId}`);
  // }
}
