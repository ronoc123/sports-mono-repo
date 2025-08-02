import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from '@sports-ui/http-client';
import {
  Code,
  ServiceResponse,
  PaginatedResponse,
  CodeFilter,
  GenerateCodeRequest,
  RedeemCodeRequest,
  CodeRedemptionResponse,
} from '@sports-ui/api-types';

@Injectable({
  providedIn: 'root'
})
export class CodeService {
  private readonly apiService = inject(ApiService);

  /**
   * Get all codes with pagination and filtering
   */
  getCodes(filter: CodeFilter): Observable<ServiceResponse<PaginatedResponse<Code>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.searchTerm) {
      params = params.set('searchTerm', filter.searchTerm);
    }
    if (filter.organizationId) {
      params = params.set('organizationId', filter.organizationId);
    }
    if (filter.userId) {
      params = params.set('userId', filter.userId);
    }
    if (filter.isRedeemed !== undefined) {
      params = params.set('isRedeemed', filter.isRedeemed.toString());
    }
    if (filter.isExpired !== undefined) {
      params = params.set('isExpired', filter.isExpired.toString());
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortDescending !== undefined) {
      params = params.set('sortDescending', filter.sortDescending.toString());
    }

    return this.apiService.get<ServiceResponse<PaginatedResponse<Code>>>('api/Code/all', params);
  }

  /**
   * Get a single code by ID
   */
  getCode(codeId: string): Observable<ServiceResponse<Code>> {
    return this.apiService.get<ServiceResponse<Code>>(`api/Code/${codeId}`);
  }

  /**
   * Get code by code value
   */
  getCodeByValue(codeValue: string): Observable<ServiceResponse<Code>> {
    const params = new HttpParams().set('codeValue', codeValue);
    return this.apiService.get<ServiceResponse<Code>>('api/Code/by-value', params);
  }

  /**
   * Generate a new code
   */
  generateCode(request: GenerateCodeRequest): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, GenerateCodeRequest>('api/Code/generate', request);
  }

  /**
   * Redeem a code
   */
  redeemCode(request: RedeemCodeRequest): Observable<ServiceResponse<CodeRedemptionResponse>> {
    return this.apiService.post<ServiceResponse<CodeRedemptionResponse>, RedeemCodeRequest>('api/Code/redeem', request);
  }

  /**
   * Delete a code
   */
  deleteCode(codeId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.delete<ServiceResponse<boolean>>(`api/Code/delete/${codeId}`);
  }

  /**
   * Get codes by organization
   */
  getCodesByOrganization(organizationId: string): Observable<ServiceResponse<Code[]>> {
    const params = new HttpParams().set('organizationId', organizationId);
    return this.apiService.get<ServiceResponse<Code[]>>('api/Code/by-organization', params);
  }

  /**
   * Get codes by user
   */
  getCodesByUser(userId: string): Observable<ServiceResponse<Code[]>> {
    const params = new HttpParams().set('userId', userId);
    return this.apiService.get<ServiceResponse<Code[]>>('api/Code/by-user', params);
  }

  /**
   * Get active codes
   */
  getActiveCodes(): Observable<ServiceResponse<Code[]>> {
    return this.apiService.get<ServiceResponse<Code[]>>('api/Code/active');
  }

  /**
   * Get expired codes
   */
  getExpiredCodes(): Observable<ServiceResponse<Code[]>> {
    return this.apiService.get<ServiceResponse<Code[]>>('api/Code/expired');
  }

  /**
   * Get redeemed codes
   */
  getRedeemedCodes(): Observable<ServiceResponse<Code[]>> {
    return this.apiService.get<ServiceResponse<Code[]>>('api/Code/redeemed');
  }

  /**
   * Get unredeemed codes
   */
  getUnredeemedCodes(): Observable<ServiceResponse<Code[]>> {
    return this.apiService.get<ServiceResponse<Code[]>>('api/Code/unredeemed');
  }

  /**
   * Validate code
   */
  validateCode(codeValue: string): Observable<ServiceResponse<CodeValidationResult>> {
    const params = new HttpParams().set('codeValue', codeValue);
    return this.apiService.get<ServiceResponse<CodeValidationResult>>('api/Code/validate', params);
  }

  /**
   * Get code statistics
   */
  getCodeStats(): Observable<ServiceResponse<CodeStats>> {
    return this.apiService.get<ServiceResponse<CodeStats>>('api/Code/stats');
  }

  /**
   * Get organization code statistics
   */
  getOrganizationCodeStats(organizationId: string): Observable<ServiceResponse<OrganizationCodeStats>> {
    return this.apiService.get<ServiceResponse<OrganizationCodeStats>>(`api/Code/organization/${organizationId}/stats`);
  }

  /**
   * Bulk generate codes
   */
  bulkGenerateCodes(request: BulkGenerateCodesRequest): Observable<ServiceResponse<string[]>> {
    return this.apiService.post<ServiceResponse<string[]>, BulkGenerateCodesRequest>('api/Code/bulk-generate', request);
  }

  /**
   * Expire code
   */
  expireCode(codeId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { codeId: string }>('api/Code/expire', { codeId });
  }

  /**
   * Extend code expiry
   */
  extendCodeExpiry(codeId: string, newExpiryDate: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { codeId: string; newExpiryDate: string }>(
      'api/Code/extend-expiry',
      { codeId, newExpiryDate }
    );
  }
}

// Additional interfaces for code operations
export interface CodeValidationResult {
  isValid: boolean;
  isExpired: boolean;
  isRedeemed: boolean;
  votes: number;
  organizationId?: string;
  organizationName?: string;
  expiresAt?: string;
  message: string;
}

export interface CodeStats {
  totalCodes: number;
  activeCodes: number;
  expiredCodes: number;
  redeemedCodes: number;
  unredeemedCodes: number;
  totalVotesDistributed: number;
  averageVotesPerCode: number;
  redemptionRate: number;
}

export interface OrganizationCodeStats {
  organizationId: string;
  organizationName: string;
  totalCodes: number;
  activeCodes: number;
  expiredCodes: number;
  redeemedCodes: number;
  unredeemedCodes: number;
  totalVotesDistributed: number;
  redemptionRate: number;
}

export interface BulkGenerateCodesRequest {
  organizationId: string;
  count: number;
  votes: number;
  expiresAt?: string;
}
