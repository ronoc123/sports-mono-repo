import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UiFeatureTheme } from './ui-feature-theme';

describe('UiFeatureTheme', () => {
  let component: UiFeatureTheme;
  let fixture: ComponentFixture<UiFeatureTheme>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiFeatureTheme],
    }).compileComponents();

    fixture = TestBed.createComponent(UiFeatureTheme);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
