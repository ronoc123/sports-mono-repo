import { Component, input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Player } from "@sports-ui/api-types";

@Component({
  selector: "lib-player-card",
  imports: [CommonModule],
  standalone: true,
  templateUrl: "./player-card.html",
  styleUrl: "./player-card.css",
})
export class PlayerCardComponent {
  player = input.required<Player>();
}
