import { Component, OnInit, inject, signal, computed, DestroyRef, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CategoryService } from '@services/category.service';
import { ToastService } from '@core/services/toast.service';
import { AuthService } from '@core/services/auth.service';
import { RefreshService } from '@core/services/refresh.service';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { StatusBadgeComponent } from '@shared/components/status-badge/status-badge.component';
import { CategoryDto } from '@models/category.models';

declare const bootstrap: any;

@Component({
  selector: 'app-categories-list',
  standalone: true,
  imports: [StatusBadgeComponent, FormsModule, ReactiveFormsModule, ConfirmModalComponent],
  templateUrl: './categories-list.component.html',
  styleUrl: './categories-list.component.css'
})
export class CategoriesListComponent implements OnInit {
  private service = inject(CategoryService);
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
  categories = signal<CategoryDto[]>([]);
  selected = signal<CategoryDto | null>(null);
  catForm: FormGroup | null = null;
  get f() { return this.catForm?.controls ?? {}; }
  searchTerm = signal('');
  filterStatus = signal<'all' | 'active' | 'inactive'>('all');

  clearFilters(): void {
    this.searchTerm.set('');
    this.filterStatus.set('all');
  }

  filtered = computed(() => {
    let list = this.categories();
    const status = this.filterStatus();
    if (status === 'active') list = list.filter(c => c.isActive);
    if (status === 'inactive') list = list.filter(c => !c.isActive);
    const term = this.searchTerm().trim();
    if (term) {
      const q = term.toLowerCase();
      list = list.filter(c => c.categoryName.toLowerCase().includes(q) || c.categoryCode.toLowerCase().includes(q));
    }
    return list;
  });

  activeCount = computed(() => this.categories().filter(c => c.isActive).length);
  inactiveCount = computed(() => this.categories().filter(c => !c.isActive).length);

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.load();
    this.refreshService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  load(): void {
    this.service.getCategories().subscribe({
      next: c => { this.categories.set(c); this.loading.set(false); this.refreshService.notifyComplete(); },
      error: () => { this.loading.set(false); this.refreshService.notifyComplete(false); }
    });
  }


  openCreate(): void {
    this.editMode.set(false);
    this.catForm = this.fb.group({ categoryName: ['', Validators.required] });
    new bootstrap.Modal(document.getElementById('catFormModal')).show();
  }

  openEdit(c: CategoryDto): void {
    this.editMode.set(true);
    this.selected.set(c);
    this.catForm = this.fb.group({ categoryName: [c.categoryName, Validators.required], isActive: [c.isActive] });
    new bootstrap.Modal(document.getElementById('catFormModal')).show();
  }

  onSave(): void {
    this.catForm!.markAllAsTouched();
    if (this.catForm!.invalid) return;
    this.saving.set(true);
    const onNext = () => { this.saving.set(false); this.toast.success(this.editMode() ? 'Category updated.' : 'Category created.'); bootstrap.Modal.getInstance(document.getElementById('catFormModal'))?.hide(); this.load(); };
    const onError = (e: any) => { this.saving.set(false); this.toast.error(e?.error?.message ?? 'Error.'); };
    if (this.editMode()) {
      this.service.updateCategory(this.selected()!.categoryID, this.catForm!.value).subscribe({ next: onNext, error: onError });
    } else {
      this.service.createCategory(this.catForm!.value).subscribe({ next: onNext, error: onError });
    }
  }

  onDeactivate(): void {
    const c = this.selected();
    if (!c) return;
    this.service.updateCategory(c.categoryID, { categoryName: c.categoryName, isActive: false }).subscribe({
      next: () => { this.toast.success('Category marked as inactive.'); this.load(); },
      error: (e: any) => this.toast.error(e?.error?.message ?? 'Error.')
    });
  }
}