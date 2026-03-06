USE [Budget-Track];
GO

-- Retrieves all expenses across all budgets (non-deleted only)
CREATE
	OR

ALTER VIEW vwGetAllExpenses
AS
    SELECT e.ExpenseID,
        e.BudgetID,
        b.Title AS BudgetTitle,
        b.Code AS BudgetCode,
        e.CategoryID,
        c.CategoryName,
        e.Title,
        e.Amount,
        e.MerchantName,
        e.STATUS,
        CASE e.STATUS
		WHEN 1
			THEN 'Pending'
		WHEN 2
			THEN 'Approved'
		WHEN 3
			THEN 'Rejected'
		WHEN 4
			THEN 'Cancelled'
		ELSE 'Unknown'
		END AS StatusName,
        e.SubmittedDate,
        e.SubmittedByUserID,
        CONCAT (
		submitter.FirstName,
		' ',
		submitter.LastName
		) AS SubmittedByUserName,
        submitter.EmployeeID AS SubmittedByEmployeeID,
        ISNULL(d.DepartmentName, 'No Department') AS DepartmentName,
        e.ManagerUserID,
        CONCAT (
		approver.FirstName,
		' ',
		approver.LastName
		) AS ApprovedByUserName,
        e.StatusApprovedDate,
        e.ApprovalComments,
        e.RejectionReason,
        e.Notes,
        e.CreatedDate,
        e.UpdatedDate
    FROM tExpense e
        INNER JOIN tBudget b ON e.BudgetID = b.BudgetID
        INNER JOIN tCategory c ON e.CategoryID = c.CategoryID
        INNER JOIN tUser submitter ON e.SubmittedByUserID = submitter.UserID
        LEFT JOIN tDepartment d ON b.DepartmentID = d.DepartmentID
        LEFT JOIN tUser approver ON e.ManagerUserID = approver.UserID
    WHERE e.IsDeleted = 0
GO

-- Retrieves all expenses based on budgetId
CREATE
	OR

ALTER VIEW vwGetExpensesByBudgetID
AS
    SELECT e.ExpenseID,
        e.BudgetID,
        e.CategoryID,
        c.CategoryName AS CategoryName,
        e.Title,
        e.Amount,
        e.MerchantName,
        e.STATUS,
        CASE e.STATUS
		WHEN 1
			THEN 'Pending'
		WHEN 2
			THEN 'Approved'
		WHEN 3
			THEN 'Rejected'
		WHEN 4
			THEN 'Cancelled'
		ELSE 'Unknown'
		END AS StatusName,
        e.SubmittedDate,
        e.SubmittedByUserID,
        submitter.FirstName + ' ' + submitter.LastName AS SubmittedByUserName,
        submitter.EmployeeID AS SubmittedByEmployeeID,
        e.ManagerUserID,
        approver.FirstName + ' ' + approver.LastName AS ApprovedByUserName,
        e.ApprovalComments,
        e.RejectionReason,
        e.Notes
    FROM tExpense e
        INNER JOIN tCategory c ON e.CategoryID = c.CategoryID
        INNER JOIN tUser submitter ON e.SubmittedByUserID = submitter.UserID
        LEFT JOIN tUser approver ON e.ManagerUserID = approver.UserID
    WHERE e.IsDeleted = 0
GO

-- Create Expense with Audit and Notification
CREATE
	OR

