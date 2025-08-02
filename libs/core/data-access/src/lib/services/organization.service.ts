import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from '@sports-ui/http-client';
import {
  Organization,
  ServiceResponse,
  PaginatedResponse,
  OrganizationFilter,
  CreateOrganizationRequest,
  UpdateOrganizationRequest,
} from '@sports-ui/api-types';

@Injectable({
  providedIn: 'root'
})
export class OrganizationService {
  private readonly apiService = inject(ApiService);

  /**
   * Get all organizations with pagination and filtering
   */
  getOrganizations(filter: OrganizationFilter): Observable<ServiceResponse<PaginatedResponse<Organization>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.searchTerm) {
      params = params.set('searchTerm', filter.searchTerm);
    }
    if (filter.leagueId) {
      params = params.set('leagueId', filter.leagueId);
    }
    if (filter.sport) {
      params = params.set('sport', filter.sport);
    }
    if (filter.isLocked !== undefined) {
      params = params.set('isLocked', filter.isLocked.toString());
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortDescending !== undefined) {
      params = params.set('sortDescending', filter.sortDescending.toString());
    }

    return this.apiService.get<ServiceResponse<PaginatedResponse<Organization>>>('api/Org/all', params);
  }

  /**
   * Get a single organization by ID
   */
  getOrganization(organizationId: string): Observable<ServiceResponse<Organization>> {
    return this.apiService.get<ServiceResponse<Organization>>(`api/Org/${organizationId}`);
  }

  /**
   * Create a new organization
   */
  createOrganization(organization: CreateOrganizationRequest): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, CreateOrganizationRequest>('api/Org/create', organization);
  }

  /**
   * Update an existing organization
   */
  updateOrganization(organization: UpdateOrganizationRequest): Observable<ServiceResponse<boolean>> {
    return this.apiService.put<ServiceResponse<boolean>, UpdateOrganizationRequest>('api/Org/update', organization);
  }

  /**
   * Delete an organization
   */
  deleteOrganization(organizationId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.delete<ServiceResponse<boolean>>(`api/Org/delete/${organizationId}`);
  }

  /**
   * Lock an organization
   */
  lockOrganization(organizationId: string, reason: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { organizationId: string; reason: string }>(
      'api/Org/lock',
      { organizationId, reason }
    );
  }

  /**
   * Unlock an organization
   */
  unlockOrganization(organizationId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { organizationId: string }>(
      'api/Org/unlock',
      { organizationId }
    );
  }

  /**
   * Get organizations by league
   */
  getOrganizationsByLeague(leagueId: string): Observable<ServiceResponse<Organization[]>> {
    const params = new HttpParams().set('leagueId', leagueId);
    return this.apiService.get<ServiceResponse<Organization[]>>('api/Org/by-league', params);
  }

  /**
   * Get organization statistics
   */
  getOrganizationStats(organizationId: string): Observable<ServiceResponse<OrganizationStats>> {
    return this.apiService.get<ServiceResponse<OrganizationStats>>(`api/Org/${organizationId}/stats`);
  }

  /**
   * Get player options for organization
   */
  getOrganizationPlayerOptions(organizationId: string): Observable<ServiceResponse<any[]>> {
    return this.apiService.get<ServiceResponse<any[]>>(`api/Org/playerOptions/${organizationId}`);
  }

  /**
   * Get players for organization
   */
  getOrganizationPlayers(organizationId: string): Observable<ServiceResponse<any[]>> {
    return this.apiService.get<ServiceResponse<any[]>>(`api/Org/${organizationId}/players`);
  }

  /**
   * Get codes for organization
   */
  getOrganizationCodes(organizationId: string): Observable<ServiceResponse<any[]>> {
    return this.apiService.get<ServiceResponse<any[]>>(`api/Org/${organizationId}/codes`);
  }

  /**
   * Update team information
   */
  updateTeamInfo(organizationId: string, teamInfo: UpdateTeamInfoRequest): Observable<ServiceResponse<boolean>> {
    return this.apiService.put<ServiceResponse<boolean>, UpdateTeamInfoRequest>(
      `api/Org/${organizationId}/team-info`,
      teamInfo
    );
  }
}

// Additional interfaces for organization operations
export interface OrganizationStats {
  totalPlayers: number;
  activePlayers: number;
  totalPlayerOptions: number;
  activePlayerOptions: number;
  totalVotes: number;
  averagePlayerAge: number;
  totalMarketValue: number;
  popularPlayerOption?: any;
  trendingPlayerOptions: any[];
}

export interface UpdateTeamInfoRequest {
  teamId?: string;
  teamName?: string;
  teamShortName?: string;
  sport?: string;
}
