import { inject, Injectable } from "@angular/core";
import { ApiService } from "@sports-ui/http-client";
import {
  environment,
  PaginatedList,
  ServiceResponse,
} from "@sports-ui/api-types";
import { Observable } from "rxjs";
import {
  GetAllNotificationsQuery,
  NotificationDto,
} from "./notification.model";
import { HttpParams } from "@angular/common/http";

@Injectable({ providedIn: "root" })
export class NotificationApi {
  private readonly http = inject(ApiService);
  private readonly base = `${environment.sportsApi}Notification`;

  // -----------------------------------------------------
  // GET: All notifications
  // -----------------------------------------------------
  getAll(
    query: GetAllNotificationsQuery
  ): Observable<ServiceResponse<PaginatedList<NotificationDto>>> {
    if (!query.userId) {
      throw new Error("[Notifications] userId is required");
    }

    const params = new HttpParams()
      .set("userId", query.userId)
      .set("pageNumber", query.pageNumber.toString())
      .set("pageSize", query.pageSize.toString());

    return this.http.get<ServiceResponse<PaginatedList<NotificationDto>>>(
      `${this.base}/get-all`,
      params
    );
  }

  markAsRead(notificationId: string) {
    return this.http.post<ServiceResponse<boolean>>(
      `${this.base}/${notificationId}/read`
    );
  }
}
