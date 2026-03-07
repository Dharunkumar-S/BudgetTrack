## Category API

All routes are served from `http://localhost:5131/api/categories` and require JWT Bearer authentication.

---

### Get All Categories

`GET http://localhost:5131/api/categories`

#### Access: Admin, Manager, Employee (JWT Bearer)

**Description:** Retrieves a list of all active categories in the system. Categories are used to classify expenses for better tracking and reporting.

#### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 200  | OK                    | Categories retrieved          |
| 401  | Unauthorized          | Unauthorized                  |
| 403  | Forbidden             | Forbidden                     |
| 500  | Internal Server Error | Failed to retrieve categories |

#### Response Body

**200 OK**
```json
[
    {
        "categoryID": 1,
        "categoryName": "Software Licenses",
        "description": "Software licensing and subscriptions",
        "createdDate": "2026-02-25T01:24:20",
        "updatedDate": "2026-02-25T01:24:20"
    },
    {
        "categoryID": 2,
        "categoryName": "Hardware",
        "description": "Computing equipment and hardware",
        "createdDate": "2026-02-25T01:24:20",
        "updatedDate": "2026-02-25T01:24:20"
    },
    {
        "categoryID": 3,
        "categoryName": "Travel",
        "description": "Business travel expenses",
        "createdDate": "2026-02-25T01:24:20",
        "updatedDate": "2026-02-25T01:24:20"
    }
]
```

---

### Create Category

`POST http://localhost:5131/api/categories`

#### Access: Admin only (JWT Bearer)

**Description:** Creates a new expense category. Category names must be unique within the system.

#### Request Body
```json
{
    "categoryName": "Cloud Services",
    "description": "Cloud hosting and infrastructure costs"
}
```

#### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 201  | Created               | Category is created           |
| 400  | Bad Request           | Validation or duplicate error |
| 401  | Unauthorized          | Unauthorized                  |
| 403  | Forbidden             | Forbidden                     |
| 409  | Conflict              | Category already exists       |
| 500  | Internal Server Error | Failed to create category     |

#### Response Body

**201 Created**
```json
{
    "categoryId": 5,
    "message": "Category is created"
}
```

**409 Conflict**
```json
{
    "success": false,
    "message": "Category already exists"
}
```

---

### Update Category

`PUT http://localhost:5131/api/categories/{categoryID}`

#### Route Parameters

| Parameter  | Type | Required | Description          |
| ---------- | ---- | -------- | -------------------- |
| categoryID | int  | Yes      | Internal category ID |

#### Access: Admin only (JWT Bearer)

**Description:** Updates an existing category's name and description.

#### Request Body
```json
{
    "categoryName": "Cloud Services Updated",
    "description": "All cloud hosting and infrastructure costs"
}
```

#### Status Codes

| Code | Status                | Message                   |
| ---- | --------------------- | ------------------------- |
| 200  | OK                    | Category is updated       |
| 400  | Bad Request           | Validation error          |
| 401  | Unauthorized          | Unauthorized              |
| 403  | Forbidden             | Forbidden                 |
| 404  | Not Found             | Category not found        |
| 500  | Internal Server Error | Failed to update category |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Category is updated"
}
```

**404 Not Found**
```json
{
    "success": false,
    "message": "Category not found"
}
```

---

### Delete Category

`DELETE http://localhost:5131/api/categories/{categoryID}`

#### Route Parameters

| Parameter  | Type | Required | Description          |
| ---------- | ---- | -------- | -------------------- |
| categoryID | int  | Yes      | Internal category ID |

#### Access: Admin only (JWT Bearer)

**Description:** Deletes a category from the system. Category can only be deleted if not in use by any expenses.

#### Status Codes

| Code | Status                | Message                   |
| ---- | --------------------- | ------------------------- |
| 200  | OK                    | Category is deleted       |
| 401  | Unauthorized          | Unauthorized              |
| 403  | Forbidden             | Forbidden                 |
| 404  | Not Found             | Category not found        |
| 500  | Internal Server Error | Failed to delete category |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Category is deleted"
}
```

**404 Not Found**
```json
{
    "success": false,
    "message": "Category not found"
}
```

`GET http://localhost:5131/api/categories`

### Access Admin,Manager,Employee with Jwt bearer

**Note:** Retrieves all active categories using the `uspGetAllCategories` stored procedure. This provides the master list for classifying expenses. The procedure filters categories where `IsDeleted = 0` and orders them alphabetically by `CategoryName`. Soft-deleted categories are automatically excluded from the results, ensuring only valid classification options are available for expense entries.

### Status Codes

| Code | Status                | Message                           |
| ---- | --------------------- | --------------------------------- |
| 200  | OK                    | Categories retrieved successfully |
| 500  | Internal Server Error | Failed to retrieve categories     |

### Response Body

**200 OK**
```json
[
    {
        "categoryID": 1,
        "categoryName": "SaaS Subscriptions",
        "categoryCode": "CAT2601",
        "isActive": true
    },
    {
        "categoryID": 2,
        "categoryName": "Cloud Infrastructure",
        "categoryCode": "CAT2602",
        "isActive": true
    },
    {
        "categoryID": 5,
        "categoryName": "Paid Social Advertising",
        "categoryCode": "ADV-PSOC",
        "isActive": true
    }
]
```
*Note: Returns data from uspGetAllCategories. Categories where IsDeleted = 1 are automatically omitted by the stored procedure*

