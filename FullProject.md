# Full Project Code — ASP.NET Core Web API v10 + Angular 21 + MS SQL Server

---

# PART A — ASP.NET Core Web API

---

## `MyApp.API/Entities/User.cs`

```csharp
namespace MyApp.API.Entities;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public List<Order> Orders { get; set; } = new();
}
```

---

## `MyApp.API/Entities/Product.cs`

```csharp
namespace MyApp.API.Entities;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();
}
```

---

## `MyApp.API/Entities/Order.cs`

```csharp
namespace MyApp.API.Entities;

public class Order
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }

    public User User { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
}
```

---

## `MyApp.API/Entities/OrderItem.cs`

```csharp
namespace MyApp.API.Entities;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
```

---

## `MyApp.API/Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using MyApp.API.Entities;

namespace MyApp.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Users
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.UserId);
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).IsRequired().HasDefaultValue("Customer");
            e.HasCheckConstraint("CHK_Users_Role", "Role IN ('Customer','Owner')");
            e.Property(u => u.RefreshToken).HasMaxLength(500);
        });

        // Products
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.ProductId);
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(p => p.Name).IsUnique();
            e.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
            e.HasCheckConstraint("CHK_Products_Price", "Price >= 0");
            e.Property(p => p.Stock).IsRequired().HasDefaultValue(0);
            e.HasCheckConstraint("CHK_Products_Stock", "Stock >= 0");
        });

        // Orders
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.OrderId);
            e.Property(o => o.OrderDate).HasDefaultValueSql("GETUTCDATE()");
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.HasCheckConstraint("CHK_Orders_Amount", "TotalAmount >= 0");
            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // OrderItems
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(oi => oi.OrderItemId);
            e.Property(oi => oi.Quantity).IsRequired();
            e.HasCheckConstraint("CHK_OI_Quantity", "Quantity > 0");
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            e.HasCheckConstraint("CHK_OI_UnitPrice", "UnitPrice >= 0");
            e.HasIndex(oi => new { oi.OrderId, oi.ProductId }).IsUnique();
            e.HasOne(oi => oi.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(oi => oi.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(oi => oi.ProductId)
             .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
```

---

## `MyApp.API/Interfaces/Repositories/IRepository.cs`

```csharp
namespace MyApp.API.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> SaveChangesAsync();
}
```

---

## `MyApp.API/Interfaces/Repositories/IUserRepository.cs`

```csharp
using MyApp.API.Entities;

namespace MyApp.API.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string token);
}
```

---

## `MyApp.API/Interfaces/Repositories/IProductRepository.cs`

```csharp
using MyApp.API.Entities;

namespace MyApp.API.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
}
```

---

## `MyApp.API/Interfaces/Repositories/IOrderRepository.cs`

```csharp
using MyApp.API.Entities;

namespace MyApp.API.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Order?> GetWithItemsAsync(int orderId);
}
```

---

## `MyApp.API/Interfaces/Services/IAuthService.cs`

```csharp
using MyApp.API.DTOs.Auth;

namespace MyApp.API.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}
```

---

## `MyApp.API/Interfaces/Services/ITokenService.cs`

```csharp
using System.Security.Claims;
using MyApp.API.Entities;

namespace MyApp.API.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipal(string token);
}
```

---

## `MyApp.API/Interfaces/Services/IProductService.cs`

```csharp
using MyApp.API.DTOs.Products;

namespace MyApp.API.Interfaces.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(ProductDto dto);
    Task<bool> UpdateAsync(int id, ProductDto dto);
    Task<bool> DeleteAsync(int id);
}
```

---

## `MyApp.API/Interfaces/Services/IOrderService.cs`

```csharp
using MyApp.API.DTOs.Orders;
using MyApp.API.Entities;

