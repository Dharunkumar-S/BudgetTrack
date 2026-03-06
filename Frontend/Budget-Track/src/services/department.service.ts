import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto } from '@models/department.models';
import { ApiResponse } from '@models/auth.models';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl;

    getDepartments(): Observable<DepartmentDto[]> {
        return this.http.get<DepartmentDto[]>(`${this.apiUrl}/api/departments`);
    }

    createDepartment(dto: CreateDepartmentDto): Observable<{ departmentId: number; message: string }> {
        return this.http.post<{ departmentId: number; message: string }>(`${this.apiUrl}/api/departments`, dto);
    }

    updateDepartment(departmentId: number, dto: UpdateDepartmentDto): Observable<ApiResponse> {
        return this.http.put<ApiResponse>(`${this.apiUrl}/api/departments/${departmentId}`, dto);
    }

    deleteDepartment(departmentId: number): Observable<ApiResponse> {
        return this.http.delete<ApiResponse>(`${this.apiUrl}/api/departments/${departmentId}`);
    }
}
