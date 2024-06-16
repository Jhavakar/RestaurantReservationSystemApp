import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CustomerService } from '../services/customer.service';
import { CustomerModel } from '../models/customer.model';

@Component({
  selector: 'app-customers',
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})

export class CustomersComponent implements OnInit {
  customerForm: FormGroup;
  customers: CustomerModel[] = []; // Store the list of customers
  isEdit = false; // Flag to check if form is used for edit
  editCustomerId: number | null = null; // Store the editing customer's ID

  constructor(
    private fb: FormBuilder, 
    private customerService: CustomerService
  ) { 
    this.customerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/)]], 
      phoneNumber: [''],
      password: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadCustomers();
  }

  onSubmit(): void {
    if (this.customerForm.valid) {
      if (this.isEdit && this.editCustomerId !== null) {
        // Update customer
        const updatedCustomer = { ...this.customerForm.value, id: this.editCustomerId };
        this.customerService.updateCustomer(updatedCustomer).subscribe({
          next: () => {
            console.log('Customer updated successfully');
            this.resetForm();
            this.loadCustomers();
          },
          error: (error) => console.error('Error updating customer:', error)
        });
      } else {
        // Add new customer
        this.customerService.addCustomer(this.customerForm.value).subscribe({
          next: (customer) => {
            console.log('Customer created successfully:', customer);
            this.resetForm();
            this.loadCustomers();
          },
          error: (error) => console.error('Error creating customer:', error)
        });
      }
    } else {
      console.error('Form is not valid');
    }
  }

  loadCustomers(): void {
    this.customerService.getAllCustomers().subscribe({
      next: (customers) => this.customers = customers,
      error: (error) => console.error('Failed to load customers', error)
    });
  }

  editCustomer(customerId: number): void {
    // Assume getCustomerById method exists in your service
    this.customerService.getCustomerById(customerId).subscribe({
      next: (customer) => {
        this.customerForm.patchValue(customer);
        this.isEdit = true;
        this.editCustomerId = customerId;
      },
      error: (error) => console.error('Error loading customer for edit', error)
    });
  }

  deleteCustomer(customerId: number): void {
    this.customerService.deleteCustomer(customerId).subscribe({
      next: () => {
        console.log(`Customer with ID ${customerId} deleted successfully`);
        this.loadCustomers();
      },
      error: (error) => {
        console.error('Failed to delete customer', error);
      }
    });
  }

  resetForm(): void {
    this.customerForm.reset();
    this.isEdit = false;
    this.editCustomerId = null;
  }
}
