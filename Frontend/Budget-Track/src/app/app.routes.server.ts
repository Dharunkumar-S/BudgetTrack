import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // All static routes — prerendered at build time (SSG)
  // Guards return true server-side via isPlatformBrowser(); components skip API calls server-side.
  { path: '', renderMode: RenderMode.Prerender },
  { path: 'login', renderMode: RenderMode.Prerender },
  { path: 'dashboard', renderMode: RenderMode.Prerender },
  { path: 'budgets', renderMode: RenderMode.Prerender },
  { path: 'expenses', renderMode: RenderMode.Prerender },
  { path: 'categories', renderMode: RenderMode.Prerender },
  { path: 'departments', renderMode: RenderMode.Prerender },
  { path: 'reports', renderMode: RenderMode.Prerender },
  { path: 'users', renderMode: RenderMode.Prerender },
  { path: 'audits', renderMode: RenderMode.Prerender },
  { path: 'notifications', renderMode: RenderMode.Prerender },
  { path: 'profile', renderMode: RenderMode.Prerender },

  // Dynamic route and catch-all — cannot prerender (unknown IDs / paths)
  { path: 'budgets/:id/expenses', renderMode: RenderMode.Client },
  { path: '**', renderMode: RenderMode.Client },
];
