import { Component, OnInit, inject, signal, computed, DestroyRef, PLATFORM_ID } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgClass, NgIf, SlicePipe, isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@core/services/auth.service';
import { BudgetService } from '@services/budget.service';
import { DepartmentService } from '@services/department.service';
import { ToastService } from '@core/services/toast.service';
import { RefreshService } from '@core/services/refresh.service';
import { StatusBadgeComponent } from '@shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { UserAvatarComponent } from '@shared/components/user-avatar/user-avatar.component';
import { BudgetDto, CreateBudgetDto, UpdateBudgetDto } from '@models/budget.models';
import { DepartmentDto } from '@models/department.models';
import { PagedResult } from '@models/pagination.models';

declare const bootstrap: any;

@Component({
  selector: 'app-budgets-list',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, NgClass, NgIf, SlicePipe, RouterLink, StatusBadgeComponent, PaginationComponent, ConfirmModalComponent, UserAvatarComponent],
  templateUrl: './budgets-list.component.html',
  styleUrl: './budgets-list.component.css'
})

export class BudgetsListComponent implements OnInit {
  private budgetService = inject(BudgetService);
  private deptService = inject(DepartmentService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private refreshService = inject(RefreshService);
  private destroyRef = inject(DestroyRef);
  private fb = inject(FormBuilder);
  private platformId = inject(PLATFORM_ID);

  protected Math = Math;

  loading = signal(true);
  saving = signal(false);
  formError = signal('');
  editMode = signal(false);
  selectedBudget = signal<BudgetDto | null>(null);
  budgetForm!: FormGroup;

  data = signal<PagedResult<BudgetDto>>({ data: [], pageNumber: 1, pageSize: 10, totalRecords: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false, firstPage: 1, lastPage: 1, nextPage: null, previousPage: null, firstItemIndex: 1, lastItemIndex: 0, currentPageItemCount: 0, isFirstPage: true, isLastPage: true });
  departments = signal<DepartmentDto[]>([]);

  currentPage = 1;
  pageSize = 7;
  searchTitle = '';
  searchCode = '';
  filterStatus = '';
  filterDeptId = '';

  role = () => this.authService.userRole();
  isManager = () => this.authService.isManager();
  isAdmin = () => this.authService.isAdmin();

  // KPI computed values (from current page data)
  totalBudgets = computed(() => this.data().totalRecords);
  totalAllocated = computed(() => this.data().data.reduce((sum, b) => sum + b.amountAllocated, 0));
  totalSpent = computed(() => this.data().data.reduce((sum, b) => sum + b.amountSpent, 0));
  totalRemaining = computed(() => this.data().data.reduce((sum, b) => sum + b.amountRemaining, 0));

  // Frontend filtering for Expired/OverBudget/Department (backend doesn't support these)
  filteredData = computed(() => {
    let items = this.data().data;

    // Client-side status filters
    if (this._clientFilter === 'Expired') {
      items = items.filter(b => b.isExpired);
    } else if (this._clientFilter === 'OverBudget') {
      items = items.filter(b => b.isOverBudget);
    }

    // Client-side department filter
    if (this._clientDeptId) {
      items = items.filter(b => b.departmentID === this._clientDeptId);
    }

    return items;
  });

  // Internal tracking for client-side filters (updated on each load)
  private _clientFilter = '';
  private _clientDeptId = 0;

  get f() { return this.budgetForm.controls; }

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    if (this.authService.isEmployee()) {
      this.filterStatus = 'Active';
    }
    this.loadBudgets();
    this.deptService.getDepartments().subscribe(d => this.departments.set(d));
    this.refreshService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadBudgets());
  }

  get deleteBudgetMessage(): string {
    return `Are you sure you want to delete budget "${this.selectedBudget()?.title ?? ''}" ? This cannot be undone.`;
  }

  getDisplayStatus(b: BudgetDto): string {
    if (b.isDeleted) return 'Deleted';
    if (b.isExpired && b.statusName !== 'Closed') return 'Expired';
    if (b.statusName === 'Active' || b.statusName === 'Closed') return b.statusName;
    return b.statusName;
  }

  rowClass(b: BudgetDto): string {
    if (b.isDeleted) return 'row-deleted';
    return '';
  }

  loadBudgets(): void {
    this.loading.set(true);

    // Reset client-side filters
    this._clientFilter = '';
    this._clientDeptId = 0;

    const needsClientFilter =
      this.filterStatus === 'Expired' ||
      this.filterStatus === 'OverBudget' ||
      !!this.filterDeptId;

    const params: any = {
      pageNumber: needsClientFilter ? 1 : this.currentPage,
      pageSize: needsClientFilter ? 500 : this.pageSize,
      ...(this.searchTitle && { title: this.searchTitle }),
      ...(this.searchCode && { code: this.searchCode }),
    };

    // Map filterStatus to API params or client-side filters
    if (this.filterStatus === 'Active') {
      params.status = [1];
      params.isDeleted = false;
    } else if (this.filterStatus === 'Closed') {
      params.status = [2];
      params.isDeleted = false;
    } else if (this.filterStatus === 'Deleted') {
      params.isDeleted = true;
    } else if (this.filterStatus === 'Expired' || this.filterStatus === 'OverBudget') {
      params.isDeleted = false;
      this._clientFilter = this.filterStatus;
    }

    // Department is client-side filtered
    if (this.filterDeptId) {
      this._clientDeptId = +this.filterDeptId;
    }

    const obs = this.authService.isAdmin()
      ? this.budgetService.getAdminBudgets(params)
      : this.budgetService.getBudgets(params);
    obs.subscribe({
      next: (r) => { this.data.set(r); this.loading.set(false); this.refreshService.notifyComplete(); },
      error: () => { this.loading.set(false); this.refreshService.notifyComplete(false); }
    });
  }

  onSearch(): void { this.currentPage = 1; this.loadBudgets(); }
  onPageChange(p: number): void { this.currentPage = p; this.loadBudgets(); }
  onPageSizeChange(size: number): void { this.pageSize = size; this.currentPage = 1; this.loadBudgets(); }
  clearFilters(): void { this.searchTitle = ''; this.searchCode = ''; this.filterStatus = ''; this.filterDeptId = ''; this.currentPage = 1; this.loadBudgets(); }

  openCreate(): void {
    this.editMode.set(false);
    this.formError.set('');

    const user = this.authService.currentUser();
    const deptId = user?.departmentId ?? user?.departmentID ?? '';

    this.budgetForm = this.fb.group({
      title: ['', Validators.required],
      departmentID: [deptId, Validators.required],
      amountAllocated: ['', [Validators.required, Validators.min(1)]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      notes: [''],
    }, { validators: this.dateRangeValidator });
    this.showModal('budgetFormModal');
  }

  openEdit(b: BudgetDto): void {
    this.editMode.set(true);
    this.selectedBudget.set(b);
    this.formError.set('');

    // For managers, department is not editable so validation should not require it
    const deptValidators = this.role() === 'Admin' ? Validators.required : [];

    this.budgetForm = this.fb.group({
      title: [b.title, Validators.required],
      departmentID: [b.departmentID, deptValidators],
      amountAllocated: [b.amountAllocated, [Validators.required, Validators.min(1)]],
      startDate: [b.startDate.substring(0, 10), Validators.required],
      endDate: [b.endDate.substring(0, 10), Validators.required],
      status: [b.status, Validators.required],
      notes: [b.notes ?? ''],
    }, { validators: this.dateRangeValidator });
    this.showModal('budgetFormModal');
  }

  openDelete(b: BudgetDto): void { this.selectedBudget.set(b); }

  onSave(): void {
    this.budgetForm.markAllAsTouched();
    if (this.budgetForm.invalid) return;
    this.saving.set(true);
    this.formError.set('');

    let v = this.budgetForm.value;

    // Ensure departmentID has a valid value
    if (!v.departmentID || v.departmentID <= 0) {
      if (this.selectedBudget()) {
        v.departmentID = this.selectedBudget()!.departmentID;
      }
    }

    // Ensure status is a number
    if (v.status) {
      v.status = Number(v.status);
    }

    // Clean up optional fields - convert empty strings to null
    if (!v.notes || v.notes.trim() === '') {
      v.notes = null;
    }

    // For edit mode, only Status is strictly required
    // For create mode, Title, AmountAllocated, StartDate, EndDate are required
    if (this.editMode()) {
      if (v.status === null || v.status === undefined) {
        this.formError.set('Status is required.');
        this.saving.set(false);
        return;
      }
    }

    const onNext = () => {
      this.saving.set(false);
      this.toastService.success(this.editMode() ? 'Budget updated successfully.' : 'Budget created successfully.');
      this.hideModal('budgetFormModal');
      this.loadBudgets();
    };
    const onError = (err: any) => {
      this.saving.set(false);
      console.error('Budget update error:', err);

      if (err?.status === 403) {
        this.formError.set('You can only update budgets you created.');
        this.toastService.error('You can only update budgets you created.');
      } else if (err?.status === 409) {
        this.formError.set(err?.error?.message ?? 'A budget with this title already exists.');
      } else if (err?.status === 400) {
        // Extract validation errors from ModelState if available
        const modelErrors = err?.error?.errors;
        if (modelErrors) {
          const firstError = Object.values(modelErrors)[0];
          this.formError.set(Array.isArray(firstError) ? firstError[0] : (firstError as string));
        } else {
          this.formError.set(err?.error?.message ?? 'Invalid input. Please check your entries.');
        }
      } else if (err?.status === 404) {
        this.formError.set(err?.error?.message ?? 'Budget not found.');
      } else {
        this.formError.set(err?.error?.message ?? 'An error occurred while updating the budget. Please try again.');
      }
    };

    if (this.editMode()) {
      this.budgetService.updateBudget(this.selectedBudget()!.budgetID, v as UpdateBudgetDto).subscribe({ next: onNext, error: onError });
    } else {
      this.budgetService.createBudget(v as CreateBudgetDto).subscribe({ next: onNext, error: onError });
    }
  }

  onDelete(): void {
    const b = this.selectedBudget();
    if (!b) return;
    this.budgetService.deleteBudget(b.budgetID).subscribe({
      next: () => { this.toastService.success('Budget deleted.'); this.loadBudgets(); },
      error: (err) => this.toastService.error(err?.error?.message ?? 'Delete failed.')
    });
  }

  formatAmt(v: number): string {
    if (v >= 10_000_000) return `\u20B9${(v / 10_000_000).toFixed(2)} Cr`;
    if (v >= 100_000) return `\u20B9${(v / 100_000).toFixed(2)} L`;
    return `\u20B9${v.toLocaleString('en-IN')} `;
  }

  utilClass(p: number): string { return p >= 80 ? 'high' : p >= 50 ? 'medium' : 'low'; }
  utilColor(p: number): string {
    if (p > 100) return 'var(--danger)';
    if (p >= 80) return 'var(--warning)';
    return 'var(--success)';
  }

  private dateRangeValidator(group: any): { [key: string]: any } | null {
    const start = group.get('startDate')?.value;
    const end = group.get('endDate')?.value;
    if (start && end && new Date(start) >= new Date(end)) {
      return { dateRange: true };
    }
    return null;
  }

  private showModal(id: string): void { const m = new bootstrap.Modal(document.getElementById(id)); m.show(); }
  private hideModal(id: string): void { bootstrap.Modal.getInstance(document.getElementById(id))?.hide(); }
}