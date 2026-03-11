# 🗄️ Entity Framework Core 8.0 — Complete Reference Guide

---

## Table of Contents

1. [Overview of EF Core 8 and .NET 8 Integration](#1-overview-of-ef-core-8-and-net-8-integration)
2. [Setting Up EF Core in a .NET 8 Project](#2-setting-up-ef-core-in-a-net-8-project)
3. [Creating a Simple Database Model](#3-creating-a-simple-database-model)
4. [Performing Basic CRUD Operations](#4-performing-basic-crud-operations)
5. [LINQ Queries in EF Core 8](#5-linq-queries-in-ef-core-8)
6. [EF Core Migrations and Database Updates](#6-ef-core-migrations-and-database-updates)
7. [Handling Relationships and Data Loading](#7-handling-relationships-and-data-loading)
8. [Performance Optimizations and Best Practices](#8-performance-optimizations-and-best-practices)

---

## 1. Overview of EF Core 8 and .NET 8 Integration

### 1.1 What is ORM (Object-Relational Mapping)?

An **ORM** is a technique that lets you interact with a relational database using **object-oriented code** instead of raw SQL. It maps database tables to C# classes, and rows to objects.

**Without ORM (raw ADO.NET):**
```csharp
var cmd = new SqlCommand("SELECT * FROM Products WHERE Id = @id", conn);
cmd.Parameters.AddWithValue("@id", 1);
var reader = cmd.ExecuteReader();
var product = new Product
{
    Id = (int)reader["Id"],
    Name = (string)reader["Name"]
};
```

**With EF Core ORM:**
```csharp
var product = await context.Products.FindAsync(1);
// EF generates and executes the SQL for you
```

**Benefits of ORM:**
| Benefit | Description |
|---|---|
| **Productivity** | No hand-written SQL for common operations |
| **Type Safety** | Compile-time errors instead of runtime SQL bugs |
| **Maintainability** | Database schema lives in C# classes |
| **Database Portability** | Switch databases with minimal code changes |
| **Change Tracking** | Automatically detects and persists object changes |

---

### 1.2 EF Core vs EF Framework: Key Differences

| Feature | EF Framework (6.x) | EF Core (8.0) |
|---|---|---|
| **Platform** | .NET Framework only | Cross-platform (.NET 8) |
| **Performance** | Slower | Significantly faster |
| **Open Source** | Partial | Fully open source |
| **Migrations** | Yes | Yes (improved) |
| **Lazy Loading** | Default on | Opt-in |
| **Raw SQL** | Limited | Full support with `FromSql` |
| **JSON Columns** | No | ✅ Yes (EF Core 7+) |
| **Bulk Operations** | No | ✅ `ExecuteUpdate` / `ExecuteDelete` |
| **Compiled Queries** | No | ✅ Yes |
| **Many-to-Many** | Manual join entity | Auto-configured |

---

### 1.3 New Features in EF Core 8

#### Primitive Collections
```csharp
// Store a list of strings/ints directly in a column (as JSON)
public class Product
{
    public int Id { get; set; }
    public List<string> Tags { get; set; } = new();  // Stored as JSON column
}

// Query against the collection
var products = await context.Products
    .Where(p => p.Tags.Contains("electronics"))
    .ToListAsync();
```

#### JSON Columns (Enhanced)
```csharp
public class Order
{
    public int Id { get; set; }
    public Address ShippingAddress { get; set; }  // Stored as JSON column
}

// Query into nested JSON
var orders = await context.Orders
    .Where(o => o.ShippingAddress.City == "London")
    .ToListAsync();
```

#### `ExecuteUpdate` and `ExecuteDelete` (Bulk ops without loading entities)
```csharp
// Update without loading into memory
await context.Products
    .Where(p => p.Category == "Electronics")
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Price, p => p.Price * 0.9m));

// Delete without loading
await context.Orders
    .Where(o => o.Status == "Cancelled")
    .ExecuteDeleteAsync();
```

#### Keyed Services Support for `DbContext`
```csharp
// Register multiple DbContexts with keys (.NET 8 DI)
builder.Services.AddKeyedDbContext<AppDbContext>("main", options =>
    options.UseSqlServer(connectionString));
```

---

## 2. Setting Up EF Core in a .NET 8 Project

### 2.1 Installing EF Core Packages via NuGet

```bash
# Core EF package
dotnet add package Microsoft.EntityFrameworkCore

# SQL Server provider
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# SQLite provider (lightweight, great for dev/testing)
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# Design-time tools (for migrations)
dotnet add package Microsoft.EntityFrameworkCore.Design

# EF Core CLI tools (global install)
dotnet tool install --global dotnet-ef
```

> 💡 All packages should be on the **same version** to avoid conflicts (e.g., all `8.0.x`).

---

### 2.2 Configuring DbContext

`DbContext` is the primary class you work with — it represents a session with the database.

```csharp
// AppDbContext.cs
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // Each DbSet maps to a database table
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API configuration goes here
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}
```

---

### 2.3 Connecting to SQL Server

**In `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyAppDb;Trusted_Connection=true;"
  }
}
```

**In `Program.cs` (.NET 8 minimal API style):**
```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with DI container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
```

**For SQLite (simpler setup for development):**
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=myapp.db"));
```

---

### 2.4 Basic EF Core CLI Commands

```bash
# Add a new migration
dotnet ef migrations add InitialCreate

# Apply migrations to the database
dotnet ef database update

# View pending migrations
dotnet ef migrations list

# Remove the last unapplied migration
dotnet ef migrations remove

# Drop the database entirely
dotnet ef database drop

# Generate SQL script from migrations (for production deployment)
dotnet ef migrations script

# Scaffold DbContext from an existing database (DB-first)
dotnet ef dbcontext scaffold "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer
```

---

## 3. Creating a Simple Database Model

### 3.1 Defining Entities and Relationships

An **entity** is a C# class that maps to a database table. EF Core uses conventions to automatically infer most configuration.

```csharp
// Customer.cs
public class Customer
{
    public int Id { get; set; }             // Convention: PK named "Id"
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property — one Customer has many Orders
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

// Product.cs
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string Category { get; set; } = "";
    public bool IsAvailable { get; set; } = true;
}

// Order.cs
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }

    // Foreign Key
    public int CustomerId { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

// OrderItem.cs (Join table for Order ↔ Product)
public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
```

---

### 3.2 Primary Keys, Foreign Keys, and Navigation Properties

#### Primary Keys
```csharp
// Convention: property named "Id" or "EntityNameId" → auto PK
public int Id { get; set; }

// Explicit via Data Annotation
[Key]
public int ProductId { get; set; }

// Explicit via Fluent API
modelBuilder.Entity<Product>()
    .HasKey(p => p.ProductId);

// Composite Key (Fluent API only)
modelBuilder.Entity<OrderItem>()
    .HasKey(oi => new { oi.OrderId, oi.ProductId });
```

#### Foreign Keys & Navigation Properties
```csharp
public class Order
{
    public int Id { get; set; }

    // FK property (convention: navigation property name + "Id")
    public int CustomerId { get; set; }

    // Navigation property — reference to related entity
    public Customer Customer { get; set; } = null!;
}
```

#### Configuring Relationships with Fluent API
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // One-to-Many: Customer → Orders
    modelBuilder.Entity<Order>()
        .HasOne(o => o.Customer)           // Order has one Customer
        .WithMany(c => c.Orders)           // Customer has many Orders
        .HasForeignKey(o => o.CustomerId)  // FK column
        .OnDelete(DeleteBehavior.Cascade); // Delete orders when customer deleted
}
```

---

### 3.3 Code-First Approach Overview

In **Code-First**, you define C# classes → EF Core generates the database schema.

```
1. Write C# entity classes
        ↓
2. Configure with Data Annotations or Fluent API
        ↓
3. dotnet ef migrations add <MigrationName>
        ↓
4. dotnet ef database update
        ↓
5. Database created/updated ✅
```

**Data Annotations (inline configuration):**
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [NotMapped]  // Exclude from database
    public string DisplayLabel => $"{Name} (${Price})";
}
```

---

## 4. Performing Basic CRUD Operations

### Setup: Injecting DbContext
```csharp
public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }
}
```

---

### 4.1 Inserting Records — `AddAsync`

```csharp
// Insert a single record
public async Task<Product> CreateProductAsync(string name, decimal price)
{
    var product = new Product { Name = name, Price = price };

    await _context.Products.AddAsync(product);
    await _context.SaveChangesAsync();  // Executes INSERT

    return product;  // product.Id is now populated
}

// Insert multiple records at once
public async Task CreateProductsAsync(List<Product> products)
{
    await _context.Products.AddRangeAsync(products);
    await _context.SaveChangesAsync();
}
```

> 💡 `SaveChangesAsync()` is the commit step — nothing is written to the DB until you call it.

---

### 4.2 Retrieving Data

#### `FindAsync` — by Primary Key (uses cache first)
```csharp
var product = await _context.Products.FindAsync(1);
// Returns null if not found
```

#### `FirstOrDefaultAsync` — with a condition
```csharp
var product = await _context.Products
    .FirstOrDefaultAsync(p => p.Name == "Laptop");
// Returns first match or null
```

#### `ToListAsync` — retrieve multiple records
```csharp
// All products
var all = await _context.Products.ToListAsync();

// Filtered products
var expensive = await _context.Products
    .Where(p => p.Price > 500)
    .ToListAsync();

// With related data (eager loading)
var orders = await _context.Orders
    .Include(o => o.Customer)
    .Include(o => o.Items)
        .ThenInclude(i => i.Product)
    .ToListAsync();
```

#### `SingleOrDefaultAsync` — expects exactly one or zero
```csharp
var product = await _context.Products
    .SingleOrDefaultAsync(p => p.Id == 1);
// Throws if MORE than one match found
```

---

### 4.3 Updating Records and Tracking Changes

EF Core's **change tracker** automatically detects modifications to tracked entities.

```csharp
// Method 1: Update a tracked entity (automatic detection)
public async Task UpdatePriceAsync(int id, decimal newPrice)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null) return;

    product.Price = newPrice;  // Change tracker detects this
    await _context.SaveChangesAsync();  // Executes UPDATE
}

// Method 2: Attach and update a detached entity
public async Task UpdateProductAsync(Product updatedProduct)
{
    _context.Products.Update(updatedProduct);  // Marks entire entity as modified
    await _context.SaveChangesAsync();
}

// Method 3: ExecuteUpdate (bulk, no loading required — EF Core 7+)
public async Task DiscountAllAsync(decimal discount)
{
    await _context.Products
        .Where(p => p.Category == "Electronics")
        .ExecuteUpdateAsync(s =>
            s.SetProperty(p => p.Price, p => p.Price * (1 - discount)));
}
```

---

### 4.4 Deleting Records

```csharp
// Remove a single tracked entity
public async Task DeleteProductAsync(int id)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null) return;

    _context.Products.Remove(product);
    await _context.SaveChangesAsync();  // Executes DELETE
}

// RemoveRange — delete multiple entities
public async Task DeleteProductsAsync(List<Product> products)
{
    _context.Products.RemoveRange(products);
    await _context.SaveChangesAsync();
}

// ExecuteDelete — bulk delete without loading (EF Core 7+)
public async Task DeleteDiscontinuedAsync()
{
    await _context.Products
        .Where(p => !p.IsAvailable)
        .ExecuteDeleteAsync();
    // No SaveChangesAsync needed — executes immediately
}
```

| Method | Loads entity first? | Best for |
|---|---|---|
| `Remove` | Yes (tracked) | Single entity deletion |
| `RemoveRange` | Yes (tracked) | Multiple entities |
| `ExecuteDelete` | ❌ No | Bulk deletion, best performance |

---

## 5. LINQ Queries in EF Core 8

### 5.1 Where, Select, and OrderBy

```csharp
// WHERE — filter rows
var affordableProducts = await _context.Products
    .Where(p => p.Price < 100 && p.IsAvailable)
    .ToListAsync();

// SELECT — project specific columns
var names = await _context.Products
    .Select(p => p.Name)
    .ToListAsync();

// ORDER BY
var sorted = await _context.Products
    .OrderBy(p => p.Price)           // Ascending
    .ThenByDescending(p => p.Name)   // Secondary sort
    .ToListAsync();

// Combined
var result = await _context.Products
    .Where(p => p.Category == "Electronics")
    .OrderByDescending(p => p.Price)
    .Take(10)                         // TOP 10
    .Skip(0)                          // Pagination: skip 0
    .ToListAsync();
```

---

### 5.2 Projection into DTOs

Never expose entity classes directly to clients — project into **Data Transfer Objects (DTOs)**:

```csharp
// DTO definition
public record ProductDto(int Id, string Name, decimal Price, string Category);

// Projection query
var products = await _context.Products
    .Where(p => p.IsAvailable)
    .Select(p => new ProductDto(
        p.Id,
        p.Name,
        p.Price,
        p.Category
    ))
    .ToListAsync();

// Complex DTO with nested data
public record OrderSummaryDto(int OrderId, string CustomerName, decimal Total, int ItemCount);

var summaries = await _context.Orders
    .Select(o => new OrderSummaryDto(
        o.Id,
        o.Customer.Name,
        o.Total,
        o.Items.Count
    ))
    .ToListAsync();
```

> 💡 Projections with `Select` are **more efficient** than loading full entities — EF only queries the columns you need.

---

### 5.3 Filtering and Aggregating Data

```csharp
// COUNT
var totalProducts = await _context.Products.CountAsync();
var availableCount = await _context.Products.CountAsync(p => p.IsAvailable);

// SUM, AVG, MIN, MAX
var totalRevenue = await _context.Orders.SumAsync(o => o.Total);
var avgPrice = await _context.Products.AverageAsync(p => p.Price);
var cheapest = await _context.Products.MinAsync(p => p.Price);
var mostExpensive = await _context.Products.MaxAsync(p => p.Price);

// EXISTS
var hasPendingOrders = await _context.Orders.AnyAsync(o => o.Status == "Pending");

// GROUP BY
var salesByCategory = await _context.Products
    .GroupBy(p => p.Category)
    .Select(g => new
    {
        Category = g.Key,
        Count = g.Count(),
        AveragePrice = g.Average(p => p.Price)
    })
    .ToListAsync();
```

---

### 5.4 Asynchronous Queries with `ToListAsync()`

Always use **async versions** of EF Core methods to avoid blocking threads:

```csharp
// ✅ Async — non-blocking
var products = await _context.Products.ToListAsync();
var product  = await _context.Products.FirstOrDefaultAsync(p => p.Id == 1);
var count    = await _context.Products.CountAsync();
var exists   = await _context.Products.AnyAsync(p => p.Name == "Laptop");

// ❌ Sync — blocks the calling thread (avoid in web apps)
var products = _context.Products.ToList();

// Async with CancellationToken (for request cancellation)
public async Task<List<Product>> GetProductsAsync(CancellationToken ct)
{
    return await _context.Products
        .Where(p => p.IsAvailable)
        .ToListAsync(ct);
}
```

---

## 6. EF Core Migrations and Database Updates

### 6.1 Adding, Removing, and Updating Migrations

```bash
# Add a migration (snapshot of current model changes)
dotnet ef migrations add InitialCreate
dotnet ef migrations add AddProductCategory
dotnet ef migrations add RenameCustomerTable

# Apply all pending migrations to the database
dotnet ef database update

# Roll back to a specific migration
dotnet ef database update AddProductCategory

# Roll back ALL migrations (empty database)
dotnet ef database update 0

# Remove the last migration (only if NOT yet applied to DB)
dotnet ef migrations remove

# List all migrations and their status
dotnet ef migrations list
```

**What a migration file looks like:**
```csharp
// Migrations/20240101_AddProductCategory.cs
public partial class AddProductCategory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Applied when migrating forward
        migrationBuilder.AddColumn<string>(
            name: "Category",
            table: "Products",
            type: "nvarchar(100)",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Applied when rolling back
        migrationBuilder.DropColumn(name: "Category", table: "Products");
    }
}
```

---

### 6.2 Seeding Data during Migrations

```csharp
// In OnModelCreating — seed static/reference data
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Category>().HasData(
        new Category { Id = 1, Name = "Electronics" },
        new Category { Id = 2, Name = "Clothing" },
        new Category { Id = 3, Name = "Books" }
    );
}

