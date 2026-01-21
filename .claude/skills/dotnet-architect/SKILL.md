---
name: dotnet-architect
description: Design and validate .NET 9 enterprise architecture with 3-layer pattern (Backend API, Frontend Razor, Shared library). Use when creating new solutions, validating architecture compliance, setting up project structure, or reviewing Repository/UnitOfWork/DTO patterns.
---

# .NET Enterprise Architect

Design, validate and supervise 3-layer enterprise architecture for .NET 9 systems.

## Mandatory Project Structure

```
[ProjectName]/
├── [ProjectName].Backend/       # ASP.NET Core Web API - JWT Auth
├── [ProjectName].Frontend/      # ASP.NET Core Razor Pages - Cookie Auth
├── [ProjectName].FrontendAdmin/ # ASP.NET Core Razor Pages - Admin Panel
└── [ProjectName].Shared/        # Class Library - Entities/DTOs
```

## Project Responsibilities

| Project | Type | Responsibility |
|---------|------|----------------|
| Backend | Web API | Controllers, Services, Repositories, Unit of Work, JWT |
| Frontend | Razor Pages | Public UI, HTTP calls, Cookie Auth |
| FrontendAdmin | Razor Pages | Admin panel, role authorization |
| Shared | Class Library | Entities, DTOs, Enums, ActionResponse |

## Required Architectural Patterns

### 1. Repository Pattern
```csharp
public interface IGenericRepository<T> where T : class
{
    Task<ActionResponse<T>> GetAsync(int id);
    Task<ActionResponse<IEnumerable<T>>> GetAllAsync();
    Task<ActionResponse<T>> AddAsync(T entity);
    Task<ActionResponse<T>> UpdateAsync(T entity);
    Task<ActionResponse<T>> DeleteAsync(int id);
}
```

### 2. Unit of Work Pattern
```csharp
public interface IGenericUnitOfWork<T> where T : class
{
    Task<ActionResponse<T>> GetAsync(int id);
    Task<ActionResponse<IEnumerable<T>>> GetAllAsync();
    Task<ActionResponse<T>> AddAsync(T entity);
    Task<ActionResponse<T>> UpdateAsync(T entity);
    Task<ActionResponse<T>> DeleteAsync(int id);
}
```

### 3. ActionResponse Pattern
```csharp
public class ActionResponse<T>
{
    public bool WasSuccess { get; set; }
    public string? Message { get; set; }
    public T? Result { get; set; }
}
```

## Data Flow

```
User → Razor Page → PageModel Handler → HTTP → Controller → UnitOfWork → Repository → DbContext → SQL Server
```

## Unbreakable Rules

1. **NEVER** let Frontend access database directly
2. **ALWAYS** communicate Frontend → Backend via HTTP
3. **ALWAYS** use ActionResponse<T> for responses
4. **NEVER** expose DbContext outside Backend
5. **ALWAYS** program against interfaces
6. **ALWAYS** inject dependencies, never use `new` for services

## Commands to Create Solution

```bash
# Create solution and projects
dotnet new sln -n [ProjectName]
dotnet new webapi -n [ProjectName].Backend -f net9.0
dotnet new razor -n [ProjectName].Frontend -f net9.0
dotnet new razor -n [ProjectName].FrontendAdmin -f net9.0
dotnet new classlib -n [ProjectName].Shared -f net9.0

# Add to solution
dotnet sln add [ProjectName].Backend
dotnet sln add [ProjectName].Frontend
dotnet sln add [ProjectName].FrontendAdmin
dotnet sln add [ProjectName].Shared

# Add references
dotnet add [ProjectName].Backend reference [ProjectName].Shared
dotnet add [ProjectName].Frontend reference [ProjectName].Shared
dotnet add [ProjectName].FrontendAdmin reference [ProjectName].Shared
```

## NuGet Packages by Project

### Backend
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" />
```

### Frontend/FrontendAdmin
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" />
```

### Shared
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" />
```

## Architecture Validation Checklist

When reviewing code, verify:
- [ ] 3-layer structure respected
- [ ] Repository + Unit of Work implemented
- [ ] ActionResponse used in all responses
- [ ] DTOs instead of entities in APIs
- [ ] Correct Dependency Injection
- [ ] Async/await in I/O operations
