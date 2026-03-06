using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Budget_Track.Migrations
{
    /// <inheritdoc />
    public partial class BudgetTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tAuditLog",
                columns: table => new
                {
                    AuditLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityID = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tAuditLog", x => x.AuditLogID);
                });

            migrationBuilder.CreateTable(
                name: "tBudget",
                columns: table => new
                {
                    BudgetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    AmountAllocated = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountSpent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountRemaining = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tBudget", x => x.BudgetID);
                });

            migrationBuilder.CreateTable(
                name: "tCategory",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tCategory", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "tDepartment",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tDepartment", x => x.DepartmentID);
                });

            migrationBuilder.CreateTable(
                name: "tExpense",
                columns: table => new
                {
                    ExpenseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetID = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MerchantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubmittedByUserID = table.Column<int>(type: "int", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ManagerUserID = table.Column<int>(type: "int", nullable: true),
                    StatusApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovalComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tExpense", x => x.ExpenseID);
                    table.ForeignKey(
                        name: "FK_tExpense_tBudget_BudgetID",
                        column: x => x.BudgetID,
                        principalTable: "tBudget",
                        principalColumn: "BudgetID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tExpense_tCategory_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "tCategory",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tNotification",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderUserID = table.Column<int>(type: "int", nullable: false),
                    ReceiverUserID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tNotification", x => x.NotificationID);
                });

            migrationBuilder.CreateTable(
                name: "tReport",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    Metrics = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    GeneratedByUserID = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tReport", x => x.ReportID);
                });

            migrationBuilder.CreateTable(
                name: "tRole",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tRole", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "tUser",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ManagerID = table.Column<int>(type: "int", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tUser", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_tUser_tDepartment_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "tDepartment",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tUser_tRole_RoleID",
                        column: x => x.RoleID,
                        principalTable: "tRole",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tUser_tUser_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "tUser",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tUser_tUser_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "tUser",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tUser_tUser_ManagerID",
                        column: x => x.ManagerID,
                        principalTable: "tUser",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tUser_tUser_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "tUser",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tAuditLog_EntityType",
                table: "tAuditLog",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_tAuditLog_UserID",
                table: "tAuditLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_tBudget_Code",
                table: "tBudget",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tBudget_CreatedByUserID",
                table: "tBudget",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tBudget_DeletedByUserID",
                table: "tBudget",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tBudget_DepartmentID",
                table: "tBudget",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_tBudget_Title",
                table: "tBudget",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tBudget_UpdatedByUserID",
                table: "tBudget",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tCategory_CategoryCode",
                table: "tCategory",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tCategory_CategoryName",
                table: "tCategory",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tCategory_CreatedByUserID",
                table: "tCategory",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tCategory_DeletedByUserID",
                table: "tCategory",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tCategory_UpdatedByUserID",
                table: "tCategory",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tDepartment_CreatedByUserID",
                table: "tDepartment",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tDepartment_DeletedByUserID",
                table: "tDepartment",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tDepartment_DepartmentCode",
                table: "tDepartment",
                column: "DepartmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tDepartment_DepartmentName",
                table: "tDepartment",
                column: "DepartmentName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tDepartment_UpdatedByUserID",
                table: "tDepartment",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tExpense_BudgetID",
                table: "tExpense",
                column: "BudgetID");

            migrationBuilder.CreateIndex(
                name: "IX_tExpense_CategoryID",
                table: "tExpense",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_tExpense_DeletedByUserID",
                table: "tExpense",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tExpense_ManagerUserID",
                table: "tExpense",
                column: "ManagerUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tExpense_Status",
                table: "tExpense",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tExpense_SubmittedByUserID",
                table: "tExpense",
                column: "SubmittedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tExpense_Title",
                table: "tExpense",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_tExpense_UpdatedByUserID",
                table: "tExpense",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tNotification_ReceiverUserID",
                table: "tNotification",
                column: "ReceiverUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tNotification_SenderUserID",
                table: "tNotification",
                column: "SenderUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tReport_DeletedByUserID",
                table: "tReport",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tReport_GeneratedByUserID",
                table: "tReport",
                column: "GeneratedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tRole_CreatedByUserID",
                table: "tRole",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tRole_DeletedByUserID",
                table: "tRole",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tRole_RoleName",
                table: "tRole",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tRole_UpdatedByUserID",
                table: "tRole",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tUser_CreatedByUserID",
                table: "tUser",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tUser_DeletedByUserID",
                table: "tUser",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_tUser_DepartmentID",
                table: "tUser",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_tUser_Email",
                table: "tUser",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tUser_EmployeeID",
                table: "tUser",
                column: "EmployeeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tUser_ManagerID",
                table: "tUser",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_tUser_RoleID",
                table: "tUser",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_tUser_UpdatedByUserID",
                table: "tUser",
                column: "UpdatedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_tAuditLog_tUser_UserID",
                table: "tAuditLog",
                column: "UserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tBudget_tDepartment_DepartmentID",
                table: "tBudget",
                column: "DepartmentID",
                principalTable: "tDepartment",
                principalColumn: "DepartmentID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tBudget_tUser_CreatedByUserID",
                table: "tBudget",
                column: "CreatedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tBudget_tUser_DeletedByUserID",
                table: "tBudget",
                column: "DeletedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tBudget_tUser_UpdatedByUserID",
                table: "tBudget",
                column: "UpdatedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tCategory_tUser_CreatedByUserID",
                table: "tCategory",
                column: "CreatedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tCategory_tUser_DeletedByUserID",
                table: "tCategory",
                column: "DeletedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tCategory_tUser_UpdatedByUserID",
                table: "tCategory",
                column: "UpdatedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tDepartment_tUser_CreatedByUserID",
                table: "tDepartment",
                column: "CreatedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tDepartment_tUser_DeletedByUserID",
                table: "tDepartment",
                column: "DeletedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tDepartment_tUser_UpdatedByUserID",
                table: "tDepartment",
                column: "UpdatedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tExpense_tUser_DeletedByUserID",
                table: "tExpense",
                column: "DeletedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tExpense_tUser_ManagerUserID",
                table: "tExpense",
                column: "ManagerUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tExpense_tUser_SubmittedByUserID",
                table: "tExpense",
                column: "SubmittedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tExpense_tUser_UpdatedByUserID",
                table: "tExpense",
                column: "UpdatedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tNotification_tUser_ReceiverUserID",
                table: "tNotification",
                column: "ReceiverUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tNotification_tUser_SenderUserID",
                table: "tNotification",
                column: "SenderUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tReport_tUser_DeletedByUserID",
                table: "tReport",
                column: "DeletedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tReport_tUser_GeneratedByUserID",
                table: "tReport",
                column: "GeneratedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tRole_tUser_CreatedByUserID",
                table: "tRole",
                column: "CreatedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tRole_tUser_DeletedByUserID",
                table: "tRole",
                column: "DeletedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tRole_tUser_UpdatedByUserID",
                table: "tRole",
                column: "UpdatedByUserID",
                principalTable: "tUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tDepartment_tUser_CreatedByUserID",
                table: "tDepartment");

            migrationBuilder.DropForeignKey(
                name: "FK_tDepartment_tUser_DeletedByUserID",
                table: "tDepartment");

            migrationBuilder.DropForeignKey(
                name: "FK_tDepartment_tUser_UpdatedByUserID",
                table: "tDepartment");

            migrationBuilder.DropForeignKey(
                name: "FK_tRole_tUser_CreatedByUserID",
                table: "tRole");

            migrationBuilder.DropForeignKey(
                name: "FK_tRole_tUser_DeletedByUserID",
                table: "tRole");

            migrationBuilder.DropForeignKey(
                name: "FK_tRole_tUser_UpdatedByUserID",
                table: "tRole");

            migrationBuilder.DropTable(
                name: "tAuditLog");

            migrationBuilder.DropTable(
                name: "tExpense");

            migrationBuilder.DropTable(
                name: "tNotification");

            migrationBuilder.DropTable(
                name: "tReport");

            migrationBuilder.DropTable(
                name: "tBudget");

            migrationBuilder.DropTable(
                name: "tCategory");

            migrationBuilder.DropTable(
                name: "tUser");

            migrationBuilder.DropTable(
                name: "tDepartment");

            migrationBuilder.DropTable(
                name: "tRole");
        }
    }
}
