import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { switchMap, tap, catchError } from 'rxjs';
import { of } from 'rxjs';
import { OrganizationService } from '../services/organization.service';
import {
  Organization,
  ServiceResponse,
  OrganizationFilter,
  PaginatedResponse,
  CreateOrganizationRequest,
  UpdateOrganizationRequest,
} from '@sports-ui/api-types';

export type OrganizationState = {
  organizations: Organization[];
  selectedOrganization: Organization | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export const organizationInitialState: OrganizationState = {
  organizations: [],
  selectedOrganization: null,
  loading: false,
  error: null,
  totalCount: 0,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 0,
};

export const OrganizationStore = signalStore(
  { providedIn: 'root' },

  withState<OrganizationState>(organizationInitialState),

  withMethods((store) => {
    const organizationService = inject(OrganizationService);

    return {
      // Load organizations with pagination and filtering
      loadOrganizations: rxMethod<OrganizationFilter>(
        switchMap((filter) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return organizationService.getOrganizations(filter).pipe(
            tap((res: ServiceResponse<PaginatedResponse<Organization>>) => {
              if (res.success && res.data) {
                patchState(store, {
                  organizations: res.data.items,
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
                error: error.message || 'Failed to load organizations',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Load single organization
      loadOrganization: rxMethod<string>(
        switchMap((organizationId) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return organizationService.getOrganization(organizationId).pipe(
            tap((res: ServiceResponse<Organization>) => {
              if (res.success && res.data) {
                patchState(store, {
                  selectedOrganization: res.data,
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
                error: error.message || 'Failed to load organization',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Create organization
      createOrganization: rxMethod<CreateOrganizationRequest>(
        switchMap((request) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return organizationService.createOrganization(request).pipe(
            tap((res: ServiceResponse<string>) => {
              if (res.success) {
                patchState(store, {
                  loading: false,
                });
                // Optionally reload organizations list
              } else {
                patchState(store, {
                  error: res.message,
                  loading: false,
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to create organization',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Update organization
      updateOrganization: rxMethod<UpdateOrganizationRequest>(
        switchMap((request) => {
          patchState(store, {
            loading: true,
            error: null,
          });

          return organizationService.updateOrganization(request).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Update the organization in the list if it exists
                const currentOrganizations = store.organizations();
                const updatedOrganizations = currentOrganizations.map(org =>
                  org.id === request.id ? { ...org, ...request } : org
                );
                
                patchState(store, {
                  organizations: updatedOrganizations,
                  selectedOrganization: store.selectedOrganization()?.id === request.id 
                    ? { ...store.selectedOrganization()!, ...request }
                    : store.selectedOrganization(),
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
                error: error.message || 'Failed to update organization',
                loading: false,
              });
              return of(null);
            })
          );
        })
      ),

      // Lock organization
      lockOrganization: rxMethod<{ organizationId: string; reason: string }>(
        switchMap(({ organizationId, reason }) => {
          return organizationService.lockOrganization(organizationId, reason).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Update the organization's locked status
                const currentOrganizations = store.organizations();
                const updatedOrganizations = currentOrganizations.map(org =>
                  org.id === organizationId ? { ...org, isLocked: true } : org
                );
                
                patchState(store, {
                  organizations: updatedOrganizations,
                  selectedOrganization: store.selectedOrganization()?.id === organizationId 
                    ? { ...store.selectedOrganization()!, isLocked: true }
                    : store.selectedOrganization(),
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to lock organization',
              });
              return of(null);
            })
          );
        })
      ),

      // Unlock organization
      unlockOrganization: rxMethod<string>(
        switchMap((organizationId) => {
          return organizationService.unlockOrganization(organizationId).pipe(
            tap((res: ServiceResponse<boolean>) => {
              if (res.success) {
                // Update the organization's locked status
                const currentOrganizations = store.organizations();
                const updatedOrganizations = currentOrganizations.map(org =>
                  org.id === organizationId ? { ...org, isLocked: false } : org
                );
                
                patchState(store, {
                  organizations: updatedOrganizations,
                  selectedOrganization: store.selectedOrganization()?.id === organizationId 
                    ? { ...store.selectedOrganization()!, isLocked: false }
                    : store.selectedOrganization(),
                });
              }
            }),
            catchError((error) => {
              patchState(store, {
                error: error.message || 'Failed to unlock organization',
              });
              return of(null);
            })
          );
        })
      ),

      // Set selected organization
      setSelectedOrganization: (organization: Organization | null) => {
        patchState(store, { selectedOrganization: organization });
      },

      // Clear error
      clearError: () => {
        patchState(store, { error: null });
      },

      // Clear store
      clear: () => {
        patchState(store, organizationInitialState);
      },
    };
  })
);
