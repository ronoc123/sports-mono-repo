import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ApiService } from "@sports-ui/http-client";
import { environment } from "@sports-ui/api-types";
import { Observable } from "rxjs";
import { SendVotesCommand, SendVotesUserInfo } from "./send-votes.model";

@Injectable({ providedIn: "root" })
export class SendVotesApi {
  private readonly http = inject(ApiService);
  private readonly httpClient = inject(HttpClient);

  getUsers(): Observable<SendVotesUserInfo[]> {
    return this.httpClient.get<SendVotesUserInfo[]>(
      `${environment.identityApiBaseUrl}/auth/users`
    );
  }

  sendVotes(cmd: SendVotesCommand): Observable<unknown> {
    return this.http.post<unknown, SendVotesCommand>(
      `${environment.sportsApi}voteaccount/reward-for-user`,
      cmd
    );
  }
}
