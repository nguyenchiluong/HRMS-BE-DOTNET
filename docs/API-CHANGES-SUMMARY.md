# API Changes Summary - Quick Reference

## 🆕 New Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/JobLevels` | GET | Get all job levels |
| `/api/JobLevels/{id}` | GET | Get job level by ID |
| `/api/EmploymentTypes` | GET | Get all employment types |
| `/api/EmploymentTypes/{id}` | GET | Get employment type by ID |
| `/api/TimeTypes` | GET | Get all time types |
| `/api/TimeTypes/{id}` | GET | Get time type by ID |
| `/api/Employees/stats` | GET | Get employee statistics |

## 🔄 Changed Endpoints

### `GET /api/Employees`
- **Response:** Changed from array to paginated object
- **New filters:** `jobLevel`, `employmentType`, `timeType` (filter by names)
- **Query params:** Now accept arrays (multiple values)

### `POST /api/Employees/initial-profile`
- **BREAKING:** `jobLevel`, `employeeType`, `timeType` changed from strings to IDs
- **New fields:** `jobLevelId`, `employmentTypeId`, `timeTypeId` (required)

## 📊 Response Format Changes

### Employee List (GET /api/Employees)
```json
// OLD
[{ "id": 1, "fullName": "John", ... }]

// NEW
{
  "data": [{ "id": "1", "fullName": "John", ... }],
  "pagination": {
    "currentPage": 1,
    "pageSize": 14,
    "totalItems": 50,
    "totalPages": 4
  }
}
```

## 🔑 Lookup Table IDs

### Job Levels
- 1: Intern
- 2: Junior
- 3: Mid-level
- 4: Senior
- 5: Lead
- 6: Principal
- 7: Manager
- 8: Director

### Employment Types
- 1: Full-time
- 2: Part-time
- 3: Contract
- 4: Intern
- 5: Temporary
- 6: Consultant

### Time Types
- 1: On-site
- 2: Remote
- 3: Hybrid

## ⚠️ Action Items

1. ✅ Update employee list to handle paginated response
2. ✅ Fetch lookup tables on form load (JobLevels, EmploymentTypes, TimeTypes)
3. ✅ Update employee creation to use IDs instead of strings
4. ✅ Update filters to use names from lookup tables
5. ✅ Add statistics endpoint integration (optional)

