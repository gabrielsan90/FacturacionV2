---
name: dotnet-backend
description: Implement ASP.NET Core Web API with Controllers, Repositories, Unit of Work, and Services. Use when creating API endpoints, implementing data access layer, configuring JWT authentication, or registering dependencies in Program.cs.
---

# .NET Backend Developer

Implement REST API with ASP.NET Core Web API in the Backend project.

## Folder Structure

```
[ProjectName].Backend/
├── Controllers/              # HTTP Endpoints
├── Data/
│   └── DataContext.cs       # EF Core DbContext
├── Repositories/
│   ├── Interfaces/          # IGenericRepository, ISpecific...
│   └── Implementations/     # GenericRepository, Specific...
├── UnitsOfWork/
│   ├── Interfaces/
│   └── Implementations/
├── Services/                # Complex business logic
├── Helpers/                 # Utilities
└── Program.cs               # Configuration and DI
```

## Generic Repository Implementation

```csharp
public interface IGenericRepository<T> where T : class
{
    Task<ActionResponse<T>> GetAsync(int id);
    Task<ActionResponse<IEnumerable<T>>> GetAllAsync();
    Task<ActionResponse<T>> AddAsync(T entity);
    Task<ActionResponse<T>> UpdateAsync(T entity);
    Task<ActionResponse<T>> DeleteAsync(int id);
}

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DataContext _context;
    private readonly DbSet<T> _entity;

    public GenericRepository(DataContext context)
    {
        _context = context;
        _entity = context.Set<T>();
    }

    public virtual async Task<ActionResponse<IEnumerable<T>>> GetAllAsync()
    {
        try
        {
            var result = await _entity.AsNoTracking().ToListAsync();
            return new ActionResponse<IEnumerable<T>> { WasSuccess = true, Result = result };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<T>> { WasSuccess = false, Message = ex.Message };
        }
    }

    public virtual async Task<ActionResponse<T>> GetAsync(int id)
    {
        try
        {
            var entity = await _entity.FindAsync(id);
            if (entity == null)
                return new ActionResponse<T> { WasSuccess = false, Message = "Not found" };
            return new ActionResponse<T> { WasSuccess = true, Result = entity };
        }
        catch (Exception ex)
        {
            return new ActionResponse<T> { WasSuccess = false, Message = ex.Message };
        }
    }

    public virtual async Task<ActionResponse<T>> AddAsync(T entity)
    {
        try
        {
            _entity.Add(entity);
            await _context.SaveChangesAsync();
            return new ActionResponse<T> { WasSuccess = true, Result = entity };
        }
        catch (Exception ex)
        {
            return new ActionResponse<T> { WasSuccess = false, Message = ex.Message };
        }
    }

    public virtual async Task<ActionResponse<T>> UpdateAsync(T entity)
    {
        try
        {
            _entity.Update(entity);
            await _context.SaveChangesAsync();
            return new ActionResponse<T> { WasSuccess = true, Result = entity };
        }
        catch (Exception ex)
        {
            return new ActionResponse<T> { WasSuccess = false, Message = ex.Message };
        }
    }

    public virtual async Task<ActionResponse<T>> DeleteAsync(int id)
    {
        try
        {
            var entity = await _entity.FindAsync(id);
            if (entity == null)
                return new ActionResponse<T> { WasSuccess = false, Message = "Not found" };
            _entity.Remove(entity);
            await _context.SaveChangesAsync();
            return new ActionResponse<T> { WasSuccess = true };
        }
        catch (Exception ex)
        {
            return new ActionResponse<T> { WasSuccess = false, Message = ex.Message };
        }
    }
}
```

## Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EntityController : ControllerBase
{
    private readonly IEntityUnitOfWork _unitOfWork;
    private readonly ILogger<EntityController> _logger;

    public EntityController(IEntityUnitOfWork unitOfWork, ILogger<EntityController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var action = await _unitOfWork.GetAllAsync();
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var action = await _unitOfWork.GetAsync(id);
        return action.WasSuccess ? Ok(action.Result) : NotFound(action.Message);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] EntityDto dto)
    {
        var entity = new Entity { /* map from dto */ };
        var action = await _unitOfWork.AddAsync(entity);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] EntityDto dto)
    {
        var action = await _unitOfWork.UpdateAsync(/* entity */);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var action = await _unitOfWork.DeleteAsync(id);
        return action.WasSuccess ? NoContent() : BadRequest(action.Message);
    }
}
```

## Service Registration (Program.cs)

```csharp
// DbContext
builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IEntityRepository, EntityRepository>();

// Units of Work
builder.Services.AddScoped(typeof(IGenericUnitOfWork<>), typeof(GenericUnitOfWork<>));
builder.Services.AddScoped<IEntityUnitOfWork, EntityUnitOfWork>();

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
```

## Unbreakable Rules

1. **ALWAYS** use async/await for I/O operations
2. **ALWAYS** return ActionResponse<T> from repositories
3. **ALWAYS** use IActionResult in controllers
4. **ALWAYS** decorate with [ApiController] and [Route("api/[controller]")]
5. **ALWAYS** use .AsNoTracking() for read-only queries
6. **ALWAYS** use Include() for relationships
7. **NEVER** expose entities directly, use DTOs
8. **NEVER** create services with `new`
9. **ALWAYS** inject ILogger for logging
10. **ALWAYS** wrap repository operations in try-catch

## Naming Conventions

- Controllers: `{Entity}Controller`
- Repositories: `I{Entity}Repository`, `{Entity}Repository`
- Unit of Work: `I{Entity}UnitOfWork`, `{Entity}UnitOfWork`
- Async methods: suffix `Async`
