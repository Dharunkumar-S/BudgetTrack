USE [Budget-Track];
GO

-- Get Notifications by ReceiverUserID with status filter and OUTPUT total count
CREATE OR ALTER PROCEDURE uspGetNotificationsByReceiverUserId
    @ReceiverUserID INT,
    @Message        NVARCHAR(500) = NULL,
    @Status         NVARCHAR(10)  = NULL,
    @SortOrder      NVARCHAR(10)  = 'desc',
    @PageNumber     INT           = 1,
    @PageSize       INT           = 10,
    @TotalRecords   INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Resolve status string to integer (Status=0 treated as Unread defensively)
        DECLARE @StatusInt INT = NULL;
        IF @Status = 'unread' SET @StatusInt = 1;
        IF @Status = 'read'   SET @StatusInt = 2;

        -- Total matching records before pagination
        SELECT @TotalRecords = COUNT(*)
    FROM tNotification n
    WHERE
            n.ReceiverUserID = @ReceiverUserID
        AND n.IsDeleted  = 0
        AND (
                @StatusInt IS NULL
        OR n.Status = @StatusInt
        OR (@StatusInt = 1 AND n.Status = 0)
            )
        AND (@Message IS NULL OR n.Message LIKE '%' + @Message + '%');

        -- Paged result
        SELECT
        n.NotificationID,
        n.Type,
        n.Message,
        n.Status,
        n.CreatedDate,
        ISNULL(sender.EmployeeID, 'SYSTEM')                         AS SenderEmployeeID,
        ISNULL(sender.FirstName + ' ' + sender.LastName, 'System') AS SenderName
    FROM tNotification n
        LEFT JOIN tUser sender ON n.SenderUserID = sender.UserID
    WHERE
            n.ReceiverUserID = @ReceiverUserID
        AND n.IsDeleted  = 0
        AND (
                @StatusInt IS NULL
        OR n.Status = @StatusInt
        OR (@StatusInt = 1 AND n.Status = 0)
            )
        AND (@Message IS NULL OR n.Message LIKE '%' + @Message + '%')
    ORDER BY
            CASE WHEN @SortOrder = 'asc'  THEN n.CreatedDate END ASC,
            CASE WHEN @SortOrder != 'asc' THEN n.CreatedDate END DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Get Unread Notification Count by ReceiverUserID
CREATE OR ALTER PROCEDURE uspGetUnreadNotificationCount
    @ReceiverUserID INT,
    @UnreadCount    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT @UnreadCount = COUNT(*)
    FROM tNotification
    WHERE
            ReceiverUserID = @ReceiverUserID
        AND IsDeleted  = 0
        AND Status     IN (0, 1);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Mark Single Notification as Read
CREATE OR ALTER PROCEDURE uspMarkNotificationAsRead
    @NotificationID INT,
    @ReceiverUserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Validate notification exists and belongs to user
        IF @ReceiverUserID IS NOT NULL
        BEGIN
        IF NOT EXISTS (
                SELECT 1
        FROM tNotification
        WHERE NotificationID = @NotificationID
            AND ReceiverUserID = @ReceiverUserID
            AND IsDeleted = 0
            )
            BEGIN
            RAISERROR('Notification not found or does not belong to user', 16, 1);
            RETURN;
        END
    END
        ELSE
        BEGIN
        IF NOT EXISTS (
                SELECT 1
        FROM tNotification
        WHERE NotificationID = @NotificationID
            AND IsDeleted = 0
            )
            BEGIN
            RAISERROR('Notification not found', 16, 1);
            RETURN;
        END
    END

        -- Mark as read
        UPDATE tNotification
        SET
            Status = 2, -- Read
            ReadDate = GETUTCDATE()
        WHERE NotificationID = @NotificationID
        AND Status = 1; -- Only update if currently unread

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Mark All Notifications as Read
CREATE OR ALTER PROCEDURE uspMarkAllNotificationsAsRead
    @ReceiverUserID INT,
    @UpdatedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Mark all unread notifications as read
        UPDATE tNotification
        SET
            Status = 2, -- Read (enum value)
            ReadDate = GETUTCDATE()
        WHERE
            ReceiverUserID = @ReceiverUserID
        AND Status IN (0, 1) -- Unread (enum value, 0 treated as Unread defensively)
        AND IsDeleted = 0;

        SET @UpdatedCount = @@ROWCOUNT;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Delete Notification (Soft Delete)
CREATE OR ALTER PROCEDURE uspDeleteNotification
    @NotificationID INT,
    @ReceiverUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @OldValue NVARCHAR(MAX), @Message NVARCHAR(500);

        -- Capture notification state before deletion as JSON
        SELECT
        @OldValue = (
                SELECT
            NotificationID, Type, Message, Status,
            CAST(0 AS BIT) AS IsDeleted
        FROM tNotification
        WHERE NotificationID = @NotificationID AND IsDeleted = 0
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            ),
        @Message = Message
    FROM tNotification
    WHERE NotificationID = @NotificationID AND IsDeleted = 0;

        -- Validate notification exists and is not already deleted
        IF @OldValue IS NULL
        BEGIN
        RAISERROR('Notification not found or has already been deleted', 16, 1);
        RETURN;
    END

        -- Check if the user owns this notification
        IF NOT EXISTS (
            SELECT 1
    FROM tNotification
    WHERE NotificationID = @NotificationID
        AND ReceiverUserID = @ReceiverUserID
        AND IsDeleted = 0
        )
        BEGIN
        RAISERROR('Notification does not belong to user', 16, 1);
        RETURN;
    END

        -- Soft delete the notification
        UPDATE tNotification
        SET
            IsDeleted = 1,
            DeletedDate = GETUTCDATE()
        WHERE NotificationID = @NotificationID AND IsDeleted = 0;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
    ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Delete All Notifications (Soft Delete)
CREATE OR ALTER PROCEDURE uspDeleteAllNotifications
    @ReceiverUserID INT,
    @DeletedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Soft delete all active notifications for the user
        UPDATE tNotification
        SET 
            IsDeleted = 1,
            DeletedDate = GETUTCDATE()
        WHERE 
            ReceiverUserID = @ReceiverUserID
        AND IsDeleted = 0;

        SET @DeletedCount = @@ROWCOUNT;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
