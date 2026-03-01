import { Component, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RedeemFacade } from "@sports-ui/redeem-data-access";
import { AuthFacade } from "@sports-ui/auth-data-access";
import { VoteAccountFacade } from "@sports-ui/vote-account-data-access";
import { CodeRedemptionComponent } from "@sports-ui/ui";

@Component({
  selector: "lib-feature-redeem",
  imports: [CommonModule, CodeRedemptionComponent],
  templateUrl: "./feature-redeem.html",
  styleUrl: "./feature-redeem.css",
})
export class FeatureRedeem {
  redeemFacade = inject(RedeemFacade);
  authFacade = inject(AuthFacade);
  voteAccountFacade = inject(VoteAccountFacade);

  loading = this.redeemFacade.loading;
  error = this.redeemFacade.error;
  balance = this.voteAccountFacade.balance;

  onRedeemCode(code: string) {
    const userId = this.authFacade.user()?.id;
    if (!userId) return;
    this.redeemFacade.redeemReward(userId, code);
  }
}
