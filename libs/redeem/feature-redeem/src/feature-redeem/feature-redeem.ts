import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { inject } from "@angular/core";
import { RedeemFacade } from "@sports-ui/redeem-data-access";
import { AuthFacade } from "@sports-ui/auth-data-access";

@Component({
  selector: "lib-feature-redeem",
  imports: [CommonModule],
  templateUrl: "./feature-redeem.html",
  styleUrl: "./feature-redeem.css",
})
export class FeatureRedeem {
  redeemFacade = inject(RedeemFacade);
  authFacade = inject(AuthFacade);

  redeemReward() {
    this.redeemFacade.redeemReward("user123", "reward456");

    console.log("Redeem Reward Clicked");
  }
}
