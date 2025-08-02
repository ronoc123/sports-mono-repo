import { ComponentFixture, TestBed } from "@angular/core/testing";
import { RedeemDataAccess } from "./redeem-data-access";

describe("RedeemDataAccess", () => {
  let component: RedeemDataAccess;
  let fixture: ComponentFixture<RedeemDataAccess>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RedeemDataAccess],
    }).compileComponents();

    fixture = TestBed.createComponent(RedeemDataAccess);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