ALTER PROCEDURE uspCreateExpense
    @BudgetID INT,
    @CategoryID INT,
    @Title NVARCHAR(500),
    @Amount DECIMAL(18, 2),
    @MerchantName NVARCHAR(200) = NULL,
    @SubmittedByUserID INT,
    @Notes NVARCHAR(1000) = NULL,
    @NewExpenseID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
		DECLARE @ManagerID INT;
		DECLARE @BudgetTitle NVARCHAR(200);
		DECLARE @CategoryName NVARCHAR(100);

		-- Get manager ID and budget title
		SELECT @ManagerID = ManagerID
    FROM tUser
    WHERE UserID = @SubmittedByUserID
        AND IsDeleted = 0;

		SELECT @BudgetTitle = Title
    FROM tBudget
    WHERE BudgetID = @BudgetID
        AND IsDeleted = 0;

		-- Get category name
		SELECT @CategoryName = CategoryName
    FROM tCategory
    WHERE CategoryID = @CategoryID
        AND IsDeleted = 0;

		-- Validate budget exists
		IF @BudgetTitle IS NULL
		BEGIN
        RAISERROR ('Budget not found or has been deleted',
					16,
					1
					);

        RETURN;
    END

		-- Insert expense with Status = 1 (Pending)
		INSERT INTO tExpense
        (
        BudgetID,
        CategoryID,
        Title,
        Amount,
        MerchantName,
        SubmittedByUserID,
        SubmittedDate,
        Status,
        Notes,
        CreatedDate,
        IsDeleted
        )
    VALUES
        (
            @BudgetID,
            @CategoryID,
            @Title,
            @Amount,
            @MerchantName,
            @SubmittedByUserID,
            GETUTCDATE(),
            1,
            @Notes,
            GETUTCDATE(),
            0
			);

		SET @NewExpenseID = SCOPE_IDENTITY();

		-- Insert audit log with full entity JSON
		DECLARE @NewExpenseJSON NVARCHAR(MAX);

		SELECT @NewExpenseJSON = (
				SELECT ExpenseID,
            BudgetID,
            CategoryID,
            Title,
            Amount,
            MerchantName,
            SubmittedByUserID,
            SubmittedDate,
            Status,
            Notes,
            CreatedDate
        FROM tExpense
        WHERE ExpenseID = @NewExpenseID
        FOR JSON PATH,
					WITHOUT_ARRAY_WRAPPER
				);

		INSERT INTO tAuditLog
        (
        UserID,
        EntityType,
        EntityID,
        Action,
        NewValue,
        Description,
        CreatedDate
        )
    VALUES
        (
            @SubmittedByUserID,
            'Expense',
            @NewExpenseID,
            1, -- Create
            @NewExpenseJSON,
            'Expense created: ' + @Title + ' for ' + @BudgetTitle + ' - Rs.' + CAST(@Amount AS NVARCHAR(20)),
            GETUTCDATE()
			);

		-- Insert notification for manager (if exists)
		IF @ManagerID IS NOT NULL
		BEGIN
        INSERT INTO tNotification
            (
            SenderUserID,
            ReceiverUserID,
            Type,
            Message,
            Status,
            RelatedEntityType,
            RelatedEntityID,
            CreatedDate,
            IsDeleted
            )
        VALUES
            (
                @SubmittedByUserID,
                @ManagerID,
                1, -- ExpenseApprovalReminder
                'New expense pending approval: ' + @Title + ' for ' + @BudgetTitle + ' - Rs.' + CAST(@Amount AS NVARCHAR(20)),
                1, -- Unread
                'Expense',
                @NewExpenseID,
                GETUTCDATE(),
                0
				);
    END

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
GO

--  Update Expense Status (Approve/Reject)
CREATE
	OR

