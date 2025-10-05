import { Component, input, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatToolbarModule } from "@angular/material/toolbar";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatMenuModule } from "@angular/material/menu";
import { MatBadgeModule } from "@angular/material/badge";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatDividerModule } from "@angular/material/divider";
import { MatSidenav } from "@angular/material/sidenav";
import { Router } from "@angular/router";

import { LayoutConfig } from "./main-layout.component";

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: "ui-navbar",
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatBadgeModule,
    MatTooltipModule,
    MatDividerModule,
  ],
  templateUrl: "./navbar.component.html",
  styleUrl: "./navbar.component.css",
})
export class NavbarComponent {
  private readonly router = inject(Router);

  // Inputs
  readonly config = input.required<LayoutConfig>();
  readonly drawer = input.required<MatSidenav>();
  readonly currentUser = input<any>(null);

  // Navigation methods
  navigateToProfile() {
    this.router.navigate(["/profile"]);
  }

  navigateToSettings() {
    this.router.navigate(["/settings"]);
  }

  navigateToNotifications() {
    this.router.navigate(["/notifications"]);
  }

  logout() {
    // This should be handled by the parent component or auth service
    console.log("Logout requested");
  }

  // Toggle sidebar
  toggleSidebar() {
    this.drawer().toggle();
  }

  // Search functionality
  onSearch(query: string) {
    console.log("Search query:", query);
    // Implement search functionality
  }

  // Notification count (placeholder)
  getNotificationCount(): number {
    return 3; // Placeholder
  }
}
