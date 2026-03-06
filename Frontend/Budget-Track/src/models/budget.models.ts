import { PagedResult, PaginationParams, SortParams } from './pagination.models';

// Budget status enum
export enum BudgetStatus {
    Active = 1,
    Closed = 2,
}

export interface BudgetDto {
    budgetID: number;
    title: string;
    code: string;
    departmentID: number;
    departmentName: string;
    amountAllocated: number;
    amountSpent: number;
    amountRemaining: number;
    utilizationPercentage: number;
    startDate: string;
    endDate: string;
    status: number;
    statusName: string;
    notes: string | null;
    createdByUserID: number;
    createdByName: string;
    createdByEmployeeID: string;
    createdDate: string;
    updatedDate: string;
    daysRemaining: number;
    isExpired: boolean;
    isOverBudget: boolean;
    isDeleted: boolean;
}

export interface CreateBudgetDto {
    title: string;
    departmentID: number;
    amountAllocated: number;
    startDate: string;
    endDate: string;
    notes?: string;
}

export interface UpdateBudgetDto extends CreateBudgetDto {
    status: number;
}

export interface BudgetListParams extends PaginationParams, SortParams {
    title?: string;
    code?: string;
    status?: number[];
    isDeleted?: boolean;
    departmentId?: number;
}

export type BudgetPagedResult = PagedResult<BudgetDto>;
