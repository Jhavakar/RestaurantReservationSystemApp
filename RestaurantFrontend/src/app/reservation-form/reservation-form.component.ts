import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  templateUrl: './reservation-form.component.html',
  styleUrls: ['./reservation-form.component.css']
})
export class ReservationFormComponent implements OnInit {
  reservationForm: FormGroup;
  timeSlots: string[] = [];

  constructor(private fb: FormBuilder) {
    this.reservationForm = this.fb.group({
      reservationDate: ['', [Validators.required]],
      numberOfGuests: ['', [Validators.required, Validators.min(1)]],
      selectedTimeSlot: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.generateTimeSlots();
  }

  generateTimeSlots() {
    // Assuming the restaurant opens at 12 PM and last reservation is at 11 PM
    const openingTime = 12; // 12 PM
    const closingTime = 23; // 11 PM

    for (let hour = openingTime; hour <= closingTime; hour++) {
      const time = hour > 12 ? `${hour - 12}:00 PM` : `${hour}:00 PM`;
      this.timeSlots.push(time);
    }
  }

  submit() {
    if (this.reservationForm.valid) {
      console.log(this.reservationForm.value);
      // Handle the form submission, e.g., send data to a backend
    }
  }
}
