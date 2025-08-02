import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Player } from '@sports-ui/api-types';
import { PlayerCardComponent} from '@sports-ui/ui';

@Component({
  selector: 'lib-offensive-lineup',
  imports: [CommonModule, PlayerCardComponent],
  templateUrl: './offensive-lineup.html',
  styleUrl: './offensive-lineup.css',
})
export class OffensiveLineup {
  players = input.required<Player[]>();

  getPlayer(position: string, index = 0): Player | undefined {
    return this.players().filter(p => p.position === position)[index];
  }
}