namespace MyApp.API.Interfaces.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetMyOrdersAsync(int userId);
    Task<Order?> GetByIdAsync(int orderId, int userId);
    Task<Order> CreateAsync(int userId, CreateOrderDto dto);
}
```

---

## `MyApp.API/DTOs/Auth/LoginDto.cs`

```csharp
namespace MyApp.API.DTOs.Auth;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

---

## `MyApp.API/DTOs/Auth/RegisterDto.cs`

```csharp
namespace MyApp.API.DTOs.Auth;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
}
```

---

## `MyApp.API/DTOs/Auth/AuthResponseDto.cs`

```csharp
namespace MyApp.API.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Role { get; set; } = string.Empty;
}
```

---

## `MyApp.API/DTOs/Products/ProductDto.cs`

```csharp
namespace MyApp.API.DTOs.Products;

public class ProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
```

---

## `MyApp.API/DTOs/Orders/CreateOrderDto.cs`

```csharp
namespace MyApp.API.DTOs.Orders;

public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; } = new();
}
```

---

## `MyApp.API/DTOs/Orders/OrderItemDto.cs`

```csharp
namespace MyApp.API.DTOs.Orders;

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
```

---

## `MyApp.API/Repositories/UserRepository.cs` — LINQ-based (v2)

```csharp
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;

namespace MyApp.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
        => await _context.Users.ToListAsync();

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByRefreshTokenAsync(string token)
        => await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == token);

    public async Task AddAsync(User entity)
        => await _context.Users.AddAsync(entity);

    public void Update(User entity)
        => _context.Users.Update(entity);

    public void Delete(User entity)
        => _context.Users.Remove(entity);

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
```

---

## `MyApp.API/Repositories/UserRepositorySp.cs` — Stored Procedure-based

```csharp
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;

namespace MyApp.API.Repositories;

public class UserRepositorySp : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepositorySp(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
        => await _context.Users.FromSqlRaw("EXEC sp_GetAllUsers").ToListAsync();

    public async Task<User?> GetByIdAsync(int id)
    {
        var param = new SqlParameter("@UserId", id);
        return await _context.Users
            .FromSqlRaw("EXEC sp_GetUserById @UserId", param)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var param = new SqlParameter("@Email", email);
        return await _context.Users
            .FromSqlRaw("EXEC sp_GetUserByEmail @Email", param)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByRefreshTokenAsync(string token)
    {
        var param = new SqlParameter("@Token", token);
        return await _context.Users
            .FromSqlRaw("EXEC sp_GetUserByRefreshToken @Token", param)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(User entity)
    {
        var p1 = new SqlParameter("@Email", entity.Email);
        var p2 = new SqlParameter("@PasswordHash", entity.PasswordHash);
        var p3 = new SqlParameter("@Role", entity.Role);
        await _context.Database.ExecuteSqlRawAsync("EXEC sp_InsertUser @Email, @PasswordHash, @Role", p1, p2, p3);
    }

    public void Update(User entity)
    {
        var p1 = new SqlParameter("@UserId", entity.UserId);
        var p2 = new SqlParameter("@RefreshToken", (object?)entity.RefreshToken ?? DBNull.Value);
        var p3 = new SqlParameter("@RefreshTokenExpiresAt", (object?)entity.RefreshTokenExpiresAt ?? DBNull.Value);
        _context.Database.ExecuteSqlRaw("EXEC sp_UpdateUserRefreshToken @UserId, @RefreshToken, @RefreshTokenExpiresAt", p1, p2, p3);
    }

    public void Delete(User entity)
    {
        var param = new SqlParameter("@UserId", entity.UserId);
        _context.Database.ExecuteSqlRaw("EXEC sp_DeleteUser @UserId", param);
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() >= 0;
}
```

---

## `MyApp.API/Repositories/ProductRepository.cs` — LINQ-based (v2)

```csharp
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;

namespace MyApp.API.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await _context.Products.ToListAsync();

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

    public async Task AddAsync(Product entity)
        => await _context.Products.AddAsync(entity);

    public void Update(Product entity)
        => _context.Products.Update(entity);

    public void Delete(Product entity)
        => _context.Products.Remove(entity);

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
```

