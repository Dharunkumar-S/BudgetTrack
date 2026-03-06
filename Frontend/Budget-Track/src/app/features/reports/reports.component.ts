import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
  ViewChild,
  ElementRef,
  AfterViewInit,
  DestroyRef,
  PLATFORM_ID,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgClass, SlicePipe, isPlatformBrowser } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import * as XLSX from 'xlsx';
import { ReportService } from '../../../services/report.service';
import { BudgetService } from '../../../services/budget.service';
import { ToastService } from '../../../core/services/toast.service';
import { RefreshService } from '../../../core/services/refresh.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  PeriodReportDto,
  DepartmentReportDto,
  BudgetReportDto,
  DepartmentReportItem,
} from '../../../models/report.models';
import { BudgetDto } from '../../../models/budget.models';
import {
  Chart,
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  ArcElement,
  DoughnutController,
  PieController,
  Tooltip,
  Legend,
} from 'chart.js';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [FormsModule, NgClass, SlicePipe, PaginationComponent],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css',
})
export class ReportsComponent implements OnInit, AfterViewInit {
  private reportService = inject(ReportService);
  private budgetService = inject(BudgetService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);
  private refreshService = inject(RefreshService);
  private destroyRef = inject(DestroyRef);
  private platformId = inject(PLATFORM_ID);

  protected Math = Math;

  @ViewChild('periodBarChart') periodBarChartRef!: ElementRef<HTMLCanvasElement>;
  private periodBarChart?: Chart;

  @ViewChild('deptBarChart') deptBarChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('deptPieChart') deptPieChartRef!: ElementRef<HTMLCanvasElement>;
  private deptBarChart?: Chart;
  private deptPieChart?: Chart;

  @ViewChild('budgetStatusDonut') budgetStatusDonutRef!: ElementRef<HTMLCanvasElement>;
  private budgetStatusDonutChart?: Chart;

  @ViewChild('budgetBarChart') budgetBarChartRef!: ElementRef<HTMLCanvasElement>;
  private budgetFinancialBarChart?: Chart;

  @ViewChild('budgetCategoryPie') budgetCategoryPieRef!: ElementRef<HTMLCanvasElement>;
  private budgetCategoryPieChart?: Chart;

  isAdmin = computed(() => this.authService.userRole() === 'Admin');
  isAdminOrManager = computed(() =>
    ['Admin', 'Manager'].includes(this.authService.userRole() ?? ''),
  );

  activeTab = signal<'period' | 'department' | 'budget'>('budget');
  periodLoading = signal(false);
  deptLoading = signal(false);
  budgetLoading = signal(false);

  periodReport = signal<PeriodReportDto | null>(null);
  deptReport = signal<DepartmentReportDto | null>(null);
  budgetReport = signal<BudgetReportDto | null>(null);

  periodStart = '';
  periodEnd = '';
  budgetCode = '';

  budgetOptions = signal<BudgetDto[]>([]);
  budgetOptionsLoading = signal(false);
  budgetOptionsError = signal<string | null>(null);

  budgetExpensePage = signal(1);
  budgetExpensePageSize = signal(10);

  employeePage = signal(0);
  employeeStats = computed(() => {
    const report = this.budgetReport();
    if (!report || !report.expenses) return [];

    const map = new Map<string, any>();
    for (const e of report.expenses) {
      if (!map.has(e.submittedEmployeeId)) {
        const names = e.submittedBy.trim().split(' ');
        const initials =
          names.length > 1
            ? names[0].charAt(0) + names[names.length - 1].charAt(0)
            : names[0].charAt(0);

        map.set(e.submittedEmployeeId, {
          employeeId: e.submittedEmployeeId,
          name: e.submittedBy,
          initials: initials.toUpperCase(),
          pending: 0,
          approved: 0,
          rejected: 0,
          total: 0,
        });
      }
      const stat = map.get(e.submittedEmployeeId);
      stat.total++;
      if (e.status === 'Pending') stat.pending++;
      else if (e.status === 'Approved') stat.approved++;
      else if (e.status === 'Rejected') stat.rejected++;
    }

    return Array.from(map.values()).sort((a, b) => b.total - a.total);
  });

  visibleEmployees = computed(() => {
    const stats = this.employeeStats();
    const start = this.employeePage() * 5;
    return stats.slice(start, start + 5);
  });

  hasPrevEmployeePage = computed(() => this.employeePage() > 0);
  hasNextEmployeePage = computed(() => (this.employeePage() + 1) * 5 < this.employeeStats().length);

  employeePageEnd = computed(() => {
    return Math.min((this.employeePage() + 1) * 5, this.employeeStats().length);
  });

  private chartsRegistered = false;

  /** Returns min-width for period chart carousel — 90px per budget, min 100% */
  periodChartMinWidth = computed(() => {
    const count = this.periodReport()?.budgets?.length ?? 0;
    if (count <= 8) return '100%';
    return `${count * 90} px`;
  });

  /** Departments excluding 'Administration', used in table and charts */
  visibleDepartments = computed(() => {
    const data = this.deptReport()?.departments ?? [];
    return data.filter((d) => d.departmentName !== 'Administration');
  });

  // Date string getters (periodStart and periodEnd are already strings)
  get periodStartString(): string {
    return this.periodStart;
  }

  get periodEndString(): string {
    return this.periodEnd;
  }

  Chart = Chart;

  constructor() {
    this.destroyRef.onDestroy(() => this.destroyCharts());
  }

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const today = new Date();
    const yearAgo = new Date(today.getFullYear() - 1, today.getMonth(), today.getDate());
    this.periodStart = this.toDateString(yearAgo);
    this.periodEnd = this.toDateString(today);

