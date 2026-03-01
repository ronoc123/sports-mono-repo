import { Component, input, output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { PlayerOptionDto } from "@sports-ui/player-options-data-access";
import { FormsModule } from "@angular/forms";

@Component({
  selector: "lib-player-option-card",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./player-option-card.html",
  styleUrl: "./player-option-card.css",
})
export class PlayerOptionCardComponent {
  option = input.required<PlayerOptionDto>();
  vote = output<{ option: PlayerOptionDto; amount: number }>();

  voteAmount = 1;

  increment() {
    this.voteAmount++;
  }

  decrement() {
    if (this.voteAmount > 1) {
      this.voteAmount--;
    }
  }

  onAmountChange() {
    const v = Math.floor(Number(this.voteAmount));
    this.voteAmount = isNaN(v) || v < 1 ? 1 : v;
  }

  applyVote() {
    this.vote.emit({
      option: this.option(),
      amount: this.voteAmount,
    });
  }
}
