import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppCardSelectionComponent } from './app-card-selection.component';

describe('AppCardSelectionComponent', () => {
  let component: AppCardSelectionComponent;
  let fixture: ComponentFixture<AppCardSelectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AppCardSelectionComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppCardSelectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
