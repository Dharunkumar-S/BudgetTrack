import { Component, OnInit, inject, signal, computed, DestroyRef, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { toIST } from '@shared/pipes/ist-date.pipe';
import { NotificationService } from '@services/notification.service';
import { ToastService } from '@core/services/toast.service';
import { RefreshService } from '@core/services/refresh.service';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { NotificationDto, NotificationListParams } from '@models/notification.models';
import { PagedResult } from '@models/pagination.models';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [PaginationComponent, FormsModule, ConfirmModalComponent],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css'
})
export class NotificationsComponent implements OnInit {
  private notifService = inject(NotificationService);
  private toast = inject(ToastService);
  private refreshService = inject(RefreshService);
  private destroyRef = inject(DestroyRef);
  private platformId = inject(PLATFORM_ID);

  loading = signal(true);
  markingAll = signal(false);
  deletingAll = signal(false);
  data = signal<PagedResult<NotificationDto>>({
    data: [], pageNumber: 1, pageSize: 10, totalRecords: 0, totalPages: 0,
    hasNextPage: false, hasPreviousPage: false, firstPage: 1, lastPage: 1,
    nextPage: null, previousPage: null, firstItemIndex: 1, lastItemIndex: 0,
    currentPageItemCount: 0, isFirstPage: true, isLastPage: true
  });

  currentPage = 1;
  pageSize = 10;
  searchTerm = signal('');
  statusFilter = signal<'all' | 'read' | 'unread'>('all');

  // KPI computed values
  readCount = computed(() => {
    const total = this.data().totalRecords;
    const unread = this.unreadCount();
    return Math.max(0, total - unread);
  });

  unreadCount = computed(() => {
    return this.data().data.filter(n => !n.isRead).length;
  });

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.load();
    this.refreshService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  load(): void {
    this.loading.set(true);
    const params: NotificationListParams = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      message: this.searchTerm() || undefined,
      status: this.statusFilter() === 'all' ? undefined : this.statusFilter(),
    };
    this.notifService.getNotifications(params).subscribe({
      next: r => { this.data.set(r); this.loading.set(false); this.refreshService.notifyComplete(); },
      error: () => { this.loading.set(false); this.refreshService.notifyComplete(false); }
    });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.currentPage = 1;
    this.load();
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.statusFilter.set('all');
    this.currentPage = 1;
    this.load();
  }

  setStatusFilter(status: 'all' | 'read' | 'unread'): void {
    this.statusFilter.set(status);
    this.currentPage = 1;
    this.load();
  }

  onPageChange(p: number): void { this.currentPage = p; this.load(); }
  onPageSizeChange(s: number): void { this.pageSize = s; this.currentPage = 1; this.load(); }

  markRead(id: number, notification: NotificationDto): void {
    this.notifService.markAsRead(id).subscribe({
      next: () => {
        // Update the notification in place to show as read
        const current = this.data();
        const updatedData = current.data.map(n =>
          n.notificationID === id ? { ...n, isRead: true } : n
        );
        this.data.set({ ...current, data: updatedData });
        this.notifService.notifyRefresh();
      },
      error: () => { }
    });
  }

  deleteNotification(id: number): void {
    this.notifService.deleteNotification(id).subscribe({
      next: () => {
        // Remove the notification from the list
        const current = this.data();
        const nextData = current.data.filter(n => n.notificationID !== id);
        this.data.set({
          ...current,
          data: nextData,
          totalRecords: Math.max(0, current.totalRecords - 1),
        });
        this.notifService.notifyRefresh();
        this.toast.success('Notification deleted.');
      },
      error: () => { this.toast.error('Failed to delete notification.'); }
    });
  }

  markAll(): void {
    this.markingAll.set(true);
    this.notifService.markAllAsRead().subscribe({
      next: r => {
        this.markingAll.set(false);
        this.toast.success(`${r.count} notifications marked as read.`);
        this.load();
        this.notifService.notifyRefresh();
      },
      error: () => { this.markingAll.set(false); this.toast.error('Error marking notifications.'); }
    });
  }

  deleteAll(): void {
    this.deletingAll.set(true);
    this.notifService.deleteAllNotifications().subscribe({
      next: r => {
        this.deletingAll.set(false);
        this.toast.success(`${r.count} notifications deleted.`);
        this.currentPage = 1;
        this.load();
        this.notifService.notifyRefresh();
      },
      error: () => {
        this.deletingAll.set(false);
        this.toast.error('Failed to delete all notifications.');
      }
    });
  }

  /** Returns relative time string like "2h ago", "1 day ago", "just now" */
  timeAgo(date: string): string {
    const now = new Date();
    const then = new Date(date);
    const diffMs = now.getTime() - then.getTime();
    const mins = Math.floor(diffMs / 60_000);
    if (mins < 1) return 'just now';
    if (mins < 60) return `${mins}m ago`;
    const hours = Math.floor(mins / 60);
    if (hours < 24) return `${hours}h ago`;
    const days = Math.floor(hours / 24);
    if (days < 7) return `${days} day${days > 1 ? 's' : ''} ago`;
    const weeks = Math.floor(days / 7);
    if (weeks < 5) return `${weeks} week${weeks > 1 ? 's' : ''} ago`;
    const months = Math.floor(days / 30);
    return `${months} month${months > 1 ? 's' : ''} ago`;
  }

  formatDateTime(date: string): string {
    return toIST(date, 'datetime');
  }

  getIcon(type: number): string {
    const map: Record<number, string> = {
      1: 'fas fa-clock', 2: 'fas fa-check-circle', 3: 'fas fa-times-circle',
      4: 'fas fa-wallet', 5: 'fas fa-pen', 6: 'fas fa-trash'
    };
    return map[type] ?? 'fas fa-bell';
  }

  getIconBg(type: number): string {
    if (type === 2) return '#dcfce7'; // Green bg (Read/Approved)
    if (type === 4) return '#e0e7ff'; // Indigo bg (Wallet)
    if (type === 5) return '#dbeafe'; // Blue bg (Update/Edit)
    if (type === 3 || type === 6) return 'var(--danger-bg)';
    return 'var(--primary-light)';
  }

  getIconColor(type: number): string {
    if (type === 2) return '#166534'; // Green text
    if (type === 4) return '#4338ca'; // Indigo text
    if (type === 5) return '#1d4ed8'; // Blue text
    if (type === 3 || type === 6) return 'var(--danger)';
    return 'var(--primary)';
  }
}
