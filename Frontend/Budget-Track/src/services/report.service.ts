import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '@env/environment';
import { PeriodReportDto, DepartmentReportDto, BudgetReportDto } from '@models/report.models';

@Injectable({ providedIn: 'root' })
export class ReportService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl;

    /**
     * Get period report with start and end dates
     * Accepts Date objects or date strings, converts to UTC ISO format (YYYY-MM-DDTHH:mm:ss)
     * @param startDate - Start date as Date object or string (YYYY-MM-DD)
     * @param endDate - End date as Date object or string (will include T23:59:59 time)
     */
    getPeriodReport(startDate: Date | string, endDate: Date | string): Observable<PeriodReportDto> {
        // Validate dates
        if (!startDate || !endDate) {
            return throwError(() => new Error('Start date and end date are required'));
        }

        // Convert to Date objects if strings
        const start = typeof startDate === 'string' ? new Date(startDate) : startDate;
        const end = typeof endDate === 'string' ? new Date(endDate) : endDate;

        // Validate Date objects
        if (isNaN(start.getTime()) || isNaN(end.getTime())) {
            return throwError(() => new Error('Invalid date format'));
        }

        // Convert to UTC ISO format
        const startDateTime = this.toUtcIsoString(start, false);
        const endDateTime = this.toUtcIsoString(end, true);

        const params = new HttpParams()
            .set('startDate', startDateTime)
            .set('endDate', endDateTime);

        return this.http.get<PeriodReportDto>(`${this.apiUrl}/api/reports/period`, { params })
            .pipe(
                catchError((err) => this.handleError(err))
            );
    }

    /**
     * Get department report (no parameters required)
     */
    getDepartmentReport(): Observable<DepartmentReportDto> {
        return this.http.get<DepartmentReportDto>(`${this.apiUrl}/api/reports/department`)
            .pipe(
                catchError((err) => this.handleError(err))
            );
    }

    /**
     * Get detailed budget report by budget code
     */
    getBudgetReport(budgetCode: string): Observable<BudgetReportDto> {
        if (!budgetCode || !budgetCode.trim()) {
            return throwError(() => new Error('Budget code is required'));
        }

        const params = new HttpParams().set('budgetCode', budgetCode.trim());
        return this.http.get<BudgetReportDto>(`${this.apiUrl}/api/reports/budget`, { params })
            .pipe(
                catchError((err) => this.handleError(err))
            );
    }

    /**
     * Converts Date to UTC ISO string format (YYYY-MM-DDTHH:mm:ss)
     * Uses UTC methods to avoid timezone issues
     * @param date - The Date object to convert
     * @param isEndDate - If true, sets time to 23:59:59; otherwise 00:00:00
     */
    private toUtcIsoString(date: Date, isEndDate: boolean): string {
        const year = date.getUTCFullYear();
        const month = String(date.getUTCMonth() + 1).padStart(2, '0');
        const day = String(date.getUTCDate()).padStart(2, '0');
        const time = isEndDate ? '23:59:59' : '00:00:00';
        return `${year}-${month}-${day}T${time}`;
    }

    /**
     * Global error handler for HTTP requests
     */
    private handleError(error: HttpErrorResponse): Observable<never> {
        let errorMessage = 'An unknown error occurred';

        if (error.status === 0) {
            // Client-side or network error
            errorMessage = 'Network error: Unable to connect to the server';
        } else if (error.status === 401) {
            // Unauthorized - token expired or invalid
            errorMessage = 'Session expired. Please log in again.';
        } else if (error.status === 403) {
            // Forbidden - insufficient permissions
            errorMessage = 'You do not have permission to access this resource.';
        } else if (error.status === 404) {
            // Not found
            errorMessage = 'The requested resource was not found.';
        } else if (error.status === 400) {
            // Bad request
            errorMessage = error.error?.message ?? 'Invalid request parameters.';
        } else if (error.status === 500) {
            // Server error
            errorMessage = error.error?.message ?? 'A server error occurred. Please try again later.';
        } else if (error.error?.message) {
            // API returned specific error message
            errorMessage = error.error.message;
        }

        return throwError(() => new Error(errorMessage));
    }
}
