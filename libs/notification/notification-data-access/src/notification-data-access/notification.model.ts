export interface NotificationDto {
  notificationId: string;
  userId: string;
  organizationId: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
}

export interface GetAllNotificationsQuery {
  userId: string;
  pageNumber: number;
  pageSize: number;
}
