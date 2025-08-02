import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DefensiveLineup } from './defensive-lineup';

describe('DefensiveLineup', () => {
  let component: DefensiveLineup;
  let fixture: ComponentFixture<DefensiveLineup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DefensiveLineup],
    }).compileComponents();

    fixture = TestBed.createComponent(DefensiveLineup);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
