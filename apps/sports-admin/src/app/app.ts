import { Component, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MainLayoutComponent, NavItem } from "@sports-ui/ui";

@Component({
  imports: [CommonModule, RouterModule, MainLayoutComponent],
  selector: "app-root",
  template: `
    <ui-main-layout
      [navItems]="navItems"
      [config]="layoutConfig"
      [permissionChecker]="checkPermission"
      [currentUser]="currentUser"
      [organizations]="organizations"
      [selectedOrganization]="selectedOrganization"
    ></ui-main-layout>
  `,
  styles: [
    `
      :host {
        display: block;
        height: 100vh;
      }
    `,
  ],
})
export class App {
  protected title = "sport-admin";

  // Mock data for now
  currentUser = null;
  organizations: any[] = [];
  selectedOrganization = null;

  // Layout configuration for Admin Portal
  layoutConfig = {
    appTitle: "Admin Portal",
    appLogo: "/assets/admin-logo.png",
    showUserMenu: true,
    showNotifications: true,
    showSearch: true,
    sidenavMode: "side" as const,
    sidenavOpened: true,
    showFooter: true,
  };

  // Permission checker function (simplified for now)
  checkPermission = (item: NavItem): boolean => {
    return true; // Allow all for now
  };

  // Admin-specific navigation items
  navItems: NavItem[] = [
    { name: "Dashboard", icon: "dashboard", route: "/" },
    {
      name: "User Management",
      icon: "people",
      children: [
        { name: "All Users", icon: "group", route: "/users" },
        { name: "Roles & Permissions", icon: "security", route: "/roles" },
        { name: "User Activity", icon: "history", route: "/user-activity" },
      ],
    },
    {
      name: "Organization Management",
      icon: "business",
      children: [
        {
          name: "All Organizations",
          icon: "business",
          route: "/organizations",
        },
        {
          name: "Organization Settings",
          icon: "settings",
          route: "/org-settings",
        },
        { name: "League Management", icon: "sports", route: "/leagues" },
      ],
    },
    {
      name: "Content Management",
      icon: "content_copy",
      children: [
        {
          name: "Player Options",
          icon: "how_to_vote",
          route: "/player-options",
        },
        { name: "Players", icon: "person", route: "/players" },
        { name: "Themes", icon: "palette", route: "/themes" },
        { name: "Codes", icon: "confirmation_number", route: "/codes" },
      ],
    },
    {
      name: "System",
      icon: "settings",
      children: [
        { name: "System Settings", icon: "tune", route: "/system-settings" },
        { name: "Audit Logs", icon: "assignment", route: "/audit-logs" },
        { name: "Analytics", icon: "analytics", route: "/analytics" },
        { name: "Reports", icon: "assessment", route: "/reports" },
      ],
    },
    { name: "Profile", icon: "person", route: "/profile" },
    { name: "Settings", icon: "settings", route: "/settings" },
  ];
}
