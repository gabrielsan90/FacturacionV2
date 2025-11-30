# GUÍA DE ARQUITECTURA DEL SISTEMA MJL

## PROPÓSITO
Este documento define la arquitectura de 3 capas que DEBE seguirse en todos los proyectos basados en el patrón MJL.

## ESTRUCTURA DE PROYECTOS

### OBLIGATORIO: 3 Proyectos Base

```
[NOMBREPROYECTO]/
├── [NOMBREPROYECTO].Backend/       (ASP.NET Core Web API)
├── [NOMBREPROYECTO].Frontend/      (ASP.NET Core Razor Pages)
├── [NOMBREPROYECTO].FrontendAdmin/ (ASP.NET Core Razor Pages - Admin)
└── [NOMBREPROYECTO].Shared/        (Class Library)
```

### DESCRIPCIÓN DE PROYECTOS

#### 1. [NOMBREPROYECTO].Backend
- **Tipo**: ASP.NET Core Web API (.NET 9)
- **Propósito**: API REST con autenticación JWT
- **Responsabilidades**:
  - Exposición de endpoints HTTP (Controllers)
  - Lógica de negocio (Services)
  - Acceso a datos (Repositories)
  - Gestión de transacciones (Unit of Work)
  - Autenticación y autorización JWT
  - Validaciones de datos
  - Interacción con base de datos SQL Server

#### 2. [NOMBREPROYECTO].Frontend
- **Tipo**: ASP.NET Core Razor Pages (.NET 9)
- **Propósito**: Aplicación web pública para clientes/usuarios
- **Responsabilidades**:
  - Presentación de páginas HTML (Razor Pages)
  - Interacción con Backend vía HTTP (IHttpClientFactory)
  - Autenticación con cookies
  - Validación de formularios del lado del cliente
  - UX/UI con Bootstrap, jQuery, DataTables

#### 3. [NOMBREPROYECTO].FrontendAdmin
- **Tipo**: ASP.NET Core Razor Pages (.NET 9)
- **Propósito**: Panel administrativo para gestión del sistema
- **Responsabilidades**: Iguales a Frontend pero con autorización Admin

#### 4. [NOMBREPROYECTO].Shared
- **Tipo**: Class Library (.NET 9)
- **Propósito**: Compartir código entre Backend y Frontend
- **Responsabilidades**:
  - Definición de entidades (Entities)
  - DTOs (Data Transfer Objects)
  - Enumeraciones (Enums)
  - Clases de respuesta (ActionResponse)

## FLUJO DE DATOS

### Frontend → Backend → Database

```
Usuario interactúa con:
  ↓
[Razor Page .cshtml]
  ↓ (AJAX call con jQuery)
[PageModel Handler .cshtml.cs]
  ↓ (HTTP call con IHttpClientFactory)
[Backend Controller]
  ↓ (llama a)
[Unit of Work]
  ↓ (llama a)
[Repository]
  ↓ (usa)
[DbContext (EF Core)]
  ↓ (ejecuta query)
[SQL Server Database]
```

### REGLAS DEL FLUJO

1. **NUNCA** el Frontend accede directamente a la base de datos
2. **SIEMPRE** el Frontend llama al Backend vía HTTP
3. **SIEMPRE** el Backend responde con `ActionResponse<T>`
4. **NUNCA** se expone DbContext fuera del proyecto Backend

## TECNOLOGÍAS Y DEPENDENCIAS

### Backend
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" />
<PackageReference Include="Microsoft.EntityFrameworkCore" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" />
<ProjectReference Include="..\[NOMBREPROYECTO].Shared\[NOMBREPROYECTO].Shared.csproj" />
```

### Frontend / FrontendAdmin
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" />
<ProjectReference Include="..\[NOMBREPROYECTO].Shared\[NOMBREPROYECTO].Shared.csproj" />
```

