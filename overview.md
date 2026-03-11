# BudgetTrack — Self Introduction

---

## Project Overview

I worked on a project called **BudgetTrack**, which is an internal budget planning and expense management system developed for organizations to handle departmental budgets and employee expense approvals. The project was built using **Angular 21** on the frontend, **ASP.NET Core 10 Web API** on the backend, and **SQL Server** as the database. The system has three user roles — Admin, Manager, and Employee — where Admins manage users and master data, Managers create budgets and approve or reject expenses, and Employees submit expenses and track their approval status. We used JWT-based authentication with access and refresh token rotation to secure the application.

---

## My Role — Budget Module

In this project, I developed the **Budget Module** end-to-end. On the database side, I designed the budget table and developed stored procedures for creating, updating, and soft-deleting budgets, where each procedure also handles audit logging and employee notifications within a single transaction. I also created SQL views to fetch budgets based on the user's role. On the backend, I developed the repository, service, and controller layers following a clean layered architecture. The API exposes role-based endpoints where Admins can view all budgets, Managers manage only their own budgets, and Employees can view their manager's budgets — which I implemented by embedding the manager's ID as a custom claim inside the Employee's JWT token. On the frontend, I developed the budget list page where the UI adapts based on the logged-in user's role — Admins get a full view, Managers see their own budgets with edit and delete options, and Employees get a read-only view. I also built the create and edit budget forms with validations, a utilization bar to show how much of the budget has been spent, and a navigation to drill into the expenses under each budget.

---

*BudgetTrack · 2026-03-11*
