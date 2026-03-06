import { Injectable, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { Observable, tap, catchError, throwError, BehaviorSubject, filter, take, switchMap, of, map } from 'rxjs';
import { environment } from '../../environments/environment';
import {
    LoginRequest, LoginResponse, RefreshTokenRequest, RefreshTokenResponse,
    ResetPasswordRequest, ApiResponse, UserProfileDto,
} from '../../models/auth.models';

const REFRESH_TOKEN_KEY = 'bt_refresh_token';
const ACCESS_TOKEN_KEY = 'bt_access_token';
const USER_PROFILE_KEY = 'bt_user_profile';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);
    private platformId = inject(PLATFORM_ID);
    private apiUrl = environment.apiUrl;

    // Access token kept in memory only — never persisted
    private _accessToken = signal<string | null>(null);
    private _currentUser = signal<UserProfileDto | null>(null);

    // Public readonly signals
    readonly isAuthenticated = computed(() => !!this._accessToken());
    readonly currentUser = computed(() => this._currentUser());
    readonly userRole = computed(() => this._currentUser()?.roleName ?? null);
    readonly accessToken = computed(() => this._accessToken());

    // Token refresh state management (prevents concurrent refresh loops)
    private isRefreshing = false;
    private refreshTokenSubject = new BehaviorSubject<string | null>(null);

    // ─── Auth API calls ──────────────────────────────────────────────────

    login(credentials: LoginRequest): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${this.apiUrl}/api/auth/login`, credentials).pipe(
            tap((response) => {
                if (response.success && response.token) {
                    this.setTokens(response.token.accessToken, response.token.refreshToken);
                    this.setCurrentUser(response.user);
                }
            })
        );
    }

    logout(): Observable<ApiResponse> {
        // Manually attach the token — the interceptor strips it from publicEndpoints.
        // This ensures the backend can revoke the token server-side.
        const token = this.accessToken();
        const headers: Record<string, string> = token
            ? { Authorization: `Bearer ${token}` }
            : {};

        return this.http.post<ApiResponse>(`${this.apiUrl}/api/auth/logout`, {}, { headers }).pipe(
            tap(() => this.clearSession()),
            catchError(() => {
                // Clear session regardless of API result (expired token, network error, etc.)
                this.clearSession();
                return of({ success: true, message: 'Logged out' } as ApiResponse);
            })
        );
    }

    refreshAccessToken(): Observable<RefreshTokenResponse> {
        const refreshToken = this.getStoredRefreshToken();
        const currentAccessToken = this._accessToken();
        if (!refreshToken || !currentAccessToken) {
            return throwError(() => new Error('No tokens to refresh'));
        }
        const body: RefreshTokenRequest = { accessToken: currentAccessToken, refreshToken };
        return this.http.post<RefreshTokenResponse>(`${this.apiUrl}/api/auth/token/refresh`, body).pipe(
            tap((response) => {
                if (response.token) {
                    this.setTokens(response.token.accessToken, response.token.refreshToken);
                }
            }),
            catchError((err) => {
                this.clearSession();
                return throwError(() => err);
            })
        );
    }

    resetPassword(dto: ResetPasswordRequest): Observable<ApiResponse> {
        return this.http.post<ApiResponse>(`${this.apiUrl}/api/auth/resetpassword`, dto);
    }

    // For queuing multiple 401 requests while refresh is happening
    handleTokenRefresh(): Observable<string | null> {
        if (this.isRefreshing) {
            return this.refreshTokenSubject.pipe(
                filter((token) => token !== null),
                take(1)
            );
        }

        this.isRefreshing = true;
        this.refreshTokenSubject.next(null);

        return this.refreshAccessToken().pipe(
            switchMap((response) => {
                this.isRefreshing = false;
                this.refreshTokenSubject.next(response.token.accessToken);
                return this.refreshTokenSubject.asObservable().pipe(take(1));
            }),
            catchError((err) => {
                this.isRefreshing = false;
                this.clearSession();
                this.router.navigate(['/login']);
                return throwError(() => err);
            })
        );
    }

    // ─── Token management ────────────────────────────────────────────────

    setTokens(accessToken: string, refreshToken: string): void {
        this._accessToken.set(accessToken);
        if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
            localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
        }
    }

    getStoredRefreshToken(): string | null {
        if (isPlatformBrowser(this.platformId)) {
            return localStorage.getItem(REFRESH_TOKEN_KEY);
        }
        return null;
    }

    getStoredAccessToken(): string | null {
        if (isPlatformBrowser(this.platformId)) {
            return localStorage.getItem(ACCESS_TOKEN_KEY);
        }
        return null;
    }

    clearSession(): void {
        this._accessToken.set(null);
        this._currentUser.set(null);
        if (isPlatformBrowser(this.platformId)) {
            localStorage.removeItem(ACCESS_TOKEN_KEY);
            localStorage.removeItem(REFRESH_TOKEN_KEY);
            localStorage.removeItem(USER_PROFILE_KEY);
        }
    }

    /**
     * Attempt to restore session from stored tokens on page refresh.
     * Priority:
     *   1. Cached profile in localStorage  — instant, no network
     *   2. Decode profile from JWT claims  — instant, no network (works even first refresh)
     *   3. Refresh expired token then decode — one network call, no profile fetch needed
     */
    tryRestoreSession(): Observable<boolean> {
        const accessToken = this.getStoredAccessToken();
        const refreshToken = this.getStoredRefreshToken();
        if (!accessToken || !refreshToken) {
            return of(false);
        }

        // ── Fast path 1: cached profile + valid token ──────────────────
        const storedProfile = this.getStoredUserProfile();
        if (storedProfile && !this.isTokenExpired(accessToken)) {
            this._accessToken.set(accessToken);
            this._currentUser.set(storedProfile);
            return of(true);
        }

        // ── Fast path 2: decode profile from JWT claims (no network) ───
        if (!this.isTokenExpired(accessToken)) {
            const profileFromToken = this.decodeUserFromToken(accessToken);
            if (profileFromToken) {
                this._accessToken.set(accessToken);
                this.setCurrentUser(profileFromToken); // also caches to localStorage
                return of(true);
            }
        }

        // ── Token expired — use refresh token to get a new one ─────────
        return this.refreshAccessToken().pipe(
            switchMap((response) => {
                const newToken = response.token?.accessToken;
                if (newToken) {
                    const profileFromNewToken = this.decodeUserFromToken(newToken);
                    if (profileFromNewToken) {
                        this.setCurrentUser(profileFromNewToken);
                        return of(true);
                    }
                }
                // Last resort: fetch profile from API
                return this.http.get<{ success: boolean; data: UserProfileDto }>(`${this.apiUrl}/api/users/profile`).pipe(
                    tap(res => { if (res.success && res.data) this.setCurrentUser(res.data); }),
                    map(() => true),
                    catchError(() => { this.clearSession(); return of(false); })
                );
            }),
            catchError(() => { this.clearSession(); return of(false); })
        );
    }

    setCurrentUser(user: UserProfileDto): void {
        this._currentUser.set(user);
        if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem(USER_PROFILE_KEY, JSON.stringify(user));
        }
    }

    getStoredUserProfile(): UserProfileDto | null {
        if (!isPlatformBrowser(this.platformId)) return null;
        const raw = localStorage.getItem(USER_PROFILE_KEY);
        if (!raw) return null;
        try { return JSON.parse(raw) as UserProfileDto; } catch { return null; }
    }

    /** Returns true if the JWT access token is expired (client-side check, no network). */
    private isTokenExpired(token: string): boolean {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload.exp ? (Date.now() / 1000) > payload.exp : false;
        } catch { return true; }
    }

    /**
     * Decode user profile from JWT claims without any network call.
     * .NET serialises ClaimTypes.* as XML namespace URLs inside the JWT payload.
     */
    private decodeUserFromToken(token: string): UserProfileDto | null {
        try {
            const p = JSON.parse(atob(token.split('.')[1]));
            const userId = parseInt(
                p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? '0'
            );
            const email = p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ?? '';
            if (!userId || !email) return null;

            const firstName = p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] ?? '';
            const lastName = p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname'] ?? '';
            const roleName = p['RoleName'] ?? p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] ?? '';
            const roleId = parseInt(p['RoleID'] ?? '0');
            const employeeId = p['EmployeeId'] ?? '';
            const status = parseInt(p['UserStatus'] ?? '0');
            const managerId = p['ManagerId'] ? parseInt(p['ManagerId']) : null;
            const departmentId = p['DepartmentId'] ? parseInt(p['DepartmentId']) : undefined;

            return {
                id: userId, userId,
                firstName, lastName, email, employeeId,
                departmentId, departmentID: departmentId, departmentName: '',
                managerId, managerID: managerId,
                roleId, roleID: roleId, roleName, status,
                fullName: `${firstName} ${lastName}`.trim(),
            };
        } catch { return null; }
    }

    // ─── Role helpers ────────────────────────────────────────────────────

    hasRole(...roles: string[]): boolean {
        const role = this.userRole();
        if (!role) return false;
        const normalizedRole = role.toLowerCase();
        return roles.some(r => r.toLowerCase() === normalizedRole);
    }

    isAdmin(): boolean {
        const user = this._currentUser();
        if (!user) return false;
        const roleName = (user.roleName || '').toLowerCase().trim();
        const roleId = user.roleId || user.roleID;
        return roleName === 'admin' || roleName === 'administrator' || roleId === 1;
    }

    isManager(): boolean { return this.hasRole('Manager'); }
    isEmployee(): boolean { return this.hasRole('Employee'); }
    isAdminOrManager(): boolean { return this.isAdmin() || this.isManager(); }
}
