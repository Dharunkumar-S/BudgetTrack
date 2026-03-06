import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { environment } from '@env/environment';
import { NotificationDto, NotificationListParams, MarkAllReadResponse } from '@models/notification.models';
import { PagedResult } from '@models/pagination.models';
import { ApiResponse } from '@models/auth.models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl;
    private refreshSubject = new Subject<void>();
    refresh$ = this.refreshSubject.asObservable();

    private unreadCountSubject = new BehaviorSubject<number>(0);
    unreadCount$ = this.unreadCountSubject.asObservable();

    loadUnreadCount(): void {
        this.http.get<{ count: number }>(`${this.apiUrl}/api/notifications/unread-count`)
            .subscribe({ next: res => this.unreadCountSubject.next(res.count), error: () => this.unreadCountSubject.next(0) });
    }

    getNotifications(params: NotificationListParams = {}): Observable<PagedResult<NotificationDto>> {
        let p = new HttpParams();
        if (params.message) p = p.set('message', params.message);
        if (params.status && params.status !== 'all') p = p.set('status', params.status);
        if (params.sortOrder) p = p.set('sortOrder', params.sortOrder);
        if (params.pageNumber) p = p.set('pageNumber', params.pageNumber.toString());
        if (params.pageSize) p = p.set('pageSize', params.pageSize.toString());
        return this.http.get<PagedResult<NotificationDto>>(`${this.apiUrl}/api/notifications`, { params: p });
    }

    markAsRead(notificationId: number): Observable<ApiResponse> {
        return this.http.put<ApiResponse>(`${this.apiUrl}/api/notifications/read/${notificationId}`, {});
    }

    markAllAsRead(): Observable<MarkAllReadResponse> {
        return this.http.put<MarkAllReadResponse>(`${this.apiUrl}/api/notifications/readAll`, {});
    }

    deleteNotification(notificationId: number): Observable<ApiResponse> {
        return this.http.delete<ApiResponse>(`${this.apiUrl}/api/notifications/${notificationId}`);
    }

    deleteAllNotifications(): Observable<{ count: number; message: string }> {
        return this.http.delete<{ count: number; message: string }>(`${this.apiUrl}/api/notifications/deleteAll`);
    }

    notifyRefresh(): void {
        this.refreshSubject.next();
        this.loadUnreadCount();
    }
}
