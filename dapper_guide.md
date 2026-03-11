# ⚡ Dapper (C#) — Complete Reference Guide

---

## Table of Contents

1. [Getting Started](#1-getting-started)
2. [Database Providers with Dapper](#2-database-providers-with-dapper)
3. [Querying Data with Dapper](#3-querying-data-with-dapper)
4. [Executing Non-Query Commands](#4-executing-non-query-commands-with-dapper)
5. [Using Parameters with Dapper](#5-using-parameters-with-dapper)
6. [Using Relationships with Dapper](#6-using-relationships-with-dapper)
7. [Bulk Operations with Dapper](#7-bulk-operations-with-dapper)

---

## 1. Getting Started

### 1.1 What is Dapper?

**Dapper** is a lightweight, open-source **micro-ORM** for .NET, created by the Stack Overflow team. It extends `IDbConnection` with powerful helper methods that map SQL query results directly to C# objects — with minimal overhead.

```bash
dotnet add package Dapper
```

> 💡 Dapper is sometimes called a **"SQL mapper"** — it doesn't generate SQL for you, but it eliminates the tedious manual mapping of `SqlDataReader` columns to object properties.

**Without Dapper (raw ADO.NET):**
```csharp
var cmd = new SqlCommand("SELECT Id, Name, Price FROM Products WHERE Id = @id", conn);
cmd.Parameters.AddWithValue("@id", 1);
var reader = cmd.ExecuteReader();
var product = new Product
{
    Id    = (int)reader["Id"],
    Name  = (string)reader["Name"],
    Price = (decimal)reader["Price"]
};
```

**With Dapper:**
```csharp
var product = conn.QuerySingleOrDefault<Product>(
    "SELECT Id, Name, Price FROM Products WHERE Id = @id",
    new { id = 1 });
```

---

### 1.2 When Should You Use Dapper?

| Use Dapper When... | Use EF Core Instead When... |
|---|---|
| You need **maximum performance** | You prefer auto-generated SQL |
| You're comfortable writing SQL | You want LINQ query syntax |
| You need **fine-grained SQL control** | You need complex migration management |
| Working with **complex/legacy schemas** | You want full change tracking |
| Building **read-heavy** reporting apps | Rapid prototyping / CRUD apps |
| Query results don't map cleanly to models | Domain model ↔ DB schema match closely |

---

### 1.3 What Does Dapper Do?

Dapper adds **extension methods** to `IDbConnection`. Its core responsibilities:

| Feature | Description |
|---|---|
| **Object Mapping** | Maps SQL result columns → C# object properties automatically |
| **Parameter Binding** | Safely binds C# values into SQL queries |
| **Multiple Result Sets** | Read several result sets from one query |
| **Relationship Mapping** | Maps JOINs to nested objects (`SplitOn`) |
| **Async Support** | Full async/await via `QueryAsync`, `ExecuteAsync` |
| **Stored Procedures** | First-class support with `CommandType.StoredProcedure` |

---

### 1.4 Dapper vs Entity Framework

| Feature | Dapper | Entity Framework Core |
|---|---|---|
| **Type** | Micro-ORM | Full ORM |
| **SQL Writing** | Manual | Auto-generated |
| **Performance** | ⚡ Faster | Slightly slower |
| **Learning Curve** | Low (need SQL knowledge) | Medium-High |
| **Change Tracking** | ❌ None | ✅ Full |
| **Migrations** | ❌ Manual | ✅ Built-in |
| **LINQ Queries** | ❌ No | ✅ Yes |
| **Complex Queries** | ✅ Full SQL control | Sometimes awkward |
| **Setup** | Minimal | More configuration |
| **Best For** | Performance, reporting, legacy DB | CRUD apps, domain modeling |

---

## 2. Database Providers with Dapper

Dapper works with **any ADO.NET-compatible database**. Just install the right provider and create the connection.

### 2.1 SQL Server
```bash
dotnet add package Microsoft.Data.SqlClient
```
```csharp
using Microsoft.Data.SqlClient;

var connectionString = "Server=localhost;Database=MyDb;Trusted_Connection=true;";
using var conn = new SqlConnection(connectionString);

var products = conn.Query<Product>("SELECT * FROM Products").ToList();
```

---

### 2.2 Oracle
```bash
dotnet add package Oracle.ManagedDataAccess.Core
```
```csharp
using Oracle.ManagedDataAccess.Client;

var connectionString = "User Id=myuser;Password=mypass;Data Source=localhost:1521/ORCLCDB";
using var conn = new OracleConnection(connectionString);

var result = conn.Query<Product>("SELECT * FROM Products").ToList();
```

---

### 2.3 SQLite
```bash
dotnet add package Microsoft.Data.Sqlite
```
```csharp
using Microsoft.Data.Sqlite;

var connectionString = "Data Source=myapp.db";
using var conn = new SqliteConnection(connectionString);

var result = conn.Query<Product>("SELECT * FROM Products").ToList();
```

---

### 2.4 MySQL
```bash
dotnet add package MySqlConnector
```
```csharp
using MySqlConnector;

var connectionString = "Server=localhost;Database=MyDb;User=root;Password=secret;";
using var conn = new MySqlConnection(connectionString);

var result = conn.Query<Product>("SELECT * FROM Products").ToList();
```

---

### 2.5 PostgreSQL
```bash
dotnet add package Npgsql
```
```csharp
using Npgsql;

var connectionString = "Host=localhost;Database=mydb;Username=postgres;Password=secret;";
using var conn = new NpgsqlConnection(connectionString);

var result = conn.Query<Product>("SELECT * FROM products").ToList();
```

> 💡 **The Dapper code is identical across all providers** — only the connection class and connection string change. This makes switching databases straightforward.

---

## 3. Querying Data with Dapper

### Setup — Entity & Connection Helper
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string Category { get; set; } = "";
}

// Typical setup in a service/repository
private readonly string _connStr = "Server=...;Database=MyDb;Trusted_Connection=true;";
private SqlConnection GetConnection() => new SqlConnection(_connStr);
```

---

### 3.1 Querying Scalar Values — `ExecuteScalar`

Returns a **single value** (first column of first row):

```csharp
using var conn = GetConnection();

// Count
int count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Products");

// Single value with parameter
decimal maxPrice = conn.ExecuteScalar<decimal>(
    "SELECT MAX(Price) FROM Products WHERE Category = @category",
    new { category = "Electronics" });

// Check existence
bool exists = conn.ExecuteScalar<bool>(
    "SELECT CASE WHEN EXISTS(SELECT 1 FROM Products WHERE Id = @id) THEN 1 ELSE 0 END",
    new { id = 5 });

// Async version
int total = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Orders");
```

---

### 3.2 Querying Single Rows

| Method | Behavior when 0 rows | Behavior when 2+ rows |
|---|---|---|
| `QueryFirst<T>` | Throws exception | Returns first row |
| `QueryFirstOrDefault<T>` | Returns `null`/default | Returns first row |
| `QuerySingle<T>` | Throws exception | Throws exception |
| `QuerySingleOrDefault<T>` | Returns `null`/default | Throws exception |

```csharp
using var conn = GetConnection();

// QueryFirst — returns first row, throws if none
var product = conn.QueryFirst<Product>(
    "SELECT * FROM Products ORDER BY Price DESC");

// QueryFirstOrDefault — safe, returns null if not found
var product = conn.QueryFirstOrDefault<Product>(
    "SELECT * FROM Products WHERE Id = @id",
    new { id = 99 });

if (product is null)
    Console.WriteLine("Not found");

// QuerySingle — expects exactly one row
var product = conn.QuerySingle<Product>(
    "SELECT * FROM Products WHERE Id = @id",
    new { id = 1 });

// QuerySingleOrDefault — one or none, throws if multiple
var product = conn.QuerySingleOrDefault<Product>(
    "SELECT * FROM Products WHERE Name = @name",
    new { name = "Laptop" });

// Async
var product = await conn.QueryFirstOrDefaultAsync<Product>(
    "SELECT * FROM Products WHERE Id = @id", new { id = 1 });
```

---

### 3.3 Querying Multiple Rows — `Query<T>`

```csharp
using var conn = GetConnection();

// All rows
var products = conn.Query<Product>("SELECT * FROM Products").ToList();

// Filtered
var expensive = conn.Query<Product>(
    "SELECT * FROM Products WHERE Price > @minPrice ORDER BY Price DESC",
    new { minPrice = 500 }).ToList();

// Map to anonymous type
var names = conn.Query<string>("SELECT Name FROM Products").ToList();

// Map to dynamic (no class required)
var rows = conn.Query("SELECT Id, Name FROM Products").ToList();
foreach (var row in rows)
    Console.WriteLine($"{row.Id}: {row.Name}");

// Async
var products = (await conn.QueryAsync<Product>("SELECT * FROM Products")).ToList();
```

---

### 3.4 Querying Multiple Result Sets — `QueryMultiple`

Execute one SQL batch and read multiple result sets:

```csharp
using var conn = GetConnection();

var sql = @"
    SELECT COUNT(*) FROM Products;
    SELECT * FROM Products WHERE IsAvailable = 1;
    SELECT * FROM Categories;";

using var multi = conn.QueryMultiple(sql);

int totalCount        = multi.ReadFirst<int>();
var availableProducts = multi.Read<Product>().ToList();
var categories        = multi.Read<Category>().ToList();

// With parameters
var sql2 = @"
    SELECT * FROM Orders WHERE CustomerId = @id;
    SELECT * FROM Customers WHERE Id = @id;";

using var multi2 = conn.QueryMultiple(sql2, new { id = 1 });
var orders   = multi2.Read<Order>().ToList();
var customer = multi2.ReadFirst<Customer>();
```

---

### 3.5 Querying Specific Columns

Dapper maps **by column name** — only columns present in both the query and the class are mapped:

```csharp
// Only select what you need
var products = conn.Query<Product>(
    "SELECT Id, Name FROM Products")  // Price not selected → defaults to 0
    .ToList();

// Project into a lightweight DTO
public record ProductSummary(int Id, string Name);

var summaries = conn.Query<ProductSummary>(
    "SELECT Id, Name FROM Products WHERE IsAvailable = 1")
    .ToList();

// Column alias when DB name differs from C# property
var items = conn.Query<Product>(
    "SELECT product_id AS Id, product_name AS Name, unit_price AS Price FROM products")
    .ToList();
```

---

### 3.6 Executing Reader with Dapper

Use `ExecuteReader` for low-level streaming access when you need raw `IDataReader`:

```csharp
using var conn = GetConnection();
using var reader = conn.ExecuteReader("SELECT * FROM Products");

while (reader.Read())
{
    var id    = reader.GetInt32(reader.GetOrdinal("Id"));
    var name  = reader.GetString(reader.GetOrdinal("Name"));
    var price = reader.GetDecimal(reader.GetOrdinal("Price"));
    Console.WriteLine($"{id}: {name} — ${price}");
}
```

> 💡 `ExecuteReader` is useful for **streaming large result sets** without loading everything into memory at once.

---

### 3.7 Executing Stored Procedures with Dapper

```csharp
using var conn = GetConnection();

// Query with stored procedure
var products = conn.Query<Product>(
    "sp_GetProductsByCategory",
    new { Category = "Electronics" },
    commandType: CommandType.StoredProcedure).ToList();

// Single result
var product = conn.QueryFirstOrDefault<Product>(
    "sp_GetProductById",
    new { Id = 1 },
    commandType: CommandType.StoredProcedure);

// Execute (non-query) stored procedure
conn.Execute(
    "sp_ArchiveOldOrders",
    new { OlderThanDays = 365 },
    commandType: CommandType.StoredProcedure);

// With output parameter
var parameters = new DynamicParameters();
parameters.Add("@ProductId", 5);
parameters.Add("@TotalSold", dbType: DbType.Int32, direction: ParameterDirection.Output);

conn.Execute("sp_GetTotalSold", parameters, commandType: CommandType.StoredProcedure);
int totalSold = parameters.Get<int>("@TotalSold");
```

---

## 4. Executing Non-Query Commands with Dapper

All non-query operations use `Execute` / `ExecuteAsync` which returns the **number of rows affected**.

### 4.1 Dapper Insert

```csharp
using var conn = GetConnection();

// Insert single row
var sql = "INSERT INTO Products (Name, Price, Category) VALUES (@Name, @Price, @Category)";

int rowsAffected = conn.Execute(sql, new
{
    Name     = "Wireless Mouse",
    Price    = 29.99m,
    Category = "Electronics"
});

// Insert and return generated Id (SQL Server)
var insertSql = @"
    INSERT INTO Products (Name, Price, Category)
    VALUES (@Name, @Price, @Category);
    SELECT CAST(SCOPE_IDENTITY() AS INT);";

int newId = conn.ExecuteScalar<int>(insertSql, new
{
    Name     = "Keyboard",
    Price    = 49.99m,
    Category = "Electronics"
});

// Insert multiple rows (batch)
var products = new List<Product>
{
    new() { Name = "Monitor", Price = 299m, Category = "Electronics" },
    new() { Name = "Desk",    Price = 199m, Category = "Furniture" }
};

int rows = conn.Execute(sql, products);  // Dapper iterates the list
Console.WriteLine($"{rows} products inserted");
```

---

### 4.2 Dapper Update

```csharp
using var conn = GetConnection();

// Update single record
var sql = "UPDATE Products SET Price = @Price, Name = @Name WHERE Id = @Id";

int rowsAffected = conn.Execute(sql, new { Id = 1, Name = "Gaming Mouse", Price = 59.99m });

// Update multiple records
var updates = new List<dynamic>
{
    new { Id = 1, Price = 59.99m, Name = "Gaming Mouse" },
    new { Id = 2, Price = 89.99m, Name = "Mechanical Keyboard" }
};
conn.Execute(sql, updates);

// Conditional update
conn.Execute(
    "UPDATE Products SET IsAvailable = 0 WHERE Price > @maxPrice",
    new { maxPrice = 1000m });
```

---

### 4.3 Dapper Delete

```csharp
using var conn = GetConnection();

// Delete by Id
int rows = conn.Execute(
    "DELETE FROM Products WHERE Id = @id",
    new { id = 5 });

// Delete with condition
conn.Execute(
    "DELETE FROM Orders WHERE Status = @status AND OrderDate < @cutoff",
    new { status = "Cancelled", cutoff = DateTime.UtcNow.AddYears(-1) });

// Delete multiple by list of ids
conn.Execute(
    "DELETE FROM Products WHERE Id = @id",
    new[] { new { id = 1 }, new { id = 2 }, new { id = 3 } });
```

---

### 4.4 Dapper `ExecuteAsync`

All Dapper methods have async equivalents. Always prefer async in web applications:

```csharp
using var conn = GetConnection();

// Async insert
await conn.ExecuteAsync(
    "INSERT INTO Logs (Message, CreatedAt) VALUES (@Message, @CreatedAt)",
    new { Message = "User logged in", CreatedAt = DateTime.UtcNow });

// Async update
await conn.ExecuteAsync(
    "UPDATE Products SET Price = @Price WHERE Id = @Id",
    new { Id = 1, Price = 79.99m });

// Async delete
await conn.ExecuteAsync(
    "DELETE FROM Sessions WHERE ExpiresAt < @now",
    new { now = DateTime.UtcNow });

// Async in a transaction
using var transaction = conn.BeginTransaction();
try
{
    await conn.ExecuteAsync(
        "INSERT INTO Orders (CustomerId, Total) VALUES (@CustomerId, @Total)",
        new { CustomerId = 1, Total = 99.99m }, transaction);

    await conn.ExecuteAsync(
        "UPDATE Customers SET LastOrderDate = @date WHERE Id = @id",
        new { date = DateTime.UtcNow, id = 1 }, transaction);

    transaction.Commit();
}
catch
{
    transaction.Rollback();
    throw;
}
```

---

## 5. Using Parameters with Dapper

### 5.1 Why You Should Use Parameters?

**Never concatenate user input into SQL strings.** Always use parameterized queries:

```csharp
// ❌ DANGEROUS — SQL injection vulnerability
var name = userInput;  // e.g., "'; DROP TABLE Products;--"
conn.Query($"SELECT * FROM Products WHERE Name = '{name}'");

// ✅ SAFE — parameterized
conn.Query<Product>("SELECT * FROM Products WHERE Name = @name", new { name = userInput });
```

---

### 5.2 What is SQL Injection?

SQL Injection is an attack where malicious SQL is inserted into a query through user input.

```
Input:  admin' OR '1'='1
Query:  SELECT * FROM Users WHERE Name = 'admin' OR '1'='1'
Result: Returns ALL users — authentication bypassed!
```

Dapper **automatically parameterizes** values — the database treats them as data, never as SQL commands.

---

### 5.3 Dapper Anonymous Parameters

The simplest and most common approach — use C# anonymous objects:

```csharp
// Single parameter
conn.Query<Product>("SELECT * FROM Products WHERE Id = @id", new { id = 1 });

// Multiple parameters
conn.Query<Product>(
    "SELECT * FROM Products WHERE Category = @category AND Price < @maxPrice",
    new { category = "Electronics", maxPrice = 500m });

// Property names must match SQL parameter names (@name = Name)
conn.Execute(
    "UPDATE Products SET Name = @Name, Price = @Price WHERE Id = @Id",
    new { Id = 1, Name = "Laptop Pro", Price = 1299m });
```

---

### 5.4 Dapper Dynamic Parameters

Use `DynamicParameters` when you need to **build parameters programmatically** or use special parameter types:

```csharp
var parameters = new DynamicParameters();
parameters.Add("@Id", 1);
parameters.Add("@Name", "Laptop");
parameters.Add("@Price", 999m, DbType.Decimal);

var product = conn.QueryFirstOrDefault<Product>(
    "SELECT * FROM Products WHERE Id = @Id", parameters);

// Add parameters conditionally
var p = new DynamicParameters();
p.Add("@Category", "Electronics");

if (maxPrice.HasValue)
    p.Add("@MaxPrice", maxPrice.Value);

var sql = "SELECT * FROM Products WHERE Category = @Category"
        + (maxPrice.HasValue ? " AND Price <= @MaxPrice" : "");

var results = conn.Query<Product>(sql, p).ToList();
```

---

### 5.5 Dapper String Parameters

Control string parameter size and type explicitly to avoid implicit conversions:

```csharp
var parameters = new DynamicParameters();

// Specify size to match column definition (avoids index scan issues)
parameters.Add("@Name", "Laptop", DbType.String, size: 200);
parameters.Add("@Category", "Electronics", DbType.AnsiString, size: 100); // VARCHAR

var product = conn.QueryFirstOrDefault<Product>(
    "SELECT * FROM Products WHERE Name = @Name", parameters);
```

---

### 5.6 Dapper WHERE IN Parameters

Pass a collection to an `IN` clause using Dapper's built-in list expansion:

```csharp
// Dapper automatically expands the list into (@ids1, @ids2, @ids3, ...)
var ids = new[] { 1, 2, 3, 4, 5 };

var products = conn.Query<Product>(
    "SELECT * FROM Products WHERE Id IN @ids",
    new { ids }).ToList();

// Works with strings too
var categories = new[] { "Electronics", "Furniture", "Books" };

var products = conn.Query<Product>(
    "SELECT * FROM Products WHERE Category IN @categories",
    new { categories }).ToList();

// Dynamic list
var activeIds = GetActiveProductIds();
var results = conn.Query<Product>(
    "SELECT * FROM Products WHERE Id IN @activeIds AND IsAvailable = 1",
    new { activeIds }).ToList();
```

> 💡 Dapper expands `IN @param` automatically — no manual string building needed.

---

### 5.7 Dapper Table-Valued Parameters (SQL Server)

Pass a whole table as a parameter (SQL Server only) — great for bulk lookups:

```sql
-- In SQL Server, create a User-Defined Table Type first:
CREATE TYPE ProductIdList AS TABLE (Id INT NOT NULL)
```

```csharp
// Create DataTable matching the TVP schema
var table = new DataTable();
table.Columns.Add("Id", typeof(int));
table.Rows.Add(1);
table.Rows.Add(2);
table.Rows.Add(5);

var parameters = new DynamicParameters();
parameters.Add("@ProductIds", table.AsTableValuedParameter("ProductIdList"));

var products = conn.Query<Product>(
    "SELECT * FROM Products WHERE Id IN (SELECT Id FROM @ProductIds)",
    parameters).ToList();
```

---

### 5.8 Dapper Output Parameters

Read values returned by SQL output parameters or stored procedures:

```csharp
var parameters = new DynamicParameters();
parameters.Add("@Name", "New Product");
parameters.Add("@Price", 99.99m);
parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

conn.Execute(@"
    INSERT INTO Products (Name, Price)
    VALUES (@Name, @Price);
    SET @NewId = SCOPE_IDENTITY();", parameters);

int newId = parameters.Get<int>("@NewId");
Console.WriteLine($"Inserted with Id: {newId}");

// Input/Output parameter
parameters.Add("@Counter", 10, dbType: DbType.Int32,
    direction: ParameterDirection.InputOutput);
conn.Execute("sp_IncrementCounter", parameters, commandType: CommandType.StoredProcedure);
int updatedCounter = parameters.Get<int>("@Counter");
```

---

## 6. Using Relationships with Dapper

### 6.1 Dapper Relationships Overview

Dapper doesn't have built-in relationship tracking like EF Core. You handle relationships by:
1. Writing **JOIN queries**
2. Using `SplitOn` to tell Dapper where one object ends and another begins
3. Using **QueryMultiple** for separate queries

---

### 6.2 Dapper `SplitOn`

`SplitOn` tells Dapper which column marks the **start of a new object** in a JOIN result:

```csharp
public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public Customer Customer { get; set; } = null!;
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

using var conn = GetConnection();

var sql = @"
    SELECT o.Id, o.Total,
           c.Id, c.Name, c.Email        -- 'c.Id' is the split point
    FROM Orders o
    INNER JOIN Customers c ON o.CustomerId = c.Id";

var orders = conn.Query<Order, Customer, Order>(
    sql,
    (order, customer) =>
    {
        order.Customer = customer;  // Wire up the relationship
        return order;
    },
    splitOn: "Id"  // Split when second "Id" column is encountered
).ToList();
```

---

### 6.3 Dapper One-to-Many Relationships

One Customer → Many Orders. Use a dictionary to group rows:

```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Order> Orders { get; set; } = new();
}

public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

using var conn = GetConnection();

var sql = @"
    SELECT c.Id, c.Name,
           o.Id, o.Total, o.OrderDate
    FROM Customers c
    LEFT JOIN Orders o ON c.Id = o.CustomerId
    ORDER BY c.Id";

var customerDict = new Dictionary<int, Customer>();

conn.Query<Customer, Order, Customer>(
    sql,
    (customer, order) =>
    {
        // Get existing customer or add new one
        if (!customerDict.TryGetValue(customer.Id, out var existing))
        {
            existing = customer;
            customerDict[customer.Id] = existing;
        }

        if (order != null)
            existing.Orders.Add(order);  // Add order to the customer

        return existing;
    },
    splitOn: "Id"
);

var customers = customerDict.Values.ToList();
```

---

### 6.4 Dapper Many-to-Many Relationships

Students ↔ Courses through an Enrollments join table:

```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Course> Courses { get; set; } = new();
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
}

using var conn = GetConnection();

var sql = @"
    SELECT s.Id, s.Name,
           c.Id, c.Title
    FROM Students s
    INNER JOIN Enrollments e ON s.Id = e.StudentId
    INNER JOIN Courses c ON e.CourseId = c.Id
    ORDER BY s.Id";

var studentDict = new Dictionary<int, Student>();

conn.Query<Student, Course, Student>(
    sql,
    (student, course) =>
    {
        if (!studentDict.TryGetValue(student.Id, out var existing))
        {
            existing = student;
            studentDict[student.Id] = existing;
        }

        existing.Courses.Add(course);
        return existing;
    },
    splitOn: "Id"
);

var students = studentDict.Values.ToList();
```

---

### 6.5 Dapper Multiple Relationships

Map a single row to **three or more related objects**:

```csharp
public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}

using var conn = GetConnection();

var sql = @"
    SELECT oi.Id, oi.Quantity,
           o.Id,  o.Total,
           p.Id,  p.Name, p.Price,
           c.Id,  c.Name
    FROM OrderItems oi
    INNER JOIN Orders   o ON oi.OrderId   = o.Id
    INNER JOIN Products p ON oi.ProductId = p.Id
    INNER JOIN Customers c ON o.CustomerId = c.Id";

// Map 4 types: OrderItem, Order, Product, Customer
var items = conn.Query<OrderItem, Order, Product, Customer, OrderItem>(
    sql,
    (item, order, product, customer) =>
    {
        item.Order    = order;
        item.Product  = product;
        item.Customer = customer;
        return item;
    },
    splitOn: "Id, Id, Id"  // Split on each "Id" column — one per joined table
).ToList();
```

> 💡 `SplitOn` accepts a **comma-separated list** when splitting on multiple columns. Dapper supports mapping up to **7 types** in a single query.

**Alternative: Use QueryMultiple for complex cases (cleaner for deep nesting)**
```csharp
var sql = @"
    SELECT * FROM Orders WHERE CustomerId = @id;
    SELECT * FROM Customers WHERE Id = @id;
    SELECT oi.* FROM OrderItems oi
    INNER JOIN Orders o ON oi.OrderId = o.Id
    WHERE o.CustomerId = @id;";

using var multi = conn.QueryMultiple(sql, new { id = 1 });

var orders   = multi.Read<Order>().ToList();
var customer = multi.ReadFirst<Customer>();
var items    = multi.Read<OrderItem>().ToList();

// Wire up manually
foreach (var order in orders)
    order.Items = items.Where(i => i.OrderId == order.Id).ToList();
```

---

## 7. Bulk Operations with Dapper

### 7.1 Dapper Plus Overview

**Dapper Plus** is a premium extension library that adds high-performance bulk operations to Dapper.

```bash
dotnet add package Z.Dapper.Plus
```

```csharp
// One-time global configuration (e.g., in startup)
DapperPlusManager.Entity<Product>().Table("Products").Identity(x => x.Id);
DapperPlusManager.Entity<Customer>().Table("Customers").Identity(x => x.Id);
```

---

### 7.2 BulkInsert

Insert thousands of records in a single round-trip:

```csharp
using var conn = GetConnection();

var products = Enumerable.Range(1, 10000).Select(i => new Product
{
    Name     = $"Product {i}",
    Price    = i * 1.5m,
    Category = i % 2 == 0 ? "Electronics" : "Furniture"
}).ToList();

// ✅ BulkInsert — single DB operation
conn.BulkInsert(products);

// Compare with standard Dapper (10,000 round-trips):
// conn.Execute("INSERT INTO Products...", products);  ← Much slower

// Async
await conn.BulkInsertAsync(products);
```

---

### 7.3 BulkUpdate

```csharp
using var conn = GetConnection();

// Update specific columns only
conn.BulkUpdate(products);

// Configure which columns to update
DapperPlusManager.Entity<Product>("UpdatePriceOnly")
    .Table("Products")
    .Identity(x => x.Id)
    .Ignore(x => x.Name)       // Don't update Name
    .Ignore(x => x.Category);  // Don't update Category

conn.BulkUpdate("UpdatePriceOnly", products);

// Async
await conn.BulkUpdateAsync(products);
```

---

### 7.4 BulkDelete

```csharp
using var conn = GetConnection();

// Delete a list of entities by primary key
var toDelete = conn.Query<Product>(
    "SELECT * FROM Products WHERE IsAvailable = 0").ToList();

conn.BulkDelete(toDelete);

// Async
await conn.BulkDeleteAsync(toDelete);
```

---

### 7.5 BulkMerge (Upsert)

Insert new records and update existing ones in a single operation:

```csharp
using var conn = GetConnection();

// Records with Id = 0 → INSERT; Records with existing Id → UPDATE
var productsToSync = new List<Product>
{
    new() { Id = 0,  Name = "New Product A", Price = 49m },   // Will INSERT
    new() { Id = 1,  Name = "Updated Laptop", Price = 1199m }, // Will UPDATE
    new() { Id = 0,  Name = "New Product B", Price = 89m },   // Will INSERT
    new() { Id = 5,  Name = "Updated Mouse", Price = 39m }    // Will UPDATE
};

conn.BulkMerge(productsToSync);

// After merge, all objects have their Id populated
foreach (var p in productsToSync)
    Console.WriteLine($"Id: {p.Id} — {p.Name}");

// Async
await conn.BulkMergeAsync(productsToSync);
```

---

### Performance Comparison

| Operation | Standard Dapper | Dapper Plus |
|---|---|---|
| Insert 10,000 rows | ~10,000 round-trips | 1 round-trip |
| Update 5,000 rows | ~5,000 round-trips | 1 round-trip |
| Delete 2,000 rows | ~2,000 round-trips | 1 round-trip |
| Merge 8,000 rows | Not built-in | 1 round-trip |
| **Speed improvement** | Baseline | **10–50x faster** |

> 💡 For bulk inserts without Dapper Plus, you can use `SqlBulkCopy` (SQL Server) combined with Dapper for maximum performance at no cost.

```csharp
// Free alternative: SqlBulkCopy for SQL Server inserts
var table = new DataTable();
table.Columns.Add("Name", typeof(string));
table.Columns.Add("Price", typeof(decimal));

foreach (var p in products)
    table.Rows.Add(p.Name, p.Price);

using var bulkCopy = new SqlBulkCopy(conn);
bulkCopy.DestinationTableName = "Products";
await bulkCopy.WriteToServerAsync(table);
```

---

*End of Dapper Complete Reference Guide*