// After adding seed, create a migration to capture it:
// dotnet ef migrations add SeedCategories
// dotnet ef database update
```

**Seeding in `Program.cs` (runtime seed — more flexible):**
```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync(); // Apply pending migrations

    if (!context.Products.Any())           // Only seed if empty
    {
        context.Products.AddRange(
            new Product { Name = "Laptop", Price = 999m },
            new Product { Name = "Mouse", Price = 29m }
        );
        await context.SaveChangesAsync();
    }
}
```

---

### 6.3 Managing Database Schema Changes

| Scenario | Approach |
|---|---|
| Add a new column | Add property to entity → `migrations add` → `database update` |
| Rename a column | Use `MigrationBuilder.RenameColumn` in migration |
| Change column type | Modify property + use `HasColumnType` in Fluent API |
| Add a new table | Add new entity + `DbSet` → `migrations add` |
| Drop a table | Remove entity + `DbSet` → `migrations add` |
| Production deployment | Use `migrations script` to generate SQL for DBA |

```bash
# Generate a SQL script for production (idempotent)
dotnet ef migrations script --idempotent -o deploy.sql
```

---

## 7. Handling Relationships and Data Loading

### 7.1 Eager, Lazy, and Explicit Loading

#### Eager Loading — load related data immediately with `Include`
```csharp
// Load Orders WITH their Customer and Items in one query
var orders = await _context.Orders
    .Include(o => o.Customer)
    .Include(o => o.Items)
        .ThenInclude(i => i.Product)    // Nested include
    .Where(o => o.Total > 100)
    .ToListAsync();
