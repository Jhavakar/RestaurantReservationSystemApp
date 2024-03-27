import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Customer } from '../models/customer.model'; 

@Injectable({
  providedIn: 'root'
})

export class CustomerService {
  private apiUrl = 'http://localhost:5068/api/customer'; // Note the use of 'customer' to match your ASP.NET Core route

  constructor(private http: HttpClient) { }

  // Method to create a new customer
  addCustomer(customer: Customer): Observable<Customer> {
    return this.http.post<Customer>(this.apiUrl, customer);
  }

  // Method to retrieve all customers
  getAllCustomers(): Observable<Customer[]> {
    return this.http.get<Customer[]>(this.apiUrl);
  }

  // Method to retrieve a single customer by ID
  getCustomerById(customerId: number): Observable<Customer> {
    return this.http.get<Customer>(`${this.apiUrl}/${customerId}`);
  }

  // Method to update a customer
  updateCustomer(customerId: number, customer: Customer): Observable<Customer> {
    return this.http.put<Customer>(`${this.apiUrl}/${customerId}`, customer);
  }

  // Method to delete a customer
  deleteCustomer(customerId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${customerId}`);
  }
}
