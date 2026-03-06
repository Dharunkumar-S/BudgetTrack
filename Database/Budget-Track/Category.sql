USE [Budget-Track];
GO

-- Get All Categories
CREATE OR ALTER PROCEDURE uspGetAllCategories
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CategoryID,
        CategoryName,
        CategoryCode,
        IsActive
    FROM tCategory
    WHERE IsDeleted = 0
    ORDER BY CategoryName ASC;
END
GO

-- Create Category
CREATE OR ALTER PROCEDURE uspCreateCategory
    @CategoryName NVARCHAR(100),
    @CreatedByUserID INT,
    @NewCategoryID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Check if category name already exists
        IF EXISTS (SELECT 1
    FROM tCategory
    WHERE CategoryName = @CategoryName AND IsDeleted = 0)
        BEGIN
        RAISERROR('Category with this name already exists', 16, 1);
        RETURN;
    END

        -- Auto-generate category code: CAT001, CAT002, ...
        DECLARE @CategoryCode NVARCHAR(50);
        DECLARE @NextSeq INT;
        SELECT @NextSeq = ISNULL(MAX(CAST(SUBSTRING(CategoryCode, 4, 3) AS INT)), 0) + 1
    FROM tCategory
    WHERE CategoryCode LIKE 'CAT[0-9][0-9][0-9]';
        SET @CategoryCode = 'CAT' + RIGHT('000' + CAST(@NextSeq AS VARCHAR(10)), 3);

        -- Insert category
        INSERT INTO tCategory
        (
        CategoryName, CategoryCode, IsActive, CreatedByUserID, CreatedDate, IsDeleted
        )
    VALUES
        (
            @CategoryName, @CategoryCode, 1, @CreatedByUserID, GETUTCDATE(), 0
        );

        SET @NewCategoryID = SCOPE_IDENTITY();

        -- Capture new state for audit
        DECLARE @NewValueJSON NVARCHAR(MAX);
        SELECT @NewValueJSON = (
            SELECT
            CategoryID, CategoryName, CategoryCode, IsActive, CreatedByUserID, CreatedDate
        FROM tCategory
        WHERE CategoryID = @NewCategoryID
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- Insert audit log
        INSERT INTO tAuditLog
        (UserID, EntityType, EntityID, Action, OldValue, NewValue, Description, CreatedDate)
    VALUES
        (
            @CreatedByUserID,
            'Category',
            @NewCategoryID,
            1, -- AuditAction.Create
            NULL,
            @NewValueJSON,
            'Category created: ' + @CategoryName,
            GETUTCDATE()
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Update Category
CREATE OR ALTER PROCEDURE uspUpdateCategory
    @CategoryID INT,
    @CategoryName NVARCHAR(100),
    @IsActive BIT,
    @UpdatedByUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Validate category exists
        IF NOT EXISTS (SELECT 1
    FROM tCategory
    WHERE CategoryID = @CategoryID AND IsDeleted = 0)
        BEGIN
        RAISERROR('Category not found or has been deleted', 16, 1);
        RETURN;
    END

        -- Check if new name conflicts with another category
        IF EXISTS (
            SELECT 1
    FROM tCategory
    WHERE CategoryName = @CategoryName
        AND CategoryID != @CategoryID
        AND IsDeleted = 0
        )
        BEGIN
        RAISERROR('Another category with this name already exists', 16, 1);
        RETURN;
    END

        -- Capture old state BEFORE the update
        DECLARE @OldValueJSON NVARCHAR(MAX);
        SELECT @OldValueJSON = (
            SELECT
            CategoryID, CategoryName, CategoryCode, IsActive, UpdatedByUserID, UpdatedDate
        FROM tCategory
        WHERE CategoryID = @CategoryID
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- Update category
        UPDATE tCategory
        SET 
            CategoryName = @CategoryName,
            IsActive = @IsActive,
            UpdatedByUserID = @UpdatedByUserID,
            UpdatedDate = GETUTCDATE()
        WHERE CategoryID = @CategoryID;

        -- Capture new state AFTER the update
        DECLARE @NewValueJSON NVARCHAR(MAX);
        SELECT @NewValueJSON = (
            SELECT
            CategoryID, CategoryName, CategoryCode, IsActive, UpdatedByUserID, UpdatedDate
        FROM tCategory
        WHERE CategoryID = @CategoryID
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- Insert audit log
        INSERT INTO tAuditLog
        (UserID, EntityType, EntityID, Action, OldValue, NewValue, Description, CreatedDate)
    VALUES
        (
            @UpdatedByUserID,
            'Category',
            @CategoryID,
            2, -- AuditAction.Update
            @OldValueJSON,
            @NewValueJSON,
            'Category updated: ' + @CategoryName + CASE WHEN @IsActive = 0 THEN ' (Inactive)' ELSE '' END,
            GETUTCDATE()
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Delete Category (Soft Delete)
CREATE OR ALTER PROCEDURE uspDeleteCategory
    @CategoryID INT,
    @DeletedByUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Validate category exists
        IF NOT EXISTS (SELECT 1
    FROM tCategory
    WHERE CategoryID = @CategoryID AND IsDeleted = 0)
        BEGIN
        RAISERROR('Category not found or has been deleted', 16, 1);
        RETURN;
    END

        -- Check if category is in use by any expenses
        IF EXISTS (SELECT 1
    FROM tExpense
    WHERE CategoryID = @CategoryID AND IsDeleted = 0)
        BEGIN
        RAISERROR('Cannot delete category that is in use by expenses', 16, 1);
        RETURN;
    END

        -- Capture current state before soft delete
        DECLARE @CategoryName NVARCHAR(100);
        DECLARE @OldValueJSON NVARCHAR(MAX);
        
        SELECT
        @CategoryName = CategoryName,
        @OldValueJSON = (
                SELECT
            CategoryID, CategoryName, CategoryCode, IsActive, UpdatedByUserID, UpdatedDate
        FROM tCategory
        WHERE CategoryID = @CategoryID
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            )
    FROM tCategory
    WHERE CategoryID = @CategoryID;

        -- Soft delete category
        UPDATE tCategory
        SET 
            IsDeleted = 1,
            IsActive = 0,
            DeletedDate = GETUTCDATE(),
            DeletedByUserID = @DeletedByUserID,
            UpdatedByUserID = @DeletedByUserID,
            UpdatedDate = GETUTCDATE()
        WHERE CategoryID = @CategoryID;

        -- Insert audit log
        INSERT INTO tAuditLog
        (UserID, EntityType, EntityID, Action, OldValue, NewValue, Description, CreatedDate)
    VALUES
        (
            @DeletedByUserID,
            'Category',
            @CategoryID,
            3, -- AuditAction.Delete
            @OldValueJSON,
            NULL,
            'Category deleted: ' + @CategoryName,
            GETUTCDATE()
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO