// Period report
export interface PeriodReportParams {
    startDate: string;
    endDate: string;
}

export interface PeriodBudgetItem {
    budgetCode: string;
    budgetTitle: string;
    allocatedAmount: number;
    amountSpent: number;
    amountRemaining: number;
    utilizationPercentage: number;
}

export interface PeriodReportDto {
    startDate: string;
    endDate: string;
    totalBudgetCount: number;
    totalBudgetAmount: number;
    totalBudgetAmountSpent: number;
    totalBudgetAmountRemaining: number;
    utilizationPercentage: number;
    budgets: PeriodBudgetItem[];
}

// Department report
export interface DepartmentReportItem {
    departmentCode: string;
    departmentName: string;
    amountAllocated: number;
    amountSpent: number;
    amountRemaining: number;
    utilizationPercentage: number;
    budgetcount: number;
    expensecount: number;
}

export interface DepartmentReportDto {
    totalbudgetAmount: number;
    totalbudgetAmountUsed: number;
    totalbudgetAmountRemaining: number;
    totalbudgetUtilizationPercentage: number;
    totalDepartmentcount: number;
    departments: DepartmentReportItem[];
}

// Budget detail report
export interface BudgetReportExpense {
    title: string;
    merchantName?: string;
    category: string;
    amount: number;
    status: string;
    submittedBy: string;
    submittedEmployeeId: string;
    submittedDate: string;
}

export interface BudgetReportDto {
    budgetCode: string;
    budgetTitle: string;
    departmentName: string;
    managerName: string;
    managerEmployeeId: string;
    allocatedAmount: number;
    amountSpent: number;
    amountRemaining: number;
    startDate: string;
    endDate: string;
    daysRemaining: number;
    status: string;
    isExpired: boolean;
    utilizationPercentage: number;
    totalExpenseCount: number;
    pendingExpenseCount: number;
    approvedExpenseCount: number;
    rejectedExpenseCount: number;
    approvalRate: number;
    expenses: BudgetReportExpense[];
}
