import { ComponentFixture, TestBed } from "@angular/core/testing";
import { UiH2h } from "./ui-h2h";

describe("UiH2h", () => {
  let component: UiH2h;
  let fixture: ComponentFixture<UiH2h>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiH2h],
    }).compileComponents();

    fixture = TestBed.createComponent(UiH2h);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
