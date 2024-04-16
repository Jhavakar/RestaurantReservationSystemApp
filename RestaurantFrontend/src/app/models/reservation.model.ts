export interface ReservationModel {
  reservationTime: Date; 
  numberOfGuests: number;
  customerId?: number; 
  firstName: string;
  lastName: string;
  emailAddress: string;
  phoneNo?: string;
}
