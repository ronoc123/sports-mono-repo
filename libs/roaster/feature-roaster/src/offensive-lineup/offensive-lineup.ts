import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RosterPlayer } from '@sports-ui/roaster-data-access';
import { PlayerCardComponent} from '@sports-ui/ui';

@Component({
  selector: 'lib-offensive-lineup',
  imports: [CommonModule, PlayerCardComponent],
  templateUrl: './offensive-lineup.html',
  styleUrl: './offensive-lineup.css',
})
export class OffensiveLineup {
  players = input.required<RosterPlayer[]>();

  getPlayer(position: string, index = 0): RosterPlayer | undefined {
    return this.players().filter(p => p.position === position)[index];
  }
}
