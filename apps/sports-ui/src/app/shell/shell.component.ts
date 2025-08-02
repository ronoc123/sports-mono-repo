import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MainLayoutComponent, NavItem } from '@sports-ui/ui';
import { AuthService } from '@sports-ui/feature-auth';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MainLayoutComponent,
  ],
  template: `
    <ui-main-layout 
      [navItems]="navItems"
      [config]="layoutConfig"
      [permissionChecker]="checkPermission"
      [currentUser]="authService.currentUser()"
      [organizations]="organizations"
      [selectedOrganization]="selectedOrganization"
    ></ui-main-layout>
  `,
  styles: [`
    :host {
      display: block;
      height: 100vh;
    }
  `],
})
export class ShellComponent {
  readonly authService = inject(AuthService);

  // Mock data for now
  organizations: any[] = [];
  selectedOrganization = null;

  // Layout configuration for Sports UI
  layoutConfig = {
    appTitle: 'Sports UI',
    appLogo: '/assets/sports-logo.png',
    showUserMenu: true,
    showNotifications: true,
    showSearch: false,
    sidenavMode: 'side' as const,
    sidenavOpened: true,
    showFooter: false,
  };

  // Permission checker function (simplified for now)
  checkPermission = (item: NavItem): boolean => {
    return true; // Allow all for now
  };

  // Sports UI navigation items with role-based permissions
  navItems: NavItem[] = [
    { name: "Dashboard", icon: "dashboard", route: "/" },
    { name: "Player Options", icon: "how_to_vote", route: "/player-option" },
    { name: "Active Roster", icon: "people_alt", route: "/active-roaster" },
    { name: "Redeem Codes", icon: "redeem", route: "/redeem" },
    {
      name: "Management",
      icon: "admin_panel_settings",
      children: [
        {
          name: "Player Options Management",
          icon: "sports",
          route: "/player-options-management",
        },
        {
          name: "Organizations",
          icon: "business",
          route: "/organization",
        },
        {
          name: "Players",
          icon: "people",
          route: "/players",
        },
        {
          name: "Leagues",
          icon: "sports",
          route: "/leagues",
        },
        {
          name: "Users",
          icon: "group",
          route: "/users",
        },
        {
          name: "Codes",
          icon: "confirmation_number",
          route: "/codes",
        },
        {
          name: "Themes",
          icon: "palette",
          route: "/themes",
        },
      ],
    },
    { name: "Profile", icon: "person", route: "/profile" },
    { name: "Settings", icon: "settings", route: "/settings" },
  ];
}
