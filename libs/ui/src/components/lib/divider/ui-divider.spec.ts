import { ComponentFixture, TestBed } from "@angular/core/testing";
import { UiDivider } from "./ui-divider";

describe("UiDivider", () => {
  let component: UiDivider;
  let fixture: ComponentFixture<UiDivider>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiDivider],
    }).compileComponents();

    fixture = TestBed.createComponent(UiDivider);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
