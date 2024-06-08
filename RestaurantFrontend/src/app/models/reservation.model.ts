import { CustomerModel } from './customer.model';

export interface ReservationModel {
  user: CustomerModel;
  reservationId?: number;
  reservationDateTime: Date;
  numberOfGuests: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  reservationDate: string; 
  reservationTime: string;
  specialRequests: string;
  isNewAccount?: boolean;
}
