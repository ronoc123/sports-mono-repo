import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RosterPlayer } from '@sports-ui/roaster-data-access';
import { PlayerCardComponent } from '@sports-ui/ui';

@Component({
  selector: 'lib-defensive-lineup',
  imports: [CommonModule, PlayerCardComponent],
  templateUrl: './defensive-lineup.html',
  styleUrl: './defensive-lineup.css',
})
export class DefensiveLineup {
  players = input.required<RosterPlayer[]>();

  getPlayer(position: string, index = 0): RosterPlayer | undefined {
    return this.players().filter(p => p.position === position)[index];
  }
}
