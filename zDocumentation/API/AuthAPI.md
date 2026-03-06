## Auth API

All routes are served from `http://localhost:5131/api/auth` and handle authentication, user registration, password management, and token operations.

---

### Register User (Admin Only)

`POST http://localhost:5131/api/auth/createuser`

#### Access: Admin only (JWT Bearer)

**Description:** Admin-only endpoint for registering new employees, managers, or other admins in the system. Employees cannot self-register. This endpoint handles user creation with automatic password hashing using BCrypt, assigns roles and departments, and creates comprehensive audit trails. The Employee ID is auto-generated based on the assigned role: `EMP` + 4-digit sequence for Employees (e.g., `EMP0001`), `MGR` + 4-digit sequence for Managers (e.g., `MGR0001`), and `ADM` + 4-digit sequence for Admins (e.g., `ADM0001`).

#### Request Body (Creating Employee - roleID: 3)
```json
{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@company.com",
    "password": "SecurePass123!",
    "roleID": 3,
    "departmentID": 1,
    "managerEmployeeId": "MGR0001"
}
```

#### Request Body (Creating Manager - roleID: 2)
```json
{
    "firstName": "Sarah",
    "lastName": "Johnson",
    "email": "sarah.johnson@company.com",
    "password": "Manager@123!",
    "roleID": 2,
    "departmentID": 2,
    "managerEmployeeId": null
}
```

#### Request Body (Creating Admin - roleID: 1)
```json
{
    "firstName": "Michael",
    "lastName": "Chen",
    "email": "michael.chen@company.com",
    "password": "Admin@123!",
    "roleID": 1,
    "departmentID": 1,
    "managerEmployeeId": null
}
```

#### Status Codes

| Code | Status                | Message                            |
| ---- | --------------------- | ---------------------------------- |
| 201  | Created               | User registered successfully       |
| 400  | Bad Request           | Validation or business logic error |
| 401  | Unauthorized          | User not authenticated             |
| 403  | Forbidden             | Forbidden                          |
| 500  | Internal Server Error | Registration failed                |

#### Response Body

**201 Created**
```json
{
    "success": true,
    "message": "User registered successfully as Employee",
    "user": {
        "id": 4,
        "employeeId": "EMP2601",
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@company.com",
        "departmentID": 1,
        "departmentName": "Engineering Operations",
        "roleID": 3,
        "roleName": "Employee",
        "managerID": 2,
        "managerName": "Sanika Anil",
        "status": 1,
        "createdDate": "2026-02-25T10:30:00.1234567",
        "updatedDate": "2026-02-25T10:30:00.1234567"
    },
    "token": null
}
```

**400 Bad Request (Email Already Exists)**
```json
{
    "success": false,
    "message": "Email already registered"
}
```

**400 Bad Request (Manager Not Found)**
```json
{
    "success": false,
    "message": "Invalid manager assignment: manager not found or not a Manager role"
}
```

---

### Login User

`POST http://localhost:5131/api/auth/login`

#### Access: Public (No authentication required)

**Description:** Authenticates users and issues JWT access and refresh tokens for subsequent API requests. Validates user credentials against BCrypt password hashes, verifies active status, and generates tokens. Access token is valid for 15 minutes, refresh token for 7 days. Includes user profile information and token details in response.

#### Request Body
```json
{
    "email": "john.doe@company.com",
    "password": "SecurePass123!"
}
```

#### Status Codes

| Code | Status                | Message             |
| ---- | --------------------- | ------------------- |
| 200  | OK                    | Login successful    |
| 400  | Bad Request           | Validation error    |
| 401  | Unauthorized          | Invalid credentials |
| 500  | Internal Server Error | Login failed        |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Login successful",
    "user": {
        "id": 3,
        "employeeId": "EMP2601",
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@company.com",
        "departmentID": 1,
        "departmentName": "Engineering Operations",
        "roleID": 3,
        "roleName": "Employee",
        "managerID": 2,
        "managerName": "Sanika Anil",
        "status": 1,
        "createdDate": "2026-02-25T01:24:20.3843916",
        "updatedDate": "2026-02-25T01:24:20.3843916"
    },
    "token": {
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        "refreshToken": "c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8...",
        "expiresAt": "2026-02-25T07:11:30.4567890Z",
        "expiresIn": 900,
        "tokenType": "Bearer"
    }
}
```

**401 Unauthorized**
```json
{
    "success": false,
    "message": "Invalid email or password"
}
```

---

### Change Password

`POST http://localhost:5131/api/auth/changepassword`

#### Access: All authenticated users (JWT Bearer)

**Description:** Allows authenticated users to change their password. Validates the old password against the stored BCrypt hash, verifies the new password meets security requirements, and updates the password hash with audit logging.

#### Request Body
```json
{
    "oldPassword": "TempPassword123!",
    "newPassword": "MyNewSecurePass456!"
}
```

#### Status Codes

