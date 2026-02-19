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
    { name: "Player Options", icon: "how_to_vote", route: "player-option" },
    { name: "Active Raoster", icon: "people_alt", route: "active-roaster" },
    { name: "Redeem Codes", icon: "redeem", route: "redeem" },
    { name: "Profile", icon: "person", route: "profile" },
    {
      name: "Create Player Option",
      icon: "how_to_vote",
      route: "create-player-option",
    },
  ];
}
