import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@sports-ui/http-client';
import { ServiceResponse } from '@sports-ui/api-types';
import { environment } from '@sports-ui/api-types';
import {
  PackConfigDto,
  RarityTierConfigDto,
  UpdatePackConfigRequest,
  UpdateRarityTierRequest,
} from './economy-admin.model';

@Injectable({ providedIn: 'root' })
export class EconomyAdminApi {
  private readonly http = inject(ApiService);

  getRarityTiers(orgId: string): Observable<ServiceResponse<RarityTierConfigDto[]>> {
    return this.http.get<ServiceResponse<RarityTierConfigDto[]>>(
      `${environment.sportsApi}admin/economy/rarity-tiers/${orgId}`
    );
  }

  updateRarityTier(
    configId: string,
    body: UpdateRarityTierRequest
  ): Observable<ServiceResponse<RarityTierConfigDto>> {
    return this.http.put<ServiceResponse<RarityTierConfigDto>, UpdateRarityTierRequest>(
      `${environment.sportsApi}admin/economy/rarity-tiers/${configId}`,
      body
    );
  }

  getPackConfig(orgId: string): Observable<ServiceResponse<PackConfigDto>> {
    return this.http.get<ServiceResponse<PackConfigDto>>(
      `${environment.sportsApi}admin/economy/pack-config/${orgId}`
    );
  }

  updatePackConfig(
    orgId: string,
    body: UpdatePackConfigRequest
  ): Observable<ServiceResponse<PackConfigDto>> {
    return this.http.put<ServiceResponse<PackConfigDto>, UpdatePackConfigRequest>(
      `${environment.sportsApi}admin/economy/pack-config/${orgId}`,
      body
    );
  }
}
