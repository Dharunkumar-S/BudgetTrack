# Budget-Track
### Internal Budget Planning & Expense Management System

> A full-stack enterprise web application for managing departmental budgets, tracking expenses, approving workflows, and generating financial reports — built with Angular 21 and ASP.NET Core (.NET 10).

---

## Table of Contents

1. [Tech Stack](#1-tech-stack)
2. [Architecture Overview](#2-architecture-overview)
3. [Features](#3-features)
4. [Project Structure](#4-project-structure)
5. [Prerequisites](#5-prerequisites)
6. [API Endpoints](#6-api-endpoints)
7. [Authentication & Authorization](#7-authentication--authorization)
8. [Frontend Architecture](#8-frontend-architecture)
   - [Rendering Strategy (SSG)](#rendering-strategy-ssg)
9. [Backend Architecture](#9-backend-architecture)
10. [Database Entities](#10-database-entities)
11. [Stored Procedures & Views](#11-stored-procedures--views)
12. [NuGet Packages](#12-nuget-packages)

---

## 1. Tech Stack

### Frontend

| Technology   | Version | Purpose                                                                                                 |
| ------------ | ------- | ------------------------------------------------------------------------------------------------------- |
| Angular      | 21.1.0  | SPA framework (standalone components, Signals, `@for`/`@if`)                                            |
| Angular SSR  | 21.2.0  | SSG — all static routes prerendered at build time; guards skip auth server-side via `isPlatformBrowser` |
| TypeScript   | ~5.9.2  | Typed language                                                                                          |
| Bootstrap    | ^5.3.8  | CSS utility framework                                                                                   |
| Chart.js     | ^4.5.1  | Dashboard & report charts (bar, doughnut, pie)                                                          |
| Font Awesome | ^7.2.0  | Icon library                                                                                            |
| RxJS         | ~7.8.0  | Reactive streams & HTTP observables                                                                     |
| Angular CLI  | ^21.1.4 | Build toolchain                                                                                         |
| Vitest       | ^4.0.8  | Unit testing                                                                                            |

### Backend

| Technology                           | Version  | Purpose                            |
| ------------------------------------ | -------- | ---------------------------------- |
| ASP.NET Core                         | .NET 10  | REST API framework                 |
| Entity Framework Core                | 10.0.2   | ORM with SQL Server provider       |
| ASP.NET Identity `PasswordHasher<T>` | Built-in | Password hashing (BCrypt-strength) |
| JWT Bearer                           | 10.0.2   | Stateless authentication           |
| Swashbuckle / Swagger                | 6.5.0    | API documentation UI               |
| SQL Server                           | —        | Relational database                |

---

## 2. Architecture Overview

### Build Time (SSG)

```
┌─────────────────────────────────────────────────────────────┐
│                Angular CLI  (ng build)                      │
│                                                             │
│  app.routes.server.ts  →  RenderMode.Prerender  (all static routes)       │
│                        →  RenderMode.Client     (dynamic/:id + catch-all)  │
│  outputMode: "static"  →  Prerenders all known routes to HTML              │
└──────────────────────────┬──────────────────────────────────┘
                           │  Static HTML + JS + CSS
┌──────────────────────────▼──────────────────────────────────┐
│              dist/Budget-Track/browser/                     │
│   index.html · index.csr.html · per-route *.html files      │
│   Deployable to: Nginx · Azure Static Web Apps · CDN        │
└─────────────────────────────────────────────────────────────┘
```

### Runtime (Browser → API → Database)

```
┌─────────────────────────────────────────────────────────────┐
│              Angular 21 (Client Hydration)                  │
│                                                             │
│  authGuard / roleGuard → Components → Services             │
│  HTTP Client  →  auth.interceptor (Bearer JWT)             │
└──────────────────────────┬──────────────────────────────────┘
                           │  HTTPS  (Bearer JWT)
┌──────────────────────────▼──────────────────────────────────┐
│                  ASP.NET Core REST API                       │
│                                                             │
│  Controllers  →  Services  →  Repositories  →  EF Core     │
│                                    │                        │
│             JwtMiddleware  ◄────────┘                       │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                      SQL Server                             │
│         Tables · Views · Stored Procedures                  │
└─────────────────────────────────────────────────────────────┘
```

### Key Patterns

| Pattern            | Description                                                                                                               |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------- |
| Repository Pattern | Each entity has `IRepository` / `Implementation` pair; controllers never touch `DbContext` directly                       |
| Service Layer      | Business logic isolated in `IService` / `Implementation`; repos are injected via DI                                       |
| Soft-Delete        | All major entities have `IsDeleted`, `DeletedDate`, `DeletedByUserID`; EF global query filters exclude them automatically |
| Audit Logging      | Every create / update / delete action writes a row to `tAuditLog` via the service layer                                   |
| JWT Flow           | Login → `access_token` (15 min) + `refresh_token` (7 days stored in DB); `/api/auth/token/refresh` rotates both           |

---

## 3. Features

### Admin
- Create, update, and soft-delete users (Admin / Manager / Employee)
- Manage departments and categories
- View all budgets (including deleted) with real-time utilization metrics
- Approve or override expenses across all departments
- Full audit log viewer with per-user drilldown
- Generate Period, Department, and Budget reports with charts

### Manager
- Create and manage budgets for their department
- Approve or reject expenses submitted by their team
- View team member list and budget utilization
- Receive notifications for expense submissions

### Employee
- Submit expenses against an active budget
- Track expense approval status (Pending / Approved / Rejected)
- View own budget allocations
- Receive notifications for approval decisions

### Common (All Roles)
- Secure login with JWT access token + refresh token rotation
- Change own password
- View and update personal profile
- Real-time notification badge with mark-read / mark-all-read
- Dashboard with KPI cards and Chart.js visualizations

---

## 4. Project Structure

```
Budget-Track/
├── README.md
├── Agent.md
│
├── API/                          # API contract documentation (Markdown)
│   ├── AuthAPI.md
│   ├── UserAPI.md
│   ├── BudgetAPI.md
│   ├── ExpenseAPI.md
│   ├── CategoryAPI.md
│   ├── DepartmentAPI.md
│   ├── AuditAPI.md
│   ├── NotificationAPI.md
│   └── ReportAPI.md
│
├── Backend/
│   └── Budget-Track/
│       ├── Program.cs                    # DI registration, middleware pipeline
│       ├── appsettings.json              # Connection string, JWT config
│       ├── Controllers/                  # Route handlers (thin layer)
│       │   ├── BaseApiController.cs
│       │   ├── AuthController.cs
│       │   ├── UserController.cs
│       │   ├── BudgetController.cs
│       │   ├── ExpenseController.cs
│       │   ├── CategoryController.cs
│       │   ├── DepartmentController.cs
│       │   ├── AuditController.cs
│       │   ├── NotificationController.cs
│       │   └── ReportController.cs
│       ├── Services/
│       │   ├── Interfaces/               # Service contracts
│       │   └── Implementation/           # Business logic
│       ├── Repositories/
│       │   ├── Interfaces/               # Data access contracts
│       │   └── Implementation/           # EF Core queries
│       ├── Models/
│       │   ├── Entities/                 # EF Core entity classes
│       │   ├── DTOs/                     # Request / Response DTOs per feature
│       │   └── Enums/                    # Status, Role, Action enums
│       ├── Data/
│       │   ├── BudgetTrackDbContext.cs   # EF Core context + global filters
│       │   └── DataSeeder.cs            # Development seed data
│       ├── Middleware/
│       │   ├── JwtMiddleware.cs          # Validates token, attaches user to context
│       │   └── JwtSettings.cs            # JWT configuration POCO
│       └── Migrations/                   # EF Core migration history
│
├── Frontend/
│   └── Budget-Track/
│       ├── angular.json
│       ├── tsconfig.json
│       ├── package.json
│       └── src/
│           ├── main.ts                   # Bootstrap (browser)
           ├── main.server.ts            # Bootstrap (SSG build-time prerender)
│           ├── styles.css                # Global styles
│           ├── environments/             # environment.ts / environment.prod.ts
│           ├── models/                   # TypeScript interfaces per domain
│           ├── services/                 # Shared Angular services
│           ├── core/
│           │   ├── guards/               # auth.guard.ts · role.guard.ts
│           │   ├── interceptors/         # auth.interceptor.ts (Bearer token)
│           │   └── services/             # Core singleton services
│           └── app/
│               ├── app.routes.ts         # Lazy-loaded route definitions               ├── app.routes.server.ts  # SSG render mode per route
               ├── app.config.ts         # Browser application config
               ├── app.config.server.ts  # SSG server config (merges browser config)│               ├── auth/                 # Login component
│               ├── layout/               # Shell (sidebar + topbar)
│               ├── shared/               # Reusable UI components
│               └── features/
│                   ├── dashboard/
│                   ├── budgets/
│                   ├── expenses/
│                   ├── categories/
│                   ├── departments/
│                   ├── users/
│                   ├── reports/
│                   ├── audits/
│                   ├── notifications/
│                   └── profile/
│
└── Database/
    └── Budget-Track/
        ├── User.sql
        ├── Budget.sql
        ├── Expense.sql
        ├── Category.sql
        ├── Department.sql
        ├── Notification.sql
        ├── Report.sql
        └── zSelect.sql
```

---

## 5. Prerequisites

| Requirement   | Version | Notes                                                         |
| ------------- | ------- | ------------------------------------------------------------- |
| .NET SDK      | 10.0+   | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Node.js       | 20+ LTS | [nodejs.org](https://nodejs.org)                              |
| npm           | 10+     | Bundled with Node.js                                          |
| Angular CLI   | 21.x    | `npm install -g @angular/cli@21`                              |
| SQL Server    | 2019+   | LocalDB, Developer, or Express edition                        |
| EF Core Tools | 10.0.2  | `dotnet tool install --global dotnet-ef`                      |

### Setup Steps

**1. Database**
```sql
-- Run SQL scripts in order:
-- Database/Budget-Track/User.sql
-- Database/Budget-Track/Department.sql
-- Database/Budget-Track/Category.sql
-- Database/Budget-Track/Budget.sql
-- Database/Budget-Track/Expense.sql
-- Database/Budget-Track/Notification.sql
-- Database/Budget-Track/Report.sql
```

**2. Backend**
```bash
cd Backend/Budget-Track
# Update appsettings.json with your connection string and JWT secret
dotnet restore
dotnet ef database update
dotnet run
# API available at https://localhost:7xxx | Swagger at /swagger
```

**3. Frontend**
```bash
cd Frontend/Budget-Track
npm install

# Development server
ng serve
# App available at http://localhost:4200

# Production SSG build (generates fully static files in dist/)
ng build
# Deploy the dist/Budget-Track/browser/ folder to any static host
# (Azure Static Web Apps, Nginx, GitHub Pages, CDN, etc.)
```

---

## 6. API Endpoints

### Auth — `/api/auth`

| Method | Endpoint                   | Description                                       | Authorization     |
| ------ | -------------------------- | ------------------------------------------------- | ----------------- |
| `POST` | `/api/auth/login`          | Authenticate user, returns access + refresh token | Public            |
| `POST` | `/api/auth/createuser`     | Admin creates a new user account                  | Admin             |
| `POST` | `/api/auth/changepassword` | Change own password                               | Any authenticated |
| `POST` | `/api/auth/token/refresh`  | Rotate access token using refresh token           | Public            |
| `POST` | `/api/auth/logout`         | Revoke refresh token                              | Public            |
| `GET`  | `/api/auth/verify`         | Verify access token validity                      | Any authenticated |

### Users — `/api/users`

| Method   | Endpoint                    | Description                        | Authorization     |
| -------- | --------------------------- | ---------------------------------- | ----------------- |
| `GET`    | `/api/users`                | Paginated, filterable user list    | Admin, Manager    |
| `GET`    | `/api/users/profile`        | Own profile with department & role | Any               |
| `GET`    | `/api/users/stats`          | Role and status counts             | Admin             |
| `GET`    | `/api/users/managers`       | All manager accounts               | Any authenticated |
| `GET`    | `/api/users/{id}/employees` | Employees assigned to a manager    | Any authenticated |
| `PUT`    | `/api/users/{id}`           | Update user details                | Admin             |
| `DELETE` | `/api/users/{id}`           | Soft-delete user                   | Admin             |

### Budgets — `/api/budgets`

| Method   | Endpoint             | Description                                | Authorization     |
| -------- | -------------------- | ------------------------------------------ | ----------------- |
| `GET`    | `/api/budgets/admin` | All budgets including deleted (admin view) | Admin             |
| `GET`    | `/api/budgets`       | Budgets scoped by caller's role/department | Any authenticated |
| `POST`   | `/api/budgets`       | Create a new budget                        | Manager           |
| `PUT`    | `/api/budgets/{id}`  | Update budget                              | Manager           |
| `DELETE` | `/api/budgets/{id}`  | Soft-delete budget                         | Admin             |

### Expenses — `/api/expenses`

| Method   | Endpoint                     | Description                       | Authorization     |
| -------- | ---------------------------- | --------------------------------- | ----------------- |
| `GET`    | `/api/expenses`              | All expenses paginated & filtered | Any authenticated |
| `GET`    | `/api/expenses/stats`        | Expense aggregate statistics      | Any authenticated |
| `POST`   | `/api/expenses`              | Submit a new expense              | Employee          |
| `PUT`    | `/api/expenses/{id}/approve` | Approve an expense                | Manager           |
| `PUT`    | `/api/expenses/{id}/reject`  | Reject an expense with reason     | Manager           |
| `DELETE` | `/api/expenses/{id}`         | Soft-delete expense               | Admin             |

### Categories — `/api/categories`

| Method | Endpoint               | Description           | Authorization     |
| ------ | ---------------------- | --------------------- | ----------------- |
| `GET`  | `/api/categories`      | All active categories | Any authenticated |
| `POST` | `/api/categories`      | Create new category   | Admin, Manager    |
| `PUT`  | `/api/categories/{id}` | Update category name  | Admin, Manager    |

### Departments — `/api/departments`

| Method | Endpoint                | Description            | Authorization     |
| ------ | ----------------------- | ---------------------- | ----------------- |
| `GET`  | `/api/departments`      | All active departments | Any authenticated |
| `POST` | `/api/departments`      | Create new department  | Admin             |
| `PUT`  | `/api/departments/{id}` | Update department name | Admin             |

### Audit Logs — `/api/audits`

| Method | Endpoint               | Description                    | Authorization |
| ------ | ---------------------- | ------------------------------ | ------------- |
| `GET`  | `/api/audits`          | All audit logs paginated       | Admin         |
| `GET`  | `/api/audits/{userId}` | Audit logs for a specific user | Admin         |

### Notifications — `/api/notifications`

| Method | Endpoint                       | Description                      | Authorization     |
| ------ | ------------------------------ | -------------------------------- | ----------------- |
| `GET`  | `/api/notifications`           | Own notifications paginated      | Any authenticated |
| `PUT`  | `/api/notifications/read/{id}` | Mark single notification as read | Any authenticated |
| `PUT`  | `/api/notifications/readAll`   | Mark all notifications as read   | Any authenticated |

### Reports — `/api/reports`

| Method | Endpoint                  | Description                                          | Authorization     |
| ------ | ------------------------- | ---------------------------------------------------- | ----------------- |
| `GET`  | `/api/reports/period`     | Budget summary filtered by date range                | Admin             |
| `GET`  | `/api/reports/department` | Budget & expense stats grouped by department         | Admin             |
| `GET`  | `/api/reports/budget`     | Detailed single-budget report with expense breakdown | Any authenticated |

---

## 7. Authentication & Authorization

### JWT Token Flow

```
1. POST /api/auth/login  { email, password }
        ↓
2. Server validates credentials, returns:
   {
     "accessToken":  "<JWT, 15 min>",
     "refreshToken": "<opaque token, 7 days>"
   }
        ↓
3. Angular stores tokens; auth.interceptor.ts attaches:
   Authorization: Bearer <accessToken>
        ↓
4. JwtMiddleware validates signature + expiry on every request,
   attaches UserID, Email, Role to HttpContext.Items
        ↓
5. When accessToken expires:
   POST /api/auth/token/refresh  { refreshToken }
   → new accessToken + new refreshToken returned
```

### Role-Based Authorization

| Role         | Level | Capabilities                                                                        |
| ------------ | ----- | ----------------------------------------------------------------------------------- |
| **Admin**    | 1     | Full system access — user management, all budgets, audit logs, reports, departments |
| **Manager**  | 2     | Own-department budgets, expense approval, team visibility, reports (filtered)       |
| **Employee** | 3     | Submit expenses, view assigned budgets, own profile, notifications                  |

### Security Measures

- Passwords hashed with ASP.NET Identity `PasswordHasher<User>` (PBKDF2 + salt)
- Refresh tokens stored as hashed values in `tUser.RefreshToken`
- Soft-deleted users have their email and EmployeeID scrambled to free unique DB indexes
- All protected routes guarded on both frontend (`authGuard`, `roleGuard`) and backend (`[Authorize(Roles = "...")]`)

---

## 8. Frontend Architecture

### Standalone Components & Lazy Loading

Every component is `standalone: true`. Routes use `loadComponent()` for code-splitting:

```typescript
{
  path: 'budgets',
  loadComponent: () =>
    import('./features/budgets/budgets-list/budgets-list.component')
      .then(m => m.BudgetsListComponent),
}
```

### Signals

Component state is managed with Angular Signals (`signal()`, `computed()`, `effect()`), avoiding `ngOnChanges` boilerplate.

### Route Guards

| Guard              | File                        | Purpose                                 |
| ------------------ | --------------------------- | --------------------------------------- |
| `authGuard`        | `core/guards/auth.guard.ts` | Redirects to `/login` if no valid token |
| `roleGuard(roles)` | `core/guards/role.guard.ts` | Blocks routes for insufficient role     |

**Route access summary:**

| Route                   | Roles Allowed     |
| ----------------------- | ----------------- |
| `/dashboard`            | All               |
| `/budgets`              | All               |
| `/budgets/:id/expenses` | All               |
| `/expenses`             | All               |
| `/categories`           | Admin, Manager    |
| `/departments`          | Admin, Manager    |
| `/reports`              | Admin, Manager    |
| `/users`                | Admin, Manager    |
| `/audits`               | Admin only        |
| `/notifications`        | Manager, Employee |
| `/profile`              | All               |

### Rendering Strategy

All known static routes are **prerendered at build time (SSG)**. Every feature component guards its `ngOnInit` with `isPlatformBrowser(this.platformId)` — API calls are skipped during prerender and run only in the browser. Guards (`authGuard`, `roleGuard`) and the interceptor likewise use `isPlatformBrowser()` to pass through server-side without touching localStorage.

Only dynamic and catch-all routes remain `RenderMode.Client` because their path segments are unknown at build time.

Session restore on page refresh is handled entirely in-browser without any API call: `tryRestoreSession()` decodes the user profile from the stored JWT claims or reads it from the `bt_user_profile` localStorage cache — no `/api/users/profile` request needed.

| Route                   | Render Mode      | Notes                                                            |
| ----------------------- | ---------------- | ---------------------------------------------------------------- |
| `/`                     | Prerender (SSG)  | Public — no API calls                                            |
| `/login`                | Prerender (SSG)  | Public — no API calls                                            |
| `/dashboard`            | Prerender (SSG)  | `ngOnInit` skips API calls server-side via `isPlatformBrowser()` |
| `/budgets`              | Prerender (SSG)  | Same                                                             |
| `/expenses`             | Prerender (SSG)  | Same                                                             |
| `/categories`           | Prerender (SSG)  | Same                                                             |
| `/departments`          | Prerender (SSG)  | Same                                                             |
| `/reports`              | Prerender (SSG)  | Same                                                             |
| `/users`                | Prerender (SSG)  | Same                                                             |
| `/audits`               | Prerender (SSG)  | Same                                                             |
| `/notifications`        | Prerender (SSG)  | Same                                                             |
| `/profile`              | Prerender (SSG)  | Same                                                             |
| `/budgets/:id/expenses` | Client-side only | Dynamic `:id` — path unknown at build time                       |
| `**`                    | Client-side only | Catch-all                                                        |

> **How refresh-without-logout works:** On page refresh, `authGuard` calls `tryRestoreSession()`. This checks for a valid token in localStorage, then decodes the user profile directly from the JWT payload (no API call). Only if the token is expired does it make one network call to `/api/auth/token/refresh`, then decodes the new JWT. The backend being down has zero effect on staying logged in.

### HTTP Interceptor

`core/interceptors/auth.interceptor.ts` automatically attaches the `Authorization: Bearer <token>` header to every outbound request and handles 401 responses by triggering token refresh. It uses `isPlatformBrowser()` to skip all auth handling during SSG prerendering (server-side has no localStorage).

### Model Layer

Strongly-typed TypeScript interfaces in `src/models/`:

`auth.models.ts` · `user.models.ts` · `budget.models.ts` · `expense.models.ts` · `category.models.ts` · `department.models.ts` · `audit.models.ts` · `notification.models.ts` · `report.models.ts` · `pagination.models.ts`

---

## 9. Backend Architecture

### Layered Structure

```
Controller  →  IService  →  IRepository  →  DbContext  →  SQL Server
                  ↓
            AuditService (cross-cutting)
            NotificationService (cross-cutting)
```

### Dependency Injection (Program.cs)

All services and repositories are registered with `AddScoped<>`, following the request-per-scope lifetime appropriate for EF Core DbContext.

### EF Core — Global Query Filters

Soft-deleted records are transparently filtered at the `DbContext` level:

```csharp
modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
modelBuilder.Entity<Budget>().HasQueryFilter(b => !b.IsDeleted);
modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);
// ... (applied to all soft-delete entities)
```

Use `.IgnoreQueryFilters()` in repository methods that intentionally need to see deleted records (e.g., `GenerateEmployeeIdAsync`).

### Audit Logging

Every create / update / delete call in the service layer writes to `tAuditLog` with:
- `EntityType` (e.g., `"Budget"`, `"User"`)
- `EntityID`
- `Action` (`Create` / `Update` / `Delete`)
- `OldValue` / `NewValue` (JSON snapshots)
- `UserID` of the actor

### Code Auto-Generation

| Entity     | Format         | Example   |
| ---------- | -------------- | --------- |
| Budget     | `BT<YY><seq>`  | `BT25001` |
| Category   | `CAT<seq:D3>`  | `CAT007`  |
| Department | `DEPT<seq:D3>` | `DEPT003` |
| Employee   | `EMP<seq:D3>`  | `EMP012`  |
| Manager    | `MGR<seq:D3>`  | `MGR002`  |

---

## 10. Database Entities

| Table           | Key Columns                                                                                                                                                       | Relationships                                           |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------- |
| `tUser`         | `UserID`, `EmployeeID` (unique), `Email` (unique), `RoleID`, `DepartmentID`, `ManagerID`, `Status`, `RefreshToken`                                                | → `tRole`, `tDepartment`; self-ref `ManagerID → UserID` |
| `tRole`         | `RoleID`, `RoleName`                                                                                                                                              | ← `tUser.RoleID`                                        |
| `tDepartment`   | `DepartmentID`, `DepartmentCode` (unique), `DepartmentName` (unique), `IsActive`, `IsDeleted`                                                                     | ← `tUser`, `tBudget`                                    |
| `tBudget`       | `BudgetID`, `Code` (unique), `Title` (unique), `DepartmentID`, `AmountAllocated`, `AmountSpent`, `AmountRemaining`, `StartDate`, `EndDate`, `Status`, `IsDeleted` | → `tDepartment`; ← `tExpense`                           |
| `tExpense`      | `ExpenseID`, `BudgetID`, `CategoryID`, `Title`, `Amount`, `MerchantName`, `SubmittedByUserID`, `ManagerUserID`, `Status`, `RejectionReason`, `IsDeleted`          | → `tBudget`, `tCategory`, `tUser`                       |
| `tCategory`     | `CategoryID`, `CategoryCode` (unique), `CategoryName` (unique), `IsActive`, `IsDeleted`                                                                           | ← `tExpense`                                            |
| `tNotification` | `NotificationID`, `SenderUserID`, `ReceiverUserID`, `Type`, `Message`, `Status`, `RelatedEntityType`, `RelatedEntityID`, `IsDeleted`                              | → `tUser` (sender & receiver)                           |
| `tReport`       | `ReportID`, `Title`, `Scope`, `Metrics` (JSON), `GeneratedDate`, `GeneratedByUserID`, `IsDeleted`                                                                 | → `tUser`                                               |
| `tAuditLog`     | `AuditLogID`, `UserID`, `EntityType`, `EntityID`, `Action`, `OldValue`, `NewValue`, `Description`, `CreatedDate`                                                  | → `tUser`                                               |

### Enums

| Enum                 | Values                                                                                    |
| -------------------- | ----------------------------------------------------------------------------------------- |
| `UserRole`           | Admin (1), Manager (2), Employee (3)                                                      |
| `UserStatus`         | Active, Inactive, Suspended                                                               |
| `BudgetStatus`       | Active, Closed, Archived                                                                  |
| `ExpenseStatus`      | Pending, Approved, Rejected                                                               |
| `NotificationType`   | ExpenseSubmitted, ExpenseApproved, ExpenseRejected, BudgetCreated, BudgetUpdated, General |
| `NotificationStatus` | Unread, Read                                                                              |
| `AuditAction`        | Create, Update, Delete                                                                    |
| `ReportScopeType`    | Period, Department, Budget                                                                |
| `SortOrder`          | Asc, Desc                                                                                 |

---

## 11. Stored Procedures & Views

### Views

| Name                      | Table      | Description                                                                                                   |
| ------------------------- | ---------- | ------------------------------------------------------------------------------------------------------------- |
| `vwGetUserProfile`        | `tUser`    | Active users enriched with department name, role name, manager full name                                      |
| `vwGetAllBudgetsAdmin`    | `tBudget`  | All budgets including soft-deleted; calculates `UtilizationPct`, `DaysRemaining`, `IsExpired`, `IsOverBudget` |
| `vwGetAllBudgets`         | `tBudget`  | Non-deleted budgets only; same enriched calculated fields                                                     |
| `vwGetAllExpenses`        | `tExpense` | Non-deleted expenses joined with budget, category, submitter name, approver name, department                  |
| `vwGetExpensesByBudgetID` | `tExpense` | Same as above filtered to a specific `BudgetID`                                                               |

### Stored Procedures

| Procedure                             | Parameters                                                                                                                   | Description                                                                                              |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `uspGetUserProfile`                   | `@UserId`                                                                                                                    | Returns single user profile from `vwGetUserProfile`                                                      |
| `uspGetUsersList`                     | `@PageNumber`, `@PageSize`, `@Role`, `@Status`, `@IsDeleted`, `@SearchTerm`, `@SortBy`, `@SortOrder`                         | Paginated, filterable user list with total count                                                         |
| `uspCreateBudget`                     | `@Title`, `@DepartmentID`, `@AmountAllocated`, `@StartDate`, `@EndDate`, `@CreatedByUserID`, `@Notes`, `@NewBudgetID OUTPUT` | Creates budget with auto-code `BT<YY><seq>`, writes audit log, notifies Admin                            |
| `uspCreateExpense`                    | `@BudgetID`, `@CategoryID`, `@Title`, `@Amount`, `@MerchantName`, `@SubmittedByUserID`, `@Notes`, `@NewExpenseID OUTPUT`     | Creates expense, notifies manager, updates `AmountSpent` / `AmountRemaining` on budget, writes audit log |
| `uspGetAllCategories`                 | —                                                                                                                            | All active categories ordered alphabetically                                                             |
| `uspCreateCategory`                   | `@CategoryName`, `@CreatedByUserID`, `@NewCategoryID OUTPUT`                                                                 | Auto-codes `CAT<seq:D3>`, uniqueness check, writes audit log                                             |
| `uspUpdateCategory`                   | `@CategoryID`, `@CategoryName`, `@UpdatedByUserID`                                                                           | Updates category with before/after audit log                                                             |
| `uspGetAllDepartments`                | —                                                                                                                            | All active departments ordered alphabetically                                                            |
| `uspCreateDepartment`                 | `@DepartmentName`, `@CreatedByUserID`, `@NewDepartmentID OUTPUT`                                                             | Auto-codes `DEPT<seq:D3>`, uniqueness check, writes audit log                                            |
| `uspUpdateDepartment`                 | `@DepartmentID`, `@DepartmentName`, `@UpdatedByUserID`                                                                       | Updates department with audit log                                                                        |
| `uspGetNotificationsByReceiverUserId` | `@ReceiverUserID`, `@PageNumber`, `@PageSize`, `@Status`, `@SortOrder`                                                       | Paginated notifications for a user with read/unread filter                                               |
| `uspMarkNotificationAsRead`           | `@NotificationID`, `@ReceiverUserID`                                                                                         | Marks single notification read; validates ownership                                                      |
| `uspMarkAllNotificationsAsRead`       | `@ReceiverUserID`                                                                                                            | Marks all unread notifications as read for a user                                                        |
| `uspGetPeriodReport`                  | `@StartDate`, `@EndDate`                                                                                                     | Budget summary filtered by date range (approved expenses only) with totals                               |
| `uspGetDepartmentReport`              | `@DepartmentName`                                                                                                            | Budget & expense stats grouped by department with utilization percentages                                |
| `uspGetBudgetReport`                  | `@BudgetCode`                                                                                                                | Detailed single-budget report including all associated expenses, department, and manager information     |

---

## 12. NuGet Packages

| Package                                         | Version | Purpose                                            |
| ----------------------------------------------- | ------- | -------------------------------------------------- |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.2  | JWT Bearer token authentication middleware         |
| `Microsoft.EntityFrameworkCore.SqlServer`       | 10.0.2  | EF Core database provider for SQL Server           |
| `Microsoft.EntityFrameworkCore.Design`          | 10.0.2  | EF Core design-time tools (migrations scaffolding) |
| `Microsoft.EntityFrameworkCore.Tools`           | 10.0.2  | `dotnet ef` CLI commands                           |
| `Swashbuckle.AspNetCore`                        | 6.5.0   | Swagger / OpenAPI UI at `/swagger`                 |

> `Microsoft.AspNetCore.Identity` `PasswordHasher<T>` is included via the ASP.NET Core shared framework and does not require a separate NuGet reference.
