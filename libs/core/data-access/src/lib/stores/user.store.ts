import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { switchMap, tap, catchError } from 'rxjs';
import { of } from 'rxjs';
import { UserService } from '../services/user.service';
import {
  User,
  ServiceResponse,
  UserFilter,
  PaginatedResponse,
  CreateUserRequest,
  UpdateUserRequest,
  AddVotesRequest,
} from '@sports-ui/api-types';

export type UserState = {
  users: User[];
  currentUser: User | null;
  selectedUser: User | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export const userInitialState: UserState = {
  users: [],
  currentUser: null,
  selectedUser: null,
  loading: false,
  error: null,
  totalCount: 0,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 0,
};

export const UserStore = signalStore(
  { providedIn: 'root' },

  withState<UserState>(userInitialState),

  withMethods((store) => {
    const userService = inject(UserService);

    return {
      // Load users with pagination and filtering
      loadUsers: rxMethod<UserFilter>(
        switchMap((filter) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return userService.getUsers(filter).pipe(
            tap((res: ServiceResponse<PaginatedResponse<User>>) => {
              if (res.success && res.data) {
                patchState(store, {
                  users: res.data.items,
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
                error: error.message || 'Failed to load users',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Load current user
      loadCurrentUser: rxMethod<void>(
        switchMap(() => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return userService.getCurrentUser().pipe(
            tap((res: ServiceResponse<User>) => {
              if (res.success && res.data) {
                patchState(store, {
                  currentUser: res.data,
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
                error: error.message || 'Failed to load current user',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Load single user
      loadUser: rxMethod<string>(
        switchMap((userId) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return userService.getUser(userId).pipe(
            tap((res: ServiceResponse<User>) => {
              if (res.success && res.data) {
                patchState(store, {
                  selectedUser: res.data,
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
                error: error.message || 'Failed to load user',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Create user
      createUser: rxMethod<CreateUserRequest>(
        switchMap((request) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return userService.createUser(request).pipe(
            tap((res: ServiceResponse<string>) => {
              if (res.success) {
                patchState(store, {
                  loading: false,
                });
                // Optionally reload users list
              } else {
                patchState(store, {
                  error: res.message,
                  loading: false,
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to create user',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Update user
      updateUser: rxMethod<UpdateUserRequest>(
        switchMap((request) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return userService.updateUser(request).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Update the user in the list if it exists
                const currentUsers = store.users();
                const updatedUsers = currentUsers.map(user =>
                  user.id === request.id ? { ...user, ...request } : user
                );
                
                patchState(store, {
                  users: updatedUsers,
                  currentUser: store.currentUser()?.id === request.id 
                    ? { ...store.currentUser()!, ...request }
                    : store.currentUser(),
                  selectedUser: store.selectedUser()?.id === request.id 
                    ? { ...store.selectedUser()!, ...request }
                    : store.selectedUser(),
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
                error: error.message || 'Failed to update user',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Add votes for user
      addVotesForUser: rxMethod<AddVotesRequest>(
        switchMap((request) => {
          return userService.addVotesForUser(request).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Optionally update user's vote count locally
                // This would require loading user's vote data
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to add votes for user',
              });
              return of(null);
            })
          );
        })
      ),

      // Use vote for organization
      useVoteForOrganization: rxMethod<{ userId: string; organizationId: string }>(
        switchMap(({ userId, organizationId }) => {
          return userService.useVoteForOrganization(userId, organizationId).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Optionally update user's vote count locally
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to use vote',
              });
              return of(null);
            })
          );
        })
      ),

      // Update email
      updateEmail: rxMethod<{ userId: string; email: string }>(
        switchMap(({ userId, email }) => {
          return userService.updateEmail(userId, email).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Update the user's email locally
                const currentUsers = store.users();
                const updatedUsers = currentUsers.map(user =>
                  user.id === userId ? { ...user, email } : user
                );
                
                patchState(store, {
                  users: updatedUsers,
                  currentUser: store.currentUser()?.id === userId 
                    ? { ...store.currentUser()!, email }
                    : store.currentUser(),
                  selectedUser: store.selectedUser()?.id === userId 
                    ? { ...store.selectedUser()!, email }
                    : store.selectedUser(),
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to update email',
              });
              return of(null);
            })
          );
        })
      ),

      // Set current user
      setCurrentUser: (user: User | null) => {
        patchState(store, { currentUser: user });
      },

      // Set selected user
      setSelectedUser: (user: User | null) => {
        patchState(store, { selectedUser: user });
      },

      // Clear error
      clearError: () => {
        patchState(store, { error: null });
      },

      // Clear store
      clear: () => {
        patchState(store, userInitialState);
      },
    };
  })
);
