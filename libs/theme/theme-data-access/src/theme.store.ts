// libs/ui-theme/src/lib/theme.store.ts
import {
  signalStore,
  withState,
  withMethods,
} from '@ngrx/signals';
import { ThemeConfig } from './models/theme.model';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { inject } from '@angular/core';
import { ThemeService } from './services/theme.service';
import { pipe } from 'rxjs';


export const ThemeStore = signalStore(
  withState<{ theme: ThemeConfig | null; loading: boolean }>({
    theme: null,
    loading: false,
  }),

  withMethods((store) => {
    const themeService = inject(ThemeService);

    return {
      loadTheme: rxMethod<string>(
        pipe(
        )
      ),
    };
  })
);
