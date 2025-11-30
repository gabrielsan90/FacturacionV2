# GUÍA DE CONVENCIONES DE NOMBRES

## PROPÓSITO
Este documento define TODAS las convenciones de nombres que DEBEN usarse en TODOS los proyectos.

## 1. PROYECTOS

### Nombres de Proyectos

```
✅ CORRECTO:
[NOMBREPROYECTO].Backend
[NOMBREPROYECTO].Frontend
[NOMBREPROYECTO].FrontendAdmin
[NOMBREPROYECTO].Shared

Ejemplo:
MJL.Backend
MJL.Frontend
MJL.FrontendAdmin
MJL.Shared

❌ INCORRECTO:
[NOMBREPROYECTO]_Backend    (guión bajo)
[NOMBREPROYECTO]-Backend    (guión)
Backend[NOMBREPROYECTO]     (orden invertido)
[nombreproyecto].backend    (minúsculas)
```

### REGLAS:
- **SIEMPRE** usar punto (.) como separador
- **SIEMPRE** PascalCase
- **SIEMPRE** sufijo que describe el propósito

## 2. CARPETAS

### Backend

```
✅ CORRECTO:
Controllers/
Repositories/
  Interfaces/
  Implementations/
UnitsOfWork/
  Interfaces/
  Implementations/
Services/
  Interfaces/
  Implementation/
Helpers/
Data/
Migrations/

❌ INCORRECTO:
controllers/           (minúsculas)
Repository/            (singular)
Unit_Of_Work/          (guiones bajos)
service/               (minúsculas, singular)
```

### Frontend

```
✅ CORRECTO:
Pages/
  Auth/
  [Modulo]/
  Shared/
wwwroot/
  css/
  js/
  lib/
  img/

❌ INCORRECTO:
pages/                 (minúsculas)
Views/                 (nombre de MVC, no Razor Pages)
Components/            (para Razor Pages usar Shared)
```

### Shared

```
✅ CORRECTO:
Entities/
DTOs/
Enums/
Responses/

❌ INCORRECTO:
entities/              (minúsculas)
DTO/                   (singular)
Models/                (ambiguo, usar Entities)
```

### REGLAS:
- **SIEMPRE** PascalCase
- **SIEMPRE** plural para carpetas de colecciones
- **SIEMPRE** nombres descriptivos en inglés

## 3. ARCHIVOS

### Clases C#

```
✅ CORRECTO:
PaqueteController.cs
PaqueteRepository.cs
IPaqueteRepository.cs
PaqueteUnitOfWork.cs
DataContext.cs
SeedDb.cs

❌ INCORRECTO:
paquete-controller.cs      (minúsculas, guión)
PaqueteControllerCs.cs     (redundante)
IPaquete_Repository.cs     (guión bajo)
Paquete.Controller.cs      (punto innecesario)
```

### Razor Pages

```
✅ CORRECTO:
Paquetes.cshtml            (plural, nombre de entidad)
Paquetes.cshtml.cs
Usuarios.cshtml
TicketsPago.cshtml
Login.cshtml               (proceso específico)
ForgotPassword.cshtml

❌ INCORRECTO:
Index.cshtml               (NUNCA para entidades)
List.cshtml                (genérico)
Manage.cshtml              (genérico)
paquetes.cshtml            (minúsculas)
PaquetesList.cshtml        (redundante)
```

### JavaScript

```
✅ CORRECTO:
site.js
paquetes.js
usuarios.js

❌ INCORRECTO:
Site.js                    (PascalCase en JS)
paquetes-script.js         (redundante)
```

### CSS

```
✅ CORRECTO:
site.css
custom.css
login.css

❌ INCORRECTO:
Site.css                   (PascalCase en CSS)
my-styles.css              (genérico)
```

### REGLAS:
- **SIEMPRE** PascalCase para archivos C#
- **SIEMPRE** lowercase para archivos web (js, css)
- **SIEMPRE** extensión completa (.cshtml.cs, no .cs solamente)
- **NUNCA** usar "Index" para páginas de entidades

## 4. CLASES

### Controllers

```csharp
✅ CORRECTO:
public class PaqueteController : Controller
public class UsuarioController : Controller
public class AccountsController : ControllerBase

❌ INCORRECTO:
public class Paquete : Controller              (falta sufijo)
public class PaquetesController : Controller   (plural)
public class ControllerPaquete : Controller    (orden invertido)
```