`POST http://localhost:5131/api/categories`

Request Body
```json
{
    "categoryName": "Software Licensing",
    "categoryCode": "CAT2611"
}
```
### Access Admin only with Jwt bearer

**Note:** Creates a new category entry using the `uspCreateCategory` stored procedure with comprehensive validation and audit logging. The procedure ensures no two active categories share the same `CategoryName`, performing duplicate checking for category names. Initializes `IsActive` to 1 and `IsDeleted` to 0, capturing the current UTC timestamp for audit purposes. The stored procedure uses SCOPE_IDENTITY() to return the newly created CategoryID. Restricted to administrators and managers to maintain category creation governance and ensure proper expense classification taxonomy.

### Status Codes

| Code | Status       | Message                                |
| ---- | ------------ | -------------------------------------- |
| 201  | Created      | Category is created                    |
| 400  | Bad Request  | Bad request (invalid input)            |
| 401  | Unauthorized | Unauthorized                           |
| 403  | Forbidden    | Forbidden                              |
| 409  | Conflict     | Category with this name already exists |

### Response Body

**201 Created**
```json
{
    "categoryId": 12,
    "message": "Category is created"
}
```
*Note: Returned when uspCreateCategory successfully inserts a new category record with audit logging*

**409 Conflict**
```json
{
    "success": false,
    "message": "Category with this name already exists"
}
```
*Note: Triggered by uspCreateCategory duplicate validation for existing active category names*

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
        "$.categoryName": [
            "The categoryName field is required."
        ]
    },
    "traceId": "00-7ad91d1010b41dfaba5ca03a3854a031-3dc702851dd78dc1-00"
}
```

`PUT http://localhost:5131/api/categories/{categoryID}`

Request Body
```json
{
    "categoryName": "Paid Social Advertising",
    "categoryCode": "ADV-PSOC",
    "isActive": true
}
```
### Access Admin only with Jwt bearer

**Note:** Updates existing category metadata using the `uspUpdateCategory` stored procedure with intelligent change detection and comprehensive validation. The procedure includes a safety check to ensure the new name doesn't conflict with another existing category (excluding the current category). Validates that the category exists and is not deleted before performing the update. Updates the `UpdatedDate` to UTC current time and sets `UpdatedByUserID`. When `IsActive` is set to `false`, the audit log `Description` automatically appends `(Inactive)` to the category name for clear audit trail visibility. Supports partial updates while preserving audit compliance and maintaining category classification integrity across the expense management system.

### Status Codes

| Code | Status       | Message                                        |
| ---- | ------------ | ---------------------------------------------- |
| 200  | OK           | Category is updated                            |
| 400  | Bad Request  | Bad request (invalid input or no changes)      |
| 401  | Unauthorized | Unauthorized                                   |
| 403  | Forbidden    | Forbidden                                      |
| 404  | Not Found    | Category not found or has been deleted         |
| 409  | Conflict     | Another category with this name already exists |

### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Category is updated"
}
```
*Note: Returned when uspUpdateCategory successfully updates category with audit logging*

**400 Bad Request (No changes)**
```json
{
    "success": false,
    "message": "No changes made"
}
```
*Note: Triggered by uspUpdateCategory change detection when input matches existing data exactly*

**404 Not Found**
```json
{
    "success": false,
    "message": "Category not found or has been deleted"
}
```
*Note: Returned when uspUpdateCategory cannot find active category with specified ID*

**409 Conflict**
```json
{
    "success": false,
    "message": "Another category with this name already exists"
}
```
*Note: Triggered by uspUpdateCategory duplicate validation for name conflicts with other categories*

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
        "$.categoryName": [
            "The categoryName field is required."
        ]
    },
    "traceId": "00-7ad91d1010b41dfaba5ca03a3854a031-3dc702851dd78dc1-00"
}
```

`DELETE http://localhost:5131/api/categories/{categoryID}`

### Access Admin only with Jwt bearer

**Note:** Performs a soft delete operation using the `uspDeleteCategory` stored procedure, marking categories as deleted rather than physically removing them. This preserves data integrity and audit trails while preventing access to deleted categories in standard queries. The procedure updates the `IsDeleted` flag to 1 and records the `DeletedDate` with current UTC time and `DeletedByUserID`. To maintain referential integrity, the procedure prevents deletion if any active expenses (IsDeleted = 0) are currently linked to this CategoryID, ensuring expense classification data remains consistent and preventing orphaned references.

### Status Codes

| Code | Status       | Message                                           |
| ---- | ------------ | ------------------------------------------------- |
| 200  | OK           | Category is deleted                               |
| 401  | Unauthorized | Unauthorized                                      |
| 403  | Forbidden    | Forbidden                                         |
| 404  | Not Found    | Category not found or has been deleted            |
| 400  | Bad Request  | Cannot delete category that is in use by expenses |

### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Category is deleted"
}
```
*Note: Returned when uspDeleteCategory successfully performs soft delete with audit logging*

**404 Not Found**
```json
{
    "success": false,
    "message": "Category not found or has been deleted"
}
```
*Note: Triggered when uspDeleteCategory cannot find active category or category already deleted*

**400 Bad Request**
```json
{
    "success": false,
    "message": "Cannot delete category that is in use by expenses"
}
```
*Note: Triggered by uspDeleteCategory referential integrity check when active expenses are linked to the category*