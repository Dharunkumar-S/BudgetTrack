USE [Budget-Track];
GO

-- Get All Departments
CREATE OR ALTER PROCEDURE uspGetAllDepartments
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DepartmentID,
        DepartmentName,
        DepartmentCode,
        IsActive
    FROM tDepartment
    WHERE IsDeleted = 0
    ORDER BY DepartmentName ASC;
END
GO

-- Create Department with Audit
CREATE OR ALTER PROCEDURE uspCreateDepartment
    @DepartmentName NVARCHAR(100),
    @CreatedByUserID INT,
    @NewDepartmentID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Check if department name already exists
        IF EXISTS (SELECT 1
    FROM tDepartment
    WHERE DepartmentName = @DepartmentName AND IsDeleted = 0)
        BEGIN
        RAISERROR('Department with this name already exists', 16, 1);
        RETURN;
    END

        -- Auto-generate department code: DEPT001, DEPT002, ...
        DECLARE @DepartmentCode NVARCHAR(50);
        DECLARE @NextSeq INT;
        SELECT @NextSeq = ISNULL(MAX(CAST(SUBSTRING(DepartmentCode, 5, 3) AS INT)), 0) + 1
    FROM tDepartment
    WHERE DepartmentCode LIKE 'DEPT[0-9][0-9][0-9]';
        SET @DepartmentCode = 'DEPT' + RIGHT('000' + CAST(@NextSeq AS VARCHAR(10)), 3);

        -- Insert department
        INSERT INTO tDepartment
        (
        DepartmentName, DepartmentCode, IsActive, CreatedByUserID, CreatedDate, IsDeleted
        )
    VALUES
        (
            @DepartmentName, @DepartmentCode, 1, @CreatedByUserID, GETUTCDATE(), 0
        );

        SET @NewDepartmentID = SCOPE_IDENTITY();

        -- Capture new state for audit
        DECLARE @NewValueJSON NVARCHAR(MAX);
        SELECT @NewValueJSON = (
            SELECT
            DepartmentID, DepartmentName, DepartmentCode, IsActive, CreatedByUserID, CreatedDate
        FROM tDepartment
        WHERE DepartmentID = @NewDepartmentID
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- Insert audit log
        INSERT INTO tAuditLog
        (UserID, EntityType, EntityID, Action, OldValue, NewValue, Description, CreatedDate)
    VALUES
        (
            @CreatedByUserID,
            'Department',
            @NewDepartmentID,
            1, -- AuditAction.Create
            NULL,
            @NewValueJSON,
            'Department created: ' + @DepartmentName,
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

-- Update Department with Audit
CREATE OR ALTER PROCEDURE uspUpdateDepartment
    @DepartmentID INT,
    @DepartmentName NVARCHAR(100),
    @IsActive BIT,
    @UpdatedByUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Validate department exists
        IF NOT EXISTS (SELECT 1
    FROM tDepartment
    WHERE DepartmentID = @DepartmentID AND IsDeleted = 0)
        BEGIN
        RAISERROR('Department not found or has been deleted', 16, 1);
        RETURN;
    END

        -- Check if new name conflicts with another department
        IF EXISTS (
            SELECT 1
    FROM tDepartment
    WHERE DepartmentName = @DepartmentName
        AND DepartmentID != @DepartmentID
        AND IsDeleted = 0
        )
        BEGIN
        RAISERROR('Another department with this name already exists', 16, 1);
        RETURN;
    END

        -- Capture old state BEFORE the update
        DECLARE @OldValueJSON NVARCHAR(MAX);
        SELECT @OldValueJSON = (
            SELECT
            DepartmentID, DepartmentName, DepartmentCode, IsActive, UpdatedByUserID, UpdatedDate
        FROM tDepartment
        WHERE DepartmentID = @DepartmentID
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- Update department
        UPDATE tDepartment
        SET 
            DepartmentName = @DepartmentName,
            IsActive = @IsActive,
            UpdatedByUserID = @UpdatedByUserID,
            UpdatedDate = GETUTCDATE()
        WHERE DepartmentID = @DepartmentID;

        -- Capture new state AFTER the update
        DECLARE @NewValueJSON NVARCHAR(MAX);
        SELECT @NewValueJSON = (
            SELECT
            DepartmentID, DepartmentName, DepartmentCode, IsActive, UpdatedByUserID, UpdatedDate
        FROM tDepartment
        WHERE DepartmentID = @DepartmentID
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- Insert audit log
        INSERT INTO tAuditLog
        (UserID, EntityType, EntityID, Action, OldValue, NewValue, Description, CreatedDate)
    VALUES
        (
            @UpdatedByUserID,
            'Department',
            @DepartmentID,
            2, -- AuditAction.Update
            @OldValueJSON,
            @NewValueJSON,
            'Department updated: ' + @DepartmentName + CASE WHEN @IsActive = 0 THEN ' (Inactive)' ELSE '' END,
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

-- Delete Department with Audit (Soft Delete)
CREATE OR ALTER PROCEDURE uspDeleteDepartment
    @DepartmentID INT,
    @DeletedByUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Validate department exists
        IF NOT EXISTS (SELECT 1
    FROM tDepartment
    WHERE DepartmentID = @DepartmentID AND IsDeleted = 0)
        BEGIN
        RAISERROR('Department not found or has been deleted', 16, 1);
        RETURN;
    END

        -- Check if department is in use by any users
        IF EXISTS (SELECT 1
    FROM tUser
    WHERE DepartmentID = @DepartmentID AND IsDeleted = 0)
        BEGIN
        RAISERROR('Cannot delete department that is in use by users', 16, 1);
        RETURN;
    END

        -- Check if department is in use by any budgets
        IF EXISTS (SELECT 1
    FROM tBudget
    WHERE DepartmentID = @DepartmentID AND IsDeleted = 0)
        BEGIN
        RAISERROR('Cannot delete department that is in use by budgets', 16, 1);
        RETURN;
    END

        -- Capture current state before soft delete
        DECLARE @DeptName NVARCHAR(100);
        DECLARE @OldValueJSON NVARCHAR(MAX);
        
        SELECT
        @DeptName = DepartmentName,
        @OldValueJSON = (
                SELECT
            DepartmentID, DepartmentName, DepartmentCode, IsActive, UpdatedByUserID, UpdatedDate
        FROM tDepartment
        WHERE DepartmentID = @DepartmentID
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            )
    FROM tDepartment
    WHERE DepartmentID = @DepartmentID;

        -- Soft delete department
        UPDATE tDepartment
        SET 
            IsDeleted = 1,
            IsActive = 0,
            DeletedDate = GETUTCDATE(),
            DeletedByUserID = @DeletedByUserID,
            UpdatedByUserID = @DeletedByUserID,
            UpdatedDate = GETUTCDATE()
        WHERE DepartmentID = @DepartmentID;

        -- Insert audit log
        INSERT INTO tAuditLog
        (UserID, EntityType, EntityID, Action, OldValue, NewValue, Description, CreatedDate)
    VALUES
        (
            @DeletedByUserID,
            'Department',
            @DepartmentID,
            3, -- AuditAction.Delete
            @OldValueJSON,
            NULL,
            'Department deleted: ' + @DeptName,
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