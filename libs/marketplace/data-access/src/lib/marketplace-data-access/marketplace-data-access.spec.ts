import { ComponentFixture, TestBed } from "@angular/core/testing";
import { MarketplaceDataAccess } from "./marketplace-data-access";

describe("MarketplaceDataAccess", () => {
  let component: MarketplaceDataAccess;
  let fixture: ComponentFixture<MarketplaceDataAccess>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MarketplaceDataAccess],
    }).compileComponents();

    fixture = TestBed.createComponent(MarketplaceDataAccess);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
