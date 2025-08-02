import { Component, inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatChipsModule } from "@angular/material/chips";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { Router } from "@angular/router";
import { AuthService } from "@sports-ui/feature-auth";

interface DashboardCard {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: string;
  color: "primary" | "accent" | "warn" | "success" | "info";
  action?: {
    label: string;
    route: string;
  };
}

interface QuickAction {
  label: string;
  icon: string;
  route: string;
  color: "primary" | "accent" | "warn";
}

interface RecentActivity {
  title: string;
  description: string;
  timestamp: Date;
  type: "info" | "success" | "warning" | "error";
}

@Component({
  selector: "lib-feature-dashboard",
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressBarModule,
  ],
  templateUrl: "./feature-dashboard.html",
  styleUrl: "./feature-dashboard.css",
})
export class FeatureDashboard implements OnInit {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  currentUser = this.authService.currentUser;

  // Dashboard data
  statsCards: DashboardCard[] = [];
  quickActions: QuickAction[] = [];
  recentActivities: RecentActivity[] = [];

  ngOnInit() {
    this.loadDashboardData();
  }

  private loadDashboardData() {
    // Load stats cards
    this.statsCards = [
      {
        title: "Active Organizations",
        value: 3,
        subtitle: "2 new this month",
        icon: "business",
        color: "primary",
        action: {
          label: "View All",
          route: "/organizations",
        },
      },
      {
        title: "Total Users",
        value: 1247,
        subtitle: "+12% from last month",
        icon: "people",
        color: "success",
        action: {
          label: "Manage Users",
          route: "/users",
        },
      },
      {
        title: "Active Player Options",
        value: 8,
        subtitle: "3 ending soon",
        icon: "how_to_vote",
        color: "accent",
        action: {
          label: "View Options",
          route: "/player-options",
        },
      },
      {
        title: "System Health",
        value: "98.5%",
        subtitle: "All systems operational",
        icon: "health_and_safety",
        color: "success",
        action: {
          label: "View Details",
          route: "/system",
        },
      },
    ];

    // Load quick actions
    this.quickActions = [
      {
        label: "Create Player Option",
        icon: "add_circle",
        route: "/player-options/create",
        color: "primary",
      },
      {
        label: "Add New User",
        icon: "person_add",
        route: "/users/create",
        color: "accent",
      },
      {
        label: "View Reports",
        icon: "assessment",
        route: "/reports",
        color: "primary",
      },
      {
        label: "System Settings",
        icon: "settings",
        route: "/system/settings",
        color: "warn",
      },
    ];

    // Load recent activities
    this.recentActivities = [
      {
        title: "New user registered",
        description: "John Doe joined the platform",
        timestamp: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
        type: "success",
      },
      {
        title: "Player option created",
        description: "Trade deadline decision for Team Alpha",
        timestamp: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
        type: "info",
      },
      {
        title: "System maintenance",
        description: "Scheduled maintenance completed successfully",
        timestamp: new Date(Date.now() - 1000 * 60 * 60 * 4), // 4 hours ago
        type: "success",
      },
      {
        title: "High vote activity",
        description: "Unusual voting pattern detected",
        timestamp: new Date(Date.now() - 1000 * 60 * 60 * 6), // 6 hours ago
        type: "warning",
      },
    ];
  }

  navigateTo(route: string) {
    this.router.navigate([route]);
  }

  getActivityIcon(type: string): string {
    switch (type) {
      case "success":
        return "check_circle";
      case "warning":
        return "warning";
      case "error":
        return "error";
      default:
        return "info";
    }
  }

  getTimeAgo(timestamp: Date): string {
    const now = new Date();
    const diffMs = now.getTime() - timestamp.getTime();
    const diffMins = Math.floor(diffMs / (1000 * 60));
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    if (diffMins < 60) {
      return `${diffMins} minutes ago`;
    } else if (diffHours < 24) {
      return `${diffHours} hours ago`;
    } else {
      return `${diffDays} days ago`;
    }
  }
}
