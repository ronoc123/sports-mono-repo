import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OffensiveLineup } from './offensive-lineup';

describe('OffensiveLineup', () => {
  let component: OffensiveLineup;
  let fixture: ComponentFixture<OffensiveLineup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OffensiveLineup],
    }).compileComponents();

    fixture = TestBed.createComponent(OffensiveLineup);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