| Code | Status                | Message                   |
| ---- | --------------------- | ------------------------- |
| 200  | OK                    | Password changed          |
| 400  | Bad Request           | Validation or logic error |
| 401  | Unauthorized          | Unauthorized              |
| 500  | Internal Server Error | Password change failed    |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Password changed successfully"
}
```

**400 Bad Request (Wrong Old Password)**
```json
{
    "success": false,
    "message": "Current password is incorrect"
}
```

---

### Refresh Token

`POST http://localhost:5131/api/auth/token/refresh`

#### Access: Public (No authentication required)

**Description:** Generates a new access token using a valid refresh token. Allows clients to obtain a new short-lived access token without requiring the user to log in again. Refresh tokens are valid for 7 days.

#### Request Body
```json
{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8..."
}
```

#### Status Codes

| Code | Status                | Message                  |
| ---- | --------------------- | ------------------------ |
| 200  | OK                    | Token refreshed          |
| 401  | Unauthorized          | Invalid or expired token |
| 500  | Internal Server Error | Token refresh failed     |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Token refreshed successfully",
    "token": {
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        "refreshToken": "c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8...",
        "expiresAt": "2026-02-25T07:15:30.1234567Z",
        "expiresIn": 900,
        "tokenType": "Bearer"
    }
}
```

---

### Logout

`POST http://localhost:5131/api/auth/logout`

#### Access: All authenticated users (JWT Bearer)

**Description:** Revokes the user's token and logs them out of the system. The token is invalidated and cannot be used for subsequent API requests.

#### Status Codes

| Code | Status                | Message       |
| ---- | --------------------- | ------------- |
| 200  | OK                    | Logged out    |
| 401  | Unauthorized          | Unauthorized  |
| 500  | Internal Server Error | Logout failed |

#### Response Body

**200 OK**
```json
{
    "message": "Logged out successfully"
}
```

---

### Verify Token

`GET http://localhost:5131/api/auth/verify`

#### Access: All authenticated users (JWT Bearer)

**Description:** Verifies that the current JWT token is valid and the user is authenticated. Returns the user's ID if token is valid.

#### Status Codes

| Code | Status       | Message        |
| ---- | ------------ | -------------- |
| 200  | OK           | Token is valid |
| 401  | Unauthorized | Invalid token  |

#### Response Body

**200 OK**
```json
{
    "message": "Token is valid",
    "userId": 3
}
```

---

### Get User Profile

`GET http://localhost:5131/api/users/profile`

#### Access: Admin, Manager, Employee (JWT Bearer)

**Description:** Retrieves the authenticated user's complete profile information based on JWT claims. Returns current user's details including department, role, and manager information.

#### Status Codes

| Code | Status                | Message                    |
| ---- | --------------------- | -------------------------- |
| 200  | OK                    | Profile retrieved          |
| 401  | Unauthorized          | Unauthorized               |
| 404  | Not Found             | User profile not found     |
| 500  | Internal Server Error | Failed to retrieve profile |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "data": {
        "id": 2,
        "employeeId": "MGR2601",
        "firstName": "Sanika",
        "lastName": "Anil",
        "email": "sanika.anil@company.com",
        "departmentID": 1,
        "departmentName": "Engineering Operations",
        "roleID": 2,
        "roleName": "Manager",
        "managerID": null,
        "managerName": null,
        "managerEmployeeId": null,
        "status": 1,
        "createdDate": "2026-02-25T01:24:20.3843916",
        "updatedDate": "2026-02-25T01:24:20.3843916"
    }
}
```

---

### Get Users List

`GET http://localhost:5131/api/users`

#### Query Parameters

| Parameter      | Type   | Required | Description                                        |
| -------------- | ------ | -------- | -------------------------------------------------- |
| `roleId`       | int    | No       | Filter by role ID (1=Admin, 2=Manager, 3=Employee) |
| `search`       | string | No       | Search by employee ID or name                      |
| `departmentId` | int    | No       | Filter by department ID                            |
| `isDeleted`    | bool   | No       | Filter by deletion status                          |
| `isActive`     | bool   | No       | Filter by active status                            |
| `sortBy`       | string | No       | Sort field (default: "CreatedDate")                |
| `sortOrder`    | string | No       | Sort direction: "asc" or "desc" (default: "desc")  |
| `pageNumber`   | int    | No       | Page number (default: 1)                           |
| `pageSize`     | int    | No       | Records per page (default: 10)                     |

#### Access: Admin, Manager (JWT Bearer)

**Description:** Retrieves a paginated list of users. Managers can only view employees reporting directly to them. Supports filtering, searching, and sorting.

#### Status Codes

| Code | Status                | Message                 |
| ---- | --------------------- | ----------------------- |
| 200  | OK                    | Users list retrieved    |
| 401  | Unauthorized          | Unauthorized            |
| 500  | Internal Server Error | Failed to retrieve list |

#### Response Body

**200 OK**
```json
{
    "data": [
        {
            "id": 7,
            "employeeId": "EMP2601",
            "firstName": "John",
            "lastName": "Doe",
            "email": "john.doe@company.com",
            "departmentID": 1,
            "departmentName": "Engineering Operations",
            "roleID": 3,
            "roleName": "Employee",
            "managerID": 2,
            "managerName": "Sanika Anil",
            "managerEmployeeId": "MGR2601",
            "status": 1,
            "createdDate": "2026-02-26T10:00:00",
            "updatedDate": "2026-02-26T10:00:00"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 5,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
}
```