---

## `MyApp.API/Repositories/ProductRepositorySp.cs` — Stored Procedure-based

```csharp
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;

namespace MyApp.API.Repositories;

public class ProductRepositorySp : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepositorySp(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await _context.Products.FromSqlRaw("EXEC sp_GetAllProducts").ToListAsync();

    public async Task<Product?> GetByIdAsync(int id)
    {
        var param = new SqlParameter("@ProductId", id);
        return await _context.Products
            .FromSqlRaw("EXEC sp_GetProductById @ProductId", param)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Product entity)
    {
        var p1 = new SqlParameter("@Name", entity.Name);
        var p2 = new SqlParameter("@Price", entity.Price);
        var p3 = new SqlParameter("@Stock", entity.Stock);
        await _context.Database.ExecuteSqlRawAsync("EXEC sp_InsertProduct @Name, @Price, @Stock", p1, p2, p3);
    }

    public void Update(Product entity)
    {
        var p1 = new SqlParameter("@ProductId", entity.ProductId);
        var p2 = new SqlParameter("@Name", entity.Name);
        var p3 = new SqlParameter("@Price", entity.Price);
        var p4 = new SqlParameter("@Stock", entity.Stock);
        _context.Database.ExecuteSqlRaw("EXEC sp_UpdateProduct @ProductId, @Name, @Price, @Stock", p1, p2, p3, p4);
    }

    public void Delete(Product entity)
    {
        var param = new SqlParameter("@ProductId", entity.ProductId);
        _context.Database.ExecuteSqlRaw("EXEC sp_DeleteProduct @ProductId", param);
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() >= 0;
}
```

---

## `MyApp.API/Repositories/OrderRepository.cs` — LINQ-based (v2)

```csharp
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;

namespace MyApp.API.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
        => await _context.Orders.Include(o => o.Items).ToListAsync();

    public async Task<Order?> GetByIdAsync(int id)
        => await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == id);

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        => await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ToListAsync();

    public async Task<Order?> GetWithItemsAsync(int orderId)
        => await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task AddAsync(Order entity)
        => await _context.Orders.AddAsync(entity);

    public void Update(Order entity)
        => _context.Orders.Update(entity);

    public void Delete(Order entity)
        => _context.Orders.Remove(entity);

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
```

---

## `MyApp.API/Repositories/OrderRepositorySp.cs` — Stored Procedure-based

```csharp
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;

namespace MyApp.API.Repositories;

public class OrderRepositorySp : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepositorySp(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
        => await _context.Orders.FromSqlRaw("EXEC sp_GetAllOrders").ToListAsync();

    public async Task<Order?> GetByIdAsync(int id)
    {
        var param = new SqlParameter("@OrderId", id);
        return await _context.Orders
            .FromSqlRaw("EXEC sp_GetOrderById @OrderId", param)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        var param = new SqlParameter("@UserId", userId);
        return await _context.Orders
            .FromSqlRaw("EXEC sp_GetOrdersByUserId @UserId", param)
            .ToListAsync();
    }

    public async Task<Order?> GetWithItemsAsync(int orderId)
    {
        var param = new SqlParameter("@OrderId", orderId);
        return await _context.Orders
            .FromSqlRaw("EXEC sp_GetOrderWithItems @OrderId", param)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Order entity)
    {
        var p1 = new SqlParameter("@UserId", entity.UserId);
        var p2 = new SqlParameter("@TotalAmount", entity.TotalAmount);
        await _context.Database.ExecuteSqlRawAsync("EXEC sp_InsertOrder @UserId, @TotalAmount", p1, p2);
    }

    public void Update(Order entity)
    {
        var p1 = new SqlParameter("@OrderId", entity.OrderId);
        var p2 = new SqlParameter("@TotalAmount", entity.TotalAmount);
        _context.Database.ExecuteSqlRaw("EXEC sp_UpdateOrder @OrderId, @TotalAmount", p1, p2);
    }

    public void Delete(Order entity)
    {
        var param = new SqlParameter("@OrderId", entity.OrderId);
        _context.Database.ExecuteSqlRaw("EXEC sp_DeleteOrder @OrderId", param);
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() >= 0;
}
```

