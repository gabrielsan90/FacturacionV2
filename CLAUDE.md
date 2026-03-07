# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Environment
This project runs on **Windows/.NET 9 (Visual Studio)**. Do NOT attempt to build, run migrations, or start the application from WSL/Linux — it will fail or be extremely slow. Always assume the user will verify builds in Visual Studio unless explicitly told otherwise.

## Session Continuations
When continuing from a previous session, do NOT re-read the entire codebase from scratch. Ask the user for a brief summary of where they left off and which files were being modified. Keep responses focused and avoid exceeding context limits.

## Bug Fix Workflow
When fixing bugs, verify variable names, field mappings, and function references against the ACTUAL codebase before submitting changes. Do not guess at names. After each fix, check for related downstream breakages (e.g., fixing a controller may break JS that references it). Prefer fixing all related issues in one pass rather than iterative back-and-forth.

## CSS & UI Changes
When modifying CSS or UI layouts, check for duplicate/conflicting styles before adding new ones. Always verify z-index stacking contexts when working with modals or overlays. Test that grid/flex layouts render correctly (cards horizontal vs vertical).

## DataTables Pattern
This project uses DataTables extensively. When adding or modifying DataTable pages, always ensure:
1. `dataTableConfig` utility object is defined or imported
2. Column field names match the API response exactly (camelCase)
3. Pagination parameters are sent correctly to the backend
4. Check ALL pages that share DataTable utilities when modifying shared config
5. Always use `initDataTable()` helper with Spanish locale

---

## Project Overview

**FacturacionV2** — Full ERP + Electronic Invoicing system for Costa Rica (Facturación Electrónica v4.4)

- **Framework**: .NET 9.0
- **Architecture**: 3-Layer (Backend API / Frontend Razor Pages / Shared Library)
- **Database**: SQL Server (`ATFE` on `www.smarttechcr.com`) via Entity Framework Core 9
- **Auth**: JWT (Backend) + Cookie `"FacturacionAuth"` (Frontend)
- **Last migration**: `IntegracionERP_Fase7_Contabilidad`
- **Entities**: 120+ classes, 70+ controllers

### Functional Modules

| Module | Frontend Folder | Key Controllers |
|--------|----------------|-----------------|
| Ventas | `Pages/Ventas/` | VentasController, CotizacionesController, PedidosVentaController |
| Compras | `Pages/Compras/` | OrdenesCompraController, RecepcionesCompraController, RequisicionesController |
| Contabilidad | `Pages/Contabilidad/` | AsientosContablesController, PlanCuentasController (via ConfiguracionContable), MayorizacionController |
| Bancos | `Pages/Bancos/` | MovimientosBancariosController, ConciliacionesBancariasController, CuentasBancariasController |
| Activos Fijos | `Pages/ActivosFijos/` | ActivosFijosController, DepreciacionesActivoController |
| RRHH | `Pages/RRHH/` | EmpleadosController, PlanillasController, VacacionesController |
| Maestros | `Pages/Maestros/` | ClientesController, ProveedoresController, ProductosController |
| Stock | `Pages/Stock/` | InventariosController, KardexController, AjustesInventarioController |
| CxC | `Pages/CxC/` | CuentasPorCobrarController |
| CxP | `Pages/CxP/` | CuentasPorPagarController |
| Gastos | `Pages/Gastos/` | GastosController |
| Documentos Electrónicos | `Pages/DocumentosElectronicos/` | DocumentosController |
| Seguridad | `Pages/Seguridad/` | UsuariosController, RolesController, PrivilegiosController |

### Database Migrations

```bash
cd Facturacion.Backend
dotnet ef migrations add MigrationName
dotnet ef database update
# If dotnet-ef version conflicts:
DOTNET_ROLL_FORWARD=Major dotnet ef migrations add MigrationName
```

No test projects currently configured.

---

## Architecture (CRITICAL)

### 3-Layer Separation

| Layer | Project | Role |
|-------|---------|------|
| **Presentation** | `Facturacion.Frontend` | Razor Pages + jQuery. NEVER accesses DB directly. |
| **API** | `Facturacion.Backend` | Controllers, Services, Repositories, UoW. JWT auth. |
| **Shared** | `Facturacion.Shared` | Entities, DTOs, Enums, `ActionResponse<T>` |

### Frontend → Backend Communication (CRITICAL RULE)

**JavaScript NEVER calls the API directly.** The flow is always:

```
Browser JS → AJAX to Razor PageHandler → PageModel calls API via IHttpClientFactory → Backend API
```

```javascript
// CORRECT: JS calls page handler
$.ajax({ url: '?handler=GetData', type: 'GET', ... });
```

```csharp
// CORRECT: PageModel handler proxies to API
public async Task<IActionResult> OnGetDataAsync() {
    var client = _httpClientFactory.CreateClient("FacturacionApi");
    var token = User.FindFirst("Token")?.Value;  // JWT stored as claim in auth cookie
    if (!string.IsNullOrEmpty(token))
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    var response = await client.GetAsync("/api/endpoint");
    // ...
}
```

### Backend Patterns

- **Generic Repository**: `IGenericRepository<T>` / `GenericRepository<T>` for standard CRUD
- **Specific Repositories**: Only when complex queries needed (e.g., `IDocumentoRepository` with `Include()` joins)
- **Unit of Work**: `IGenericUnitOfWork<T>` wraps repository + transaction management
- **All repositories return `ActionResponse<T>`**:

