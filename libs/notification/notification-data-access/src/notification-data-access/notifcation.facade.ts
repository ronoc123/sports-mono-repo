import { Injectable, inject } from "@angular/core";
import { firstValueFrom } from "rxjs";

import { NotificationStore } from "./notification.store";
import { NotificationApi } from "./notification-data-access";
import { GetAllNotificationsQuery } from "./notification.model";

@Injectable({ providedIn: "root" })
export class NotificationFacade {
  private readonly store = inject(NotificationStore);
  private readonly api = inject(NotificationApi);

  // selectors
  readonly notifications = this.store.notifications;
  readonly unreadCount = this.store.unreadCount;
  readonly status = this.store.status;
  readonly error = this.store.error;

  async loadNotifications(query: GetAllNotificationsQuery) {
    if (!query.userId) {
      throw new Error("[Notifications] userId is required");
    }

    this.store.setLoading();

    try {
      const res = await firstValueFrom(this.api.getAll(query));
      const items = res.data?.items ?? [];
      this.store.setNotifications(items);
    } catch (e: any) {
      this.store.setError(e?.message ?? "Failed to load notifications");
    }
  }

  async markAsRead(notificationId: string) {
    console.log("Marking notification as read:", notificationId);
    try {
      await firstValueFrom(this.api.markAsRead(notificationId));
      this.store.markAsRead(notificationId);
    } catch (e) {
      console.error("Failed to mark notification as read", e);
    }
  }
}
