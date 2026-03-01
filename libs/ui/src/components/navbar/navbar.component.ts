import { Component, input, output, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { LayoutConfig } from "../sidebar/sidebar.component";
import { NotificationDto } from "@sports-ui/notification-data-access";
import { NotificationBadgeComponent } from "../notification-badge/notification-badge";

@Component({
  selector: "lib-ui-navbar",
  imports: [CommonModule, NotificationBadgeComponent],
  standalone: true,
  templateUrl: "./navbar.component.html",
  styleUrls: ["./navbar.component.css"],
})
export class NavbarComponent {
  readonly config        = input<LayoutConfig>();
  readonly user          = input<any>();
  readonly notifications = input<NotificationDto[]>([]);

  readonly toggleNavBar  = output<void>();
  readonly logout        = output<void>();
  readonly openProfile   = output<void>();
  readonly openSettings  = output<void>();
  readonly toggleTheme   = output<void>();
  readonly markRead      = output<string>();
  readonly buyVotes      = output<void>();

  userMenuOpen = signal(false);

  toggleUserMenu() {
    this.userMenuOpen.update((v) => !v);
  }

  closeUserMenu() {
    this.userMenuOpen.set(false);
  }

  get userInitial(): string {
    const u = this.user();
    return (u?.firstName || u?.email || "U").substring(0, 1).toUpperCase();
  }
}
