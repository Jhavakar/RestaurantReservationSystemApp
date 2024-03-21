import { Injectable } from '@angular/core';
import { of, Observable } from 'rxjs';
import { Table } from '../models/table.model';

@Injectable({
  providedIn: 'root'
})
export class TableService {
  private tables: Table[] = [
    // Mock tables
    { tableId: 1, tableNumber: '101', capacity: 4, isAvailable: true,}
  ];

  constructor() { }

  getTables(): Observable<Table[]> {
    return of(this.tables);
  }

  addTable(table: Table): Observable<Table> {
    const newTable = { ...table, tableId: this.tables.length + 1 };
    this.tables.push(newTable);
    return of(newTable);
  }

  // Implement update and delete methods as needed
}
