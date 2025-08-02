import { Component, inject, input } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "lib-dashboard-card",
  imports: [CommonModule],
  templateUrl: "./dashboard-card.html",
  styleUrl: "./dashboard-card.css",
})
export class DashboardCard {
  readonly title = input.required<string>();
}
