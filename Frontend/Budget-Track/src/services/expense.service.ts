import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import {
    ExpenseDetailDto, CreateExpenseDto, UpdateExpenseStatusDto,
    ExpenseListParams, ExpensePagedResult,
    AllExpenseListParams, AllExpensePagedResult, ExpenseStatisticsDto
} from '@models/expense.models';
import { ApiResponse } from '@models/auth.models';

@Injectable({ providedIn: 'root' })
export class ExpenseService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl;

    getExpenseStatistics(params: Partial<AllExpenseListParams> = {}): Observable<ExpenseStatisticsDto> {
        let p = new HttpParams();
        if (params.budgetId) p = p.set('budgetID', params.budgetId.toString());
        if (params.title) p = p.set('title', params.title);
        if (params.budgetTitle) p = p.set('budgetTitle', params.budgetTitle);
        if (params.status) p = p.set('status', params.status);
        if (params.categoryName) p = p.set('categoryName', params.categoryName);
        if (params.submittedUserName) p = p.set('submittedUserName', params.submittedUserName);
        if (params.submittedByEmployeeID) p = p.set('submittedByEmployeeID', params.submittedByEmployeeID);
        if (params.departmentName) p = p.set('departmentName', params.departmentName);
        if (params.myExpensesOnly) p = p.set('myExpensesOnly', params.myExpensesOnly.toString());

        return this.http.get<ExpenseStatisticsDto>(`${this.apiUrl}/api/expenses/stats`, { params: p });
    }

    getAllExpenses(params: AllExpenseListParams = { pageNumber: 1, pageSize: 10 }): Observable<AllExpensePagedResult> {
        let p = new HttpParams()
            .set('pageNumber', params.pageNumber.toString())
            .set('pageSize', params.pageSize.toString());
        if (params.title) p = p.set('title', params.title);
        if (params.budgetTitle) p = p.set('budgetTitle', params.budgetTitle);
        if (params.status) p = p.set('status', params.status);
        if (params.categoryName) p = p.set('categoryName', params.categoryName);
        if (params.submittedUserName) p = p.set('submittedUserName', params.submittedUserName);
        if (params.submittedByEmployeeID) p = p.set('submittedByEmployeeID', params.submittedByEmployeeID);
        if (params.departmentName) p = p.set('departmentName', params.departmentName);
        if (params.sortBy) p = p.set('sortBy', params.sortBy);
        if (params.sortOrder) p = p.set('sortOrder', params.sortOrder);
        return this.http.get<AllExpensePagedResult>(`${this.apiUrl}/api/expenses`, { params: p });
    }

    getManagedExpenses(params: AllExpenseListParams = { pageNumber: 1, pageSize: 10 }): Observable<AllExpensePagedResult> {
        let p = new HttpParams()
            .set('pageNumber', params.pageNumber.toString())
            .set('pageSize', params.pageSize.toString());
        if (params.title) p = p.set('title', params.title);
        if (params.status) p = p.set('status', params.status);
        if (params.categoryName) p = p.set('categoryName', params.categoryName);
        if (params.submittedUserName) p = p.set('submittedUserName', params.submittedUserName);
        if (params.submittedByEmployeeID) p = p.set('submittedByEmployeeID', params.submittedByEmployeeID);
        if (params.sortBy) p = p.set('sortBy', params.sortBy);
        if (params.sortOrder) p = p.set('sortOrder', params.sortOrder);
        if (params.myExpensesOnly !== undefined) p = p.set('myExpensesOnly', params.myExpensesOnly.toString());
        return this.http.get<AllExpensePagedResult>(`${this.apiUrl}/api/expenses/managed`, { params: p });
    }

    getExpensesByBudget(budgetId: number, params: ExpenseListParams = { pageNumber: 1, pageSize: 10 }): Observable<AllExpensePagedResult> {
        let p = new HttpParams()
            .set('pageNumber', params.pageNumber.toString())
            .set('pageSize', params.pageSize.toString());
        if (params.title) p = p.set('title', params.title);
        if (params.status) p = p.set('status', params.status);
        if (params.categoryName) p = p.set('categoryName', params.categoryName);
        if (params.submittedUserName) p = p.set('submittedUserName', params.submittedUserName);
        if (params.submittedByEmployeeID) p = p.set('submittedByEmployeeID', params.submittedByEmployeeID);
        if (params.sortBy) p = p.set('sortBy', params.sortBy);
        if (params.sortOrder) p = p.set('sortOrder', params.sortOrder);
        return this.http.get<AllExpensePagedResult>(`${this.apiUrl}/api/budgets/${budgetId}/expenses`, { params: p });
    }

    createExpense(dto: CreateExpenseDto): Observable<{ expenseId: number; message: string }> {
        return this.http.post<{ expenseId: number; message: string }>(`${this.apiUrl}/api/expenses`, dto);
    }

    updateExpenseStatus(expenseId: number, dto: UpdateExpenseStatusDto): Observable<ApiResponse> {
        return this.http.put<ApiResponse>(`${this.apiUrl}/api/expenses/status/${expenseId}`, dto);
    }
}
