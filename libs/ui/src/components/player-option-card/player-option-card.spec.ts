import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PlayerOptionCardComponent } from './player-option-card';

describe('PlayerOptionCardComponent', () => {
  let component: PlayerOptionCardComponent;
  let fixture: ComponentFixture<PlayerOptionCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerOptionCardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerOptionCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
