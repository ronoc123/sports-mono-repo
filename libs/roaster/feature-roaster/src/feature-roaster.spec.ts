import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FeatureRoaster } from './feature-roaster.component';

describe('FeatureRoaster', () => {
  let component: FeatureRoaster;
  let fixture: ComponentFixture<FeatureRoaster>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureRoaster],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureRoaster);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
