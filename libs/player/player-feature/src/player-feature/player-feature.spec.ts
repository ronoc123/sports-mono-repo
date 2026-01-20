import { ComponentFixture, TestBed } from "@angular/core/testing";
import { PlayerFeature } from "./player-feature";

describe("PlayerFeature", () => {
  let component: PlayerFeature;
  let fixture: ComponentFixture<PlayerFeature>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerFeature],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerFeature);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
