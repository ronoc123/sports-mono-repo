import { Component, input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RosterPlayer } from "@sports-ui/roaster-data-access";

@Component({
  selector: "lib-player-card",
  imports: [CommonModule],
  standalone: true,
  templateUrl: "./player-card.html",
  styleUrl: "./player-card.css",
})
export class PlayerCardComponent {
  player = input.required<RosterPlayer>();
}
