import { ComponentFixture, TestBed } from "@angular/core/testing";
import { UiAlert } from "./ui-alert";

describe("UiAlert", () => {
  let component: UiAlert;
  let fixture: ComponentFixture<UiAlert>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiAlert],
    }).compileComponents();

    fixture = TestBed.createComponent(UiAlert);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
