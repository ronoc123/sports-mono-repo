import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root",
})
export class ApiConfigService {
  private readonly baseUrl = "http://localhost:5000/api"; // Update this to match your API URL

  get apiBaseUrl(): string {
    return this.baseUrl;
  }

  // Helper methods for common endpoints
  getUserProfileUrl(userId: string): string {
    return `${this.baseUrl}/User/profile/${userId}`;
  }

  getUpdateProfileUrl(userId: string): string {
    return `${this.baseUrl}/User/profile/${userId}`;
  }

  getUpdatePreferencesUrl(userId: string): string {
    return `${this.baseUrl}/User/profile/${userId}/preferences`;
  }

  getUploadAvatarUrl(userId: string): string {
    return `${this.baseUrl}/User/profile/${userId}/avatar`;
  }

  getAvailableCodesUrl(userId: string): string {
    return `${this.baseUrl}/Code/available/${userId}`;
  }

  getUserBalanceUrl(userId: string): string {
    return `${this.baseUrl}/Code/balance/${userId}`;
  }

  getRedeemCodeUrl(): string {
    return `${this.baseUrl}/Code/redeem-by-code`;
  }

  getRedemptionHistoryUrl(userId: string): string {
    return `${this.baseUrl}/Code/history/${userId}`;
  }

  getDashboardStatsUrl(): string {
    return `${this.baseUrl}/Dashboard/stats`;
  }

  getUserDashboardUrl(userId: string): string {
    return `${this.baseUrl}/Dashboard/user/${userId}`;
  }

  // Player Option endpoints
  getPlayerOptionsForUserUrl(userId: string): string {
    return `${this.baseUrl}/PlayerOption/user/${userId}`;
  }

  getPlayerOptionUrl(playerOptionId: string): string {
    return `${this.baseUrl}/PlayerOption/${playerOptionId}`;
  }

  getPlayerOptionStatsUrl(): string {
    return `${this.baseUrl}/PlayerOption/stats`;
  }

  getVoteOnPlayerOptionUrl(): string {
    return `${this.baseUrl}/PlayerOption/vote`;
  }

  getAllPlayerOptionsUrl(): string {
    return `${this.baseUrl}/PlayerOption/all`;
  }

  // User Management endpoints
  getUsersForManagementUrl(): string {
    return `${this.baseUrl}/User/management`;
  }

  getUserStatsOverviewUrl(): string {
    return `${this.baseUrl}/User/stats`;
  }

  getCreateUserUrl(): string {
    return `${this.baseUrl}/User/create`;
  }

  getUpdateUserStatusUrl(): string {
    return `${this.baseUrl}/User/status`;
  }

  getAssignUserRoleUrl(): string {
    return `${this.baseUrl}/User/assign-role`;
  }

  getBulkUserActionUrl(): string {
    return `${this.baseUrl}/User/bulk-action`;
  }
}
