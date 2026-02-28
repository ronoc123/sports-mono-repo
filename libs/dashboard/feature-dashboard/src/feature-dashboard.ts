import { Component, computed, inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Router } from "@angular/router";
import { AuthStore } from "@sports-ui/auth-data-access";

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
  imports: [CommonModule],
  templateUrl: "./feature-dashboard.html",
  styleUrl: "./feature-dashboard.css",
})
export class FeatureDashboard implements OnInit {
  private readonly router = inject(Router);
  private readonly authStore = inject(AuthStore);

  // Dashboard data
  statsCards: DashboardCard[] = [];
  quickActions: QuickAction[] = [];
  recentActivities: RecentActivity[] = [];
  currentUser = computed(() => this.authStore.user());
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
        route: "/player-option",
        color: "primary",
      },
      {
        label: "Profile Management",
        icon: "person",
        route: "/profile",
        color: "accent",
      },
      {
        label: "Roaster",
        icon: "assessment",
        route: "/active-roaster",
        color: "primary",
      },
      {
        label: "System Settings",
        icon: "settings",
        route: "/system/settings",
        color: "warn",
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
