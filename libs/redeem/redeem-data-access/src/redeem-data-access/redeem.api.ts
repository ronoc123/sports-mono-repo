import { Injectable, inject } from "@angular/core";
import { ApiService } from "@sports-ui/http-client";
import { environment, ServiceResponse } from "@sports-ui/api-types";
import { Observable } from "rxjs";

@Injectable({ providedIn: "root" })
export class RedeemApi {
  private readonly http = inject(ApiService);

  redeemReward(
    userId: string,
    rewardId: string
  ): Observable<ServiceResponse<string>> {
    return this.http.post<ServiceResponse<string>, any>(
      `${environment.sportsApi}vote/redeem-vote/${userId}/reward/${rewardId}`,
      {}
    );
  }
}
