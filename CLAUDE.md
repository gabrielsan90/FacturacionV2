# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**FacturacionV2** - Electronic invoicing system for Costa Rica (Facturación Electrónica v4.4 compliant)
- **Framework**: .NET 9.0
- **Architecture**: MJL 3-Layer Architecture
- **Database**: SQL Server (remote: www.smarttechcr.com)
- **Business Domain**: Multi-tenant electronic invoicing, inventory, and expense management

## Solution Structure

```
FacturacionV2/
├── Facturacion.Backend/       # ASP.NET Core Web API (JWT authentication)
├── Facturacion.Frontend/      # ASP.NET Core Razor Pages (Cookie authentication)
└── Facturacion.Shared/        # Class Library (Entities, DTOs, Enums, Responses)
```

## Build and Run Commands

### Build & Restore
```bash
# Restore dependencies
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build Facturacion.Backend/Facturacion.Backend.csproj
```

### Run Projects
```bash
# Run Backend API (default port: 7030)
cd Facturacion.Backend
dotnet run

# Run Frontend (default port: 7031)
cd Facturacion.Frontend
dotnet run

# Run both in separate terminals for full stack development
```

### Database Migrations
```bash
# Create new migration (run from solution root or Backend directory)
cd Facturacion.Backend
dotnet ef migrations add MigrationName

# Apply migrations to database
dotnet ef database update

# Rollback to specific migration
dotnet ef database update MigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Testing
```bash
# No test projects currently configured
# When tests are added, run with:
dotnet test
```

## Architecture Patterns

### MJL 3-Layer Architecture

**CRITICAL**: This project follows strict separation of concerns:

1. **Presentation Layer** (Frontend)
   - Razor Pages for UI
   - Page handlers call Backend API via IHttpClientFactory
   - Cookie-based authentication
   - NEVER accesses database directly

2. **Business Logic Layer** (Backend)
   - RESTful API controllers
   - Services for business logic
   - Repositories for data access
   - Unit of Work for transaction management
   - JWT authentication
   - NEVER exposes DbContext outside Backend project

3. **Data Layer** (Shared)
   - Entity classes (database models)
   - DTOs for data transfer
   - Enums
   - ActionResponse<T> wrapper for all API responses

### Repository Pattern

- **Generic Repository**: `IGenericRepository<T>` / `GenericRepository<T>` for standard CRUD
- **Specific Repositories**: Created only when complex queries needed (e.g., `IDocumentoRepository` for Include() joins)
- All repositories return `ActionResponse<T>`

### Unit of Work Pattern

- **Generic UoW**: `IGenericUnitOfWork<T>` / `GenericUnitOfWork<T>`
- **Specific UoW**: Created when entity needs specific repository
- Manages transactions across multiple repositories

### ActionResponse Pattern

All API responses use the wrapper pattern:
```csharp
public class ActionResponse<T>
{
    public bool WasSuccess { get; set; }
    public string? Message { get; set; }
    public T? Result { get; set; }
}
```

## Frontend-Backend Communication Pattern

**CRITICAL RULE**: Frontend JavaScript NEVER calls API directly.

### ✅ CORRECT Pattern:
```javascript
// In .cshtml file - AJAX calls page handler
$.ajax({
    url: '?handler=GetData',
    type: 'GET',
    success: function(data) { ... }
});
```

```csharp
// In .cshtml.cs PageModel - Handler calls API via IHttpClientFactory
private readonly IHttpClientFactory _httpClientFactory;

