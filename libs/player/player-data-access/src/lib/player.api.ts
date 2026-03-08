import { Injectable, inject } from "@angular/core";
import { HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { PlayerDto } from "./models/player.model";
import { ServiceResponse } from "./models/get-all-players.query";
import { environment } from "@sports-ui/api-types";
import { ApiService } from "@sports-ui/http-client";

@Injectable({ providedIn: "root" })
export class PlayerApi {
  private base = `${environment.sportsApi}player`;
  private http = inject(ApiService);

  getAll(leagueId?: string): Observable<ServiceResponse<PlayerDto[]>> {
    const params = leagueId
      ? new HttpParams().set("leagueId", leagueId)
      : new HttpParams();
    return this.http.get<ServiceResponse<PlayerDto[]>>(
      `${this.base}/all`,
      params
    );
  }

  update(command: any): Observable<any> {
    return this.http.put(`${this.base}/update`, command);
  }

  delete(playerId: string): Observable<any> {
    return this.http.delete(`${this.base}/delete/${playerId}`);
  }
}
