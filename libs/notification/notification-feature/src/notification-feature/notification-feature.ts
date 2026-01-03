import { Component, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { NotificationFacade } from "@sports-ui/notification-data-access";
import { NotificationComponent } from "@sports-ui/ui";
@Component({
  selector: "lib-notification-feature",
  imports: [CommonModule, NotificationComponent],
  templateUrl: "./notification-feature.html",
  styleUrl: "./notification-feature.css",
})
export class NotificationFeature {
  facade = inject(NotificationFacade);
  readonly notifications = this.facade.notifications;
  readonly status = this.facade.status;

  onMarkRead(id: string) {
    this.facade.markAsRead(id);
  }
}
