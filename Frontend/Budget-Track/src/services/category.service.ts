import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { CategoryDto, CreateCategoryDto, UpdateCategoryDto } from '@models/category.models';
import { ApiResponse } from '@models/auth.models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl;

    getCategories(): Observable<CategoryDto[]> {
        return this.http.get<CategoryDto[]>(`${this.apiUrl}/api/categories`);
    }

    createCategory(dto: CreateCategoryDto): Observable<{ categoryId: number; message: string }> {
        return this.http.post<{ categoryId: number; message: string }>(`${this.apiUrl}/api/categories`, dto);
    }

    updateCategory(categoryId: number, dto: UpdateCategoryDto): Observable<ApiResponse> {
        return this.http.put<ApiResponse>(`${this.apiUrl}/api/categories/${categoryId}`, dto);
    }

    deleteCategory(categoryId: number): Observable<ApiResponse> {
        return this.http.delete<ApiResponse>(`${this.apiUrl}/api/categories/${categoryId}`);
    }
}
