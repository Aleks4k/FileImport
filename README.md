# FileImport API

> Simple, extensible ASP.NET Core (.NET 10) microservice for file storage and transfer with Google sign-in and JWT authentication.

## What the project does

`FileImport` implements a backend API for uploading, downloading, managing and indexing files. It exposes endpoints to:

- Upload, download and stream files (supports range requests).
- Generate and manage file routes and file/folder operations (rename, delete).
- Request and manage download links and cache invalidation.
- Authenticate users via Google sign-in and issue JWT tokens for secured endpoints.

The solution is split into four projects:

- `FileImport.Api` - ASP.NET Core Web API and controllers.
- `FileImport.Application` - Application layer, MediatR requests/handlers, DTOs and validators.
- `FileImport.Infrastructure` - Implementations for storage, repositories, JWT and Google services.
- `FileImport.Domain` - EF Core `DbContext`, entities and domain exceptions.

## Why this project is useful

- Clear separation of concerns (API / Application / Infrastructure / Domain).
- Uses MediatR for request/response decoupling and FluentValidation for input validation.
- Supports JWT authentication and Google OAuth validation out of the box.
- Designed to be extended with custom storage backends, repositories and validators.

## Key features

- File upload/download with support for streaming and range processing.
- Folder exploration APIs (list content, detect subfolders).
- Route generation for file access and cache management endpoints.
- Google sign-in flow and JWT token issuance.
- Centralized error handling with consistent ProblemDetails responses.

## Getting started

Prerequisites

- .NET 10 SDK
- SQL Server (or a connection string to an accessible SQL Server instance)
- Optional: Docker (if you prefer running SQL Server in a container)

Clone the repository

```bash
git clone <repo-url>
cd <repo-directory>
```

Environment configuration

The app reads several configuration values from environment variables. For local development you can create a `.env` file at the repository root (the project uses `dotenv.net`) or set environment variables in your OS / IDE.

Create a `.env` file with the following keys (example values shown):

```
CONNECTIONSTRING_FILEIMPORT=Server=localhost;Database=FileImportDb;User Id=sa;Password=Your_password123;
FILESTORAGE_ROOTPATH=C:\file_storage\root
FILESTORAGE_ROOTPATH_CHECKED=C:\file_storage\checked
GoogleSettings_ClientId=your-google-client-id.apps.googleusercontent.com
JWT_AccessTokenKey=supersecretlongkeyhere
JWT_Issuer=fileimport.example
JWT_Audience=fileimport.example
JWT_AccessTokenTTL=60
```

Notes

- Ensure `CONNECTIONSTRING_FILEIMPORT` points to a valid SQL Server instance and that the database exists or apply migrations before first run.
- The app configures CORS policies for `Development`, `Test` and `Production` environments; by default the `Development` policy allows `https://localhost:7244`.

Build and run

Restore and build the solution:

```bash
dotnet restore
dotnet build
```

Run the API (development):

```bash
dotnet run --project FileImport.Api
```

Or publish for production (example used in the project comments):

```bash
dotnet publish -c Release -o ./publish /p:EnvironmentName=Test
```

## Project structure

- `FileImport.Api` — Controllers, filters, startup (`Program.cs`) and API surface.
- `FileImport.Application` — MediatR handlers, commands/queries, DTOs and FluentValidation validators.
- `FileImport.Infrastructure` — Concrete repositories, services (JWT, Google auth), settings and DI wiring.
- `FileImport.Domain` — EF Core entities, `AppDbContext`, and domain exceptions.

## Where to get help

- Open an issue in this repository for bugs or feature requests.
- For design or architecture questions, create a discussion / issue with the `design` tag.
- See inline code and the `FileImport.Api/Filters` and `FileImport.Infrastructure/DependencyInjection.cs` for how errors and services are wired.