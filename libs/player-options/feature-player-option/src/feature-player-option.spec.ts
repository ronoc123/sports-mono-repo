import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FeaturePlayerOption } from './feature-player-option';

describe('FeaturePlayerOption', () => {
  let component: FeaturePlayerOption;
  let fixture: ComponentFixture<FeaturePlayerOption>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeaturePlayerOption],
    }).compileComponents();

    fixture = TestBed.createComponent(FeaturePlayerOption);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
