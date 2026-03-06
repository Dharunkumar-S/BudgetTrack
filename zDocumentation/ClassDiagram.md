# BudgetTrack — Backend Class Diagram

> **Stack:** ASP.NET Core 10 · Entity Framework Core 10 · SQL Server  
> **Generated:** 2026-03-06

---

## Table of Contents

1. [Domain Entities](#1-domain-entities)
2. [Enumerations](#2-enumerations)
3. [Repository Layer](#3-repository-layer)
4. [Service Layer](#4-service-layer)
5. [Controller Layer](#5-controller-layer)
6. [Infrastructure](#6-infrastructure)
7. [Full Architecture Overview](#7-full-architecture-overview)

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
        +int? CreatedByUserID
        +DateTime? UpdatedDate
        +int? UpdatedByUserID
        +bool IsDeleted
        +DateTime? DeletedDate
        +int? DeletedByUserID
        --
        +ICollection~User~ Users
    }

    class Department {
        +int DepartmentID
        +string DepartmentName
        +string DepartmentCode
        +bool IsActive
        +DateTime CreatedDate
        +int? CreatedByUserID
        +DateTime? UpdatedDate
        +int? UpdatedByUserID
        +bool IsDeleted
        +DateTime? DeletedDate
        +int? DeletedByUserID
        --
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
        +int? ManagerID
        +string? RefreshToken
        +DateTime? RefreshTokenExpiryTime
        +DateTime? LastLoginDate
        +DateTime CreatedDate
        +int? CreatedByUserID
        +DateTime? UpdatedDate
        +int? UpdatedByUserID
        +bool IsDeleted
        +DateTime? DeletedDate
        +int? DeletedByUserID
        --
        +Department Department
        +Role Role
        +User? Manager
        +ICollection~User~ Subordinates
        +ICollection~Budget~ BudgetsCreated
        +ICollection~Expense~ ExpensesSubmitted
        +ICollection~Expense~ ExpensesApproved
        +ICollection~Report~ ReportsGenerated
        +ICollection~Notification~ Notifications
        +ICollection~AuditLog~ AuditLogs
    }

    class Budget {
        +int BudgetID
        +string Title
        +string? Code
        +int DepartmentID
        +decimal AmountAllocated
        +decimal AmountSpent
        +decimal AmountRemaining
        +DateTime StartDate
        +DateTime EndDate
        +BudgetStatus Status
        +int CreatedByUserID
        +string? Notes
        +DateTime CreatedDate
        +DateTime? UpdatedDate
        +int? UpdatedByUserID
        +bool IsDeleted
        +DateTime? DeletedDate
        +int? DeletedByUserID
        --
        +Department Department
        +User? CreatedByUser
        +User? UpdatedByUser
        +User? DeletedByUser
        +ICollection~Expense~ Expenses
    }

    class Category {
        +int CategoryID
        +string CategoryName
        +string CategoryCode
        +bool IsActive
        +DateTime CreatedDate
        +int? CreatedByUserID
        +DateTime? UpdatedDate
        +int? UpdatedByUserID
        +bool IsDeleted
        +DateTime? DeletedDate
        +int? DeletedByUserID
        --
        +User? CreatedByUser
        +User? UpdatedByUser
        +User? DeletedByUser
        +ICollection~Expense~ Expenses
    }

    class Expense {
        +int ExpenseID
        +int BudgetID
        +int CategoryID
        +string Title
        +decimal Amount
        +string? MerchantName
        +int SubmittedByUserID
        +DateTime SubmittedDate
        +ExpenseStatus Status
        +int? ManagerUserID
        +DateTime? StatusApprovedDate
        +string? RejectionReason
        +string? Notes
        +string? ApprovalComments
        +DateTime CreatedDate
        +DateTime? UpdatedDate
        +int? UpdatedByUserID
        +bool IsDeleted
        +DateTime? DeletedDate
        +int? DeletedByUserID
        --
        +Budget Budget
        +Category Category
        +User SubmittedByUser
        +User? ApprovedByUser
        +User? UpdatedByUser
        +User? DeletedByUser
    }

    class Notification {
        +int NotificationID
        +int SenderUserID
        +int ReceiverUserID
        +NotificationType Type
        +string Message
        +NotificationStatus Status
        +DateTime CreatedDate
        +DateTime? ReadDate
        +string? RelatedEntityType
        +int? RelatedEntityID
        +bool IsDeleted
        +DateTime? DeletedDate
        --
        +User? Sender
        +User? Receiver
    }

    class AuditLog {
        +int AuditLogID
        +int? UserID
        +string EntityType
        +int EntityID
        +AuditAction Action
        +string? OldValue
        +string? NewValue
        +string? Description
        +DateTime CreatedDate
        --
        +User? User
    }

    class Report {
        +int ReportID
        +string Title
        +ReportScopeType? Scope
        +string? Metrics
        +DateTime GeneratedDate
        +int GeneratedByUserID
        +bool IsDeleted
        +DateTime? DeletedDate
        +int? DeletedByUserID
        --
        +User? GeneratedByUser
        +User? DeletedByUser
    }

    %% Entity Relationships
    Role "1" --> "0..*" User : has
    Department "1" --> "0..*" User : employs
    Department "1" --> "0..*" Budget : funds
    User "1" --> "0..*" User : manages (self-ref)
    User "1" --> "0..*" Budget : creates
    User "1" --> "0..*" Expense : submits
    User "1" --> "0..*" Expense : approves
    User "1" --> "0..*" Report : generates
    User "1" --> "0..*" Notification : receives
    User "1" --> "0..*" AuditLog : logs
    Budget "1" --> "0..*" Expense : contains
    Category "1" --> "0..*" Expense : classifies
    Notification --> User : sender
    Notification --> User : receiver
```

---

## 2. Enumerations

```mermaid
classDiagram
    direction LR

    class UserStatus {
        <<enumeration>>
        Active = 0
        Inactive = 1
    }

    class BudgetStatus {
        <<enumeration>>
        Active = 1
        Closed = 2
    }

    class ExpenseStatus {
        <<enumeration>>
        Pending = 0
        Approved = 1
        Rejected = 2
    }

    class NotificationStatus {
        <<enumeration>>
        Unread = 1
        Read = 2
    }

    class NotificationType {
        <<enumeration>>
        ExpenseSubmitted = 1
        ExpenseApproved = 2
        ExpenseRejected = 3
        BudgetCreated = 4
        BudgetUpdated = 5
        BudgetDeleted = 6
    }

    class AuditAction {
        <<enumeration>>
        Create = 1
        Update = 2
        Delete = 3
    }

    class ReportScopeType {
        <<enumeration>>
        Period = 1
        Department = 2
        Budget = 3
    }

    class UserRole {
        <<enumeration>>
        Admin = 1
        Manager = 2
        Employee = 3
    }

    class SortOrder {
        <<enumeration>>
        Asc
        Desc
    }

    User --> UserStatus : uses
    Budget --> BudgetStatus : uses
    Expense --> ExpenseStatus : uses
    Notification --> NotificationStatus : uses
    Notification --> NotificationType : uses
    AuditLog --> AuditAction : uses
    Report --> ReportScopeType : uses
```

---

## 3. Repository Layer

```mermaid
classDiagram
    direction TB

    class IUserRepository {
        <<interface>>
        +GetByIdAsync(id) Task~User~
        +GetByEmailAsync(email) Task~User~
        +GetUserForLoginAsync(email) Task~User~
        +ValidateRefreshTokenAsync(userId, token) Task~User~
        +GetByEmployeeIdAsync(employeeId) Task~User~
        +GetByIdWithManagerAsync(id) Task~User~
        +GetByIdWithDetailsAsync(id) Task~User~
        +GetAllAsync() Task~IEnumerable~User~~
        +GetAllManagersAsync() Task~IEnumerable~User~~
        +GetEmployeesByManagerIdAsync(managerId) Task~IEnumerable~User~~
        +UserExistsAsync(id) Task~bool~
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
        <<interface>>
        +CreateBudgetAsync(dto, userId) Task~int~
        +UpdateBudgetAsync(budgetID, dto, userId) Task
        +DeleteBudgetAsync(budgetID, userId) Task
        +GetBudgetByIdAsync(budgetID) Task~BudgetDto~
        +GetAllBudgetsAsync(filters) Task~PagedResult~BudgetDto~~
        +GetBudgetsByCreatedByUserIdWithPaginationAsync(userId, filters) Task~PagedResult~BudgetDto~~
    }

    class IExpenseRepository {
        <<interface>>
        +GetAllExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +GetExpensesByBudgetIDAsync(budgetID, filters) Task~PagedResult~AllExpenseDto~~
        +GetManagedExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +CreateExpenseAsync(dto, userId) Task~int~
        +UpdateExpenseStatusAsync(id, status, userId, comments, reason) Task~bool~
        +GetExpenseStatisticsAsync(filters) Task~ExpenseStatisticsDto~
    }

    class ICategoryRepository {
        <<interface>>
        +GetAllCategoriesAsync() Task~List~CategoryResponseDto~~
        +CreateCategoryAsync(dto, userId) Task~int~
        +UpdateCategoryAsync(id, dto, userId) Task~bool~
        +DeleteCategoryAsync(id, userId) Task~bool~
    }

    class IDepartmentRepository {
        <<interface>>
        +GetAllDepartmentsAsync() Task~List~DepartmentResponseDto~~
        +CreateDepartmentAsync(dto, userId) Task~int~
        +UpdateDepartmentAsync(id, dto, userId) Task~bool~
        +DeleteDepartmentAsync(id, userId) Task~bool~
    }

    class INotificationRepository {
        <<interface>>
        +GetNotificationsByReceiverUserIdAsync(userId, message, status, sortOrder, page, size) Task~PagedResult~GetNotificationDto~~
        +MarkNotificationAsReadAsync(id, userId) Task~bool~
        +MarkAllNotificationsAsReadAsync(userId) Task~int~
        +DeleteNotificationAsync(id, userId) Task~bool~
        +DeleteAllNotificationsAsync(userId) Task~int~
    }

    class IAuditRepository {
        <<interface>>
        +GetAllAuditLogsAsync() Task~List~AuditLogDto~~
        +GetAllAuditLogsPaginatedAsync(page, size, search, action, entityType) Task~PagedResult~AuditLogDto~~
        +GetAuditLogsByUserIdAsync(userId) Task~List~AuditLogDto~~
    }

    class IReportRepository {
        <<interface>>
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
        ...
    }

    class BudgetRepository {
        -BudgetTrackDbContext _context
        +CreateBudgetAsync(dto, userId) Task~int~
        +UpdateBudgetAsync(budgetID, dto, userId) Task
        +DeleteBudgetAsync(budgetID, userId) Task
        +GetBudgetByIdAsync(budgetID) Task~BudgetDto~
        +GetAllBudgetsAsync(filters) Task~PagedResult~BudgetDto~~
        +GetBudgetsByCreatedByUserIdWithPaginationAsync(userId, filters) Task~PagedResult~BudgetDto~~
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
        +GetNotificationsByReceiverUserIdAsync(...) Task~PagedResult~GetNotificationDto~~
        +MarkNotificationAsReadAsync(id, userId) Task~bool~
        +MarkAllNotificationsAsReadAsync(userId) Task~int~
        +DeleteNotificationAsync(id, userId) Task~bool~
        +DeleteAllNotificationsAsync(userId) Task~int~
    }

    class AuditRepository {
        -BudgetTrackDbContext _context
        +GetAllAuditLogsAsync() Task~List~AuditLogDto~~
        +GetAllAuditLogsPaginatedAsync(...) Task~PagedResult~AuditLogDto~~
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
        <<interface>>
        +CreateBudgetAsync(dto, userId) Task~int~
        +UpdateBudgetAsync(budgetID, dto, userId) Task
        +DeleteBudgetAsync(budgetID, userId) Task
        +GetAllBudgetsAsync(filters) Task~PagedResult~BudgetDto~~
        +GetBudgetsByCreatedByUserIdWithPaginationAsync(userId, filters) Task~PagedResult~BudgetDto~~
    }

    class IExpenseService {
        <<interface>>
        +GetAllExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +GetExpensesByBudgetIDAsync(budgetID, filters) Task~PagedResult~AllExpenseDto~~
        +GetManagedExpensesAsync(filters) Task~PagedResult~AllExpenseDto~~
        +CreateExpenseAsync(dto, userId) Task~int~
        +UpdateExpenseStatusAsync(id, status, userId, comments, reason) Task~bool~
        +GetExpenseStatisticsAsync(filters) Task~ExpenseStatisticsDto~
    }

    class ICategoryService {
        <<interface>>
        +GetAllCategoriesAsync() Task~List~CategoryResponseDto~~
        +CreateCategoryAsync(dto, userId) Task~int~
        +UpdateCategoryAsync(id, dto, userId) Task~bool~
        +DeleteCategoryAsync(id, userId) Task~bool~
    }

    class IDepartmentService {
        <<interface>>
        +GetAllDepartmentsAsync() Task~List~DepartmentResponseDto~~
        +CreateDepartmentAsync(dto, userId) Task~int~
        +UpdateDepartmentAsync(id, dto, userId) Task~bool~
        +DeleteDepartmentAsync(id, userId) Task~bool~
    }

    class INotificationService {
        <<interface>>
        +GetNotificationsByReceiverUserIdAsync(userId, message, status, sortOrder, page, size) Task~PagedResult~GetNotificationDto~~
        +MarkNotificationAsReadAsync(id, userId) Task~bool~
        +MarkAllNotificationsAsReadAsync(userId) Task~int~
        +DeleteNotificationAsync(id, userId) Task~bool~
        +DeleteAllNotificationsAsync(userId) Task~int~
    }

    class IAuditService {
        <<interface>>
        +GetAllAuditLogsAsync() Task~List~AuditLogDto~~
        +GetAllAuditLogsPaginatedAsync(page, size, search, action, entityType) Task~PagedResult~AuditLogDto~~
        +GetAuditLogsByUserIdAsync(userId) Task~List~AuditLogDto~~
    }

    class IReportService {
        <<interface>>
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
        +GetNotificationsByReceiverUserIdAsync(...) Task~PagedResult~GetNotificationDto~~
        +MarkNotificationAsReadAsync(id, userId) Task~bool~
        +MarkAllNotificationsAsReadAsync(userId) Task~int~
        +DeleteNotificationAsync(id, userId) Task~bool~
        +DeleteAllNotificationsAsync(userId) Task~int~
    }

    class AuditService {
        -IAuditRepository _repo
        +GetAllAuditLogsAsync() Task~List~AuditLogDto~~
        +GetAllAuditLogsPaginatedAsync(...) Task~PagedResult~AuditLogDto~~
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
        <<ASP.NET Core>>
    }

    class BaseApiController {
        <<abstract>>
        #int UserId
        #int GetUserId()
    }

    class AuthController {
        -IAuthService _authService
        -IUserRepository _userRepository
        +AdminRegister(dto) Task POST /api/auth/createuser
        +Login(dto) Task POST /api/auth/login
        +ChangePassword(dto) Task POST /api/auth/changepassword
        +RefreshToken(dto) Task POST /api/auth/token/refresh
        +Logout() Task POST /api/auth/logout
        +Verify() IActionResult GET /api/auth/verify
        +GetUserProfile() Task GET /api/users/profile
        +GetUsers(filters) Task GET /api/users
        +UpdateUser(userId, dto) Task PUT /api/users/userId
    }

    class BudgetController {
        -IBudgetService _budgetService
        +GetAllBudgets(filters) Task GET /api/budgets/admin
        +GetBudgetsByUserWithPagination(filters) Task GET /api/budgets
        +CreateBudget(dto) Task POST /api/budgets
        +UpdateBudget(budgetID, dto) Task PUT /api/budgets/id
        +DeleteBudget(budgetID) Task DELETE /api/budgets/id
        +GetExpensesByBudgetID(budgetID, filters) Task GET /api/budgets/id/expenses
        -IsDuplicateBudgetKey(ex) bool
        -GetDuplicateBudgetMessage(ex) string
    }

    class ExpenseController {
        -IExpenseService _service
        +GetExpenseStatistics(filters) Task GET /api/expenses/stats
        +GetAllExpenses(filters) Task GET /api/expenses
        +GetManagedExpenses(filters) Task GET /api/expenses/managed
        +CreateExpense(dto) Task POST /api/expenses
        +UpdateExpenseStatus(expenseID, dto) Task PUT /api/expenses/status/id
    }

    class CategoryController {
        -ICategoryService _service
        +GetAllCategories() Task GET /api/categories
        +CreateCategory(dto) Task POST /api/categories
        +UpdateCategory(categoryID, dto) Task PUT /api/categories/id
        +DeleteCategory(categoryID) Task DELETE /api/categories/id
    }

    class DepartmentController {
        -IDepartmentService _service
        +GetAllDepartments() Task GET /api/departments
        +CreateDepartment(dto) Task POST /api/departments
        +UpdateDepartment(departmentID, dto) Task PUT /api/departments/id
        +DeleteDepartment(departmentID) Task DELETE /api/departments/id
    }

    class UserController {
        -IUserRepository _repo
        +GetUserStats() Task GET /api/users/stats
        +GetManagers() Task GET /api/users/managers
        +GetEmployeesByManagerId(managerId) Task GET /api/users/id/employees
        +SoftDeleteUser(userId) Task DELETE /api/users/id
    }

    class NotificationController {
        -INotificationService _service
        +GetNotifications(filters) Task GET /api/notifications
        +MarkAsRead(notificationID) Task PUT /api/notifications/read/id
        +MarkAllAsRead() Task PUT /api/notifications/readAll
        +DeleteNotification(notificationID) Task DELETE /api/notifications/id
        +DeleteAllNotifications() Task DELETE /api/notifications/deleteAll
    }

    class AuditController {
        -IAuditService _service
        +GetAllAuditLogs(filters) Task GET /api/audits
        +GetAuditLogsByUserId(userId) Task GET /api/audits/userId
    }

    class ReportController {
        -IReportService _service
        +GetPeriodReport(startDate, endDate) Task GET /api/reports/period
        +GetDepartmentReport() Task GET /api/reports/department
        +GetBudgetReport(budgetCode) Task GET /api/reports/budget
    }

    ControllerBase <|-- BaseApiController
    BaseApiController <|-- BudgetController
    BaseApiController <|-- ExpenseController
    BaseApiController <|-- CategoryController
    BaseApiController <|-- DepartmentController
    BaseApiController <|-- UserController
    BaseApiController <|-- NotificationController
    BaseApiController <|-- AuditController
    BaseApiController <|-- ReportController
    BaseApiController <|-- AuthController

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
        +DbSet~Report~ Reports
        +DbSet~AuditLog~ AuditLogs
        +DbSet~Department~ Departments
        +DbSet~Role~ Roles
        #OnModelCreating(modelBuilder) void
    }

    class JwtMiddleware {
        -RequestDelegate _next
        -ILogger _logger
        +JwtMiddleware(next, logger)
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
        <<interface>>
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
        +Create(items, page, size, total) PagedResult~T~$
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
    BudgetTrackDbContext --> Report
```

---

## 7. Full Architecture Overview

```mermaid
classDiagram
    direction LR

    class AngularSPA {
        <<Frontend>>
        +authInterceptor
        +authGuard
        +roleGuard
        +BudgetService
        +ExpenseService
        +AuthService
    }

    class JwtMiddleware {
        <<Middleware>>
        +InvokeAsync()
        +ValidateToken()
        +AttachUserToContext()
    }

    class BaseApiController {
        <<abstract Controller>>
        #int UserId
    }

    namespace Controllers {
        class AuthController
        class BudgetController
        class ExpenseController
        class CategoryController
        class DepartmentController
        class UserController
        class NotificationController
        class AuditController
        class ReportController
    }

    namespace Services {
        class IAuthService
        class IBudgetService
        class IExpenseService
        class ICategoryService
        class IDepartmentService
        class INotificationService
        class IAuditService
        class IReportService
    }

    namespace Repositories {
        class IUserRepository
        class IBudgetRepository
        class IExpenseRepository
        class ICategoryRepository
        class IDepartmentRepository
        class INotificationRepository
        class IAuditRepository
        class IReportRepository
    }

    class BudgetTrackDbContext {
        <<EF Core DbContext>>
    }

    class SQLServer {
        <<Database>>
        +tUser
        +tBudget
        +tExpense
        +tCategory
        +tDepartment
        +tRole
        +tNotification
        +tAuditLog
        +tReport
        +Views
        +StoredProcedures
    }

    AngularSPA --> JwtMiddleware : HTTP Request + Bearer Token
    JwtMiddleware --> BaseApiController : Validated context
    BaseApiController <|-- AuthController
    BaseApiController <|-- BudgetController
    BaseApiController <|-- ExpenseController
    BaseApiController <|-- CategoryController
    BaseApiController <|-- DepartmentController
    BaseApiController <|-- UserController
    BaseApiController <|-- NotificationController
    BaseApiController <|-- AuditController
    BaseApiController <|-- ReportController

    AuthController --> IAuthService
    BudgetController --> IBudgetService
    ExpenseController --> IExpenseService
    CategoryController --> ICategoryService
    DepartmentController --> IDepartmentService
    NotificationController --> INotificationService
    AuditController --> IAuditService
    ReportController --> IReportService

    IAuthService --> IUserRepository
    IBudgetService --> IBudgetRepository
    IExpenseService --> IExpenseRepository
    ICategoryService --> ICategoryRepository
    IDepartmentService --> IDepartmentRepository
    INotificationService --> INotificationRepository
    IAuditService --> IAuditRepository
    IReportService --> IReportRepository

    IUserRepository --> BudgetTrackDbContext
    IBudgetRepository --> BudgetTrackDbContext
    IExpenseRepository --> BudgetTrackDbContext
    ICategoryRepository --> BudgetTrackDbContext
    IDepartmentRepository --> BudgetTrackDbContext
    INotificationRepository --> BudgetTrackDbContext
    IAuditRepository --> BudgetTrackDbContext
    IReportRepository --> BudgetTrackDbContext

    BudgetTrackDbContext --> SQLServer : EF Core SQL Queries + SP Calls
```

---

## Role-Based Access Summary

| Controller               | Endpoint                           | Roles             |
| ------------------------ | ---------------------------------- | ----------------- |
| `AuthController`         | POST `/api/auth/createuser`        | Admin             |
| `AuthController`         | POST `/api/auth/login`             | Public            |
| `AuthController`         | POST `/api/auth/token/refresh`     | Public            |
| `AuthController`         | GET `/api/users`                   | Admin, Manager    |
| `AuthController`         | PUT `/api/users/{id}`              | Admin             |
| `BudgetController`       | GET `/api/budgets/admin`           | Admin             |
| `BudgetController`       | GET `/api/budgets`                 | Manager, Employee |
| `BudgetController`       | POST `/api/budgets`                | Manager           |
| `BudgetController`       | PUT `/api/budgets/{id}`            | Manager           |
| `BudgetController`       | DELETE `/api/budgets/{id}`         | Manager           |
| `BudgetController`       | GET `/api/budgets/{id}/expenses`   | All               |
| `ExpenseController`      | GET `/api/expenses/stats`          | All               |
| `ExpenseController`      | GET `/api/expenses`                | Admin             |
| `ExpenseController`      | GET `/api/expenses/managed`        | Manager, Employee |
| `ExpenseController`      | POST `/api/expenses`               | Manager, Employee |
| `ExpenseController`      | PUT `/api/expenses/status/{id}`    | Manager           |
| `CategoryController`     | GET `/api/categories`              | All               |
| `CategoryController`     | POST `/api/categories`             | Admin             |
| `CategoryController`     | PUT `/api/categories/{id}`         | Admin             |
| `CategoryController`     | DELETE `/api/categories/{id}`      | Admin             |
| `DepartmentController`   | GET `/api/departments`             | All               |
| `DepartmentController`   | POST/PUT/DELETE `/api/departments` | Admin             |
| `UserController`         | GET `/api/users/stats`             | Admin             |
| `UserController`         | GET `/api/users/managers`          | All               |
| `UserController`         | GET `/api/users/{id}/employees`    | All               |
| `NotificationController` | All endpoints                      | Manager, Employee |
| `AuditController`        | All endpoints                      | Admin             |
| `ReportController`       | GET `/api/reports/period`          | Admin             |
| `ReportController`       | GET `/api/reports/department`      | Admin             |
| `ReportController`       | GET `/api/reports/budget`          | Admin, Manager    |

---

*BudgetTrack Class Diagram — Generated 2026-03-06*
