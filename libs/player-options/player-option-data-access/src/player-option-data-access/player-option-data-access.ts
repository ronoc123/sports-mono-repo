import { Injectable, inject } from "@angular/core";
import { HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { ServiceResponse } from "@sports-ui/api-types";
import {
  PlayerOptionDto,
  GetAllPlayerOptionsQuery,
  CreatePlayerOptionCommand,
} from "./player-option.model"; // <- your TS interfaces file
import { ApiService } from "@sports-ui/http-client";
import { environment } from "@sports-ui/api-types";

@Injectable({ providedIn: "root" })
export class PlayerOptionApi {
  private readonly http = inject(ApiService);

  getPlayerOptions(
    query: GetAllPlayerOptionsQuery = {}
  ): Observable<ServiceResponse<PlayerOptionDto[]>> {
    return this.http.get(
      `${environment.sportsApi}PlayerOption/GetPlayerOptionsByOrganization`,
      this.toParams(query)
    );
  }

  createPlayerOption(
    command: CreatePlayerOptionCommand
  ): Observable<ServiceResponse<PlayerOptionDto>> {
    return this.http.post<
      ServiceResponse<PlayerOptionDto>,
      CreatePlayerOptionCommand
    >(`${environment.sportsApi}PlayerOption/create`, command);
  }

  private toParams(q: GetAllPlayerOptionsQuery): HttpParams {
    let params = new HttpParams();

    if (q.organizationId)
      params = params.set("organizationId", q.organizationId);
    if (q.playerId) params = params.set("playerId", q.playerId);
    if (q.pageNumber) params = params.set("pageNumber", q.pageNumber);
    if (q.pageSize) params = params.set("pageSize", q.pageSize);
    if (q.searchTerm) params = params.set("searchTerm", q.searchTerm);
    if (q.sortBy) params = params.set("sortBy", q.sortBy);

    return params;
  }
}
