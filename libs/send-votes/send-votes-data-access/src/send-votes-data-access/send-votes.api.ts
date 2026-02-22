import { Injectable, inject } from "@angular/core";
import { ApiService } from "@sports-ui/http-client";
import { environment } from "@sports-ui/api-types";
import { Observable } from "rxjs";
import { SendVotesCommand, SendVotesUserInfo } from "./send-votes.model";

@Injectable({ providedIn: "root" })
export class SendVotesApi {
  private readonly http = inject(ApiService);

  getUsers(): Observable<SendVotesUserInfo[]> {
    return this.http.get<SendVotesUserInfo[]>(
      `${environment.identityApi}auth/users`
    );
  }

  sendVotes(cmd: SendVotesCommand): Observable<unknown> {
    return this.http.post<unknown, SendVotesCommand>(
      `${environment.sportsApi}voteaccount/reward-for-user`,
      cmd
    );
  }
}
