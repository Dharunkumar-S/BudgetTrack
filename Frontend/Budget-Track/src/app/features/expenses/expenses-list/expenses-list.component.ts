import { Component, OnInit, inject, signal, computed, DestroyRef, PLATFORM_ID } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgIf, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@core/services/auth.service';
import { ExpenseService } from '@services/expense.service';
import { CategoryService } from '@services/category.service';
import { BudgetService } from '@services/budget.service';
import { UserService } from '@services/user.service';
import { ToastService } from '@core/services/toast.service';
import { RefreshService } from '@core/services/refresh.service';
import { StatusBadgeComponent } from '@shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { UserAvatarComponent } from '@shared/components/user-avatar/user-avatar.component';
import { toIST } from '@shared/pipes/ist-date.pipe';
import { ExpenseDetailDto, CreateExpenseDto, AllExpenseDto, ExpenseStatisticsDto, AllExpenseListParams } from '@models/expense.models';
import { CategoryDto } from '@models/category.models';
import { BudgetDto } from '@models/budget.models';
import { UserDto } from '@models/user.models';
import { PagedResult } from '@models/pagination.models';

declare const bootstrap: any;

@Component({
  selector: 'app-expenses-list',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, NgIf, StatusBadgeComponent, PaginationComponent, UserAvatarComponent],
  templateUrl: './expenses-list.component.html',
  styleUrl: './expenses-list.component.css'
})
export class ExpensesListComponent implements OnInit {
  private expenseService = inject(ExpenseService);
  private categoryService = inject(CategoryService);
  private budgetService = inject(BudgetService);
  private userService = inject(UserService);
  public authService = inject(AuthService);
  private toastService = inject(ToastService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private refreshService = inject(RefreshService);
  private destroyRef = inject(DestroyRef);
  private platformId = inject(PLATFORM_ID);

  loading = signal(true);
  saving = signal(false);
  formError = signal('');
  accessDenied = signal(false);
  budgetId = signal<number>(0);
  categories = signal<CategoryDto[]>([]);
  budgets = signal<BudgetDto[]>([]);
  allBudgetsMap = signal<Map<number, BudgetDto>>(new Map());
  managedEmployees = signal<UserDto[]>([]);
  selectedExpense = signal<AllExpenseDto | null>(null);
  approvalAction = signal<'approve' | 'reject'>('approve');

  approvalBudgetOverrun = computed(() => {
    const expense = this.selectedExpense();
    if (!expense || this.approvalAction() !== 'approve') return null;
    const budget = this.allBudgetsMap().get(expense.budgetID);
    if (!budget) return null;
    if (expense.amount > budget.amountRemaining) {
      return {
        remaining: budget.amountRemaining,
        excess: expense.amount - budget.amountRemaining,
        allocated: budget.amountAllocated
      };
    }
    return null;
  });
  stats = signal<ExpenseStatisticsDto>({
    totalCount: 0,
    pendingCount: 0,
    approvedCount: 0,
    rejectedCount: 0,
    totalAmount: 0
  });

  expenseForm: FormGroup | null = null;
  statusForm: FormGroup | null = null;

  data = signal<PagedResult<AllExpenseDto>>({
    data: [], pageNumber: 1, pageSize: 10, totalRecords: 0, totalPages: 0,
    hasNextPage: false, hasPreviousPage: false, firstPage: 1, lastPage: 1,
    nextPage: null, previousPage: null, firstItemIndex: 1, lastItemIndex: 0,
    currentPageItemCount: 0, isFirstPage: true, isLastPage: true
  });

  currentPage = 1;
  pageSize = 10;
  searchQuery = '';
  filterStatus = '';
  filterCategoryId = '';
  filterEmployee = '';
  myExpensesOnly = false;

  isManager = () => this.authService.isManager();
  canCreate = () => this.authService.isEmployee() && !this.authService.isManager() && !this.authService.isAdmin();

  get f() { return this.expenseForm?.controls; }
  get sf() { return this.statusForm?.controls; }

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = params.get('id');
        this.budgetId.set(id ? +id : 0);
        this.loadExpenses();
      });

    this.categoryService.getCategories().subscribe(c => this.categories.set(c.filter(x => x.isActive)));
    this.budgetService.getBudgets({ pageNumber: 1, pageSize: 1000, isDeleted: false }).subscribe(r => {
      const map = new Map<number, BudgetDto>();
      r.data.forEach(b => map.set(b.budgetID, b));
      this.allBudgetsMap.set(map);
      this.budgets.set(r.data.filter(b => b.status === 1 && !b.isExpired));
    });
    this.fetchManagedEmployees();

    this.refreshService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.fetchManagedEmployees();
        this.loadExpenses();
      });
  }

  fetchManagedEmployees(): void {
    const user = this.authService.currentUser();
    const userId = user?.userId || user?.id;
    if (!userId) return;

    if (this.authService.isAdmin()) {
      // Admins see only employees in the dropdown
      this.userService.getUsers({ pageNumber: 1, pageSize: 1000, roleId: 3 }).subscribe({
        next: (res) => this.managedEmployees.set(res.data),
        error: (err) => console.error('Error fetching employees for admin', err)
      });
    } else if (this.authService.isManager() || this.authService.isEmployee()) {
      const isManager = this.authService.isManager();
      const targetManagerId = isManager ? userId : (user.managerId || user.managerID);

      if (targetManagerId) {
        this.userService.getEmployeesByManager(targetManagerId).subscribe({
          next: (emps) => {
            // Include self in the list if not already there
            const hasSelf = emps.some(e => e.userId === userId || e.employeeId === user.employeeId);
            let list = [...emps];
            if (!hasSelf && user) {
              list.unshift({
                userId: userId,
                firstName: user.firstName,
                lastName: user.lastName,
                employeeId: user.employeeId,
                roleName: user.roleName,
                departmentName: user.departmentName
              } as UserDto);
            }
            this.managedEmployees.set(list);
          },
          error: (err) => console.error('Error fetching team for dropdown', err)
        });
      } else {
        // Fallback: just own name if no manager
        this.managedEmployees.set([{
          userId: userId,
          firstName: user.firstName,
          lastName: user.lastName,
          employeeId: user.employeeId,
          roleName: user.roleName,
          departmentName: user.departmentName
        } as UserDto]);
      }
    }
  }

  loadExpenses(): void {
    this.loading.set(true);
    const budgetId = this.budgetId();

    const categoryName = this.filterCategoryId
      ? this.categories().find(c => c.categoryID === +this.filterCategoryId)?.categoryName
      : undefined;

    const statsParams: Partial<AllExpenseListParams> = {
      ...(budgetId > 0 && { budgetId }),
      ...(this.searchQuery && { title: this.searchQuery }),
      ...(this.filterEmployee && (this.managedEmployees().length > 0 ? { submittedByEmployeeID: this.filterEmployee } : { submittedUserName: this.filterEmployee })),
      // Status is intentionally excluded from statsParams so KPI cards show all counts
      ...(categoryName && { categoryName }),
      ...(this.myExpensesOnly && { myExpensesOnly: true }),
    };

    this.loadStats(statsParams);

    if (budgetId === 0) {
      const filters = {
        pageNumber: this.currentPage,
        pageSize: this.pageSize,
        sortBy: 'SubmittedDate',
        sortOrder: 'desc' as 'desc' | 'asc',
        ...statsParams,
        ...(this.filterStatus && { status: this.filterStatus }),
      };

      const obs = (this.authService.isManager() || this.authService.isEmployee())
        ? this.expenseService.getManagedExpenses(filters as any)
        : this.expenseService.getAllExpenses(filters as any);

      obs.subscribe({
        next: r => { this.data.set(r as PagedResult<AllExpenseDto>); this.loading.set(false); this.refreshService.notifyComplete(); },
        error: () => { this.loading.set(false); this.refreshService.notifyComplete(false); }
      });
      return;
    }

    // Budget-scoped view (/budgets/:id/expenses route)
    this.expenseService.getExpensesByBudget(budgetId, {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      ...(this.filterStatus && { status: this.filterStatus }),
      ...(categoryName && { categoryName }),
      ...(this.searchQuery && { title: this.searchQuery }),
    }).subscribe({
      next: r => { this.accessDenied.set(false); this.data.set(r as unknown as PagedResult<AllExpenseDto>); this.loading.set(false); this.refreshService.notifyComplete(); },
      error: (err) => {
        this.loading.set(false);
        this.refreshService.notifyComplete(false);
        if (err?.status === 403) {
          this.accessDenied.set(true);
        }
      }
    });
  }

  loadStats(params: Partial<AllExpenseListParams>): void {
    this.expenseService.getExpenseStatistics(params).subscribe({
      next: (s) => this.stats.set(s),
      error: (err) => console.error('Error loading expense stats', err)
    });
  }

  onSearch(): void { this.currentPage = 1; this.loadExpenses(); }

  formatAmt(v: number): string {
    if (v >= 10_000_000) return `\u20B9${(v / 10_000_000).toFixed(2)}Cr`;
    if (v >= 100_000) return `\u20B9${(v / 100_000).toFixed(2)}L`;
    return `\u20B9${v.toLocaleString('en-IN')}`;
  }

  onPageChange(p: number): void { this.currentPage = p; this.loadExpenses(); }
  onPageSizeChange(s: number): void { this.pageSize = s; this.currentPage = 1; this.loadExpenses(); }

  clearFilters(): void {
    this.searchQuery = '';
    this.filterStatus = '';
    this.filterCategoryId = '';
    this.filterEmployee = '';
    this.myExpensesOnly = false;
    this.pageSize = 10;
    this.currentPage = 1;
    this.loadExpenses();
  }

  openCreate(): void {
    this.formError.set('');
    this.expenseForm = this.fb.group({
      budgetId: [this.budgetId() || null, Validators.required],
      categoryId: [null, Validators.required],
      title: ['', Validators.required],
      amount: ['', [Validators.required, Validators.min(1)]],
      merchantName: [''],
      notes: [''],
    }, { validators: this.validateExpenseSelection.bind(this) });
    new bootstrap.Modal(document.getElementById('expenseFormModal')).show();
  }

  private validateExpenseSelection(group: any): { [key: string]: any } | null {
    const budgetId = group.get('budgetId')?.value;
    const categoryId = group.get('categoryId')?.value;

    if (budgetId != null && budgetId !== '') {
      const budget = this.budgets().find(b => b.budgetID === +budgetId);
      if (budget) {
        if (budget.status !== 1) return { budgetInactive: true };
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const end = new Date(budget.endDate);
        if (end < today) return { budgetExpired: true };
      }
    }

    if (categoryId != null && categoryId !== '') {
      const category = this.categories().find(c => c.categoryID === +categoryId);
      if (category && !category.isActive) return { categoryInactive: true };
    }

    return null;
  }

  openApprove(e: AllExpenseDto): void {
    this.selectedExpense.set(e);
    this.approvalAction.set('approve');
    this.statusForm = this.fb.group({ comments: [''] });
    new bootstrap.Modal(document.getElementById('statusModal')).show();
  }

  openReject(e: AllExpenseDto): void {
    this.selectedExpense.set(e);
    this.approvalAction.set('reject');
    this.statusForm = this.fb.group({ reason: ['', Validators.required] });
    new bootstrap.Modal(document.getElementById('statusModal')).show();
  }

  onSaveExpense(): void {
    this.expenseForm!.markAllAsTouched();
    if (this.expenseForm!.invalid) return;
    this.saving.set(true);
    const v = this.expenseForm!.value;
    const dto: CreateExpenseDto = {
      budgetId: +v.budgetId,
      categoryId: +v.categoryId,
      title: v.title,
      amount: +v.amount,
      merchantName: v.merchantName,
      notes: v.notes,
    };
    this.expenseService.createExpense(dto).subscribe({
      next: () => {
        this.saving.set(false);
        this.toastService.success('Expense submitted.');
        bootstrap.Modal.getInstance(document.getElementById('expenseFormModal'))?.hide();
        this.loadExpenses();
      },
      error: err => { this.saving.set(false); this.formError.set(err?.error?.message ?? 'Error submitting expense.'); }
    });
  }

  onUpdateStatus(): void {
    const e = this.selectedExpense()!;
    const isApprove = this.approvalAction() === 'approve';
    const dto = isApprove
      ? { status: 2, comments: this.statusForm!.value.comments }
      : { status: 3, reason: this.statusForm!.value.reason };
    this.saving.set(true);
    this.expenseService.updateExpenseStatus(e.expenseID, dto).subscribe({
      next: () => {
        this.saving.set(false);
        this.toastService.success(isApprove ? 'Expense approved.' : 'Expense rejected.');
        bootstrap.Modal.getInstance(document.getElementById('statusModal'))?.hide();
        this.loadExpenses();
      },
      error: err => { this.saving.set(false); this.toastService.error(err?.error?.message ?? 'Error updating status.'); }
    });
  }


  formatDate(d: string): string {
    return toIST(d, 'date');
  }
}