---

## `MyApp.API/Services/JwtTokenService.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Services;

namespace MyApp.API.Services;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role)
        };

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal GetPrincipal(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var validation = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = key,
            ValidateIssuer           = false,
            ValidateAudience         = false,
            ValidateLifetime         = false
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
    }
}
```

---

## `MyApp.API/Services/AuthService.cs`

```csharp
using Microsoft.AspNetCore.Identity;
using MyApp.API.DTOs.Auth;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;
using MyApp.API.Interfaces.Services;

namespace MyApp.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly ITokenService _tokenSvc;
    private readonly IPasswordHasher<User> _hasher;

    public AuthService(IUserRepository userRepo, ITokenService tokenSvc, IPasswordHasher<User> hasher)
    {
        _userRepo = userRepo;
        _tokenSvc = tokenSvc;
        _hasher   = hasher;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userRepo.GetByEmailAsync(dto.Email);
        if (existing is not null)
            throw new InvalidOperationException("Email already in use.");

        var user = new User
        {
            Email = dto.Email,
            Role  = dto.Role
        };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        var refreshToken = _tokenSvc.GenerateRefreshToken();
        user.RefreshToken          = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken  = _tokenSvc.GenerateAccessToken(user),
            RefreshToken = refreshToken,
            ExpiresAt    = user.RefreshTokenExpiresAt.Value,
            Role         = user.Role
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var refreshToken = _tokenSvc.GenerateRefreshToken();
        user.RefreshToken          = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken  = _tokenSvc.GenerateAccessToken(user),
            RefreshToken = refreshToken,
            ExpiresAt    = user.RefreshTokenExpiresAt.Value,
            Role         = user.Role
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepo.GetByRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired.");

        var newRefresh = _tokenSvc.GenerateRefreshToken();
        user.RefreshToken          = newRefresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken  = _tokenSvc.GenerateAccessToken(user),
            RefreshToken = newRefresh,
            ExpiresAt    = user.RefreshTokenExpiresAt.Value,
            Role         = user.Role
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var user = await _userRepo.GetByRefreshTokenAsync(refreshToken);
        if (user is null) return false;

        user.RefreshToken          = null;
        user.RefreshTokenExpiresAt = null;
        _userRepo.Update(user);
        return await _userRepo.SaveChangesAsync();
    }
}
```

---

## `MyApp.API/Services/ProductService.cs`

```csharp
using MyApp.API.DTOs.Products;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;
using MyApp.API.Interfaces.Services;

namespace MyApp.API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _repo.GetAllAsync();
        return products.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            Name      = p.Name,
            Price     = p.Price,
            Stock     = p.Stock
        });
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p is null) return null;
        return new ProductDto { ProductId = p.ProductId, Name = p.Name, Price = p.Price, Stock = p.Stock };
    }

    public async Task<ProductDto> CreateAsync(ProductDto dto)
    {
        var product = new Product { Name = dto.Name, Price = dto.Price, Stock = dto.Stock };
        await _repo.AddAsync(product);
        await _repo.SaveChangesAsync();
        dto.ProductId = product.ProductId;
        return dto;
    }

    public async Task<bool> UpdateAsync(int id, ProductDto dto)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product is null) return false;
        product.Name  = dto.Name;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        _repo.Update(product);
        return await _repo.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product is null) return false;
        _repo.Delete(product);
        return await _repo.SaveChangesAsync();
    }
}
```

---

## `MyApp.API/Services/OrderService.cs`

```csharp
using MyApp.API.DTOs.Orders;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;
using MyApp.API.Interfaces.Services;

