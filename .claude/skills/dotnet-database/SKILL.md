---
name: dotnet-database
description: Design and optimize data layer with SQL Server and Entity Framework Core 9. Use when creating DataContext, configuring entities with Fluent API, creating migrations, optimizing queries, avoiding N+1, implementing SeedDb, or designing relationships.
---

# .NET Database Specialist

Design and optimize data layer with SQL Server and Entity Framework Core 9.

## DataContext

```csharp
public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigurePackage(modelBuilder);
        ConfigureStatus(modelBuilder);
        DisableCascadeDelete(modelBuilder);
    }

    private void ConfigurePackage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TrackingNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.TrackingNumber).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Status)
                .WithMany(s => s.Packages)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasData(
                new Status { Id = 1, Name = "Pending", Order = 1 },
                new Status { Id = 2, Name = "In Progress", Order = 2 },
                new Status { Id = 3, Name = "Completed", Order = 3 },
                new Status { Id = 4, Name = "Cancelled", Order = 4 }
            );
        });
    }

    private void DisableCascadeDelete(ModelBuilder modelBuilder)
    {
        var fks = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);
        foreach (var fk in fks)
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
```

## Connection Configuration (Program.cs)

```csharp
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            sql.CommandTimeout(60);
        });
    if (builder.Environment.IsDevelopment())
    {
        opt.EnableSensitiveDataLogging();
        opt.EnableDetailedErrors();
    }
});
```

## Optimized Repository

```csharp
public class PackageRepository : GenericRepository<Package>, IPackageRepository
{
    private readonly DataContext _context;

    public PackageRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    // Optimized query with Include
    public async Task<ActionResponse<Package>> GetWithDetailsAsync(int id)
    {
        try
        {
            var package = await _context.Packages
                .Include(p => p.Status)
                .Include(p => p.User)
                .Include(p => p.Attachments)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return package == null
                ? ActionResponse<Package>.Failure("Not found")
                : ActionResponse<Package>.Success(package);
        }
        catch (Exception ex)
        {
            return ActionResponse<Package>.Failure(ex.Message);
        }
    }

    // Avoid N+1 with in-memory loading
    public async Task<ActionResponse<IEnumerable<PackageSummaryDto>>> GetSummaryAsync()
    {
        try
        {
            var packages = await _context.Packages.AsNoTracking().ToListAsync();
            var statuses = await _context.Statuses.AsNoTracking().ToDictionaryAsync(e => e.Id);
            var attachments = await _context.Attachments
                .GroupBy(a => a.PackageId)
                .Select(g => new { PackageId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PackageId, x => x.Count);

            var result = packages.Select(p => new PackageSummaryDto
            {
                Id = p.Id,
                TrackingNumber = p.TrackingNumber,
                StatusName = statuses.GetValueOrDefault(p.StatusId)?.Name ?? "N/A",
                AttachmentCount = attachments.GetValueOrDefault(p.Id, 0),
                CreatedAt = p.CreatedAt
            });

            return ActionResponse<IEnumerable<PackageSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return ActionResponse<IEnumerable<PackageSummaryDto>>.Failure(ex.Message);
        }
    }

    // Efficient pagination
    public async Task<ActionResponse<PaginatedResult<Package>>> GetPaginatedAsync(int page, int pageSize, string? search = null)
    {
        try
        {
            var query = _context.Packages.Include(p => p.Status).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.TrackingNumber.Contains(search) || p.Description!.Contains(search));

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return ActionResponse<PaginatedResult<Package>>.Success(new PaginatedResult<Package>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            return ActionResponse<PaginatedResult<Package>>.Failure(ex.Message);
        }
    }
}
```

## Migration Commands

```bash
# Create migration
cd [ProjectName].Backend
dotnet ef migrations add DescriptiveName --output-dir Data/Migrations

# Apply migration
dotnet ef database update

# Revert migration
dotnet ef database update PreviousMigration

# SQL script for production
dotnet ef migrations script --idempotent -o ./Scripts/migration.sql
```

## SeedDb

```csharp
public class SeedDb
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SeedDb(DataContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await CheckRolesAsync();
        await CheckAdminUserAsync();
    }

    private async Task CheckRolesAsync()
    {
        var roles = new[] { "Admin", "Manager", "Employee", "User" };
        foreach (var role in roles)
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
    }

    private async Task CheckAdminUserAsync()
    {
        var email = "admin@system.com";
        if (await _userManager.FindByEmailAsync(email) == null)
        {
            var user = new User
            {
                UserName = email, Email = email,
                FirstName = "Admin", LastName = "System",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(user, "Admin123!");
            await _userManager.AddToRoleAsync(user, "Admin");
        }
    }
}

// Program.cs
builder.Services.AddTransient<SeedDb>();
var app = builder.Build();
SeedData(app);

void SeedData(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var service = scope.ServiceProvider.GetService<SeedDb>();
    service!.SeedAsync().Wait();
}
```

## Unbreakable Rules

1. **ALWAYS** use Code First with Migrations
2. **ALWAYS** use Fluent API in OnModelCreating
3. **ALWAYS** add indexes on search columns
4. **ALWAYS** use AsNoTracking() for read-only queries
5. **NEVER** use Delete Cascade without review
6. **ALWAYS** use transactions for multiple operations
7. **NEVER** allow N+1 queries, load in memory
8. **ALWAYS** paginate large results
9. **NEVER** expose DbContext outside Backend
10. **ALWAYS** create SQL scripts for production

## Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Table | PascalCase Plural | Packages |
| Column | PascalCase | TrackingNumber |
| FK | EntityId | StatusId |
| Index | IX_Table_Column | IX_Packages_TrackingNumber |
