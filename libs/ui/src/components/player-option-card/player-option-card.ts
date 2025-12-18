import { Component, input, output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { PlayerOptionDto } from "@sports-ui/player-options-data-access";

@Component({
  selector: "lib-player-option-card",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./player-option-card.html",
  styleUrl: "./player-option-card.css",
})
export class PlayerOptionCardComponent {
  option = input.required<PlayerOptionDto>();
  selected = output<PlayerOptionDto>();

  onClick() {
    this.selected.emit(this.option());
  }
}
