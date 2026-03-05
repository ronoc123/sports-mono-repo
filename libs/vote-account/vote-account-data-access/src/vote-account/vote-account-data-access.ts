import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { VoteAccount } from "./vote-account.model";
import { ApiService } from "@sports-ui/http-client";
import { ServiceResponse } from "@sports-ui/api-types";
import { environment } from "@sports-ui/api-types";

@Injectable({ providedIn: "root" })
export class VoteAccountApi {
  private readonly http = inject(ApiService);

  getVoteAccount(
    userId: string,
    leagueId: string
  ): Observable<ServiceResponse<VoteAccount>> {
    return this.http.get(
      `${environment.sportsApi}voteaccount/get-vote-account/${userId}/league/${leagueId}`
    );
  }

  castVote(
    playerOptionId: string,
    userId: string,
    voteAmount: number,
    leagueId: string
  ): Observable<ServiceResponse<boolean>> {
    return this.http.post(`${environment.sportsApi}PlayerOption/vote`, {
      playerOptionId,
      userId,
      voteAmount,
      leagueId,
    });
  }
}