ALTER PROCEDURE uspUpdateExpenseStatus
    @ExpenseID INT,
    @Status INT,
    @ManagerUserID INT,
    @ApprovalComments NVARCHAR(1000) = NULL,
    @RejectionReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
		DECLARE @BudgetID INT;
		DECLARE @BudgetTitle NVARCHAR(200);
		DECLARE @BudgetCode NVARCHAR(50);
		DECLARE @CategoryName NVARCHAR(100);
		DECLARE @Title NVARCHAR(500);
		DECLARE @Amount DECIMAL(18, 2);
		DECLARE @SubmittedByUserID INT;
		DECLARE @CurrentStatus INT;
		DECLARE @OldAmountSpent DECIMAL(18, 2);
		DECLARE @OldAmountRemaining DECIMAL(18, 2);
		DECLARE @AmountAllocated DECIMAL(18, 2);
		DECLARE @OldExpenseJSON NVARCHAR(MAX);
		DECLARE @NewExpenseJSON NVARCHAR(MAX);

		-- Get expense details
		SELECT @BudgetID = e.BudgetID,
        @Title = e.Title,
        @Amount = e.Amount,
        @SubmittedByUserID = e.SubmittedByUserID,
        @CurrentStatus = e.STATUS,
        @BudgetTitle = b.Title,
        @BudgetCode = b.Code,
        @CategoryName = c.CategoryName
    FROM tExpense e
        INNER JOIN tBudget b ON e.BudgetID = b.BudgetID
        INNER JOIN tCategory c ON e.CategoryID = c.CategoryID
    WHERE e.ExpenseID = @ExpenseID
        AND e.IsDeleted = 0;

		-- Validate expense exists
		IF @BudgetID IS NULL
		BEGIN
        RAISERROR (
					'Expense not found or has been deleted',
					16,
					1
					);

        RETURN;
    END

		-- Validate current status allows modification (Pending, Approved, or Rejected)
		IF @CurrentStatus NOT IN (1, 2, 3)
		BEGIN
        RAISERROR (
					'Expense cannot be updated in its current state',
					16,
					1
					);

        RETURN;
    END

		-- Validate incoming status is either Approved or Rejected
		IF @Status NOT IN (2, 3)
		BEGIN
        RAISERROR (
					'Invalid status. Use 2 (Approved) or 3 (Rejected)',
					16,
					1
					);

        RETURN;
    END

		-- Capture current comments and reason for no-changes comparison
		DECLARE @CurrentApprovalComments NVARCHAR(1000);
		DECLARE @CurrentRejectionReason NVARCHAR(500);

		SELECT @CurrentApprovalComments = ApprovalComments,
        @CurrentRejectionReason = RejectionReason
    FROM tExpense
    WHERE ExpenseID = @ExpenseID;

		-- Prevent no-change updates (same status + same comments + same reason)
		IF @CurrentStatus = @Status
        AND ISNULL(@ApprovalComments, '') = ISNULL(@CurrentApprovalComments, '')
        AND ISNULL(@RejectionReason, '') = ISNULL(@CurrentRejectionReason, '')
		BEGIN
        RAISERROR (
					'No changes detected. The expense data is identical to the existing record.',
					16,
					1
					);

        RETURN;
    END

		-- Capture old state BEFORE the update
		SELECT @OldExpenseJSON = (
				SELECT ExpenseID,
            BudgetID,
            CategoryID,
            Title,
            Amount,
            MerchantName,
            SubmittedByUserID,
            SubmittedDate,
            Status,
            ManagerUserID,
            StatusApprovedDate,
            ApprovalComments,
            RejectionReason,
            Notes,
            CreatedDate,
            UpdatedDate
        FROM tExpense
        WHERE ExpenseID = @ExpenseID
        FOR JSON PATH,
					WITHOUT_ARRAY_WRAPPER
				);

		-- Update expense status
		UPDATE tExpense
		SET STATUS = @Status,
			ManagerUserID = @ManagerUserID,
			StatusApprovedDate = CASE 
				WHEN @Status = 2
					THEN GETUTCDATE()
				ELSE NULL
				END,
			ApprovalComments = @ApprovalComments,
			RejectionReason = @RejectionReason,
			UpdatedByUserID = @ManagerUserID,
			UpdatedDate = GETUTCDATE()
		WHERE ExpenseID = @ExpenseID;

		-- Capture new state AFTER the update
		SELECT @NewExpenseJSON = (
				SELECT ExpenseID,
            BudgetID,
            CategoryID,
            Title,
            Amount,
            MerchantName,
            SubmittedByUserID,
            SubmittedDate,
            Status,
            ManagerUserID,
            StatusApprovedDate,
            ApprovalComments,
            RejectionReason,
            Notes,
            CreatedDate,
            UpdatedDate
        FROM tExpense
        WHERE ExpenseID = @ExpenseID
        FOR JSON PATH,
					WITHOUT_ARRAY_WRAPPER
				);

		-- Budget amount adjustments based on status transition
		-- Case 1: Newly Approved (was Pending or Rejected) → add amount to budget spent
		-- Case 2: Changed to Rejected (was Approved)       → reverse amount from budget spent
		IF (
				@Status = 2
        AND @CurrentStatus != 2
				)
        OR (
				@Status = 3
        AND @CurrentStatus = 2
				)
		BEGIN
        -- Get current budget amounts
        SELECT @OldAmountSpent = AmountSpent,
            @OldAmountRemaining = AmountRemaining,
            @AmountAllocated = AmountAllocated
        FROM tBudget
        WHERE BudgetID = @BudgetID;

        -- Update budget: add if approving, subtract if rejecting a previously approved expense
        UPDATE tBudget
			SET AmountSpent = CASE 
					WHEN @Status = 2
						THEN @OldAmountSpent + @Amount
					ELSE @OldAmountSpent - @Amount
					END,
				AmountRemaining = CASE 
					WHEN @Status = 2
						THEN CASE 
								WHEN (@OldAmountSpent + @Amount) > @AmountAllocated
									THEN 0
								ELSE @AmountAllocated - (@OldAmountSpent + @Amount)
								END
					ELSE @OldAmountRemaining + @Amount
					END,
				UpdatedByUserID = @ManagerUserID,
				UpdatedDate = GETUTCDATE()
			WHERE BudgetID = @BudgetID;

        -- Insert audit log for budget update with JSON
        DECLARE @OldBudgetJSON NVARCHAR(MAX);
        DECLARE @NewBudgetJSON NVARCHAR(MAX);

        SET @OldBudgetJSON = (
					SELECT @AmountAllocated AS AmountAllocated,
            @OldAmountSpent AS AmountSpent,
            @OldAmountRemaining AS AmountRemaining
        FOR JSON PATH,
						WITHOUT_ARRAY_WRAPPER
					);
        SET @NewBudgetJSON = (
					SELECT @AmountAllocated AS AmountAllocated,
            CASE 
							WHEN @Status = 2
								THEN @OldAmountSpent + @Amount
							ELSE @OldAmountSpent - @Amount
							END AS AmountSpent,
            CASE 
							WHEN @Status = 2
								THEN CASE 
										WHEN (@OldAmountSpent + @Amount) > @AmountAllocated
											THEN 0
										ELSE @AmountAllocated - (@OldAmountSpent + @Amount)
										END
							ELSE @OldAmountRemaining + @Amount
							END AS AmountRemaining
        FOR JSON PATH,
						WITHOUT_ARRAY_WRAPPER
					);

        INSERT INTO tAuditLog
            (
            UserID,
            EntityType,
            EntityID,
            Action,
            OldValue,
            NewValue,
            Description,
            CreatedDate
            )
        VALUES
            (
                @ManagerUserID,
                'Budget',
                @BudgetID,
                2, -- Update
                @OldBudgetJSON,
                @NewBudgetJSON,
                'Budget: ' + ISNULL(@BudgetCode, '') + ' ' + @BudgetTitle + ' Expense - Updated',
                GETUTCDATE()
				);
    END

		-- Insert audit log for expense
		INSERT INTO tAuditLog
        (
        UserID,
        EntityType,
        EntityID,
        Action,
        OldValue,
        NewValue,
        Description,
        CreatedDate
        )
    VALUES
        (
            @ManagerUserID,
            'Expense',
            @ExpenseID,
            2, -- Update
            @OldExpenseJSON,
            @NewExpenseJSON,
            'Expense updated: ' + @Title + ' for ' + @BudgetTitle + ' - ' + CASE 
				WHEN @Status = 2
					THEN 'Approved'
				ELSE 'Rejected'
				END,
            GETUTCDATE()
			);

		-- Insert notification to employee
		INSERT INTO tNotification
        (
        SenderUserID,
        ReceiverUserID,
        Type,
        Message,
        STATUS,
        RelatedEntityType,
        RelatedEntityID,
        CreatedDate,
        IsDeleted
        )
    VALUES
        (
            @ManagerUserID,
            @SubmittedByUserID,
            CASE 
				WHEN @Status = 2
					THEN 2 -- NotificationType.ExpenseApproved
				WHEN @Status = 3
					THEN 3 -- NotificationType.ExpenseRejected
				END,
            CASE 
				WHEN @Status = 2
					THEN 'Expense Approved: ' + @Title + ' for ' + @BudgetTitle
						+ CASE 
							WHEN @ApprovalComments IS NOT NULL
								THEN '. Comments: ' + @ApprovalComments
							ELSE ''
							END
				WHEN @Status = 3
					THEN 'Expense Rejected: ' + @Title + ' for ' + @BudgetTitle
						+ CASE 
							WHEN @RejectionReason IS NOT NULL
								THEN '. Reason: ' + @RejectionReason
							ELSE ''
							END
				END,
            1, -- NotificationStatus.Unread
            'Expense',
            @ExpenseID,
            GETUTCDATE(),
            0
			);

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
GO


