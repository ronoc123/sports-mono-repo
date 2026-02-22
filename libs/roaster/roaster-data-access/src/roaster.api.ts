import { Injectable, inject } from "@angular/core";
import { ApiService } from "@sports-ui/http-client";
import { environment } from "@sports-ui/api-types";
import { Observable } from "rxjs";
import { RosterPlayer } from "./roaster.model";
import { ServiceResponse } from "@sports-ui/api-types";

@Injectable({ providedIn: "root" })
export class RoasterApi {
  private readonly http = inject(ApiService);

  getRosterByOrganization(organizationId: string): Observable<ServiceResponse<RosterPlayer[]>> {
    return this.http.get<ServiceResponse<RosterPlayer[]>>(
      `${environment.sportsApi}player/roster?organizationId=${organizationId}`
    );
  }
}
