import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-terms-and-conditions-modal',
  standalone: true,
  imports: [],
  templateUrl: './terms-and-conditions-modal.component.html',
  styleUrls: ['./terms-and-conditions-modal.component.css']
})
export class TermsAndConditionsModalComponent {
  @Output() accepted = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  accept() {
    this.accepted.emit();
  }

  close() {
    this.closed.emit();
  }
}
