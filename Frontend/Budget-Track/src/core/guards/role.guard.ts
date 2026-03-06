import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * RoleGuard factory — creates a guard for specific role(s).
 * Usage: canActivate: [roleGuard('Admin')] or roleGuard('Admin', 'Manager')
 *
 * Redirects to /dashboard if authenticated but lacking the required role.
 * Redirects to /login if not authenticated at all.
 *
 * During SSG prerendering (server-side), role check is skipped — no user
 * context is available on the server. The real check runs in the browser.
 */
export function roleGuard(...allowedRoles: string[]): CanActivateFn {
    return () => {
        const authService = inject(AuthService);
        const router = inject(Router);
        const platformId = inject(PLATFORM_ID);

        // SSG prerendering — skip role check, allow static rendering
        if (!isPlatformBrowser(platformId)) {
            return true;
        }

        if (!authService.isAuthenticated()) {
            return router.createUrlTree(['/login']);
        }

        if (authService.hasRole(...allowedRoles)) {
            return true;
        }

        // Authenticated but wrong role → send to dashboard
        return router.createUrlTree(['/dashboard']);
    };
}