public async Task<IActionResult> OnGetDataAsync()
{
    var client = _httpClientFactory.CreateClient("FacturacionApi");
    if (Request.Cookies.TryGetValue("jwtAdmin", out var jwt))
        client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);

    var response = await client.GetAsync("/api/endpoint");
    if (response.IsSuccessStatusCode)
    {
        var data = await response.Content.ReadFromJsonAsync<ActionResponse<T>>();
        return new JsonResult(data);
    }
    return BadRequest();
}
```

### ❌ INCORRECT Pattern:
```javascript
// NEVER do this - no direct API calls from JavaScript
$.ajax({
    url: 'https://localhost:7030/api/endpoint',  // ❌ WRONG
    type: 'GET',
    ...
});
```

## Key Technologies

### Backend
- **Microsoft.EntityFrameworkCore** 9.0 - ORM for database access
- **Microsoft.AspNetCore.Identity** - User/role management
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation
- **FirmaXadesNet** - Digital signature generation (XAdES format for Hacienda)
- **MailKit/MimeKit** - Email sending

### Frontend
- **Bootstrap** - UI framework
- **jQuery** - DOM manipulation and AJAX
- **DataTables** - Advanced table features with search/sort/pagination

### External Integrations
- **Hacienda API** - Costa Rica government electronic invoicing API
  - Staging: `https://api-sandbox.comprobanteselectronicos.go.cr`
  - Production: `https://api.comprobanteselectronicos.go.cr`
- **BCCR API** - Costa Rica Central Bank for exchange rates
- **Hacienda IDP** - OAuth2 token service for API authentication

## Important Configuration

### Backend (appsettings.json)
- `ConnectionStrings:LocalConnection` - SQL Server connection
- `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key` - JWT configuration
- `Jwt:ExpirationHours` - Token expiration (default: 8 hours)
- `HaciendaApi:UrlRecepcionStaging/Production` - Government API URLs
- `HaciendaIdp:UrlStaging/Production` - OAuth2 token endpoints

### Frontend (appsettings.json)
- `ApiBaseUrl` - Backend API URL (e.g., `https://localhost:7030`)

## Database Schema Highlights

**55+ Entity Framework migrations** establishing:

### Core Entities
- `User` - System users (ASP.NET Identity)
- `Empresa` - Multi-tenant companies
- `Sucursal` - Branches
- `Terminal` - Point of sale terminals
- `Cliente` - Customers
- `Proveedor` - Suppliers
- `Producto` - Products/services
- `Categoria` - Product categories

### Electronic Documents (Hacienda v4.4)
- `Documento` - Main document (FE, TE, NC, ND, FEC, FEE, MR, REP)
- `DocumentoDetalle` - Line items
- `DocumentoDetalleImpuesto` - Taxes per line
- `DocumentoDetalleDescuento` - Discounts per line
- `DocumentoMedioPago` - Payment methods

### Catalog Entities (Costa Rica specific)
- `Provincia`, `Canton`, `Distrito`, `Barrio` - Geographic divisions
- `CAByS` - Goods and services classifier (13-digit code)
- `TipoCodigo`, `TipoDocumento`, `UnidadMedida`, `Impuesto`, etc.

### Business Entities
- `Inventario` - Stock tracking
- `MovimientoInventario` - Stock transactions
- `Gasto` - Expenses
- `Consecutivo` - Document sequential numbering
- `HaciendaToken` - OAuth2 token storage
- `Auditoria` - Change audit logs

## Coding Conventions

### Naming
- **Razor Pages**: Use entity name in plural (e.g., `Productos.cshtml`, NOT `Index.cshtml`)
- **Page Handlers**: Suffix with "Async" (e.g., `OnPostSaveAsync()`)
- **Controllers**: Suffix with "Controller" (e.g., `ClientesController`)
- **Interfaces**: Prefix with "I" (e.g., `IDocumentoService`)

### File Organization
```
Backend/
├── Controllers/          # API endpoints with [ApiController] and [Route]
├── Services/
│   ├── Interfaces/      # Service contracts
│   └── Implementations/ # Business logic
├── Repositories/
│   ├── Interfaces/      # Repository contracts
│   └── Implementations/ # Data access
├── UnitsOfWork/
│   ├── Interfaces/      # UoW contracts
│   └── Implementations/ # Transaction management
├── Helpers/             # UserHelper, etc.
├── Data/
│   ├── DataContext.cs   # EF Core DbContext
│   └── SeedDb.cs        # Database seeding
└── Migrations/          # EF Core migrations

Frontend/
├── Pages/
│   ├── [Entity].cshtml      # Razor view
│   ├── [Entity].cshtml.cs   # PageModel code-behind
│   └── Auth/                # Authentication pages
├── Services/            # IApiService, IAuthService
├── Helpers/
└── wwwroot/
    ├── css/
    ├── js/
    └── lib/             # Bootstrap, jQuery, DataTables

Shared/
├── Entities/            # Database models
├── DTOs/                # Data transfer objects
├── Enums/               # Enumerations
└── Responses/           # ActionResponse<T>
```

