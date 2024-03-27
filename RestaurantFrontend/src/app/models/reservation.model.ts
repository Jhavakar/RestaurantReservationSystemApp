// reservation.model.ts
import { Customer } from '../models/customer.model';
import { Table } from '../models/table.model';

export interface Reservation {
  reservationTime: Date; 
  numberOfGuests: number;
  customerId?: number; 
  firstName?: string;
  lastName?: string;
  emailAddress?: string;
  phoneNo?: string;
}