### Repositories

```csharp
✅ CORRECTO:
public interface IPaqueteRepository
public class PaqueteRepository : IPaqueteRepository
public interface IGenericRepository<T>
public class GenericRepository<T> : IGenericRepository<T>

❌ INCORRECTO:
public interface PaqueteRepository             (sin prefijo I)
public class PaqueteRepositoryImpl             (sufijo Impl)
public interface IPaquetesRepository           (plural)
```

### Unit of Work

```csharp
✅ CORRECTO:
public interface IPaqueteUnitOfWork
public class PaqueteUnitOfWork : IPaqueteUnitOfWork
public interface IGenericUnitOfWork<T>
public class GenericUnitOfWork<T> : IGenericUnitOfWork<T>

❌ INCORRECTO:
public interface IPaqueteUOW                   (abreviatura)
public class PaqueteUow                        (abreviatura)
public interface IPaquetesUnitOfWork           (plural)
```

### Services

```csharp
✅ CORRECTO:
public interface ICuponService
public class CuponService : ICuponService

❌ INCORRECTO:
public interface ICuponServiceImpl             (sufijo Impl)
public class CuponesService                    (plural)
```

### Helpers

```csharp
✅ CORRECTO:
public interface IMailHelper
public class MailHelper : IMailHelper
public interface IWhatsAppHelper
public class WhatsAppHelper : IWhatsAppHelper

❌ INCORRECTO:
public interface IEmailHelper                  (usar Mail no Email)
public class EmailService                      (Helper no Service)
```

### PageModels

```csharp
✅ CORRECTO:
public class PaquetesModel : PageModel
public class UsuariosModel : PageModel
public class LoginModel : PageModel

❌ INCORRECTO:
public class PaquetesPageModel                 (redundante)
public class PaqueteModel                      (singular)
public class IndexModel                        (solo para páginas no-entidad)
```

### REGLAS:
- **SIEMPRE** sufijo que describe el tipo
- **SIEMPRE** prefijo "I" para interfaces
- **SIEMPRE** singular (excepto Razor Pages)
- **NUNCA** abreviaturas (UOW, Impl, etc.)

## 5. MÉTODOS

### Controllers

```csharp
✅ CORRECTO:
public async Task<IActionResult> GetAsync()
public async Task<IActionResult> GetAsync(int id)
public async Task<IActionResult> PostAsync(Paquete model)
public async Task<IActionResult> PutAsync(Paquete model)
public async Task<IActionResult> DeleteAsync(int id)
public async Task<IActionResult> GetByTrackingAsync(string numeroTracking)

❌ INCORRECTO:
public async Task<IActionResult> Get()         (sin Async)
public async Task<IActionResult> ObtenerAsync() (español)
public async Task<IActionResult> GetAll()       (no usar All, usar Get())
public async Task<IActionResult> Create()       (usar Post)
public async Task<IActionResult> Update()       (usar Put)
public async Task<IActionResult> Remove()       (usar Delete)
```

### Repositories

```csharp
✅ CORRECTO:
public async Task<ActionResponse<Paquete>> GetPaqueteAsync(int id)
public async Task<ActionResponse<IEnumerable<Paquete>>> GetPaquetesAsync()
public async Task<ActionResponse<Paquete>> AddAsync(Paquete paquete)
public async Task<ActionResponse<Paquete>> UpdateAsync(Paquete paquete)
public async Task<ActionResponse<bool>> DeleteAsync(int id)
public async Task<ActionResponse<Paquete>> GetPaqueteByTrackingAsync(string numeroTracking)

❌ INCORRECTO:
public async Task<ActionResponse<Paquete>> Get(int id)          (sin Async)
public async Task<ActionResponse<Paquete>> ObtenerAsync(int id) (español)
public async Task<ActionResponse<Paquete>> CreateAsync()        (usar Add)
public async Task<ActionResponse<Paquete>> SaveAsync()          (ambiguo)
```

### PageModel Handlers