### Shared
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" />
```

## PATRONES ARQUITECTÓNICOS

### 1. Repository Pattern
- **DEBE** existir `IGenericRepository<T>` y `GenericRepository<T>`
- **DEBE** crear repositorios específicos solo cuando se necesiten queries complejas
- **Ejemplo**: `IPaqueteRepository` para queries con `Include()` y filtros complejos

### 2. Unit of Work Pattern
- **DEBE** existir `IGenericUnitOfWork<T>` y `GenericUnitOfWork<T>`
- **DEBE** inyectar repositorios en Unit of Work
- **DEBE** exponer métodos del repository a través del Unit of Work

### 3. Dependency Injection
- **SIEMPRE** registrar servicios en `Program.cs` del Backend:
  ```csharp
  builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
  builder.Services.AddScoped(typeof(IGenericUnitOfWork<>), typeof(GenericUnitOfWork<>));
  builder.Services.AddScoped<ISpecificRepository, SpecificRepository>();
  builder.Services.AddScoped<ISpecificUnitOfWork, SpecificUnitOfWork>();
  ```

### 4. DTO Pattern
- **SIEMPRE** usar DTOs para transferir datos entre capas
- **NUNCA** exponer entidades directamente en APIs
- **Ejemplo**: `PaqueteDto` en lugar de `Paquete` cuando se devuelven listas

## SEGURIDAD

### Backend: JWT Authentication
```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
});
```

### Frontend: Cookie Authentication
```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });
```

### Autorización por Roles
```csharp
// En Controller (Backend)
[Authorize(Roles = "Admin")]

// En PageModel (Frontend)
[Authorize(Roles = "Admin,Employee")]
```

## CONVENCIONES DE CÓDIGO

### Nombres de Páginas
- **NUNCA** usar "Index.cshtml" para entidades
- **SIEMPRE** usar el nombre de la entidad: `Paquetes.cshtml`, `Usuarios.cshtml`
- **Formato**: PascalCase, plural para listados

### Nombres de Handlers
- **SIEMPRE** usar sufijo "Async": `OnPostSaveAsync()`, `OnGetDataAsync()`
- **SIEMPRE** declarar handlers con `public async Task<IActionResult>`

### Nombres de Controllers
- **SIEMPRE** usar sufijo "Controller": `PaqueteController`, `UsuarioController`
- **SIEMPRE** decorar con `[ApiController]` y `[Route("api/[controller]")]`

## PERFORMANCE

### Lazy Loading
- **SIEMPRE** cargar datos bajo demanda cuando sea posible
- **Ejemplo**: Cargar adjuntos de paquetes solo cuando el usuario hace clic

### Evitar N+1 Queries
- **SIEMPRE** usar `Include()` para cargar relaciones necesarias
- **SIEMPRE** cargar todo de una vez y agregar en memoria cuando hay múltiples relaciones
- **Ejemplo**:
  ```csharp
  // Cargar todos los tickets, detalles y usuarios en 3 queries
  var tickets = await _context.Tickets.ToListAsync();
  var detalles = await _context.Detalles.ToListAsync();
  var usuarios = await _context.Users.ToListAsync();

  // Agrupar en memoria con LINQ
  var detallesPorTicket = detalles.GroupBy(d => d.TicketId)
      .ToDictionary(g => g.Key, g => g.Count());
  var usuariosPorId = usuarios.ToDictionary(u => u.Id);
  ```

### AsNoTracking
- **SIEMPRE** usar `.AsNoTracking()` en queries de solo lectura
- **Ejemplo**:
  ```csharp
  var paquetes = await _context.Paquetes
      .AsNoTracking()
      .ToListAsync();
  ```

## MANEJO DE ERRORES

### ActionResponse Pattern
```csharp
public class ActionResponse<T>
{
    public bool WasSuccess { get; set; }
    public string? Message { get; set; }
    public T? Result { get; set; }
}
```

### En Repository
```csharp
try
{
    // operación
    return new ActionResponse<T>
    {
        WasSuccess = true,
        Result = entity
    };
}
catch (Exception ex)
{
    return new ActionResponse<T>
    {
        WasSuccess = false,
        Message = ex.Message
    };
}
```

### En Controller
```csharp
var action = await _unitOfWork.GetAsync(id);
if (action.WasSuccess)
{
    return Ok(action.Result);
}
return NotFound(action.Message);
```

### En Frontend Handler
```csharp
var response = await client.GetAsync("/api/endpoint");
if (response.IsSuccessStatusCode)
{
    var data = await response.Content.ReadFromJsonAsync<T>();
    return new JsonResult(new { success = true, data });
}
return new JsonResult(new { success = false, message = "Error" });
```

## CONFIGURACIÓN

### appsettings.json (Backend)
```json
{
  "ConnectionStrings": {
    "LocalConnection": "Server=...;Database=...;Trusted_Connection=True;"
  },
  "Jwt": {
    "Issuer": "https://localhost:7030",
    "Audience": "https://localhost:7030",
    "Key": "clave-secreta-minimo-32-caracteres"
  }
}
```

### appsettings.json (Frontend)
```json
{
  "ApiBaseUrl": "https://localhost:7030/"
}
```

## MIGRACIONES DE BASE DE DATOS

### Crear Migración
```bash
cd [NOMBREPROYECTO].Backend
dotnet ef migrations add NombreMigracion
```

### Aplicar Migración
```bash
dotnet ef database update
```

### NUNCA
- **NUNCA** ejecutar migraciones desde Frontend
- **NUNCA** tener DbContext en proyecto Shared
- **NUNCA** compartir connection strings entre proyectos

## PRINCIPIOS SOLID

### Single Responsibility
- Un Controller = Un recurso/entidad
- Un Repository = Acceso a datos de una entidad
- Un Unit of Work = Coordinación de operaciones de una entidad

### Open/Closed
- Usar clases base genéricas (`GenericRepository<T>`, `GenericUnitOfWork<T>`)
- Extender mediante herencia para casos específicos

### Dependency Inversion
- **SIEMPRE** programar contra interfaces (`IRepository`, `IUnitOfWork`)
- **SIEMPRE** inyectar dependencias, **NUNCA** crear instancias con `new`

## LOGGING

### Backend
```csharp
private readonly ILogger<ControllerName> _logger;

