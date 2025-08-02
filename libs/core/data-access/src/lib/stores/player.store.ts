import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { switchMap, tap, catchError } from 'rxjs';
import { of } from 'rxjs';
import { PlayerService, CreatePlayerRequest, UpdatePlayerRequest } from '../services/player.service';
import {
  Player,
  ServiceResponse,
  PlayerFilter,
  PaginatedResponse,
} from '@sports-ui/api-types';

export type PlayerState = {
  players: Player[];
  selectedPlayer: Player | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export const playerInitialState: PlayerState = {
  players: [],
  selectedPlayer: null,
  loading: false,
  error: null,
  totalCount: 0,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 0,
};

export const PlayerStore = signalStore(
  { providedIn: 'root' },

  withState<PlayerState>(playerInitialState),

  withMethods((store) => {
    const playerService = inject(PlayerService);

    return {
      // Load players with pagination and filtering
      loadPlayers: rxMethod<PlayerFilter>(
        switchMap((filter) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return playerService.getPlayers(filter).pipe(
            tap((res: ServiceResponse<PaginatedResponse<Player>>) => {
              if (res.success && res.data) {
                patchState(store, {
                  players: res.data.items,
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
                error: error.message || 'Failed to load players',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Load single player
      loadPlayer: rxMethod<string>(
        switchMap((playerId) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return playerService.getPlayer(playerId).pipe(
            tap((res: ServiceResponse<Player>) => {
              if (res.success && res.data) {
                patchState(store, {
                  selectedPlayer: res.data,
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
                error: error.message || 'Failed to load player',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Create player
      createPlayer: rxMethod<CreatePlayerRequest>(
        switchMap((request) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return playerService.createPlayer(request).pipe(
            tap((res: ServiceResponse<string>) => {
              if (res.success) {
                patchState(store, {
                  loading: false,
                });
                // Optionally reload players list
              } else {
                patchState(store, {
                  error: res.message,
                  loading: false,
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to create player',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Update player
      updatePlayer: rxMethod<UpdatePlayerRequest>(
        switchMap((request) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return playerService.updatePlayer(request).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Update the player in the list if it exists
                const currentPlayers = store.players();
                const updatedPlayers = currentPlayers.map(player =>
                  player.id === request.id ? { ...player, ...request } : player
                );
                
                patchState(store, {
                  players: updatedPlayers,
                  selectedPlayer: store.selectedPlayer()?.id === request.id 
                    ? { ...store.selectedPlayer()!, ...request }
                    : store.selectedPlayer(),
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
                error: error.message || 'Failed to update player',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Transfer player to organization
      transferPlayerToOrganization: rxMethod<{ playerId: string; organizationId: string }>(
        switchMap(({ playerId, organizationId }) => {
          return playerService.transferPlayerToOrganization(playerId, organizationId).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Update the player's organization
                const currentPlayers = store.players();
                const updatedPlayers = currentPlayers.map(player =>
                  player.id === playerId ? { ...player, organizationId } : player
                );
                
                patchState(store, {
                  players: updatedPlayers,
                  selectedPlayer: store.selectedPlayer()?.id === playerId 
                    ? { ...store.selectedPlayer()!, organizationId }
                    : store.selectedPlayer(),
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to transfer player',
              });
              return of(null);
            })
          );
        })
      ),

      // Remove player from organization
      removePlayerFromOrganization: rxMethod<string>(
        switchMap((playerId) => {
          return playerService.removePlayerFromOrganization(playerId).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Update the player's organization
                const currentPlayers = store.players();
                const updatedPlayers = currentPlayers.map(player =>
                  player.id === playerId ? { ...player, organizationId: undefined } : player
                );
                
                patchState(store, {
                  players: updatedPlayers,
                  selectedPlayer: store.selectedPlayer()?.id === playerId 
                    ? { ...store.selectedPlayer()!, organizationId: undefined }
                    : store.selectedPlayer(),
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to remove player from organization',
              });
              return of(null);
            })
          );
        })
      ),

      // Load players by organization
      loadPlayersByOrganization: rxMethod<string>(
        switchMap((organizationId) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return playerService.getPlayersByOrganization(organizationId).pipe(
            tap((res: ServiceResponse<Player[]>) => {
              if (res.success && res.data) {
                patchState(store, {
                  players: res.data,
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
                error: error.message || 'Failed to load players by organization',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Set selected player
      setSelectedPlayer: (player: Player | null) => {
        patchState(store, { selectedPlayer: player });
      },

      // Clear error
      clearError: () => {
        patchState(store, { error: null });
      },

      // Clear store
      clear: () => {
        patchState(store, playerInitialState);
      },
    };
  })
);
