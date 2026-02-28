import { Component, computed, effect, inject } from "@angular/core";
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

  voteAccount  = this.voteAccountFacade.account;
  transactions = this.voteAccountFacade.transactions;
  user         = this.authFacade.user;
  organization = this.organizationFacade.selectedOrganization;

  initials = computed(() => {
    const u = this.user();
    if (!u) return "U";
    const name = u.fullName || u.email || "";
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return name.substring(0, 2).toUpperCase();
  });

  constructor() {
    effect(() => {
      const user = this.user();
      const org  = this.organization();
      if (!user || !org) return;
      this.voteAccountFacade.load(user.id, org.id);
    });
  }

  logout() {
    this.authFacade.logout();
  }

  getRefId(t: any): string | null {
    return t.refId || t.spendId || t.playerOptionId || null;
  }
}
