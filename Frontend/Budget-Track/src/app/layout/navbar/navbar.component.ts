import { Component, Output, EventEmitter, inject, computed, effect, signal, DestroyRef, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterLink, Router, NavigationEnd } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { NotificationService } from '@services/notification.service';
import { RefreshService } from '@core/services/refresh.service';
import { ToastService } from '@core/services/toast.service';
import { UserAvatarComponent } from '@shared/components/user-avatar/user-avatar.component';
import { toSignal, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs';

const ROUTE_LABELS: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/budgets': 'Budgets',
  '/expenses': 'Expenses',
  '/categories': 'Categories',
  '/departments': 'Departments',
  '/reports': 'Reports & Analytics',
  '/users': 'User Management',
  '/audits': 'Audit Logs',
  '/notifications': 'Notifications',
  '/profile': 'Profile & Settings',
};

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, UserAvatarComponent],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  @Output() toggleSidebar = new EventEmitter<void>();

  public authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private refreshService = inject(RefreshService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);

  refreshState = signal<'idle' | 'spinning'>('idle');
  unreadCount = signal(0);
  private previousUnreadCount = -1; // -1 = not yet initialized
  private stoppingTimer: ReturnType<typeof setTimeout> | null = null;
  private spinStartTime = 0;
  private readonly destroyRef = inject(DestroyRef);
  private refreshTick = signal(0);

  private currentUrl = toSignal(
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd),
      map(e => e.urlAfterRedirects),
      startWith(this.router.url)
    )
  );

  pageLabel = computed(() => {
    const url = this.currentUrl() ?? '/dashboard';
    const match = Object.keys(ROUTE_LABELS).find(key => url.startsWith(key));
    return match ? ROUTE_LABELS[match] : 'Dashboard';
  });

  fullName = computed(() => {
    const u = this.authService.currentUser();
    return u ? `${u.firstName} ${u.lastName}` : 'User';
  });

  userRole = computed(() => this.authService.userRole() ?? '');

  showNotifications = computed(() => this.authService.hasRole('Manager', 'Employee'));
  hasUnread = computed(() => this.unreadCount() > 0);

  constructor() {
    // Subscribe to the dedicated unread count stream
    this.notificationService.unreadCount$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((newCount) => {
        if (!this.showNotifications() || !isPlatformBrowser(this.platformId)) return;

        if (this.previousUnreadCount === -1 && newCount > 0) {
          // First check after login / page refresh
          this.toastService.info(
            `You have ${newCount} unread notification${newCount > 1 ? 's' : ''}!`
          );
        } else if (this.previousUnreadCount !== -1 && newCount > this.previousUnreadCount) {
          // New notifications arrived since last check
          const diff = newCount - this.previousUnreadCount;
          this.toastService.info(
            `You have ${diff} new notification${diff > 1 ? 's' : ''}!`
          );
        }

        this.previousUnreadCount = newCount;
        this.unreadCount.set(newCount);
      });

    this.notificationService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.refreshTick.update(v => v + 1);
      });

    this.refreshService.refreshComplete$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((success) => {
        const elapsed = Date.now() - this.spinStartTime;
        const wait = Math.max(0, 700 - elapsed);
        if (this.stoppingTimer) { clearTimeout(this.stoppingTimer); this.stoppingTimer = null; }
        this.stoppingTimer = setTimeout(() => {
          this.stoppingTimer = null;
          this.refreshState.set('idle');
          if (success) this.toastService.success('Successfully refreshed.', 500);
        }, wait);
      });

    effect(() => {
      this.currentUrl();
      this.refreshTick();
      if (!this.showNotifications() || !isPlatformBrowser(this.platformId)) {
        this.unreadCount.set(0);
        this.previousUnreadCount = -1;
        return;
      }
      this.notificationService.loadUnreadCount();
    });
  }

  onLogout(): void {
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login'])
    });
  }

  onRefresh(event: Event): void {
    event.stopPropagation();
    if (this.stoppingTimer) { clearTimeout(this.stoppingTimer); this.stoppingTimer = null; }
    this.spinStartTime = Date.now();
    this.refreshState.set('spinning');
    this.refreshTick.update(v => v + 1);
    this.refreshService.notifyRefresh();
  }
}
