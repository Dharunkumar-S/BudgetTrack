import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { catchError, switchMap, throwError, of } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (
    req: HttpRequest<unknown>,
    next: HttpHandlerFn
) => {
    const platformId = inject(PLATFORM_ID);

    // SSG prerendering — skip all auth handling server-side (no localStorage, no tokens)
    if (!isPlatformBrowser(platformId)) {
        return next(req);
    }

    const authService = inject(AuthService);
    const toastService = inject(ToastService);
    const router = inject(Router);

    // Skip auth for public endpoints
    const publicEndpoints = [
        '/api/auth/login',
        '/api/auth/register',
        '/api/auth/forgotpassword',
        '/api/auth/resetpassword',
        '/api/auth/token/refresh',
        '/api/auth/logout'
    ];

    const isPublicEndpoint = publicEndpoints.some(ep => req.url.includes(ep));

    // Attach access token to every request if available
    const token = authService.accessToken();
    const authReq = token && !isPublicEndpoint ? addToken(req, token) : req;

    return next(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
            // Handle 401 Unauthorized — attempt token refresh
            if (error.status === 401) {
                // Skip refresh for auth endpoints to prevent loops
                if (isPublicEndpoint) {
                    return throwError(() => error);
                }

                // Try refreshing the token
                return authService.handleTokenRefresh().pipe(
                    switchMap((newToken) => {
                        if (newToken) {
                            return next(addToken(req, newToken));
                        }
                        // Token refresh failed - redirect to login
                        toastService.error('Session expired. Please log in again.');
                        router.navigate(['/login']);
                        return throwError(() => new Error('Session expired'));
                    }),
                    catchError((refreshError) => {
                        // Refresh failed - redirect to login
                        authService.clearSession();
                        toastService.error('Session expired. Please log in again.');
                        router.navigate(['/login']);
                        return throwError(() => refreshError);
                    })
                );
            }

            // Handle 403 Forbidden
            if (error.status === 403) {
                toastService.error('You do not have permission to perform this action.');
            }

            // Handle 500 errors
            if (error.status >= 500) {
                const message = error.error?.message ?? 'A server error occurred. Please try again later.';
                toastService.error(message);
            }

            return throwError(() => error);
        })
    );
};

function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
    return req.clone({
        setHeaders: {
            Authorization: `Bearer ${token}`,
            'Content-Type': req.headers.get('Content-Type') || 'application/json'
        }
    });
}
