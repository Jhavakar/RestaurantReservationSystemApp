// reservation.model.ts
import { Customer } from '../models/customer.model';
import { Table } from '../models/table.model';

export interface Reservation {
  id: number;
  reservationDate: Date;
  reservationTime: Date;
  reservationEndTime: Date;
  title: string;
  tableId: number;
  total: number;
  customer: Customer;
  table: Table;
  customerId: number;
  isActive: boolean;
}
