import { Component, OnInit, AfterViewInit, inject, signal, computed, ViewChild, ElementRef, effect, DestroyRef, PLATFORM_ID } from '@angular/core';
import { NgClass, NgStyle, isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@core/services/auth.service';
import { BudgetService } from '@services/budget.service';
import { ExpenseService } from '@services/expense.service';
import { CategoryService } from '@services/category.service';
import { RefreshService } from '@core/services/refresh.service';
import { BudgetDto } from '@models/budget.models';
import { AllExpenseDto } from '@models/expense.models';
import { CategoryDto } from '@models/category.models';
import { forkJoin } from 'rxjs';
import {
  Chart,
  BarController, BarElement,
  DoughnutController, ArcElement,
  CategoryScale, LinearScale,
  Tooltip, Legend
} from 'chart.js';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NgClass, NgStyle, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private budgetService = inject(BudgetService);
  private expenseService = inject(ExpenseService);
  private categoryService = inject(CategoryService);
  private authService = inject(AuthService);
  private refreshService = inject(RefreshService);
  private destroyRef = inject(DestroyRef);
  private platformId = inject(PLATFORM_ID);

  @ViewChild('barCanvas') barCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('donutCanvas') donutCanvas!: ElementRef<HTMLCanvasElement>;

  loading = signal(true);
  budgets = signal<BudgetDto[]>([]);
  expenses = signal<AllExpenseDto[]>([]);

  /** categoryName → categoryCode lookup */
  private categoryCodeMap = new Map<string, string>();

  private barChart?: Chart;
  private donutChart?: Chart;

  readonly donutColors = ['#3b82f6', '#f59e0b', '#10b981', '#ec4899', '#8b5cf6', '#06b6d4', '#ef4444', '#84cc16'];

  // Budget KPIs
  activeBudgets = computed(() => this.budgets().filter(b => b.statusName === 'Active').length);
  totalAllocated = computed(() => this.budgets().reduce((s, b) => s + b.amountAllocated, 0));
  totalSpent = computed(() => this.budgets().reduce((s, b) => s + b.amountSpent, 0));
  totalRemaining = computed(() => this.budgets().reduce((s, b) => s + b.amountRemaining, 0));
  totalExpenseCount = computed(() => this.expenses().length);

  avgUtilization = computed(() => {
    const list = this.budgets();
    if (!list.length) return '0.0';
    return (list.reduce((s, b) => s + b.utilizationPercentage, 0) / list.length).toFixed(1);
  });

  // Expense KPIs
  pendingExpenseCount = computed(() => this.expenses().filter(e => e.statusName === 'Pending').length);
  approvedExpenseCount = computed(() => this.expenses().filter(e => e.statusName === 'Approved').length);
  rejectedExpenseCount = computed(() => this.expenses().filter(e => e.statusName === 'Rejected').length);
  approvedTotal = computed(() => this.expenses().filter(e => e.statusName === 'Approved').reduce((s, e) => s + e.amount, 0));

  isEmployeeRole = computed(() => this.authService.isEmployee());

  // Top 5 budgets for Budget Overview
  topBudgets = computed(() => this.budgets().slice(0, 6));

  recentExpenses = computed(() => {
    return [...this.expenses()]
      .sort((a, b) => new Date(b.submittedDate).getTime() - new Date(a.submittedDate).getTime())
      .slice(0, 7);
  });

  constructor() {
    Chart.register(
      BarController, BarElement,
      DoughnutController, ArcElement,
      CategoryScale, LinearScale,
      Tooltip, Legend
    );
  }

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.loadData();
    this.refreshService.refresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadData());
  }

  loadData(): void {
    this.loading.set(true);
    const isAdmin = this.authService.isAdmin();
    const isManager = this.authService.isManager();
    const currentUser = this.authService.currentUser();

    const budgetObs = isAdmin
      ? this.budgetService.getAdminBudgets({ pageNumber: 1, pageSize: 50 })
      : this.budgetService.getBudgets({ pageNumber: 1, pageSize: 50 });

    const expenseObs = this.expenseService.getAllExpenses({ pageNumber: 1, pageSize: 200 });
    const categoryObs = this.categoryService.getCategories();

    forkJoin([budgetObs, expenseObs, categoryObs]).subscribe({
      next: ([budgetRes, expenseRes, categories]) => {
        this.budgets.set(budgetRes.data);

        // Filter expenses based on role
        let filteredExpenses = expenseRes.data;
        const isEmployee = this.authService.isEmployee();

        if (isManager || isEmployee) {
          // Manager/Employee sees only expenses under budgets they have access to
          const accessibleBudgetIds = new Set(budgetRes.data.map(b => b.budgetID));
          filteredExpenses = expenseRes.data.filter(e => accessibleBudgetIds.has(e.budgetID));
        }
        this.expenses.set(filteredExpenses);

        // Build name→code lookup
        this.categoryCodeMap.clear();
        (categories as CategoryDto[]).forEach(c => {
          this.categoryCodeMap.set(c.categoryName, c.categoryCode);
        });
        this.loading.set(false);
        this.refreshService.notifyComplete();
        setTimeout(() => this.renderCharts(), 0);
      },
      error: () => { this.loading.set(false); this.refreshService.notifyComplete(false); }
    });
  }

  private renderCharts(): void {
    this.renderBarChart();
    this.renderDonutChart();
  }

  private renderBarChart(): void {
    if (!this.barCanvas) return;
    this.barChart?.destroy();
    const ctx = this.barCanvas.nativeElement.getContext('2d')!;
    const budgets = this.budgets()
      .filter(b => b.statusName === 'Active' && !b.isExpired)
      .slice(0, 5);

    this.barChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: budgets.map(b => b.code),
        datasets: [
          {
            label: 'Allocated',
            data: budgets.map(b => b.amountAllocated),
            backgroundColor: '#3b82f6',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
          {
            label: 'Remaining',
            data: budgets.map(b => b.amountRemaining),
            backgroundColor: '#ec4899',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          },
          {
            label: 'Spent',
            data: budgets.map(b => b.amountSpent),
            backgroundColor: '#10b981',
            borderRadius: 3,
            borderSkipped: false,
            barPercentage: 0.95,
            categoryPercentage: 0.75,
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: {
          duration: 1000,
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
            }
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
                return budgets[idx].title;
              },
              label: (ctx) => {
                const idx = ctx.dataIndex;
                const b = budgets[idx];
                return [
                  `Allocated:  ₹${b.amountAllocated.toLocaleString('en-IN')}`,
                  `Spent:         ₹${b.amountSpent.toLocaleString('en-IN')}`,
                  `Remaining: ₹${b.amountRemaining.toLocaleString('en-IN')}`,
                  `Utilization:  ${b.utilizationPercentage.toFixed(1)}%`,
                ];
              },
              afterLabel: () => ''
            }
          }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: { color: '#94a3b8', font: { size: 11, weight: 'bold' as const } },
            border: { display: false },
          },
          y: {
            grid: { color: '#f1f5f9' },
            ticks: {
              color: '#94a3b8',
              font: { size: 10 },
              callback: (val) => {
                const v = Number(val);
                if (v >= 10_000_000) return (v / 10_000_000).toFixed(0) + 'M';
                if (v >= 100_000) return (v / 100_000).toFixed(0) + 'L';
                if (v >= 1_000) return (v / 1_000).toFixed(0) + 'K';
                return v.toString();
              }
            },
            border: { display: false },
          }
        }
      }
    });
  }

  private renderDonutChart(): void {
    if (!this.donutCanvas) return;
    this.donutChart?.destroy();
    const ctx = this.donutCanvas.nativeElement.getContext('2d')!;

    // Build category totals from approved expenses only, sorted highest → lowest
    const map = new Map<string, number>();
    this.expenses().filter(e => e.statusName === 'Approved').forEach(e => {
      map.set(e.categoryName, (map.get(e.categoryName) || 0) + e.amount);
    });
    const sorted = [...map.entries()].sort((a, b) => b[1] - a[1]);

    // Top 8 shown individually; rest collapsed into "Others"
    const TOP_N = 8;
    const top = sorted.slice(0, TOP_N);
    const rest = sorted.slice(TOP_N);
    const othersTotal = rest.reduce((s, [, v]) => s + v, 0);
    const entries: [string, number][] = othersTotal > 0
      ? [...top, ['Others', othersTotal]]
      : top;

    const colors = [
      '#3b82f6', '#f59e0b', '#10b981', '#ec4899', '#8b5cf6',
      '#06b6d4', '#ef4444', '#84cc16', '#94a3b8', // last = Others grey
    ];

    this.donutChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: entries.map(([name]) => name),
        datasets: [{
          data: entries.map(([, v]) => v),
          backgroundColor: entries.map((_, i) => colors[i] ?? '#94a3b8'),
          borderWidth: 2,
          borderColor: '#ffffff',
          hoverOffset: 10,
        }]
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
            labels: {
              usePointStyle: true,
              pointStyle: 'circle' as const,
              padding: 14,
              font: { size: 11.5, family: 'Inter, system-ui, sans-serif' },
              color: '#475569',
              // Only category name in label; amount/pct shown in tooltip
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
              }
            }
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleFont: { size: 12, weight: 'bold' as const, family: 'Inter, system-ui, sans-serif' },
            bodyFont: { size: 12, family: 'Inter, system-ui, sans-serif' },
            padding: 12,
            cornerRadius: 8,
            displayColors: true,
            callbacks: {
              title: (items) => {
                const label = items[0].label ?? '';
                const code = this.categoryCodeMap.get(label)
                  ?? label.split(/\s+/).map(w => w[0]?.toUpperCase() ?? '').join('');
                const rank = items[0].dataIndex + 1;
                return `#${rank}  [${code}]  ${label}`;
              },
              label: (ctx) => {
                const total = (ctx.dataset.data as number[]).reduce((s, v) => s + (v as number), 0);
                const val = ctx.parsed as number;
                const pct = total > 0 ? ((val / total) * 100).toFixed(1) : '0';
                return `  \u20b9${val.toLocaleString('en-IN')}  (${pct}%)`;
              }
            }
          }
        }
      }
    });
  }

  fmtShort(v: number): string {
    if (v >= 10_000_000) return `${(v / 10_000_000).toFixed(1)}M`;
    if (v >= 100_000) return `${(v / 100_000).toFixed(1)}L`;
    if (v >= 1_000) return `${(v / 1_000).toFixed(1)}K`;
    return v.toFixed(0);
  }

  formatAmt(v: number): string {
    return '₹' + v.toLocaleString('en-IN', { maximumFractionDigits: 0 });
  }

  utilClass(pct: number): string {
    if (pct >= 80) return 'high';
    if (pct >= 50) return 'medium';
    return 'low';
  }

  statusColor(status: string): string {
    switch (status) {
      case 'Approved': return '#10b981';
      case 'Pending': return '#f59e0b';
      case 'Rejected': return '#ef4444';
      default: return '#6b7280';
    }
  }

  statusBg(status: string): string {
    switch (status) {
      case 'Approved': return '#ecfdf5';
      case 'Pending': return '#fef3c7';
      case 'Rejected': return '#fef2f2';
      default: return '#f3f4f6';
    }
  }
}