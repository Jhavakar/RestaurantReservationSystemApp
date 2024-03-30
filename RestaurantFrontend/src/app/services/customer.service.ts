import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustomerModel } from '../models/customer.model'; 

@Injectable({
  providedIn: 'root'
})

export class CustomerService {
  private apiUrl = 'http://localhost:5068/api/customer'; // Note the use of 'customer' to match your ASP.NET Core route

  constructor(private http: HttpClient) { }

  // Method to create a new customer
  addCustomer(customer: CustomerModel): Observable<CustomerModel> {
    return this.http.post<CustomerModel>(this.apiUrl, customer);
  }

  // Method to retrieve all customers
  getAllCustomers(): Observable<CustomerModel[]> {
    return this.http.get<CustomerModel[]>(this.apiUrl);
  }

  // Method to retrieve a single customer by ID
  getCustomerById(customerId: number): Observable<CustomerModel> {
    return this.http.get<CustomerModel>(`${this.apiUrl}/${customerId}`);
  }

  // Method to update a customer
  updateCustomer(customerId: number, customer: CustomerModel): Observable<CustomerModel> {
    return this.http.put<CustomerModel>(`${this.apiUrl}/${customerId}`, customer);
  }

  // Method to delete a customer
  deleteCustomer(customerId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${customerId}`);
  }
}
