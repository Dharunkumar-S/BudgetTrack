import { PagedResult, PaginationParams } from './pagination.models';

// Expense status enum
export enum ExpenseStatus {
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
}

export interface ExpenseDetailDto {
    expenseID: number;
    budgetID: number;
    categoryID: number;
    categoryName: string;
    title: string;
    amount: number;
    merchantName: string;
    status: number;
    statusName: string;
    submittedDate: string;
    submittedByUserID: number;
    submittedByUserName: string;
    submittedByEmployeeID: string;
    approvedByUserID: number | null;
    approvedByUserName: string | null;
    approvalComments: string | null;
    rejectionReason: string | null;
    notes: string | null;
}

export interface CreateExpenseDto {
    budgetId: number;
    categoryId: number;
    title: string;
    amount: number;
    merchantName: string;
    notes?: string;
}

export interface UpdateExpenseStatusDto {
    status: number;        // 2=Approved, 3=Rejected
    comments?: string;    // for approval
    reason?: string;      // for rejection
}

export interface ExpenseListParams extends PaginationParams {
    title?: string;
    status?: string;
    categoryName?: string;
    submittedUserName?: string;
    submittedByEmployeeID?: string;
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
}

export interface AllExpenseDto {
    expenseID: number;
    budgetID: number;
    budgetTitle: string;
    budgetCode: string;
    categoryID: number;
    categoryName: string;
    title: string;
    amount: number;
    merchantName: string;
    status: number;
    statusName: string;
    submittedDate: string;
    submittedByUserID: number;
    submittedByUserName: string;
    submittedByEmployeeID: string;
    departmentName: string;
    approvedByUserID: number | null;
    approvedByUserName: string | null;
    approvedDate: string | null;
    approvalComments: string | null;
    rejectionReason: string | null;
    notes: string | null;
    createdDate: string;
    updatedDate: string;
}

export interface AllExpenseListParams extends PaginationParams {
    budgetId?: number;
    title?: string;
    budgetTitle?: string;
    status?: string;
    categoryName?: string;
    submittedUserName?: string;
    submittedByEmployeeID?: string;
    departmentName?: string;
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
    myExpensesOnly?: boolean;
}

export type ExpensePagedResult = PagedResult<ExpenseDetailDto>;
export type AllExpensePagedResult = PagedResult<AllExpenseDto>;

export interface ExpenseStatisticsDto {
    totalCount: number;
    pendingCount: number;
    approvedCount: number;
    rejectedCount: number;
    totalAmount: number;
}
