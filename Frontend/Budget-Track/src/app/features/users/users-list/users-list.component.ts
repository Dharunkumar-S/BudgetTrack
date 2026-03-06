import { Component, OnInit, inject, signal, computed, DestroyRef, PLATFORM_ID } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgIf, isPlatformBrowser } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserService } from '@services/user.service';
import { DepartmentService } from '@services/department.service';
import { AuthService } from '@core/services/auth.service';
import { ToastService } from '@core/services/toast.service';
import { RefreshService } from '@core/services/refresh.service';
import { StatusBadgeComponent } from '@shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { UserAvatarComponent } from '@shared/components/user-avatar/user-avatar.component';
import { UserDto, CreateUserDto, UpdateUserDto } from '@models/user.models';
import { DepartmentDto } from '@models/department.models';
import { PagedResult } from '@models/pagination.models';

declare const bootstrap: any;

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, NgIf, StatusBadgeComponent, PaginationComponent, ConfirmModalComponent, UserAvatarComponent],
  templateUrl: './users-list.component.html',
  styleUrl: './users-list.component.css'
})
export class UsersListComponent implements OnInit {
  private userService = inject(UserService);
  private deptService = inject(DepartmentService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);
  private refreshService = inject(RefreshService);
  private destroyRef = inject(DestroyRef);
  private platformId = inject(PLATFORM_ID);

  loading = signal(true);
  saving = signal(false);
  formError = signal('');
  editMode = signal(false);
  selectedUser = signal<UserDto | null>(null);
  departments = signal<DepartmentDto[]>([]);
  managers = signal<any[]>([]);
  subordinates = signal<UserDto[]>([]);
  data = signal<PagedResult<UserDto>>({ data: [], pageNumber: 1, pageSize: 10, totalRecords: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false, firstPage: 1, lastPage: 1, nextPage: null, previousPage: null, firstItemIndex: 1, lastItemIndex: 0, currentPageItemCount: 0, isFirstPage: true, isLastPage: true });

  kpiStats = signal({ totalUsers: 0, admins: 0, managers: 0, employees: 0, activeUsers: 0, inactiveUsers: 0 });
  adminCount = computed(() => this.data().data.filter(u => u.roleName === 'Admin').length);
  managerCount = computed(() => this.data().data.filter(u => u.roleName === 'Manager').length);
  employeeCount = computed(() => this.data().data.filter(u => u.roleName === 'Employee').length);
  isAdmin = computed(() => this.authService.isAdmin());

  currentPage = 1;
  pageSize = 10;
  searchId = '';
  filterRole = '';
  filterDeptId = '';
  userForm: FormGroup | null = null;
  editForm: FormGroup | null = null;

  get uf() { return this.userForm?.controls; }
  get ef() { return this.editForm?.controls; }

