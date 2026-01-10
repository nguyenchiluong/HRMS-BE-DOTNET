# HRMS - Human Resource Management System (Backend)

This repository contains the backend API for a Human Resource Management System (HRMS). It's a .NET Web API project primarily focused on employee management, requests (leave/WFH/timesheet/profile updates), and attendance (check-in/check-out) functionality.

NOTE: This README covers local development and basic deployment setup. For production deployments, follow your organization’s infra and secret-management procedures.

---

## Highlights

- Backend: ASP.NET Core Web API (C#)
- ORM: Entity Framework Core (Npgsql provider for PostgreSQL)
- Database: PostgreSQL (SearchPath used for schema selection)
- Pattern: Repository → Service → Controller
- Endpoints for Requests and Attendance implemented according to OpenAPI specs

---

## Project Structure

```
HRMS-BE-DOTNET/
├─ Controllers/                # API controllers (Requests, Attendance, Employees)
├─ Data/                       # EF DbContext
├─ Dtos/                       # DTOs used for API input/output
├─ Migrations/                 # EF Core migrations
├─ Models/                     # Database models / Entities
├─ Repositories/               # Repository interfaces & implementations
├─ Services/                   # Business logic services
├─ Properties/                 # launch settings, etc.
├─ Program.cs                  # Application startup
├─ EmployeeApi.csproj          # .NET project file
└─ infra/                      # Infra docs and helper artifacts
```

---

## Prerequisites

- .NET SDK 9.0 (or the target SDK indicated in `EmployeeApi.csproj`) — confirm with `dotnet --info`.
- PostgreSQL server (local or remote)
- dotnet-ef tool (for migrations):

```pwsh
dotnet tool install --global dotnet-ef
# or update
dotnet tool update --global dotnet-ef
```

---

## Configuration

- Database connection strings are stored in `appsettings.json` and `appsettings.Development.json`.
- Example connection string (use one environment only in development, avoid committing secrets):

```json
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5432;Database=hrms;Username=postgres;Password=YOUR_PASSWORD;SearchPath=dotnet"
}
```

- `SearchPath` tells Npgsql which PostgreSQL schema to use. Change `dotnet` to your desired schema name.
- If you prefer environment variables, set `ConnectionStrings__Default` or use `dotnet user-secrets`.

---

## Initialize Database (Local)

If you're switching from a different provider (MySQL), remove old migrations that contain provider-specific code:

```pwsh
# Delete previous provider migrations (if any), then create new ones
Remove-Item .\Migrations -Recurse -Force
```

Create migrations and apply them:

```pwsh
# Create a migration using the project's EF Core provider (Npgsql)
dotnet ef migrations add InitialCreate --project EmployeeApi.csproj

# Apply the migration to the database
dotnet ef database update --project EmployeeApi.csproj
```

If you prefer Docker for dev, spin up a local PostgreSQL and create the schema:

```yml
# Example docker-compose.yml service (put in infra/ if desired)
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: hrms
    ports:
      - "5432:5432"
    volumes:
      - db_data:/var/lib/postgresql/data

volumes:
  db_data:
```

Then create a schema (if you’re using a non-default schema):

```sql
CREATE SCHEMA dotnet;  -- or your schema name
```

---

## Run the API

Build and run the API locally:

```pwsh
cd path/to/HRMS-BE-DOTNET
dotnet restore EmployeeApi.csproj
dotnet build EmployeeApi.csproj
dotnet run --project EmployeeApi.csproj
```

By default you’ll see the port in the console (e.g., `http://localhost:5000`). The app also exposes Swagger UI when running in Development mode.

---

## Main Endpoints (Implemented)

Requests Controller (`Controllers/RequestsController.cs`):

- GET `/api/v1/requests` — get list (query params: page, limit, status, request_type, employee_id, date_from, date_to)
- POST `/api/v1/requests` — create: CREATE Request
- GET `/api/v1/requests/{id}` — get details
- PATCH `/api/v1/requests/{id}` — update (PENDING only)
- POST `/api/v1/requests/{id}/cancel` — cancel (PENDING only)
- POST `/api/v1/requests/{id}/approve` — approve (Manager/Admin)
- POST `/api/v1/requests/{id}/reject` — reject (Manager/Admin)
- GET `/api/v1/requests/summary` — manager’s summary

Attendance Controller (`Controllers/AttendanceController.cs`):

- POST `/api/v1/attendance/check-in` — check in (location optional)
- POST `/api/v1/attendance/check-out` — check out
- GET `/api/v1/attendance/history` — get paginated history

Use Swagger (when enabled) to see models and try endpoints interactively.

---

## Example curl commands

```bash
# List requests
curl -s "http://localhost:5000/api/v1/requests?page=1&limit=10"

# Check-in
curl -X POST "http://localhost:5000/api/v1/attendance/check-in" -H "Content-Type: application/json" -d '{"location": {"latitude":10.0, "longitude":106.0}}'

# Create Request
curl -X POST "http://localhost:5000/api/v1/requests" -H "Content-Type: application/json" -d '{"request_type":"LEAVE","effective_from":"2026-01-01","effective_to":"2026-01-05","reason":"Vacation" }'
```

> Notes: Controller code currently uses placeholder `currentEmployeeId` values. In production, integrate authentication (JWT or ASP.NET Identity) and use the authenticated user id.

---

## Development Notes & Troubleshooting

- If you get a `.NET SDK does not support targeting .NET 10.0` error: either install .NET 10 SDK or change `TargetFramework` in `EmployeeApi.csproj` to a version installed on your machine (e.g., `net9.0`).
- If you see `MySqlModelBuilderExtensions` errors on build, it means there are migrations created for MySQL. Remove the migrations folder and re-run migrations for Npgsql provider.
- Package version compatibility: Keep `Microsoft.EntityFrameworkCore` and `Npgsql.EntityFrameworkCore.PostgreSQL` compatible. If you see downgrade or mismatch errors, update packages to a consistent version.
- Connection strings: Avoid committing credentials. Use `dotnet user-secrets`, environment variables, or secret managers for local dev and CI.

---

## Tests & CI

- This repository currently doesn’t include automated unit/integration tests. Add tests under `Tests/` as needed and include them in CI.

---

## Contributing

- Use feature branches: `feature/my-feature`.
- Follow commit conventions: `feat:`, `fix:`, `docs:`, `test:`, etc.
- Create a Merge Request to `develop`, request at least one reviewer.

---

## Get Help / Contact

If you need help with local setup or project changes, reach out to collaborators listed in the repo's original README or submit a new issue/PR.

---

Last updated: December 7, 2025