```csharp
✅ CORRECTO:
public async Task OnGetAsync()
public async Task<IActionResult> OnPostAsync()
public async Task<IActionResult> OnPostSaveAsync()
public async Task<IActionResult> OnPostDeleteAsync(int id)
public async Task<IActionResult> OnGetDataAsync()
public async Task<IActionResult> OnGetDetailsAsync(int id)

❌ INCORRECTO:
public void OnGet()                            (no async cuando se hace I/O)
public async Task<IActionResult> OnPost()      (sin Async)
public async Task<IActionResult> OnPostGuardar() (español)
public async Task<IActionResult> SaveAsync()   (falta prefijo OnPost)
```

### Métodos Privados

```csharp
✅ CORRECTO:
private async Task LoadSelectListsAsync()
private async Task<string> GenerateTokenAsync(User user)
private bool ValidateModel(Paquete paquete)
private decimal CalculateTotal(List<Item> items)

❌ INCORRECTO:
private async Task loadSelectLists()           (camelCase)
private async Task<string> GenerateToken(User user) (sin Async)
private bool Validate_Model(Paquete paquete)   (guión bajo)
```

### REGLAS:
- **SIEMPRE** sufijo "Async" en métodos async
- **SIEMPRE** PascalCase
- **SIEMPRE** verbos en inglés
- **SIEMPRE** descriptivos (GetByTracking, no GetByNo)
- **NUNCA** abreviaturas (GetPaq, no GetPaquete)
- **Handlers**: SIEMPRE prefijo "OnGet" o "OnPost"

## 6. VARIABLES Y PROPIEDADES

### Propiedades Públicas

```csharp
✅ CORRECTO:
public int Id { get; set; }
public string NumeroTracking { get; set; } = null!;
public DateTime? FechaPrealerta { get; set; }
public User? User { get; set; }
public IEnumerable<SelectListItem> Estados { get; set; }

❌ INCORRECTO:
public int id { get; set; }                    (camelCase)
public string numero_tracking { get; set; }    (snake_case)
public DateTime? fechaPrealerta { get; set; }  (camelCase)
public User? user { get; set; }                (camelCase)
```

### Variables Privadas / Campos

```csharp
✅ CORRECTO:
private readonly DataContext _context;
private readonly IMailHelper _mailHelper;
private readonly ILogger<PaqueteController> _logger;

❌ INCORRECTO:
private readonly DataContext context;          (sin prefijo)
private readonly IMailHelper mailHelper;       (sin prefijo)
private readonly ILogger<PaqueteController> m_logger; (prefijo m_)
private readonly DataContext _Context;         (PascalCase con prefijo)
```

### Variables Locales

```csharp
✅ CORRECTO:
var paquete = await _context.Paquetes.FindAsync(id);
var user = await _userManager.FindByEmailAsync(email);
var client = _httpClientFactory.CreateClient("MJLApi");
int count = paquetes.Count();

❌ INCORRECTO:
var Paquete = await _context.Paquetes.FindAsync(id);     (PascalCase)
var _paquete = await _context.Paquetes.FindAsync(id);    (prefijo)
var numero_tracking = paquete.NumeroTracking;            (snake_case)
```

### Parámetros de Métodos

```csharp
✅ CORRECTO:
public async Task<ActionResponse<Paquete>> GetPaqueteAsync(int id)
public async Task UpdateAsync(Paquete paquete, User user)
public bool Validate(string numeroTracking, DateTime fecha)

❌ INCORRECTO:
public async Task<ActionResponse<Paquete>> GetPaqueteAsync(int Id)      (PascalCase)
public async Task UpdateAsync(Paquete Paquete, User User)                (PascalCase)
public bool Validate(string numero_tracking, DateTime Fecha)            (mixto)
```

### REGLAS:
- **Propiedades públicas**: PascalCase
- **Campos privados**: camelCase con prefijo _underscore
- **Variables locales**: camelCase
- **Parámetros**: camelCase
- **NUNCA** usar snake_case
- **NUNCA** usar prefijo m_ o hungarian notation

## 7. NAMESPACES

```csharp
✅ CORRECTO:
namespace MJL.Backend.Controllers.Paquetes
namespace MJL.Backend.Repositories.Interfaces
namespace MJL.Backend.UnitsOfWork.Implementations
namespace MJL.Shared.Entities
namespace MJL.Shared.DTOs
namespace MJL.FrontendAdmin.Pages.Auth

❌ INCORRECTO:
namespace MJL.Backend.Controllers.paquetes     (minúsculas)
namespace MJL_Backend_Controllers              (guiones bajos)
namespace MJL.Backend.Controllers.Paquete      (singular en módulo)
namespace Controllers.Paquetes                 (sin proyecto)
```

