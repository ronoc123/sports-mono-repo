import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from '@sports-ui/http-client';
import {
  League,
  ServiceResponse,
  PaginatedResponse,
  PaginationRequest,
  CreateLeagueRequest,
  UpdateLeagueRequest,
} from '@sports-ui/api-types';

@Injectable({
  providedIn: 'root'
})
export class LeagueService {
  private readonly apiService = inject(ApiService);

  /**
   * Get all leagues with pagination and filtering
   */
  getLeagues(filter: LeagueFilter): Observable<ServiceResponse<PaginatedResponse<League>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.searchTerm) {
      params = params.set('searchTerm', filter.searchTerm);
    }
    if (filter.sport) {
      params = params.set('sport', filter.sport);
    }
    if (filter.country) {
      params = params.set('country', filter.country);
    }
    if (filter.isActive !== undefined) {
      params = params.set('isActive', filter.isActive.toString());
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortDescending !== undefined) {
      params = params.set('sortDescending', filter.sortDescending.toString());
    }

    return this.apiService.get<ServiceResponse<PaginatedResponse<League>>>('api/League/all', params);
  }

  /**
   * Get a single league by ID
   */
  getLeague(leagueId: string): Observable<ServiceResponse<League>> {
    return this.apiService.get<ServiceResponse<League>>(`api/League/${leagueId}`);
  }

  /**
   * Create a new league
   */
  createLeague(league: CreateLeagueRequest): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, CreateLeagueRequest>('api/League/create', league);
  }

  /**
   * Update an existing league
   */
  updateLeague(league: UpdateLeagueRequest): Observable<ServiceResponse<boolean>> {
    return this.apiService.put<ServiceResponse<boolean>, UpdateLeagueRequest>('api/League/update', league);
  }

  /**
   * Delete a league
   */
  deleteLeague(leagueId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.delete<ServiceResponse<boolean>>(`api/League/delete/${leagueId}`);
  }

  /**
   * Activate a league
   */
  activateLeague(leagueId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { leagueId: string }>(
      'api/League/activate',
      { leagueId }
    );
  }

  /**
   * Deactivate a league
   */
  deactivateLeague(leagueId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { leagueId: string }>(
      'api/League/deactivate',
      { leagueId }
    );
  }

  /**
   * Get leagues by sport
   */
  getLeaguesBySport(sport: string): Observable<ServiceResponse<League[]>> {
    const params = new HttpParams().set('sport', sport);
    return this.apiService.get<ServiceResponse<League[]>>('api/League/by-sport', params);
  }

  /**
   * Get leagues by country
   */
  getLeaguesByCountry(country: string): Observable<ServiceResponse<League[]>> {
    const params = new HttpParams().set('country', country);
    return this.apiService.get<ServiceResponse<League[]>>('api/League/by-country', params);
  }

  /**
   * Get active leagues
   */
  getActiveLeagues(): Observable<ServiceResponse<League[]>> {
    return this.apiService.get<ServiceResponse<League[]>>('api/League/active');
  }

  /**
   * Get league statistics
   */
  getLeagueStats(leagueId: string): Observable<ServiceResponse<LeagueStats>> {
    return this.apiService.get<ServiceResponse<LeagueStats>>(`api/League/${leagueId}/stats`);
  }

  /**
   * Get organizations in league
   */
  getLeagueOrganizations(leagueId: string): Observable<ServiceResponse<any[]>> {
    return this.apiService.get<ServiceResponse<any[]>>(`api/League/${leagueId}/organizations`);
  }

  /**
   * Get players in league
   */
  getLeaguePlayers(leagueId: string): Observable<ServiceResponse<any[]>> {
    return this.apiService.get<ServiceResponse<any[]>>(`api/League/${leagueId}/players`);
  }

  /**
   * Get available sports
   */
  getAvailableSports(): Observable<ServiceResponse<string[]>> {
    return this.apiService.get<ServiceResponse<string[]>>('api/League/sports');
  }

  /**
   * Get available countries
   */
  getAvailableCountries(): Observable<ServiceResponse<string[]>> {
    return this.apiService.get<ServiceResponse<string[]>>('api/League/countries');
  }
}

// Filter interface for league operations
export interface LeagueFilter extends PaginationRequest {
  sport?: string;
  country?: string;
  isActive?: boolean;
}

// Additional interfaces for league operations
export interface LeagueStats {
  totalOrganizations: number;
  activeOrganizations: number;
  totalPlayers: number;
  activePlayers: number;
  totalPlayerOptions: number;
  activePlayerOptions: number;
  totalVotes: number;
  averagePlayerAge: number;
  totalMarketValue: number;
  topOrganizations: any[];
  popularSports: string[];
}
