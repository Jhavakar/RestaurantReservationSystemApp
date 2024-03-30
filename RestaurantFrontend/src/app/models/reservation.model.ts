// reservation.model.ts
import { CustomerModel } from '../models/customer.model';
import { TableModel } from '../models/table.model';

export interface ReservationModel {
  reservationTime: Date; 
  numberOfGuests: number;
  customerId?: number; 
  firstName?: string;
  lastName?: string;
  emailAddress?: string;
  phoneNo?: string;
}
