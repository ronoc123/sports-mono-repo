import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FeatureDashboard } from "./feature-dashboard";

describe("FeatureDashboard", () => {
  let component: FeatureDashboard;
  let fixture: ComponentFixture<FeatureDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureDashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
