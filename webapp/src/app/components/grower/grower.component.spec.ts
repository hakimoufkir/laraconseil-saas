import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GrowerComponent } from './grower.component';

describe('GrowerComponent', () => {
  let component: GrowerComponent;
  let fixture: ComponentFixture<GrowerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [GrowerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GrowerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
