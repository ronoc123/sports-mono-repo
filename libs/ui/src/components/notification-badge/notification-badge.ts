import { Component, input, output, signal } from "@angular/core";
import { CommonModule } from "@angular/common";

import { NotificationDto } from "@sports-ui/notification-data-access";
import { NotificationComponent } from "../notification/notification";

@Component({
  selector: "lib-notification-badge",
  imports: [CommonModule, NotificationComponent],
  standalone: true,
  templateUrl: "./notification-badge.html",
  styleUrls: ["./notification-badge.css"],
})
export class NotificationBadgeComponent {
  // ----------- INPUTS -----------
  notifications = input<NotificationDto[]>([]);
  // ----------- OUTPUTS -----------
  markRead = output<string>();

  dropdownOpen = signal(false);

  toggleDropdown() {
    this.dropdownOpen.update((v) => !v);
  }

  unreadCount() {
    return this.notifications().filter((n) => !n.isRead).length;
  }
}
