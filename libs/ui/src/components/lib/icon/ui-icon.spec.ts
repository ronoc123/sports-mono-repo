import { ComponentFixture, TestBed } from "@angular/core/testing";
import { UiIcon } from "./ui-icon";

describe("UiIcon", () => {
  let component: UiIcon;
  let fixture: ComponentFixture<UiIcon>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiIcon],
    }).compileComponents();

    fixture = TestBed.createComponent(UiIcon);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
