import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { PlayerDto } from "./models/player.model";
import { GetAllPlayersQuery } from "./models/get-all-players.query";

@Injectable({ providedIn: "root" })
export class PlayerApi {
  private base = "http://localhost:5000/api/player";
  private http = inject(HttpClient);

  getAll(leagueId?: string): Observable<PlayerDto[]> {
    return this.http.get<PlayerDto[]>(`${this.base}/all`, {
      params: leagueId ? { leagueId } : {},
    });
  }

  update(command: any): Observable<any> {
    return this.http.put(`${this.base}/update`, command);
  }

  delete(playerId: string): Observable<any> {
    return this.http.delete(`${this.base}/delete/${playerId}`);
  }
}
