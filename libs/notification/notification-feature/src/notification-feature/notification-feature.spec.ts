import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NotificationFeature } from "./notification-feature";

describe("NotificationFeature", () => {
  let component: NotificationFeature;
  let fixture: ComponentFixture<NotificationFeature>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotificationFeature],
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationFeature);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
