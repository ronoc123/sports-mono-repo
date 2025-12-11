import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { VoteAccount } from "./vote-account.model";
import { ApiService } from "@sports-ui/http-client";
import { ServiceResponse } from "@sports-ui/api-types";
import { environment } from "@sports-ui/api-types";
import { HttpParams } from "@angular/common/http";

@Injectable({ providedIn: "root" })
export class VoteAccountApi {
  private readonly http = inject(ApiService);

  getVoteAccount(
    userId: string,
    organizationId: string
  ): Observable<ServiceResponse<VoteAccount>> {
    return this.http.get(
      `${environment.sportsApi}voteaccount/get-vote-account/${userId}/organization/${organizationId}`
    );
  }
}