  get deleteUserMessage(): string {
    return `Are you sure you want to delete user "${this.selectedUser()?.firstName ?? ''} ${this.selectedUser()?.lastName ?? ''}" (${this.selectedUser()?.employeeId ?? ''})? This cannot be undone.`;
  }

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    if (this.isAdmin()) {
      this.loadStats();
    }
    this.load();
    this.deptService.getDepartments().subscribe(d => this.departments.set(d));
    this.userService.getManagers().subscribe(m => this.managers.set(m));
    this.refreshService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => { if (this.isAdmin()) this.loadStats(); this.load(); });
  }

  getFilteredManagers(userRoleId: any): any[] {
    const roleId = +userRoleId;
    if (!roleId || roleId === 1) return [];
    return this.managers().filter(m => +m.roleId === 2);
  }

  loadStats(): void {
    if (!this.isAdmin()) return;
    this.userService.getUserStats().subscribe({
      next: s => this.kpiStats.set(s),
      error: (e) => console.error('[Users] stats error:', e)
    });
  }

  load(): void {
    this.loading.set(true);
    const params: any = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      ...(this.filterRole && { roleId: +this.filterRole }),
      ...(this.searchId && { search: this.searchId }),
      ...(this.filterDeptId && { departmentId: +this.filterDeptId }),
    };
    this.userService.getUsers(params).subscribe({
      next: r => {
        this.data.set(r);
        this.loading.set(false);
        this.refreshService.notifyComplete();
      },
      error: (e) => {
        console.error('[Users] load error:', e);
        this.loading.set(false);
        this.refreshService.notifyComplete(false);
      }
    });
  }

  onSearch(): void { this.currentPage = 1; this.load(); }
  onPageChange(p: number): void { this.currentPage = p; this.load(); }
  onPageSizeChange(s: number): void { this.pageSize = s; this.currentPage = 1; this.load(); }
  clearFilters(): void {
    this.searchId = '';
    this.filterRole = '';
    this.filterDeptId = '';
    this.pageSize = 10;
    this.currentPage = 1;
    this.load();
  }

  openCreate(): void {
    if (!this.isAdmin()) {
      this.toast.error('Only admins can create users.');
      return;
    }
    this.editMode.set(false);
    this.formError.set('');
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
    this.userForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100), Validators.pattern(/^[^@]+@budgettrack\.com$/)]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(passwordPattern)]],
      roleID: ['', Validators.required],
      departmentID: ['', Validators.required],
      managerEmployeeId: [''],
    });

    this.userForm.get('roleID')?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(roleId => {
      const managerCtrl = this.userForm?.get('managerEmployeeId');
      const deptCtrl = this.userForm?.get('departmentID');

      if (+roleId === 3) {
        managerCtrl?.setValidators([Validators.required]);
        deptCtrl?.disable();
        const managerEmpId = managerCtrl?.value;
        if (managerEmpId) {
          const mgr = this.managers().find(m => (m.employeeId ?? m.employeeID ?? m.EmployeeID) === managerEmpId);
          if (mgr) {
            const dId = mgr.departmentId ?? mgr.departmentID ?? mgr.departmentid ?? mgr.DepartmentId ?? mgr.DepartmentID;
            if (dId) {
              deptCtrl?.setValue(dId);
              deptCtrl?.markAsTouched();
            }
          }
        }
      } else {
        managerCtrl?.clearValidators();
        deptCtrl?.enable();
        if (+roleId !== 2) {
          managerCtrl?.setValue('');
          deptCtrl?.setValue('');
        }
      }
      managerCtrl?.updateValueAndValidity();
      deptCtrl?.updateValueAndValidity();
    });

    this.userForm.get('managerEmployeeId')?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(managerEmpId => {
      const roleId = +this.userForm?.get('roleID')?.value;
      if (roleId === 3) {
        const deptCtrl = this.userForm?.get('departmentID');
        if (managerEmpId) {
          const mgr = this.managers().find(m => (m.employeeId ?? m.employeeID ?? m.EmployeeID) === managerEmpId);
          if (mgr) {
            const dId = mgr.departmentId ?? mgr.departmentID ?? mgr.departmentid ?? mgr.DepartmentId ?? mgr.DepartmentID;
            if (dId) {
              deptCtrl?.setValue(dId);
              deptCtrl?.markAsTouched();
              deptCtrl?.updateValueAndValidity();
            } else {
              console.warn('Manager found but department data missing. Refresh or check backend.', mgr);
            }
          }
        } else {
          deptCtrl?.setValue('');
          deptCtrl?.updateValueAndValidity();
        }
      }
    });
    new bootstrap.Modal(document.getElementById('createUserModal')).show();
  }

  openEdit(u: UserDto): void {
    if (!this.isAdmin()) {
      this.toast.error('Only admins can edit users.');
      return;
    }
    this.editMode.set(true);
    this.selectedUser.set(u);
    this.formError.set('');
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
    this.editForm = this.fb.group({
      firstName: [u.firstName, [Validators.required, Validators.maxLength(50)]],
      lastName: [u.lastName, [Validators.required, Validators.maxLength(50)]],
      email: [u.email, [Validators.required, Validators.email, Validators.maxLength(100), Validators.pattern(/^[^@]+@budgettrack\.com$/)]],
      password: ['', [Validators.minLength(8), Validators.pattern(passwordPattern)]],
      roleID: [u.roleId, Validators.required],
      departmentID: [{ value: u.departmentId, disabled: u.roleId === 3 }, Validators.required],
      managerEmployeeId: [u.managerEmployeeId ?? ''],
      status: [u.isActive ? 1 : 2],
    });

    this.editForm.get('roleID')?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(roleId => {
      const managerCtrl = this.editForm?.get('managerEmployeeId');
      const deptCtrl = this.editForm?.get('departmentID');

      if (+roleId === 3) {
        managerCtrl?.setValidators([Validators.required]);
        deptCtrl?.disable();
        const managerEmpId = managerCtrl?.value;
        if (managerEmpId) {
          const mgr = this.managers().find(m => (m.employeeId ?? m.employeeID ?? m.EmployeeID) === managerEmpId);
          if (mgr) {
            const dId = mgr.departmentId ?? mgr.departmentID ?? mgr.departmentid ?? mgr.DepartmentId ?? mgr.DepartmentID;
            if (dId) {
              deptCtrl?.setValue(dId);
              deptCtrl?.markAsTouched();
            }
          }
        }
      } else {
        managerCtrl?.clearValidators();
        deptCtrl?.enable();
        if (+roleId !== 2) {
          managerCtrl?.setValue('');
          deptCtrl?.setValue('');
        }
      }
      managerCtrl?.updateValueAndValidity();
      deptCtrl?.updateValueAndValidity();
    });

    this.editForm.get('managerEmployeeId')?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(managerEmpId => {
      const roleId = +this.editForm?.get('roleID')?.value;
      if (roleId === 3) {
        const deptCtrl = this.editForm?.get('departmentID');
        if (managerEmpId) {
          const mgr = this.managers().find(m => (m.employeeId ?? m.employeeID ?? m.EmployeeID) === managerEmpId);
          if (mgr) {
            const dId = mgr.departmentId ?? mgr.departmentID ?? mgr.departmentid ?? mgr.DepartmentId ?? mgr.DepartmentID;
            if (dId) {
              deptCtrl?.setValue(dId);
              deptCtrl?.markAsTouched();
              deptCtrl?.updateValueAndValidity();
            } else {
              console.warn('Manager found but department data missing. Refresh or check backend.', mgr);
            }
          }
        } else {
          deptCtrl?.setValue('');
          deptCtrl?.updateValueAndValidity();
        }
      }
    });
    new bootstrap.Modal(document.getElementById('editUserModal')).show();
  }

  openView(u: UserDto): void {
    this.selectedUser.set(u);
    this.subordinates.set([]);

    if (u.roleName === 'Manager') {
      this.userService.getEmployeesByManager(u.userId).subscribe({
        next: (employees) => this.subordinates.set(employees),
        error: (err) => console.error('Error fetching subordinates:', err)
      });
    }

    new bootstrap.Modal(document.getElementById('viewUserModal')).show();
  }

  confirmDelete(u: UserDto): void {
    this.selectedUser.set(u);
  }

  onCreateUser(): void {
    if (!this.isAdmin()) {
      this.toast.error('Only admins can create users.');
      return;
    }
    this.userForm!.markAllAsTouched();
    if (this.userForm!.invalid) return;

    this.saving.set(true);
    const raw = this.userForm!.getRawValue();

    // Explicitly map values to match backend requirements
    const dto: CreateUserDto = {
      firstName: raw.firstName?.trim(),
      lastName: raw.lastName?.trim(),
      email: raw.email?.trim(),
      password: raw.password,
      roleID: +raw.roleID,
      departmentID: (raw.departmentID && raw.departmentID !== '') ? +raw.departmentID : 0,
      // Manager required for Employee, optional for Manager, none for Admin
      managerEmployeeId: (+raw.roleID > 1 && raw.managerEmployeeId) ? raw.managerEmployeeId.trim() : null
    };

    if (dto.roleID === 3 && dto.departmentID === 0) {
      this.toast.error('Unable to fetch department from the selected manager. Please check if the manager has a department assigned and refresh the page.');
      this.saving.set(false);
      return;
    }

    this.userService.createUser(dto).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('User registered successfully.');
        bootstrap.Modal.getInstance(document.getElementById('createUserModal'))?.hide();
        this.load();
        if (this.isAdmin()) this.loadStats();
        this.userService.getManagers().subscribe(m => this.managers.set(m));
      },
      error: (e: any) => {
        this.saving.set(false);
        // Try to extract detailed error messages from ModelState if available
        let msg = 'Error creating user.';
        if (e?.error?.errors) {
          msg = Object.values(e.error.errors).flat().join(' ') || msg;
        } else if (e?.error?.message) {
          msg = e.error.message;
        }
        this.formError.set(msg);
      }
    });
  }

  onUpdateUser(): void {
    if (!this.isAdmin()) {
      this.toast.error('Only admins can update users.');
      return;
    }
    this.editForm!.markAllAsTouched();
    if (this.editForm!.invalid) return;
    this.saving.set(true);
    const raw = this.editForm!.getRawValue();
    const u = this.selectedUser()!;
    const dto: UpdateUserDto = {
      firstName: raw.firstName?.trim(),
      lastName: raw.lastName?.trim(),
      email: raw.email?.trim(),
      roleID: +raw.roleID,
      departmentID: (raw.departmentID && raw.departmentID !== '') ? +raw.departmentID : 0,
      // Manager required for Employee, optional for Manager, none for Admin
      managerEmployeeId: (+raw.roleID > 1 && raw.managerEmployeeId) ? raw.managerEmployeeId.trim() : null,
      status: +raw.status,
      ...(raw.password ? { password: raw.password } : {}),
    };

    if (dto.roleID === 3 && dto.departmentID === 0) {
      this.toast.error('Unable to fetch department from the selected manager. Please check if the manager has a department assigned and refresh the page.');
      this.saving.set(false);
      return;
    }
    this.userService.updateUser(u.userId, dto).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('User updated successfully.');
        bootstrap.Modal.getInstance(document.getElementById('editUserModal'))?.hide();
        this.load();
        if (this.isAdmin()) this.loadStats();
        this.userService.getManagers().subscribe(m => this.managers.set(m));
      },
      error: (e: any) => {
        this.saving.set(false);
        let msg = 'Update failed.';
        if (e?.error?.errors) {
          msg = Object.values(e.error.errors).flat().join(' ') || msg;
        } else if (e?.error?.message) {
          msg = e.error.message;
        }
        this.formError.set(msg);
      }
    });
  }

  onDeleteUser(): void {
    if (!this.isAdmin()) {
      this.toast.error('Only admins can delete users.');
      return;
    }
    const u = this.selectedUser();
    if (!u) return;
    this.userService.deleteUser(u.userId).subscribe({
      next: () => {
        this.toast.success('User deleted.');
        this.load();
        if (this.isAdmin()) this.loadStats();
      },
      error: (e: any) => this.toast.error(e?.error?.message ?? 'Error deleting user.')
    });
  }
}