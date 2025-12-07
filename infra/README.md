# Infrastructure (infra)

This folder documents infrastructure and database setup for the HRMS Back End project (EmployeeApi).

> ⚠️ Note: This README focuses on the local developer environment. For production deployments, follow your organization's infrastructure and secret management practices (e.g., environment variables, vaults, Key Vault, AWS Secrets Manager, etc.).

---

## Overview

The backend project uses PostgreSQL with Entity Framework Core (Npgsql provider). We configure the database connection in `appsettings.json` and `appsettings.Development.json`.

The app uses a dedicated database schema via the `SearchPath` parameter in the connection string, so your tables can be created in a specific schema (e.g., `dotnet`).

This repository provides EF Core migrations and example commands to create the database and update schema.

---

## Quick Start (Local Development)

### Prerequisites

- .NET SDK (make sure you have a version that supports the project's `TargetFramework` in `EmployeeApi.csproj` — e.g. .NET 10.0 if targeting `net10.0`, or adjust csproj if you prefer an older SDK.)
- PostgreSQL server (local or remote)
- dotnet-ef tooling (for EF migrations)

To install the `dotnet-ef` tool (if needed):

```pwsh
dotnet tool install --global dotnet-ef
# or update
dotnet tool update --global dotnet-ef
```

### Setup PostgreSQL (example)

1. Start a PostgreSQL server locally (or use a cloud instance).
2. Create a database and schema for the app (example using psql):

```sql
CREATE DATABASE hrms;
\c hrms
CREATE SCHEMA dotnet;
```

3. Update your connection strings in `appsettings.json` and `appsettings.Development.json` (or set environment variables):

```json
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5432;Database=hrms;Username=postgres;Password=YOUR_PASSWORD;SearchPath=dotnet"
}
```

> Tip: Replace `YOUR_PASSWORD` with the correct password. Avoid committing passwords; for development use environment variables or `dotnet user-secrets`.

### Run EF Migrations

If you already have migrations for a different provider (e.g., MySQL), you'll get provider-specific code in the `Migrations/` folder. In that case delete or remove old migrations and create new ones for PostgreSQL.

Commands:

```pwsh
# Remove old migrations manually by deleting the Migrations folder if switching providers
Remove-Item .\Migrations -Recurse -Force

# Create a new migration
dotnet ef migrations add InitialCreate --project EmployeeApi.csproj

# Apply migrations to the configured DB
dotnet ef database update --project EmployeeApi.csproj
```

### Run the API

```pwsh
# Run the API
dotnet run --project EmployeeApi.csproj
```

Open http://localhost:5000 or the URL printed in the terminal to reach the API (port depends on your launchSettings and environment).

---

## Schema/Connection Considerations

- `SearchPath` tells Npgsql which schema(s) to use. The sample config uses `SearchPath=dotnet` so EF will create tables in that schema.
- If migration errors occur, verify:
  - The connection string points to the correct database and schema
  - The `Migrations/` folder corresponds to the same EF provider (Npgsql) and not MySQL or another provider
  - The installed EF Core provider packages match your project target framework and SDK.

---

## Version Compatibility Issues

If you see version or SDK errors while building or running migrations, consider the following:

- Verify your .NET SDK version using `dotnet --info`. If the project targets `net10.0`, make sure you have .NET 10 SDK installed.
- EF Core provider packages (e.g., `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore`) should be compatible with each other. If you get package downgrade or compatibility errors, update package versions accordingly.

---

## Docker (Optional)

Example `docker-compose.yml` to run Postgres locally:

```yml
version: '3.8'
services:
  postgres:
    image: postgres:15
    restart: always
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_USER: postgres
      POSTGRES_DB: hrms
    ports:
      - "5432:5432"
    volumes:
      - db_data:/var/lib/postgresql/data

volumes:
  db_data:
```

After starting postgres with docker-compose, connect and create the schema:

```pwsh
docker exec -it <container> psql -U postgres -d hrms
# Then run
CREATE SCHEMA dotnet;
```

---

## Endpoints (High-level Implementation)

This project contains the new REST endpoints implemented in the API (see controller classes in `Controllers/`):

- Requests API  (`Controllers/RequestsController.cs`)
  - GET `/api/v1/requests`
  - POST `/api/v1/requests`
  - GET `/api/v1/requests/{id}`
  - PATCH `/api/v1/requests/{id}`
  - POST `/api/v1/requests/{id}/cancel`
  - POST `/api/v1/requests/{id}/approve`
  - POST `/api/v1/requests/{id}/reject`
  - GET `/api/v1/requests/summary`

- Attendance API (`Controllers/AttendanceController.cs`)
  - POST `/api/v1/attendance/check-in`
  - POST `/api/v1/attendance/check-out`
  - GET `/api/v1/attendance/history`

These endpoints are implemented using a repository-service-controller pattern:
- `Repositories/*` handle DB access
- `Services/*` contain business logic
- `Controllers/*` expose the API routes

The code uses DTOs located in `Dtos/` for input/output.

---

## Authentication (Notes)

- The OpenAPI spec expects bearer token (JWT) auth in the docs (e.g., `securitySchemes`), but the implementation in controllers uses placeholder values for `currentEmployeeId` for now.
- Integrate your preferred authentication (ASP.NET Core Identity or JWT) for production/testing and remove hardcoded user ids.

---

## Troubleshooting & Tips

- Error: "The current .NET SDK does not support targeting .NET 10.0" — install the appropriate .NET SDK or change the `TargetFramework` in `EmployeeApi.csproj` if using an older SDK.
- Error: "MySqlModelBuilderExtensions does not exist" — indicates old MySQL migrations. Delete `Migrations/` and re-run `dotnet ef migrations add InitialCreate`.
- Ensure package versions of EF Core and Npgsql are compatible.

---

## Useful Commands

```pwsh
# Restore, build, run
dotnet restore EmployeeApi.csproj
dotnet build EmployeeApi.csproj
dotnet run --project EmployeeApi.csproj

# Migrations
dotnet ef migrations add <MigrationName> --project EmployeeApi.csproj
dotnet ef database update --project EmployeeApi.csproj

# Run test curl example
curl -X GET http://localhost:5000/api/v1/requests
```

---

If you'd like, I can add a docker-compose and a small convenience script to automatically create the database and schema, apply migrations, and run the API. Would you like that? 

---

Last updated: December 7, 2025
