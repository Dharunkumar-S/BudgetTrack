## Department API

All routes are served from `http://localhost:5131/api/departments` and require JWT Bearer authentication.

---

### Get All Departments

`GET http://localhost:5131/api/departments`

#### Access: Admin, Manager, Employee (JWT Bearer)

**Description:** Retrieves a list of all departments in the system. Departments are the organizational units that manage budgets and expenses.

#### Status Codes

| Code | Status                | Message                        |
| ---- | --------------------- | ------------------------------ |
| 200  | OK                    | Departments retrieved          |
| 401  | Unauthorized          | Unauthorized                   |
| 403  | Forbidden             | Forbidden                      |
| 500  | Internal Server Error | Failed to retrieve departments |

#### Response Body

**200 OK**
```json
[
    {
        "departmentID": 1,
        "departmentName": "Engineering Operations",
        "description": "Core engineering and operations team",
        "createdDate": "2026-02-25T01:24:20",
        "updatedDate": "2026-02-25T01:24:20"
    },
    {
        "departmentID": 2,
        "departmentName": "Finance",
        "description": "Financial management and accounting",
        "createdDate": "2026-02-25T01:24:20",
        "updatedDate": "2026-02-25T01:24:20"
    },
    {
        "departmentID": 3,
        "departmentName": "Human Resources",
        "description": "HR and employee management",
        "createdDate": "2026-02-25T01:24:20",
        "updatedDate": "2026-02-25T01:24:20"
    }
]
```

---

### Create Department

`POST http://localhost:5131/api/departments`

#### Access: Admin only (JWT Bearer)

**Description:** Creates a new department in the system. Department names must be unique.

#### Request Body
```json
{
    "departmentName": "Sales and Marketing",
    "description": "Sales operations and marketing initiatives"
}
```

#### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 201  | Created               | Department is created         |
| 400  | Bad Request           | Validation or duplicate error |
| 401  | Unauthorized          | Unauthorized                  |
| 403  | Forbidden             | Forbidden                     |
| 409  | Conflict              | Department already exists     |
| 500  | Internal Server Error | Failed to create department   |

#### Response Body

**201 Created**
```json
{
    "departmentId": 4,
    "message": "Department is created"
}
```

**409 Conflict**
```json
{
    "success": false,
    "message": "Department already exists"
}
```

---

### Update Department

`PUT http://localhost:5131/api/departments/{departmentID}`

#### Route Parameters

| Parameter    | Type | Required | Description            |
| ------------ | ---- | -------- | ---------------------- |
| departmentID | int  | Yes      | Internal department ID |

#### Access: Admin only (JWT Bearer)

**Description:** Updates an existing department's name and description.

#### Request Body
```json
{
    "departmentName": "Sales and Marketing Updated",
    "description": "Updated sales and marketing department"
}
```

#### Status Codes

| Code | Status                | Message                     |
| ---- | --------------------- | --------------------------- |
| 200  | OK                    | Department is updated       |
| 400  | Bad Request           | Validation error            |
| 401  | Unauthorized          | Unauthorized                |
| 403  | Forbidden             | Forbidden                   |
| 404  | Not Found             | Department not found        |
| 500  | Internal Server Error | Failed to update department |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Department is updated"
}
```

**404 Not Found**
```json
{
    "success": false,
    "message": "Department not found"
}
```

---

### Delete Department

`DELETE http://localhost:5131/api/departments/{departmentID}`

#### Route Parameters

| Parameter    | Type | Required | Description            |
| ------------ | ---- | -------- | ---------------------- |
| departmentID | int  | Yes      | Internal department ID |

#### Access: Admin only (JWT Bearer)

**Description:** Deletes a department from the system. Department can only be deleted if it has no associated users or budgets.

#### Status Codes

| Code | Status                | Message                     |
| ---- | --------------------- | --------------------------- |
| 200  | OK                    | Department is deleted       |
| 401  | Unauthorized          | Unauthorized                |
| 403  | Forbidden             | Forbidden                   |
| 404  | Not Found             | Department not found        |
| 500  | Internal Server Error | Failed to delete department |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Department is deleted"
}
```

**404 Not Found**
```json
{
    "success": false,
    "message": "Department not found"
}
```

`GET http://localhost:5131/api/departments`

