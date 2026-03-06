import { Routes } from '@angular/router';
import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';

export const routes: Routes = [
    // Public routes (SSG prerendered)
    {
        path: '',
        title: 'BudgetTrack',
        loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent),
    },
    {
        path: 'login',
        title: 'BudgetTrack',
        loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent),
    },
    // Main authenticated shell
    {
        path: '',
        loadComponent: () => import('./layout/shell/shell.component').then(m => m.ShellComponent),
        canActivate: [authGuard],
        children: [
            {
                path: 'dashboard',
                title: 'BudgetTrack | Dashboard',
                loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
            },
            {
                path: 'budgets',
                title: 'BudgetTrack | Budgets',
                loadComponent: () => import('./features/budgets/budgets-list/budgets-list.component').then(m => m.BudgetsListComponent),
            },
            {
                path: 'budgets/:id/expenses',
                title: 'BudgetTrack | Expenses',
                loadComponent: () => import('./features/expenses/expenses-list/expenses-list.component').then(m => m.ExpensesListComponent),
            },
            {
                path: 'expenses',
                title: 'BudgetTrack | Expenses',
                loadComponent: () => import('./features/expenses/expenses-list/expenses-list.component').then(m => m.ExpensesListComponent),
            },
            {
                path: 'categories',
                title: 'BudgetTrack | Categories',
                canActivate: [roleGuard('Admin', 'Manager')],
                loadComponent: () => import('./features/categories/categories-list/categories-list.component').then(m => m.CategoriesListComponent),
            },
            {
                path: 'departments',
                title: 'BudgetTrack | Departments',
                canActivate: [roleGuard('Admin', 'Manager')],
                loadComponent: () => import('./features/departments/departments-list/departments-list.component').then(m => m.DepartmentsListComponent),
            },
            {
                path: 'reports',
                title: 'BudgetTrack | Reports',
                canActivate: [roleGuard('Admin', 'Manager')],
                loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent),
            },
            {
                path: 'users',
                title: 'BudgetTrack | Users',
                canActivate: [roleGuard('Admin', 'Manager')],
                loadComponent: () => import('./features/users/users-list/users-list.component').then(m => m.UsersListComponent),
            },
            {
                path: 'audits',
                title: 'BudgetTrack | Audit Logs',
                canActivate: [roleGuard('Admin')],
                loadComponent: () => import('./features/audits/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent),
            },
            {
                path: 'notifications',
                title: 'BudgetTrack | Notifications',
                canActivate: [roleGuard('Manager', 'Employee')],
                loadComponent: () => import('./features/notifications/notifications/notifications.component').then(m => m.NotificationsComponent),
            },
            {
                path: 'profile',
                title: 'BudgetTrack | Profile',
                loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent),
            },
            // Default redirect to dashboard
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
        ]
    },
    // Catch-all
    { path: '**', redirectTo: '/login' }
];