```

#### Lazy Loading — load related data on access (opt-in)
```bash
dotnet add package Microsoft.EntityFrameworkCore.Proxies
```

```csharp
// Enable in DbContext
options.UseLazyLoadingProxies().UseSqlServer(connectionString);

// Mark navigation properties as virtual
public class Order
{
    public virtual Customer Customer { get; set; } = null!;  // Loaded on access
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

// Access triggers a DB query automatically
var order = await _context.Orders.FindAsync(1);
var name = order.Customer.Name;   // ← DB query fires here (N+1 risk!)
```

> ⚠️ Lazy loading can cause the **N+1 query problem** — use eager loading for predictable performance.

#### Explicit Loading — manually load related data when needed
```csharp
var order = await _context.Orders.FindAsync(1);

// Explicitly load when needed
await _context.Entry(order)
    .Reference(o => o.Customer)
    .LoadAsync();

await _context.Entry(order)
    .Collection(o => o.Items)
    .LoadAsync();

Console.WriteLine(order.Customer.Name);  // Now available
```

---

### 7.2 Configuring Relationships

#### One-to-One
```csharp
public class Customer
{
    public int Id { get; set; }
    public CustomerProfile? Profile { get; set; }
}

public class CustomerProfile
{
    public int Id { get; set; }
    public string Bio { get; set; } = "";
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}

// Fluent API
modelBuilder.Entity<Customer>()
    .HasOne(c => c.Profile)
    .WithOne(p => p.Customer)
    .HasForeignKey<CustomerProfile>(p => p.CustomerId);
```

#### One-to-Many
```csharp
// Convention is usually enough — but explicit for clarity:
modelBuilder.Entity<Order>()
    .HasOne(o => o.Customer)
    .WithMany(c => c.Orders)
    .HasForeignKey(o => o.CustomerId)
    .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete
```

#### Many-to-Many (EF Core 5+ — automatic join table)
```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public ICollection<Student> Students { get; set; } = new List<Student>();
}

