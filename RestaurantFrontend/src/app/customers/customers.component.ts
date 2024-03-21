import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomerService } from '../services/customer.service';
import { Customer } from '../models/customer.model';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-customers',
  templateUrl: './customers.component.html',
  imports: [HttpClientModule, ReactiveFormsModule],
  standalone: true,
  providers: [CustomerService]
})
export class CustomersComponent implements OnInit {
  customers: Customer[] = [];
  customerForm: FormGroup;

  constructor(private customerService: CustomerService, private fb: FormBuilder) {
    this.customerForm = this.fb.group({
      // Assuming you're not using customerId for creating new customers
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      emailAddress: ['', [Validators.required, Validators.email]],
      phoneNo: [''],
    });
  }

  ngOnInit(): void {
  }

  // This method is adjusted to only handle adding new customers
  onSubmit(): void {
    if (this.customerForm.valid) {
      const customer: Customer = this.customerForm.value;
      this.customerService.register(customer).subscribe({
        next: (newCustomer) => {
          console.log('Customer added:', newCustomer);
          // If you wish to reload customers from the server
          // this.loadCustomers();
        },
        error: (error) => {
          console.error('Error adding customer:', error);
        }
      });
      this.customerForm.reset();
    }
  }
}
