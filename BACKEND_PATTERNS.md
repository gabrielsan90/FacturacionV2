# GUÍA DE PATRONES DEL BACKEND

## PROPÓSITO
Este documento define TODOS los patrones de código que DEBEN usarse en el proyecto Backend.

## ESTRUCTURA DE CARPETAS

```
[NOMBREPROYECTO].Backend/
├── Controllers/
│   ├── AccountsController.cs
│   ├── [Modulo]/
│   │   ├── [Entidad]Controller.cs
│   │   └── [OtraEntidad]Controller.cs
├── Data/
│   ├── DataContext.cs
│   └── SeedDb.cs
├── Helpers/
│   ├── IMailHelper.cs
│   ├── MailHelper.cs
│   ├── IWhatsAppHelper.cs
│   └── WhatsAppHelper.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IGenericRepository.cs
│   │   └── I[Entidad]Repository.cs
│   └── Implementations/
│       ├── GenericRepository.cs
│       └── [Entidad]Repository.cs
├── Services/
│   ├── Interfaces/
│   │   └── I[Servicio]Service.cs
│   └── Implementation/
│       └── [Servicio]Service.cs
├── UnitsOfWork/
│   ├── Interfaces/
│   │   ├── IGenericUnitOfWork.cs
│   │   └── I[Entidad]UnitOfWork.cs
│   └── Implementations/
│       ├── GenericUnitOfWork.cs
│       └── [Entidad]UnitOfWork.cs
├── Migrations/
├── Program.cs
├── appsettings.json
└── web.config
```

## 1. REPOSITORY PATTERN

### IGenericRepository.cs (OBLIGATORIO)

```csharp
using MJL.Shared.Responses;

namespace MJL.Backend.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<ActionResponse<T>> GetAsync(int id);
        Task<ActionResponse<IEnumerable<T>>> GetAsync();
        Task<ActionResponse<T>> AddAsync(T entity);
        Task<ActionResponse<T>> UpdateAsync(T entity);
        Task<ActionResponse<bool>> DeleteAsync(int id);
    }
}
```

### GenericRepository.cs (OBLIGATORIO)

```csharp
using Microsoft.EntityFrameworkCore;
using MJL.Backend.Data;
using MJL.Backend.Repositories.Interfaces;
using MJL.Shared.Responses;

namespace MJL.Backend.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DataContext _context;
        private readonly DbSet<T> _entity;

        public GenericRepository(DataContext context)
        {
            _context = context;
            _entity = _context.Set<T>();
        }

        public virtual async Task<ActionResponse<T>> GetAsync(int id)
        {
            var row = await _entity.FindAsync(id);
            if (row == null)
            {
                return new ActionResponse<T>
                {
                    WasSuccess = false,
                    Message = "Registro no encontrado"
                };
            }

            return new ActionResponse<T>
            {
                WasSuccess = true,
                Result = row
            };
        }

        public virtual async Task<ActionResponse<IEnumerable<T>>> GetAsync()
        {
            return new ActionResponse<IEnumerable<T>>
            {
                WasSuccess = true,
                Result = await _entity.ToListAsync()
            };
        }

        public virtual async Task<ActionResponse<T>> AddAsync(T entity)
        {
            try
            {
                _context.Add(entity);
                await _context.SaveChangesAsync();
                return new ActionResponse<T>
                {
                    WasSuccess = true,
                    Result = entity
                };
            }
            catch (DbUpdateException ex)
            {
                return new ActionResponse<T>
                {
                    WasSuccess = false,
                    Message = ex.InnerException?.Message ?? ex.Message
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
        }

        public virtual async Task<ActionResponse<T>> UpdateAsync(T entity)
        {
            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync();
                return new ActionResponse<T>
                {
                    WasSuccess = true,
                    Result = entity
                };
            }
            catch (DbUpdateException ex)
            {
                return new ActionResponse<T>
                {
                    WasSuccess = false,
                    Message = ex.InnerException?.Message ?? ex.Message
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
        }

        public virtual async Task<ActionResponse<bool>> DeleteAsync(int id)
        {
            var row = await _entity.FindAsync(id);
            if (row == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Registro no encontrado"
                };
            }

            try
            {
                _entity.Remove(row);
                await _context.SaveChangesAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
```

### Repositorio Específico (SOLO cuando se necesite)

**CREAR** repositorio específico cuando:
- Se necesiten queries con `Include()` para relaciones
- Se necesiten filtros complejos (`Where`, `GroupBy`, etc.)
- Se necesiten operaciones de negocio específicas

