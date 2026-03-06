## User API

All routes are served from `http://localhost:5131/api/users` and require JWT Bearer authentication unless otherwise specified.

---

### Get User Statistics

`GET http://localhost:5131/api/users/stats`

#### Access: Admin only (JWT Bearer)

**Description:** Retrieves comprehensive user statistics including total users, users by role (Admin, Manager, Employee), active and inactive users. Returns aggregated metrics useful for dashboard and system monitoring.

#### Status Codes

| Code | Status                | Message                  |
| ---- | --------------------- | ------------------------ |
| 200  | OK                    | User stats retrieved     |
| 401  | Unauthorized          | Unauthorized             |
| 403  | Forbidden             | Forbidden                |
| 500  | Internal Server Error | Failed to get user stats |

#### Response Body

**200 OK**
```json
{
    "totalUsers": 8,
    "admins": 1,
    "managers": 2,
    "employees": 5,
    "activeUsers": 7,
    "inactiveUsers": 1
}
```

---

### Get All Managers

`GET http://localhost:5131/api/users/managers`

#### Access: All authenticated users (JWT Bearer)

**Description:** Retrieves a list of all managers in the system with basic information. Returns manager details including employee ID, first name, last name, email, and full name.

#### Status Codes

| Code | Status                | Message                |
| ---- | --------------------- | ---------------------- |
| 200  | OK                    | Managers retrieved     |
| 401  | Unauthorized          | Unauthorized           |
| 500  | Internal Server Error | Failed to get managers |

#### Response Body

**200 OK**
```json
[
    {
        "employeeId": "MGR2601",
        "firstName": "Sanika",
        "lastName": "Anil",
        "email": "sanika.anil@company.com",
        "fullName": "Sanika Anil",
        "roleId": 2,
        "departmentId": 1
    },
    {
        "employeeId": "MGR2602",
        "firstName": "David",
        "lastName": "Smith",
        "email": "david.smith@company.com",
        "fullName": "David Smith",
        "roleId": 2,
        "departmentId": 1
    }
]
```

---

### Get User by ID

`GET http://localhost:5131/api/users/{userId}`

#### Route Parameters

| Parameter | Type | Required | Description                               |
| --------- | ---- | -------- | ----------------------------------------- |
| `userId`  | int  | Yes      | Internal primary key of the user (UserID) |

#### Access: Admin only (JWT Bearer)

**Description:** Retrieves detailed user information by internal `userId` (integer primary key). Returns comprehensive user details including department, role, and manager information. Restricted to administrators to protect sensitive employee information.

#### Status Codes

| Code | Status                | Message            |
| ---- | --------------------- | ------------------ |
| 200  | OK                    | User retrieved     |
| 401  | Unauthorized          | Unauthorized       |
| 403  | Forbidden             | Forbidden          |
| 404  | Not Found             | User not found     |
| 500  | Internal Server Error | Failed to get user |

#### Response Body

**200 OK**
```json
{
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
```

---

### Get Employees by Manager

`GET http://localhost:5131/api/users/{managerUserId}/employees`

#### Route Parameters

| Parameter       | Type | Required | Description                     |
| --------------- | ---- | -------- | ------------------------------- |
| `managerUserId` | int  | Yes      | Internal ID of the manager user |

#### Access: Admin, Manager, Employee (JWT Bearer)

**Description:** Retrieves all employees under a specific manager who work in the same department. Managers can only view their own employees, employees can view employees of their own manager. Admins can view any manager's employees.

#### Status Codes

| Code | Status                | Message             |
| ---- | --------------------- | ------------------- |
| 200  | OK                    | Employees retrieved |
| 401  | Unauthorized          | Unauthorized        |
| 403  | Forbidden             | Forbidden           |
| 404  | Not Found             | Manager not found   |
| 500  | Internal Server Error | Failed to get       |

#### Response Body

**200 OK**
```json
[
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
]
```

---

### Delete User

`DELETE http://localhost:5131/api/users/{userId}`

#### Route Parameters

| Parameter | Type | Required | Description      |
| --------- | ---- | -------- | ---------------- |
| `userId`  | int  | Yes      | Internal user ID |

#### Access: Admin only (JWT Bearer)

**Description:** Soft deletes a user from the system. This operation marks the user as deleted but preserves the record for audit purposes.

#### Status Codes

| Code | Status                | Message               |
| ---- | --------------------- | --------------------- |
| 200  | OK                    | User deleted          |
| 401  | Unauthorized          | Unauthorized          |
| 403  | Forbidden             | Forbidden             |
| 404  | Not Found             | User not found        |
| 500  | Internal Server Error | Failed to delete user |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "User deleted successfully"
}
```

---

### Get User Profile (Current User)

`GET http://localhost:5131/api/users/profile`

#### Access: Admin, Manager, Employee (JWT Bearer)

**Description:** Retrieves the authenticated user's profile information based on JWT claims. Returns the current user's complete profile including manager and department details.

#### Status Codes

| Code | Status                | Message                    |
| ---- | --------------------- | -------------------------- |
| 200  | OK                    | User profile retrieved     |
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

### Get Users List (Paginated)

`GET http://localhost:5131/api/users`

#### Query Parameters

| Parameter      | Type   | Required | Description                                           |
| -------------- | ------ | -------- | ----------------------------------------------------- |
| `roleId`       | int    | No       | Filter by role ID (1=Admin, 2=Manager, 3=Employee)    |
| `search`       | string | No       | Search by employee ID or name (partial match)         |
| `departmentId` | int    | No       | Filter by department ID                               |
| `isDeleted`    | bool   | No       | Filter by deletion status                             |
| `isActive`     | bool   | No       | Filter by active status                               |
| `sortBy`       | string | No       | Sort field: "CreatedDate" (default), others available |
| `sortOrder`    | string | No       | Sort direction: "asc" or "desc" (default: "desc")     |
| `pageNumber`   | int    | No       | Page number (default: 1)                              |
| `pageSize`     | int    | No       | Records per page (default: 10)                        |

#### Access: Admin, Manager (JWT Bearer)

**Description:** Retrieves a paginated list of users. Managers can only view employees reporting directly to them. Supports filtering, searching, and sorting capabilities.

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
    "hasPreviousPage": false,
    "firstPage": 1,
    "lastPage": 1,
    "nextPage": null,
    "previousPage": null,
    "firstItemIndex": 1,
    "lastItemIndex": 5,
    "currentPageItemCount": 5,
    "isFirstPage": true,
    "isLastPage": true
}
```
