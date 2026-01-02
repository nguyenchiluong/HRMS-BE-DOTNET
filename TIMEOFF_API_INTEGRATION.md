# Time-Off Request API - Frontend Integration Guide

## Overview

This document summarizes the changes made to the backend API for time-off request functionality. The frontend team should use this guide to integrate with the new endpoints and update existing code.

---

## 🆕 New Endpoints

### 1. POST `/api/time-off/requests` - Submit Time Off Request

**Endpoint:** `POST /api/time-off/requests`  
**Content-Type:** `multipart/form-data`  
**Authentication:** Required (JWT Bearer token)

**Request Parameters (Form Data):**

- `type` (string, required): Request type - one of:
  - `"PAID_LEAVE"`
  - `"UNPAID_LEAVE"`
  - `"PAID_SICK_LEAVE"`
  - `"UNPAID_SICK_LEAVE"`
  - `"WFH"`
- `startDate` (string, required): ISO date string (yyyy-MM-dd)
- `endDate` (string, required): ISO date string (yyyy-MM-dd)
- `reason` (string, required): Minimum 10 characters
- `attachments` (File[], optional): Array of files
  - Max 5 files
  - Max 10MB per file
  - Allowed types: PDF, JPG, JPEG, PNG, DOC, DOCX
  - Required for sick leave > 3 days

**Response (200 OK):**

```json
{
  "id": "REQ-001",
  "type": "PAID_LEAVE",
  "startDate": "2025-11-15",
  "endDate": "2025-11-17",
  "duration": 3,
  "submittedDate": "2025-10-26T10:00:00Z",
  "status": "pending",
  "reason": "Personal vacation",
  "attachments": ["/uploads/time-off-requests/file1.pdf"],
  "message": "Request submitted successfully"
}
```

**Error Responses:**

- `400 Bad Request`: Validation errors (dates, file size, etc.)
- `400 Bad Request`: Insufficient leave balance
- `400 Bad Request`: Missing medical certificate for sick leave > 3 days

---

### 2. GET `/api/time-off/balances` - Get Leave Balances

**Endpoint:** `GET /api/time-off/balances?year=2025`  
**Authentication:** Required (JWT Bearer token)  
**Query Parameters:**

- `year` (optional, default: current year): Year to get balances for

**Response (200 OK):**

```json
{
  "balances": [
    {
      "type": "Annual Leave",
      "total": 15,
      "used": 5,
      "remaining": 10
    },
    {
      "type": "Sick Leave",
      "total": 10,
      "used": 2,
      "remaining": 8
    },
    {
      "type": "Parental Leave",
      "total": 14,
      "used": 0,
      "remaining": 14
    },
    {
      "type": "Other Leave",
      "total": 5,
      "used": 1,
      "remaining": 4
    }
  ]
}
```

**Notes:**

- Balances are automatically initialized with default values if they don't exist
- `used` is calculated from approved requests for the specified year
- `remaining = total - used`

---

### 3. GET `/api/time-off/requests` - Get Request History

**Endpoint:** `GET /api/time-off/requests?page=1&limit=10&status=pending&type=PAID_LEAVE`  
**Authentication:** Required (JWT Bearer token)  
**Query Parameters:**

- `page` (optional, default: 1): Page number
- `limit` (optional, default: 10): Items per page
- `status` (optional): Filter by status - `pending`, `approved`, `rejected`, `cancelled`
- `type` (optional): Filter by type - `paid-leave`, `unpaid-leave`, `paid-sick-leave`, `unpaid-sick-leave`, `wfh`

**Response (200 OK):**

```json
{
  "data": [
    {
      "id": "REQ-001",
      "type": "PAID_LEAVE",
      "startDate": "2025-11-15",
      "endDate": "2025-11-17",
      "duration": 3,
      "submittedDate": "2025-10-26T10:00:00Z",
      "status": "pending",
      "reason": "Personal vacation",
      "attachments": ["/uploads/time-off-requests/file1.pdf"]
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 25,
    "totalPages": 3
  }
}
```

