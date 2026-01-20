import { ComponentFixture, TestBed } from "@angular/core/testing";
import { PlayerDataAccess } from "./player-data-access";

describe("PlayerDataAccess", () => {
  let component: PlayerDataAccess;
  let fixture: ComponentFixture<PlayerDataAccess>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerDataAccess],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerDataAccess);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
