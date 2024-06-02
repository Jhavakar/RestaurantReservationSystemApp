import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-reservation-success-modal',
  standalone: true,
  imports: [],
  templateUrl: './reservation-success-modal.component.html',
  styleUrl: './reservation-success-modal.component.css'
})
export class ReservationSuccessModalComponent {
  @Output() closeModal = new EventEmitter<void>();

  close(): void {
    this.closeModal.emit();
  }
}
