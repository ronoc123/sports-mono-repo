import { ComponentFixture, TestBed } from "@angular/core/testing";
import { LeagueFeature } from "./league-feature";

describe("LeagueFeature", () => {
  let component: LeagueFeature;
  let fixture: ComponentFixture<LeagueFeature>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LeagueFeature],
    }).compileComponents();

    fixture = TestBed.createComponent(LeagueFeature);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
