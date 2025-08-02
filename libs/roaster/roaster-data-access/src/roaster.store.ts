import {
  signalStore,
  withState,
  withMethods,
  patchState,
} from '@ngrx/signals';
import { DEFENSE_PLAYERS, OFFENSE_PLAYERS, Player } from '@sports-ui/api-types';


export interface RoasterState {
  offense: Player[];
  defense: Player[];
}

const initialRosterState: RoasterState = {
  offense: OFFENSE_PLAYERS,
  defense: DEFENSE_PLAYERS,
};

export const RoasterStore = signalStore(
  { providedIn: 'root' },
  withState<RoasterState>(initialRosterState),

  withMethods((store) => ({
    setRoster: (offense: Player[], defense: Player[]) => {

      patchState(store, {
        offense,
        defense,
      });
    },
  }))
);
