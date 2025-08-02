import { ComponentFixture, TestBed } from "@angular/core/testing";
import { QuickAction } from "./quick-action";

describe("QuickAction", () => {
  let component: QuickAction;
  let fixture: ComponentFixture<QuickAction>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuickAction],
    }).compileComponents();

    fixture = TestBed.createComponent(QuickAction);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
