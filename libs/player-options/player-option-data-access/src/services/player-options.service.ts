import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { HttpParams } from "@angular/common/http";
import { ApiService } from "@sports-ui/http-client";
import {
  PlayerOption,
  ServiceResponse,
  PaginatedResponse,
  PlayerOptionFilter,
} from "@sports-ui/api-types";

@Injectable({
  providedIn: "root",
})
export class PlayerOptionService {
  private readonly apiService = inject(ApiService);

  /**
   * Get all player options with pagination and filtering
   */
  getPlayerOptions(
    filter: PlayerOptionFilter
  ): Observable<ServiceResponse<PaginatedResponse<PlayerOption>>> {
    let params = new HttpParams()
      .set("pageNumber", filter.pageNumber.toString())
      .set("pageSize", filter.pageSize.toString());

    if (filter.searchTerm) {
      params = params.set("searchTerm", filter.searchTerm);
    }
    if (filter.organizationId) {
      params = params.set("organizationId", filter.organizationId);
    }
    if (filter.playerId) {
      params = params.set("playerId", filter.playerId);
    }
    if (filter.isActive !== undefined) {
      params = params.set("isActive", filter.isActive.toString());
    }
    if (filter.isExpired !== undefined) {
      params = params.set("isExpired", filter.isExpired.toString());
    }
    if (filter.sortBy) {
      params = params.set("sortBy", filter.sortBy);
    }
    if (filter.sortDescending !== undefined) {
      params = params.set("sortDescending", filter.sortDescending.toString());
    }

    return this.apiService.get<
      ServiceResponse<PaginatedResponse<PlayerOption>>
    >("api/PlayerOption/all", params);
  }

  /**
   * Get player options by organization ID (legacy method for backward compatibility)
   */
  getPlayerOptionsByOrganization(
    orgId: string
  ): Observable<ServiceResponse<PlayerOption[]>> {
    return this.apiService.get<ServiceResponse<PlayerOption[]>>(
      `api/Org/playerOptions/${orgId}`
    );
  }

  /**
   * Get a single player option by ID
   */
  getPlayerOption(
    playerOptionId: string
  ): Observable<ServiceResponse<PlayerOption>> {
    return this.apiService.get<ServiceResponse<PlayerOption>>(
      `api/PlayerOption/${playerOptionId}`
    );
  }

  /**
   * Create a new player option
   */
  createPlayerOption(
    playerOption: CreatePlayerOptionRequest
  ): Observable<ServiceResponse<string>> {
    return this.apiService.post<
      ServiceResponse<string>,
      CreatePlayerOptionRequest
    >("api/PlayerOption/create", playerOption);
  }

  /**
   * Update an existing player option
   */
  updatePlayerOption(
    playerOption: UpdatePlayerOptionRequest
  ): Observable<ServiceResponse<boolean>> {
    return this.apiService.put<
      ServiceResponse<boolean>,
      UpdatePlayerOptionRequest
    >("api/PlayerOption/update", playerOption);
  }

  /**
   * Delete a player option
   */
  deletePlayerOption(
    playerOptionId: string
  ): Observable<ServiceResponse<boolean>> {
    return this.apiService.delete<ServiceResponse<boolean>>(
      `api/PlayerOption/delete/${playerOptionId}`
    );
  }

  /**
   * Vote on a player option
   */
  voteOnPlayerOption(
    playerOptionId: string
  ): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, {}>(
      `api/PlayerOption/${playerOptionId}/vote`,
      {}
    );
  }

  /**
   * Expire a player option
   */
  expirePlayerOption(
    playerOptionId: string
  ): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, {}>(
      `api/PlayerOption/${playerOptionId}/expire`,
      {}
    );
  }

  /**
   * Get active player options
   */
  getActivePlayerOptions(): Observable<ServiceResponse<PlayerOption[]>> {
    return this.apiService.get<ServiceResponse<PlayerOption[]>>(
      "api/PlayerOption/active"
    );
  }

  /**
   * Get trending player options
   */
  getTrendingPlayerOptions(): Observable<ServiceResponse<PlayerOption[]>> {
    return this.apiService.get<ServiceResponse<PlayerOption[]>>(
      "api/PlayerOption/trending"
    );
  }

  /**
   * Get popular player options
   */
  getPopularPlayerOptions(): Observable<ServiceResponse<PlayerOption[]>> {
    return this.apiService.get<ServiceResponse<PlayerOption[]>>(
      "api/PlayerOption/popular"
    );
  }

  /**
   * Get player options by player
   */
  getPlayerOptionsByPlayer(
    playerId: string
  ): Observable<ServiceResponse<PlayerOption[]>> {
    const params = new HttpParams().set("playerId", playerId);
    return this.apiService.get<ServiceResponse<PlayerOption[]>>(
      "api/PlayerOption/by-player",
      params
    );
  }

  /**
   * Extend player option expiry
   */
  extendPlayerOptionExpiry(
    playerOptionId: string,
    newExpiryDate: string
  ): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<
      ServiceResponse<boolean>,
      { playerOptionId: string; newExpiryDate: string }
    >("api/PlayerOption/extend-expiry", { playerOptionId, newExpiryDate });
  }
}

// Request interfaces for player option operations
export interface CreatePlayerOptionRequest {
  title: string;
  description: string;
  playerId: string;
  organizationId: string;
  expiresAt?: string;
}

export interface UpdatePlayerOptionRequest {
  id: string;
  title?: string;
  description?: string;
  expiresAt?: string;
}