**EJEMPLO**: IPaqueteRepository.cs

```csharp
using MJL.Shared.DTO;
using MJL.Shared.Entities;
using MJL.Shared.Responses;

namespace MJL.Backend.Repositories.Interfaces
{
    public interface IPaqueteRepository
    {
        Task<ActionResponse<IEnumerable<PaqueteDto>>> GetPaquetesAsync();
        Task<ActionResponse<Paquete>> GetPaqueteAsync(int id);
        Task<ActionResponse<Paquete>> GetPaqueteByTrackingAsync(string numeroTracking);
        Task<ActionResponse<IEnumerable<Paquete>>> GetPaquetesPorUsuarioAsync(string userId);
        Task<ActionResponse<Paquete>> AddAsync(Paquete paquete);
        Task<ActionResponse<Paquete>> UpdateAsync(Paquete paquete);
        Task<ActionResponse<Paquete>> UpdateWeightPriceAndStatusAsync(Paquete paquete);
        Task<ActionResponse<Paquete>> UpdateWithNotificationsAsync(Paquete paquete);
        Task<ActionResponse<bool>> BulkUpdateStatusAsync(List<int> paqueteIds, int nuevoEstadoId);
        Task<ActionResponse<bool>> DeleteAsync(int id);
    }
}
```

**EJEMPLO**: PaqueteRepository.cs (implementación parcial)

```csharp
using Microsoft.EntityFrameworkCore;
using MJL.Backend.Data;
using MJL.Backend.Helpers;
using MJL.Backend.Repositories.Interfaces;
using MJL.Shared.DTO;
using MJL.Shared.Entities;
using MJL.Shared.Responses;

namespace MJL.Backend.Repositories.Implementations
{
    public class PaqueteRepository : IPaqueteRepository
    {
        private readonly DataContext _context;
        private readonly IMailHelper _mailHelper;
        private readonly IWhatsAppHelper _whatsAppHelper;

        public PaqueteRepository(DataContext context, IMailHelper mailHelper, IWhatsAppHelper whatsAppHelper)
        {
            _context = context;
            _mailHelper = mailHelper;
            _whatsAppHelper = whatsAppHelper;
        }

        public async Task<ActionResponse<IEnumerable<PaqueteDto>>> GetPaquetesAsync()
        {
            try
            {
                var paquetes = await _context.Paquetes
                    .Include(p => p.User)
                    .Include(p => p.DetallesTicket)
                    .Include(p => p.Estado)
                    .Select(p => new PaqueteDto
                    {
                        Id = p.Id,
                        NumeroTracking = p.NumeroTracking,
                        Descripcion = p.Descripcion,
                        Peso = p.Peso,
                        Valor = p.Valor,
                        UserId = p.UserId,
                        User = p.User,
                        EstadoId = p.EstadoId,
                        Estado = p.Estado,
                        Proveniente = p.Proveniente,
                        FechaPrealerta = p.FechaPrealerta,
                        FechaTransito = p.FechaTransito,
                        FechaInicioDisponible = p.FechaInicioDisponible,
                        FechaFinDisponible = p.FechaFinDisponible,
                        FechaRuta = p.FechaRuta,
                        FechaEntrega = p.FechaEntrega,
                        DetallesTicket = p.DetallesTicket,
                        Warehouse = p.Warehouse
                    })
                    .OrderByDescending(p => p.FechaPrealerta)
                    .ToListAsync();

                return new ActionResponse<IEnumerable<PaqueteDto>>
                {
                    WasSuccess = true,
                    Result = paquetes
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<IEnumerable<PaqueteDto>>
                {
                    WasSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse<Paquete>> UpdateAsync(Paquete paquete)
        {
            try
            {
                // PATRÓN: Buscar entidad existente primero
                var existingPaquete = await _context.Paquetes
                    .Include(p => p.User)
                    .Include(p => p.Estado)
                    .FirstOrDefaultAsync(p => p.Id == paquete.Id);

                if (existingPaquete == null)
                {
                    return new ActionResponse<Paquete>
                    {
                        WasSuccess = false,
                        Message = "Paquete no encontrado"
                    };
                }

                // PATRÓN: Actualizar solo propiedades escalares, NO propiedades de navegación
                existingPaquete.NumeroTracking = paquete.NumeroTracking;
                existingPaquete.Descripcion = paquete.Descripcion;
                existingPaquete.Peso = paquete.Peso;
                existingPaquete.Valor = paquete.Valor;
                existingPaquete.EstadoId = paquete.EstadoId;
                existingPaquete.UserId = paquete.UserId;
                existingPaquete.Proveniente = paquete.Proveniente;
                existingPaquete.FechaPrealerta = paquete.FechaPrealerta;
                existingPaquete.FechaTransito = paquete.FechaTransito;
                existingPaquete.FechaInicioDisponible = paquete.FechaInicioDisponible;
                existingPaquete.FechaFinDisponible = paquete.FechaFinDisponible;
                existingPaquete.FechaRuta = paquete.FechaRuta;
                existingPaquete.FechaEntrega = paquete.FechaEntrega;
                existingPaquete.Observaciones = paquete.Observaciones;

                await _context.SaveChangesAsync();

                return new ActionResponse<Paquete>
                {
                    WasSuccess = true,
                    Result = existingPaquete
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<Paquete>
                {
                    WasSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // Otros métodos similares...
    }
}
```

