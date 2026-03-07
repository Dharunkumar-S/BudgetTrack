# BudgetTrack — Backend Class Diagram

> **Stack:** ASP.NET Core 10 · Entity Framework Core 10 · SQL Server
> **Generated:** 2026-03-07

---

## Table of Contents

1. [Domain Entities](#1-domain-entities)
2. [Enumerations](#2-enumerations)
3. [Repository Layer](#3-repository-layer)
4. [Service Layer](#4-service-layer)
5. [Controller Layer](#5-controller-layer)
6. [Infrastructure](#6-infrastructure)
7. [Full Architecture Overview](#7-full-architecture-overview)
8. [Role-Based Access Summary](#8-role-based-access-summary)

---

## 1. Domain Entities

```mermaid
classDiagram
    direction TB

    class Role {
        +int RoleID
        +string RoleName
        +bool IsActive
        +DateTime CreatedDate
        +int CreatedByUserID
        +DateTime UpdatedDate
        +int UpdatedByUserID
        +bool IsDeleted
        +DateTime DeletedDate
        +int DeletedByUserID
        +ICollection~User~ Users
    }

    class Department {
        +int DepartmentID
        +string DepartmentName
        +string DepartmentCode
        +bool IsActive
        +DateTime CreatedDate
        +int CreatedByUserID
        +DateTime UpdatedDate
        +int UpdatedByUserID
        +bool IsDeleted
        +DateTime DeletedDate
        +int DeletedByUserID
        +ICollection~User~ Users
        +ICollection~Budget~ Budgets
    }

    class User {
        +int UserID
        +string FirstName
        +string LastName
        +string EmployeeID
        +string Email
        +string PasswordHash
        +int DepartmentID
        +int RoleID
        +UserStatus Status
        +int ManagerID
        +string RefreshToken
        +DateTime RefreshTokenExpiryTime
        +DateTime LastLoginDate
        +DateTime CreatedDate
        +int CreatedByUserID
        +DateTime UpdatedDate
        +int UpdatedByUserID
        +bool IsDeleted
        +DateTime DeletedDate
        +int DeletedByUserID
        +Department Department
        +Role Role
        +User Manager
        +ICollection~User~ Subordinates
        +ICollection~Budget~ BudgetsCreated
        +ICollection~Expense~ ExpensesSubmitted
        +ICollection~Expense~ ExpensesApproved
        +ICollection~Notification~ NotificationsSent
        +ICollection~Notification~ NotificationsReceived
        +ICollection~AuditLog~ AuditLogs
    }

    class Budget {
        +int BudgetID
        +string Title
        +string Code
        +int DepartmentID
        +decimal AmountAllocated
        +decimal AmountSpent
        +decimal AmountRemaining
        +DateTime StartDate
        +DateTime EndDate
        +BudgetStatus Status
        +int CreatedByUserID
        +string Notes
        +DateTime CreatedDate
        +DateTime UpdatedDate
        +int UpdatedByUserID
        +bool IsDeleted
        +DateTime DeletedDate
        +int DeletedByUserID
        +Department Department
        +User CreatedByUser
        +User UpdatedByUser
        +User DeletedByUser
        +ICollection~Expense~ Expenses
    }

    class Category {
        +int CategoryID
        +string CategoryName
        +string CategoryCode
        +bool IsActive
        +DateTime CreatedDate
        +int CreatedByUserID
        +DateTime UpdatedDate
        +int UpdatedByUserID
        +bool IsDeleted
        +DateTime DeletedDate
        +int DeletedByUserID
        +User CreatedByUser
        +User UpdatedByUser
        +User DeletedByUser
        +ICollection~Expense~ Expenses
    }

    class Expense {
        +int ExpenseID
        +int BudgetID
        +int CategoryID
        +string Title
        +decimal Amount
        +string MerchantName
        +int SubmittedByUserID
        +DateTime SubmittedDate
        +ExpenseStatus Status
        +int ManagerUserID
        +DateTime StatusApprovedDate
        +string RejectionReason
        +string Notes
        +string ApprovalComments
        +DateTime CreatedDate
        +DateTime UpdatedDate
        +int UpdatedByUserID
        +bool IsDeleted
        +DateTime DeletedDate
        +int DeletedByUserID
        +Budget Budget
        +Category Category
        +User SubmittedByUser
        +User ApprovedByUser
        +User UpdatedByUser
        +User DeletedByUser
    }

    class Notification {
        +int NotificationID
        +int SenderUserID
        +int ReceiverUserID
        +NotificationType Type
        +string Message
        +NotificationStatus Status
        +DateTime CreatedDate
        +DateTime ReadDate
        +string RelatedEntityType
        +int RelatedEntityID
        +bool IsDeleted
        +DateTime DeletedDate
        +User Sender
        +User Receiver
    }

    class AuditLog {
        +int AuditLogID
        +int UserID
        +string EntityType
        +int EntityID
        +AuditAction Action
        +string OldValue
        +string NewValue
        +string Description
        +DateTime CreatedDate
        +User User
    }

    Role "1" --> "0..*" User : has
    Department "1" --> "0..*" User : employs
    Department "1" --> "0..*" Budget : funds
    User "1" --> "0..*" User : manages
    User "1" --> "0..*" Budget : creates
    User "1" --> "0..*" Expense : submits
    User "1" --> "0..*" Expense : approves
    User "1" --> "0..*" Notification : sends
    User "1" --> "0..*" Notification : receives
    User "1" --> "0..*" AuditLog : logs
    Budget "1" --> "0..*" Expense : contains
    Category "1" --> "0..*" Expense : classifies
```

---

## 2. Enumerations

```mermaid
classDiagram
    direction LR

    class UserStatus {
        Active = 0
        Inactive = 1
    }

    class BudgetStatus {
        Active = 1
        Closed = 2
    }

    class ExpenseStatus {
        Pending = 1
        Approved = 2
        Rejected = 3
    }

    class NotificationStatus {
        Unread = 1
        Read = 2
    }

    class NotificationType {
        ExpenseSubmitted = 1
        ExpenseApproved = 2
        ExpenseRejected = 3
        BudgetCreated = 4
        BudgetUpdated = 5
        BudgetDeleted = 6
    }

    class AuditAction {
        Create = 1
        Update = 2
        Delete = 3
    }

    class UserRole {
        Admin = 1
        Manager = 2
        Employee = 3
    }

    class SortOrder {
        Asc
        Desc
    }
```

---

## 3. Repository Layer

```mermaid
classDiagram
    direction TB

    class IUserRepository {
        +GetByIdAsync(id) Task~User~
        +GetByEmailAsync(email) Task~User~
        +GetUserForLoginAsync(email) Task~User~
        +ValidateRefreshTokenAsync(userId, token) Task~User~
        +GetByEmployeeIdAsync(employeeId) Task~User~
        +GetAllManagersAsync() Task~IEnumerable~User~~
        +GetEmployeesByManagerIdAsync(managerId) Task~IEnumerable~User~~
        +EmailExistsAsync(email) Task~bool~
        +GenerateEmployeeIdAsync(roleId) Task~string~
        +AddAsync(user) Task
        +UpdateAsync(user) Task
        +DeleteAsync(id, deletedByUserId) Task
        +SaveChangesAsync() Task
        +GetUserProfileByIdAsync(userId) Task~UserProfileResponseDto~
        +GetUsersListAsync(filters) Task~List~UserListResponseDto~~
    }

    class IBudgetRepository {
        +CreateBudgetAsync(dto, userId) Task~int~
        +UpdateBudgetAsync(budgetID, dto, userId) Task
        +DeleteBudgetAsync(budgetID, userId) Task
        +GetBudgetByIdAsync(budgetID) Task~BudgetDto~
        +GetAllBudgetsAsync(filters) Task~PagedResult~BudgetDto~~
        +GetBudgetsByCreatedByUserIdWithPaginationAsync(userId, filters) Task~PagedResult~BudgetDto~~
    }

    class IExpenseRepository {
        +GetAllExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +GetExpensesByBudgetIDAsync(budgetID, filters) Task~PagedResult~AllExpenseDto~~
        +GetManagedExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +CreateExpenseAsync(dto, userId) Task~int~
        +UpdateExpenseStatusAsync(id, status, userId, comments, reason) Task~bool~
        +GetExpenseStatisticsAsync(filters) Task~ExpenseStatisticsDto~
    }

    class ICategoryRepository {
        +GetAllCategoriesAsync() Task~List~CategoryResponseDto~~
        +CreateCategoryAsync(dto, userId) Task~int~
        +UpdateCategoryAsync(id, dto, userId) Task~bool~
        +DeleteCategoryAsync(id, userId) Task~bool~
    }

    class IDepartmentRepository {
        +GetAllDepartmentsAsync() Task~List~DepartmentResponseDto~~
        +CreateDepartmentAsync(dto, userId) Task~int~
        +UpdateDepartmentAsync(id, dto, userId) Task~bool~
        +DeleteDepartmentAsync(id, userId) Task~bool~
    }

    class INotificationRepository {
        +GetNotificationsByReceiverUserIdAsync(userId, msg, status, sort, page, size) Task~PagedResult~GetNotificationDto~~
        +MarkNotificationAsReadAsync(id, userId) Task~bool~
        +MarkAllNotificationsAsReadAsync(userId) Task~int~
        +DeleteNotificationAsync(id, userId) Task~bool~
        +DeleteAllNotificationsAsync(userId) Task~int~
    }

    class IAuditRepository {
        +GetAllAuditLogsAsync() Task~List~AuditLogDto~~
        +GetAllAuditLogsPaginatedAsync(page, size, search, action, entityType) Task~PagedResult~AuditLogDto~~
        +GetAuditLogsByUserIdAsync(userId) Task~List~AuditLogDto~~
    }

    class IReportRepository {
        +GetPeriodReportAsync(startDate, endDate) Task~PeriodReportDto~
        +GetDepartmentReportAsync(departmentName) Task~DepartmentReportDto~
        +GetBudgetReportAsync(budgetCode) Task~BudgetReportDto~
    }

    class UserRepository {
        -BudgetTrackDbContext _context
        +GetByIdAsync(id) Task~User~
        +GetByEmailAsync(email) Task~User~
        +AddAsync(user) Task
        +UpdateAsync(user) Task
        +DeleteAsync(id, deletedByUserId) Task
    }

    class BudgetRepository {
        -BudgetTrackDbContext _context
        +CreateBudgetAsync(dto, userId) Task~int~
        +UpdateBudgetAsync(budgetID, dto, userId) Task
        +DeleteBudgetAsync(budgetID, userId) Task
        +GetAllBudgetsAsync(filters) Task~PagedResult~BudgetDto~~
    }

    class ExpenseRepository {
        -BudgetTrackDbContext _context
        +GetAllExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +CreateExpenseAsync(dto, userId) Task~int~
        +UpdateExpenseStatusAsync(id, status, userId, comments, reason) Task~bool~
        +GetExpenseStatisticsAsync(filters) Task~ExpenseStatisticsDto~
    }

    class CategoryRepository {
        -BudgetTrackDbContext _context
        +GetAllCategoriesAsync() Task~List~CategoryResponseDto~~
        +CreateCategoryAsync(dto, userId) Task~int~
        +UpdateCategoryAsync(id, dto, userId) Task~bool~
        +DeleteCategoryAsync(id, userId) Task~bool~
    }

    class DepartmentRepository {
        -BudgetTrackDbContext _context
        +GetAllDepartmentsAsync() Task~List~DepartmentResponseDto~~
        +CreateDepartmentAsync(dto, userId) Task~int~
        +UpdateDepartmentAsync(id, dto, userId) Task~bool~
        +DeleteDepartmentAsync(id, userId) Task~bool~
    }

    class NotificationRepository {
        -BudgetTrackDbContext _context
        +GetNotificationsByReceiverUserIdAsync() Task~PagedResult~GetNotificationDto~~
        +MarkNotificationAsReadAsync(id, userId) Task~bool~
        +MarkAllNotificationsAsReadAsync(userId) Task~int~
        +DeleteNotificationAsync(id, userId) Task~bool~
        +DeleteAllNotificationsAsync(userId) Task~int~
    }

    class AuditRepository {
        -BudgetTrackDbContext _context
        +GetAllAuditLogsAsync() Task~List~AuditLogDto~~
        +GetAllAuditLogsPaginatedAsync() Task~PagedResult~AuditLogDto~~
        +GetAuditLogsByUserIdAsync(userId) Task~List~AuditLogDto~~
    }

    class ReportRepository {
        -BudgetTrackDbContext _context
        +GetPeriodReportAsync(startDate, endDate) Task~PeriodReportDto~
        +GetDepartmentReportAsync(departmentName) Task~DepartmentReportDto~
        +GetBudgetReportAsync(budgetCode) Task~BudgetReportDto~
    }

    IUserRepository <|.. UserRepository
    IBudgetRepository <|.. BudgetRepository
    IExpenseRepository <|.. ExpenseRepository
    ICategoryRepository <|.. CategoryRepository
    IDepartmentRepository <|.. DepartmentRepository
    INotificationRepository <|.. NotificationRepository
    IAuditRepository <|.. AuditRepository
    IReportRepository <|.. ReportRepository
```

---

## 4. Service Layer

```mermaid
classDiagram
    direction TB

    class IBudgetService {
        +CreateBudgetAsync(dto, userId) Task~int~
        +UpdateBudgetAsync(budgetID, dto, userId) Task
        +DeleteBudgetAsync(budgetID, userId) Task
        +GetAllBudgetsAsync(filters) Task~PagedResult~BudgetDto~~
        +GetBudgetsByCreatedByUserIdWithPaginationAsync(userId, filters) Task~PagedResult~BudgetDto~~
    }

    class IExpenseService {
        +GetAllExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +GetExpensesByBudgetIDAsync(budgetID, filters) Task~PagedResult~AllExpenseDto~~
        +GetManagedExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +CreateExpenseAsync(dto, userId) Task~int~
        +UpdateExpenseStatusAsync(id, status, userId, comments, reason) Task~bool~
        +GetExpenseStatisticsAsync(filters) Task~ExpenseStatisticsDto~
    }

    class ICategoryService {
        +GetAllCategoriesAsync() Task~List~CategoryResponseDto~~
        +CreateCategoryAsync(dto, userId) Task~int~
        +UpdateCategoryAsync(id, dto, userId) Task~bool~
        +DeleteCategoryAsync(id, userId) Task~bool~
    }

    class IDepartmentService {
        +GetAllDepartmentsAsync() Task~List~DepartmentResponseDto~~
        +CreateDepartmentAsync(dto, userId) Task~int~
        +UpdateDepartmentAsync(id, dto, userId) Task~bool~
        +DeleteDepartmentAsync(id, userId) Task~bool~
    }

    class INotificationService {
        +GetNotificationsByReceiverUserIdAsync(userId, msg, status, sort, page, size) Task~PagedResult~GetNotificationDto~~
        +MarkNotificationAsReadAsync(id, userId) Task~bool~
        +MarkAllNotificationsAsReadAsync(userId) Task~int~
        +DeleteNotificationAsync(id, userId) Task~bool~
        +DeleteAllNotificationsAsync(userId) Task~int~
    }

    class IAuditService {
        +GetAllAuditLogsAsync() Task~List~AuditLogDto~~
        +GetAllAuditLogsPaginatedAsync(page, size, search, action, entityType) Task~PagedResult~AuditLogDto~~
        +GetAuditLogsByUserIdAsync(userId) Task~List~AuditLogDto~~
    }

    class IReportService {
        +GetPeriodReportAsync(startDate, endDate) Task~PeriodReportDto~
        +GetDepartmentReportAsync(departmentName) Task~DepartmentReportDto~
        +GetBudgetReportAsync(budgetCode) Task~BudgetReportDto~
    }

    class BudgetService {
        -IBudgetRepository _repo
        +CreateBudgetAsync(dto, userId) Task~int~
        +UpdateBudgetAsync(budgetID, dto, userId) Task
        +DeleteBudgetAsync(budgetID, userId) Task
        +GetAllBudgetsAsync(filters) Task~PagedResult~BudgetDto~~
        +GetBudgetsByCreatedByUserIdWithPaginationAsync(userId, filters) Task~PagedResult~BudgetDto~~
    }

    class ExpenseService {
        -IExpenseRepository _repo
        +GetAllExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +GetManagedExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +CreateExpenseAsync(dto, userId) Task~int~
        +UpdateExpenseStatusAsync(id, status, userId, comments, reason) Task~bool~
        +GetExpenseStatisticsAsync(filters) Task~ExpenseStatisticsDto~
    }

    class CategoryService {
        -ICategoryRepository _repo
        +GetAllCategoriesAsync() Task~List~CategoryResponseDto~~
        +CreateCategoryAsync(dto, userId) Task~int~
        +UpdateCategoryAsync(id, dto, userId) Task~bool~
        +DeleteCategoryAsync(id, userId) Task~bool~
    }

    class DepartmentService {
        -IDepartmentRepository _repo
        +GetAllDepartmentsAsync() Task~List~DepartmentResponseDto~~
        +CreateDepartmentAsync(dto, userId) Task~int~
        +UpdateDepartmentAsync(id, dto, userId) Task~bool~
        +DeleteDepartmentAsync(id, userId) Task~bool~
    }

    class NotificationService {
        -INotificationRepository _repo
        +GetNotificationsByReceiverUserIdAsync() Task~PagedResult~GetNotificationDto~~
        +MarkNotificationAsReadAsync(id, userId) Task~bool~
        +MarkAllNotificationsAsReadAsync(userId) Task~int~
        +DeleteNotificationAsync(id, userId) Task~bool~
        +DeleteAllNotificationsAsync(userId) Task~int~
    }

    class AuditService {
        -IAuditRepository _repo
        +GetAllAuditLogsAsync() Task~List~AuditLogDto~~
        +GetAllAuditLogsPaginatedAsync() Task~PagedResult~AuditLogDto~~
        +GetAuditLogsByUserIdAsync(userId) Task~List~AuditLogDto~~
    }

    class ReportService {
        -IReportRepository _repo
        +GetPeriodReportAsync(startDate, endDate) Task~PeriodReportDto~
        +GetDepartmentReportAsync(departmentName) Task~DepartmentReportDto~
        +GetBudgetReportAsync(budgetCode) Task~BudgetReportDto~
    }

    class JwtTokenService {
        -JwtSettings _jwtSettings
        +GenerateAccessToken(user) string
        +GenerateRefreshToken() string
        +GetPrincipalFromExpiredToken(token) ClaimsPrincipal
    }

    IBudgetService <|.. BudgetService
    IExpenseService <|.. ExpenseService
    ICategoryService <|.. CategoryService
    IDepartmentService <|.. DepartmentService
    INotificationService <|.. NotificationService
    IAuditService <|.. AuditService
    IReportService <|.. ReportService

    BudgetService --> IBudgetRepository
    ExpenseService --> IExpenseRepository
    CategoryService --> ICategoryRepository
    DepartmentService --> IDepartmentRepository
    NotificationService --> INotificationRepository
    AuditService --> IAuditRepository
    ReportService --> IReportRepository
```

---

## 5. Controller Layer

```mermaid
classDiagram
    direction TB

    class ControllerBase {
        ASP.NET Core Base
    }

    class BaseApiController {
        int UserId
        +GetUserId() int
    }

    class AuthController {
        -IAuthService _authService
        -IUserRepository _userRepository
        +AdminRegister(dto) Task
        +Login(dto) Task
        +ChangePassword(dto) Task
        +RefreshToken(dto) Task
        +Logout() Task
        +Verify() IActionResult
        +GetUserProfile() Task
        +GetUsers(filters) Task
        +UpdateUser(userId, dto) Task
    }

    class BudgetController {
        -IBudgetService _budgetService
        +GetAllBudgets(filters) Task
        +GetBudgetsByUserWithPagination(filters) Task
        +CreateBudget(dto) Task
        +UpdateBudget(budgetID, dto) Task
        +DeleteBudget(budgetID) Task
        +GetExpensesByBudgetID(budgetID, filters) Task
    }

    class ExpenseController {
        -IExpenseService _service
        +GetExpenseStatistics(filters) Task
        +GetAllExpenses(filters) Task
        +GetManagedExpenses(filters) Task
        +CreateExpense(dto) Task
        +UpdateExpenseStatus(expenseID, dto) Task
    }

    class CategoryController {
        -ICategoryService _service
        +GetAllCategories() Task
        +CreateCategory(dto) Task
        +UpdateCategory(categoryID, dto) Task
        +DeleteCategory(categoryID) Task
    }

    class DepartmentController {
        -IDepartmentService _service
        +GetAllDepartments() Task
        +CreateDepartment(dto) Task
        +UpdateDepartment(departmentID, dto) Task
        +DeleteDepartment(departmentID) Task
    }

    class UserController {
        -IUserRepository _repo
        +GetUserStats() Task
        +GetManagers() Task
        +GetEmployeesByManagerId(managerId) Task
        +SoftDeleteUser(userId) Task
    }

    class NotificationController {
        -INotificationService _service
        +GetNotifications(filters) Task
        +GetUnreadCount() Task
        +MarkAsRead(notificationID) Task
        +MarkAllAsRead() Task
        +DeleteNotification(notificationID) Task
        +DeleteAllNotifications() Task
    }

    class AuditController {
        -IAuditService _service
        +GetAllAuditLogs(filters) Task
        +GetAuditLogsByUserId(userId) Task
    }

    class ReportController {
        -IReportService _service
        +GetPeriodReport(startDate, endDate) Task
        +GetDepartmentReport(departmentName) Task
        +GetBudgetReport(budgetCode) Task
    }

    ControllerBase <|-- BaseApiController
    BaseApiController <|-- BudgetController
    BaseApiController <|-- ExpenseController
    BaseApiController <|-- CategoryController
    BaseApiController <|-- DepartmentController
    BaseApiController <|-- UserController
    BaseApiController <|-- NotificationController
    BaseApiController <|-- ReportController
    BaseApiController <|-- AuthController
    ControllerBase <|-- AuditController

    BudgetController --> IBudgetService
    ExpenseController --> IExpenseService
    CategoryController --> ICategoryService
    DepartmentController --> IDepartmentService
    NotificationController --> INotificationService
    AuditController --> IAuditService
    ReportController --> IReportService
    AuthController --> IAuthService
    UserController --> IUserRepository
```

---

## 6. Infrastructure

```mermaid
classDiagram
    direction TB

    class BudgetTrackDbContext {
        +DbSet~User~ Users
        +DbSet~Budget~ Budgets
        +DbSet~Expense~ Expenses
        +DbSet~Category~ Categories
        +DbSet~Notification~ Notifications
        +DbSet~AuditLog~ AuditLogs
        +DbSet~Department~ Departments
        +DbSet~Role~ Roles
        +OnModelCreating(modelBuilder) void
    }

    class JwtMiddleware {
        -RequestDelegate _next
        -ILogger _logger
        +InvokeAsync(context, jwtSettings, userRepository) Task
        -ExtractTokenFromHeader(context) string
        -AttachUserToContext(context, token, settings, repo) Task
    }

    class JwtSettings {
        +string SecretKey
        +string Issuer
        +string Audience
        +int AccessTokenExpiryMinutes
        +int RefreshTokenExpiryDays
    }

    class IAuthService {
        +AdminRegisterUserAsync(dto, adminId) Task~User~
        +LoginAsync(dto) Task~AuthResponseDto~
        +ChangePasswordAsync(userId, oldPwd, newPwd) Task
        +RefreshTokenAsync(accessToken, refreshToken) Task~AuthResponseDto~
        +RevokeTokenAsync(userId) Task
        +UpdateUserAsync(userId, dto, adminId) Task
    }

    class AuthService {
        -IUserRepository _userRepo
        -JwtTokenService _jwtService
        +AdminRegisterUserAsync(dto, adminId) Task~User~
        +LoginAsync(dto) Task~AuthResponseDto~
        +ChangePasswordAsync(userId, oldPwd, newPwd) Task
        +RefreshTokenAsync(accessToken, refreshToken) Task~AuthResponseDto~
        +RevokeTokenAsync(userId) Task
        +UpdateUserAsync(userId, dto, adminId) Task
    }

    class PagedResult~T~ {
        +List~T~ Data
        +int PageNumber
        +int PageSize
        +int TotalRecords
        +int TotalPages
        +bool HasNextPage
        +bool HasPreviousPage
    }

    IAuthService <|.. AuthService
    AuthService --> IUserRepository
    AuthService --> JwtTokenService
    JwtMiddleware --> IUserRepository
    JwtMiddleware --> JwtSettings
    BudgetTrackDbContext --> User
    BudgetTrackDbContext --> Budget
    BudgetTrackDbContext --> Expense
    BudgetTrackDbContext --> Category
    BudgetTrackDbContext --> Department
    BudgetTrackDbContext --> Role
    BudgetTrackDbContext --> Notification
    BudgetTrackDbContext --> AuditLog
```

---

## 7. Full Architecture Overview

```mermaid
flowchart LR
    subgraph Frontend["Angular 21 SPA (Port 4200)"]
        SPA["Components + Services"]
        INT["authInterceptor"]
        GRD["authGuard + roleGuard"]
    end

    subgraph Backend["ASP.NET Core 10 API (Port 5131)"]
        MW["JwtMiddleware"]
        CTL["Controllers"]
        SVC["Services"]
        REPO["Repositories"]
    end

    subgraph Database["SQL Server"]
        TABLES["tUser, tBudget, tExpense,\ntCategory, tDepartment, tRole,\ntNotification, tAuditLog"]
        VIEWS["vwGetAllExpenses\nvwGetAllBudgets\nvwGetUserProfile"]
        SPS["Stored Procedures: usp*"]
    end

    SPA --> INT
    GRD --> SPA
    INT -->|"HTTP + Bearer JWT"| MW
    MW --> CTL --> SVC --> REPO
    REPO -->|"EF Core + Raw SQL"| TABLES
    REPO --> VIEWS
    REPO --> SPS
```

---

## 8. Role-Based Access Summary

| Controller               | Endpoint                           | Roles             |
| ------------------------ | ---------------------------------- | ----------------- |
| `AuthController`         | POST `/api/auth/createuser`        | Admin             |
| `AuthController`         | POST `/api/auth/login`             | Public            |
| `AuthController`         | POST `/api/auth/token/refresh`     | Public            |
| `AuthController`         | GET `/api/users`                   | Admin, Manager    |
| `AuthController`         | PUT `/api/users/{id}`              | Admin             |
| `BudgetController`       | GET `/api/budgets/admin`           | Admin             |
| `BudgetController`       | GET `/api/budgets`                 | All authenticated |
| `BudgetController`       | POST `/api/budgets`                | Admin, Manager    |
| `BudgetController`       | PUT `/api/budgets/{id}`            | Admin, Manager    |
| `BudgetController`       | DELETE `/api/budgets/{id}`         | Admin, Manager    |
| `BudgetController`       | GET `/api/budgets/{id}/expenses`   | All authenticated |
| `ExpenseController`      | GET `/api/expenses/stats`          | All authenticated |
| `ExpenseController`      | GET `/api/expenses`                | Admin             |
| `ExpenseController`      | GET `/api/expenses/managed`        | Manager, Employee |
| `ExpenseController`      | POST `/api/expenses`               | Manager, Employee |
| `ExpenseController`      | PUT `/api/expenses/status/{id}`    | Manager           |
| `CategoryController`     | GET `/api/categories`              | All authenticated |
| `CategoryController`     | POST `/api/categories`             | Admin, Manager    |
| `CategoryController`     | PUT `/api/categories/{id}`         | Admin, Manager    |
| `CategoryController`     | DELETE `/api/categories/{id}`      | Admin, Manager    |
| `DepartmentController`   | GET `/api/departments`             | All authenticated |
| `DepartmentController`   | POST/PUT/DELETE `/api/departments` | Admin             |
| `NotificationController` | GET `/api/notifications/unread-count` | Manager, Employee |
| `NotificationController` | All other endpoints                | Manager, Employee |
| `UserController`         | GET `/api/users/stats`             | Admin             |
| `UserController`         | GET `/api/users/managers`          | All authenticated |
| `UserController`         | GET `/api/users/{id}/employees`    | All authenticated |
| `UserController`         | DELETE `/api/users/{id}`           | Admin             |
| `AuditController`        | All endpoints                      | Admin             |
| `ReportController`       | GET `/api/reports/period`          | Admin             |
| `ReportController`       | GET `/api/reports/department`      | Admin             |
| `ReportController`       | GET `/api/reports/budget`          | Admin, Manager    |

---

*BudgetTrack Class Diagram — Generated 2026-03-07*