// EF Core creates "CourseStudent" join table automatically
// Fluent API (optional — to customize join table name)
modelBuilder.Entity<Student>()
    .HasMany(s => s.Courses)
    .WithMany(c => c.Students)
    .UsingEntity(j => j.ToTable("Enrollments"));
```

---

### 7.3 Navigating Circular References

Circular references occur when `Order → Customer → Orders → Order → ...` causing infinite loops in serialization.

**Solution 1: Use DTOs (recommended)**
```csharp
// Never serialize entities directly — map to DTOs
var orderDto = new OrderDto
{
    Id = order.Id,
    CustomerName = order.Customer.Name,  // Only what you need
    Total = order.Total
};
```

**Solution 2: Configure JSON serializer to handle cycles**
```csharp
// Program.cs
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
```

**Solution 3: `[JsonIgnore]` on navigation properties**
```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    [JsonIgnore]  // Break the cycle
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

---

## 8. Performance Optimizations and Best Practices

### 8.1 Query Caching and `AsNoTracking`

By default, EF Core **tracks all queried entities** (watches for changes). For read-only queries, disable tracking:

```csharp
// ❌ Tracked (default) — overhead for read-only data
var products = await _context.Products.ToListAsync();

// ✅ No tracking — faster, less memory for read-only queries
var products = await _context.Products
    .AsNoTracking()
    .ToListAsync();

// Set no-tracking as default for the whole context (read-only context)
builder.Services.AddDbContext<ReadOnlyDbContext>(options =>
    options.UseSqlServer(connectionString)
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

// AsNoTrackingWithIdentityResolution — prevents duplicate objects in graph
var orders = await _context.Orders
    .AsNoTrackingWithIdentityResolution()
    .Include(o => o.Customer)
    .ToListAsync();
```

