import { ComponentFixture, TestBed } from "@angular/core/testing";
import { VoteBalance } from "./vote-balance";

describe("VoteBalance", () => {
  let component: VoteBalance;
  let fixture: ComponentFixture<VoteBalance>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VoteBalance],
    }).compileComponents();

    fixture = TestBed.createComponent(VoteBalance);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
