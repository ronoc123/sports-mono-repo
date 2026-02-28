import { Component, input, output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { SendVotesUserInfo } from "@sports-ui/send-votes-data-access";

@Component({
  selector: "lib-user-card",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./user-card.html",
  styleUrl: "./user-card.css",
})
export class UserCardComponent {
  user = input.required<SendVotesUserInfo>();
  selected = output<SendVotesUserInfo>();

  onClick() {
    this.selected.emit(this.user());
  }

  get initials(): string {
    const u = this.user();
    const name = u.fullName || `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || u.email || '';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return name.substring(0, 2).toUpperCase();
  }
}