namespace MyApp.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;

    public OrderService(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepo   = orderRepo;
        _productRepo = productRepo;
    }

    public async Task<IEnumerable<Order>> GetMyOrdersAsync(int userId)
        => await _orderRepo.GetByUserIdAsync(userId);

    public async Task<Order?> GetByIdAsync(int orderId, int userId)
    {
        var order = await _orderRepo.GetWithItemsAsync(orderId);
        if (order is null || order.UserId != userId) return null;
        return order;
    }

    public async Task<Order> CreateAsync(int userId, CreateOrderDto dto)
    {
        var order = new Order
        {
            UserId    = userId,
            OrderDate = DateTime.UtcNow,
            Items     = new List<OrderItem>()
        };

        decimal total = 0;
        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepo.GetByIdAsync(itemDto.ProductId)
                ?? throw new InvalidOperationException($"Product {itemDto.ProductId} not found.");

            if (product.Stock < itemDto.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}.");

            product.Stock -= itemDto.Quantity;
            _productRepo.Update(product);

            var item = new OrderItem
            {
                ProductId = product.ProductId,
                Quantity  = itemDto.Quantity,
                UnitPrice = product.Price
            };
            order.Items.Add(item);
            total += item.Quantity * item.UnitPrice;
        }

        order.TotalAmount = total;
        await _orderRepo.AddAsync(order);
        await _orderRepo.SaveChangesAsync();
        return order;
    }
}
```

---

## `MyApp.API/Controllers/AuthController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.API.DTOs.Auth;
using MyApp.API.Interfaces.Services;

namespace MyApp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string token)
    {
        var result = await _authService.RefreshTokenAsync(token);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var result = await _authService.RevokeTokenAsync(token);
        return Ok(result);
    }
}
```

---

## `MyApp.API/Controllers/ProductsController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.API.DTOs.Products;
using MyApp.API.Interfaces.Services;

namespace MyApp.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Update(int id, ProductDto dto)
    {
        var result = await _productService.UpdateAsync(id, dto);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
```

---

## `MyApp.API/Controllers/OrdersController.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.API.DTOs.Orders;
using MyApp.API.Interfaces.Services;

namespace MyApp.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var orders = await _orderService.GetMyOrdersAsync(GetUserId());
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id, GetUserId());
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        var order = await _orderService.CreateAsync(GetUserId(), dto);
        return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
    }
}
```

---

## `MyApp.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "super_secret_key_change_in_production_32chars!!",
    "Issuer": "MyApp.API",
    "Audience": "MyApp.Client"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## `MyApp.API/Program.cs`

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApp.API.Data;
using MyApp.API.Entities;
using MyApp.API.Interfaces.Repositories;
using MyApp.API.Interfaces.Services;
using MyApp.API.Repositories;
using MyApp.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

---

# PART B — Stored Procedure SQL Scripts

---

## `SQL/StoredProcedures.sql`

