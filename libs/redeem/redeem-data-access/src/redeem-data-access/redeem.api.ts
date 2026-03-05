import { Injectable, inject } from "@angular/core";
import { ApiService } from "@sports-ui/http-client";
import { environment, ServiceResponse } from "@sports-ui/api-types";
import { Observable } from "rxjs";

@Injectable({ providedIn: "root" })
export class RedeemApi {
  private readonly http = inject(ApiService);

  redeemReward(
    userId: string,
    promoCode: string,
    leagueId: string
  ): Observable<ServiceResponse<string>> {
    return this.http.post<ServiceResponse<string>, any>(
      `${environment.sportsApi}voteaccount/redeem-vote/${userId}/reward/${promoCode}?leagueId=${leagueId}`,
      {}
    );
  }
}