```csharp
public class ActionResponse<T> {
    public bool WasSuccess { get; set; }
    public string? Message { get; set; }
    public T? Result { get; set; }
}
```

### Backend JSON Serialization Config

Configured in `Backend/Program.cs`:
```csharp
options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
// NOTE: PropertyNameCaseInsensitive is NOT set (defaults to false)
```

**Implication**: When adding DTO properties sent from JS (camelCase) to Backend, use `[JsonPropertyName("camelCaseName")]` on the C# property for correct deserialization.

### Frontend Global JS Helpers

Defined in `wwwroot/js/site.js` — use these instead of raw Swal/jQuery:
- `showSuccess(message)` / `showError(message)` / `showWarning(message)` — SweetAlert2 toasts
- `confirmDelete(message, onConfirm)` — Red delete confirmation dialog
- `confirmAction(title, text, onConfirm)` — Generic confirmation (in `helpers.js`)
- `handleAjaxError(xhr)` — Standard AJAX error handler (401 redirects to login)
- `initDataTable(selector, options)` — DataTable with Spanish locale, responsive, 25 rows default

---

## Coding Conventions

- **Razor Pages**: Plural entity names (`Productos.cshtml`, NOT `Index.cshtml`)
- **Page Handlers**: Suffix `Async` (`OnPostSaveAsync()`)
- **Controllers**: Suffix `Controller` (`ClientesController`), decorated with `[ApiController]` and `[Route("api/[controller]")]`
- **Interfaces**: Prefix `I` (`IDocumentoService`)
- **Decimal precision**: Prices=5 dec, Quantities=3 dec, Totals=2 dec, Exchange rates=5 dec. ALWAYS `decimal`, NEVER `float`/`double`
- **PKs**: Use `Guid` for all business entities, `string` for User (ASP.NET Identity)
- **Multi-tenancy**: All queries MUST filter by `EmpresaId` where applicable
- **Soft delete**: Most entities use soft delete (mark inactive), not physical deletion

---

## Common Gotchas

1. **Navigation Properties to User**: ALL must be configured in `DataContext.OnModelCreating()` with `HasOne().WithMany().HasForeignKey().OnDelete(DeleteBehavior.Restrict)`. Missing = runtime 500 errors.
2. **JWT Token Access**: Frontend gets JWT via `User.FindFirst("Token")?.Value` (stored as a Claim inside the `"FacturacionAuth"` cookie), NOT from a separate cookie.
3. **HttpClient**: Always use named client `"FacturacionApi"` in Frontend.
4. **API Response Parsing**: Some endpoints return arrays directly, others wrap in `ActionResponse<T>`. PageModel handlers must check `doc.RootElement.ValueKind == JsonValueKind.Array` before calling `TryGetProperty("result", ...)` to avoid `InvalidOperationException`.
5. **Multi-tenancy**: Always filter by `EmpresaId`.
6. **Contabilidad**: Asientos are auto-generated by many modules (Ventas, Compras, Bancos). Do NOT duplicate entries manually.
7. **EmpresaId vs UserId**: Controllers use `User.FindFirst(ClaimTypes.NameIdentifier)?.Value` for `UserId`. `EmpresaId` comes from User's linked `UsuarioEmpresa` record.
8. **Hacienda token**: `HaciendaToken` entity stores OAuth2 tokens per empresa for signing XML. Must be refreshed before document submission.

---

## Adding a New Entity (Workflow)

1. Entity class in `Shared/Entities/`
2. DTOs in `Shared/DTOs/` if needed
3. `DbSet<T>` in `Backend/Data/DataContext.cs`
4. **Configure navigation properties** in `DataContext.OnModelCreating` (see Gotcha #1)
5. Migration: `dotnet ef migrations add Add[Entity]` + `dotnet ef database update`
6. Repository interface/implementation (only if complex queries needed)
7. Unit of Work interface/implementation (only if custom repository)
8. Register in DI container (`Backend/Program.cs`)
9. API Controller in `Backend/Controllers/[Module]/`
10. Razor Page (`.cshtml` + `.cshtml.cs`) in `Frontend/Pages/[Module]/`

---

## Security

- **Roles**: SuperUser, Administrador de Empresa, Contador, Facturador, Vendedor, Inventarista, Consultor
- **Password Policy**: Min 6 chars, lockout after 5 failed attempts (15 min)
- **Controllers**: `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "...")]`
- **Pages**: `[Authorize(Roles = "...")]`
- **Default admin**: `admin@facturacion.com` / `Admin123!` (SuperUser role, created by SeedDb)

---

## Reference Documentation

Consult these files (in project root) for detailed specifications:

| File | Contents |
|------|----------|
| `ESPECIFICACION_SISTEMA.md` | Business requirements |
| `BACKEND_PATTERNS.md` | Backend code patterns |
| `FRONTEND_PATTERNS.md` | Frontend code patterns |
| `ARCHITECTURE_GUIDE.md` | Architecture principles |
| `DOCUMENTACION_CAMPOS_V44.md` | Hacienda v4.4 field specs |
| `ESTADO_ACTUAL_BD.md` | Current DB schema (120 entities) |
| `SECURITY_CONFIG.md` | Auth/security config details |
| `NAMING_CONVENTIONS.md` | Naming rules |
