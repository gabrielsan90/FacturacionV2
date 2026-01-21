---
name: dotnet-shared
description: Define domain entities, DTOs with validation, enums, and data contracts in the Shared library. Use when creating entities, Data Transfer Objects, ActionResponse, enumerations, or any class shared between Backend and Frontend projects.
---

# .NET Shared Layer Specialist

Define entities, DTOs and data contracts in the Shared project.

## Folder Structure

```
[ProjectName].Shared/
├── Entities/           # Domain entities
├── DTOs/
│   ├── Auth/          # LoginDto, RegisterDto, TokenDto
│   ├── Entities/      # DTOs per entity
│   └── Common/        # PaginatedResult, etc.
├── Enums/             # Enumerations
├── Responses/         # ActionResponse
└── Extensions/        # Extension methods
```

## Base Entity

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

## Domain Entity

```csharp
public class Package : BaseEntity
{
    [Required(ErrorMessage = "Tracking number is required")]
    [MaxLength(50, ErrorMessage = "Maximum 50 characters")]
    public string TrackingNumber { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int StatusId { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    public decimal? Weight { get; set; }

    // Navigation properties
    public Status? Status { get; set; }
    public User? User { get; set; }
    public ICollection<Attachment>? Attachments { get; set; }
}
```

## User with Identity

```csharp
public class User : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    [MaxLength(500)]
    public string? Address { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
```

## Generic ActionResponse

```csharp
public class ActionResponse<T>
{
    public bool WasSuccess { get; set; }
    public string? Message { get; set; }
    public T? Result { get; set; }

    public static ActionResponse<T> Success(T result) => new()
    {
        WasSuccess = true,
        Result = result
    };

    public static ActionResponse<T> Failure(string message) => new()
    {
        WasSuccess = false,
        Message = message
    };
}
```

## DTOs with Validation

```csharp
// DTO for create
public class PackageCreateDto
{
    [Required(ErrorMessage = "Tracking number is required")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Between 5 and 50 characters")]
    public string TrackingNumber { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a valid status")]
    public int StatusId { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "Invalid weight")]
    public decimal? Weight { get; set; }
}

// DTO for read
public class PackageDto
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = null!;
    public string? Description { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public decimal? Weight { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

// DTO for lists (lightweight)
public class PackageSummaryDto
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public int AttachmentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Authentication DTOs

```csharp
public class LoginDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = null!;
}

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = null!;

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;
}

public class TokenDto
{
    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
    public DateTime Expiration { get; set; }
}
```

## Paginated Result

```csharp
public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

## Enumerations

```csharp
public enum AttachmentType
{
    Image = 1,
    Document = 2,
    Receipt = 3,
    Other = 99
}

public enum PriorityLevel
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}
```

## Entity to DTO Mapping

```csharp
// In Backend - Extension methods
public static class PackageMappings
{
    public static PackageDto ToDto(this Package entity) => new()
    {
        Id = entity.Id,
        TrackingNumber = entity.TrackingNumber,
        Description = entity.Description,
        StatusId = entity.StatusId,
        StatusName = entity.Status?.Name ?? "N/A",
        UserId = entity.UserId,
        UserName = entity.User?.FullName ?? "N/A",
        Weight = entity.Weight,
        CreatedAt = entity.CreatedAt,
        IsActive = entity.IsActive
    };

    public static Package ToEntity(this PackageCreateDto dto) => new()
    {
        TrackingNumber = dto.TrackingNumber,
        Description = dto.Description,
        StatusId = dto.StatusId,
        Weight = dto.Weight
    };

    public static IEnumerable<PackageSummaryDto> ToSummaryList(this IEnumerable<Package> entities) =>
        entities.Select(e => new PackageSummaryDto
        {
            Id = e.Id,
            TrackingNumber = e.TrackingNumber,
            StatusName = e.Status?.Name ?? "N/A",
            CreatedAt = e.CreatedAt
        });
}
```

## Unbreakable Rules

1. **NEVER** include business logic in Shared
2. **NEVER** add EF Core dependencies (except Identity)
3. **ALWAYS** use Data Annotations for validation
4. **ALWAYS** create separate DTOs for Create/Read/Update
5. **ALWAYS** use lightweight DTOs for lists
6. **NEVER** include navigation properties in DTOs
7. **ALWAYS** document with XML comments
8. **ALWAYS** use nullable reference types
9. **NEVER** create circular dependencies
10. **ALWAYS** version breaking changes in DTOs
