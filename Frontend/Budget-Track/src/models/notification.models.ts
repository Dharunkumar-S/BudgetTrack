// Notification type enum
export enum NotificationType {
    ExpenseApprovalReminder = 1,
    ExpenseApproved = 2,
    ExpenseRejected = 3,
    BudgetCreated = 4,
    BudgetUpdated = 5,
    BudgetDeleted = 6,
}

export interface NotificationDto {
    notificationID: number;
    type: number;
    message: string;
    createdDate: string;
    senderEmployeeID: string;
    senderName: string;
    isRead: boolean;
}

export interface MarkAllReadResponse {
    count: number;
    message: string;
}

export interface NotificationListParams {
    message?: string;
    status?: 'all' | 'read' | 'unread';
    sortOrder?: 'asc' | 'desc';
    pageNumber?: number;
    pageSize?: number;
}
