import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Player } from '@sports-ui/api-types';
import { PlayerCardComponent } from '@sports-ui/ui';

@Component({
  selector: 'lib-defensive-lineup',
  imports: [CommonModule, PlayerCardComponent],
  templateUrl: './defensive-lineup.html',
  styleUrl: './defensive-lineup.css',
})
export class DefensiveLineup {
  players = input.required<Player[]>();

  getPlayer(position: string, index = 0): Player | undefined {
    return this.players().filter(p => p.position === position)[index];
  }
}