**When to use:**
| Scenario | Tracking |
|---|---|
| Reading data for display | `AsNoTracking` ✅ |
| Updating/deleting entities | Default tracking ✅ |
| Dashboard/report queries | `AsNoTracking` ✅ |
| Single entity CRUD | Default tracking ✅ |

---

### 8.2 Batch Processing and Bulk Operations

```csharp
// ❌ N+1 updates — one DB call per product (slow)
foreach (var product in products)
{
    product.Price *= 0.9m;
    await _context.SaveChangesAsync();
}

// ✅ Single SaveChanges for all changes
foreach (var product in products)
    product.Price *= 0.9m;
await _context.SaveChangesAsync();  // One DB round-trip

// ✅ ExecuteUpdate — best performance, no entity loading
await _context.Products
    .Where(p => p.Category == "Electronics")
    .ExecuteUpdateAsync(s =>
        s.SetProperty(p => p.Price, p => p.Price * 0.9m));

// Chunk large datasets to avoid memory issues
var skip = 0;
const int batchSize = 500;
while (true)
{
    var batch = await _context.Products
        .Skip(skip).Take(batchSize).ToListAsync();
    if (!batch.Any()) break;

    foreach (var p in batch) p.IsAvailable = true;
    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();  // Free memory between batches
    skip += batchSize;
}
```

