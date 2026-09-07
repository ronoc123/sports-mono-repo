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

  navItems: NavItem[] = [
    { name: "Dashboard", icon: "dashboard", route: "dashboard" },
    { name: "collection", icon: "dashboard", route: "collection" },
    { name: "h2h", icon: "dashboard", route: "h2h" },
    {
      name: "Franchise",
      icon: "sports_football",
      children: [
        { name: "Player Options", icon: "how_to_vote", route: "player-option" },
        { name: "Active Roster", icon: "people_alt", route: "active-roaster" },
      ],
    },

    {
      name: "Store & Marketplace",
      icon: "storefront",
      children: [
        { name: "Card Packs", icon: "style", route: "card-packs" },
        { name: "Marketplace", icon: "store", route: "marketplace" },
        { name: "Redeem Codes", icon: "redeem", route: "redeem" },
      ],
    },

    {
      name: "Operations",
      icon: "settings_suggest",
      children: [
        { name: "Send Votes", icon: "send", route: "send-votes" },
        {
          name: "Create Player Option",
          icon: "add_circle",
          route: "create-player-option",
        },
        {
          name: "Trivia Management",
          icon: "quiz",
          route: "admin/trivia-management",
        },
        {
          name: "Poll Management",
          icon: "poll",
          route: "admin/poll-management",
        },
      ],
    },

    {
      name: "Administration",
      icon: "admin_panel_settings",
      children: [
        { name: "Economy Admin", icon: "tune", route: "admin/economy" },
        { name: "Audit Log", icon: "receipt_long", route: "admin/audit-log" },
      ],
    },

    {
      name: "Social Media",
      icon: "video_library",
      children: [
        { name: "Channels", icon: "subscriptions", route: "social-media/channels" },
      ],
    },
  ];
}
