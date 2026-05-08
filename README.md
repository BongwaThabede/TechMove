# TechMove

ASP.NET Core MVC prototype for TechMove with EF Core, workflow validation, PDF file handling, external currency API integration, and xUnit tests.

## Features

- EF Core SQL Server database with entities:
  - `Client`
  - `Contract`
  - `ServiceRequest`
- Business workflow rule:
  - A `ServiceRequest` cannot be created when the parent `Contract` is `Expired` or `OnHold`
- File handling:
  - Upload signed contract agreements (`.pdf` only)
  - File extension + PDF signature validation
  - Download uploaded agreement files from UI
- External API integration:
  - Consumes exchange-rate API for USD to ZAR
  - Auto-calculates local cost for service requests
- Filtering/search:
  - Contracts filter by date range and status
- Unit testing:
  - xUnit tests for currency conversion and file validation

## Tech Stack

- .NET 8
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server (LocalDB connection by default)
- xUnit

## Project Structure

- `Controllers/` - MVC controllers
- `Models/` - domain models
- `Data/` - `ApplicationDbContext`
- `Services/` - business/integration services
- `Views/` - Razor views
- `Migrations/` - EF Core migrations
- `TechMove.Tests/` - unit test project

## Getting Started

### 1) Clone

```bash
git clone https://github.com/BongwaThabede/TechMove.git
cd TechMove
```

### 2) Restore and Build

```bash
dotnet restore TechMove.slnx
dotnet build TechMove.slnx
```

### 3) Apply Database Migration

```bash
dotnet ef database update
```

> The app also calls `Database.Migrate()` on startup.

### 4) Run the App

```bash
dotnet run --project TechMove.csproj
```

## Migrations

Initial migration included:

- `InitialCreate`

To create a new migration later:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Running Tests

```bash
dotnet test TechMove.Tests/TechMove.Tests.csproj
```

## Notes

- Default DB connection is in `appsettings.json` under `ConnectionStrings:DefaultConnection`.
- If external currency API is unavailable, a fallback exchange rate is used.