---

### 8.3 Handling Concurrency with RowVersion Columns

Prevent two users from overwriting each other's changes (**optimistic concurrency**):

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }

    [Timestamp]  // EF automatically adds WHERE RowVersion = @original in UPDATE
    public byte[] RowVersion { get; set; } = null!;
}

// Fluent API alternative
modelBuilder.Entity<Product>()
    .Property(p => p.RowVersion)
    .IsRowVersion();

// Handling concurrency conflict
public async Task UpdatePriceAsync(int id, decimal newPrice, byte[] rowVersion)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null) return;

    product.Price = newPrice;
    _context.Entry(product).Property(p => p.RowVersion).OriginalValue = rowVersion;

    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException ex)
    {
        var entry = ex.Entries.Single();
        var dbValues = await entry.GetDatabaseValuesAsync();
        // Handle conflict: show user the current DB value and let them decide
        throw new Exception($"Data was modified by another user. Current price: {dbValues!["Price"]}");
    }
}
```

---

### 8.4 Using Compiled Queries for Performance

For **frequently-executed queries**, pre-compile them to skip expression tree translation on every call:

```csharp
// Define compiled queries as static fields
public class ProductQueries
{
    // Compiled once, reused many times
    private static readonly Func<AppDbContext, int, Task<Product?>> GetByIdAsync
        = EF.CompileAsyncQuery((AppDbContext ctx, int id) =>
            ctx.Products.FirstOrDefault(p => p.Id == id));

    private static readonly Func<AppDbContext, string, IAsyncEnumerable<Product>> GetByCategoryAsync
        = EF.CompileAsyncQuery((AppDbContext ctx, string category) =>
            ctx.Products.Where(p => p.Category == category).AsNoTracking());

    // Use in service
    public async Task<Product?> FindProductAsync(AppDbContext context, int id)
    {
        return await GetByIdAsync(context, id);
    }

    public async Task<List<Product>> GetByCategoryAsync(AppDbContext context, string category)
    {
        var results = new List<Product>();
        await foreach (var p in GetByCategoryAsync(context, category))
            results.Add(p);
        return results;
    }
}
```

> 💡 Compiled queries are most beneficial for **hot paths** — queries called hundreds/thousands of times per second.

---

### Quick Reference: Best Practices Summary

| Practice | Rule |
|---|---|
| **Async always** | Use `ToListAsync`, `SaveChangesAsync`, `FindAsync` |
| **AsNoTracking** | Use for all read-only queries |
| **DTOs for output** | Never expose entity classes to API consumers |
| **ExecuteUpdate/Delete** | Use for bulk operations — no entity loading |
| **Include selectively** | Only `Include` what you actually need |
| **Avoid lazy loading** | Prefer eager loading to prevent N+1 queries |
| **Batch large saves** | Don't call `SaveChangesAsync` in a loop |
| **Compiled queries** | Use for high-frequency query paths |
| **RowVersion** | Add to entities shared by concurrent users |
| **Pagination** | Always use `Skip/Take` — never load entire tables |

---

*End of Entity Framework Core 8.0 Complete Reference Guide*