```sql
-- ============================================================
-- USER STORED PROCEDURES
-- ============================================================

CREATE OR ALTER PROCEDURE sp_GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, Email, PasswordHash, Role, RefreshToken, RefreshTokenExpiresAt
    FROM Users;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, Email, PasswordHash, Role, RefreshToken, RefreshTokenExpiresAt
    FROM Users
    WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetUserByEmail
    @Email NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, Email, PasswordHash, Role, RefreshToken, RefreshTokenExpiresAt
    FROM Users
    WHERE Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetUserByRefreshToken
    @Token NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, Email, PasswordHash, Role, RefreshToken, RefreshTokenExpiresAt
    FROM Users
    WHERE RefreshToken = @Token;
END;
GO

CREATE OR ALTER PROCEDURE sp_InsertUser
    @Email        NVARCHAR(200),
    @PasswordHash NVARCHAR(500),
    @Role         NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Users (Email, PasswordHash, Role)
    VALUES (@Email, @PasswordHash, @Role);
END;
GO

CREATE OR ALTER PROCEDURE sp_UpdateUserRefreshToken
    @UserId               INT,
    @RefreshToken         NVARCHAR(500),
    @RefreshTokenExpiresAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users
    SET RefreshToken          = @RefreshToken,
        RefreshTokenExpiresAt = @RefreshTokenExpiresAt
    WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE sp_DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Users WHERE UserId = @UserId;
END;
GO

-- ============================================================
-- PRODUCT STORED PROCEDURES
-- ============================================================

CREATE OR ALTER PROCEDURE sp_GetAllProducts
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProductId, Name, Price, Stock FROM Products;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetProductById
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProductId, Name, Price, Stock
    FROM Products
    WHERE ProductId = @ProductId;
END;
GO

CREATE OR ALTER PROCEDURE sp_InsertProduct
    @Name  NVARCHAR(200),
    @Price DECIMAL(18,2),
    @Stock INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Products (Name, Price, Stock)
    VALUES (@Name, @Price, @Stock);
END;
GO

CREATE OR ALTER PROCEDURE sp_UpdateProduct
    @ProductId INT,
    @Name      NVARCHAR(200),
    @Price     DECIMAL(18,2),
    @Stock     INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Products
    SET Name  = @Name,
        Price = @Price,
        Stock = @Stock
    WHERE ProductId = @ProductId;
END;
GO

CREATE OR ALTER PROCEDURE sp_DeleteProduct
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Products WHERE ProductId = @ProductId;
END;
GO

-- ============================================================
-- ORDER STORED PROCEDURES
-- ============================================================

CREATE OR ALTER PROCEDURE sp_GetAllOrders
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrderId, UserId, OrderDate, TotalAmount FROM Orders;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetOrderById
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrderId, UserId, OrderDate, TotalAmount
    FROM Orders
    WHERE OrderId = @OrderId;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetOrdersByUserId
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrderId, UserId, OrderDate, TotalAmount
    FROM Orders
    WHERE UserId = @UserId
    ORDER BY OrderDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetOrderWithItems
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.OrderId, o.UserId, o.OrderDate, o.TotalAmount
    FROM Orders o
    WHERE o.OrderId = @OrderId;

    SELECT oi.OrderItemId, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice,
           p.Name AS ProductName
    FROM OrderItems oi
    INNER JOIN Products p ON oi.ProductId = p.ProductId
    WHERE oi.OrderId = @OrderId;
END;
GO

CREATE OR ALTER PROCEDURE sp_InsertOrder
    @UserId      INT,
    @TotalAmount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Orders (UserId, TotalAmount)
    VALUES (@UserId, @TotalAmount);
    SELECT SCOPE_IDENTITY() AS OrderId;
END;
GO

CREATE OR ALTER PROCEDURE sp_UpdateOrder
    @OrderId     INT,
    @TotalAmount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Orders
    SET TotalAmount = @TotalAmount
    WHERE OrderId = @OrderId;
END;
GO

CREATE OR ALTER PROCEDURE sp_DeleteOrder
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Orders WHERE OrderId = @OrderId;
END;
GO
```

---

---

# PART C — Angular 21

---

## `src/app/models/user.model.ts`

```typescript
export interface User {
  userId: number;
  email: string;
  role: string;
}
```

---

## `src/app/models/product.model.ts`

```typescript
export interface ProductDto {
  productId: number;
  name: string;
  price: number;
  stock: number;
}
```

---

## `src/app/models/order.model.ts`

```typescript
export interface OrderItemDto {
  productId: number;
  quantity: number;
}

export interface CreateOrderDto {
  items: OrderItemDto[];
}

export interface OrderItem {
  orderItemId: number;
  productId: number;
  quantity: number;
  unitPrice: number;
}

export interface Order {
  orderId: number;
  userId: number;
  orderDate: string;
  totalAmount: number;
  items: OrderItem[];
}
```

