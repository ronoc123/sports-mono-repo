import { ComponentFixture, TestBed } from "@angular/core/testing";
import { ProfileDataAccess } from "./profile-data-access";

describe("ProfileDataAccess", () => {
  let component: ProfileDataAccess;
  let fixture: ComponentFixture<ProfileDataAccess>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfileDataAccess],
    }).compileComponents();

    fixture = TestBed.createComponent(ProfileDataAccess);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
