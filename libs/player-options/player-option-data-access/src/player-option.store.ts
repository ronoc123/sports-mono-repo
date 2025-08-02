import { signalStore, withState, withMethods, patchState } from "@ngrx/signals";
import { inject } from "@angular/core";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { switchMap, tap, catchError } from "rxjs";
import { of } from "rxjs";
import { PlayerOptionService } from "./services/player-options.service";
import {
  PlayerOption,
  ServiceResponse,
  PlayerOptionFilter,
  PaginatedResponse,
} from "@sports-ui/api-types";
import {
  PlayerOptionState,
  playerOptionInitialState,
} from "./playey-option.model";

export const PlayerOptionStore = signalStore(
  { providedIn: "root" },

  withState<PlayerOptionState>(playerOptionInitialState),

  withMethods((store) => {
    const playerOptionService = inject(PlayerOptionService);

    return {
      // Legacy method for backward compatibility
      getPlayerOptions: rxMethod<string>(
        switchMap((orgId) => {
          patchState(store, {
            playerOptions: [],
            loading: true,
            error: null,
          });

          return playerOptionService.getPlayerOptionsByOrganization(orgId).pipe(
            tap((res: ServiceResponse<PlayerOption[]>) => {
              if (res.success && res.data) {
                patchState(store, {
                  playerOptions: res.data,
                  loading: false,
                });
              } else {
                patchState(store, {
                  error: res.message,
                  loading: false,
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || "Failed to load player options",
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // New method using pagination and filtering
      loadPlayerOptions: rxMethod<PlayerOptionFilter>(
        switchMap((filter) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return playerOptionService.getPlayerOptions(filter).pipe(
            tap((res: ServiceResponse<PaginatedResponse<PlayerOption>>) => {
              if (res.success && res.data) {
                patchState(store, {
                  playerOptions: res.data.items,
                  totalCount: res.data.totalCount,
                  pageNumber: res.data.pageNumber,
                  pageSize: res.data.pageSize,
                  totalPages: res.data.totalPages,
                  loading: false,
                });
              } else {
                patchState(store, {
                  error: res.message,
                  loading: false,
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || "Failed to load player options",
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Vote on player option
      voteOnPlayerOption: rxMethod<string>(
        switchMap((playerOptionId) => {
          return playerOptionService.voteOnPlayerOption(playerOptionId).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Update the vote count locally
                const currentOptions = store.playerOptions();
                const updatedOptions = currentOptions.map((option) =>
                  option.id === playerOptionId
                    ? { ...option, votes: option.votes + 1 }
                    : option
                );
                patchState(store, {
                  playerOptions: updatedOptions,
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || "Failed to vote on player option",
              });
              return of(null);
            })
          );
        })
      ),

      clearError: () => {
        patchState(store, { error: null });
      },

      clear: () => {
        patchState(store, playerOptionInitialState);
      },
    };
  })
);
