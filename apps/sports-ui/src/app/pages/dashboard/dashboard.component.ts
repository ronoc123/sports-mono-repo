import { Component, inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatGridListModule } from "@angular/material/grid-list";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { Router } from "@angular/router";
import { UserStore, OrganizationStore } from "@sports-ui/data-access";
import { PlayerOptionStore } from "@sports-ui/player-options-data-access";
import {
  Organization,
  PlayerOption,
  PlayerOptionFilter,
} from "@sports-ui/api-types";

@Component({
  selector: "app-dashboard",
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatGridListModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: "./dashboard.component.html",
  styleUrl: "./dashboard.component.css",
})
export class DashboardComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly userStore = inject(UserStore);
  private readonly organizationStore = inject(OrganizationStore);
  private readonly playerOptionStore = inject(PlayerOptionStore);

  // Store signals
  currentUser = this.userStore.currentUser;
  organizations = this.organizationStore.organizations;
  playerOptions = this.playerOptionStore.playerOptions;

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    // Load user data
    this.userStore.loadCurrentUser();

    // Load organizations
    this.organizationStore.loadOrganizations({
      pageNumber: 1,
      pageSize: 10,
      sortBy: "name",
      sortDescending: false,
    });

    // Load recent player options
    this.playerOptionStore.loadPlayerOptions({
      pageNumber: 1,
      pageSize: 5,
      sortBy: "createdAt",
      sortDescending: true,
    });
  }

  navigateToPlayerOptions() {
    this.router.navigate(["/player-option"]);
  }

  navigateToActiveRoster() {
    this.router.navigate(["/active-roaster"]);
  }

  navigateToRedeem() {
    this.router.navigate(["/redeem"]);
  }

  navigateToOrganizations() {
    this.router.navigate(["/organization"]);
  }

  getUserGreeting(): string {
    const user = this.currentUser();
    if (user) {
      const hour = new Date().getHours();
      let greeting = "Hello";

      if (hour < 12) {
        greeting = "Good morning";
      } else if (hour < 18) {
        greeting = "Good afternoon";
      } else {
        greeting = "Good evening";
      }

      return `${greeting}, ${user.firstName || user.userName || "User"}!`;
    }
    return "Welcome to Sports UI!";
  }

  getActiveOrganizationsCount(): number {
    return this.organizations().filter((org: Organization) => !org.isLocked)
      .length;
  }

  getActivePlayerOptionsCount(): number {
    return this.playerOptions().filter(
      (option: PlayerOption) => option.isActive && !option.isExpired
    ).length;
  }
}
