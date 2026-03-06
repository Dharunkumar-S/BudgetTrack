USE [Budget-Track];
GO

-- Retrieves all budgets including deleted ones for admin
CREATE OR ALTER VIEW vwGetAllBudgetsAdmin
AS
    SELECT
        b.BudgetID,
        b.Title,
        b.Code,
        b.DepartmentID,
        d.DepartmentName,
        b.AmountAllocated,
        b.AmountSpent,
        b.AmountRemaining,
        CASE 
        WHEN b.AmountAllocated > 0 
        THEN (b.AmountSpent / b.AmountAllocated) * 100 
        ELSE 0 
    END AS UtilizationPercentage,
        b.StartDate,
        b.EndDate,
        b.Status,
        CASE b.Status
        WHEN 1 THEN 'Active'
        WHEN 2 THEN 'Closed'
    END AS StatusName,
        b.Notes,
        b.CreatedByUserID,
        CONCAT(u.FirstName, ' ', u.LastName) AS CreatedByName,
        u.EmployeeID AS CreatedByEmployeeID,
        b.CreatedDate,
        b.UpdatedDate,
        b.DeletedDate,
        b.DeletedByUserID,
        CONCAT(du.FirstName, ' ', du.LastName) AS DeletedByName,
        IIF(DATEDIFF(DAY, GETUTCDATE(), b.EndDate) < 0, 0, DATEDIFF(DAY, GETUTCDATE(), b.EndDate)) AS DaysRemaining,
        CAST(CASE WHEN b.EndDate < GETUTCDATE() THEN 1 ELSE 0 END AS BIT) AS IsExpired,
        CAST(CASE WHEN b.AmountSpent > b.AmountAllocated THEN 1 ELSE 0 END AS BIT) AS IsOverBudget,
        b.IsDeleted
    FROM tBudget b
        INNER JOIN tUser u ON b.CreatedByUserID = u.UserID
        INNER JOIN tDepartment d ON b.DepartmentID = d.DepartmentID
        LEFT JOIN tUser du ON b.DeletedByUserID = du.UserID
GO

-- Retrieves all budgets (non-deleted only)
CREATE OR ALTER VIEW vwGetAllBudgets
AS
    SELECT
        b.BudgetID,
        b.Title,
        b.Code,
        b.DepartmentID,
        d.DepartmentName,
        b.AmountAllocated,
        b.AmountSpent,
        b.AmountRemaining,
        CASE 
        WHEN b.AmountAllocated > 0 
        THEN (b.AmountSpent / b.AmountAllocated) * 100 
        ELSE 0 
    END AS UtilizationPercentage,
        b.StartDate,
        b.EndDate,
        b.Status,
        CASE b.Status
        WHEN 1 THEN 'Active'
        WHEN 2 THEN 'Closed'
    END AS StatusName,
        b.Notes,
        b.CreatedByUserID,
        CONCAT(u.FirstName, ' ', u.LastName) AS CreatedByName,
        u.EmployeeID AS CreatedByEmployeeID,
        b.CreatedDate,
        b.UpdatedDate,
        b.DeletedDate,
        b.DeletedByUserID,
        CONCAT(du.FirstName, ' ', du.LastName) AS DeletedByName,
        IIF(DATEDIFF(DAY, GETUTCDATE(), b.EndDate) < 0, 0, DATEDIFF(DAY, GETUTCDATE(), b.EndDate)) AS DaysRemaining,
        CAST(CASE WHEN b.EndDate < GETUTCDATE() THEN 1 ELSE 0 END AS BIT) AS IsExpired,
        CAST(CASE WHEN b.AmountSpent > b.AmountAllocated THEN 1 ELSE 0 END AS BIT) AS IsOverBudget,
        b.IsDeleted
    FROM tBudget b
        INNER JOIN tUser u ON b.CreatedByUserID = u.UserID
        INNER JOIN tDepartment d ON b.DepartmentID = d.DepartmentID
        LEFT JOIN tUser du ON b.DeletedByUserID = du.UserID
    WHERE b.IsDeleted = 0
GO

