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

  get initial(): string {
    const name = this.user().fullName || this.user().email;
    return name.charAt(0).toUpperCase();
  }
}
