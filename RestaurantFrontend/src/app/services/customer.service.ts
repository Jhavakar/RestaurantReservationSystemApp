import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustomerModel } from '../models/customer.model';

@Injectable({
  providedIn: 'root'
})

export class CustomerService {
  private apiUrl = 'http://localhost:5068/api/customer'; 

  constructor(private http: HttpClient) { }

  addCustomer(customer: CustomerModel): Observable<CustomerModel> {
    return this.http.post<CustomerModel>(this.apiUrl, customer);
  }

  getAllCustomers(): Observable<CustomerModel[]> {
    return this.http.get<CustomerModel[]>(this.apiUrl);
  }

  getCustomerById(customerId: number): Observable<CustomerModel> {  
    return this.http.get<CustomerModel>(`${this.apiUrl}/${customerId}`);
  }

  getCustomerByEmail(email: string): Observable<CustomerModel> {
    return this.http.get<CustomerModel>(`${this.apiUrl}/email/${email}`);
  }

  updateCustomer(customer: CustomerModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/email/${customer.email}`, customer);
  }

  deleteCustomer(customerId: number): Observable<void> {  
    return this.http.delete<void>(`${this.apiUrl}/${customerId}`);
  }
}
