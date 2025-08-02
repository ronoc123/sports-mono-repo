import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import {PlayerOption} from '@sports-ui/api-types';

@Component({
  selector: 'lib-player-option-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './player-option-card.html',
  styleUrl: './player-option-card.css',
})

export class PlayerOptionCardComponent {

  option = input.required<PlayerOption>();
  selected = output<PlayerOption>();

  onClick() {
    // this.selected.emit(this.option());
  }
}
