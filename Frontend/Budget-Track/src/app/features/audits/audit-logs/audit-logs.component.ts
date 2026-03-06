import { Component, OnInit, inject, signal, computed, DestroyRef, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuditService } from '@services/audit.service';
import { toIST } from '@shared/pipes/ist-date.pipe';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { UserAvatarComponent } from '@shared/components/user-avatar/user-avatar.component';
import { AuditLogDto } from '@models/audit.models';
import { PagedResult } from '@models/pagination.models';
import { RefreshService } from '@core/services/refresh.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [FormsModule, PaginationComponent, UserAvatarComponent],
  templateUrl: './audit-logs.component.html',
  styleUrl: './audit-logs.component.css'
})
export class AuditLogsComponent implements OnInit {
  private auditService = inject(AuditService);
  private refreshService = inject(RefreshService);
  private destroyRef = inject(DestroyRef);
  private platformId = inject(PLATFORM_ID);

  loading = signal(true);
  data = signal<PagedResult<AuditLogDto>>({
    data: [], pageNumber: 1, pageSize: 10, totalRecords: 0, totalPages: 0,
    hasNextPage: false, hasPreviousPage: false, firstPage: 1, lastPage: 1,
    nextPage: null, previousPage: null, firstItemIndex: 1, lastItemIndex: 0,
    currentPageItemCount: 0, isFirstPage: true, isLastPage: true
  });

  currentPage = 1;
  pageSize = 10;
  searchQuery = '';
  filterAction = '';
  filterModule = '';

  // KPI computed from current page data
  createCount = computed(() => this.data().data.filter(l => l.action === 'Create').length);
  updateCount = computed(() => this.data().data.filter(l => l.action === 'Update').length);
  deleteCount = computed(() => this.data().data.filter(l => l.action === 'Delete').length);

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.load();
    this.refreshService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  load(): void {
    this.loading.set(true);
    this.auditService.getAuditLogs(
      this.currentPage, this.pageSize,
      this.searchQuery || undefined,
      this.filterAction || undefined,
      this.filterModule || undefined
    ).subscribe({
      next: r => { this.data.set(r); this.loading.set(false); this.refreshService.notifyComplete(); },
      error: () => { this.loading.set(false); this.refreshService.notifyComplete(false); }
    });
  }

  onSearch(): void { this.currentPage = 1; this.load(); }
  onPageChange(p: number): void { this.currentPage = p; this.load(); }
  onPageSizeChange(s: number): void { this.pageSize = s; this.currentPage = 1; this.load(); }

  clearFilters(): void {
    this.searchQuery = '';
    this.filterAction = '';
    this.filterModule = '';
    this.pageSize = 10;
    this.currentPage = 1;
    this.load();
  }

  formatDate(ts: string): string {
    return toIST(ts, 'date');
  }

  formatTime(ts: string): string {
    return toIST(ts, 'time');
  }
}