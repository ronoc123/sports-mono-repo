import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FeatureH2h } from "./feature-h2h";

describe("FeatureH2h", () => {
  let component: FeatureH2h;
  let fixture: ComponentFixture<FeatureH2h>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureH2h],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureH2h);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