### Access Admin,Manager,Employee with Jwt bearer

**Note:** Retrieves a list of all active departments using the `uspGetAllDepartments` stored procedure. This endpoint provides organizational context for users and budgets. The results are filtered to exclude soft-deleted records (`IsDeleted = 0`) and are sorted alphabetically by `DepartmentName`. This is the primary source for populating department dropdowns across the application, ensuring only valid organizational units are available for user and budget assignments.

### Status Codes

| Code | Status                | Message                            |
| ---- | --------------------- | ---------------------------------- |
| 200  | OK                    | Departments retrieved successfully |
| 500  | Internal Server Error | Failed to retrieve departments     |

### Response Body

**200 OK**
```json
[
    {
        "departmentID": 1,
        "departmentName": "Global Marketing & Brand",
        "departmentCode": "MKT-GLBL",
        "isActive": true
    },
    {
        "departmentID": 2,
        "departmentName": "Cloud Site Reliability",
        "departmentCode": "DEPT2602",
        "isActive": true
    },
    {
        "departmentID": 3,
        "departmentName": "Product Engineering",
        "departmentCode": "ENG-PROD",
        "isActive": true
    }
]
```
*Note: Returns data from uspGetAllDepartments. Soft-deleted departments are automatically excluded from the results*

`POST http://localhost:5131/api/departments`

Request Body
```json
{
    "departmentName": "Human Resources",
    "departmentCode": "HR-2026"
}
```
### Access Admin with Jwt bearer

**Note:** Creates a new department record using the `uspCreateDepartment` stored procedure with comprehensive validation and audit logging. To maintain data integrity, the procedure performs dual uniqueness validation: it ensures both the `DepartmentName` and the `DepartmentCode` are unique among active departments (IsDeleted = 0). If either value conflicts with an existing department, a 409 Conflict response is returned. The procedure initializes `IsActive` to 1 and `IsDeleted` to 0, and returns the newly created DepartmentID using an OUTPUT parameter. Restricted to administrators only due to organizational impact.

### Status Codes

| Code | Status       | Message                                                                             |
| ---- | ------------ | ----------------------------------------------------------------------------------- |
| 201  | Created      | Department is created                                                               |
| 400  | Bad Request  | Bad request (invalid input)                                                         |
| 401  | Unauthorized | Unauthorized                                                                        |
| 403  | Forbidden    | Forbidden                                                                           |
| 409  | Conflict     | Department with this name already exists / Department with this code already exists |

### Response Body

**201 Created**
```json
{
    "departmentId": 7,
    "message": "Department is created"
}
```
*Note: Returned when uspCreateDepartment successfully inserts a new department record with audit logging*

**409 Conflict**
```json
{
    "success": false,
    "message": "Department with this name already exists"
}
```
*Note: Triggered by uspCreateDepartment duplicate validation for existing active department names*

**409 Conflict**
```json
{
    "success": false,
    "message": "Department with this code already exists"
}
```
*Note: Triggered by uspCreateDepartment duplicate validation for existing active department codes*

**400 Bad Request**
```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "dto": [
            "The dto field is required."
        ],
        "$.departmentName": [
            "The departmentName field is required."
        ]
    },
    "traceId": "00-7ad91d1010b41dfaba5ca03a3854a031-3dc702851dd78dc1-00"
}
```

`PUT http://localhost:5131/api/departments/{departmentID}`

Request Body
```json
{
    "departmentName": "Cloud Site Reliability & Ops",
    "departmentCode": "DEPT2602",
    "isActive": true
}
```
### Access Admin with Jwt bearer

