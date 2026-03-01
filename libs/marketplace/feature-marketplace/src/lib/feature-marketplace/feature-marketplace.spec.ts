import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FeatureMarketplace } from "./feature-marketplace";

describe("FeatureMarketplace", () => {
  let component: FeatureMarketplace;
  let fixture: ComponentFixture<FeatureMarketplace>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureMarketplace],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureMarketplace);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
