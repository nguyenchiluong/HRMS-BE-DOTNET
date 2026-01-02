# Time-Off API - Quick Reference

## New Endpoints Summary

| Method | Endpoint                                                                | Purpose                                       |
| ------ | ----------------------------------------------------------------------- | --------------------------------------------- |
| POST   | `/api/time-off/requests`                                                | Submit time-off request (multipart/form-data) |
| GET    | `/api/time-off/balances?year=2025`                                      | Get leave balances                            |
| GET    | `/api/time-off/requests?page=1&limit=10&status=pending&type=PAID_LEAVE` | Get request history                           |
| PATCH  | `/api/time-off/requests/{requestId}/cancel`                             | Cancel request                                |
| GET    | `/api/v1/request-types`                                                 | Get all available request types (dynamic)     |

## Request Types (uppercase snake_case)

- `PAID_LEAVE` → Annual Leave balance
- `UNPAID_LEAVE` → No balance deduction
- `PAID_SICK_LEAVE` → Sick Leave balance
- `UNPAID_SICK_LEAVE` → No balance deduction
- `WFH` → No balance deduction

## Key Changes

1. ✅ **New**: Time-off specific endpoints at `/api/time-off/*`
2. ✅ **New**: Leave balance tracking and validation
3. ✅ **New**: File upload support for attachments
4. ✅ **New**: Dynamic request types endpoint `/api/v1/request-types`
5. ❌ **Removed**: `emergencyContact` field (no longer exists)
6. 🔄 **Format**: All endpoints use uppercase snake_case consistently (`PAID_LEAVE`, `TIMESHEET_WEEKLY`)
7. 📝 **Update**: Fetch request types from API instead of hardcoding

## Request ID Format

- Format: `"REQ-XXX"` (e.g., "REQ-001")
- Also accepts numeric ID (e.g., `1`, `123`)

## File Upload Rules

- Max 5 files per request
- Max 10MB per file
- Allowed: PDF, JPG, JPEG, PNG, DOC, DOCX
- Required for sick leave > 3 days

## Leave Balance Defaults

- Annual Leave: 15 days
- Sick Leave: 10 days
- Parental Leave: 14 days
- Other Leave: 5 days

## Status Values

- `pending` - Initial status
- `approved` - Manager approved
- `rejected` - Manager rejected
- `cancelled` - Employee cancelled

## Date Format

- ISO 8601: `yyyy-MM-dd` (e.g., "2025-11-15")
- All timestamps in UTC
