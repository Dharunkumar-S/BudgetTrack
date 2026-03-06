import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * AuthGuard — blocks unauthenticated users from protected routes.
 * On page refresh, attempts to restore session from localStorage tokens first.
 * Redirects to /login only if restoration also fails.
 *
 * During SSG prerendering (server-side), auth is skipped — localStorage is
 * unavailable on the server. The real check runs in the browser on page load.
 */
export const authGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const platformId = inject(PLATFORM_ID);

    // SSG prerendering — skip auth, allow all routes to be statically rendered
    if (!isPlatformBrowser(platformId)) {
        return true;
    }

    // Already authenticated in memory — allow immediately
    if (authService.isAuthenticated()) {
        return true;
    }

    // Try restoring session from stored tokens (page refresh scenario)
    return authService.tryRestoreSession().pipe(
        map(restored => restored ? true : router.createUrlTree(['/login']))
    );
};
