import { Component, OnInit, inject, signal, computed, DestroyRef, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DepartmentService } from '@services/department.service';
import { ToastService } from '@core/services/toast.service';
import { AuthService } from '@core/services/auth.service';
import { RefreshService } from '@core/services/refresh.service';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { StatusBadgeComponent } from '@shared/components/status-badge/status-badge.component';
import { DepartmentDto } from '@models/department.models';

declare const bootstrap: any;

@Component({
  selector: 'app-departments-list',
  standalone: true,
  imports: [StatusBadgeComponent, FormsModule, ReactiveFormsModule, ConfirmModalComponent],
  templateUrl: './departments-list.component.html',
  styleUrl: './departments-list.component.css'
})
export class DepartmentsListComponent implements OnInit {
  private service = inject(DepartmentService);
  private toast = inject(ToastService);
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private refreshService = inject(RefreshService);
  private destroyRef = inject(DestroyRef);
  private platformId = inject(PLATFORM_ID);

  isAdmin = () => this.authService.isAdmin();

  loading = signal(true);
  saving = signal(false);
  editMode = signal(false);
  departments = signal<DepartmentDto[]>([]);
  selected = signal<DepartmentDto | null>(null);
  deptForm: FormGroup | null = null;
  get f() { return this.deptForm?.controls ?? {}; }
  searchTerm = signal('');
  filterStatus = signal<'all' | 'active' | 'inactive'>('all');

  clearFilters(): void {
    this.searchTerm.set('');
    this.filterStatus.set('all');
  }

  filtered = computed(() => {
    let list = this.departments();
    const status = this.filterStatus();
    if (status === 'active') list = list.filter(d => d.isActive);
    if (status === 'inactive') list = list.filter(d => !d.isActive);
    const term = this.searchTerm().trim();
    if (term) {
      const q = term.toLowerCase();
      list = list.filter(d => d.departmentName.toLowerCase().includes(q) || d.departmentCode.toLowerCase().includes(q));
    }
    return list;
  });

  activeCount = computed(() => this.departments().filter(d => d.isActive).length);
  inactiveCount = computed(() => this.departments().filter(d => !d.isActive).length);

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.load();
    this.refreshService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  load(): void {
    this.service.getDepartments().subscribe({
      next: d => { this.departments.set(d); this.loading.set(false); this.refreshService.notifyComplete(); },
      error: () => { this.loading.set(false); this.refreshService.notifyComplete(false); }
    });
  }

  openCreate(): void {
    this.editMode.set(false);
    this.deptForm = this.fb.group({ departmentName: ['', Validators.required] });
    new bootstrap.Modal(document.getElementById('deptFormModal')).show();
  }

  openEdit(d: DepartmentDto): void {
    this.editMode.set(true);
    this.selected.set(d);
    this.deptForm = this.fb.group({ departmentName: [d.departmentName, Validators.required], isActive: [d.isActive] });
    new bootstrap.Modal(document.getElementById('deptFormModal')).show();
  }

  onSave(): void {
    this.deptForm!.markAllAsTouched();
    if (this.deptForm!.invalid) return;
    this.saving.set(true);
    const onNext = () => { this.saving.set(false); this.toast.success(this.editMode() ? 'Department updated.' : 'Department created.'); bootstrap.Modal.getInstance(document.getElementById('deptFormModal'))?.hide(); this.load(); };
    const onError = (e: any) => { this.saving.set(false); this.toast.error(e?.error?.message ?? 'Error.'); };
    if (this.editMode()) {
      this.service.updateDepartment(this.selected()!.departmentID, this.deptForm!.value).subscribe({ next: onNext, error: onError });
    } else {
      this.service.createDepartment(this.deptForm!.value).subscribe({ next: onNext, error: onError });
    }
  }

  onDeactivate(): void {
    const d = this.selected();
    if (!d) return;
    this.service.updateDepartment(d.departmentID, { departmentName: d.departmentName, isActive: false }).subscribe({
      next: () => { this.toast.success('Department marked as inactive.'); this.load(); },
      error: (e: any) => this.toast.error(e?.error?.message ?? 'Error.')
    });
  }
}