---

### Update User

`PUT http://localhost:5131/api/users/{userId}`

#### Route Parameters

| Parameter | Type | Required | Description      |
| --------- | ---- | -------- | ---------------- |
| `userId`  | int  | Yes      | Internal user ID |

#### Access: Admin only (JWT Bearer)

**Description:** Updates a user's profile by internal userId. Admin can modify first name, last name, role, department, email, password, and active status. Email and password are optional fields.

#### Request Body
```json
{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.newemail@company.com",
    "password": "NewSecurePass@123",
    "roleId": 3,
    "departmentId": 1,
    "isActive": true
}
```

#### Status Codes

| Code | Status                | Message                   |
| ---- | --------------------- | ------------------------- |
| 200  | OK                    | User updated successfully |
| 400  | Bad Request           | Validation error          |
| 401  | Unauthorized          | Unauthorized              |
| 403  | Forbidden             | Forbidden                 |
| 404  | Not Found             | User not found            |
| 500  | Internal Server Error | Failed to update user     |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "User updated successfully"
}
```

**400 Bad Request (No Changes)**
```json
{
    "success": false,
    "message": "No changes made"
}
```
```json
{
    "firstName": "John",
    "lastName":"Doe",
    "email": "john.doe@company.com",
    "password": "SecurePass123!",
    "roleID": 3,
    "departmentID": 1,
    "managerEmployeeId": "MGR0001"
}
```

### Request Body (Creating Manager - roleID: 2)
```json
{
    "firstName": "Sarah",
    "lastName":"Johnson",
    "email": "sarah.johnson@company.com",
    "password": "Manager@123!",
    "roleID": 2,
    "departmentID": 2,
    "managerEmployeeId": null
}
```
### Request Body (Creating Admin - roleID: 1)
```json
{
    "firstName": "Sarah",
    "lastName":"Johnson",
    "email": "sarah.johnson@company.com",
    "password": "Manager@123!",
    "roleID": 1,
    "departmentID": 1,
    "managerEmployeeId": null
}
```

### Status Codes

| Code | Status                | Message                                                    |
| ---- | --------------------- | ---------------------------------------------------------- |
| 201  | Created               | User registered successfully as {Role}                     |
| 400  | Bad Request           | Multiple validation messages (see response examples below) |
| 401  | Unauthorized          | User not authenticated                                     |
| 403  | Forbidden             | Forbidden                                                  |
| 500  | Internal Server Error | Registration failed                                        |

### Response Body

**201 Created (Employee Role - roleID: 3)**
```json
{
    "success": true,
    "message": "User registered successfully as Employee",
    "user": {
        "id": 4,
        "employeeId": "EMP2601",
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@company.com",
        "departmentID": 1,
        "departmentName": "Engineering Operations",
        "roleID": 3,
        "roleName": "Employee",
        "managerID": 2,
        "managerName": "Sanika Anil",
        "status": 1,
        "createdDate": "2026-02-25T10:30:00.1234567",
        "updatedDate": "2026-02-25T10:30:00.1234567"
    },
    "token": null
}
```
*Note: Returned when AdminRegisterUserAsync successfully creates a new Employee (roleID = 3). Employee users must have a manager assigned, so managerID and managerName are populated. The password is hashed using BCrypt, and an audit log entry is created. Employees are the standard users who report to managers and can create budgets/expenses under their manager's oversight.*

**201 Created (Manager Role - roleID: 2)**
```json
{
    "success": true,
    "message": "User registered successfully as Manager",
    "user": {
        "id": 5,
        "employeeId": "MGR2603",
        "firstName": "Sarah",
        "lastName": "Johnson",
        "email": "sarah.johnson@company.com",
        "departmentID": 2,
        "departmentName": "Finance",
        "roleID": 2,
        "roleName": "Manager",
        "managerID": null,
        "managerName": null,
        "status": 1,
        "createdDate": "2026-02-25T11:15:00.7654321",
        "updatedDate": "2026-02-25T11:15:00.7654321"
    },
    "token": null
}
```
*Note: Returned when AdminRegisterUserAsync successfully creates a new Manager (roleID = 2). Manager users do not have a manager assigned (managerID and managerName are null) as they are supervisory roles. Managers can oversee employees, approve budgets/expenses, and have department-level oversight. The managerEmployeeId field in the request can be omitted or set to null for managers.*

**201 Created (Admin Role - roleID: 1)**
```json
{
    "success": true,
    "message": "User registered successfully as Admin",
    "user": {
        "id": 6,
        "employeeId": "ADMIN2602",
        "firstName": "Michael",
        "lastName": "Chen",
        "email": "michael.chen@company.com",
        "departmentID": 1,
        "departmentName": "Engineering Operations",
        "roleID": 1,
        "roleName": "Admin",
        "managerID": null,
        "managerName": null,
        "status": 1,
        "createdDate": "2026-02-25T12:45:00.9876543",
        "updatedDate": "2026-02-25T12:45:00.9876543"
    },
    "token": null
}
```
*Note: Returned when AdminRegisterUserAsync successfully creates a new Admin (roleID = 1). Admin users have full system access including user management, all budget/expense operations, department management, and system configuration. Admins do not have managers (managerID is null). Only existing admins can create new admin accounts. This is the highest privilege level in the system.*

**400 Bad Request**
```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Email": [
            "The Email field is required.",
            "Invalid email format"
        ],
        "Password": [
            "Password must be at least 8 characters"
        ]
    },
    "traceId": "00-7ad91d1010b41dfaba5ca03a3854a031-3dc702851dd78dc1-00"
}
```
*Note: Triggered by ModelState validation when required fields are missing or invalid*

**401 Unauthorized**
```json
{
    "message": "User not authenticated"
}
```
*Note: Returned when JWT token is missing or invalid, or user ID cannot be extracted from claims*

**400 Bad Request (Service Validation)**
```json
{
    "success": false,
    "message": "Email already registered"
}
```
*Note: Triggered when AdminRegisterUserAsync detects duplicate email during uniqueness check*

**400 Bad Request (Duplicate Employee ID)**
```json
{
    "success": false,
    "message": "EmployeeId already registered"
}
```
*Note: Triggered when AdminRegisterUserAsync detects duplicate employee ID during uniqueness check*

**400 Bad Request (Invalid Manager)**
```json
{
    "success": false,
    "message": "Invalid manager assignment: manager not found or not a Manager role"
}
```
*Note: Triggered when ManagerEmployeeId doesn't exist or doesn't have Manager role (RoleID = 2)*

**400 Bad Request (Manager Required)**
```json
{
    "success": false,
    "message": "ManagerEmployeeId is required for Employee"
}
```
*Note: Triggered when creating Employee (RoleID = 3) without providing ManagerEmployeeId*

**400 Bad Request (Unauthorized Admin)**
```json
{
    "success": false,
    "message": "Only Admin users can register new employees/managers"
}
```
*Note: Triggered when authenticated user doesn't have Admin role (RoleID != 1)*

---

`POST http://localhost:5131/api/auth/login`

