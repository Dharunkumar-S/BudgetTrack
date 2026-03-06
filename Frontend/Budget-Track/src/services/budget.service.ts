import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { BudgetDto, CreateBudgetDto, UpdateBudgetDto, BudgetListParams, BudgetPagedResult } from '@models/budget.models';
import { ApiResponse } from '@models/auth.models';

@Injectable({ providedIn: 'root' })
export class BudgetService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl;

    // Admin-only: all budgets view
    getAdminBudgets(params: BudgetListParams = { pageNumber: 1, pageSize: 10 }): Observable<BudgetPagedResult> {
        return this.http.get<BudgetPagedResult>(`${this.apiUrl}/api/budgets/admin`, {
            params: this.buildBudgetParams(params),
        });
    }

    // Role-filtered: what the user can see
    getBudgets(params: BudgetListParams = { pageNumber: 1, pageSize: 10 }): Observable<BudgetPagedResult> {
        return this.http.get<BudgetPagedResult>(`${this.apiUrl}/api/budgets`, {
            params: this.buildBudgetParams(params),
        });
    }

    createBudget(dto: CreateBudgetDto): Observable<ApiResponse> {
        return this.http.post<ApiResponse>(`${this.apiUrl}/api/budgets`, dto);
    }

    updateBudget(budgetId: number, dto: UpdateBudgetDto): Observable<ApiResponse> {
        return this.http.put<ApiResponse>(`${this.apiUrl}/api/budgets/${budgetId}`, dto);
    }

    deleteBudget(budgetId: number): Observable<ApiResponse> {
        return this.http.delete<ApiResponse>(`${this.apiUrl}/api/budgets/${budgetId}`);
    }

    private buildBudgetParams(params: BudgetListParams): HttpParams {
        let p = new HttpParams()
            .set('pageNumber', params.pageNumber.toString())
            .set('pageSize', params.pageSize.toString());
        if (params.title) p = p.set('title', params.title);
        if (params.code) p = p.set('code', params.code);
        if (params.status?.length) {
            params.status.forEach(s => { p = p.append('status', s.toString()); });
        }
        if (params.isDeleted != null) p = p.set('isDeleted', params.isDeleted.toString());
        if (params.departmentId) p = p.set('departmentId', params.departmentId.toString());
        if (params.sortBy) p = p.set('sortBy', params.sortBy);
        if (params.sortOrder) p = p.set('sortOrder', params.sortOrder);
        return p;
    }
}
