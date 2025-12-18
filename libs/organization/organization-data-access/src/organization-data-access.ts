import { Injectable, inject } from "@angular/core";
import { HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { ApiService } from "@sports-ui/http-client";
import { environment } from "@sports-ui/api-types";

import { PaginatedList, ServiceResponse } from "@sports-ui/api-types";

import {
  OrganizationDto,
  CreateOrganizationCommand,
  UpdateOrganizationCommand,
  GetAllOrganizationsQuery,
} from "./organization.model";

@Injectable({ providedIn: "root" })
export class OrganizationApi {
  private readonly http = inject(ApiService);
  private readonly base = `${environment.sportsApi}Org`;

  // -----------------------------------------------------
  // GET: All organizations
  // -----------------------------------------------------
  getAll(
    query: any = {}
  ): Observable<ServiceResponse<PaginatedList<OrganizationDto>>> {
    return this.http.get(
      `${this.base}/GetAllOrganization`,
      this.toParams(query)
    );
  }

  // -----------------------------------------------------
  // GET: Theme
  // -----------------------------------------------------
  getTheme(name: string): Observable<any> {
    return this.http.get(
      `${this.base}/theme`,
      new HttpParams().set("name", name)
    );
  }

  // -----------------------------------------------------
  // POST: Add organization
  // -----------------------------------------------------
  addOrganization(
    payload: CreateOrganizationCommand
  ): Observable<ServiceResponse<string>> {
    return this.http.post(`${this.base}/addOrganization`, payload);
  }

  // -----------------------------------------------------
  // PUT: Update organization
  // -----------------------------------------------------
  updateOrganization(
    payload: UpdateOrganizationCommand
  ): Observable<ServiceResponse<boolean>> {
    return this.http.put(`${this.base}/updateOrganization`, payload);
  }

  // -----------------------------------------------------
  // DELETE: Remove organization
  // -----------------------------------------------------
  deleteOrganization(
    organizationId: string
  ): Observable<ServiceResponse<boolean>> {
    return this.http.delete(
      `${this.base}/deleteOrganization/${organizationId}`
    );
  }

  // -----------------------------------------------------
  // Helper: Convert query object to HttpParams
  // -----------------------------------------------------
  private toParams(q: GetAllOrganizationsQuery): HttpParams {
    let params = new HttpParams();

    if (q.pageNumber) params = params.set("pageNumber", q.pageNumber);
    if (q.pageSize) params = params.set("pageSize", q.pageSize);
    if (q.searchTerm) params = params.set("searchTerm", q.searchTerm);
    if (q.leagueId) params = params.set("leagueId", q.leagueId);
    if (q.sport) params = params.set("sport", q.sport);
    if (q.sortBy) params = params.set("sortBy", q.sortBy);

    params = params.set("sortDescending", q.sortDescending ?? false);

    return params;
  }
}
