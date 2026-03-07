import { Component, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { NavItem } from "@sports-ui/ui";
// eslint-disable-next-line @nx/enforce-module-boundaries
import { AuthService } from "@sports-ui/auth-data-access";
import { LayoutFeatureComponent } from "@sports-ui/feature-layout";

@Component({
  selector: "app-shell",
  standalone: true,
  imports: [CommonModule, RouterModule, LayoutFeatureComponent],
  template: ` <lib-feature-layout [navItems]="navItems"></lib-feature-layout> `,
})
export class ShellComponent {
  readonly authService = inject(AuthService);

  // Sports UI navigation items with role-based permissions
  navItems: NavItem[] = [
    { name: "Dashboard", icon: "dashboard", route: "dashboard" },
    { name: "Card Packs", icon: "style", route: "card-packs" },
    { name: "My Collection", icon: "collections_bookmark", route: "collection" },
    { name: "Marketplace", icon: "storefront", route: "marketplace" },
    { name: "H2H", icon: "sports_score", route: "h2h" },
    { name: "Player Options", icon: "how_to_vote", route: "player-option" },
    { name: "Active Raoster", icon: "people_alt", route: "active-roaster" },
    { name: "Redeem Codes", icon: "redeem", route: "redeem" },
    { name: "Send Votes", icon: "send", route: "send-votes" },
    { name: "Profile", icon: "person", route: "profile" },
    { name: "Economy Admin", icon: "tune", route: "admin/economy" },
    { name: "Audit Log", icon: "receipt_long", route: "admin/audit-log" },
    { name: "Trivia", icon: "quiz", route: "admin/trivia-management" },
    { name: "Polls", icon: "poll", route: "admin/poll-management" },
    {
      name: "Create Player Option",
      icon: "how_to_vote",
      route: "create-player-option",
    },
  ];
}