---

## `src/app/core/services/auth.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginDto    { email: string; password: string; }
export interface RegisterDto { email: string; password: string; role: string; }
export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = 'http://localhost:5000/api/auth';
  private token = '';
  private role  = '';

  constructor(private http: HttpClient) {
    this.token = sessionStorage.getItem('token') ?? '';
    this.role  = sessionStorage.getItem('role')  ?? '';
  }

  login(dto: LoginDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/login`, dto).pipe(
      tap(res => this.persist(res))
    );
  }

  register(dto: RegisterDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/register`, dto).pipe(
      tap(res => this.persist(res))
    );
  }

  logout(): void {
    sessionStorage.clear();
    this.token = '';
    this.role  = '';
  }

  isLoggedIn(): boolean  { return !!this.token; }
  getToken(): string     { return this.token; }
  getRole(): string      { return this.role; }

  private persist(res: AuthResponseDto): void {
    this.token = res.accessToken;
    this.role  = res.role;
    sessionStorage.setItem('token', res.accessToken);
    sessionStorage.setItem('role',  res.role);
  }
}
```

---

## `src/app/core/services/product.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProductDto } from '../../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly baseUrl = 'http://localhost:5000/api/products';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ProductDto[]> {
    return this.http.get<ProductDto[]>(this.baseUrl);
  }

  getById(id: number): Observable<ProductDto> {
    return this.http.get<ProductDto>(`${this.baseUrl}/${id}`);
  }

  create(dto: ProductDto): Observable<ProductDto> {
    return this.http.post<ProductDto>(this.baseUrl, dto);
  }

  update(id: number, dto: ProductDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
```

---

## `src/app/core/services/order.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order, CreateOrderDto } from '../../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly baseUrl = 'http://localhost:5000/api/orders';

  constructor(private http: HttpClient) {}

  getMyOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.baseUrl);
  }

  create(dto: CreateOrderDto): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, dto);
  }
}
```

---

## `src/app/core/interceptors/auth.interceptor.ts`

```typescript
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth  = inject(AuthService);
  const token = auth.getToken();

  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  return next(req);
};
```

---

## `src/app/core/guards/auth.guard.ts`

```typescript
import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  return auth.isLoggedIn() ? true : router.createUrlTree(['/auth/login']);
};
```

---

## `src/app/core/guards/owner.guard.ts`

```typescript
import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const ownerGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  return auth.getRole() === 'Owner' ? true : router.createUrlTree(['/403']);
};
```

---

## `src/app/features/auth/login/login.component.ts`

```typescript
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div>
      <h2>Login</h2>
      <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
        <div>
          <label>Email</label>
          <input type="email" formControlName="email" />
        </div>
        <div>
          <label>Password</label>
          <input type="password" formControlName="password" />
        </div>
        <button type="submit" [disabled]="loginForm.invalid">Login</button>
        <p *ngIf="error">{{ error }}</p>
      </form>
    </div>
  `
})
export class LoginComponent {
  loginForm: FormGroup;
  error = '';

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.loginForm = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) return;
    this.auth.login(this.loginForm.value).subscribe({
      next: () => this.router.navigate(['/products']),
      error: () => { this.error = 'Invalid credentials.'; }
    });
  }
}
```

---

## `src/app/features/auth/register/register.component.ts`

