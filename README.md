# 💰 BudgetTrack

<div align="center">

**Internal Budget Planning & Expense Management System**

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-21-DD0031?style=flat-square&logo=angular)
![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-CC2927?style=flat-square&logo=microsoftsqlserver)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat-square&logo=bootstrap)
![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?style=flat-square&logo=typescript)
![EF Core](https://img.shields.io/badge/EF_Core-10.0-512BD4?style=flat-square)

*A full-stack enterprise application for departmental budget planning, expense tracking, and financial reporting.*

</div>

---

## 📋 Table of Contents

- [About](#-about)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [User Roles](#-user-roles)
- [Quick Start](#-quick-start)
- [Project Structure](#-project-structure)
- [API Overview](#-api-overview)
- [Documentation](#-documentation)

---

## 🧭 About

**BudgetTrack** replaces manual, spreadsheet-driven expense workflows with a structured, role-aware digital platform. It covers the full lifecycle — from budget creation by a Manager, through expense submission by Employees, all the way to Manager approval, Admin reporting, and an immutable audit trail.

---

## ✨ Features

| Feature | Detail |
|---|---|
| 🔐 **JWT Authentication** | Access token (60 min) + refresh token (7 days) with automatic renewal via HTTP interceptor |
| 👥 **Role-Based Access** | Admin / Manager / Employee — scoped API endpoints and UI views per role |
| 📊 **Budget Management** | Create, update, soft-delete budgets with live `AmountSpent` & `AmountRemaining` tracking |
| 🧾 **Expense Workflow** | Submit → Pending → Approved / Rejected with manager comments and automatic budget recalculation |
| 🔔 **In-App Notifications** | Auto-alerts on expense events and budget changes; real-time navbar badge |
| 📜 **Immutable Audit Log** | Every create/update/delete recorded with full old/new JSON snapshots |
| 📈 **Reports & Charts** | Period, Department, and Budget-level reports with Chart.js visualisations |
| 🗑️ **Soft Delete** | Records are never hard-deleted — `IsDeleted` flag preserves complete history |
| ⚡ **SSG Prerendering** | Angular `outputMode: "static"` — all named routes prerendered at build time |
| 🏗️ **Stored Procedures** | Complex writes handled atomically inside SPs (notifications + audit + balance in one transaction) |

---

## 🛠 Tech Stack

### Backend
| Technology | Version |
|---|---|
| ASP.NET Core Web API | .NET 10.0 |
| Entity Framework Core | 10.0.2 |
| SQL Server / LocalDB | 2019+ |
| JWT Bearer Auth | 10.0.2 |
| ASP.NET Identity `PasswordHasher` | Built-in |
| Swashbuckle / Swagger | 6.5.0 |

### Frontend
| Technology | Version |
|---|---|
| Angular (Standalone + Signals) | 21.1.0 |
| Angular SSR (Static Generation) | 21.2.0 |
| Bootstrap | 5.3.8 |
| Chart.js | 4.5.1 |
| TypeScript | 5.9.2 |
| RxJS | 7.8.0 |

---

## 🏛 Architecture

```
┌─────────────────────────────────────────┐
│         Angular 21 SPA  (:4200)         │
│  authGuard · roleGuard · authInterceptor│
└──────────────────┬──────────────────────┘
                   │  HTTPS + Bearer JWT
┌──────────────────▼──────────────────────┐
│     ASP.NET Core 10 Web API  (:5131)    │
│  JwtMiddleware → Controllers            │
│              → Services                 │
│              → Repositories             │
└──────────────────┬──────────────────────┘
                   │  EF Core + Raw SP calls
┌──────────────────▼──────────────────────┐
│   SQL Server  (Budget-Track database)   │
│   9 Tables · 5 Views · 25+ SPs         │
└─────────────────────────────────────────┘
```

**Key Patterns:** Repository · Service Layer · DTO · Soft Delete · Audit Trail · Stored Procedure Centricity · Angular Signals · SSG Prerender

---

## 👤 User Roles

| Role | Key Capabilities |
|---|---|
| 🔴 **Admin** | Register / update / delete users · Manage departments & categories · View all budgets · Full audit log · Period & department reports |
| 🟡 **Manager** | Create & own budgets · Approve / reject team expenses · Budget-level reports · Receive notifications |
| 🟢 **Employee** | Submit expenses against manager's budgets · Track approval status · Receive approval / rejection notifications |

---

## 🚀 Quick Start

### Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 10.0+ |
| Node.js | 20+ LTS |
| npm | 11+ |
| SQL Server / LocalDB | 2019+ |
| Angular CLI | 21+ |

### 1 — Backend (ASP.NET Core Web API)

```powershell
cd Backend\Budget-Track
dotnet restore
dotnet run
```

| Endpoint | URL |
|---|---|
| REST API | `http://localhost:5131` |
| Swagger UI | `http://localhost:5131/swagger` |

> **First run:** `Program.cs` automatically runs EF Core migrations and seeds default Roles, a Department, and an Admin user.

### 2 — Frontend (Angular SPA)

```powershell
cd Frontend\Budget-Track
npm install
npm start
```

| Endpoint | URL |
|---|---|
| Angular App | `http://localhost:4200` |

### Default Admin Credentials

| Email | Password |
|---|---|
| `admin@budgettrack.com` | `Admin@123` |

### Environment Config

**Backend — `appsettings.json`**
```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-at-least-32-characters",
    "Issuer": "BudgetTrack",
    "Audience": "BudgetTrackUsers",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Budget-Track;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Frontend — `src/environments/environment.ts`**
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5131'
};
```

### Production Build

```powershell
# Backend
dotnet publish -c Release -o ./publish

# Frontend (SSG — deploy dist/budget-track/browser/ to any static host)
ng build --configuration production
```

---

## 📁 Project Structure

```
BudgetTrack/
├── Backend/Budget-Track/          # ASP.NET Core 10 Web API
│   ├── Controllers/               # 10 API controllers (Auth, Budget, Expense …)
│   ├── Services/                  # Business logic (Interfaces + Implementation)
│   ├── Repositories/              # Data access (Interfaces + Implementation)
│   ├── Models/                    # Entities · DTOs · Enums
│   ├── Middleware/                # JwtMiddleware · JwtSettings
│   ├── Data/                      # BudgetTrackDbContext · DataSeeder
│   └── Program.cs                 # DI registration · middleware pipeline
├── Database/Budget-Track/         # SQL scripts — tables, views, stored procedures
├── Documentation/
│   ├── BudgetTrack.md             # ← Complete technical reference (start here)
│   ├── ClassDiagram.md            # UML class & ER diagrams
│   ├── SequenceDiagram.md         # 33 end-to-end sequence diagrams
│   ├── lld.md                     # Low-level design
│   ├── API/                       # Per-module API docs (9 files)
│   ├── Budget.md · Expense.md     # Domain module docs
│   ├── Auth.md · Category.md      #
│   ├── Department.md · Audit.md   #
│   ├── Notification.md            #
│   └── Report.md                  #
├── Frontend/Budget-Track/         # Angular 21 SPA
│   └── src/
│       ├── app/                   # Components: auth · layout · features · shared
│       ├── core/                  # Guards · interceptors · core services
│       ├── models/                # TypeScript interfaces
│       └── services/              # Domain HTTP services (8 files)
└── README.md
```

---

## 🔌 API Overview

> **Base URL:** `http://localhost:5131`  
> **Auth:** `Authorization: Bearer <access_token>`

| Module | Key Endpoints |
|---|---|
| **Auth** | `POST /api/auth/login` · `POST /api/auth/createuser` · `POST /api/auth/token/refresh` · `POST /api/auth/logout` |
| **Users** | `GET /api/users` · `GET /api/users/profile` · `PUT /api/users/{id}` · `DELETE /api/users/{id}` |
| **Budgets** | `GET /api/budgets` · `GET /api/budgets/admin` · `POST /api/budgets` · `PUT /api/budgets/{id}` · `DELETE /api/budgets/{id}` |
| **Expenses** | `GET /api/expenses/managed` · `POST /api/expenses` · `PUT /api/expenses/status/{id}` · `GET /api/expenses/stats` |
| **Categories** | `GET /api/categories` · `POST /api/categories` · `PUT /api/categories/{id}` · `DELETE /api/categories/{id}` |
| **Departments** | `GET /api/departments` · `POST /api/departments` · `PUT /api/departments/{id}` · `DELETE /api/departments/{id}` |
| **Notifications** | `GET /api/notifications` · `GET /api/notifications/unread-count` · `PUT /api/notifications/read/{id}` · `PUT /api/notifications/readAll` |
| **Reports** | `GET /api/reports/period` · `GET /api/reports/department` · `GET /api/reports/budget` |
| **Audit** | `GET /api/audits` · `GET /api/audits/{userId}` |

---

## 📚 Documentation

### Core Reference

| File | Contents |
|---|---|
| [`Documentation/BudgetTrack.md`](Documentation/BudgetTrack.md) | Architecture · DB schema · All API endpoints · Auth flow · Frontend structure · SPs · Build guide · Design patterns |
| [`Documentation/ClassDiagram.md`](Documentation/ClassDiagram.md) | Entity classes · Enums · Repository/Service interfaces · Controller layer · Full architecture flowchart |
| [`Documentation/SequenceDiagram.md`](Documentation/SequenceDiagram.md) | 33 diagrams — per-feature flows + Admin / Manager / Employee complete journeys |
| [`Documentation/lld.md`](Documentation/lld.md) | System architecture · DB design · Security model · API conventions · Data flow · Error handling |

### Module Docs (12-Section Standard)

| File | Module |
|---|---|
| [`Documentation/Budget.md`](Documentation/Budget.md) | Budget lifecycle · SPs · API · Angular frontend · Data flow · State machine |
| [`Documentation/Expense.md`](Documentation/Expense.md) | Expense submission & approval · SPs · API · Angular frontend |
| [`Documentation/Auth.md`](Documentation/Auth.md) | JWT auth · Refresh tokens · RBAC · Angular guards & interceptor |
| [`Documentation/Category.md`](Documentation/Category.md) | Category CRUD · SP · API · Angular frontend |
| [`Documentation/Department.md`](Documentation/Department.md) | Department CRUD · SP · API · Angular frontend |
| [`Documentation/Notification.md`](Documentation/Notification.md) | Notification fanout · SP · Navbar badge · Mark read / delete |
| [`Documentation/Audit.md`](Documentation/Audit.md) | Immutable audit trail · JSON snapshots · Admin-only viewer |
| [`Documentation/Report.md`](Documentation/Report.md) | Period · Department · Budget reports · Chart.js integration |

### API Reference

| File | Module |
|---|---|
| [`Documentation/API/AuthAPI.md`](Documentation/API/AuthAPI.md) | Auth & User management |
| [`Documentation/API/BudgetAPI.md`](Documentation/API/BudgetAPI.md) | Budgets |
| [`Documentation/API/ExpenseAPI.md`](Documentation/API/ExpenseAPI.md) | Expenses |
| [`Documentation/API/CategoryAPI.md`](Documentation/API/CategoryAPI.md) | Categories |
| [`Documentation/API/DepartmentAPI.md`](Documentation/API/DepartmentAPI.md) | Departments |
| [`Documentation/API/NotificationAPI.md`](Documentation/API/NotificationAPI.md) | Notifications |
| [`Documentation/API/ReportAPI.md`](Documentation/API/ReportAPI.md) | Reports |
| [`Documentation/API/AuditAPI.md`](Documentation/API/AuditAPI.md) | Audit Logs |
| [`Documentation/API/UserAPI.md`](Documentation/API/UserAPI.md) | Users |

---

<div align="center">

*BudgetTrack · Last updated 2026-03-07*

</div>
