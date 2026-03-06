// Auth models
export interface LoginRequest {
    email: string;
    password: string;
}

export interface TokenDto {
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
    expiresIn: number;
    tokenType: string;
}

export interface LoginResponse {
    success: boolean;
    message: string;
    user: UserProfileDto;
    token: TokenDto;
}

export interface RefreshTokenRequest {
    accessToken: string;
    refreshToken: string;
}

export interface RefreshTokenResponse {
    success: boolean;
    message: string;
    token: TokenDto;
}

export interface VerifyTokenResponse {
    message: string;
    userId: number;
}

export interface ResetPasswordRequest {
    oldPassword: string;
    newPassword: string;
}

export interface ApiResponse {
    success: boolean;
    message: string;
}

// User profile (from auth + profile endpoints)
export interface UserProfileDto {
    id?: number;
    userId?: number;
    firstName: string;
    lastName: string;
    email: string;
    employeeId: string;
    departmentId?: number;
    departmentID?: number;
    departmentName: string;
    managerId?: number | null;
    managerID?: number | null;
    managerEmployeeId?: string | null;
    managerName?: string | null;
    roleId?: number;
    roleID?: number;
    roleName: string;
    status: number;
    fullName?: string;
}