**Note:** Updates an existing department using the `uspUpdateDepartment` stored procedure with intelligent change detection and comprehensive validation. The procedure validates the department's existence (and that it's not deleted) and ensures that renaming it or changing its code doesn't create a conflict with another existing department. The procedure checks both DepartmentName and DepartmentCode uniqueness, excluding the current department from the conflict check. Updates the `UpdatedDate` to UTC current time and sets `UpdatedByUserID`. When `IsActive` is set to `false`, the audit log `Description` automatically appends `(Inactive)` to the department name for clear audit trail visibility. Restricted to administrators to maintain organizational structure integrity.

### Status Codes

| Code | Status       | Message                                                                                             |
| ---- | ------------ | --------------------------------------------------------------------------------------------------- |
| 200  | OK           | Department is updated                                                                               |
| 400  | Bad Request  | Bad request (invalid input or no changes)                                                           |
| 401  | Unauthorized | Unauthorized                                                                                        |
| 403  | Forbidden    | Forbidden                                                                                           |
| 404  | Not Found    | Department not found or has been deleted                                                            |
| 409  | Conflict     | Another department with this name already exists / Another department with this code already exists |

### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Department is updated"
}
```
*Note: Returned when uspUpdateDepartment successfully updates department with audit logging*

**400 Bad Request (No changes)**
```json
{
    "success": false,
    "message": "No changes made"
}
```
*Note: Triggered by uspUpdateDepartment change detection when input matches existing data exactly*

**404 Not Found**
```json
{
    "success": false,
    "message": "Department not found or has been deleted"
}
```
*Note: Returned when uspUpdateDepartment cannot find active department with specified ID*

**409 Conflict**
```json
{
    "success": false,
    "message": "Another department with this name already exists"
}
```
*Note: Triggered by uspUpdateDepartment duplicate validation for name conflicts with other departments*

**409 Conflict**
```json
{
    "success": false,
    "message": "Another department with this code already exists"
}
```
*Note: Triggered by uspUpdateDepartment duplicate validation for code conflicts with other departments*

**400 Bad Request (Validation errors)**
```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "dto": [
            "The dto field is required."
        ],
        "$.departmentName": [
            "The departmentName field is required."
        ]
    },
    "traceId": "00-7ad91d1010b41dfaba5ca03a3854a031-3dc702851dd78dc1-00"
}
```

`DELETE http://localhost:5131/api/departments/{departmentID}`

### Access Admin with Jwt bearer

**Note:** Performs a soft delete operation using the `uspDeleteDepartment` stored procedure, marking departments as deleted rather than physically removing them. This preserves data integrity and audit trails while preventing access to deleted departments in standard queries. The procedure includes high-level referential integrity checks to protect data consistency, blocking deletion if any active users (`IsDeleted = 0`) or active budgets (`IsDeleted = 0`) are still linked to this department. If the department is clear of dependencies, it marks `IsDeleted = 1` and records the `DeletedDate` with current UTC time and `DeletedByUserID`. This ensures departments with existing relationships cannot be accidentally removed, maintaining organizational structure integrity.

### Status Codes

| Code | Status       | Message                                                                                               |
| ---- | ------------ | ----------------------------------------------------------------------------------------------------- |
| 200  | OK           | Department is deleted                                                                                 |
| 401  | Unauthorized | Unauthorized                                                                                          |
| 403  | Forbidden    | Forbidden                                                                                             |
| 404  | Not Found    | Department not found or has been deleted                                                              |
| 400  | Bad Request  | Cannot delete department that is in use by users / Cannot delete department that is in use by budgets |

### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Department is deleted"
}
```
*Note: Returned when uspDeleteDepartment successfully performs soft delete with audit logging*

**404 Not Found**
```json
{
    "success": false,
    "message": "Department not found or has been deleted"
}
```
*Note: Triggered when uspDeleteDepartment cannot find active department or department already deleted*

**400 Bad Request**
```json
{
    "success": false,
    "message": "Cannot delete department that is in use by users"
}
```
*Note: Triggered by uspDeleteDepartment referential integrity check when active users are linked to the department*

**400 Bad Request**
```json
{
    "success": false,
    "message": "Cannot delete department that is in use by budgets"
}
```
*Note: Triggered by uspDeleteDepartment referential integrity check when active budgets are linked to the department*