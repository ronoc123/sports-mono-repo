import { ComponentFixture, TestBed } from "@angular/core/testing";
import { PlayerSearchFeature } from "./player-search-feature";

describe("PlayerSearchFeature", () => {
  let component: PlayerSearchFeature;
  let fixture: ComponentFixture<PlayerSearchFeature>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerSearchFeature],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerSearchFeature);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
