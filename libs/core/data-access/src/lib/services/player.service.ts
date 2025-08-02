import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from '@sports-ui/http-client';
import {
  Player,
  ServiceResponse,
  PaginatedResponse,
  PlayerFilter,
} from '@sports-ui/api-types';

@Injectable({
  providedIn: 'root'
})
export class PlayerService {
  private readonly apiService = inject(ApiService);

  /**
   * Get all players with pagination and filtering
   */
  getPlayers(filter: PlayerFilter): Observable<ServiceResponse<PaginatedResponse<Player>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.searchTerm) {
      params = params.set('searchTerm', filter.searchTerm);
    }
    if (filter.leagueId) {
      params = params.set('leagueId', filter.leagueId);
    }
    if (filter.organizationId) {
      params = params.set('organizationId', filter.organizationId);
    }
    if (filter.position) {
      params = params.set('position', filter.position);
    }
    if (filter.minAge !== undefined) {
      params = params.set('minAge', filter.minAge.toString());
    }
    if (filter.maxAge !== undefined) {
      params = params.set('maxAge', filter.maxAge.toString());
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

    return this.apiService.get<ServiceResponse<PaginatedResponse<Player>>>('api/Player/all', params);
  }

  /**
   * Get a single player by ID
   */
  getPlayer(playerId: string): Observable<ServiceResponse<Player>> {
    return this.apiService.get<ServiceResponse<Player>>(`api/Player/${playerId}`);
  }

  /**
   * Create a new player
   */
  createPlayer(player: CreatePlayerRequest): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, CreatePlayerRequest>('api/Player/create', player);
  }

  /**
   * Update an existing player
   */
  updatePlayer(player: UpdatePlayerRequest): Observable<ServiceResponse<boolean>> {
    return this.apiService.put<ServiceResponse<boolean>, UpdatePlayerRequest>('api/Player/update', player);
  }

  /**
   * Delete a player
   */
  deletePlayer(playerId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.delete<ServiceResponse<boolean>>(`api/Player/delete/${playerId}`);
  }

  /**
   * Get players by organization
   */
  getPlayersByOrganization(organizationId: string): Observable<ServiceResponse<Player[]>> {
    const params = new HttpParams().set('organizationId', organizationId);
    return this.apiService.get<ServiceResponse<Player[]>>('api/Player/by-organization', params);
  }

  /**
   * Get players by league
   */
  getPlayersByLeague(leagueId: string): Observable<ServiceResponse<Player[]>> {
    const params = new HttpParams().set('leagueId', leagueId);
    return this.apiService.get<ServiceResponse<Player[]>>('api/Player/by-league', params);
  }

  /**
   * Get players by position
   */
  getPlayersByPosition(position: string): Observable<ServiceResponse<Player[]>> {
    const params = new HttpParams().set('position', position);
    return this.apiService.get<ServiceResponse<Player[]>>('api/Player/by-position', params);
  }

  /**
   * Transfer player to organization
   */
  transferPlayerToOrganization(playerId: string, organizationId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { playerId: string; organizationId: string }>(
      'api/Player/transfer-to-organization',
      { playerId, organizationId }
    );
  }

  /**
   * Remove player from organization
   */
  removePlayerFromOrganization(playerId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { playerId: string }>(
      'api/Player/remove-from-organization',
      { playerId }
    );
  }

  /**
   * Transfer player to league
   */
  transferPlayerToLeague(playerId: string, leagueId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { playerId: string; leagueId: string }>(
      'api/Player/transfer-to-league',
      { playerId, leagueId }
    );
  }
}

// Request interfaces for player operations
export interface CreatePlayerRequest {
  name: string;
  position: string;
  imageUrl?: string;
  age: number;
  leagueId: string;
  organizationId?: string;
}

export interface UpdatePlayerRequest {
  id: string;
  name?: string;
  position?: string;
  imageUrl?: string;
  age?: number;
  leagueId?: string;
  organizationId?: string;
}