    if (this.authService.isAdmin()) {
      // Admins start on Period Report and pre-load Department data
      this.activeTab.set('period');
      this.loadDepartmentReport();
    } else {
      // Managers and Employees go straight to Budget Report
      this.activeTab.set('budget');
      this.ensureBudgetOptionsLoaded();
    }

    this.refreshService.refresh$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      const tab = this.activeTab();
      if (tab === 'period' && this.isAdmin()) {
        this.loadPeriodReport();
      } else if (tab === 'department' && this.isAdmin()) {
        this.loadDepartmentReport();
      } else if (tab === 'budget') {
        if (this.budgetCode) {
          this.loadBudgetReport();
        } else {
          this.ensureBudgetOptionsLoaded();
          this.refreshService.notifyComplete();
        }
      } else {
        this.refreshService.notifyComplete();
      }
    });
  }

  ngAfterViewInit(): void {
    // Chart will be rendered after data is loaded
  }

  prevEmployeePage() {
    if (this.hasPrevEmployeePage()) {
      this.employeePage.update((p) => p - 1);
    }
  }

  nextEmployeePage() {
    if (this.hasNextEmployeePage()) {
      this.employeePage.update((p) => p + 1);
    }
  }

  get pagedBudgetExpenses() {
    const report = this.budgetReport();
    if (!report) return [];
    const pageSize = this.budgetExpensePageSize();
    const page = this.budgetExpensePage();
    const start = (page - 1) * pageSize;
    return [...report.expenses]
      .sort((a, b) => b.submittedDate.localeCompare(a.submittedDate))
      .slice(start, start + pageSize);
  }

  get budgetExpenseTotalPages(): number {
    const total = this.budgetReport()?.expenses.length ?? 0;
    const size = this.budgetExpensePageSize();
    if (!size || !total) return 1;
    return Math.max(1, Math.ceil(total / size));
  }

  onBudgetExpensePageChange(page: number): void {
    this.budgetExpensePage.set(page);
  }

  onBudgetExpensePageSizeChange(size: number): void {
    this.budgetExpensePageSize.set(size);
    this.budgetExpensePage.set(1);
  }

  private toDateString(date: Date): string {
    if (!date) return '';
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year} -${month} -${day} `;
  }

  onTabChange(tab: 'period' | 'department' | 'budget'): void {
    this.activeTab.set(tab);

    if (tab === 'period') {
      if (this.periodReport()) {
        setTimeout(() => this.renderPeriodBarChart(), 100);
      }
    }

    if (tab === 'department') {
      if (!this.deptReport()) {
        this.loadDepartmentReport();
      } else {
        setTimeout(() => this.renderDepartmentCharts(), 100);
      }
    }

    if (tab === 'budget') {
      this.ensureBudgetOptionsLoaded();
      if (this.budgetReport()) {
        setTimeout(() => {
          this.renderBudgetStatusChart();
          this.renderBudgetBarChart();
          this.renderBudgetCategoryPie();
        }, 100);
      }
    }
  }

  onDepartmentFilterChange(): void {
    if (this.deptReport() && this.activeTab() === 'department') {
      setTimeout(() => this.renderDepartmentCharts(), 100);
    }
  }

  private ensureBudgetOptionsLoaded(): void {
    if (this.budgetOptions().length || this.budgetOptionsLoading()) {
      return;
    }

    this.budgetOptionsLoading.set(true);
    this.budgetOptionsError.set(null);

    const params = { pageNumber: 1, pageSize: 200, sortBy: 'code', sortOrder: 'asc' as const };

    // Admin sees ALL budgets; Manager sees only their own; Employee sees their manager's budgets
    const budgets$ = this.authService.isAdmin()
      ? this.budgetService.getAdminBudgets(params)
      : this.budgetService.getBudgets(params);

    budgets$.subscribe({
      next: (result) => {
        this.budgetOptions.set(result.data ?? []);
        this.budgetOptionsLoading.set(false);
      },
      error: (e: any) => {
        this.budgetOptionsLoading.set(false);
        this.budgetOptionsError.set(e?.error?.message ?? 'Unable to load budgets. Try again.');
      },
    });
  }

  private ensureChartsRegistered(): void {
    if (this.chartsRegistered) return;
    Chart.register(
      BarController,
      BarElement,
      CategoryScale,
      LinearScale,
      ArcElement,
      DoughnutController,
      PieController,
      Tooltip,
      Legend,
    );
    this.chartsRegistered = true;
  }

  private destroyCharts(): void {
    this.periodBarChart?.destroy();
    this.deptBarChart?.destroy();
    this.deptPieChart?.destroy();
    this.budgetStatusDonutChart?.destroy();
    this.budgetFinancialBarChart?.destroy();
    this.budgetCategoryPieChart?.destroy();
  }

  loadPeriodReport(): void {
    // Validate dates
    if (!this.periodStart || !this.periodEnd) {
      this.toast.warning('Please select a date range.');
      return;
    }
    // Compare as strings (ISO format allows string comparison)
    if (this.periodStart > this.periodEnd) {
      this.toast.warning('Start date must be before end date.');
      return;
    }

    this.periodLoading.set(true);
    this.periodReport.set(null);

    // Pass strings directly - service will convert to Date and then to UTC ISO format
    this.reportService.getPeriodReport(this.periodStart, this.periodEnd).subscribe({
      next: (r) => {
        this.periodReport.set(r);
        this.periodLoading.set(false);
        this.refreshService.notifyComplete();
        if (this.activeTab() === 'period') {
          setTimeout(() => this.renderPeriodBarChart(), 100);
        }
      },
      error: (e: any) => {
        this.periodLoading.set(false);
        this.refreshService.notifyComplete(false);
        this.toast.error(e?.message ?? 'Error loading report.');
      },
    });
  }

  loadDepartmentReport(): void {
    this.deptLoading.set(true);
    this.reportService.getDepartmentReport().subscribe({
      next: (r) => {
        this.deptReport.set(r);
        this.deptLoading.set(false);
        this.refreshService.notifyComplete();
        if (this.activeTab() === 'department') {
          setTimeout(() => this.renderDepartmentCharts(), 100);
        }
      },
      error: (e: any) => {
        this.deptLoading.set(false);
        this.refreshService.notifyComplete(false);
        this.toast.error(e?.message ?? 'Error loading report.');
      },
    });
  }

  loadBudgetReport(): void {
    if (!this.budgetCode.trim()) {
      this.toast.warning('Please enter a budget code.');
      return;
    }
    this.budgetLoading.set(true);
    this.reportService.getBudgetReport(this.budgetCode.trim()).subscribe({
      next: (r) => {
        this.budgetReport.set(r);
        this.budgetLoading.set(false);
        this.refreshService.notifyComplete();
        this.budgetExpensePage.set(1);
        this.employeePage.set(0);
        if (this.activeTab() === 'budget') {
          setTimeout(() => {
            this.renderBudgetStatusChart();
            this.renderBudgetBarChart();
            this.renderBudgetCategoryPie();
          }, 100);
        }
      },
      error: (e: any) => {
        this.budgetLoading.set(false);
        this.refreshService.notifyComplete(false);
        this.toast.error(e?.message ?? 'Error loading report.');
      },
    });
  }

  /** Format currency with Rs. prefix and K/L/M/Cr suffixes */
  formatCurrency(amount: number): string {
    const safeAmount = amount ?? 0;
    const absAmount = Math.abs(safeAmount);
    const prefix = safeAmount < 0 ? '-' : '';

    if (absAmount >= 10_000_000) {
      return `${prefix} Rs.${(absAmount / 10_000_000).toFixed(2)} M`;
    } else if (absAmount >= 100_000) {
      return `${prefix} Rs.${(absAmount / 100_000).toFixed(2)} L`;
    } else if (absAmount >= 1_000) {
      return `${prefix} Rs.${(absAmount / 1_000).toFixed(1)} K`;
    } else {
      return `${prefix} Rs.${absAmount.toLocaleString('en-IN')} `;
    }
  }

  /** Format amount with ₹ symbol and Indian numbering */
  fmtAmt(v: number): string {
    const safeV = v ?? 0;
    if (safeV >= 10_000_000) return `₹${(safeV / 10_000_000).toFixed(2)} Cr`;
    if (safeV >= 100_000) return `₹${(safeV / 100_000).toFixed(2)} L`;
    return `₹${safeV.toLocaleString('en-IN')} `;
  }

  /** Safe percentage formatter with fallback to 0 */
  safePercent(value: number | null | undefined): string {
    const safeValue = value ?? 0;
    return safeValue.toFixed(1);
  }

  getUtilizationClass(p: number): string {
    const safeP = p ?? 0;
    return safeP >= 80 ? 'high' : safeP >= 50 ? 'medium' : 'low';
  }

  utilClass(p: number): string {
    const safeP = p ?? 0;
    return safeP >= 80 ? 'high' : safeP >= 50 ? 'medium' : 'low';
  }

  private renderPeriodBarChart(): void {
    if (typeof window === 'undefined') return;
    if (!this.periodBarChartRef) return;

    // Destroy existing chart
    this.periodBarChart?.destroy();
    this.ensureChartsRegistered();

    const ctx = this.periodBarChartRef.nativeElement.getContext('2d')!;
    const budgets = this.periodReport()?.budgets ?? [];

    // Sort budgets by allocated amount (descending) for better visualization, limit to top 15
    const sortedBudgets = [...budgets]
      .sort((a, b) => b.allocatedAmount - a.allocatedAmount)
      .slice(0, 15);

    const labels = sortedBudgets.map((b) => b.budgetCode);
    const allocatedData = sortedBudgets.map((b) => b.allocatedAmount);
    const spentData = sortedBudgets.map((b) => b.amountSpent);
    const remainingData = sortedBudgets.map((b) => b.amountRemaining);

    const dataLabelPlugin = this.buildBarDataLabelsPlugin('periodBarDataLabels');

    this.periodBarChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Allocated',
            data: allocatedData,
            backgroundColor: '#3b82f6',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
          {
            label: 'Remaining',
            data: remainingData,
            backgroundColor: '#ec4899',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
          {
            label: 'Spent',
            data: spentData,
            backgroundColor: '#10b981',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: {
          duration: 800,
          easing: 'easeOutQuart',
        },
        plugins: {
          legend: {
            display: true,
            position: 'top',
            align: 'end',
            labels: {
              usePointStyle: true,
              pointStyle: 'circle',
              padding: 16,
              font: { size: 11, weight: 'normal' as const },
              color: '#64748b',
            },
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleFont: { size: 12, weight: 'bold' as const },
            bodyFont: { size: 11 },
            padding: 12,
            cornerRadius: 8,
            displayColors: false,
            callbacks: {
              title: (items) => {
                const idx = items[0].dataIndex;
                return sortedBudgets[idx]?.budgetTitle ?? '';
              },
              label: (ctx) => {
                if (ctx.datasetIndex !== 0) return '';
                const idx = ctx.dataIndex;
                const b = sortedBudgets[idx];
                if (!b) return '';
                return [
                  `Allocated:   ${this.fmtAmt(b.allocatedAmount)} `,
                  `Spent:          ${this.fmtAmt(b.amountSpent)} `,
                  `Remaining:  ${this.fmtAmt(b.amountRemaining)} `,
                  `Utilization:  ${b.utilizationPercentage.toFixed(1)}% `,
                ] as unknown as string;
              },
            },
          },
        },
        scales: {
          x: {
            stacked: false,
            grid: {
              display: false,
            },
            ticks: {
              color: '#94a3b8',
              font: { size: 11, weight: 'bold' as const },
              maxRotation: 0,
              minRotation: 0,
            },
            border: {
              display: false,
            },
          },
          y: {
            stacked: false,
            grid: {
              color: '#f1f5f9',
              lineWidth: 1,
            },
            ticks: {
              color: '#94a3b8',
              font: { size: 10 },
              callback: (value) => {
                const numValue = Number(value);
                if (numValue >= 10_000_000) {
                  return `${(numValue / 10_000_000).toFixed(0)} M`;
                } else if (numValue >= 1_000_000) {
                  return `${(numValue / 1_000_000).toFixed(1)} M`;
                } else if (numValue >= 100_000) {
                  return `${(numValue / 100_000).toFixed(0)} L`;
                } else if (numValue >= 1_000) {
                  return `${(numValue / 1_000).toFixed(0)} K`;
                }
                return numValue.toString();
              },
            },
            border: {
              display: false,
            },
            beginAtZero: true,
          },
        },
        interaction: {
          mode: 'index',
          intersect: false,
        },
      },
      plugins: [dataLabelPlugin],
    });
  }

  private renderDepartmentCharts(): void {
    if (typeof window === 'undefined') return;
    if (!this.deptBarChartRef || !this.deptPieChartRef) return;
    if (!this.deptReport()) return;

    this.ensureChartsRegistered();

    this.deptBarChart?.destroy();
    this.deptPieChart?.destroy();

    const departments = this.visibleDepartments();
    if (!departments.length) {
      return;
    }

    const barCtx = this.deptBarChartRef.nativeElement.getContext('2d')!;
    const pieCtx = this.deptPieChartRef.nativeElement.getContext('2d')!;

    const labels = departments.map((d: DepartmentReportItem) => d.departmentCode);
    const allocated = departments.map((d: DepartmentReportItem) => d.amountAllocated);
    const spent = departments.map((d: DepartmentReportItem) => d.amountSpent);
    const remaining = departments.map((d: DepartmentReportItem) => d.amountRemaining);

    const barDataLabelPlugin = this.buildBarDataLabelsPlugin('deptBarDataLabels');
    const pieDataLabelPlugin = this.buildPieDataLabelsPlugin('deptPieDataLabels');

    this.deptBarChart = new Chart(barCtx, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Allocated',
            data: allocated,
            backgroundColor: '#3b82f6',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
          {
            label: 'Remaining',
            data: remaining,
            backgroundColor: '#ec4899',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
          {
            label: 'Spent',
            data: spent,
            backgroundColor: '#10b981',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: {
          duration: 800,
          easing: 'easeOutQuart',
        },
        plugins: {
          legend: {
            display: true,
            position: 'top',
            align: 'end',
            labels: {
              usePointStyle: true,
              pointStyle: 'circle',
              padding: 16,
              font: { size: 11, weight: 'normal' as const },
              color: '#64748b',
            },
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleFont: { size: 12, weight: 'bold' as const },
            bodyFont: { size: 11 },
            padding: 12,
            cornerRadius: 8,
            displayColors: false,
            callbacks: {
              title: (items) => {
                const idx = items[0].dataIndex;
                return departments[idx]?.departmentName ?? '';
              },
              label: (ctx) => {
                if (ctx.datasetIndex !== 0) return '';
                const idx = ctx.dataIndex;
                const d = departments[idx];
                if (!d) return '';
                const util = d.amountAllocated > 0
                  ? ((d.amountSpent / d.amountAllocated) * 100).toFixed(1)
                  : '0.0';
                return [
                  `Allocated:   ${this.fmtAmt(d.amountAllocated)} `,
                  `Spent:          ${this.fmtAmt(d.amountSpent)} `,
                  `Remaining:  ${this.fmtAmt(d.amountRemaining)} `,
                  `Utilization:  ${util}% `,
                ] as unknown as string;
              },
            },
          },
        },
        scales: {
          x: {
            stacked: false,
            grid: {
              display: false,
            },
            ticks: {
              color: '#94a3b8',
              font: { size: 11, weight: 'bold' as const },
              maxRotation: 0,
              minRotation: 0,
            },
            border: {
              display: false,
            },
          },
          y: {
            stacked: false,
            grid: {
              color: '#f1f5f9',
              lineWidth: 1,
            },
            ticks: {
              color: '#94a3b8',
              font: { size: 10 },
              callback: (value) => {
                const numValue = Number(value);
                if (numValue >= 10_000_000) {
                  return `${(numValue / 10_000_000).toFixed(0)} M`;
                }
                if (numValue >= 1_000_000) {
                  return `${(numValue / 1_000_000).toFixed(1)} M`;
                }
                if (numValue >= 100_000) {
                  return `${(numValue / 100_000).toFixed(0)} L`;
                }
                if (numValue >= 1_000) {
                  return `${(numValue / 1_000).toFixed(0)} K`;
                }
                return numValue.toString();
              },
            },
            border: {
              display: false,
            },
            beginAtZero: true,
          },
        },
        interaction: {
          mode: 'index',
          intersect: false,
        },
      },
      plugins: [barDataLabelPlugin],
    });

    const totalSpent = spent.reduce((sum: number, v: number) => sum + v, 0) || 1;

    this.deptPieChart = new Chart(pieCtx, {
      type: 'doughnut',
      data: {
        labels,
        datasets: [
          {
            data: spent,
            backgroundColor: [
              '#3b82f6',
              '#f59e0b',
              '#10b981',
              '#ec4899',
              '#8b5cf6',
              '#06b6d4',
              '#ef4444',
              '#84cc16',
            ],
            borderWidth: 2,
            borderColor: '#ffffff',
            hoverOffset: 10,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '62%',
        layout: { padding: { top: 4, bottom: 4 } },
        animation: {
          duration: 800,
          easing: 'easeOutQuart',
        },
        plugins: {
          legend: {
            display: true,
            position: 'bottom',
            align: 'center',
            labels: {
              usePointStyle: true,
              pointStyle: 'circle' as const,
              padding: 14,
              font: { size: 11.5, family: 'Inter, system-ui, sans-serif' },
              color: '#475569',
              generateLabels: (chart) => {
                const data = chart.data;
                return (data.labels as string[]).map((label, i) => {
                  const visible = chart.getDataVisibility(i);
                  return {
                    text: label,
                    fillStyle: (data.datasets[0].backgroundColor as string[])[i],
                    strokeStyle: (data.datasets[0].backgroundColor as string[])[i],
                    lineWidth: 0,
                    pointStyle: 'circle' as const,
                    hidden: !visible,
                    index: i,
                    datasetIndex: 0,
                    fontColor: !visible ? '#94a3b8' : '#475569',
                  };
                });
              },
            },
            maxWidth: 600,
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleFont: { size: 12, weight: 'bold' as const, family: 'Inter, system-ui, sans-serif' },
            bodyFont: { size: 12, family: 'Inter, system-ui, sans-serif' },
            padding: 12,
            cornerRadius: 8,
            displayColors: true,
            callbacks: {
              label: (ctx) => {
                const value = ctx.parsed ?? 0;
                const pct = (value / totalSpent) * 100;
                return `${ctx.label}: ${this.fmtAmt(value)} (${pct.toFixed(1)}%)`;
              },
            },
          },
        },
      },
      plugins: [pieDataLabelPlugin],
    });
  }

  private renderBudgetStatusChart(): void {
    if (typeof window === 'undefined') return;
    if (!this.budgetStatusDonutRef) return;
    const report = this.budgetReport();
    if (!report) return;

    this.ensureChartsRegistered();

    this.budgetStatusDonutChart?.destroy();

    const ctx = this.budgetStatusDonutRef.nativeElement.getContext('2d')!;

    const pending = report.pendingExpenseCount ?? 0;
    const approved = report.approvedExpenseCount ?? 0;
    const rejected = report.rejectedExpenseCount ?? 0;

    const statuses = [`Approved(${approved})`, `Pending(${pending})`, `Rejected(${rejected})`];
    const values = [approved, pending, rejected];
    const colors = ['#16a34a', '#f59e0b', '#ef4444'];

    const total = values.reduce((sum, v) => sum + v, 0) || 1;

    const pieDataLabelPlugin = this.buildPieDataLabelsPlugin('budgetStatusDataLabels');
    const centerTextPlugin = this.buildCenterTextPlugin('budgetApprovalCenter', () => ({
      title: 'Approval Rate',
      value: `${this.safePercent(report.approvalRate)}% `,
    }));

    this.budgetStatusDonutChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: statuses,
        datasets: [
          {
            data: values,
            backgroundColor: colors,
            borderWidth: 2,
            borderColor: '#ffffff',
            hoverOffset: 10,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '62%',
        layout: { padding: { top: 4, bottom: 4 } },
        animation: {
          duration: 800,
          easing: 'easeOutQuart',
        },
        plugins: {
          legend: {
            display: true,
            position: 'bottom',
            align: 'center',
            labels: {
              usePointStyle: true,
              pointStyle: 'circle' as const,
              padding: 14,
              font: { size: 11.5, family: 'Inter, system-ui, sans-serif' },
              color: '#475569',
              generateLabels: (chart) => {
                const data = chart.data;
                return (data.labels as string[]).map((label, i) => {
                  const visible = chart.getDataVisibility(i);
                  return {
                    text: label,
                    fillStyle: (data.datasets[0].backgroundColor as string[])[i],
                    strokeStyle: (data.datasets[0].backgroundColor as string[])[i],
                    lineWidth: 0,
                    pointStyle: 'circle' as const,
                    hidden: !visible,
                    index: i,
                    datasetIndex: 0,
                    fontColor: !visible ? '#94a3b8' : '#475569',
                  };
                });
              },
            },
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleFont: { size: 12, weight: 'bold' as const, family: 'Inter, system-ui, sans-serif' },
            bodyFont: { size: 12, family: 'Inter, system-ui, sans-serif' },
            padding: 12,
            cornerRadius: 8,
            displayColors: true,
            callbacks: {
              label: (ctx) => {
                const value = ctx.parsed ?? 0;
                const pct = (value / total) * 100;
                return `${ctx.label}: ${value} (${pct.toFixed(1)}%)`;
              },
            },
          },
        },
      },
      plugins: [pieDataLabelPlugin, centerTextPlugin],
    });
  }

  private buildBarDataLabelsPlugin(id: string): any {
    return {
      id,
      afterDatasetsDraw: (chart: Chart) => {
        const { ctx } = chart;
        ctx.save();
        chart.data.datasets.forEach((dataset, datasetIndex) => {
          const meta = chart.getDatasetMeta(datasetIndex);
          meta.data.forEach((element: any, index: number) => {
            const rawValue = dataset.data?.[index] as number | undefined;
            if (rawValue == null) return;
            const position = element.tooltipPosition();
            ctx.fillStyle = '#020617';
            ctx.font =
              '500 10px system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'bottom';
            const offset = 6 + datasetIndex * 12;
            const label =
              rawValue >= 100000 ? this.fmtAmt(rawValue) : `₹${rawValue.toLocaleString('en-IN')} `;
            ctx.fillText(label, position.x, position.y - offset);
          });
        });
        ctx.restore();
      },
    };
  }

  private buildPieDataLabelsPlugin(id: string): any {
    return {
      id,
      afterDatasetsDraw: (chart: Chart) => {
        const { ctx } = chart;
        const dataset = chart.data.datasets[0];
        const meta = chart.getDatasetMeta(0);
        const values = (dataset.data as number[]) ?? [];
        const total = values.reduce((sum, v) => sum + v, 0) || 1;

        ctx.save();
        meta.data.forEach((element: any, index: number) => {
          const value = values[index];
          if (!value) return;
          const pct = (value / total) * 100;
          const position = element.tooltipPosition();
          ctx.fillStyle = '#0f172a';
          ctx.font =
            '500 10px system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';
          ctx.textAlign = 'center';
          ctx.textBaseline = 'middle';
          ctx.fillText(`${pct.toFixed(1)}% `, position.x, position.y);
        });
        ctx.restore();
      },
    };
  }

  private buildCenterTextPlugin(id: string, getText: () => { title: string; value: string }): any {
    return {
      id,
      afterDraw: (chart: Chart) => {
        const { ctx } = chart;
        const { top, bottom, left, right } = chart.chartArea;
        const centerX = (left + right) / 2;
        const centerY = (top + bottom) / 2;
        const { title, value } = getText();

        ctx.save();
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = '#64748b';
        ctx.font = '500 13px system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';
        ctx.fillText(title, centerX, centerY - 12);
        ctx.fillStyle = '#0f172a';
        ctx.font =
          'normal 26px system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';
        ctx.fillText(value, centerX, centerY + 14);
        ctx.restore();
      },
    };
  }
  statusColor(status: string): string {
    switch (status) {
      case 'Approved':
        return '#10b981';
      case 'Pending':
        return '#f59e0b';
      case 'Rejected':
        return '#ef4444';
      default:
        return '#6b7280';
    }
  }

  statusBg(status: string): string {
    switch (status) {
      case 'Approved':
        return '#ecfdf5';
      case 'Pending':
        return '#fef3c7';
      case 'Rejected':
        return '#fef2f2';
      default:
        return '#f3f4f6';
    }
  }

  private renderBudgetBarChart(): void {
    if (typeof window === 'undefined') return;
    if (!this.budgetBarChartRef) return;
    const report = this.budgetReport();
    if (!report) return;

    this.ensureChartsRegistered();
    this.budgetFinancialBarChart?.destroy();

    const ctx = this.budgetBarChartRef.nativeElement.getContext('2d')!;
    const labels = [`${report.budgetCode} - ${report.budgetTitle} `];

    this.budgetFinancialBarChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Allocated',
            data: [report.allocatedAmount],
            backgroundColor: '#3b82f6',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
          {
            label: 'Remaining',
            data: [report.amountRemaining],
            backgroundColor: '#ec4899',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
          {
            label: 'Spent',
            data: [report.amountSpent],
            backgroundColor: '#10b981',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: { duration: 800, easing: 'easeOutQuart' },
        plugins: {
          legend: {
            display: true,
            position: 'top',
            align: 'end',
            labels: {
              usePointStyle: true,
              pointStyle: 'circle',
              padding: 16,
              font: { size: 11, weight: 'normal' as const },
              color: '#64748b',
            },
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleFont: { size: 12, weight: 'bold' as const },
            bodyFont: { size: 11 },
            padding: 12,
            cornerRadius: 8,
            displayColors: false,
            callbacks: {
              title: () => report.budgetTitle,
              label: (ctx) => {
                if (ctx.datasetIndex !== 0) return '';
                const util = report.allocatedAmount > 0
                  ? ((report.amountSpent / report.allocatedAmount) * 100).toFixed(1)
                  : '0.0';
                return [
                  `Allocated:   ${this.fmtAmt(report.allocatedAmount)} `,
                  `Spent:          ${this.fmtAmt(report.amountSpent)} `,
                  `Remaining:  ${this.fmtAmt(report.amountRemaining)} `,
                  `Utilization:  ${util}% `,
                ] as unknown as string;
              },
            },
          },
        },
        scales: {
          x: {
            stacked: false,
            grid: { display: false },
            ticks: { color: '#94a3b8', font: { size: 11, weight: 'bold' as const } },
            border: { display: false },
          },
          y: {
            stacked: false,
            grid: { color: '#f1f5f9', lineWidth: 1 },
            ticks: {
              color: '#94a3b8',
              font: { size: 10 },
              callback: (val) => {
                const v = Number(val);
                if (v >= 10_000_000) return `${(v / 10_000_000).toFixed(0)} M`;
                if (v >= 1_000_000) return `${(v / 1_000_000).toFixed(1)} M`;
                if (v >= 100_000) return `${(v / 100_000).toFixed(0)} L`;
                if (v >= 1_000) return `${(v / 1_000).toFixed(0)} K`;
                return v.toString();
              },
            },
            border: { display: false },
            beginAtZero: true,
          },
        },
        interaction: {
          mode: 'index',
          intersect: false,
        },
      },
    });
  }

  private renderBudgetCategoryPie(): void {
    if (typeof window === 'undefined') return;
    if (!this.budgetCategoryPieRef) return;
    const report = this.budgetReport();
    if (!report) return;

    this.ensureChartsRegistered();
    this.budgetCategoryPieChart?.destroy();

    const ctx = this.budgetCategoryPieRef.nativeElement.getContext('2d')!;

    // Build category totals from approved expenses only
    const map = new Map<string, number>();
    report.expenses
      .filter((e) => e.status === 'Approved')
      .forEach((e) => map.set(e.category, (map.get(e.category) ?? 0) + e.amount));

    const sorted = [...map.entries()].sort((a, b) => b[1] - a[1]);
    const colors = [
      '#3b82f6',
      '#f59e0b',
      '#10b981',
      '#ec4899',
      '#8b5cf6',
      '#06b6d4',
      '#ef4444',
      '#84cc16',
    ];

    if (!sorted.length) return;

    this.budgetCategoryPieChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: sorted.map(([name]) => name),
        datasets: [
          {
            data: sorted.map(([, v]) => v),
            backgroundColor: sorted.map((_, i) => colors[i] ?? '#94a3b8'),
            borderWidth: 2,
            borderColor: '#ffffff',
            hoverOffset: 10,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '62%',
        layout: { padding: { top: 4, bottom: 4 } },
        plugins: {
          legend: {
            display: true,
            position: 'bottom',
            align: 'center',
            labels: {
              usePointStyle: true,
              pointStyle: 'circle' as const,
              padding: 14,
              font: { size: 11.5, family: 'Inter, system-ui, sans-serif' },
              color: '#475569',
              generateLabels: (chart) => {
                const data = chart.data;
                return (data.labels as string[]).map((label, i) => {
                  const visible = chart.getDataVisibility(i);
                  return {
                    text: label,
                    fillStyle: (data.datasets[0].backgroundColor as string[])[i],
                    strokeStyle: (data.datasets[0].backgroundColor as string[])[i],
                    lineWidth: 0,
                    pointStyle: 'circle' as const,
                    hidden: !visible,
                    index: i,
                    datasetIndex: 0,
                    fontColor: !visible ? '#94a3b8' : '#475569',
                  };
                });
              },
            },
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleFont: { size: 12, weight: 'bold' as const, family: 'Inter, system-ui, sans-serif' },
            bodyFont: { size: 12, family: 'Inter, system-ui, sans-serif' },
            padding: 12,
            cornerRadius: 8,
            displayColors: true,
            callbacks: {
              label: (ctx) => {
                const total = (ctx.dataset.data as number[]).reduce((s, v) => s + (v as number), 0);
                const val = ctx.parsed as number;
                const pct = total > 0 ? ((val / total) * 100).toFixed(1) : '0';
                return `  \u20B9${val.toLocaleString('en-IN')} (${pct}%)`;
              },
            },
          },
        },
      },
    });
  }

  downloadReport(): void {
    const tab = this.activeTab();
    let workbook: XLSX.WorkBook | null = null;
    let fileName = 'Report';

    if (tab === 'period' && this.periodReport()) {
      const report = this.periodReport()!;
      fileName = `Period_Report_${this.periodStart}_to_${this.periodEnd}`;
      workbook = XLSX.utils.book_new();

      // Summary sheet
      const summary = [
        { 'Metric': 'Period Start', 'Value': report.startDate },
        { 'Metric': 'Period End', 'Value': report.endDate },
        { 'Metric': 'Total Budgets', 'Value': report.totalBudgetCount },
        { 'Metric': 'Total Allocated (Rs.)', 'Value': report.totalBudgetAmount },
        { 'Metric': 'Total Spent (Rs.)', 'Value': report.totalBudgetAmountSpent },
        { 'Metric': 'Total Remaining (Rs.)', 'Value': report.totalBudgetAmountRemaining },
        { 'Metric': 'Utilization (%)', 'Value': report.utilizationPercentage },
      ];
      const summarySheet = XLSX.utils.json_to_sheet(summary);
      summarySheet['!cols'] = [{ wch: 28 }, { wch: 20 }];
      XLSX.utils.book_append_sheet(workbook, summarySheet, 'Summary');

      // Budget breakdown sheet
      const budgets = report.budgets.map(b => ({
        'Budget Code': b.budgetCode,
        'Title': b.budgetTitle,
        'Allocated (Rs.)': b.allocatedAmount,
        'Spent (Rs.)': b.amountSpent,
        'Remaining (Rs.)': b.amountRemaining,
        'Utilization (%)': b.utilizationPercentage,
      }));
      const budgetsSheet = XLSX.utils.json_to_sheet(budgets);
      budgetsSheet['!cols'] = Object.keys(budgets[0] || {}).map(k => ({
        wch: Math.max(k.length, ...budgets.map(r => String(r[k as keyof typeof r] ?? '').length)) + 2
      }));
      XLSX.utils.book_append_sheet(workbook, budgetsSheet, 'Budget Breakdown');

    } else if (tab === 'department' && this.deptReport()) {
      const report = this.deptReport()!;
      fileName = `Department_Report`;
      workbook = XLSX.utils.book_new();

      // Summary sheet
      const summary = [
        { 'Metric': 'Total Allocated (Rs.)', 'Value': report.totalbudgetAmount },
        { 'Metric': 'Total Spent (Rs.)', 'Value': report.totalbudgetAmountUsed },
        { 'Metric': 'Total Remaining (Rs.)', 'Value': report.totalbudgetAmountRemaining },
        { 'Metric': 'Utilization (%)', 'Value': report.totalbudgetUtilizationPercentage },
        { 'Metric': 'Total Departments', 'Value': report.totalDepartmentcount },
      ];
      const summarySheet = XLSX.utils.json_to_sheet(summary);
      summarySheet['!cols'] = [{ wch: 28 }, { wch: 20 }];
      XLSX.utils.book_append_sheet(workbook, summarySheet, 'Summary');

      // Departments sheet
      const depts = report.departments.map(d => ({
        'Department Code': d.departmentCode,
        'Department Name': d.departmentName,
        'Allocated (Rs.)': d.amountAllocated,
        'Spent (Rs.)': d.amountSpent,
        'Remaining (Rs.)': d.amountRemaining,
        'Utilization (%)': d.utilizationPercentage,
        'Budget Count': d.budgetcount,
        'Expense Count': d.expensecount,
      }));
      const deptsSheet = XLSX.utils.json_to_sheet(depts);
      deptsSheet['!cols'] = Object.keys(depts[0] || {}).map(k => ({
        wch: Math.max(k.length, ...depts.map(r => String(r[k as keyof typeof r] ?? '').length)) + 2
      }));
      XLSX.utils.book_append_sheet(workbook, deptsSheet, 'Departments');

    } else if (tab === 'budget' && this.budgetReport()) {
      const report = this.budgetReport()!;
      fileName = `Budget_Report_${report.budgetCode}`;
      workbook = XLSX.utils.book_new();

      // Summary sheet
      const summary = [
        { 'Metric': 'Budget Code', 'Value': report.budgetCode },
        { 'Metric': 'Title', 'Value': report.budgetTitle },
        { 'Metric': 'Department', 'Value': report.departmentName },
        { 'Metric': 'Manager', 'Value': report.managerName },
        { 'Metric': 'Manager Employee ID', 'Value': report.managerEmployeeId },
        { 'Metric': 'Allocated (Rs.)', 'Value': report.allocatedAmount },
        { 'Metric': 'Spent (Rs.)', 'Value': report.amountSpent },
        { 'Metric': 'Remaining (Rs.)', 'Value': report.amountRemaining },
        { 'Metric': 'Utilization (%)', 'Value': report.utilizationPercentage },
        { 'Metric': 'Status', 'Value': report.status },
        { 'Metric': 'Start Date', 'Value': report.startDate },
        { 'Metric': 'End Date', 'Value': report.endDate },
        { 'Metric': 'Days Remaining', 'Value': report.daysRemaining },
        { 'Metric': 'Total Expenses', 'Value': report.totalExpenseCount },
        { 'Metric': 'Pending', 'Value': report.pendingExpenseCount },
        { 'Metric': 'Approved', 'Value': report.approvedExpenseCount },
        { 'Metric': 'Rejected', 'Value': report.rejectedExpenseCount },
        { 'Metric': 'Approval Rate (%)', 'Value': report.approvalRate },
      ];
      const summarySheet = XLSX.utils.json_to_sheet(summary);
      summarySheet['!cols'] = [{ wch: 26 }, { wch: 30 }];
      XLSX.utils.book_append_sheet(workbook, summarySheet, 'Summary');

      // Expenses sheet
      if (report.expenses.length > 0) {
        const expenses = report.expenses.map(e => ({
          'Title': e.title,
          'Merchant': e.merchantName ?? '',
          'Category': e.category,
          'Amount (Rs.)': e.amount,
          'Status': e.status,
          'Submitted By': e.submittedBy,
          'Employee ID': e.submittedEmployeeId,
          'Submitted Date': e.submittedDate.slice(0, 10),
        }));
        const expSheet = XLSX.utils.json_to_sheet(expenses);
        expSheet['!cols'] = Object.keys(expenses[0]).map(k => ({
          wch: Math.max(k.length, ...expenses.map(r => String(r[k as keyof typeof r] ?? '').length)) + 2
        }));
        XLSX.utils.book_append_sheet(workbook, expSheet, 'Expenses');
      }

      // Category Breakdown sheet (approved expenses only — mirrors Category Distribution chart)
      const categoryMap = new Map<string, number>();
      report.expenses
        .filter(e => e.status === 'Approved')
        .forEach(e => categoryMap.set(e.category, (categoryMap.get(e.category) ?? 0) + e.amount));
      if (categoryMap.size > 0) {
        const catTotal = Array.from(categoryMap.values()).reduce((s, v) => s + v, 0);
        const catRows = [...categoryMap.entries()]
          .sort((a, b) => b[1] - a[1])
          .map(([cat, amt]) => ({
            'Category': cat,
            'Approved Amount (Rs.)': amt,
            'Share (%)': catTotal > 0 ? parseFloat(((amt / catTotal) * 100).toFixed(2)) : 0,
          }));
        const catSheet = XLSX.utils.json_to_sheet(catRows);
        catSheet['!cols'] = Object.keys(catRows[0]).map(k => ({
          wch: Math.max(k.length, ...catRows.map(r => String(r[k as keyof typeof r] ?? '').length)) + 2
        }));
        XLSX.utils.book_append_sheet(workbook, catSheet, 'Category Breakdown');
      }

      // Employee Expenses sheet
      const empStats = this.employeeStats();
      if (empStats.length > 0) {
        const empRows = empStats.map(emp => ({
          'Employee ID': emp.employeeId,
          'Name': emp.name,
          'Total': emp.total,
          'Approved': emp.approved,
          'Pending': emp.pending,
          'Rejected': emp.rejected,
        }));
        const empSheet = XLSX.utils.json_to_sheet(empRows);
        empSheet['!cols'] = Object.keys(empRows[0]).map(k => ({
          wch: Math.max(k.length, ...empRows.map(r => String(r[k as keyof typeof r] ?? '').length)) + 2
        }));
        XLSX.utils.book_append_sheet(workbook, empSheet, 'Employee Expenses');
      }

    } else {
      this.toast.warning('No report data to export. Please generate a report first.');
      return;
    }

    const dateStr = new Date().toISOString().slice(0, 10);
    XLSX.writeFile(workbook!, `${fileName}_${dateStr}.xlsx`);
    this.toast.success('Report downloaded successfully.');
  }
}