import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable, map } from "rxjs";
import {
  PlayerOptionDto,
  GetAllPlayerOptionsQuery,
  ServiceResponse,
  PaginatedList,
} from "./playey-option.model"; // <- your TS interfaces file

@Injectable({ providedIn: "root" })
export class PlayerOptionApi {
  private http = inject(HttpClient);

  /** Full envelope (useful if you want pagination metadata) */
  getPlayerOptions(
    query: GetAllPlayerOptionsQuery = {}
  ): Observable<ServiceResponse<PaginatedList<PlayerOptionDto>>> {
    const params = toParams(query);
    return this.http.get<ServiceResponse<PaginatedList<PlayerOptionDto>>>(
      `player-options`,
      { params }
    );
  }

  /** Convenience: just the items[] for simple lists */
  getPlayerOptionItems$(
    query: GetAllPlayerOptionsQuery = {}
  ): Observable<PlayerOptionDto[]> {
    return this.getPlayerOptions(withDefaults(query)).pipe(
      map((res) => res.data?.items ?? [])
    );
  }
}

/* ---------- helpers ---------- */

function withDefaults(q: GetAllPlayerOptionsQuery): GetAllPlayerOptionsQuery {
  return {
    pageNumber: q.pageNumber ?? 1,
    pageSize: q.pageSize ?? 10,
    sortBy: q.sortBy ?? "CreatedAt",
    sortDescending: q.sortDescending ?? true,
    searchTerm: q.searchTerm ?? null,
    organizationId: q.organizationId ?? null,
    playerId: q.playerId ?? null,
    isActive: q.isActive ?? null,
    isExpired: q.isExpired ?? null,
  };
}

function toParams(q: GetAllPlayerOptionsQuery): HttpParams {
  let p = new HttpParams();
  const add = (k: string, v: unknown) =>
    v === undefined || v === null || v === "" ? p : p.set(k, String(v));

  p = add("pageNumber", q.pageNumber);
  p = add("pageSize", q.pageSize);
  p = add("pearchTerm", q.searchTerm);
  p = add("organizationId", q.organizationId);
  p = add("playerId", q.playerId);
  p = add("isActive", q.isActive);
  p = add("sExpired", q.isExpired);
  p = add("SortBy", q.sortBy);
  p = add("SortDescending", q.sortDescending);
  return p;
}
