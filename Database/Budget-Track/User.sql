USE [Budget-Track];
GO

-- Retrieves user profile
CREATE OR ALTER VIEW vwGetUserProfile
AS
    SELECT
        u.UserID,
        u.FirstName,
        u.LastName,
        u.Email,
        u.EmployeeID,
        u.DepartmentID,
        d.DepartmentName,
        u.ManagerID,
        m.EmployeeID AS ManagerEmployeeId,
        CONCAT(m.FirstName, ' ', m.LastName) AS ManagerName,
        u.RoleID,
        r.RoleName,
        u.Status,
        CASE WHEN u.Status = 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsActive,
        u.CreatedDate,
        u.UpdatedDate,
        u.IsDeleted
    FROM tUser u
        INNER JOIN tDepartment d ON u.DepartmentID = d.DepartmentID
        INNER JOIN tRole r ON u.RoleID = r.RoleID
        LEFT JOIN tUser m ON u.ManagerID = m.UserID
    WHERE u.IsDeleted = 0
GO

-- Retrieves authenticated user's profile information
CREATE OR ALTER PROCEDURE dbo.uspGetUserProfile
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.UserID AS UserId,
        u.FirstName,
        u.LastName,
        u.Email,
        u.EmployeeID AS EmployeeId,
        u.DepartmentID AS DepartmentId,
        d.DepartmentName,
        u.ManagerID AS ManagerId,
        m.EmployeeID AS ManagerEmployeeId,
        CONCAT(m.FirstName, ' ', m.LastName) AS ManagerName,
        u.RoleID AS RoleId,
        r.RoleName,
        u.Status
    FROM tUser u
        INNER JOIN tDepartment d ON u.DepartmentID = d.DepartmentID
        INNER JOIN tRole r ON u.RoleID = r.RoleID
        LEFT JOIN tUser m ON u.ManagerID = m.UserID
    WHERE u.UserID = @UserId
        AND u.IsDeleted = 0;
END
GO

-- Retrieves paginated list of users with filtering support
CREATE OR ALTER PROCEDURE dbo.uspGetUsersList
    @RoleId INT = NULL,
    @EmployeeId NVARCHAR(50) = NULL,
    @IsDeleted BIT = NULL,
    @IsActive BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM tUser u
        INNER JOIN tDepartment d ON u.DepartmentID = d.DepartmentID
        INNER JOIN tRole r ON u.RoleID = r.RoleID
        LEFT JOIN tUser m ON u.ManagerID = m.UserID
    WHERE 
        (@RoleId IS NULL OR u.RoleID = @RoleId)
        AND (@EmployeeId IS NULL OR u.EmployeeID LIKE '%' + @EmployeeId + '%')
        AND (@IsDeleted IS NULL OR u.IsDeleted = @IsDeleted)
        AND (@IsActive IS NULL OR (CASE WHEN u.Status = 1 THEN 1 ELSE 0 END) = @IsActive);

    -- Get paginated results
    SELECT
        u.UserID AS UserId,
        u.FirstName,
        u.LastName,
        u.Email,
        u.EmployeeID AS EmployeeId,
        u.DepartmentID AS DepartmentId,
        d.DepartmentName,
        u.ManagerID AS ManagerId,
        m.EmployeeID AS ManagerEmployeeId,
        CONCAT(m.FirstName, ' ', m.LastName) AS ManagerName,
        u.RoleID AS RoleId,
        r.RoleName,
        u.Status,
        CASE WHEN u.Status = 1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsActive,
        u.IsDeleted,
        u.CreatedDate,
        u.UpdatedDate
    FROM tUser u
        INNER JOIN tDepartment d ON u.DepartmentID = d.DepartmentID
        INNER JOIN tRole r ON u.RoleID = r.RoleID
        LEFT JOIN tUser m ON u.ManagerID = m.UserID
    WHERE 
        (@RoleId IS NULL OR u.RoleID = @RoleId)
        AND (@EmployeeId IS NULL OR u.EmployeeID LIKE '%' + @EmployeeId + '%')
        AND (@IsDeleted IS NULL OR u.IsDeleted = @IsDeleted)
        AND (@IsActive IS NULL OR (CASE WHEN u.Status = 1 THEN 1 ELSE 0 END) = @IsActive)
    ORDER BY u.CreatedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO