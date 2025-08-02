import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "lib-feature-dashboard",
  imports: [CommonModule],
  templateUrl: "./feature-dashboard.html",
  styleUrl: "./feature-dashboard.css",
})
export class FeatureDashboard {
  testCards = [
    { type: "account-status", label: "Account Status" },
    { type: "org-news", label: "Org News" },
    { type: "quick-actions", label: "Quick Actions" },
    { type: "vote-balance", label: "Vote Balance" },
    { type: "coming-soon", label: "Coming Soon..." },
  ];

  getCardClass(type: string): string {
    switch (type) {
      case "account-status":
        return "card-full";
      case "coming-soon":
        return "card-full";
      default:
        return "card-normal";
    }
  }
}
