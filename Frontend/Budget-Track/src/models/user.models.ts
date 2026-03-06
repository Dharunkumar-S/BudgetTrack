import { PagedResult, PaginationParams, SortParams } from './pagination.models';

export interface UserDto {
    userId: number;
    firstName: string;
    lastName: string;
    email: string;
    employeeId: string;
    departmentId: number;
    departmentName: string;
    managerId: number | null;
    managerEmployeeId: string | null;
    managerName: string | null;
    roleId: number;
    roleName: string;
    status: number;
    isActive: boolean;
    isDeleted: boolean;
    createdDate: string;
    updatedDate: string;
    fullName: string;
}

export interface CreateUserDto {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    roleID: number;
    departmentID: number;
    managerEmployeeId?: string | null;
}

export interface UpdateUserDto {
    firstName: string;
    lastName: string;
    email: string;
    password?: string;
    roleID: number;
    departmentID: number;
    managerEmployeeId?: string | null;
    status: number;  // 1=Active, 2=Inactive
}

export interface UserListParams extends PaginationParams, SortParams {
    roleId?: number | null;
    search?: string;
    departmentId?: number | null;
    isDeleted?: boolean;
    isActive?: boolean;
}

export type UserPagedResult = PagedResult<UserDto>;
