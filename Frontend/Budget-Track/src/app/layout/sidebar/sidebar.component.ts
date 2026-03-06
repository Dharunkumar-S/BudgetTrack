import { Component, Input, Output, EventEmitter, computed, inject, OnInit, signal, effect } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';

import { AuthService } from '@core/services/auth.service';
import { NotificationService } from '@services/notification.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  roles?: string[];
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', icon: 'fas fa-chart-line', route: '/dashboard' },
  { label: 'Budgets', icon: 'fas fa-wallet', route: '/budgets' },
  { label: 'Expenses', icon: 'fas fa-receipt', route: '/expenses' },
  { label: 'Categories', icon: 'fas fa-tags', route: '/categories', roles: ['Admin', 'Manager'] },
  { label: 'Departments', icon: 'fas fa-building', route: '/departments', roles: ['Admin', 'Manager'] },
  { label: 'Reports', icon: 'fas fa-chart-bar', route: '/reports', roles: ['Admin', 'Manager'] },
  { label: 'Users', icon: 'fas fa-users', route: '/users', roles: ['Admin', 'Manager'] },
  { label: 'Audit Logs', icon: 'fas fa-history', route: '/audits', roles: ['Admin'] },
  { label: 'Notifications', icon: 'fas fa-bell', route: '/notifications', roles: ['Manager', 'Employee'] },
];

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent implements OnInit {
  @Input() collapsed = false;
  @Output() toggleCollapse = new EventEmitter<void>();

  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);

  unreadCount = toSignal(this.notificationService.unreadCount$, { initialValue: 0 });

  ngOnInit(): void {
    this.notificationService.loadUnreadCount();
  }

  visibleNavItems = computed(() => {
    const role = this.authService.userRole();
    return NAV_ITEMS.filter(item => {
      if (!item.roles) return true; // visible to all
      return role ? item.roles.includes(role) : false;
    });
  });

  userName = computed(() => {
    const u = this.authService.currentUser();
    return u ? `${u.firstName} ${u.lastName}` : '';
  });

  userRole = computed(() => this.authService.userRole() ?? '');

  userInitials = computed(() => {
    const u = this.authService.currentUser();
    if (!u) return 'U';
    return `${u.firstName.charAt(0)}${u.lastName.charAt(0)}`.toUpperCase();
  });
}