```typescript
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div>
      <h2>Register</h2>
      <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
        <div>
          <label>Email</label>
          <input type="email" formControlName="email" />
        </div>
        <div>
          <label>Password</label>
          <input type="password" formControlName="password" />
        </div>
        <div>
          <label>Role</label>
          <select formControlName="role">
            <option value="Customer">Customer</option>
            <option value="Owner">Owner</option>
          </select>
        </div>
        <button type="submit" [disabled]="registerForm.invalid">Register</button>
        <p *ngIf="error">{{ error }}</p>
      </form>
    </div>
  `
})
export class RegisterComponent {
  registerForm: FormGroup;
  error = '';

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.registerForm = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      role:     ['Customer', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;
    this.auth.register(this.registerForm.value).subscribe({
      next: () => this.router.navigate(['/products']),
      error: () => { this.error = 'Registration failed.'; }
    });
  }
}
```

---

## `src/app/features/auth/auth.routes.ts`

```typescript
import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./register/register.component').then(m => m.RegisterComponent)
  }
];
```

---

## `src/app/features/products/product-list/product-list.component.ts`

```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { ProductDto } from '../../../models/product.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div>
      <h2>Products</h2>
      <a routerLink="/products/create">Add Product</a>
      <ul>
        <li *ngFor="let p of products">
          {{ p.name }} — \${{ p.price }} (Stock: {{ p.stock }})
          <button (click)="delete(p.productId)">Delete</button>
        </li>
      </ul>
    </div>
  `
})
export class ProductListComponent implements OnInit {
  products: ProductDto[] = [];

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.productService.getAll().subscribe(data => this.products = data);
  }

  delete(id: number): void {
    this.productService.delete(id).subscribe(() => {
      this.products = this.products.filter(p => p.productId !== id);
    });
  }
}
```

---

## `src/app/features/products/product-form/product-form.component.ts`

```typescript
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div>
      <h2>Create Product</h2>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div>
          <label>Name</label>
          <input formControlName="name" />
        </div>
        <div>
          <label>Price</label>
          <input type="number" formControlName="price" />
        </div>
        <div>
          <label>Stock</label>
          <input type="number" formControlName="stock" />
        </div>
        <button type="submit" [disabled]="form.invalid">Save</button>
      </form>
    </div>
  `
})
export class ProductFormComponent {
  form: FormGroup;

  constructor(private fb: FormBuilder, private productService: ProductService, private router: Router) {
    this.form = this.fb.group({
      name:  ['', Validators.required],
      price: [0, [Validators.required, Validators.min(0)]],
      stock: [0, [Validators.required, Validators.min(0)]]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.productService.create({ ...this.form.value, productId: 0 }).subscribe(() => {
      this.router.navigate(['/products']);
    });
  }
}
```

---

## `src/app/features/products/products.routes.ts`

```typescript
import { Routes } from '@angular/router';
import { ownerGuard } from '../../core/guards/owner.guard';

export const PRODUCT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./product-list/product-list.component').then(m => m.ProductListComponent)
  },
  {
    path: 'create',
    loadComponent: () => import('./product-form/product-form.component').then(m => m.ProductFormComponent),
    canActivate: [ownerGuard]
  }
];
```

---

## `src/app/features/orders/order-list/order-list.component.ts`

```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../models/order.model';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div>
      <h2>My Orders</h2>
      <ul>
        <li *ngFor="let o of orders">
          Order #{{ o.orderId }} — {{ o.orderDate | date }} — \${{ o.totalAmount }}
          <ul>
            <li *ngFor="let item of o.items">
              Product #{{ item.productId }} x{{ item.quantity }} @ \${{ item.unitPrice }}
            </li>
          </ul>
        </li>
      </ul>
    </div>
  `
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.orderService.getMyOrders().subscribe(data => this.orders = data);
  }
}
```

---

## `src/app/features/orders/orders.routes.ts`

```typescript
import { Routes } from '@angular/router';

export const ORDER_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./order-list/order-list.component').then(m => m.OrderListComponent)
  }
];
```

---

## `src/app/app.routes.ts`

```typescript
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'products',
    loadChildren: () => import('./features/products/products.routes').then(m => m.PRODUCT_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'orders',
    loadChildren: () => import('./features/orders/orders.routes').then(m => m.ORDER_ROUTES),
    canActivate: [authGuard]
  },
  { path: '', redirectTo: '/products', pathMatch: 'full' },
  { path: '403', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) }
];
```

---

## `src/app/app.config.ts`

```typescript
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
};
```