### Access Public (No authentication required)

**Note:** Authenticates users and issues JWT access and refresh tokens for subsequent API requests. The `LoginAsync` service method validates user credentials by comparing the provided password against the stored BCrypt hash, verifies the user's active status, and generates a pair of tokens: a short-lived access token (15 minutes) for API authorization and a long-lived refresh token (7 days) for obtaining new access tokens. The access token contains user claims including UserID, Email, EmployeeID, and Role for role-based authorization. Failed login attempts are logged for security monitoring. Supports all user roles (Admin, Manager, Employee) and returns comprehensive user profile information along with tokens.

### Request Body (Admin Login)
```json
{
    "email": "admin@budgettrack.com",
    "password": "Admin@123"
}
```

### Request Body (Manager Login)
```json
{
    "email": "sanika.anil@company.com",
    "password": "Manager@123"
}
```

### Request Body (Employee Login)
```json
{
    "email": "john.doe@company.com",
    "password": "Employee@123"
}
```

### Status Codes

| Code | Status                | Message                                    |
| ---- | --------------------- | ------------------------------------------ |
| 200  | OK                    | Login successful                           |
| 400  | Bad Request           | Bad request (invalid input)                |
| 401  | Unauthorized          | Invalid email or password / Account locked |
| 500  | Internal Server Error | Login failed                               |

### Response Body

**200 OK (Admin Login - roleID: 1)**
```json
{
    "success": true,
    "message": "Login successful",
    "user": {
        "id": 1,
        "employeeId": "ADMIN2601",
        "firstName": "Super",
        "lastName": "Admin",
        "email": "admin@budgettrack.com",
        "departmentID": 1,
        "departmentName": "Engineering Operations",
        "roleID": 1,
        "roleName": "Admin",
        "managerID": null,
        "managerName": null,
        "status": 1,
        "createdDate": "2026-02-25T01:24:20.3843916",
        "updatedDate": "2026-02-25T01:24:20.3843916"
    },
    "token": {
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJhZG1pbkBidWRnZXR0cmFjay5jb20iLCJlbXBsb3llZUlkIjoiQURNSU4yNjAxIiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzQwNDQ4OTcwLCJleHAiOjE3NDA0NDk4NzAsImlhdCI6MTc0MDQ0ODk3MH0.signature_here",
        "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6",
        "expiresAt": "2026-02-25T07:09:30.1234567Z",
        "expiresIn": 900,
        "tokenType": "Bearer"
    }
}
```
*Note: Admin login response. The JWT access token contains claims: UserID (sub=1), Email, EmployeeID (ADMIN2601), and Role (Admin). Admin users have no manager assignment (managerID is null). The token enables full system access including user management, all CRUD operations, and administrative functions. Access token expires in 15 minutes (900 seconds), refresh token valid for 7 days.*