-- Description: Creates a new budget with audit logging and notifications
CREATE OR ALTER PROCEDURE uspCreateBudget
    @Title NVARCHAR(200),
    @DepartmentID INT,
    @AmountAllocated DECIMAL(18,2),
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @Status INT = 1,
    @Notes NVARCHAR(1000) = NULL,
    @CreatedByUserID INT,
    @BudgetID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Auto-generate budget code: BT<YY><seq>, e.g. BT26001
        DECLARE @Code NVARCHAR(50);
        DECLARE @YearPart NVARCHAR(2);
        DECLARE @NextSeq INT;
        SET @YearPart = RIGHT(CAST(YEAR(GETUTCDATE()) AS NVARCHAR(4)), 2);
        SELECT @NextSeq = ISNULL(MAX(CAST(SUBSTRING(Code, 5, 3) AS INT)), 0) + 1
    FROM tBudget
    WHERE Code LIKE 'BT' + @YearPart + '[0-9][0-9][0-9]';
        SET @Code = 'BT' + @YearPart + RIGHT('000' + CAST(@NextSeq AS VARCHAR(10)), 3);

        -- Insert new budget
        INSERT INTO tBudget
        (
        Title, Code, DepartmentID, AmountAllocated, AmountSpent, AmountRemaining,
        StartDate, EndDate, Status, Notes, CreatedByUserID, CreatedDate, IsDeleted
        )
    VALUES
        (
            @Title, @Code, @DepartmentID, @AmountAllocated, 0, @AmountAllocated,
            @StartDate, @EndDate, @Status, @Notes, @CreatedByUserID, GETUTCDATE(), 0
        );

        -- Get the new BudgetID
        SET @BudgetID = CAST(SCOPE_IDENTITY() AS INT);

        -- Build JSON representation of the new budget
        DECLARE @NewValue NVARCHAR(MAX);
        
        SELECT @NewValue = (
            SELECT
            @BudgetID AS BudgetID,
            @Title AS Title,
            @Code AS Code,
            @DepartmentID AS DepartmentID,
            @AmountAllocated AS AmountAllocated,
            0.00 AS AmountSpent,
            @AmountAllocated AS AmountRemaining,
            @StartDate AS StartDate,
            @EndDate AS EndDate,
            @Status AS Status,
            GETUTCDATE() AS CreatedDate
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- Get manager name for audit log
        DECLARE @ManagerName NVARCHAR(200);
        SELECT @ManagerName = FirstName + ' ' + LastName
    FROM tUser
    WHERE UserID = @CreatedByUserID AND IsDeleted = 0;

        -- Insert audit log with JSON state
        INSERT INTO tAuditLog
        (UserID, EntityType, EntityID, Action, OldValue, NewValue, Description, CreatedDate)
    VALUES
        (
            @CreatedByUserID,
            'Budget',
            @BudgetID,
            1, -- Create
            NULL,
            @NewValue,
            'Budget Created: ' + @Code + ' ' + @Title,
            GETUTCDATE()
        );

        -- Send notifications to subordinates using set-based operation
        INSERT INTO tNotification
        (
        SenderUserID, ReceiverUserID, Type, Message,
        Status, RelatedEntityType, RelatedEntityID, CreatedDate, IsDeleted
        )
    SELECT
        @CreatedByUserID,
        u.UserID,
        4, -- BudgetCreated
        'New Budget ' + @Code + ' ' + @Title + ' Created',
        1, -- Unread
        'Budget',
        @BudgetID,
        GETUTCDATE(),
        0
    FROM tUser u
    WHERE u.ManagerID = @CreatedByUserID AND u.IsDeleted = 0;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Updates an existing budget with audit logging and notifications
CREATE OR ALTER PROCEDURE uspUpdateBudget
    @BudgetID INT,
    @Title NVARCHAR(200) = NULL,
    @DepartmentID INT = NULL,
    @AmountAllocated DECIMAL(18,2) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @Status INT,
    @Notes NVARCHAR(1000) = NULL,
    @UpdatedByUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @OldValue NVARCHAR(MAX), @NewValue NVARCHAR(MAX);

        -- Capture old state as JSON
        SELECT @OldValue = (
            SELECT
            BudgetID, Title, Code, DepartmentID, AmountAllocated, AmountSpent,
            AmountRemaining, StartDate, EndDate, Status
        FROM tBudget
        WHERE BudgetID = @BudgetID AND IsDeleted = 0
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );
        IF @OldValue IS NULL
        BEGIN
        RAISERROR('Budget not found',16,1);
        RETURN;
    END

        -- Fetch current values for any NULL parameters
        DECLARE @CurrentTitle NVARCHAR(200),
                @CurrentCode NVARCHAR(50),
                @CurrentDepartmentID INT,
                @CurrentAmountAllocated DECIMAL(18,2),
                @CurrentStartDate DATETIME2,
                @CurrentEndDate DATETIME2,
                @CurrentNotes NVARCHAR(1000);

        SELECT
        @CurrentTitle = Title,
        @CurrentCode = Code,
        @CurrentDepartmentID = DepartmentID,
        @CurrentAmountAllocated = AmountAllocated,
        @CurrentStartDate = StartDate,
        @CurrentEndDate = EndDate,
        @CurrentNotes = Notes
    FROM tBudget
    WHERE BudgetID = @BudgetID AND IsDeleted = 0;

        -- Use provided values or current values for NULL parameters
        SET @Title = ISNULL(@Title, @CurrentTitle);
        SET @DepartmentID = ISNULL(@DepartmentID, @CurrentDepartmentID);
        SET @AmountAllocated = ISNULL(@AmountAllocated, @CurrentAmountAllocated);
        SET @StartDate = ISNULL(@StartDate, @CurrentStartDate);
        SET @EndDate = ISNULL(@EndDate, @CurrentEndDate);
        SET @Notes = ISNULL(@Notes, @CurrentNotes);

        -- Check for duplicate title (excluding the budget being updated)
        IF EXISTS (
            SELECT 1
    FROM tBudget
    WHERE Title = @Title AND BudgetID != @BudgetID AND IsDeleted = 0
        )
        BEGIN
        RAISERROR('A budget with this title already exists', 16, 1);
        RETURN;
    END

        -- Check if incoming data is identical to the existing data — skip update if nothing changed
        -- This checks if ANY field is different from the current values
        IF NOT EXISTS (
            SELECT 1
    FROM tBudget
    WHERE BudgetID = @BudgetID
        AND IsDeleted = 0
        AND (
                  Title != @Title
        OR DepartmentID != @DepartmentID
        OR AmountAllocated != @AmountAllocated
        OR CAST(StartDate AS DATE) != CAST(@StartDate AS DATE)
        OR CAST(EndDate AS DATE) != CAST(@EndDate AS DATE)
        OR Status != @Status
        OR ISNULL(Notes, '') != ISNULL(@Notes, '')
              )
        )
        BEGIN
        RAISERROR('No changes detected. The budget data is identical to the existing record.', 16, 1);
        RETURN;
    END

        UPDATE tBudget
        SET 
            Title = @Title,
            DepartmentID = @DepartmentID,
            AmountAllocated = @AmountAllocated,
            AmountRemaining = CASE WHEN @AmountAllocated < AmountSpent THEN 0 ELSE @AmountAllocated - AmountSpent END,
            StartDate = @StartDate,
            EndDate = @EndDate,
            Status = @Status,
            Notes = @Notes,
            UpdatedByUserID = @UpdatedByUserID,
            UpdatedDate = GETUTCDATE()
        WHERE BudgetID = @BudgetID AND IsDeleted = 0;

        -- Capture new state as JSON
        SELECT @NewValue = (
            SELECT
            BudgetID, Title, Code, DepartmentID, AmountAllocated, AmountSpent,
            AmountRemaining, StartDate, EndDate, Status, UpdatedDate
        FROM tBudget
        WHERE BudgetID = @BudgetID AND IsDeleted = 0
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- Get manager name for audit log
        DECLARE @ManagerName NVARCHAR(200);
        SELECT @ManagerName = FirstName + ' ' + LastName
    FROM tUser
    WHERE UserID = @UpdatedByUserID AND IsDeleted = 0;

        -- Insert audit log with old and new JSON states
        INSERT INTO tAuditLog
        (UserID, EntityType, EntityID, Action, OldValue, NewValue, Description, CreatedDate)
    VALUES
        (
            @UpdatedByUserID,
            'Budget',
            @BudgetID,
            2, -- Update
            @OldValue,
            @NewValue,
            'Budget Updated: ' + @CurrentCode +' '+ @Title ,
            GETUTCDATE()
        );

        -- Send notifications to subordinates using set-based operation
        INSERT INTO tNotification
        (
        SenderUserID, ReceiverUserID, Type, Message,
        Status, RelatedEntityType, RelatedEntityID, CreatedDate, IsDeleted
        )
    SELECT
        @UpdatedByUserID,
        u.UserID,
        5, -- BudgetUpdated
        'Budget ' + @CurrentCode + ' ' + @Title + ' Updated',
        1, -- Unread
        'Budget',
        @BudgetID,
        GETUTCDATE(),
        0
    FROM tUser u
    WHERE u.ManagerID = @UpdatedByUserID AND u.IsDeleted = 0;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Soft deletes a budget with audit logging and notifications
CREATE OR ALTER PROCEDURE uspDeleteBudget
    @BudgetID INT,
    @DeletedByUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @OldValue NVARCHAR(MAX), @Title NVARCHAR(200);

        -- Capture budget state before deletion as JSON
        SELECT
        @OldValue = (
                SELECT
            BudgetID, Title, Code, DepartmentID, AmountAllocated, AmountSpent,
            AmountRemaining, StartDate, EndDate, Status,
            CAST(0 AS BIT) AS IsDeleted
        FROM tBudget
        WHERE BudgetID = @BudgetID AND IsDeleted = 0
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            ),
        @Title = Title
    FROM tBudget
    WHERE BudgetID = @BudgetID AND IsDeleted = 0;

        -- Validate budget exists and is not already deleted
        IF @OldValue IS NULL
        BEGIN
        RAISERROR('Budget not found or has already been deleted', 16, 1);
        RETURN;
    END

        -- Get manager name and code for audit log
        DECLARE @ManagerName NVARCHAR(200), @Code NVARCHAR(50);
        SELECT @ManagerName = FirstName + ' ' + LastName
    FROM tUser
    WHERE UserID = @DeletedByUserID AND IsDeleted = 0;

        SELECT @Code = Code
    FROM tBudget
    WHERE BudgetID = @BudgetID;

        -- Soft delete the budget
        UPDATE tBudget
        SET 
            IsDeleted = 1,
            Status = 2, -- Closed
            DeletedByUserID = @DeletedByUserID,
            DeletedDate = GETUTCDATE(),
            UpdatedByUserID = @DeletedByUserID,
            UpdatedDate = GETUTCDATE()
        WHERE BudgetID = @BudgetID AND IsDeleted = 0;

        -- Insert audit log with old state in JSON and manager name
        INSERT INTO tAuditLog
        (UserID, EntityType, EntityID, Action, OldValue, NewValue, Description, CreatedDate)
    VALUES
        (
            @DeletedByUserID,
            'Budget',
            @BudgetID,
            3, -- Delete
            @OldValue,
            NULL,
            'Budget Deleted: ' + ISNULL(@Code, 'N/A') +' '+ @Title ,
            GETUTCDATE()
        );

        -- Send notifications to subordinates using set-based operation
        INSERT INTO tNotification
        (
        SenderUserID, ReceiverUserID, Type, Message,
        Status, RelatedEntityType, RelatedEntityID, CreatedDate, IsDeleted
        )
    SELECT
        @DeletedByUserID,
        u.UserID,
        6, -- BudgetDeleted
        'Budget Deleted: ' + ISNULL(@Code, 'N/A') +' '+ @Title,
        1, -- Unread
        'Budget',
        @BudgetID,
        GETUTCDATE(),
        0
    FROM tUser u
    WHERE u.ManagerID = @DeletedByUserID AND u.IsDeleted = 0;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO