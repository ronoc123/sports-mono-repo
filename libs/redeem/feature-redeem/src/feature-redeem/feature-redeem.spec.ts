import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FeatureRedeem } from "./feature-redeem";

describe("FeatureRedeem", () => {
  let component: FeatureRedeem;
  let fixture: ComponentFixture<FeatureRedeem>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureRedeem],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureRedeem);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