**200 OK (Manager Login - roleID: 2)**
```json
{
    "success": true,
    "message": "Login successful",
    "user": {
        "id": 2,
        "employeeId": "MGR2601",
        "firstName": "Sanika",
        "lastName": "Anil",
        "email": "sanika.anil@company.com",
        "departmentID": 1,
        "departmentName": "Engineering Operations",
        "roleID": 2,
        "roleName": "Manager",
        "managerID": null,
        "managerName": null,
        "status": 1,
        "createdDate": "2026-02-25T01:24:20.3843916",
        "updatedDate": "2026-02-25T01:24:20.3843916"
    },
    "token": {
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiZW1haWwiOiJzYW5pa2EuYW5pbEBjb21wYW55LmNvbSIsImVtcGxveWVlSWQiOiJNR1IyNjAxIiwicm9sZSI6Ik1hbmFnZXIiLCJuYmYiOjE3NDA0NDkwMzAsImV4cCI6MTc0MDQ0OTkzMCwiaWF0IjoxNzQwNDQ5MDMwfQ.signature_here",
        "refreshToken": "b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6a7",
        "expiresAt": "2026-02-25T07:10:30.7891234Z",
        "expiresIn": 900,
        "tokenType": "Bearer"
    }
}
```
*Note: Manager login response. The JWT access token contains claims: UserID (sub=2), Email, EmployeeID (MGR2601), and Role (Manager). Manager users do not report to anyone (managerID is null). The token enables manager-level access including: viewing/managing their team's budgets and expenses, approving employee requests, department-level reporting, and oversight of employees under their supervision.*

**200 OK (Employee Login - roleID: 3)**
```json
{
    "success": true,
    "message": "Login successful",
    "user": {
        "id": 3,
        "employeeId": "EMP2601",
        "firstName": "Rahul",
        "lastName": "Sharma",
        "email": "rahul.sharma@company.com",
        "departmentID": 1,
        "departmentName": "Engineering Operations",
        "roleID": 3,
        "roleName": "Employee",
        "managerID": 2,
        "managerName": "Sanika Anil",
        "status": 1,
        "createdDate": "2026-02-25T01:24:20.3843916",
        "updatedDate": "2026-02-25T01:24:20.3843916"
    },
    "token": {
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiZW1haWwiOiJyYWh1bC5zaGFybWFAY29tcGFueS5jb20iLCJlbXBsb3llZUlkIjoiRU1QMjYwMSIsInJvbGUiOiJFbXBsb3llZSIsIm1hbmFnZXJJZCI6IjIiLCJuYmYiOjE3NDA0NDkwOTAsImV4cCI6MTc0MDQ0OTk5MCwiaWF0IjoxNzQwNDQ5MDkwfQ.signature_here",
        "refreshToken": "c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6a7b8",
        "expiresAt": "2026-02-25T07:11:30.4567890Z",
        "expiresIn": 900,
        "tokenType": "Bearer"
    }
}
```
*Note: Employee login response. The JWT access token contains claims: UserID (sub=3), Email, EmployeeID (EMP2601), Role (Employee), and ManagerID (2). Employee users have a manager assigned (managerID: 2, managerName: "Sanika Anil") establishing the reporting hierarchy. The token enables employee-level access: creating and viewing their own budgets/expenses, viewing budgets created by their manager, submitting requests for approval, and accessing department resources. Employees have the most restricted access level in the system.*

**401 Unauthorized**
```json
{
    "success": false,
    "message": "Invalid email or password"
}
```
*Note: Triggered when email doesn't exist, password doesn't match BCrypt hash, or user account is inactive*

**400 Bad Request (Validation errors)**
```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Email": [
            "Email is required",
            "Invalid email format"
        ],
        "Password": [
            "Password is required"
        ]
    },
    "traceId": "00-7ad91d1010b41dfaba5ca03a3854a031-3dc702851dd78dc1-00"
}
```

---

`POST http://localhost:5131/api/auth/resetpassword`

### Access Admin,Manager,Employee with Jwt bearer

**Note:** Allows authenticated users to change their password after logging in with a temporary password. The `ChangePasswordAsync` service method validates the old password against the stored BCrypt hash, verifies the new password meets security requirements, updates the password hash, and creates an audit log entry for security tracking. This endpoint requires the user to be authenticated with their current (temporary) password first, ensuring password changes are authorized. Commonly used during the first login flow when users need to change their initial system-generated password to a personal one.

### Request Body
```json
{
    "oldPassword": "TempPassword123!",
    "newPassword": "MyNewSecurePass456!"
}
```

### Status Codes

| Code | Status                | Message                |
| ---- | --------------------- | ---------------------- |
| 200  | OK                    | Password is changed    |
| 400  | Bad Request           | Invalid old password   |
| 401  | Unauthorized          | User not authenticated |
| 500  | Internal Server Error | Password change failed |

### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Password is changed"
}
```
*Note: Returned when ChangePasswordAsync successfully validates old password and updates to new BCrypt hash*

**400 Bad Request**
```json
{
    "success": false,
    "message": "Invalid old password"
}
```
*Note: Triggered when the provided old password doesn't match the current BCrypt hash in database*

**401 Unauthorized**
```json
{
    "message": "User not authenticated"
}
```
*Note: Returned when JWT token is missing, invalid, or user ID cannot be extracted from claims*

---

`POST http://localhost:5131/api/auth/token/refresh`

