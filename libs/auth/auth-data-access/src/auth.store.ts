import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withState,
} from '@ngrx/signals';
import { AuthState, authInitialState, initialUserValue } from './auth.model';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe } from 'rxjs';
// import { AuthService } from './auth.service';

export const AuthStore = signalStore(
  withState<AuthState>(authInitialState),
  withMethods((store) => ({
    login: (user: any) => {
        sessionStorage.setItem('user', JSON.stringify(user));

        patchState(store, {
            loggedIn: true,
            user: user,
        })
    },
    logout: () => {
        sessionStorage.removeItem('user');

        patchState(store, {
            loggedIn: false,
            user: initialUserValue
        })
    },
  }))
);
