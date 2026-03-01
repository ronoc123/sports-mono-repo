import { ComponentFixture, TestBed } from "@angular/core/testing";
import { UiMarketplace } from "./ui-marketplace";

describe("UiMarketplace", () => {
  let component: UiMarketplace;
  let fixture: ComponentFixture<UiMarketplace>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiMarketplace],
    }).compileComponents();

    fixture = TestBed.createComponent(UiMarketplace);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