### Access Public (No authentication required)

**Note:** Refreshes expired access tokens using a valid refresh token without requiring the user to re-authenticate with credentials. The `RefreshTokenAsync` service method validates both the expired access token and the refresh token, verifies the refresh token hasn't been revoked and is still within its validity period (7 days), and issues a new pair of tokens with the same user claims. This enables seamless user experience by maintaining authenticated sessions without repeated logins. The old refresh token is typically revoked and replaced with a new one for security. Essential for single-page applications and mobile apps requiring long-lived sessions.

### Request Body
```json
{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6..."
}
```

### Status Codes

| Code | Status                | Message              |
| ---- | --------------------- | -------------------- |
| 200  | OK                    | Token refreshed      |
| 400  | Bad Request           | Invalid input        |
| 401  | Unauthorized          | Invalid token        |
| 500  | Internal Server Error | Token refresh failed |

### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Token refreshed",
    "token": {
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.new_token...",
        "refreshToken": "x1y2z3a4b5c6d7e8f9g0h1i2j3k4l5m6...",
        "expiresAt": "2026-02-25T08:30:00.0000000Z",
        "expiresIn": 900,
        "tokenType": "Bearer"
    }
}
```
*Note: Returned when RefreshTokenAsync successfully validates tokens and generates new token pair*

**401 Unauthorized**
```json
{
    "success": false,
    "message": "Invalid token"
}
```
*Note: Triggered when refresh token is revoked, expired, doesn't exist, or access token validation fails*

---

`POST http://localhost:5131/api/auth/logout`

### Access Admin,Manager,Employee with Jwt bearer

**Note:** Logs out the authenticated user by revoking their refresh token, preventing it from being used to obtain new access tokens. The `RevokeTokenAsync` service method invalidates the user's current refresh token in the database, effectively terminating their session. While the current access token remains technically valid until its expiration (15 minutes), the user cannot obtain new tokens without re-authenticating. This endpoint enhances security by ensuring users can explicitly terminate their sessions, especially important when logging out from shared devices or after completing sensitive operations.

### Status Codes

| Code | Status                | Message                 |
| ---- | --------------------- | ----------------------- |
| 200  | OK                    | Logged out successfully |
| 400  | Bad Request           | Logout failed           |
| 401  | Unauthorized          | User not authenticated  |
| 500  | Internal Server Error | Logout failed           |

### Response Body

**200 OK**
```json
{
    "message": "Logged out successfully"
}
```
*Note: Returned when RevokeTokenAsync successfully revokes the user's refresh token*

**401 Unauthorized**
```json
{
    "message": "User not authenticated"
}
```
*Note: Returned when JWT token is missing, invalid, or user ID cannot be extracted from HttpContext*

**400 Bad Request**
```json
{
    "message": "Logout failed"
}
```
*Note: Triggered when RevokeTokenAsync returns false, indicating token revocation failed*

---

`GET http://localhost:5131/api/auth/verify`

### Access Admin,Manager,Employee with Jwt bearer

**Note:** Verifies whether the user's current JWT access token is valid and the user is properly authenticated. This endpoint doesn't interact with the database or perform complex operations - it simply validates the JWT signature, expiration, and claims through the middleware. Returns the authenticated user's ID extracted from the token claims. Useful for client applications to check authentication status before making other API calls, implementing route guards in SPAs, or validating tokens after page refreshes. The JWT middleware handles all validation logic before the request reaches this endpoint.

### Status Codes

| Code | Status                | Message        |
| ---- | --------------------- | -------------- |
| 200  | OK                    | Token is valid |
| 401  | Unauthorized          | Invalid token  |
| 500  | Internal Server Error | Invalid token  |

### Response Body

**200 OK**
```json
{
    "message": "Token is valid",
    "userId": 1
}
```
*Note: Returned when JWT middleware successfully validates the token and extracts user ID from HttpContext*

**401 Unauthorized**
```json
{
    "message": "User not authenticated"
}
```
*Note: Triggered when JWT token is missing, expired, has invalid signature, or user ID cannot be extracted*

---

`GET http://localhost:5131/api/users/profile`

### Access Admin,Manager,Employee with Jwt bearer

**Note:** Retrieves the authenticated user's comprehensive profile information using the `uspGetUserProfile` stored procedure. Returns detailed user data including department assignment, role, manager details, and employment status. The procedure joins multiple tables (tUser, tDepartment, tRole) to provide a complete profile view. User ID is automatically extracted from JWT claims ensuring users can only access their own profile. Essential for profile pages, navigation bars, and personalization features. Excludes soft-deleted users and returns null if the user profile doesn't exist.

### Status Codes

| Code | Status                | Message                         |
| ---- | --------------------- | ------------------------------- |
| 200  | OK                    | Profile retrieved successfully  |
| 401  | Unauthorized          | User not authenticated          |
| 404  | Not Found             | User profile not found          |
| 500  | Internal Server Error | Failed to retrieve user profile |

### Response Body