## 2. UNIT OF WORK PATTERN

### IGenericUnitOfWork.cs (OBLIGATORIO)

```csharp
using MJL.Shared.Responses;

namespace MJL.Backend.UnitsOfWork.Interfaces
{
    public interface IGenericUnitOfWork<T> where T : class
    {
        Task<ActionResponse<T>> GetAsync(int id);
        Task<ActionResponse<IEnumerable<T>>> GetAsync();
        Task<ActionResponse<T>> AddAsync(T entity);
        Task<ActionResponse<T>> UpdateAsync(T entity);
        Task<ActionResponse<bool>> DeleteAsync(int id);
    }
}
```

### GenericUnitOfWork.cs (OBLIGATORIO)

```csharp
using MJL.Backend.Repositories.Interfaces;
using MJL.Backend.UnitsOfWork.Interfaces;
using MJL.Shared.Responses;

namespace MJL.Backend.UnitsOfWork.Implementations
{
    public class GenericUnitOfWork<T> : IGenericUnitOfWork<T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public GenericUnitOfWork(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual async Task<ActionResponse<T>> AddAsync(T entity) => await _repository.AddAsync(entity);

        public virtual async Task<ActionResponse<bool>> DeleteAsync(int id) => await _repository.DeleteAsync(id);

        public virtual async Task<ActionResponse<T>> GetAsync(int id) => await _repository.GetAsync(id);

        public virtual async Task<ActionResponse<IEnumerable<T>>> GetAsync() => await _repository.GetAsync();

        public virtual async Task<ActionResponse<T>> UpdateAsync(T entity) => await _repository.UpdateAsync(entity);
    }
}
```

### Unit of Work Específico (cuando existe Repository específico)

**EJEMPLO**: IPaqueteUnitOfWork.cs

```csharp
using MJL.Shared.DTO;
using MJL.Shared.Entities;
using MJL.Shared.Responses;

namespace MJL.Backend.UnitsOfWork.Interfaces
{
    public interface IPaqueteUnitOfWork
    {
        Task<ActionResponse<IEnumerable<PaqueteDto>>> GetPaquetesAsync();
        Task<ActionResponse<Paquete>> GetPaqueteAsync(int id);
        Task<ActionResponse<Paquete>> GetPaqueteByTrackingAsync(string numeroTracking);
        Task<ActionResponse<IEnumerable<Paquete>>> GetPaquetesPorUsuarioAsync(string userId);
        Task<ActionResponse<Paquete>> AddAsync(Paquete paquete);
        Task<ActionResponse<Paquete>> UpdateAsync(Paquete paquete);
        Task<ActionResponse<Paquete>> UpdateWeightPriceAndStatusAsync(Paquete paquete);
        Task<ActionResponse<Paquete>> UpdateWithNotificationsAsync(Paquete paquete);
        Task<ActionResponse<bool>> BulkUpdateStatusAsync(List<int> paqueteIds, int nuevoEstadoId);
        Task<ActionResponse<bool>> DeleteAsync(int id);
    }
}
```

**EJEMPLO**: PaqueteUnitOfWork.cs

