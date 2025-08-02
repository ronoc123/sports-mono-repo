import { ComponentFixture, TestBed } from "@angular/core/testing";
import { AccountStatus } from "./account-status";

describe("AccountStatus", () => {
  let component: AccountStatus;
  let fixture: ComponentFixture<AccountStatus>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountStatus],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountStatus);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
