import { Injectable, inject } from "@angular/core";
import { HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { ApiService } from "@sports-ui/http-client";
import { ServiceResponse, environment } from "@sports-ui/api-types";
import { ProductDto, CreatePurchaseRequest, CreatePurchaseResponse } from "./store.model";

@Injectable({ providedIn: "root" })
export class StoreApi {
  private readonly http = inject(ApiService);

  getBundles(organizationId: string): Observable<ServiceResponse<ProductDto[]>> {
    const params = new HttpParams().set("organizationId", organizationId);
    return this.http.get(`${environment.sportsApi}Store/bundles`, params);
  }

  createPurchase(body: CreatePurchaseRequest): Observable<ServiceResponse<CreatePurchaseResponse>> {
    return this.http.post(`${environment.sportsApi}Store/purchases`, body);
  }
}