```csharp
using MJL.Backend.Repositories.Interfaces;
using MJL.Backend.UnitsOfWork.Interfaces;
using MJL.Shared.DTO;
using MJL.Shared.Entities;
using MJL.Shared.Responses;

namespace MJL.Backend.UnitsOfWork.Implementations
{
    public class PaqueteUnitOfWork : IPaqueteUnitOfWork
    {
        private readonly IPaqueteRepository _paqueteRepository;

        public PaqueteUnitOfWork(IPaqueteRepository paqueteRepository)
        {
            _paqueteRepository = paqueteRepository;
        }

        public async Task<ActionResponse<IEnumerable<PaqueteDto>>> GetPaquetesAsync()
            => await _paqueteRepository.GetPaquetesAsync();

        public async Task<ActionResponse<Paquete>> GetPaqueteAsync(int id)
            => await _paqueteRepository.GetPaqueteAsync(id);

        public async Task<ActionResponse<Paquete>> GetPaqueteByTrackingAsync(string numeroTracking)
            => await _paqueteRepository.GetPaqueteByTrackingAsync(numeroTracking);

        public async Task<ActionResponse<IEnumerable<Paquete>>> GetPaquetesPorUsuarioAsync(string userId)
            => await _paqueteRepository.GetPaquetesPorUsuarioAsync(userId);

        public async Task<ActionResponse<Paquete>> AddAsync(Paquete paquete)
            => await _paqueteRepository.AddAsync(paquete);

        public async Task<ActionResponse<Paquete>> UpdateAsync(Paquete paquete)
            => await _paqueteRepository.UpdateAsync(paquete);

        public async Task<ActionResponse<Paquete>> UpdateWeightPriceAndStatusAsync(Paquete paquete)
            => await _paqueteRepository.UpdateWeightPriceAndStatusAsync(paquete);

        public async Task<ActionResponse<Paquete>> UpdateWithNotificationsAsync(Paquete paquete)
            => await _paqueteRepository.UpdateWithNotificationsAsync(paquete);

        public async Task<ActionResponse<bool>> BulkUpdateStatusAsync(List<int> paqueteIds, int nuevoEstadoId)
            => await _paqueteRepository.BulkUpdateStatusAsync(paqueteIds, nuevoEstadoId);

        public async Task<ActionResponse<bool>> DeleteAsync(int id)
            => await _paqueteRepository.DeleteAsync(id);
    }
}
```

## 3. CONTROLLER PATTERN

### Controller con GenericUnitOfWork (para entidades simples)

```csharp
using Microsoft.AspNetCore.Mvc;
using MJL.Backend.UnitsOfWork.Interfaces;
using MJL.Shared.Entities;

namespace MJL.Backend.Controllers.[Modulo]
{
    [ApiController]
    [Route("api/[controller]")]
    public class [Entidad]Controller : Controller
    {
        private readonly IGenericUnitOfWork<[Entidad]> _unitOfWork;

        public [Entidad]Controller(IGenericUnitOfWork<[Entidad]> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAsync()
        {
            var action = await _unitOfWork.GetAsync();
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return BadRequest();
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetAsync(int id)
        {
            var action = await _unitOfWork.GetAsync(id);
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return NotFound();
        }

        [HttpPost]
        public virtual async Task<IActionResult> PostAsync([Entidad] model)
        {
            var action = await _unitOfWork.AddAsync(model);
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return BadRequest(action.Message);
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([Entidad] model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var action = await _unitOfWork.UpdateAsync(model);
            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(int id)
        {
            var action = await _unitOfWork.DeleteAsync(id);
            if (action.WasSuccess)
            {
                return NoContent();
            }
            return BadRequest(action.Message);
        }
    }
}
```

### Controller con Unit of Work Específico

**EJEMPLO**: PaqueteController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using MJL.Backend.UnitsOfWork.Interfaces;
using MJL.Backend.Data;
using MJL.Shared.Entities;
using MJL.Shared.DTO;

