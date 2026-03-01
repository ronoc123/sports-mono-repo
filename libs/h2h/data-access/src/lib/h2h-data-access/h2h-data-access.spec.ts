import { ComponentFixture, TestBed } from "@angular/core/testing";
import { H2hDataAccess } from "./h2h-data-access";

describe("H2hDataAccess", () => {
  let component: H2hDataAccess;
  let fixture: ComponentFixture<H2hDataAccess>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [H2hDataAccess],
    }).compileComponents();

    fixture = TestBed.createComponent(H2hDataAccess);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
