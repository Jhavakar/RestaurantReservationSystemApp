export interface ReservationModel {
  user: any;
  reservationId: number;
  reservationTime: Date; 
  numberOfGuests: number;
  customerId?: number; 
  firstName: string;
  lastName: string;
  emailAddress: string;
  phoneNumber?: string;
}