**Notes:**

- Returns only time-off requests (excludes timesheet requests)
- Sorted by `submittedDate` DESC (newest first)
- Only returns requests for the authenticated employee

---

### 4. PATCH `/api/time-off/requests/{requestId}/cancel` - Cancel Request

**Endpoint:** `PATCH /api/time-off/requests/{requestId}/cancel`  
**Authentication:** Required (JWT Bearer token)  
**Path Parameters:**

- `requestId` (string): Request ID in format "REQ-XXX" or numeric ID

**Request Body (optional):**

```json
{
  "comment": "Changed my mind"
}
```

**Response (200 OK):**

```json
{
  "id": "REQ-001",
  "type": "PAID_LEAVE",
  "startDate": "2025-11-15",
  "endDate": "2025-11-17",
  "duration": 3,
  "submittedDate": "2025-10-26T10:00:00Z",
  "status": "cancelled",
  "reason": "Personal vacation",
  "attachments": ["/uploads/time-off-requests/file1.pdf"],
  "message": "Request cancelled successfully"
}
```

**Error Responses:**

- `400 Bad Request`: Request is not in pending status
- `403 Forbidden`: Request doesn't belong to the authenticated employee
- `404 Not Found`: Request not found

---

## 🔄 Changes to Existing Endpoints

### New Endpoint: GET `/api/v1/request-types` - Get Available Request Types

**Endpoint:** `GET /api/v1/request-types`  
**Authentication:** Required (JWT Bearer token)

**Response (200 OK):**

```json
{
  "requestTypes": [
    {
      "id": 1,
      "value": "PAID_LEAVE",
      "category": "time-off",
      "name": "Paid Leave",
      "description": "Paid annual leave",
      "isActive": true,
      "requiresApproval": true
    },
    {
      "id": 6,
      "value": "TIMESHEET_WEEKLY",
      "category": "timesheet",
      "name": "Weekly Timesheet",
      "description": "Weekly timesheet submission",
      "isActive": true,
      "requiresApproval": true
    }
  ]
}
```

**Categories:**

- `"time-off"`: Time-off request types (use `/api/time-off/*` endpoints)
- `"timesheet"`: Timesheet request types (use `/api/v1/timesheet/*` endpoints)
- `"profile"`: Profile update types (use `/api/v1/requests` endpoints)
- `"other"`: Other request types

**Usage:**

- Use `value` (uppercase snake_case) for all endpoints
- Use `category` to determine which endpoint to use

---

### Request Type Format

All endpoints use **uppercase snake_case** format consistently:

- `PAID_LEAVE`
- `UNPAID_LEAVE`
- `PAID_SICK_LEAVE`
- `UNPAID_SICK_LEAVE`
- `WFH`
- `TIMESHEET_WEEKLY` (for timesheet requests)
- `PROFILE_UPDATE`
- `ID_UPDATE`

**Note:** Both formats are supported. The new time-off endpoints use kebab-case, while the existing `/api/v1/requests` endpoints continue to use the uppercase snake_case format for backward compatibility.

---

## 📊 Timesheet vs Time-Off Requests

### Key Differences

| Aspect                  | Timesheet Requests                        | Time-Off Requests                   |
| ----------------------- | ----------------------------------------- | ----------------------------------- |
| **Endpoint**            | `/api/v1/timesheet/*`                     | `/api/time-off/*`                   |
| **Request Type**        | `TIMESHEET_WEEKLY`                        | `PAID_LEAVE`, `UNPAID_LEAVE`, etc.  |
| **Purpose**             | Submit weekly work hours                  | Request time off                    |
| **Data Structure**      | Has `TimesheetEntry` records              | Simple date range                   |
| **Approval**            | Via `/api/v1/timesheet/{id}/approve`      | Via `/api/v1/requests/{id}/approve` |
| **Request Type Format** | Uppercase snake_case (`TIMESHEET_WEEKLY`) | Uppercase snake_case (`PAID_LEAVE`) |