namespace MJL.Backend.Controllers.Paquetes
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaqueteController : Controller
    {
        private readonly IPaqueteUnitOfWork _unitOfWork;
        private readonly DataContext _context;

        public PaqueteController(IPaqueteUnitOfWork paqueteUnitOfWork, DataContext context)
        {
            _unitOfWork = paqueteUnitOfWork;
            _context = context;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAsync()
        {
            var action = await _unitOfWork.GetPaquetesAsync();
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return BadRequest();
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetAsync(int id)
        {
            var action = await _unitOfWork.GetPaqueteAsync(id);
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return NotFound();
        }

        [HttpGet("tracking/{numeroTracking}")]
        public virtual async Task<IActionResult> GetByTrackingAsync(string numeroTracking)
        {
            var action = await _unitOfWork.GetPaqueteByTrackingAsync(numeroTracking);
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return NotFound();
        }

        [HttpPost]
        public virtual async Task<IActionResult> PostAsync(Paquete model)
        {
            var action = await _unitOfWork.AddAsync(model);
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return BadRequest(action.Message);
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync(Paquete model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var action = await _unitOfWork.UpdateAsync(model);
            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }

        [HttpPut("UpdateWeightAndPrice")]
        public async Task<IActionResult> UpdateWeightAndPriceAsync(Paquete model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var action = await _unitOfWork.UpdateWeightPriceAndStatusAsync(model);
            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(int id)
        {
            var action = await _unitOfWork.DeleteAsync(id);
            if (action.WasSuccess)
            {
                return NoContent();
            }
            return BadRequest(action.Message);
        }

        [HttpPut("BulkStatusUpdate")]
        public async Task<IActionResult> BulkStatusUpdateAsync([FromBody] BulkStatusUpdateRequest request)
        {
            try
            {
                if (request == null || request.PaqueteIds == null || !request.PaqueteIds.Any())
                {
                    return BadRequest("Debe proporcionar al menos un paquete para actualizar");
                }

                var action = await _unitOfWork.BulkUpdateStatusAsync(request.PaqueteIds, request.NuevoEstadoId);
                if (action.WasSuccess)
                {
                    return Ok(new { success = true, message = $"Se actualizaron {request.PaqueteIds.Count} paquetes" });
                }
                return BadRequest(action.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
```

## 4. SERVICE PATTERN (cuando se necesite)

### USAR Services cuando:
- Se necesite lógica de negocio compleja que no pertenece a un Repository
- Se necesite coordinar múltiples repositorios
- Se necesiten operaciones que abarquen múltiples entidades

**EJEMPLO**: ICuponService.cs

```csharp
using MJL.Shared.Entities;
using MJL.Shared.Responses;

namespace MJL.Backend.Services.Interfaces
{
    public interface ICuponService
    {
        Task<ActionResponse<Cupon>> GenerarCuponAsync(string userId, decimal montoCompra);
        Task<ActionResponse<bool>> ValidarCuponAsync(string codigoCupon);
        Task<ActionResponse<bool>> CanjearCuponAsync(string codigoCupon, string userId);
    }
}
```

## 5. HELPER PATTERN

### USAR Helpers para:
- Funcionalidades transversales (email, SMS, WhatsApp, etc.)
- Operaciones que NO dependen de una entidad específica
- Servicios externos (APIs de terceros)

**EJEMPLO**: IMailHelper.cs

```csharp
using MJL.Shared.Entities;
using MJL.Shared.Responses;

namespace MJL.Backend.Helpers
{
    public interface IMailHelper
    {
        Task<ActionResponse<bool>> SendEmailAsync(string to, string subject, string body);
        Task<ActionResponse<bool>> SendPasswordResetEmailAsync(User user, string resetLink);
        Task<ActionResponse<bool>> SendPackageStatusUpdateEmailAsync(User user, string trackingNumber, string newStatus);
    }
}
```

## 6. DataContext

### DataContext.cs (OBLIGATORIO)

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MJL.Shared.Entities;

namespace MJL.Backend.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Paquete> Paquetes { get; set; }
        public DbSet<EstadoPaquete> EstadosPaquete { get; set; }
        public DbSet<TicketPago> TicketsPago { get; set; }
        public DbSet<DetalleTicketPago> DetallesTicketPago { get; set; }
        // ... otros DbSets

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PATRÓN: Configurar índices únicos
            modelBuilder.Entity<Paquete>()
                .HasIndex(p => p.NumeroTracking)
                .IsUnique();

            modelBuilder.Entity<TicketPago>()
                .HasIndex(t => t.NumeroTicket)
                .IsUnique();

            // PATRÓN: Ignorar propiedades calculadas
            modelBuilder.Entity<Paquete>()
                .Ignore(p => p.TieneAdjuntos);

            // PATRÓN: Configurar relaciones
            modelBuilder.Entity<Paquete>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Paquete>()
                .HasOne(p => p.Estado)
                .WithMany()
                .HasForeignKey(p => p.EstadoId)
                .OnDelete(DeleteBehavior.Restrict);

            // PATRÓN: Configurar precisión de decimales
            modelBuilder.Entity<Paquete>()
                .Property(p => p.Peso)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Paquete>()
                .Property(p => p.Valor)
                .HasPrecision(18, 2);

            // Configurar otras entidades...
        }
    }
}
```

## 7. SeedDb

### SeedDb.cs (OBLIGATORIO)

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MJL.Shared.Entities;
using MJL.Shared.Enums;

namespace MJL.Backend.Data
{
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
            await CheckEstadosPaqueteAsync();
            await CheckAdminUserAsync();
        }

        private async Task CheckRolesAsync()
        {
            await CheckRoleAsync("Admin");
            await CheckRoleAsync("User");
            await CheckRoleAsync("Employee");
        }

        private async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = roleName });
            }
        }

        private async Task CheckEstadosPaqueteAsync()
        {
            if (!_context.EstadosPaquete.Any())
            {
                _context.EstadosPaquete.Add(new EstadoPaquete { Nombre = "Prealerta" });
                _context.EstadosPaquete.Add(new EstadoPaquete { Nombre = "En tránsito" });
                _context.EstadosPaquete.Add(new EstadoPaquete { Nombre = "En bodega" });
                _context.EstadosPaquete.Add(new EstadoPaquete { Nombre = "Disponible para retiro" });
                _context.EstadosPaquete.Add(new EstadoPaquete { Nombre = "En ruta" });
                _context.EstadosPaquete.Add(new EstadoPaquete { Nombre = "Entregado" });
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckAdminUserAsync()
        {
            var adminUser = await _userManager.FindByEmailAsync("admin@mjlogistics.com");
            if (adminUser == null)
            {
                var user = new User
                {
                    FirstName = "Admin",
                    LastName = "MJL",
                    Email = "admin@mjlogistics.com",
                    UserName = "admin@mjlogistics.com",
                    PhoneNumber = "1234567890",
                    UserType = UserType.Admin,
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(user, "Admin123!");
                await _userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
```

## 8. Program.cs

### Program.cs (OBLIGATORIO)

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MJL.Backend.Data;
using MJL.Backend.Helpers;
using MJL.Backend.Repositories.Implementations;
using MJL.Backend.Repositories.Interfaces;
using MJL.Backend.UnitsOfWork.Implementations;
using MJL.Backend.UnitsOfWork.Interfaces;
using MJL.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configurar Controllers con manejo de ciclos de referencia
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.MaxDepth = 128;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PATRÓN: Registrar Repositories y UnitOfWork genéricos
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericUnitOfWork<>), typeof(GenericUnitOfWork<>));

// PATRÓN: Registrar Repositories y UnitOfWork específicos
builder.Services.AddScoped<IPaqueteRepository, PaqueteRepository>();
builder.Services.AddScoped<IPaqueteUnitOfWork, PaqueteUnitOfWork>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersUnitOfWork, UsersUnitOfWork>();

// PATRÓN: Registrar Helpers
builder.Services.AddScoped<IMailHelper, MailHelper>();
builder.Services.AddHttpClient<IWhatsAppHelper, WhatsAppHelper>();

// PATRÓN: Registrar SeedDb
builder.Services.AddTransient<SeedDb>();

// PATRÓN: Configurar DbContext
builder.Services.AddDbContext<DataContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

// PATRÓN: Configurar Identity
builder.Services.AddIdentity<User, IdentityRole>(x =>
{
    x.SignIn.RequireConfirmedEmail = true;
    x.User.RequireUniqueEmail = true;
    x.Password.RequireDigit = false;
    x.Password.RequiredUniqueChars = 0;
    x.Password.RequireLowercase = false;
    x.Password.RequireNonAlphanumeric = false;
    x.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// PATRÓN: Configurar JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
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

var app = builder.Build();

// PATRÓN: Ejecutar Seed al iniciar
SeedData(app);

void SeedData(WebApplication app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    using var scope = scopedFactory!.CreateScope();
    var service = scope.ServiceProvider.GetService<SeedDb>();
    service!.SeedAsync().Wait();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANTE: Orden correcto de middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## REGLAS OBLIGATORIAS

1. **SIEMPRE** usar `ActionResponse<T>` para respuestas de Repository
2. **SIEMPRE** usar async/await en todos los métodos de I/O
3. **SIEMPRE** usar try-catch en Repositories
4. **SIEMPRE** validar entidad existente antes de Update
5. **SIEMPRE** usar `Include()` cuando se necesiten relaciones
6. **NUNCA** actualizar propiedades de navegación en Update, solo IDs
7. **NUNCA** usar `new` para crear instancias de servicios, usar DI
8. **SIEMPRE** registrar servicios en Program.cs
9. **SIEMPRE** decorar Controllers con `[ApiController]` y `[Route]`
10. **SIEMPRE** validar ModelState en endpoints POST/PUT
