using Budget_Track.Models.Entities;
using Budget_Track.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace Budget_Track.Data
{
    public static class DataSeeder
    {
        public static void SeedData(BudgetTrackDbContext context)
        {
            if (context.Users.Any())
                return;

            var hasher = new PasswordHasher<User>();
            var now = DateTime.UtcNow;

            string[] deptNames =
            {
                "Administration",
                "Software Development",
                "IT Infrastructure & Operations",
                "Quality Assurance & Testing",
                "Cybersecurity & Risk Management",
                "Product Management & Design",
            }
            ;
            string[] budgetNames =
            {
                "Application Development",
                "Cloud Infrastructure & Hosting",
                "Automated Testing Tools",
                "Threat Detection & Security Tools",
                "UI/UX Tools & Design",
            }
            ;

            string[] catNames =
            {
                "SaaS Subscriptions",
                "Cloud Infrastructure",
                "DevOps Tooling",
                "Technical Training",
                "Hardware Assets",
                "API Managed Services",
                "Security Audits",
                "Open Source Support",
                "Data Storage Fees",
                "Quality Assurance Tools",
            };

            for (int i = 0; i < deptNames.Length; i++)
            {
                context.Departments.Add(
                    new Department
                    {
                        DepartmentName = deptNames[i],
                        DepartmentCode = $"DEPT{i + 1:D3}",
                        IsActive = true,
                        CreatedDate = now,
                        IsDeleted = false,
                    }
                );
            }

            for (int i = 0; i < catNames.Length; i++)
            {
                context.Categories.Add(
                    new Category
                    {
                        CategoryName = catNames[i],
                        CategoryCode = $"CAT{i + 1:D3}",
                        IsActive = true,
                        CreatedDate = now,
                        IsDeleted = false,
                    }
                );
            }

            context.SaveChanges();

            var roles = new List<Role>
            {
                new Role
                {
                    RoleName = "Admin",
                    IsActive = true,
                    CreatedDate = now,
                    IsDeleted = false,
                },
                new Role
                {
                    RoleName = "Manager",
                    IsActive = true,
                    CreatedDate = now,
                    IsDeleted = false,
                },
                new Role
                {
                    RoleName = "Employee",
                    IsActive = true,
                    CreatedDate = now,
                    IsDeleted = false,
                },
            };

            context.Roles.AddRange(roles);
            context.SaveChanges();

            var admin = new User
            {
                FirstName = "Dharunkumar",
                LastName = "S",
                PasswordHash = "",
                Email = "Dharunkumar.S@budgettrack.com",
                EmployeeID = "ADM001",
                RoleID = 1,
                Status = UserStatus.Active,
                DepartmentID = 1,
                CreatedDate = now,
                IsDeleted = false,
            };

            admin.PasswordHash = hasher.HashPassword(admin, "Password@123");
            context.Users.Add(admin);
            context.SaveChanges();

            int adminId = admin.UserID;

            var departments = context.Departments.ToList();
            var categories = context.Categories.ToList();
            var rolesList = context.Roles.ToList();

            foreach (var dept in departments)
            {
                dept.CreatedByUserID = adminId;
            }

            foreach (var cat in categories)
            {
                cat.CreatedByUserID = adminId;
            }

            foreach (var role in rolesList)
            {
                role.CreatedByUserID = adminId;
            }

            context.SaveChanges();

            string[] mgrFirstNames = { "Sanika", "Rahul", "Anjali", "Karthik", "Pooja" };
            string[] mgrLastNames = { "Anil", "K", "S", "A", "S" };

            var managers = new List<User>();

            for (int i = 0; i < mgrFirstNames.Length; i++)
            {
                var manager = new User
                {
                    FirstName = mgrFirstNames[i],
                    LastName = mgrLastNames[i],
                    PasswordHash = "",
                    Email = $"{mgrFirstNames[i]}.{mgrLastNames[i]}@budgettrack.com",
                    EmployeeID = $"MGR{i + 1:D3}",
                    DepartmentID = i + 2,
                    RoleID = 2,
                    Status = UserStatus.Active,
                    ManagerID = null,
                    CreatedByUserID = adminId,
                    CreatedDate = now,
                    IsDeleted = false,
                };

                manager.PasswordHash = hasher.HashPassword(manager, "Password@123");
                managers.Add(manager);
            }

            context.Users.AddRange(managers);
            context.SaveChanges();

            string[] empFirstNames =
            {
                "Shivali",
                "Bhabin",
                "Navya",
                "Chandrika",
                "Vikram",
                "Sneha",
                "Arjun",
                "Ishita",
                "Deepak",
                "Riya",
            };

            string[] empLastNames =
            {
                "Sharma",
                "Jeratso",
                "Kalla",
                "Kotte",
                "K",
                "S",
                "K",
                "A",
                "S",
                "S",
            };

            var employees = new List<User>();

            for (int i = 0; i < empFirstNames.Length; i++)
            {
                int departmentId = (i / 2) + 2;

                var employee = new User
                {
                    FirstName = empFirstNames[i],
                    LastName = empLastNames[i],
                    PasswordHash = "",
                    Email = $"{empFirstNames[i]}.{empLastNames[i]}@budgettrack.com",
                    EmployeeID = $"EMP{i + 1:D3}",
                    DepartmentID = departmentId,
                    ManagerID = managers[departmentId - 2].UserID,
                    RoleID = 3, // Employee
                    Status = UserStatus.Active,
                    CreatedByUserID = adminId,
                    CreatedDate = now,
                    IsDeleted = false,
                };

                employee.PasswordHash = hasher.HashPassword(employee, "Password@123");
                employees.Add(employee);
            }

            context.Users.AddRange(employees);
            context.SaveChanges();

            string[] expenseTitles =
            {
                "Cloud Hosting Subscription",
                "Enterprise Software License",
                "DevOps Automation Tool",
                "Security Monitoring Platform",
                "AI Analytics Engine",
                "Project Management Suite",
                "Database Hosting Service",
                "CI/CD Pipeline Tool",
                "API Gateway Subscription",
                "Enterprise Collaboration Tool",
                "Threat Detection Software",
                "Source Control Platform",
                "Container Orchestration Service",
                "Performance Monitoring Tool",
                "Load Balancer Service",
                "Identity Access Management",
                "Endpoint Protection Suite",
                "Backup Recovery Service",
                "Virtual Machine Hosting",
                "Network Firewall Service",
                "Cloud Storage Subscription",
                "Server Monitoring Tool",
                "Code Quality Analyzer",
                "Data Visualization Platform",
                "Business Intelligence Suite",
                "Microservices Platform",
                "Testing Automation Tool",
                "Release Management Tool",
                "Incident Management Software",
                "Cloud Logging Service",
                "Serverless Compute Service",
                "API Monitoring Tool",
                "Agile Tracking Software",
                "Infrastructure Management Tool",
                "DevSecOps Platform",
            };

            string[] merchantNames =
            {
                "Microsoft Corporation",
                "Amazon Web Services",
                "Google Cloud Platform",
                "Oracle Corporation",
                "IBM Software Group",
                "SAP SE",
                "Salesforce Inc",
                "Adobe Systems",
                "Atlassian Pty Ltd",
                "ServiceNow Inc",
                "Red Hat Inc",
                "VMware Inc",
                "Cisco Systems",
                "Cloudflare Inc",
                "DigitalOcean LLC",
                "MongoDB Inc",
                "Snowflake Inc",
                "Datadog Inc",
                "HashiCorp Inc",
                "Splunk Inc",
                "Elastic NV",
                "GitHub LLC",
                "JetBrains s.r.o.",
                "Zoho Corporation",
                "Freshworks Inc",
                "NVIDIA Corporation",
                "Twilio Inc",
                "Stripe Technology",
                "Okta Inc",
                "CrowdStrike Holdings",
                "Fortinet Inc",
                "Akamai Technologies",
                "Dell Technologies",
                "HP Enterprise",
                "Infosys Technologies",
            };

            decimal[] allocatedAmounts = { 20000, 30000, 50000, 30000, 60000 };
            var random = new Random();

            for (int i = 0; i < allocatedAmounts.Length; i++)
            {
                var budget = new Budget
                {
                    Title = budgetNames[i],
                    Code = $"BT2600{i + 1}",
                    DepartmentID = i + 2,
                    AmountAllocated = allocatedAmounts[i],
                    AmountSpent = 0,
                    AmountRemaining = allocatedAmounts[i],
                    StartDate = now,
                    EndDate = now.AddYears(1),
                    Status = BudgetStatus.Active,
                    CreatedByUserID = managers[i].UserID,
                    CreatedDate = now,
                    IsDeleted = false,
                };

                context.Budgets.Add(budget);
                context.SaveChanges();

                var oddEmployee = employees[i * 2];
                var evenEmployee = employees[i * 2 + 1];

                decimal budgetSpent = 0;

                void AddExpense(User user, ExpenseStatus status)
                {
                    int index = random.Next(0, 35);
                    decimal amount = random.Next(500, 1500);

                    if (status == ExpenseStatus.Approved)
                        budgetSpent += amount;

                    context.Expenses.Add(
                        new Expense
                        {
                            BudgetID = budget.BudgetID,
                            CategoryID = random.Next(1, 11),
                            Title = expenseTitles[index],
                            MerchantName = merchantNames[index],
                            Amount = amount,
                            SubmittedByUserID = user.UserID,
                            SubmittedDate = now.AddDays(-random.Next(1, 60)),
                            Status = status,
                            CreatedDate = now,
                            IsDeleted = false,
                        }
                    );
                }

                for (int j = 0; j < 6; j++)
                    AddExpense(oddEmployee, ExpenseStatus.Pending);
                for (int j = 0; j < 2; j++)
                    AddExpense(oddEmployee, ExpenseStatus.Approved);
                for (int j = 0; j < 2; j++)
                    AddExpense(oddEmployee, ExpenseStatus.Rejected);

                for (int j = 0; j < 3; j++)
                    AddExpense(evenEmployee, ExpenseStatus.Approved);
                AddExpense(evenEmployee, ExpenseStatus.Pending);
                AddExpense(evenEmployee, ExpenseStatus.Rejected);

                budget.AmountSpent = budgetSpent;
                budget.AmountRemaining = budget.AmountAllocated - budgetSpent;

                context.SaveChanges();
            }
        }
    }
}