### Timesheet Request Flow

1. **Submit Timesheet**: `POST /api/v1/timesheet/submit`

   - Creates a request with type `TIMESHEET_WEEKLY`
   - Includes multiple `TimesheetEntry` records
   - Request type returned as `"TIMESHEET_WEEKLY"` (uppercase)

2. **Get Timesheets**: `GET /api/v1/timesheet/my-timesheets`

   - Returns only timesheet requests
   - Request type is `"TIMESHEET_WEEKLY"`

3. **Approval**: Uses `/api/v1/timesheet/{id}/approve` or `/api/v1/requests/{id}/approve`
   - Both endpoints work for timesheet requests
   - Request type remains `"TIMESHEET_WEEKLY"` in responses

### Time-Off Request Flow

1. **Submit Time-Off**: `POST /api/time-off/requests`

   - Creates a request with type `PAID_LEAVE`, `UNPAID_LEAVE`, etc. (uppercase snake_case)
   - Request type returned as uppercase snake_case

2. **Get Time-Off History**: `GET /api/time-off/requests`

   - Returns only time-off requests
   - Request type is uppercase snake_case

3. **Approval**: Uses `/api/v1/requests/{id}/approve`
   - Request type in response uses uppercase format (`PAID_LEAVE`)

### Important Notes for Frontend

1. **Request Type Format:**

   - All endpoints use uppercase snake_case: `PAID_LEAVE`, `TIMESHEET_WEEKLY`, etc.
   - **Solution**: Use the `/api/v1/request-types` endpoint to get available types

2. **Filtering Requests:**

   - Use `/api/time-off/requests` for time-off requests only
   - Use `/api/v1/timesheet/my-timesheets` for timesheet requests only
   - Use `/api/v1/requests` for all requests (includes both)

3. **Request Type Detection:**

   ```javascript
   // Check if request is time-off
   const isTimeOff = [
     "PAID_LEAVE",
     "UNPAID_LEAVE",
     "PAID_SICK_LEAVE",
     "UNPAID_SICK_LEAVE",
     "WFH",
   ].includes(request.type);

   // Check if request is timesheet
   const isTimesheet = request.type === "TIMESHEET_WEEKLY";
   ```

---

## 📋 Request Type Mapping

### Time-Off Request Types

These request types are handled by the new time-off endpoints:

