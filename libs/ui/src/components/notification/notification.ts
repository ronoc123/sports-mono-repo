import { Component, input, output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { NotificationDto } from "@sports-ui/notification-data-access";

@Component({
  selector: "lib-notification",
  imports: [CommonModule],
  standalone: true,
  templateUrl: "./notification.html",
  styleUrls: ["./notification.css"],
})
export class NotificationComponent {
  notifications = input<NotificationDto>();
  loading = input<boolean>(false);

  // outputs can stay the same
  markRead = output<string>();
}
