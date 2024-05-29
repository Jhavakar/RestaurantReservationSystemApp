import { CustomerModel } from './customer.model';
export interface ReservationModel {
  user: CustomerModel;
  reservationId?: number;
  reservationTime: Date;
  numberOfGuests: number;
}