**200 OK**
```json
{
    "success": true,
    "data": {
        "userId": 2,
        "firstName": "Sanika",
        "lastName": "Anil",
        "email": "sanika.anil@company.com",
        "employeeId": "MGR2601",
        "departmentId": 1,
        "departmentName": "Engineering Operations",
        "managerId": null,
        "managerEmployeeId": null,
        "managerName": null,
        "roleId": 2,
        "roleName": "Manager",
        "status": 1,
        "fullName": "Sanika Anil"
    }
}
```
*Note: Returned when uspGetUserProfile successfully retrieves user profile with joined department and role information*

**404 Not Found**
```json
{
    "message": "User profile not found"
}
```
*Note: Triggered when uspGetUserProfile returns null, indicating user doesn't exist or is soft-deleted*

**401 Unauthorized**
```json
{
    "message": "User not authenticated"
}
```
*Note: Returned when JWT token is missing, invalid, or user ID cannot be extracted from claims*

---

`GET http://localhost:5131/api/users`

`http://localhost:5131/api/users?roleId=&employeeId=&isDeleted=false&isActive=true&sortBy=CreatedDate&sortOrder=desc&pageNumber=1&pageSize=10`

### Query Parameters

| Parameter  | Type   | Required | Message                                                                           |
| ---------- | ------ | -------- | --------------------------------------------------------------------------------- |
| roleId     | int    | No       | Filter users by role ID (1=Admin, 2=Manager, 3=Employee)                          |
| employeeId | string | No       | Filter users by employee ID (partial match)                                       |
| isDeleted  | bool   | No       | Filter by deletion status (true=deleted, false=active)                            |
| isActive   | bool   | No       | Filter by active status (true=active, false=inactive)                             |
| sortBy     | string | No       | Sort field (default: "CreatedDate"). Options: CreatedDate, FirstName, Email, etc. |
| sortOrder  | string | No       | Sort order (default: "desc"). Options: asc, desc                                  |
| pageNumber | int    | No       | Page number for pagination (default: 1)                                           |
| pageSize   | int    | No       | Number of records per page (default: 10)                                          |

### Access Admin only with Jwt bearer

**Note:** Admin-only endpoint that retrieves a paginated and filterable list of all users in the system using the `uspGetUsersList` stored procedure. This endpoint provides comprehensive user management capabilities with support for multiple filter combinations including role, employee ID, deletion status, active status, and global pagination with sorting (similar to BudgetController). The procedure efficiently handles pagination with offset-based pagination and returns total count for implementing pagination controls. Includes complete user details with department and manager information through table joins. Restricted to administrators for user oversight, reporting, and management tasks. Supports sorting by various fields to enable flexible data presentation.

### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 200  | OK                    | Users retrieved successfully  |
| 401  | Unauthorized          | User not authenticated        |
| 500  | Internal Server Error | Failed to retrieve users list |

### Response Body

**200 OK (Admin Role - Full Access)**
```json
{
    "data": [
        {
            "userId": 1,
            "firstName": "Dharunkumar",
            "lastName": "S",
            "email": "Dharunkumar.S@budgettrack.com",
            "employeeId": "ADM2601",
            "departmentId": 1,
            "departmentName": "Engineering Operations",
            "managerId": null,
            "managerEmployeeId": null,
            "managerName": null,
            "roleId": 1,
            "roleName": "Admin",
            "status": 1,
            "isActive": true,
            "isDeleted": false,
            "createdDate": "2026-02-25T01:24:20.3843916",
            "updatedDate": "2026-02-25T12:23:55.8793181",
            "fullName": "Dharunkumar S"
        },
        {
            "userId": 2,
            "firstName": "Sanika",
            "lastName": "Anil",
            "email": "Sanika.Anil@budgettrack.com",
            "employeeId": "MGR2601",
            "departmentId": 1,
            "departmentName": "Engineering Operations",
            "managerId": 1,
            "managerEmployeeId": "ADM2601",
            "managerName": "Dharunkumar S",
            "roleId": 2,
            "roleName": "Manager",
            "status": 1,
            "isActive": true,
            "isDeleted": false,
            "createdDate": "2026-02-25T01:24:20.3843916",
            "updatedDate": "2026-02-25T12:25:04.9626306",
            "fullName": "Sanika Anil"
        },
        {
            "userId": 7,
            "firstName": "Shivali",
            "lastName": "Sharma",
            "email": "Shivali.Sharma@budgettrack.com",
            "employeeId": "EMP2601",
            "departmentId": 1,
            "departmentName": "Engineering Operations",
            "managerId": 2,
            "managerEmployeeId": "MGR2601",
            "managerName": "Sanika Anil",
            "roleId": 3,
            "roleName": "Employee",
            "status": 1,
            "isActive": true,
            "isDeleted": false,
            "createdDate": "2026-02-25T01:24:20.3843916",
            "updatedDate": "2026-02-25T12:25:44.7665145",
            "fullName": "Shivali Sharma"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 3,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "firstPage": 1,
    "lastPage": 1,
    "nextPage": null,
    "previousPage": null,
    "firstItemIndex": 1,
    "lastItemIndex": 3,
    "currentPageItemCount": 3,
    "isFirstPage": true,
    "isLastPage": true
}
```
*Note: Admin users can view all users in the system including Admins, Managers, and Employees. This response shows complete user details with pagination metadata. Admins have full visibility across all roles and departments.*

