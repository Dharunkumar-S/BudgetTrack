export interface DepartmentDto {
    departmentID: number;
    departmentName: string;
    departmentCode: string;
    isActive: boolean;
}

export interface CreateDepartmentDto {
    departmentName: string;
}

export interface UpdateDepartmentDto {
    departmentName: string;
    isActive: boolean;
}
