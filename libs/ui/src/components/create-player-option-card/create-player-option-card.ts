import { Component, input, output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { PlayerDto } from "@sports-ui/player-data-access";

@Component({
  selector: "lib-create-player-option-card",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./create-player-option-card.html",
  styleUrl: "./create-player-option-card.css",
})
export class CreatePlayerOptionCardComponent {
  player = input.required<PlayerDto>();
  selected = output<PlayerDto>();

  onClick() {
    this.selected.emit(this.player());
  }
}