**200 OK (Manager Role - Filter by RoleId=2)**
```json
{
    "data": [
        {
            "userId": 2,
            "firstName": "Sanika",
            "lastName": "Anil",
            "email": "Sanika.Anil@budgettrack.com",
            "employeeId": "MGR2601",
            "departmentId": 1,
            "departmentName": "Engineering Operations",
            "managerId": 1,
            "managerEmployeeId": "ADM2601",
            "managerName": "Dharunkumar S",
            "roleId": 2,
            "roleName": "Manager",
            "status": 1,
            "isActive": true,
            "isDeleted": false,
            "createdDate": "2026-02-25T01:24:20.3843916",
            "updatedDate": "2026-02-25T12:25:04.9626306",
            "fullName": "Sanika Anil"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "firstPage": 1,
    "lastPage": 1,
    "nextPage": null,
    "previousPage": null,
    "firstItemIndex": 1,
    "lastItemIndex": 1,
    "currentPageItemCount": 1,
    "isFirstPage": true,
    "isLastPage": true
}
```
*Note: When filtering by roleId=2, only Manager role users are returned. Managers do not have hierarchical manager assignments (managerId is null). This demonstrates role-based filtering capability where admins can view users by specific roles.*

**200 OK (Employee Role - Filter by RoleId=3)**
```json
{
    "data": [
        {
            "userId": 7,
            "firstName": "Shivali",
            "lastName": "Sharma",
            "email": "Shivali.Sharma@budgettrack.com",
            "employeeId": "EMP2601",
            "departmentId": 1,
            "departmentName": "Engineering Operations",
            "managerId": 2,
            "managerEmployeeId": "MGR2601",
            "managerName": "Sanika Anil",
            "roleId": 3,
            "roleName": "Employee",
            "status": 1,
            "isActive": true,
            "isDeleted": false,
            "createdDate": "2026-02-25T01:24:20.3843916",
            "updatedDate": "2026-02-25T12:25:44.7665145",
            "fullName": "Shivali Sharma"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "firstPage": 1,
    "lastPage": 1,
    "nextPage": null,
    "previousPage": null,
    "firstItemIndex": 1,
    "lastItemIndex": 1,
    "currentPageItemCount": 1,
    "isFirstPage": true,
    "isLastPage": true
}
```
*Note: When filtering by roleId=3, only Employee role users are returned. Employees always have manager assignments showing their reporting structure (managerId, managerEmployeeId, and managerName are populated). This demonstrates the hierarchical employee-manager relationship in the system.*

**200 OK (Filtered - Active Employees Only)**
```json
{
    "data": [
        {
            "userId": 7,
            "firstName": "Shivali",
            "lastName": "Sharma",
            "email": "Shivali.Sharma@budgettrack.com",
            "employeeId": "EMP2601",
            "departmentId": 1,
            "departmentName": "Engineering Operations",
            "managerId": 2,
            "managerEmployeeId": "MGR2601",
            "managerName": "Sanika Anil",
            "roleId": 3,
            "roleName": "Employee",
            "status": 1,
            "isActive": true,
            "isDeleted": false,
            "createdDate": "2026-02-25T01:24:20.3843916",
            "updatedDate": "2026-02-25T12:25:44.7665145",
            "fullName": "Shivali Sharma"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "firstPage": 1,
    "lastPage": 1,
    "nextPage": null,
    "previousPage": null,
    "firstItemIndex": 1,
    "lastItemIndex": 1,
    "currentPageItemCount": 1,
    "isFirstPage": true,
    "isLastPage": true
}
```
*Note: When combining filters (roleId=3, isActive=true, isDeleted=false), the result shows only active, non-deleted employees. This demonstrates the flexible filtering capabilities allowing admins to segment user data for specific management needs.*

**200 OK (Empty Result)**
```json
{
    "data": [],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 0,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "firstPage": 1,
    "lastPage": 0,
    "nextPage": null,
    "previousPage": null,
    "firstItemIndex": 0,
    "lastItemIndex": 0,
    "currentPageItemCount": 0,
    "isFirstPage": true,
    "isLastPage": true
}
```
*Note: Returned when uspGetUsersList finds no users matching the filter criteria. Valid response indicating no data available for the specified filters.*

**401 Unauthorized**
```json
{
    "success": false,
    "message": "Unauthorized access"
}
```
*Note: Returned when the request lacks valid authentication credentials or the JWT token is invalid/expired.*

**403 Forbidden**
```json
{
    "success": false,
    "message": "Forbidden: Insufficient permissions"
}
```
*Note: Returned when the authenticated user does not have Admin privileges to access the user list.*

**500 Internal Server Error**
```json
{
    "success": false,
    "message": "An error occurred while retrieving users"
}
```
*Note: Returned when an unexpected error occurs during the uspGetUsersList stored procedure execution or database operation.*

---
