import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { CustomerService } from '../services/customer.service';
import { CustomerModel } from '../models/customer.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-customer-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], // Include ReactiveFormsModule here
  templateUrl: './customer-details.component.html',
  styleUrls: ['./customer-details.component.css']
})

export class CustomerDetailsComponent implements OnInit {
  customer: CustomerModel | null = null;
  editCustomerForm: FormGroup;
  passwordForm: FormGroup;
  editingState: boolean = false;

  constructor(
    private authService: AuthService,
    private customerService: CustomerService,
    private fb: FormBuilder,
    private router: Router

  ) {
    this.editCustomerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      password: ['', Validators.required],
    });
    
    this.passwordForm = this.fb.group({
      password: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.authService.getCurrentUserEmail().subscribe({
      next: (email) => {
        console.log('Email fetched:', email);
        if (email) {
          this.loadCustomerByEmail(email);
        } else {
          console.error('No email found for current user.');
        }
      },
      error: (err) => console.error('Error retrieving user email:', err)
    });
  }

  loadCustomerByEmail(email: string): void {
    this.customerService.getCustomerByEmail(email).subscribe({
      next: (customer) => {
        if (customer && customer.id) {
          this.customer = customer;
          this.editCustomerForm.patchValue(customer);
        } else {
          console.error('Customer not found or ID is missing');
        }
      },
      error: (error) => console.error('Failed to load customer', error)
    });
  }

  updateCustomer(): void {
    if (this.editCustomerForm.valid && this.passwordForm.valid) {
      const updatedCustomer: CustomerModel = {
        ...this.customer,
        ...this.editCustomerForm.value
      };
      const password = this.passwordForm.value.password;

      console.log('Updating customer:', updatedCustomer);

      if (updatedCustomer.email) {
        this.customerService.updateCustomer(updatedCustomer).subscribe({
          next: () => {
            console.log('Customer updated successfully');
            this.customer = updatedCustomer;
            this.editingState = false;
            this.notifyParent();
          },
          error: (error) => console.error('Error updating customer', error)
        });
      } else {
        console.error('Customer email is missing');
      }
    } else {
      console.error('Form is not valid');
    }
  }

  editCustomer(): void {
    this.editingState = true;
  }

  cancelEdit(): void {
    this.editingState = false;
    if (this.customer) {
      this.editCustomerForm.patchValue(this.customer);
    }
  }

  navigateToForgotPassword(): void {
    this.router.navigate(['/password-reset']);
  }

  notifyParent(): void {
    const event = new CustomEvent('customerUpdated');
    window.dispatchEvent(event);
  }
}