## Security Configuration

### Authentication
- **Backend**: JWT Bearer tokens (8-hour expiration)
- **Frontend**: Secure cookies (HttpOnly, SameSite=Lax)

### Authorization
- Role-based access control (RBAC)
- Roles: SuperUser, Administrador, Contador, Facturador, Vendedor, Inventarista, Consultor
- Privilege-based at module level with CRUD granularity

### Password Policy
- Minimum 6 characters (no special requirements)
- Account lockout: 5 failed attempts → 15 minute lockout

## Data Precision Standards

Follow Costa Rica Hacienda requirements:
- **Prices**: 5 decimals
- **Quantities**: 3 decimals
- **Totals**: 2 decimals
- **Exchange rates**: 5 decimals
- **ALWAYS use `decimal` type, NEVER `float` or `double`**

## Development Workflow

### Adding a New Entity

1. Create entity class in `Facturacion.Shared/Entities/`
2. Create DTOs in `Facturacion.Shared/DTOs/` (if needed)
3. Add `DbSet<T>` to `DataContext.cs`
4. Create migration: `dotnet ef migrations add Add[Entity]`
5. Update database: `dotnet ef database update`
6. Create repository interface/implementation (if complex queries needed)
7. Create Unit of Work interface/implementation (if custom repository)
8. Register in DI container (`Program.cs`)
9. Create API controller in `Backend/Controllers/`
10. Create Razor Page in `Frontend/Pages/`
11. Implement page handlers with IHttpClientFactory pattern

### Creating a New Page

1. Create `[Entity].cshtml` and `[Entity].cshtml.cs` in `Frontend/Pages/`
2. Use DataTables for lists with jQuery
3. Use Bootstrap modals for create/edit forms
4. Handlers call Backend via `IHttpClientFactory`
5. NEVER call API directly from JavaScript

## Testing Strategy

Currently no automated tests configured. When implementing tests:
- Unit tests for Services and Repositories
- Integration tests for API endpoints
- Consider testing Hacienda XML generation against XSD schemas

## Background Services

- `DocumentoEnvioBackgroundService` - Async queue for sending documents to Hacienda
  - Handles retries on failures
  - Updates document status in real-time

## Common Gotchas

1. **IHttpClientFactory Pattern**: Always use named client "FacturacionApi" in Frontend
2. **JWT Cookie**: Frontend must extract JWT from cookie "jwtAdmin" and pass as Bearer token
3. **Soft Delete**: Most entities use soft delete (mark inactive) rather than physical deletion
4. **Multi-tenancy**: Always filter by `EmpresaId` where applicable
5. **Consecutive Numbering**: Format is `SSS-TTTTT-NNNNNNNN-TT` (Sucursal-Terminal-Sequential-Type)
6. **Digital Signatures**: Use FirmaXadesNet for XAdES-EPES signatures (Hacienda requirement)
7. **DataTables Spanish**: Always set language to Spanish: `url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'`

## Documentation Files

Extensive documentation available in root directory:
- `ESPECIFICACION_SISTEMA.md` - Complete system specification (26KB)
- `ARCHITECTURE_GUIDE.md` - MJL architecture principles
- `BACKEND_PATTERNS.md` - Backend code patterns (35KB)
- `FRONTEND_PATTERNS.md` - Frontend code patterns (29KB)
- `SECURITY_CONFIG.md` - Security configuration details
- `HACIENDA_TOKEN_SERVICE_README.md` - OAuth2 token management
- `DOCUMENTACION_CAMPOS_V44.md` - Hacienda v4.4 field specifications
- `guia-facturacion-electronica-cr-v44.md` - Complete e-invoicing guide (80KB)

