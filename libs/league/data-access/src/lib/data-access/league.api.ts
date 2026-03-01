/* eslint-disable @typescript-eslint/no-inferrable-types */
import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { LeagueDto, CreateLeagueRequest } from "./league.model";
import { ApiService } from "@sports-ui/http-client";
import { ServiceResponse } from "@sports-ui/api-types";
import { environment } from "@sports-ui/api-types";

@Injectable({ providedIn: "root" })
export class LeaguesApi {
  private readonly http = inject(ApiService);

  getAllLeagues(
    pageNumber = 1,
    pageSize: number = 10,
    searchTerm?: string,
    sortBy: string = "Name",
    sortDescending: boolean = false
  ): Observable<ServiceResponse<LeagueDto[]>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      sortBy,
      sortDescending: sortDescending.toString(),
    });

    if (searchTerm) {
      params.append("searchTerm", searchTerm);
    }

    return this.http.get<ServiceResponse<LeagueDto[]>>(
      `${environment.sportsApi}league/all?${params.toString()}`
    );
  }

  createLeague(
    request: CreateLeagueRequest
  ): Observable<ServiceResponse<string>> {
    return this.http.post<ServiceResponse<string>, CreateLeagueRequest>(
      `${environment.sportsApi}league/add`,
      request
    );
  }
}
