Here's a thorough explanation of all five SOLID principles applied directly to your **BudgetTrack** project:

---

## S — Single Responsibility Principle

**"A class should have only one reason to change."**

In BudgetTrack, each layer has exactly one job:

- `ExpenseController` only handles HTTP routing — it doesn't contain business logic.
- `ExpenseService` only handles business rules (e.g., checking if a budget has enough remaining amount before logging an expense).
- `ExpenseRepository` only talks to the database via EF Core.

If the approval logic changes, you only touch `ExpenseService`. If the DB schema changes, you only touch `ExpenseRepository`. Neither affects the other.

**Violation example to avoid:** Putting budget balance recalculation logic directly inside the controller — that mixes HTTP concerns with business logic.

---

## O — Open/Closed Principle

**"Open for extension, closed for modification."**

Your `Report` module supports three scopes: Department, Budget, and Period. If you need to add a new scope (say, per-user reports), you should be able to add a new class/handler without modifying the existing report generation code.

A clean way to apply this in BudgetTrack is using a strategy pattern for report generation:

```csharp
// Closed for modification
public interface IReportStrategy {
    ReportResult Generate(ReportRequest request);
}

// Open for extension — add new types without touching existing ones
public class DepartmentReportStrategy : IReportStrategy { ... }
public class BudgetReportStrategy    : IReportStrategy { ... }
public class PeriodReportStrategy    : IReportStrategy { ... }
```

---

## L — Liskov Substitution Principle

**"Subtypes must be substitutable for their base types."**

BudgetTrack uses interface-based DI throughout:

```csharp
IExpenseService    → ExpenseService
IBudgetRepository  → BudgetRepository
INotificationService → NotificationService
```

Any consumer that depends on `IExpenseService` should work correctly whether it receives the real `ExpenseService` or a mock in a unit test. If you ever swap out `SqlServerBudgetRepository` for a `PostgresBudgetRepository`, the service layer should not need to change at all — because it only depends on `IBudgetRepository`.

**Violation to avoid:** A subclass that throws `NotImplementedException` on a method defined in the interface — that breaks substitutability.

---

## I — Interface Segregation Principle

**"Clients should not be forced to depend on interfaces they don't use."**

Consider your three roles — Employee, Manager, Admin. Instead of one fat interface:

```csharp
// BAD — Employee is forced to implement budget creation it doesn't need
public interface IBudgetService {
    void CreateBudget(...);     // Manager only
    void ApproveBudget(...);    // Manager only
    List<Budget> GetMyBudgets(); // Employee uses this
    void GenerateReport(...);   // Admin only
}
```

Split it properly:

```csharp
public interface IEmployeeBudgetService  { List<Budget> GetMyBudgets(); }
public interface IManagerBudgetService   { void CreateBudget(...); void ApproveBudget(...); }
public interface IAdminReportingService  { void GenerateReport(...); }
```

This is already reflected in your role-guarded Angular routes (`roleGuard('Admin', 'Manager')`) — the same thinking should carry through to your service interfaces on the backend.

---

## D — Dependency Inversion Principle

**"High-level modules should not depend on low-level modules. Both should depend on abstractions."**

Your LLD already follows this well. `ExpenseService` (high-level) does not directly instantiate `ExpenseRepository` (low-level). Instead, both depend on the `IExpenseRepository` abstraction, wired together via ASP.NET Core's built-in DI container:

```csharp
// In Program.cs
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
```

```csharp
// ExpenseService depends on abstraction, not concrete class
public class ExpenseService : IExpenseService {
    private readonly IExpenseRepository _repo;
    public ExpenseService(IExpenseRepository repo) => _repo = repo;
}
```

This is also why your known issue #5 — `UserController` injecting `DbContext` directly — is a DIP violation. It couples the controller directly to a concrete infrastructure detail, bypassing the repository abstraction entirely.

---

## Summary Table

| Principle | Where It Applies in BudgetTrack |
|---|---|
| **S** — Single Responsibility | Controller / Service / Repository each have one job |
| **O** — Open/Closed | Report scopes extensible via strategy pattern without modifying core |
| **L** — Liskov Substitution | Interface-based DI allows real/mock substitution seamlessly |
| **I** — Interface Segregation | Role-specific service interfaces — Employee, Manager, Admin |
| **D** — Dependency Inversion | Services depend on `IRepository` abstractions, not concrete EF classes |