| API Value (uppercase snake_case) | Enum Value        | Description                                   |
| -------------------------------- | ----------------- | --------------------------------------------- |
| `PAID_LEAVE`                     | `PaidLeave`       | Paid annual leave                             |
| `UNPAID_LEAVE`                   | `UnpaidLeave`     | Unpaid annual leave                           |
| `PAID_SICK_LEAVE`                | `PaidSickLeave`   | Paid sick leave                               |
| `UNPAID_SICK_LEAVE`              | `UnpaidSickLeave` | Unpaid sick leave                             |
| `WFH`                            | `WorkFromHome`    | Work from home (doesn't affect leave balance) |

### Leave Balance Type Mapping

Time-off request types map to leave balance types:

| Request Type        | Balance Type | Affects Balance?            |
| ------------------- | ------------ | --------------------------- |
| `PAID_LEAVE`        | Annual Leave | Yes (deducts from balance)  |
| `UNPAID_LEAVE`      | Annual Leave | No (doesn't deduct)         |
| `PAID_SICK_LEAVE`   | Sick Leave   | Yes (deducts from balance)  |
| `UNPAID_SICK_LEAVE` | Sick Leave   | No (doesn't deduct)         |
| `WFH`               | N/A          | No (doesn't affect balance) |

---

## 🗑️ Removed Fields

### Emergency Contact Field

The `emergencyContact` field has been **removed** from:

- Request submission
- Request responses
- Request history

**Action Required:** Remove any references to `emergencyContact` in the frontend code.

---

## 📝 Important Notes

### 1. Request ID Format

- Time-off requests use format: `"REQ-XXX"` (e.g., "REQ-001", "REQ-123")
- Request IDs are stored in the `payload` JSON field
- The numeric database ID can also be used (e.g., `1`, `123`)

### 2. File Attachments

- Files are uploaded as `multipart/form-data`
- Uploaded files are stored in `/uploads/time-off-requests/`
- File URLs are returned as relative paths (e.g., `/uploads/time-off-requests/guid.pdf`)
- In production, these should be converted to full URLs

### 3. Date Format

- All dates use ISO 8601 format: `yyyy-MM-dd`
- Example: `"2025-11-15"`
- All timestamps are in UTC

### 4. Duration Calculation

- Duration is **inclusive** of both start and end dates
- Example: Nov 15 - Nov 17 = 3 days

### 5. Leave Balance Validation

- Only `paid-leave` and `paid-sick-leave` check leave balance
- `unpaid-leave`, `unpaid-sick-leave`, and `wfh` don't require balance checks
- Balance is only deducted when request status changes to `approved`

### 6. Sick Leave Attachments

- Medical certificate is **required** for sick leave requests longer than 3 days
- Returns `400 Bad Request` if missing

### 7. Status Transitions

- `pending` → `approved` (by manager)
- `pending` → `rejected` (by manager)
- `pending` → `cancelled` (by employee)
- Once `approved`, `rejected`, or `cancelled`, status cannot be changed

---

## 🔧 Frontend Integration Checklist

### New Features to Implement

- [ ] Time-off request submission form with file upload
- [ ] Leave balance display component
- [ ] Time-off request history list with pagination
- [ ] Request cancellation functionality
- [ ] File attachment preview/download
- [ ] Leave balance validation before submission

### Code Updates Required

- [ ] Remove `emergencyContact` field from all request forms and displays
- [ ] **Fetch request types dynamically** from `/api/v1/request-types` instead of hardcoding
- [ ] Update request type constants/enums to use uppercase snake_case format
- [ ] Add file upload component for attachments
- [ ] Update request type dropdowns to use dynamic data from API
- [ ] Add leave balance display to dashboard/profile
- [ ] Update request history to filter time-off vs timesheet requests

### API Integration Points

- [ ] Use `/api/time-off/requests` for time-off specific operations
- [ ] Use `/api/v1/requests` for general request operations (approval, rejection)
- [ ] Use `/api/time-off/balances` to display leave balances
- [ ] Handle both request ID formats (`REQ-XXX` and numeric)

### Error Handling

- [ ] Handle `400 Bad Request` for validation errors
- [ ] Handle `400 Bad Request` for insufficient leave balance
- [ ] Handle `400 Bad Request` for missing medical certificate
- [ ] Handle `403 Forbidden` for unauthorized access
- [ ] Handle `404 Not Found` for non-existent requests

---

## 🚨 Breaking Changes

1. **Emergency Contact Removed**: The `emergencyContact` field is no longer available in any request-related endpoints.

2. **Request Type Format**: All endpoints now use uppercase snake_case format consistently (`PAID_LEAVE`, `TIMESHEET_WEEKLY`, etc.).

3. **Request Types Should Be Fetched Dynamically**: Instead of hardcoding request types, use the new `/api/v1/request-types` endpoint to get available types with both formats.

---

## 📝 Timesheet-Specific Updates

### What Changed for Timesheet Requests?

**Nothing changed** - Timesheet requests continue to work as before:

- Endpoint: `/api/v1/timesheet/*` (unchanged)
- Request type: `TIMESHEET_WEEKLY` (unchanged, uppercase format)
- Approval: `/api/v1/timesheet/{id}/approve` or `/api/v1/requests/{id}/approve` (unchanged)