### REGLAS:
- **SIEMPRE** seguir estructura de carpetas
- **SIEMPRE** PascalCase
- **SIEMPRE** plural para módulos de entidades

## 8. BASES DE DATOS

### Tablas

```sql
✅ CORRECTO:
Paquetes
Usuarios
EstadosPaquete
TicketsPago
DetallesTicketPago

❌ INCORRECTO:
paquetes               (minúsculas)
Paquete                (singular)
tbl_Paquetes           (prefijo)
Paquetes_Table         (sufijo)
```

### Columnas

```sql
✅ CORRECTO:
Id
NumeroTracking
FechaPrealerta
UserId
EstadoId

❌ INCORRECTO:
id                     (minúsculas)
numero_tracking        (snake_case)
fecha_prealerta        (snake_case)
UserID                 (ID en mayúsculas)
```

### REGLAS:
- **SIEMPRE** PascalCase
- **SIEMPRE** plural para tablas
- **SIEMPRE** singular para columnas
- **NUNCA** prefijos (tbl_, usr_)
- **NUNCA** snake_case

## 9. RUTAS Y URLs

### API Routes

```csharp
✅ CORRECTO:
[Route("api/[controller]")]
/api/paquete
/api/paquete/{id}
/api/paquete/tracking/{numeroTracking}
/api/accounts/Login
/api/accounts/Register

❌ INCORRECTO:
[Route("API/[controller]")]                    (mayúsculas)
/api/Paquete                                   (PascalCase)
/api/paquetes                                  (plural)
/api/paquete/get-by-tracking                   (verbos en ruta)
```

### Razor Pages Routes

```
✅ CORRECTO:
/Paquetes
/Usuarios
/TicketsPago
/Auth/Login
/Auth/ForgotPassword

❌ INCORRECTO:
/paquetes                                      (minúsculas)
/Paquete                                       (singular)
/Index                                         (genérico)
/Manage                                        (genérico)
```

### REGLAS:
- **API**: lowercase, singular
- **Razor Pages**: PascalCase, plural para entidades
- **NUNCA** verbos en rutas de API (get, post, update)

## 10. JAVASCRIPT/TYPESCRIPT

### Funciones

```javascript
✅ CORRECTO:
function loadDataTable() { }
function edit(id) { }
function save() { }
function deleteRecord(id) { }

❌ INCORRECTO:
function LoadDataTable() { }               (PascalCase)
function Editar(id) { }                    (español)
function Save_Record() { }                 (snake_case)
```

### Variables

```javascript
✅ CORRECTO:
var table;
var apiBaseUrl;
const formData = new FormData();
let paqueteId = 0;

❌ INCORRECTO:
var Table;                                 (PascalCase)
var api_base_url;                          (snake_case)
const FormData = new FormData();           (PascalCase)
```

### REGLAS:
- **SIEMPRE** camelCase
- **NUNCA** PascalCase (excepto constructores)
- **NUNCA** snake_case

## RESUMEN DE CONVENCIONES

| Elemento | Convención | Ejemplo |
|----------|-----------|---------|
| Proyectos | PascalCase con punto | MJL.Backend |
| Carpetas | PascalCase, plural | Controllers/ |
| Archivos C# | PascalCase con sufijo | PaqueteController.cs |
| Archivos web | lowercase | site.js, custom.css |
| Razor Pages | PascalCase, plural | Paquetes.cshtml |
| Clases | PascalCase con sufijo | PaqueteController |
| Interfaces | I + PascalCase | IPaqueteRepository |
| Métodos | PascalCase + Async | GetPaqueteAsync() |
| Handlers | OnGet/OnPost + Async | OnPostSaveAsync() |
| Propiedades | PascalCase | NumeroTracking |
| Campos privados | _camelCase | _context |
| Variables locales | camelCase | paquete |
| Parámetros | camelCase | id, user |
| Namespaces | PascalCase | MJL.Backend.Controllers |
| Tablas DB | PascalCase, plural | Paquetes |
| Columnas DB | PascalCase, singular | NumeroTracking |
| API Routes | lowercase, singular | /api/paquete |
| Page Routes | PascalCase, plural | /Paquetes |
| JavaScript | camelCase | loadDataTable() |
