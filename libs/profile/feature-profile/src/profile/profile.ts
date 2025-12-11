import { Component, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { AuthFacade } from "@sports-ui/auth-data-access";

@Component({
  selector: "lib-profile",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./profile.html",
  styleUrls: ["./profile.css"],
})
export class Profile {
  private authFacade = inject(AuthFacade);

  user = this.authFacade.user; // <-- signal with user info
  voteAccount = {
    balance: 250,
    version: 3,
    createdAt: "2025-01-10T10:00:00Z",
    updatedAt: "2025-12-08T12:30:00Z",
    transactions: [
      {
        id: 1001,
        amount: 50,
        reason: "Voted: Trade Proposal",
        createdAt: "2025-12-07T14:20:00Z",
        refId: "TRD-553",
        playerOptionId: "d9f03a02-e6b9-4fc5-b756-828f4c52de07",
      },
      {
        id: 1002,
        amount: -25,
        reason: "Redeemed Reward",
        createdAt: "2025-12-06T09:10:00Z",
        spendId: "RWD-884",
      },
    ],
  };
}
