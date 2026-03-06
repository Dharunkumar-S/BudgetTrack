using Budget_Track.Models.DTOs.Budget;
using Budget_Track.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Data
{
    public class BudgetTrackDbContext : DbContext
    {
        public BudgetTrackDbContext(DbContextOptions<BudgetTrackDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BudgetDto - keyless entity mapped to vwGetAllBudgetsAdmin view
            modelBuilder.Entity<BudgetDto>()
                .HasNoKey()
                .ToView("vwGetAllBudgetsAdmin");

            // Department relationships
            modelBuilder
                .Entity<Department>()
                .HasMany(d => d.Users)
                .WithOne(u => u.Department)
                .HasForeignKey(u => u.DepartmentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Department>()
                .HasMany(d => d.Budgets)
                .WithOne(b => b.Department)
                .HasForeignKey(b => b.DepartmentID)
                .OnDelete(DeleteBehavior.Restrict);

            // User-Role relationship
            modelBuilder
                .Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            // Role audit relationships
            modelBuilder
                .Entity<Role>()
                .HasOne(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Role>()
                .HasOne(r => r.UpdatedByUser)
                .WithMany()
                .HasForeignKey(r => r.UpdatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Role>()
                .HasOne(r => r.DeletedByUser)
                .WithMany()
                .HasForeignKey(r => r.DeletedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Department audit relationships
            modelBuilder
                .Entity<Department>()
                .HasOne(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Department>()
                .HasOne(d => d.UpdatedByUser)
                .WithMany()
                .HasForeignKey(d => d.UpdatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Department>()
                .HasOne(d => d.DeletedByUser)
                .WithMany()
                .HasForeignKey(d => d.DeletedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // User self-referential relationship(Manager / Subordinates)
            modelBuilder
                .Entity<User>()
                .HasOne(u => u.Manager)
                .WithMany(u => u.Subordinates)
                .HasForeignKey(u => u.ManagerID)
                .OnDelete(DeleteBehavior.Restrict);

            // User audit relationships (CreatedBy, UpdatedBy, DeletedBy)
            modelBuilder
                .Entity<User>()
                .HasOne(u => u.CreatedByUser)
                .WithMany()
                .HasForeignKey(u => u.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<User>()
                .HasOne(u => u.UpdatedByUser)
                .WithMany()
                .HasForeignKey(u => u.UpdatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<User>()
                .HasOne(u => u.DeletedByUser)
                .WithMany()
                .HasForeignKey(u => u.DeletedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Budget and User relationship (CreatedBy)
            modelBuilder
                .Entity<Budget>()
                .HasOne(b => b.CreatedByUser)
                .WithMany(u => u.BudgetsCreated)
                .HasForeignKey(b => b.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Budget audit relationships (UpdatedBy, DeletedBy)
            modelBuilder
                .Entity<Budget>()
                .HasOne(b => b.UpdatedByUser)
                .WithMany()
                .HasForeignKey(b => b.UpdatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Budget>()
                .HasOne(b => b.DeletedByUser)
                .WithMany()
                .HasForeignKey(b => b.DeletedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Budget-Expense relationship
            modelBuilder
                .Entity<Budget>()
                .HasMany(b => b.Expenses)
                .WithOne(e => e.Budget)
                .HasForeignKey(e => e.BudgetID)
                .OnDelete(DeleteBehavior.Restrict);

            // Category audit relationships
            modelBuilder
                .Entity<Category>()
                .HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Category>()
                .HasOne(c => c.UpdatedByUser)
                .WithMany()
                .HasForeignKey(c => c.UpdatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Category>()
                .HasOne(c => c.DeletedByUser)
                .WithMany()
                .HasForeignKey(c => c.DeletedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Category-Expense relationship
            modelBuilder
                .Entity<Category>()
                .HasMany(c => c.Expenses)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense and Budget relationship
            modelBuilder
                .Entity<Expense>()
                .HasOne(e => e.Budget)
                .WithMany(b => b.Expenses)
                .HasForeignKey(e => e.BudgetID)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense and Category relationship
            modelBuilder
                .Entity<Expense>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense and User relationships (SubmittedBy, ApprovedBy)
            modelBuilder
                .Entity<Expense>()
                .HasOne(e => e.SubmittedByUser)
                .WithMany(u => u.ExpensesSubmitted)
                .HasForeignKey(e => e.SubmittedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Expense>()
                .HasOne(e => e.ApprovedByUser)
                .WithMany(u => u.ExpensesApproved)
                .HasForeignKey(e => e.ManagerUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense audit relationships (UpdatedBy, DeletedBy)
            modelBuilder
                .Entity<Expense>()
                .HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Expense>()
                .HasOne(e => e.DeletedByUser)
                .WithMany()
                .HasForeignKey(e => e.DeletedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Report and User relationship (GeneratedBy)
            modelBuilder
                .Entity<Report>()
                .HasOne(r => r.GeneratedByUser)
                .WithMany(u => u.ReportsGenerated)
                .HasForeignKey(r => r.GeneratedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Report audit relationship (DeletedBy)
            modelBuilder
                .Entity<Report>()
                .HasOne(r => r.DeletedByUser)
                .WithMany()
                .HasForeignKey(r => r.DeletedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification and User relationships (Sender, Receiver)
            modelBuilder
                .Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany()
                .HasForeignKey(n => n.SenderUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.ReceiverUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // AuditLog and User relationship - nullable FK so deleting a user sets UserID to NULL
            modelBuilder
                .Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.SetNull);

            // Budget entity decimal precision
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.Property(b => b.AmountAllocated).HasPrecision(18, 2);
                entity.Property(b => b.AmountSpent).HasPrecision(18, 2);
                entity.Property(b => b.AmountRemaining).HasPrecision(18, 2);
            });

            // Expense entity decimal precision
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);
            });

            // Soft delete global query filters

            // Global query filter for soft delete on Role
            modelBuilder.Entity<Role>().HasQueryFilter(r => !r.IsDeleted);

            // Global query filter for soft delete on Department
            modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);

            // Global query filter for soft delete on User
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

            // Global query filter for soft delete on Budget
            modelBuilder.Entity<Budget>().HasQueryFilter(b => !b.IsDeleted);

            // Global query filter for soft delete on Category
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);

            // Global query filter for soft delete on Expense
            modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);

            // Global query filter for soft delete on Report
            modelBuilder.Entity<Report>().HasQueryFilter(r => !r.IsDeleted);

            // Global query filter for soft delete on Notification
            modelBuilder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);

            // Configure enum conversions (stored as integers)
            modelBuilder.Entity<User>().Property(u => u.Status).HasConversion<int>();

            modelBuilder.Entity<Budget>().Property(b => b.Status).HasConversion<int>();

            modelBuilder.Entity<Expense>().Property(e => e.Status).HasConversion<int>();

            modelBuilder.Entity<Notification>().Property(n => n.Type).HasConversion<int>();

            modelBuilder.Entity<Notification>().Property(n => n.Status).HasConversion<int>();

            modelBuilder.Entity<AuditLog>().Property(a => a.Action).HasConversion<int>();

            // Default Values for timestamps
            modelBuilder
                .Entity<Role>()
                .Property(r => r.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Entity<Department>()
                .Property(d => d.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Entity<User>()
                .Property(u => u.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Entity<Budget>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Entity<Category>()
                .Property(c => c.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Entity<Expense>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Entity<Expense>()
                .Property(e => e.SubmittedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Entity<Report>()
                .Property(r => r.GeneratedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Entity<Notification>()
                .Property(n => n.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Entity<AuditLog>()
                .Property(a => a.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
