import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { UserDto, CreateUserDto, UpdateUserDto, UserListParams, UserPagedResult } from '@models/user.models';
import { UserProfileDto } from '@models/auth.models';

@Injectable({ providedIn: 'root' })
export class UserService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl;

    getProfile(): Observable<{ success: boolean; data: UserProfileDto }> {
        return this.http.get<{ success: boolean; data: UserProfileDto }>(
            `${this.apiUrl}/api/users/profile`
        );
    }

    getUsers(params: UserListParams = { pageNumber: 1, pageSize: 10 }): Observable<UserPagedResult> {
        let httpParams = new HttpParams()
            .set('pageNumber', params.pageNumber.toString())
            .set('pageSize', params.pageSize.toString());
        if (params.roleId != null) httpParams = httpParams.set('roleId', params.roleId.toString());
        if (params.search) httpParams = httpParams.set('search', params.search);
        if (params.departmentId != null) httpParams = httpParams.set('departmentId', params.departmentId.toString());
        if (params.isDeleted != null) httpParams = httpParams.set('isDeleted', params.isDeleted.toString());
        if (params.isActive != null) httpParams = httpParams.set('isActive', params.isActive.toString());
        if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
        if (params.sortOrder) httpParams = httpParams.set('sortOrder', params.sortOrder);
        return this.http.get<UserPagedResult>(`${this.apiUrl}/api/users`, { params: httpParams });
    }

    getEmployeesByManager(managerUserId: number): Observable<UserDto[]> {
        return this.http.get<UserDto[]>(`${this.apiUrl}/api/users/${managerUserId}/employees`);
    }

    getManagers(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/api/users/managers`);
    }

    createUser(dto: CreateUserDto): Observable<{ success: boolean; message: string; user: UserDto }> {
        return this.http.post<{ success: boolean; message: string; user: UserDto }>(
            `${this.apiUrl}/api/auth/createuser`, dto
        );
    }

    getUserStats(): Observable<{
        totalUsers: number;
        admins: number;
        managers: number;
        employees: number;
        activeUsers: number;
        inactiveUsers: number;
    }> {
        return this.http.get<any>(`${this.apiUrl}/api/users/stats`);
    }

    updateUser(userId: number, dto: UpdateUserDto): Observable<{ success: boolean; message: string }> {
        return this.http.put<{ success: boolean; message: string }>(
            `${this.apiUrl}/api/users/${userId}`, dto
        );
    }

    deleteUser(userId: number): Observable<{ success: boolean; message: string }> {
        return this.http.delete<{ success: boolean; message: string }>(
            `${this.apiUrl}/api/users/${userId}`
        );
    }

    changePassword(payload: { currentPassword: string; newPassword: string }): Observable<{ success: boolean; message: string }> {
        const body = {
            oldPassword: payload.currentPassword,
            newPassword: payload.newPassword
        };
        return this.http.post<{ success: boolean; message: string }>(
            `${this.apiUrl}/api/auth/changepassword`, body
        );
    }
}
