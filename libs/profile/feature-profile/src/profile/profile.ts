import { Component, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { AuthFacade } from "@sports-ui/auth-data-access";
import { VoteAccountFacade } from "@sports-ui/vote-account-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
@Component({
  selector: "lib-profile",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./profile.html",
  styleUrls: ["./profile.css"],
})
export class Profile {
  private authFacade = inject(AuthFacade);
  private voteAccountFacade = inject(VoteAccountFacade);
  private organizationFacade = inject(OrganizationFeatureService);
  voteAccount = this.voteAccountFacade.account;
  transactions = this.voteAccountFacade.transactions;
  user = this.authFacade.user;

  ngOnInit() {
    const user = this.user();
    const org = this.organizationFacade.selectedOrganization();

    if (user && org) {
      this.voteAccountFacade.load(user.id, org.id);
    }
  }
}
