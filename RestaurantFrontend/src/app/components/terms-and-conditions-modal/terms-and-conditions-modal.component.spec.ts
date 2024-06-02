import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TermsAndConditionsModalComponent } from './terms-and-conditions-modal.component';

describe('TermsAndConditionsModalComponent', () => {
  let component: TermsAndConditionsModalComponent;
  let fixture: ComponentFixture<TermsAndConditionsModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TermsAndConditionsModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TermsAndConditionsModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