_logger.LogInformation("Mensaje informativo");
_logger.LogWarning("Advertencia");
_logger.LogError(ex, "Error procesando solicitud");
```

### Frontend
```csharp
private readonly ILogger<PageModel> _logger;

_logger.LogInformation("Usuario {Email} inició sesión", email);
```

## SEEDING DE DATOS

### DEBE existir clase SeedDb en Backend
```csharp
public class SeedDb
{
    private readonly DataContext _context;

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await CheckRolesAsync();
        await CheckStatesAsync();
        // ...
    }
}
```

### DEBE registrarse y ejecutarse en Program.cs
```csharp
builder.Services.AddTransient<SeedDb>();
var app = builder.Build();
SeedData(app);

void SeedData(WebApplication app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    using var scope = scopedFactory!.CreateScope();
    var service = scope.ServiceProvider.GetService<SeedDb>();
    service!.SeedAsync().Wait();
}
```

## RESUMEN DE REGLAS OBLIGATORIAS

1. **DEBE** haber exactamente 3 proyectos: Backend, Frontend, Shared
2. **DEBE** usar Repository + Unit of Work en Backend
3. **DEBE** usar JWT en Backend y Cookies en Frontend
4. **DEBE** comunicarse Frontend → Backend solo vía HTTP
5. **DEBE** usar `ActionResponse<T>` para todas las respuestas del Backend
6. **DEBE** usar handlers en Razor Pages (NO controllers en Frontend)
7. **DEBE** usar AJAX con jQuery para llamadas asíncronas
8. **NUNCA** usar "Index" como nombre de página
9. **SIEMPRE** usar async/await en todas las operaciones de I/O
10. **SIEMPRE** validar datos tanto en cliente como en servidor
