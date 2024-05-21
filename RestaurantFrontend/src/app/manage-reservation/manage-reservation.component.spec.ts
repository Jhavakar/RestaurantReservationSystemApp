import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageReservationComponent } from './manage-reservation.component';

describe('ManageReservationComponent', () => {
  let component: ManageReservationComponent;
  let fixture: ComponentFixture<ManageReservationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageReservationComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ManageReservationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
