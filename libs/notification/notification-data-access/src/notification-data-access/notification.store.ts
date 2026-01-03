import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { NotificationDto } from "./notification.model";

type Status = "idle" | "loading" | "error" | "success";

interface NotificationState {
  status: Status;
  error?: string;
  notifications: NotificationDto[];
  unreadCount: number;
}

export const NotificationStore = signalStore(
  withState<NotificationState>({
    status: "idle",
    notifications: [],
    unreadCount: 0,
    error: undefined,
  }),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: "loading", error: undefined });
    },

    setError(msg: string) {
      patchState(store, { status: "error", error: msg });
    },

    setNotifications(items: NotificationDto[]) {
      patchState(store, {
        notifications: items,
        unreadCount: items.filter((n) => !n.isRead).length,
        status: "success",
      });
    },

    markAsRead(notificationId: string) {
      const updated = store
        .notifications()
        .map((n) =>
          n.notificationId === notificationId
            ? { ...n, isRead: true, readAt: new Date().toISOString() }
            : n
        );

      patchState(store, {
        notifications: updated,
        unreadCount: updated.filter((n) => !n.isRead).length,
      });
    },
  }))
);