**IMPORTANT**: Always consult `ESPECIFICACION_SISTEMA.md` for business requirements and refer to pattern guides before implementing features.

## Default Credentials

Initial admin user (created by SeedDb):
- Email: `admin@facturacion.com`
- Password: `Admin123!`
- Role: SuperUser

## API Documentation

Swagger UI available at: `https://localhost:7030/swagger` (when Backend is running)

## Specialized Agents

This project includes 8 specialized agents configured in `.claude/agents/` that can be invoked for specific development tasks:

### 1. **full-stack-mjl**
**Use when**: Implementing complete end-to-end features spanning all layers
- Creates Entity/DTO in Shared layer
- Implements Repository/UnitOfWork/Controller in Backend
- Creates Razor Pages with DataTables in Frontend
- Follows complete MJL architecture workflow
- **Example**: "Create a complete Suppliers module with CRUD operations"

### 2. **dotnet-backend-architect**
**Use when**: Implementing or modifying backend functionality
- Creates Repository and Unit of Work patterns
- Implements API controllers with proper error handling
- Configures Entity Framework relationships
- Creates and manages migrations
- Ensures ActionResponse<T> pattern compliance
- **Example**: "Implement Repository pattern for Order entities"

### 3. **razor-frontend-developer**
**Use when**: Creating or modifying Razor Pages
- Creates properly named Razor Pages (plural entity names)
- Implements PageModels with async handlers
- Sets up DataTables with AJAX loading
- Creates Bootstrap modals for forms
- Implements JWT authentication from cookies
- **Example**: "Create a page to manage products with DataTable"

### 4. **database-architect**
**Use when**: Working with database schema or EF Core configuration
- Designs normalized database schemas
- Configures Entity Framework relationships in DataContext
- Creates and reviews migrations
- Defines indexes and constraints
- Ensures proper data types and precision
- **Example**: "Configure proper indexes and relationships for Paquete entity"

### 5. **code-reviewer**
**Use when**: Reviewing code for compliance with patterns
- Validates Repository/UnitOfWork pattern usage
- Checks naming conventions compliance
- Verifies security attributes are applied
- Ensures ActionResponse<T> usage
- Reviews Entity Framework optimizations
- **Example**: "Review the ProductRepository for pattern compliance"

### 6. **security-expert**
**Use when**: Implementing or reviewing security features
- Configures JWT authentication in Backend
- Sets up cookie authentication in Frontend
- Implements role-based authorization
- Reviews authentication/authorization code
- Ensures HTTPS and secure cookie policies
- Protects against CSRF, XSS, SQL Injection
- **Example**: "Review login endpoint for security best practices"

### 7. **project-manager**
**Use when**: Planning features or coordinating development tasks
- Creates user stories with acceptance criteria
- Breaks down features into Backend/Frontend tasks
- Validates architectural compliance
- Creates realistic sprint schedules
- Coordinates team assignments
- **Example**: "Create development plan for Customer module"

### 8. **ui-ux-designer**
**Use when**: Designing or reviewing user interfaces
- Designs Bootstrap and CoreUI interfaces
- Ensures accessibility (WCAG 2.1 Level AA)
- Reviews form UX and validation
- Defines reusable UI components
- Ensures design consistency
- **Example**: "Review the order form for accessibility and UX"

### How to Use Agents

To invoke an agent, use the Task tool with the appropriate `subagent_type`:
```
Task(
  subagent_type="full-stack-mjl",
  description="Create complete Product module",
  prompt="Implement a complete Product module with CRUD operations following MJL architecture..."
)
```

**Best Practice**: Choose the most specialized agent for your task. For example:
- Need full stack? → Use `full-stack-mjl`
- Backend only? → Use `dotnet-backend-architect`
- Frontend only? → Use `razor-frontend-developer`
- Database design? → Use `database-architect`

## Version Control

- **Current Branch**: `codex`
- **Main Branch**: `master`
- Use descriptive commit messages
- Recent commits show work on digital signatures (FIRMAXADES)
