import { ComponentFixture, TestBed } from "@angular/core/testing";
import { ToastFeature } from "./toast-feature";

describe("ToastFeature", () => {
  let component: ToastFeature;
  let fixture: ComponentFixture<ToastFeature>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToastFeature],
    }).compileComponents();

    fixture = TestBed.createComponent(ToastFeature);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
