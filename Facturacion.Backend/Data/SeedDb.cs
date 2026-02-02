using Facturacion.Backend.Helpers;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Entities.Catalogos;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Data;

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

        // Orden de ejecución:
        await CheckModulosAsync();
        await CheckRolesAsync();
        await CheckPrivilegiosAsync();
        await CheckSuperUserAsync();

        // Catálogos de Hacienda
        await CheckProvinciasAsync();
        await CheckCantonesAsync();
        await CheckDistritosAsync();
        await CheckTiposCodigoAsync();
        await CheckTiposDocumentoAsync();
        await CheckUnidadesMedidaAsync();
        await CheckImpuestosAsync();
        await CheckCodigosExoneracionAsync();
        await CheckCondicionesVentaAsync();
        await CheckMediosPagoAsync();

        // Catálogos Hacienda v4.4 (Nuevos)
        await CheckTarifasIVAAsync();
        await CheckTiposDescuentoHaciendaAsync();
        await CheckTiposDocumentoReferenciaAsync();
        await CheckCodigosReferenciaAsync();
        await CheckFormasFarmaceuticasAsync(); // NUEVO v4.4 - M7

        // Datos de ejemplo para empresas existentes
        await CheckCategoriasEjemploAsync();
    }

    // ===========================================
    // MÓDULOS DEL SISTEMA
    // ===========================================

    private async Task CheckModulosAsync()
    {
        if (!_context.Modulos.Any())
        {
            var modulos = new List<Modulo>
            {
                new() { Nombre = "Documentos Electrónicos", Descripcion = "Gestión de documentos electrónicos (FE, TE, NC, ND, etc.)", Orden = 1, Icono = "fa-file-invoice", Activo = true },
                new() { Nombre = "Clientes", Descripcion = "Gestión de clientes", Orden = 2, Icono = "fa-users", Activo = true },
                new() { Nombre = "Proveedores", Descripcion = "Gestión de proveedores", Orden = 3, Icono = "fa-truck", Activo = true },
                new() { Nombre = "Productos", Descripcion = "Gestión de productos y servicios", Orden = 4, Icono = "fa-box", Activo = true },
                new() { Nombre = "Gastos", Descripcion = "Gestión de gastos", Orden = 5, Icono = "fa-money-bill", Activo = true },
                new() { Nombre = "Inventario", Descripcion = "Control de inventario", Orden = 6, Icono = "fa-warehouse", Activo = true },
                new() { Nombre = "Reportes", Descripcion = "Reportes y estadísticas", Orden = 7, Icono = "fa-chart-bar", Activo = true },
                new() { Nombre = "Usuarios", Descripcion = "Gestión de usuarios", Orden = 8, Icono = "fa-user-cog", Activo = true },
                new() { Nombre = "Empresas", Descripcion = "Gestión de empresas", Orden = 9, Icono = "fa-building", Activo = true },
                new() { Nombre = "Configuración", Descripcion = "Configuración del sistema", Orden = 10, Icono = "fa-cog", Activo = true },
                new() { Nombre = "Catálogos Hacienda", Descripcion = "Catálogos de Hacienda", Orden = 11, Icono = "fa-database", Activo = true },
                new() { Nombre = "Recepción de Documentos", Descripcion = "Documentos recibidos", Orden = 12, Icono = "fa-inbox", Activo = true },
                new() { Nombre = "REP", Descripcion = "Recibos Electrónicos de Pago", Orden = 13, Icono = "fa-receipt", Activo = true },
                new() { Nombre = "Sucursales/Terminales", Descripcion = "Gestión de sucursales y terminales", Orden = 14, Icono = "fa-store", Activo = true }
            };

            _context.Modulos.AddRange(modulos);
            await _context.SaveChangesAsync();
        }
    }

    // ===========================================
    // ROLES DEL SISTEMA
    // ===========================================

    private async Task CheckRolesAsync()
    {
        // Roles predefinidos del sistema según especificación
        var roles = new List<(string Nombre, string Descripcion, bool EsSistema)>
        {
            ("SuperUser", "Control total del sistema, puede crear y gestionar empresas", true),
            ("Administrador de Empresa", "Gestión completa de su empresa, puede crear usuarios y asignar roles", true),
            ("Contador", "Acceso a reportes financieros, documentos y gastos", true),
            ("Facturador", "Crear y gestionar documentos electrónicos", true),
            ("Vendedor", "Crear facturas y gestionar clientes", true),
            ("Inventarista", "Gestión de productos, inventario y traslados", true),
            ("Consultor", "Solo lectura de reportes y documentos", true)
        };

        foreach (var (nombre, descripcion, esSistema) in roles)
        {
            var roleExists = await _context.Roles.AnyAsync(r => r.Nombre == nombre);
            if (!roleExists)
            {
                var rol = new Rol
                {
                    Id = Guid.NewGuid().ToString(),
                    Nombre = nombre,
                    Name = nombre,
                    NormalizedName = nombre.ToUpper(),
                    Descripcion = descripcion,
                    EsSistema = esSistema,
                    Activo = true,
                    FechaCreacion = FechaCostaRicaHelper.Ahora
                };

                _context.Roles.Add(rol);
            }
        }

        await _context.SaveChangesAsync();
    }

    // ===========================================
    // PRIVILEGIOS POR MÓDULO
    // ===========================================

    private async Task CheckPrivilegiosAsync()
    {
        if (_context.Privilegios.Any())
            return;

        var modulos = await _context.Modulos.ToListAsync();
        var privilegios = new List<Privilegio>();

        foreach (var modulo in modulos)
        {
            // Privilegios CRUD básicos para cada módulo
            var acciones = modulo.Nombre switch
            {
                "Documentos Electrónicos" => new[] { AccionPrivilegio.Crear, AccionPrivilegio.Ver, AccionPrivilegio.Editar, AccionPrivilegio.Eliminar },
                "Inventario" => new[] { AccionPrivilegio.Ver, AccionPrivilegio.Crear, AccionPrivilegio.Editar },
                "Reportes" => new[] { AccionPrivilegio.Ver },
                "Recepción de Documentos" => new[] { AccionPrivilegio.Ver },
                _ => new[] { AccionPrivilegio.Crear, AccionPrivilegio.Ver, AccionPrivilegio.Editar, AccionPrivilegio.Eliminar }
            };

            foreach (var accion in acciones)
            {
                privilegios.Add(new Privilegio
                {
                    ModuloId = modulo.Id,
                    Accion = accion,
                    Nombre = $"{modulo.Nombre}.{accion}",
                    Descripcion = $"Permiso para {accion.ToString().ToLower()} en {modulo.Nombre}"
                });
            }
        }

        _context.Privilegios.AddRange(privilegios);
        await _context.SaveChangesAsync();
    }

    // ===========================================
    // USUARIO SUPERUSER
    // ===========================================

    private async Task CheckSuperUserAsync()
    {
        var superUserEmail = "superuser@facturacion.com";
        var user = await _userManager.FindByEmailAsync(superUserEmail);

        if (user == null)
        {
            user = new User
            {
                FullName = "SuperUser",
                Email = superUserEmail,
                UserName = superUserEmail,
                Document = "000000000",
                PhoneNumber = "00000000",
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(user, "SuperUser123!");

            var superUserRole = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "SuperUser");
            if (superUserRole != null)
            {
                await _userManager.AddToRoleAsync(user, superUserRole.Name!);
            }

            // Asignar todos los privilegios al SuperUser
            var todosLosPrivilegios = await _context.Privilegios.ToListAsync();
            var rolesPrivilegios = todosLosPrivilegios.Select(p => new RolPrivilegio
            {
                RolId = superUserRole!.Id,
                PrivilegioId = p.Id
            }).ToList();

            _context.RolesPrivilegios.AddRange(rolesPrivilegios);
            await _context.SaveChangesAsync();
        }
    }

    // ===========================================
    // CATÁLOGOS DE HACIENDA
    // ===========================================

    private async Task CheckProvinciasAsync()
    {
        if (!_context.Provincias.Any())
        {
            var provincias = new List<Provincia>
            {
                new() { Codigo = "1", Descripcion = "San José", Activo = true },
                new() { Codigo = "2", Descripcion = "Alajuela", Activo = true },
                new() { Codigo = "3", Descripcion = "Cartago", Activo = true },
                new() { Codigo = "4", Descripcion = "Heredia", Activo = true },
                new() { Codigo = "5", Descripcion = "Guanacaste", Activo = true },
                new() { Codigo = "6", Descripcion = "Puntarenas", Activo = true },
                new() { Codigo = "7", Descripcion = "Limón", Activo = true }
            };

            _context.Provincias.AddRange(provincias);
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckCantonesAsync()
    {
        if (!_context.Cantones.Any())
        {
            var provincias = await _context.Provincias.ToListAsync();
            var cantones = new List<Canton>();

            // San José (Provincia 1)
            var sanJose = provincias.First(p => p.Codigo == "1");
            cantones.AddRange(new[]
            {
                new Canton { Codigo = "101", Descripcion = "San José", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "102", Descripcion = "Escazú", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "103", Descripcion = "Desamparados", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "104", Descripcion = "Puriscal", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "105", Descripcion = "Tarrazú", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "106", Descripcion = "Aserrí", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "107", Descripcion = "Mora", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "108", Descripcion = "Goicoechea", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "109", Descripcion = "Santa Ana", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "110", Descripcion = "Alajuelita", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "111", Descripcion = "Vázquez de Coronado", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "112", Descripcion = "Acosta", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "113", Descripcion = "Tibás", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "114", Descripcion = "Moravia", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "115", Descripcion = "Montes de Oca", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "116", Descripcion = "Turrubares", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "117", Descripcion = "Dota", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "118", Descripcion = "Curridabat", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "119", Descripcion = "Pérez Zeledón", ProvinciaId = sanJose.Id, Activo = true },
                new Canton { Codigo = "120", Descripcion = "León Cortés", ProvinciaId = sanJose.Id, Activo = true }
            });

            // Alajuela (Provincia 2)
            var alajuela = provincias.First(p => p.Codigo == "2");
            cantones.AddRange(new[]
            {
                new Canton { Codigo = "201", Descripcion = "Alajuela", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "202", Descripcion = "San Ramón", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "203", Descripcion = "Grecia", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "204", Descripcion = "San Mateo", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "205", Descripcion = "Atenas", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "206", Descripcion = "Naranjo", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "207", Descripcion = "Palmares", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "208", Descripcion = "Poás", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "209", Descripcion = "Orotina", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "210", Descripcion = "San Carlos", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "211", Descripcion = "Zarcero", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "212", Descripcion = "Sarchí", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "213", Descripcion = "Upala", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "214", Descripcion = "Los Chiles", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "215", Descripcion = "Guatuso", ProvinciaId = alajuela.Id, Activo = true },
                new Canton { Codigo = "216", Descripcion = "Río Cuarto", ProvinciaId = alajuela.Id, Activo = true }
            });

            // Cartago (Provincia 3)
            var cartago = provincias.First(p => p.Codigo == "3");
            cantones.AddRange(new[]
            {
                new Canton { Codigo = "301", Descripcion = "Cartago", ProvinciaId = cartago.Id, Activo = true },
                new Canton { Codigo = "302", Descripcion = "Paraíso", ProvinciaId = cartago.Id, Activo = true },
                new Canton { Codigo = "303", Descripcion = "La Unión", ProvinciaId = cartago.Id, Activo = true },
                new Canton { Codigo = "304", Descripcion = "Jiménez", ProvinciaId = cartago.Id, Activo = true },
                new Canton { Codigo = "305", Descripcion = "Turrialba", ProvinciaId = cartago.Id, Activo = true },
                new Canton { Codigo = "306", Descripcion = "Alvarado", ProvinciaId = cartago.Id, Activo = true },
                new Canton { Codigo = "307", Descripcion = "Oreamuno", ProvinciaId = cartago.Id, Activo = true },
                new Canton { Codigo = "308", Descripcion = "El Guarco", ProvinciaId = cartago.Id, Activo = true }
            });

            // Heredia (Provincia 4)
            var heredia = provincias.First(p => p.Codigo == "4");
            cantones.AddRange(new[]
            {
                new Canton { Codigo = "401", Descripcion = "Heredia", ProvinciaId = heredia.Id, Activo = true },
                new Canton { Codigo = "402", Descripcion = "Barva", ProvinciaId = heredia.Id, Activo = true },
                new Canton { Codigo = "403", Descripcion = "Santo Domingo", ProvinciaId = heredia.Id, Activo = true },
                new Canton { Codigo = "404", Descripcion = "Santa Bárbara", ProvinciaId = heredia.Id, Activo = true },
                new Canton { Codigo = "405", Descripcion = "San Rafael", ProvinciaId = heredia.Id, Activo = true },
                new Canton { Codigo = "406", Descripcion = "San Isidro", ProvinciaId = heredia.Id, Activo = true },
                new Canton { Codigo = "407", Descripcion = "Belén", ProvinciaId = heredia.Id, Activo = true },
                new Canton { Codigo = "408", Descripcion = "Flores", ProvinciaId = heredia.Id, Activo = true },
                new Canton { Codigo = "409", Descripcion = "San Pablo", ProvinciaId = heredia.Id, Activo = true },
                new Canton { Codigo = "410", Descripcion = "Sarapiquí", ProvinciaId = heredia.Id, Activo = true }
            });

            // Guanacaste (Provincia 5)
            var guanacaste = provincias.First(p => p.Codigo == "5");
            cantones.AddRange(new[]
            {
                new Canton { Codigo = "501", Descripcion = "Liberia", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "502", Descripcion = "Nicoya", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "503", Descripcion = "Santa Cruz", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "504", Descripcion = "Bagaces", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "505", Descripcion = "Carrillo", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "506", Descripcion = "Cañas", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "507", Descripcion = "Abangares", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "508", Descripcion = "Tilarán", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "509", Descripcion = "Nandayure", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "510", Descripcion = "La Cruz", ProvinciaId = guanacaste.Id, Activo = true },
                new Canton { Codigo = "511", Descripcion = "Hojancha", ProvinciaId = guanacaste.Id, Activo = true }
            });

            // Puntarenas (Provincia 6)
            var puntarenas = provincias.First(p => p.Codigo == "6");
            cantones.AddRange(new[]
            {
                new Canton { Codigo = "601", Descripcion = "Puntarenas", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "602", Descripcion = "Esparza", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "603", Descripcion = "Buenos Aires", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "604", Descripcion = "Montes de Oro", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "605", Descripcion = "Osa", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "606", Descripcion = "Quepos", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "607", Descripcion = "Golfito", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "608", Descripcion = "Coto Brus", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "609", Descripcion = "Parrita", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "610", Descripcion = "Corredores", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "611", Descripcion = "Garabito", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "612", Descripcion = "Monteverde", ProvinciaId = puntarenas.Id, Activo = true },
                new Canton { Codigo = "613", Descripcion = "Puerto Jiménez", ProvinciaId = puntarenas.Id, Activo = true }
            });

            // Limón (Provincia 7)
            var limon = provincias.First(p => p.Codigo == "7");
            cantones.AddRange(new[]
            {
                new Canton { Codigo = "701", Descripcion = "Limón", ProvinciaId = limon.Id, Activo = true },
                new Canton { Codigo = "702", Descripcion = "Pococí", ProvinciaId = limon.Id, Activo = true },
                new Canton { Codigo = "703", Descripcion = "Siquirres", ProvinciaId = limon.Id, Activo = true },
                new Canton { Codigo = "704", Descripcion = "Talamanca", ProvinciaId = limon.Id, Activo = true },
                new Canton { Codigo = "705", Descripcion = "Matina", ProvinciaId = limon.Id, Activo = true },
                new Canton { Codigo = "706", Descripcion = "Guácimo", ProvinciaId = limon.Id, Activo = true }
            });

            _context.Cantones.AddRange(cantones);
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckDistritosAsync()
    {
        if (!_context.Distritos.Any())
        {
            var cantones = await _context.Cantones.ToDictionaryAsync(c => c.Codigo, c => c.Id);
            var distritos = new List<Distrito>();

            // =============================================
            // PROVINCIA 1: SAN JOSÉ (20 cantones)
            // =============================================

            // 101 - San José
            if (cantones.TryGetValue("101", out var c101))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "10101", Descripcion = "Carmen", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10102", Descripcion = "Merced", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10103", Descripcion = "Hospital", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10104", Descripcion = "Catedral", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10105", Descripcion = "Zapote", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10106", Descripcion = "San Francisco de Dos Ríos", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10107", Descripcion = "Uruca", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10108", Descripcion = "Mata Redonda", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10109", Descripcion = "Pavas", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10110", Descripcion = "Hatillo", CantonId = c101, Activo = true },
                    new Distrito { Codigo = "10111", Descripcion = "San Sebastián", CantonId = c101, Activo = true }
                });
            }

            // 102 - Escazú
            if (cantones.TryGetValue("102", out var c102))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "10201", Descripcion = "Escazú", CantonId = c102, Activo = true },
                    new Distrito { Codigo = "10202", Descripcion = "San Antonio", CantonId = c102, Activo = true },
                    new Distrito { Codigo = "10203", Descripcion = "San Rafael", CantonId = c102, Activo = true }
                });
            }

            // 103 - Desamparados
            if (cantones.TryGetValue("103", out var c103))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "10301", Descripcion = "Desamparados", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10302", Descripcion = "San Miguel", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10303", Descripcion = "San Juan de Dios", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10304", Descripcion = "San Rafael Arriba", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10305", Descripcion = "San Antonio", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10306", Descripcion = "Frailes", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10307", Descripcion = "Patarrá", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10308", Descripcion = "San Cristóbal", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10309", Descripcion = "Rosario", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10310", Descripcion = "Damas", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10311", Descripcion = "San Rafael Abajo", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10312", Descripcion = "Gravilias", CantonId = c103, Activo = true },
                    new Distrito { Codigo = "10313", Descripcion = "Los Guido", CantonId = c103, Activo = true }
                });
            }

            // 104 - Puriscal
            if (cantones.TryGetValue("104", out var c104))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "10401", Descripcion = "Santiago", CantonId = c104, Activo = true },
                    new Distrito { Codigo = "10402", Descripcion = "Mercedes Sur", CantonId = c104, Activo = true },
                    new Distrito { Codigo = "10403", Descripcion = "Barbacoas", CantonId = c104, Activo = true },
                    new Distrito { Codigo = "10404", Descripcion = "Grifo Alto", CantonId = c104, Activo = true },
                    new Distrito { Codigo = "10405", Descripcion = "San Rafael", CantonId = c104, Activo = true },
                    new Distrito { Codigo = "10406", Descripcion = "Candelarita", CantonId = c104, Activo = true },
                    new Distrito { Codigo = "10407", Descripcion = "Desamparaditos", CantonId = c104, Activo = true },
                    new Distrito { Codigo = "10408", Descripcion = "San Antonio", CantonId = c104, Activo = true },
                    new Distrito { Codigo = "10409", Descripcion = "Chires", CantonId = c104, Activo = true }
                });
            }

            // 105 - Tarrazú
            if (cantones.TryGetValue("105", out var c105))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "10501", Descripcion = "San Marcos", CantonId = c105, Activo = true },
                    new Distrito { Codigo = "10502", Descripcion = "San Lorenzo", CantonId = c105, Activo = true },
                    new Distrito { Codigo = "10503", Descripcion = "San Carlos", CantonId = c105, Activo = true }
                });
            }

            // 106 - Aserrí
            if (cantones.TryGetValue("106", out var c106))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "10601", Descripcion = "Aserrí", CantonId = c106, Activo = true },
                    new Distrito { Codigo = "10602", Descripcion = "Tarbaca", CantonId = c106, Activo = true },
                    new Distrito { Codigo = "10603", Descripcion = "Vuelta de Jorco", CantonId = c106, Activo = true },
                    new Distrito { Codigo = "10604", Descripcion = "San Gabriel", CantonId = c106, Activo = true },
                    new Distrito { Codigo = "10605", Descripcion = "Legua", CantonId = c106, Activo = true },
                    new Distrito { Codigo = "10606", Descripcion = "Monterrey", CantonId = c106, Activo = true },
                    new Distrito { Codigo = "10607", Descripcion = "Salitrillos", CantonId = c106, Activo = true }
                });
            }

            // 107 - Mora
            if (cantones.TryGetValue("107", out var c107))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "10701", Descripcion = "Colón", CantonId = c107, Activo = true },
                    new Distrito { Codigo = "10702", Descripcion = "Guayabo", CantonId = c107, Activo = true },
                    new Distrito { Codigo = "10703", Descripcion = "Tabarcia", CantonId = c107, Activo = true },
                    new Distrito { Codigo = "10704", Descripcion = "Piedras Negras", CantonId = c107, Activo = true },
                    new Distrito { Codigo = "10705", Descripcion = "Picagres", CantonId = c107, Activo = true },
                    new Distrito { Codigo = "10706", Descripcion = "Jaris", CantonId = c107, Activo = true },
                    new Distrito { Codigo = "10707", Descripcion = "Quitirrisí", CantonId = c107, Activo = true }
                });
            }

            // 108 - Goicoechea
            if (cantones.TryGetValue("108", out var c108))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "10801", Descripcion = "Guadalupe", CantonId = c108, Activo = true },
                    new Distrito { Codigo = "10802", Descripcion = "San Francisco", CantonId = c108, Activo = true },
                    new Distrito { Codigo = "10803", Descripcion = "Calle Blancos", CantonId = c108, Activo = true },
                    new Distrito { Codigo = "10804", Descripcion = "Mata de Plátano", CantonId = c108, Activo = true },
                    new Distrito { Codigo = "10805", Descripcion = "Ipís", CantonId = c108, Activo = true },
                    new Distrito { Codigo = "10806", Descripcion = "Rancho Redondo", CantonId = c108, Activo = true },
                    new Distrito { Codigo = "10807", Descripcion = "Purral", CantonId = c108, Activo = true }
                });
            }

            // 109 - Santa Ana
            if (cantones.TryGetValue("109", out var c109))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "10901", Descripcion = "Santa Ana", CantonId = c109, Activo = true },
                    new Distrito { Codigo = "10902", Descripcion = "Salitral", CantonId = c109, Activo = true },
                    new Distrito { Codigo = "10903", Descripcion = "Pozos", CantonId = c109, Activo = true },
                    new Distrito { Codigo = "10904", Descripcion = "Uruca", CantonId = c109, Activo = true },
                    new Distrito { Codigo = "10905", Descripcion = "Piedades", CantonId = c109, Activo = true },
                    new Distrito { Codigo = "10906", Descripcion = "Brasil", CantonId = c109, Activo = true }
                });
            }

            // 110 - Alajuelita
            if (cantones.TryGetValue("110", out var c110))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11001", Descripcion = "Alajuelita", CantonId = c110, Activo = true },
                    new Distrito { Codigo = "11002", Descripcion = "San Josecito", CantonId = c110, Activo = true },
                    new Distrito { Codigo = "11003", Descripcion = "San Antonio", CantonId = c110, Activo = true },
                    new Distrito { Codigo = "11004", Descripcion = "Concepción", CantonId = c110, Activo = true },
                    new Distrito { Codigo = "11005", Descripcion = "San Felipe", CantonId = c110, Activo = true }
                });
            }

            // 111 - Vázquez de Coronado
            if (cantones.TryGetValue("111", out var c111))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11101", Descripcion = "San Isidro", CantonId = c111, Activo = true },
                    new Distrito { Codigo = "11102", Descripcion = "San Rafael", CantonId = c111, Activo = true },
                    new Distrito { Codigo = "11103", Descripcion = "Dulce Nombre de Jesús", CantonId = c111, Activo = true },
                    new Distrito { Codigo = "11104", Descripcion = "Patalillo", CantonId = c111, Activo = true },
                    new Distrito { Codigo = "11105", Descripcion = "Cascajal", CantonId = c111, Activo = true }
                });
            }

            // 112 - Acosta
            if (cantones.TryGetValue("112", out var c112))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11201", Descripcion = "San Ignacio", CantonId = c112, Activo = true },
                    new Distrito { Codigo = "11202", Descripcion = "Guaitil", CantonId = c112, Activo = true },
                    new Distrito { Codigo = "11203", Descripcion = "Palmichal", CantonId = c112, Activo = true },
                    new Distrito { Codigo = "11204", Descripcion = "Cangrejal", CantonId = c112, Activo = true },
                    new Distrito { Codigo = "11205", Descripcion = "Sabanillas", CantonId = c112, Activo = true }
                });
            }

            // 113 - Tibás
            if (cantones.TryGetValue("113", out var c113))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11301", Descripcion = "San Juan", CantonId = c113, Activo = true },
                    new Distrito { Codigo = "11302", Descripcion = "Cinco Esquinas", CantonId = c113, Activo = true },
                    new Distrito { Codigo = "11303", Descripcion = "Anselmo Llorente", CantonId = c113, Activo = true },
                    new Distrito { Codigo = "11304", Descripcion = "León XIII", CantonId = c113, Activo = true },
                    new Distrito { Codigo = "11305", Descripcion = "Colima", CantonId = c113, Activo = true }
                });
            }

            // 114 - Moravia
            if (cantones.TryGetValue("114", out var c114))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11401", Descripcion = "San Vicente", CantonId = c114, Activo = true },
                    new Distrito { Codigo = "11402", Descripcion = "San Jerónimo", CantonId = c114, Activo = true },
                    new Distrito { Codigo = "11403", Descripcion = "La Trinidad", CantonId = c114, Activo = true }
                });
            }

            // 115 - Montes de Oca
            if (cantones.TryGetValue("115", out var c115))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11501", Descripcion = "San Pedro", CantonId = c115, Activo = true },
                    new Distrito { Codigo = "11502", Descripcion = "Sabanilla", CantonId = c115, Activo = true },
                    new Distrito { Codigo = "11503", Descripcion = "Mercedes", CantonId = c115, Activo = true },
                    new Distrito { Codigo = "11504", Descripcion = "San Rafael", CantonId = c115, Activo = true }
                });
            }

            // 116 - Turrubares
            if (cantones.TryGetValue("116", out var c116))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11601", Descripcion = "San Pablo", CantonId = c116, Activo = true },
                    new Distrito { Codigo = "11602", Descripcion = "San Pedro", CantonId = c116, Activo = true },
                    new Distrito { Codigo = "11603", Descripcion = "San Juan de Mata", CantonId = c116, Activo = true },
                    new Distrito { Codigo = "11604", Descripcion = "San Luis", CantonId = c116, Activo = true },
                    new Distrito { Codigo = "11605", Descripcion = "Carara", CantonId = c116, Activo = true }
                });
            }

            // 117 - Dota
            if (cantones.TryGetValue("117", out var c117))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11701", Descripcion = "Santa María", CantonId = c117, Activo = true },
                    new Distrito { Codigo = "11702", Descripcion = "Jardín", CantonId = c117, Activo = true },
                    new Distrito { Codigo = "11703", Descripcion = "Copey", CantonId = c117, Activo = true }
                });
            }

            // 118 - Curridabat
            if (cantones.TryGetValue("118", out var c118))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11801", Descripcion = "Curridabat", CantonId = c118, Activo = true },
                    new Distrito { Codigo = "11802", Descripcion = "Granadilla", CantonId = c118, Activo = true },
                    new Distrito { Codigo = "11803", Descripcion = "Sánchez", CantonId = c118, Activo = true },
                    new Distrito { Codigo = "11804", Descripcion = "Tirrases", CantonId = c118, Activo = true }
                });
            }

            // 119 - Pérez Zeledón
            if (cantones.TryGetValue("119", out var c119))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "11901", Descripcion = "San Isidro de El General", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11902", Descripcion = "El General", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11903", Descripcion = "Daniel Flores", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11904", Descripcion = "Rivas", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11905", Descripcion = "San Pedro", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11906", Descripcion = "Platanares", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11907", Descripcion = "Pejibaye", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11908", Descripcion = "Cajón", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11909", Descripcion = "Barú", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11910", Descripcion = "Río Nuevo", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11911", Descripcion = "Páramo", CantonId = c119, Activo = true },
                    new Distrito { Codigo = "11912", Descripcion = "La Amistad", CantonId = c119, Activo = true }
                });
            }

            // 120 - León Cortés Castro
            if (cantones.TryGetValue("120", out var c120))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "12001", Descripcion = "San Pablo", CantonId = c120, Activo = true },
                    new Distrito { Codigo = "12002", Descripcion = "San Andrés", CantonId = c120, Activo = true },
                    new Distrito { Codigo = "12003", Descripcion = "Llano Bonito", CantonId = c120, Activo = true },
                    new Distrito { Codigo = "12004", Descripcion = "San Isidro", CantonId = c120, Activo = true },
                    new Distrito { Codigo = "12005", Descripcion = "Santa Cruz", CantonId = c120, Activo = true },
                    new Distrito { Codigo = "12006", Descripcion = "San Antonio", CantonId = c120, Activo = true }
                });
            }

            // =============================================
            // PROVINCIA 2: ALAJUELA (16 cantones)
            // =============================================

            // 201 - Alajuela
            if (cantones.TryGetValue("201", out var c201))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "20101", Descripcion = "Alajuela", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20102", Descripcion = "San José", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20103", Descripcion = "Carrizal", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20104", Descripcion = "San Antonio", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20105", Descripcion = "Guácima", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20106", Descripcion = "San Isidro", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20107", Descripcion = "Sabanilla", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20108", Descripcion = "San Rafael", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20109", Descripcion = "Río Segundo", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20110", Descripcion = "Desamparados", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20111", Descripcion = "Turrúcares", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20112", Descripcion = "Tambor", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20113", Descripcion = "Garita", CantonId = c201, Activo = true },
                    new Distrito { Codigo = "20114", Descripcion = "Sarapiquí", CantonId = c201, Activo = true }
                });
            }

            // 202 - San Ramón
            if (cantones.TryGetValue("202", out var c202))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "20201", Descripcion = "San Ramón", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20202", Descripcion = "Santiago", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20203", Descripcion = "San Juan", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20204", Descripcion = "Piedades Norte", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20205", Descripcion = "Piedades Sur", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20206", Descripcion = "San Rafael", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20207", Descripcion = "San Isidro", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20208", Descripcion = "Ángeles", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20209", Descripcion = "Alfaro", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20210", Descripcion = "Volio", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20211", Descripcion = "Concepción", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20212", Descripcion = "Zapotal", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20213", Descripcion = "Peñas Blancas", CantonId = c202, Activo = true },
                    new Distrito { Codigo = "20214", Descripcion = "San Lorenzo", CantonId = c202, Activo = true }
                });
            }

            // 203 - Grecia
            if (cantones.TryGetValue("203", out var c203))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "20301", Descripcion = "Grecia", CantonId = c203, Activo = true },
                    new Distrito { Codigo = "20302", Descripcion = "San Isidro", CantonId = c203, Activo = true },
                    new Distrito { Codigo = "20303", Descripcion = "San José", CantonId = c203, Activo = true },
                    new Distrito { Codigo = "20304", Descripcion = "San Roque", CantonId = c203, Activo = true },
                    new Distrito { Codigo = "20305", Descripcion = "Tacares", CantonId = c203, Activo = true },
                    new Distrito { Codigo = "20306", Descripcion = "Puente de Piedra", CantonId = c203, Activo = true },
                    new Distrito { Codigo = "20307", Descripcion = "Bolívar", CantonId = c203, Activo = true }
                });
            }

            // 204 - San Mateo
            if (cantones.TryGetValue("204", out var c204))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "20401", Descripcion = "San Mateo", CantonId = c204, Activo = true },
                    new Distrito { Codigo = "20402", Descripcion = "Desmonte", CantonId = c204, Activo = true },
                    new Distrito { Codigo = "20403", Descripcion = "Jesús María", CantonId = c204, Activo = true },
                    new Distrito { Codigo = "20404", Descripcion = "Labrador", CantonId = c204, Activo = true }
                });
            }

            // 205 - Atenas
            if (cantones.TryGetValue("205", out var c205))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "20501", Descripcion = "Atenas", CantonId = c205, Activo = true },
                    new Distrito { Codigo = "20502", Descripcion = "Jesús", CantonId = c205, Activo = true },
                    new Distrito { Codigo = "20503", Descripcion = "Mercedes", CantonId = c205, Activo = true },
                    new Distrito { Codigo = "20504", Descripcion = "San Isidro", CantonId = c205, Activo = true },
                    new Distrito { Codigo = "20505", Descripcion = "Concepción", CantonId = c205, Activo = true },
                    new Distrito { Codigo = "20506", Descripcion = "San José", CantonId = c205, Activo = true },
                    new Distrito { Codigo = "20507", Descripcion = "Santa Eulalia", CantonId = c205, Activo = true },
                    new Distrito { Codigo = "20508", Descripcion = "Escobal", CantonId = c205, Activo = true }
                });
            }

            // 206 - Naranjo
            if (cantones.TryGetValue("206", out var c206))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "20601", Descripcion = "Naranjo", CantonId = c206, Activo = true },
                    new Distrito { Codigo = "20602", Descripcion = "San Miguel", CantonId = c206, Activo = true },
                    new Distrito { Codigo = "20603", Descripcion = "San José", CantonId = c206, Activo = true },
                    new Distrito { Codigo = "20604", Descripcion = "Cirrí Sur", CantonId = c206, Activo = true },
                    new Distrito { Codigo = "20605", Descripcion = "San Jerónimo", CantonId = c206, Activo = true },
                    new Distrito { Codigo = "20606", Descripcion = "San Juan", CantonId = c206, Activo = true },
                    new Distrito { Codigo = "20607", Descripcion = "El Rosario", CantonId = c206, Activo = true },
                    new Distrito { Codigo = "20608", Descripcion = "Palmitos", CantonId = c206, Activo = true }
                });
            }

            // 207 - Palmares
            if (cantones.TryGetValue("207", out var c207))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "20701", Descripcion = "Palmares", CantonId = c207, Activo = true },
                    new Distrito { Codigo = "20702", Descripcion = "Zaragoza", CantonId = c207, Activo = true },
                    new Distrito { Codigo = "20703", Descripcion = "Buenos Aires", CantonId = c207, Activo = true },
                    new Distrito { Codigo = "20704", Descripcion = "Santiago", CantonId = c207, Activo = true },
                    new Distrito { Codigo = "20705", Descripcion = "Candelaria", CantonId = c207, Activo = true },
                    new Distrito { Codigo = "20706", Descripcion = "Esquipulas", CantonId = c207, Activo = true },
                    new Distrito { Codigo = "20707", Descripcion = "La Granja", CantonId = c207, Activo = true }
                });
            }

            // 208 - Poás
            if (cantones.TryGetValue("208", out var c208))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "20801", Descripcion = "San Pedro", CantonId = c208, Activo = true },
                    new Distrito { Codigo = "20802", Descripcion = "San Juan", CantonId = c208, Activo = true },
                    new Distrito { Codigo = "20803", Descripcion = "San Rafael", CantonId = c208, Activo = true },
                    new Distrito { Codigo = "20804", Descripcion = "Carrillos", CantonId = c208, Activo = true },
                    new Distrito { Codigo = "20805", Descripcion = "Sabana Redonda", CantonId = c208, Activo = true }
                });
            }

            // 209 - Orotina
            if (cantones.TryGetValue("209", out var c209))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "20901", Descripcion = "Orotina", CantonId = c209, Activo = true },
                    new Distrito { Codigo = "20902", Descripcion = "El Mastate", CantonId = c209, Activo = true },
                    new Distrito { Codigo = "20903", Descripcion = "Hacienda Vieja", CantonId = c209, Activo = true },
                    new Distrito { Codigo = "20904", Descripcion = "Coyolar", CantonId = c209, Activo = true },
                    new Distrito { Codigo = "20905", Descripcion = "La Ceiba", CantonId = c209, Activo = true }
                });
            }

            // 210 - San Carlos
            if (cantones.TryGetValue("210", out var c210))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "21001", Descripcion = "Quesada", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21002", Descripcion = "Florencia", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21003", Descripcion = "Buenavista", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21004", Descripcion = "Aguas Zarcas", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21005", Descripcion = "Venecia", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21006", Descripcion = "Pital", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21007", Descripcion = "La Fortuna", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21008", Descripcion = "La Tigra", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21009", Descripcion = "La Palmera", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21010", Descripcion = "Venado", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21011", Descripcion = "Cutris", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21012", Descripcion = "Monterrey", CantonId = c210, Activo = true },
                    new Distrito { Codigo = "21013", Descripcion = "Pocosol", CantonId = c210, Activo = true }
                });
            }

            // 211 - Zarcero
            if (cantones.TryGetValue("211", out var c211))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "21101", Descripcion = "Zarcero", CantonId = c211, Activo = true },
                    new Distrito { Codigo = "21102", Descripcion = "Laguna", CantonId = c211, Activo = true },
                    new Distrito { Codigo = "21103", Descripcion = "Tapesco", CantonId = c211, Activo = true },
                    new Distrito { Codigo = "21104", Descripcion = "Guadalupe", CantonId = c211, Activo = true },
                    new Distrito { Codigo = "21105", Descripcion = "Palmira", CantonId = c211, Activo = true },
                    new Distrito { Codigo = "21106", Descripcion = "Zapote", CantonId = c211, Activo = true },
                    new Distrito { Codigo = "21107", Descripcion = "Brisas", CantonId = c211, Activo = true }
                });
            }

            // 212 - Valverde Vega (Sarchí)
            if (cantones.TryGetValue("212", out var c212))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "21201", Descripcion = "Sarchí Norte", CantonId = c212, Activo = true },
                    new Distrito { Codigo = "21202", Descripcion = "Sarchí Sur", CantonId = c212, Activo = true },
                    new Distrito { Codigo = "21203", Descripcion = "Toro Amarillo", CantonId = c212, Activo = true },
                    new Distrito { Codigo = "21204", Descripcion = "San Pedro", CantonId = c212, Activo = true },
                    new Distrito { Codigo = "21205", Descripcion = "Rodríguez", CantonId = c212, Activo = true }
                });
            }

            // 213 - Upala
            if (cantones.TryGetValue("213", out var c213))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "21301", Descripcion = "Upala", CantonId = c213, Activo = true },
                    new Distrito { Codigo = "21302", Descripcion = "Aguas Claras", CantonId = c213, Activo = true },
                    new Distrito { Codigo = "21303", Descripcion = "San José", CantonId = c213, Activo = true },
                    new Distrito { Codigo = "21304", Descripcion = "Bijagua", CantonId = c213, Activo = true },
                    new Distrito { Codigo = "21305", Descripcion = "Delicias", CantonId = c213, Activo = true },
                    new Distrito { Codigo = "21306", Descripcion = "Dos Ríos", CantonId = c213, Activo = true },
                    new Distrito { Codigo = "21307", Descripcion = "Yolillal", CantonId = c213, Activo = true },
                    new Distrito { Codigo = "21308", Descripcion = "Canalete", CantonId = c213, Activo = true }
                });
            }

            // 214 - Los Chiles
            if (cantones.TryGetValue("214", out var c214))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "21401", Descripcion = "Los Chiles", CantonId = c214, Activo = true },
                    new Distrito { Codigo = "21402", Descripcion = "Caño Negro", CantonId = c214, Activo = true },
                    new Distrito { Codigo = "21403", Descripcion = "El Amparo", CantonId = c214, Activo = true },
                    new Distrito { Codigo = "21404", Descripcion = "San Jorge", CantonId = c214, Activo = true }
                });
            }

            // 215 - Guatuso
            if (cantones.TryGetValue("215", out var c215))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "21501", Descripcion = "San Rafael", CantonId = c215, Activo = true },
                    new Distrito { Codigo = "21502", Descripcion = "Buenavista", CantonId = c215, Activo = true },
                    new Distrito { Codigo = "21503", Descripcion = "Cote", CantonId = c215, Activo = true },
                    new Distrito { Codigo = "21504", Descripcion = "Katira", CantonId = c215, Activo = true }
                });
            }

            // 216 - Río Cuarto
            if (cantones.TryGetValue("216", out var c216))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "21601", Descripcion = "Río Cuarto", CantonId = c216, Activo = true },
                    new Distrito { Codigo = "21602", Descripcion = "Santa Rita", CantonId = c216, Activo = true },
                    new Distrito { Codigo = "21603", Descripcion = "Santa Isabel", CantonId = c216, Activo = true }
                });
            }

            // =============================================
            // PROVINCIA 3: CARTAGO (8 cantones)
            // =============================================

            // 301 - Cartago
            if (cantones.TryGetValue("301", out var c301))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "30101", Descripcion = "Oriental", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30102", Descripcion = "Occidental", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30103", Descripcion = "Carmen", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30104", Descripcion = "San Nicolás", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30105", Descripcion = "Aguacaliente", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30106", Descripcion = "Guadalupe", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30107", Descripcion = "Corralillo", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30108", Descripcion = "Tierra Blanca", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30109", Descripcion = "Dulce Nombre", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30110", Descripcion = "Llano Grande", CantonId = c301, Activo = true },
                    new Distrito { Codigo = "30111", Descripcion = "Quebradilla", CantonId = c301, Activo = true }
                });
            }

            // 302 - Paraíso
            if (cantones.TryGetValue("302", out var c302))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "30201", Descripcion = "Paraíso", CantonId = c302, Activo = true },
                    new Distrito { Codigo = "30202", Descripcion = "Santiago", CantonId = c302, Activo = true },
                    new Distrito { Codigo = "30203", Descripcion = "Orosi", CantonId = c302, Activo = true },
                    new Distrito { Codigo = "30204", Descripcion = "Cachí", CantonId = c302, Activo = true },
                    new Distrito { Codigo = "30205", Descripcion = "Llanos de Santa Lucía", CantonId = c302, Activo = true },
                    new Distrito { Codigo = "30206", Descripcion = "Birrisito", CantonId = c302, Activo = true }
                });
            }

            // 303 - La Unión
            if (cantones.TryGetValue("303", out var c303))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "30301", Descripcion = "Tres Ríos", CantonId = c303, Activo = true },
                    new Distrito { Codigo = "30302", Descripcion = "San Diego", CantonId = c303, Activo = true },
                    new Distrito { Codigo = "30303", Descripcion = "San Juan", CantonId = c303, Activo = true },
                    new Distrito { Codigo = "30304", Descripcion = "San Rafael", CantonId = c303, Activo = true },
                    new Distrito { Codigo = "30305", Descripcion = "Concepción", CantonId = c303, Activo = true },
                    new Distrito { Codigo = "30306", Descripcion = "Dulce Nombre", CantonId = c303, Activo = true },
                    new Distrito { Codigo = "30307", Descripcion = "San Ramón", CantonId = c303, Activo = true },
                    new Distrito { Codigo = "30308", Descripcion = "Río Azul", CantonId = c303, Activo = true }
                });
            }

            // 304 - Jiménez
            if (cantones.TryGetValue("304", out var c304))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "30401", Descripcion = "Juan Viñas", CantonId = c304, Activo = true },
                    new Distrito { Codigo = "30402", Descripcion = "Tucurrique", CantonId = c304, Activo = true },
                    new Distrito { Codigo = "30403", Descripcion = "Pejibaye", CantonId = c304, Activo = true }
                });
            }

            // 305 - Turrialba
            if (cantones.TryGetValue("305", out var c305))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "30501", Descripcion = "Turrialba", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30502", Descripcion = "La Suiza", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30503", Descripcion = "Peralta", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30504", Descripcion = "Santa Cruz", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30505", Descripcion = "Santa Teresita", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30506", Descripcion = "Pavones", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30507", Descripcion = "Tuis", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30508", Descripcion = "Tayutic", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30509", Descripcion = "Santa Rosa", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30510", Descripcion = "Tres Equis", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30511", Descripcion = "La Isabel", CantonId = c305, Activo = true },
                    new Distrito { Codigo = "30512", Descripcion = "Chirripó", CantonId = c305, Activo = true }
                });
            }

            // 306 - Alvarado
            if (cantones.TryGetValue("306", out var c306))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "30601", Descripcion = "Pacayas", CantonId = c306, Activo = true },
                    new Distrito { Codigo = "30602", Descripcion = "Cervantes", CantonId = c306, Activo = true },
                    new Distrito { Codigo = "30603", Descripcion = "Capellades", CantonId = c306, Activo = true }
                });
            }

            // 307 - Oreamuno
            if (cantones.TryGetValue("307", out var c307))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "30701", Descripcion = "San Rafael", CantonId = c307, Activo = true },
                    new Distrito { Codigo = "30702", Descripcion = "Cot", CantonId = c307, Activo = true },
                    new Distrito { Codigo = "30703", Descripcion = "Potrero Cerrado", CantonId = c307, Activo = true },
                    new Distrito { Codigo = "30704", Descripcion = "Cipreses", CantonId = c307, Activo = true },
                    new Distrito { Codigo = "30705", Descripcion = "Santa Rosa", CantonId = c307, Activo = true }
                });
            }

            // 308 - El Guarco
            if (cantones.TryGetValue("308", out var c308))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "30801", Descripcion = "El Tejar", CantonId = c308, Activo = true },
                    new Distrito { Codigo = "30802", Descripcion = "San Isidro", CantonId = c308, Activo = true },
                    new Distrito { Codigo = "30803", Descripcion = "Tobosi", CantonId = c308, Activo = true },
                    new Distrito { Codigo = "30804", Descripcion = "Patio de Agua", CantonId = c308, Activo = true }
                });
            }

            // =============================================
            // PROVINCIA 4: HEREDIA (10 cantones)
            // =============================================

            // 401 - Heredia
            if (cantones.TryGetValue("401", out var c401))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "40101", Descripcion = "Heredia", CantonId = c401, Activo = true },
                    new Distrito { Codigo = "40102", Descripcion = "Mercedes", CantonId = c401, Activo = true },
                    new Distrito { Codigo = "40103", Descripcion = "San Francisco", CantonId = c401, Activo = true },
                    new Distrito { Codigo = "40104", Descripcion = "Ulloa", CantonId = c401, Activo = true },
                    new Distrito { Codigo = "40105", Descripcion = "Varablanca", CantonId = c401, Activo = true }
                });
            }

            // 402 - Barva
            if (cantones.TryGetValue("402", out var c402))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "40201", Descripcion = "Barva", CantonId = c402, Activo = true },
                    new Distrito { Codigo = "40202", Descripcion = "San Pedro", CantonId = c402, Activo = true },
                    new Distrito { Codigo = "40203", Descripcion = "San Pablo", CantonId = c402, Activo = true },
                    new Distrito { Codigo = "40204", Descripcion = "San Roque", CantonId = c402, Activo = true },
                    new Distrito { Codigo = "40205", Descripcion = "Santa Lucía", CantonId = c402, Activo = true },
                    new Distrito { Codigo = "40206", Descripcion = "San José de la Montaña", CantonId = c402, Activo = true }
                });
            }

            // 403 - Santo Domingo
            if (cantones.TryGetValue("403", out var c403))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "40301", Descripcion = "Santo Domingo", CantonId = c403, Activo = true },
                    new Distrito { Codigo = "40302", Descripcion = "San Vicente", CantonId = c403, Activo = true },
                    new Distrito { Codigo = "40303", Descripcion = "San Miguel", CantonId = c403, Activo = true },
                    new Distrito { Codigo = "40304", Descripcion = "Paracito", CantonId = c403, Activo = true },
                    new Distrito { Codigo = "40305", Descripcion = "Santo Tomás", CantonId = c403, Activo = true },
                    new Distrito { Codigo = "40306", Descripcion = "Santa Rosa", CantonId = c403, Activo = true },
                    new Distrito { Codigo = "40307", Descripcion = "Tures", CantonId = c403, Activo = true },
                    new Distrito { Codigo = "40308", Descripcion = "Pará", CantonId = c403, Activo = true }
                });
            }

            // 404 - Santa Bárbara
            if (cantones.TryGetValue("404", out var c404))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "40401", Descripcion = "Santa Bárbara", CantonId = c404, Activo = true },
                    new Distrito { Codigo = "40402", Descripcion = "San Pedro", CantonId = c404, Activo = true },
                    new Distrito { Codigo = "40403", Descripcion = "San Juan", CantonId = c404, Activo = true },
                    new Distrito { Codigo = "40404", Descripcion = "Jesús", CantonId = c404, Activo = true },
                    new Distrito { Codigo = "40405", Descripcion = "Santo Domingo", CantonId = c404, Activo = true },
                    new Distrito { Codigo = "40406", Descripcion = "Purabá", CantonId = c404, Activo = true }
                });
            }

            // 405 - San Rafael
            if (cantones.TryGetValue("405", out var c405))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "40501", Descripcion = "San Rafael", CantonId = c405, Activo = true },
                    new Distrito { Codigo = "40502", Descripcion = "San Josecito", CantonId = c405, Activo = true },
                    new Distrito { Codigo = "40503", Descripcion = "Santiago", CantonId = c405, Activo = true },
                    new Distrito { Codigo = "40504", Descripcion = "Ángeles", CantonId = c405, Activo = true },
                    new Distrito { Codigo = "40505", Descripcion = "Concepción", CantonId = c405, Activo = true }
                });
            }

            // 406 - San Isidro
            if (cantones.TryGetValue("406", out var c406))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "40601", Descripcion = "San Isidro", CantonId = c406, Activo = true },
                    new Distrito { Codigo = "40602", Descripcion = "San José", CantonId = c406, Activo = true },
                    new Distrito { Codigo = "40603", Descripcion = "Concepción", CantonId = c406, Activo = true },
                    new Distrito { Codigo = "40604", Descripcion = "San Francisco", CantonId = c406, Activo = true }
                });
            }

            // 407 - Belén
            if (cantones.TryGetValue("407", out var c407))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "40701", Descripcion = "San Antonio", CantonId = c407, Activo = true },
                    new Distrito { Codigo = "40702", Descripcion = "La Ribera", CantonId = c407, Activo = true },
                    new Distrito { Codigo = "40703", Descripcion = "La Asunción", CantonId = c407, Activo = true }
                });
            }

            // 408 - Flores
            if (cantones.TryGetValue("408", out var c408))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "40801", Descripcion = "San Joaquín", CantonId = c408, Activo = true },
                    new Distrito { Codigo = "40802", Descripcion = "Barrantes", CantonId = c408, Activo = true },
                    new Distrito { Codigo = "40803", Descripcion = "Llorente", CantonId = c408, Activo = true }
                });
            }

            // 409 - San Pablo
            if (cantones.TryGetValue("409", out var c409))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "40901", Descripcion = "San Pablo", CantonId = c409, Activo = true },
                    new Distrito { Codigo = "40902", Descripcion = "Rincón de Sabanilla", CantonId = c409, Activo = true }
                });
            }

            // 410 - Sarapiquí
            if (cantones.TryGetValue("410", out var c410))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "41001", Descripcion = "Puerto Viejo", CantonId = c410, Activo = true },
                    new Distrito { Codigo = "41002", Descripcion = "La Virgen", CantonId = c410, Activo = true },
                    new Distrito { Codigo = "41003", Descripcion = "Las Horquetas", CantonId = c410, Activo = true },
                    new Distrito { Codigo = "41004", Descripcion = "Llanuras del Gaspar", CantonId = c410, Activo = true },
                    new Distrito { Codigo = "41005", Descripcion = "Cureña", CantonId = c410, Activo = true }
                });
            }

            // =============================================
            // PROVINCIA 5: GUANACASTE (11 cantones)
            // =============================================

            // 501 - Liberia
            if (cantones.TryGetValue("501", out var c501))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "50101", Descripcion = "Liberia", CantonId = c501, Activo = true },
                    new Distrito { Codigo = "50102", Descripcion = "Cañas Dulces", CantonId = c501, Activo = true },
                    new Distrito { Codigo = "50103", Descripcion = "Mayorga", CantonId = c501, Activo = true },
                    new Distrito { Codigo = "50104", Descripcion = "Nacascolo", CantonId = c501, Activo = true },
                    new Distrito { Codigo = "50105", Descripcion = "Curubandé", CantonId = c501, Activo = true }
                });
            }

            // 502 - Nicoya
            if (cantones.TryGetValue("502", out var c502))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "50201", Descripcion = "Nicoya", CantonId = c502, Activo = true },
                    new Distrito { Codigo = "50202", Descripcion = "Mansión", CantonId = c502, Activo = true },
                    new Distrito { Codigo = "50203", Descripcion = "San Antonio", CantonId = c502, Activo = true },
                    new Distrito { Codigo = "50204", Descripcion = "Quebrada Honda", CantonId = c502, Activo = true },
                    new Distrito { Codigo = "50205", Descripcion = "Sámara", CantonId = c502, Activo = true },
                    new Distrito { Codigo = "50206", Descripcion = "Nosara", CantonId = c502, Activo = true },
                    new Distrito { Codigo = "50207", Descripcion = "Belén de Nosarita", CantonId = c502, Activo = true }
                });
            }

            // 503 - Santa Cruz
            if (cantones.TryGetValue("503", out var c503))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "50301", Descripcion = "Santa Cruz", CantonId = c503, Activo = true },
                    new Distrito { Codigo = "50302", Descripcion = "Bolsón", CantonId = c503, Activo = true },
                    new Distrito { Codigo = "50303", Descripcion = "Veintisiete de Abril", CantonId = c503, Activo = true },
                    new Distrito { Codigo = "50304", Descripcion = "Tempate", CantonId = c503, Activo = true },
                    new Distrito { Codigo = "50305", Descripcion = "Cartagena", CantonId = c503, Activo = true },
                    new Distrito { Codigo = "50306", Descripcion = "Cuajiniquil", CantonId = c503, Activo = true },
                    new Distrito { Codigo = "50307", Descripcion = "Diriá", CantonId = c503, Activo = true },
                    new Distrito { Codigo = "50308", Descripcion = "Cabo Velas", CantonId = c503, Activo = true },
                    new Distrito { Codigo = "50309", Descripcion = "Tamarindo", CantonId = c503, Activo = true }
                });
            }

            // 504 - Bagaces
            if (cantones.TryGetValue("504", out var c504))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "50401", Descripcion = "Bagaces", CantonId = c504, Activo = true },
                    new Distrito { Codigo = "50402", Descripcion = "La Fortuna", CantonId = c504, Activo = true },
                    new Distrito { Codigo = "50403", Descripcion = "Mogote", CantonId = c504, Activo = true },
                    new Distrito { Codigo = "50404", Descripcion = "Río Naranjo", CantonId = c504, Activo = true }
                });
            }

            // 505 - Carrillo
            if (cantones.TryGetValue("505", out var c505))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "50501", Descripcion = "Filadelfia", CantonId = c505, Activo = true },
                    new Distrito { Codigo = "50502", Descripcion = "Palmira", CantonId = c505, Activo = true },
                    new Distrito { Codigo = "50503", Descripcion = "Sardinal", CantonId = c505, Activo = true },
                    new Distrito { Codigo = "50504", Descripcion = "Belén", CantonId = c505, Activo = true }
                });
            }

            // 506 - Cañas
            if (cantones.TryGetValue("506", out var c506))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "50601", Descripcion = "Cañas", CantonId = c506, Activo = true },
                    new Distrito { Codigo = "50602", Descripcion = "Palmira", CantonId = c506, Activo = true },
                    new Distrito { Codigo = "50603", Descripcion = "San Miguel", CantonId = c506, Activo = true },
                    new Distrito { Codigo = "50604", Descripcion = "Bebedero", CantonId = c506, Activo = true },
                    new Distrito { Codigo = "50605", Descripcion = "Porozal", CantonId = c506, Activo = true }
                });
            }

            // 507 - Abangares
            if (cantones.TryGetValue("507", out var c507))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "50701", Descripcion = "Las Juntas", CantonId = c507, Activo = true },
                    new Distrito { Codigo = "50702", Descripcion = "Sierra", CantonId = c507, Activo = true },
                    new Distrito { Codigo = "50703", Descripcion = "San Juan", CantonId = c507, Activo = true },
                    new Distrito { Codigo = "50704", Descripcion = "Colorado", CantonId = c507, Activo = true }
                });
            }

            // 508 - Tilarán
            if (cantones.TryGetValue("508", out var c508))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "50801", Descripcion = "Tilarán", CantonId = c508, Activo = true },
                    new Distrito { Codigo = "50802", Descripcion = "Quebrada Grande", CantonId = c508, Activo = true },
                    new Distrito { Codigo = "50803", Descripcion = "Tronadora", CantonId = c508, Activo = true },
                    new Distrito { Codigo = "50804", Descripcion = "Santa Rosa", CantonId = c508, Activo = true },
                    new Distrito { Codigo = "50805", Descripcion = "Líbano", CantonId = c508, Activo = true },
                    new Distrito { Codigo = "50806", Descripcion = "Tierras Morenas", CantonId = c508, Activo = true },
                    new Distrito { Codigo = "50807", Descripcion = "Arenal", CantonId = c508, Activo = true }
                });
            }

            // 509 - Nandayure
            if (cantones.TryGetValue("509", out var c509))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "50901", Descripcion = "Carmona", CantonId = c509, Activo = true },
                    new Distrito { Codigo = "50902", Descripcion = "Santa Rita", CantonId = c509, Activo = true },
                    new Distrito { Codigo = "50903", Descripcion = "Zapotal", CantonId = c509, Activo = true },
                    new Distrito { Codigo = "50904", Descripcion = "San Pablo", CantonId = c509, Activo = true },
                    new Distrito { Codigo = "50905", Descripcion = "Porvenir", CantonId = c509, Activo = true },
                    new Distrito { Codigo = "50906", Descripcion = "Bejuco", CantonId = c509, Activo = true }
                });
            }

            // 510 - La Cruz
            if (cantones.TryGetValue("510", out var c510))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "51001", Descripcion = "La Cruz", CantonId = c510, Activo = true },
                    new Distrito { Codigo = "51002", Descripcion = "Santa Cecilia", CantonId = c510, Activo = true },
                    new Distrito { Codigo = "51003", Descripcion = "La Garita", CantonId = c510, Activo = true },
                    new Distrito { Codigo = "51004", Descripcion = "Santa Elena", CantonId = c510, Activo = true }
                });
            }

            // 511 - Hojancha
            if (cantones.TryGetValue("511", out var c511))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "51101", Descripcion = "Hojancha", CantonId = c511, Activo = true },
                    new Distrito { Codigo = "51102", Descripcion = "Monte Romo", CantonId = c511, Activo = true },
                    new Distrito { Codigo = "51103", Descripcion = "Puerto Carrillo", CantonId = c511, Activo = true },
                    new Distrito { Codigo = "51104", Descripcion = "Huacas", CantonId = c511, Activo = true }
                });
            }

            // =============================================
            // PROVINCIA 6: PUNTARENAS (13 cantones)
            // =============================================

            // 601 - Puntarenas
            if (cantones.TryGetValue("601", out var c601))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "60101", Descripcion = "Puntarenas", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60102", Descripcion = "Pitahaya", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60103", Descripcion = "Chomes", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60104", Descripcion = "Lepanto", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60105", Descripcion = "Paquera", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60106", Descripcion = "Manzanillo", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60107", Descripcion = "Guacimal", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60108", Descripcion = "Barranca", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60109", Descripcion = "Isla del Coco", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60110", Descripcion = "Cóbano", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60111", Descripcion = "Chacarita", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60112", Descripcion = "Chira", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60113", Descripcion = "Acapulco", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60114", Descripcion = "El Roble", CantonId = c601, Activo = true },
                    new Distrito { Codigo = "60115", Descripcion = "Arancibia", CantonId = c601, Activo = true }
                });
            }

            // 602 - Esparza
            if (cantones.TryGetValue("602", out var c602))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "60201", Descripcion = "Espíritu Santo", CantonId = c602, Activo = true },
                    new Distrito { Codigo = "60202", Descripcion = "San Juan Grande", CantonId = c602, Activo = true },
                    new Distrito { Codigo = "60203", Descripcion = "Macacona", CantonId = c602, Activo = true },
                    new Distrito { Codigo = "60204", Descripcion = "San Rafael", CantonId = c602, Activo = true },
                    new Distrito { Codigo = "60205", Descripcion = "San Jerónimo", CantonId = c602, Activo = true },
                    new Distrito { Codigo = "60206", Descripcion = "Caldera", CantonId = c602, Activo = true }
                });
            }

            // 603 - Buenos Aires
            if (cantones.TryGetValue("603", out var c603))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "60301", Descripcion = "Buenos Aires", CantonId = c603, Activo = true },
                    new Distrito { Codigo = "60302", Descripcion = "Volcán", CantonId = c603, Activo = true },
                    new Distrito { Codigo = "60303", Descripcion = "Potrero Grande", CantonId = c603, Activo = true },
                    new Distrito { Codigo = "60304", Descripcion = "Boruca", CantonId = c603, Activo = true },
                    new Distrito { Codigo = "60305", Descripcion = "Pilas", CantonId = c603, Activo = true },
                    new Distrito { Codigo = "60306", Descripcion = "Colinas", CantonId = c603, Activo = true },
                    new Distrito { Codigo = "60307", Descripcion = "Chánguena", CantonId = c603, Activo = true },
                    new Distrito { Codigo = "60308", Descripcion = "Biolley", CantonId = c603, Activo = true },
                    new Distrito { Codigo = "60309", Descripcion = "Brunka", CantonId = c603, Activo = true }
                });
            }

            // 604 - Montes de Oro
            if (cantones.TryGetValue("604", out var c604))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "60401", Descripcion = "Miramar", CantonId = c604, Activo = true },
                    new Distrito { Codigo = "60402", Descripcion = "La Unión", CantonId = c604, Activo = true },
                    new Distrito { Codigo = "60403", Descripcion = "San Isidro", CantonId = c604, Activo = true }
                });
            }

            // 605 - Osa
            if (cantones.TryGetValue("605", out var c605))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "60501", Descripcion = "Puerto Cortés", CantonId = c605, Activo = true },
                    new Distrito { Codigo = "60502", Descripcion = "Palmar", CantonId = c605, Activo = true },
                    new Distrito { Codigo = "60503", Descripcion = "Sierpe", CantonId = c605, Activo = true },
                    new Distrito { Codigo = "60504", Descripcion = "Bahía Ballena", CantonId = c605, Activo = true },
                    new Distrito { Codigo = "60505", Descripcion = "Piedras Blancas", CantonId = c605, Activo = true },
                    new Distrito { Codigo = "60506", Descripcion = "Bahía Drake", CantonId = c605, Activo = true }
                });
            }

            // 606 - Quepos
            if (cantones.TryGetValue("606", out var c606))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "60601", Descripcion = "Quepos", CantonId = c606, Activo = true },
                    new Distrito { Codigo = "60602", Descripcion = "Savegre", CantonId = c606, Activo = true },
                    new Distrito { Codigo = "60603", Descripcion = "Naranjito", CantonId = c606, Activo = true }
                });
            }

            // 607 - Golfito
            if (cantones.TryGetValue("607", out var c607))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "60701", Descripcion = "Golfito", CantonId = c607, Activo = true },
                    new Distrito { Codigo = "60702", Descripcion = "Guaycará", CantonId = c607, Activo = true },
                    new Distrito { Codigo = "60703", Descripcion = "Pavón", CantonId = c607, Activo = true }
                });
            }

            // 608 - Coto Brus
            if (cantones.TryGetValue("608", out var c608))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "60801", Descripcion = "San Vito", CantonId = c608, Activo = true },
                    new Distrito { Codigo = "60802", Descripcion = "Sabalito", CantonId = c608, Activo = true },
                    new Distrito { Codigo = "60803", Descripcion = "Aguabuena", CantonId = c608, Activo = true },
                    new Distrito { Codigo = "60804", Descripcion = "Limoncito", CantonId = c608, Activo = true },
                    new Distrito { Codigo = "60805", Descripcion = "Pittier", CantonId = c608, Activo = true },
                    new Distrito { Codigo = "60806", Descripcion = "Gutiérrez Braun", CantonId = c608, Activo = true }
                });
            }

            // 609 - Parrita
            if (cantones.TryGetValue("609", out var c609))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "60901", Descripcion = "Parrita", CantonId = c609, Activo = true }
                });
            }

            // 610 - Corredores
            if (cantones.TryGetValue("610", out var c610))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "61001", Descripcion = "Corredor", CantonId = c610, Activo = true },
                    new Distrito { Codigo = "61002", Descripcion = "La Cuesta", CantonId = c610, Activo = true },
                    new Distrito { Codigo = "61003", Descripcion = "Canoas", CantonId = c610, Activo = true },
                    new Distrito { Codigo = "61004", Descripcion = "Laurel", CantonId = c610, Activo = true }
                });
            }

            // 611 - Garabito
            if (cantones.TryGetValue("611", out var c611))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "61101", Descripcion = "Jacó", CantonId = c611, Activo = true },
                    new Distrito { Codigo = "61102", Descripcion = "Tárcoles", CantonId = c611, Activo = true },
                    new Distrito { Codigo = "61103", Descripcion = "Lagunillas", CantonId = c611, Activo = true }
                });
            }

            // 612 - Monteverde
            if (cantones.TryGetValue("612", out var c612))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "61201", Descripcion = "Monteverde", CantonId = c612, Activo = true }
                });
            }

            // 613 - Puerto Jiménez
            if (cantones.TryGetValue("613", out var c613))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "61301", Descripcion = "Puerto Jiménez", CantonId = c613, Activo = true }
                });
            }

            // =============================================
            // PROVINCIA 7: LIMÓN (6 cantones)
            // =============================================

            // 701 - Limón
            if (cantones.TryGetValue("701", out var c701))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "70101", Descripcion = "Limón", CantonId = c701, Activo = true },
                    new Distrito { Codigo = "70102", Descripcion = "Valle La Estrella", CantonId = c701, Activo = true },
                    new Distrito { Codigo = "70103", Descripcion = "Río Blanco", CantonId = c701, Activo = true },
                    new Distrito { Codigo = "70104", Descripcion = "Matama", CantonId = c701, Activo = true }
                });
            }

            // 702 - Pococí
            if (cantones.TryGetValue("702", out var c702))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "70201", Descripcion = "Guápiles", CantonId = c702, Activo = true },
                    new Distrito { Codigo = "70202", Descripcion = "Jiménez", CantonId = c702, Activo = true },
                    new Distrito { Codigo = "70203", Descripcion = "La Rita", CantonId = c702, Activo = true },
                    new Distrito { Codigo = "70204", Descripcion = "Roxana", CantonId = c702, Activo = true },
                    new Distrito { Codigo = "70205", Descripcion = "Cariari", CantonId = c702, Activo = true },
                    new Distrito { Codigo = "70206", Descripcion = "Colorado", CantonId = c702, Activo = true },
                    new Distrito { Codigo = "70207", Descripcion = "La Colonia", CantonId = c702, Activo = true }
                });
            }

            // 703 - Siquirres
            if (cantones.TryGetValue("703", out var c703))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "70301", Descripcion = "Siquirres", CantonId = c703, Activo = true },
                    new Distrito { Codigo = "70302", Descripcion = "Pacuarito", CantonId = c703, Activo = true },
                    new Distrito { Codigo = "70303", Descripcion = "Florida", CantonId = c703, Activo = true },
                    new Distrito { Codigo = "70304", Descripcion = "Germania", CantonId = c703, Activo = true },
                    new Distrito { Codigo = "70305", Descripcion = "El Cairo", CantonId = c703, Activo = true },
                    new Distrito { Codigo = "70306", Descripcion = "Alegría", CantonId = c703, Activo = true },
                    new Distrito { Codigo = "70307", Descripcion = "Reventazón", CantonId = c703, Activo = true }
                });
            }

            // 704 - Talamanca
            if (cantones.TryGetValue("704", out var c704))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "70401", Descripcion = "Bratsi", CantonId = c704, Activo = true },
                    new Distrito { Codigo = "70402", Descripcion = "Sixaola", CantonId = c704, Activo = true },
                    new Distrito { Codigo = "70403", Descripcion = "Cahuita", CantonId = c704, Activo = true },
                    new Distrito { Codigo = "70404", Descripcion = "Telire", CantonId = c704, Activo = true }
                });
            }

            // 705 - Matina
            if (cantones.TryGetValue("705", out var c705))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "70501", Descripcion = "Matina", CantonId = c705, Activo = true },
                    new Distrito { Codigo = "70502", Descripcion = "Batán", CantonId = c705, Activo = true },
                    new Distrito { Codigo = "70503", Descripcion = "Carrandi", CantonId = c705, Activo = true }
                });
            }

            // 706 - Guácimo
            if (cantones.TryGetValue("706", out var c706))
            {
                distritos.AddRange(new[]
                {
                    new Distrito { Codigo = "70601", Descripcion = "Guácimo", CantonId = c706, Activo = true },
                    new Distrito { Codigo = "70602", Descripcion = "Mercedes", CantonId = c706, Activo = true },
                    new Distrito { Codigo = "70603", Descripcion = "Pocora", CantonId = c706, Activo = true },
                    new Distrito { Codigo = "70604", Descripcion = "Río Jiménez", CantonId = c706, Activo = true },
                    new Distrito { Codigo = "70605", Descripcion = "Duacarí", CantonId = c706, Activo = true }
                });
            }

            _context.Distritos.AddRange(distritos);
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckTiposCodigoAsync()
    {
        if (!_context.TiposCodigo.Any())
        {
            var tiposCodigo = new List<TipoCodigo>
            {
                new() { Codigo = "01", Descripcion = "Código del producto del vendedor", Activo = true },
                new() { Codigo = "02", Descripcion = "Código del producto del comprador", Activo = true },
                new() { Codigo = "03", Descripcion = "Código del producto asignado por la industria", Activo = true },
                new() { Codigo = "04", Descripcion = "Código uso interno", Activo = true },
                new() { Codigo = "05", Descripcion = "Otros", Activo = true },
                new() { Codigo = "06", Descripcion = "UPC (Universal Product Code)", Activo = true },
                new() { Codigo = "07", Descripcion = "EAN-8 (European Article Number)", Activo = true },
                new() { Codigo = "08", Descripcion = "EAN-13 (European Article Number)", Activo = true },
                new() { Codigo = "09", Descripcion = "EAN-14 (European Article Number)", Activo = true },
                new() { Codigo = "10", Descripcion = "EAN-128 (European Article Number)", Activo = true },
                new() { Codigo = "11", Descripcion = "DUN-14 (Distribution Unit Number)", Activo = true },
                new() { Codigo = "12", Descripcion = "ISBN (International Standard Book Number)", Activo = true },
                new() { Codigo = "13", Descripcion = "GTIN (Global Trade Item Number)", Activo = true }
            };

            _context.TiposCodigo.AddRange(tiposCodigo);
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckTiposDocumentoAsync()
    {
        // Catálogo oficial Hacienda v4.4
        var tiposDocumentoOficiales = new List<(string Codigo, string Descripcion, string Abreviatura)>
        {
            ("01", "Factura Electrónica", "FE"),
            ("02", "Nota de Débito Electrónica", "ND"),
            ("03", "Nota de Crédito Electrónica", "NC"),
            ("04", "Tiquete Electrónico", "TE"),
            ("05", "Confirmación de aceptación del comprobante", "MR"),
            ("06", "Confirmación de aceptación parcial del comprobante", "MRP"),
            ("07", "Confirmación de rechazo del comprobante", "MRR"),
            ("08", "Factura Electrónica de Compra", "FEC"),
            ("09", "Factura Electrónica de Exportación", "FEE"),
            ("10", "Recibo Electrónico de Pago", "REP")  // NUEVO v4.4
        };

        if (!_context.TiposDocumento.Any())
        {
            var tiposDocumento = tiposDocumentoOficiales.Select(t => new TipoDocumento
            {
                Codigo = t.Codigo,
                Descripcion = t.Descripcion,
                Abreviatura = t.Abreviatura,
                Activo = true
            }).ToList();

            _context.TiposDocumento.AddRange(tiposDocumento);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar tipos de documento faltantes (como el REP)
            var existentes = await _context.TiposDocumento.Select(t => t.Codigo).ToListAsync();
            var nuevos = tiposDocumentoOficiales
                .Where(t => !existentes.Contains(t.Codigo))
                .Select(t => new TipoDocumento
                {
                    Codigo = t.Codigo,
                    Descripcion = t.Descripcion,
                    Abreviatura = t.Abreviatura,
                    Activo = true
                }).ToList();

            if (nuevos.Any())
            {
                _context.TiposDocumento.AddRange(nuevos);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task CheckUnidadesMedidaAsync()
    {
        // Catálogo oficial Hacienda v4.4 - Unidades de Medida
        var unidadesOficiales = new List<(string Codigo, string Descripcion)>
        {
            ("Al", "Alquiler"),
            ("Alc", "Alcance cuartilla (resma)"),
            ("Cm", "Centímetro"),
            ("Cj", "Caja"),
            ("Cn", "Caneca"),
            ("Ct", "Cartucho"),
            ("d", "Día"),
            ("dm", "Decímetro"),
            ("Gal", "Galón"),
            ("g", "Gramo"),
            ("h", "Hora"),
            ("Kg", "Kilogramo"),
            ("Km", "Kilómetro"),
            ("L", "Litro"),
            ("m", "Metro"),
            ("m²", "Metro cuadrado"),
            ("m³", "Metro cúbico"),
            ("min", "Minuto"),
            ("mL", "Mililitro"),
            ("mm", "Milímetro"),
            ("Mn", "Mensualidad"),
            ("Oz", "Onza"),
            ("Otro", "Otros (por especificar)"),
            ("Paq", "Paquete"),
            ("Pl", "Pliego"),
            ("Qd", "Quintal métrico"),
            ("Rac", "Ración"),
            ("s", "Segundo"),
            ("Sb", "Sobre"),
            ("Sp", "Servicios Profesionales"),
            ("St", "Set"),
            ("Tam", "Tanda"),
            ("Tm", "Tonelada"),
            ("Unid", "Unidad"),
            ("Yd", "Yarda")
        };

        if (!_context.UnidadesMedida.Any())
        {
            var unidadesMedida = unidadesOficiales.Select(u => new UnidadMedida
            {
                Codigo = u.Codigo,
                Descripcion = u.Descripcion,
                Activo = true
            }).ToList();

            _context.UnidadesMedida.AddRange(unidadesMedida);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar unidades faltantes
            var existentes = await _context.UnidadesMedida.Select(u => u.Codigo.ToLower()).ToListAsync();
            var nuevas = unidadesOficiales
                .Where(u => !existentes.Contains(u.Codigo.ToLower()))
                .Select(u => new UnidadMedida
                {
                    Codigo = u.Codigo,
                    Descripcion = u.Descripcion,
                    Activo = true
                }).ToList();

            if (nuevas.Any())
            {
                _context.UnidadesMedida.AddRange(nuevas);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task CheckImpuestosAsync()
    {
        // Diccionario con los porcentajes correctos por código
        var porcentajesPorCodigo = new Dictionary<string, decimal>
        {
            { "01", 13.00m }, // IVA
            { "02", 0.00m },  // Selectivo de Consumo (variable según producto)
            { "03", 0.00m },  // Único a Combustibles (variable)
            { "04", 0.00m },  // Bebidas Alcohólicas (variable)
            { "05", 0.00m },  // Bebidas sin alcohol (variable)
            { "06", 0.00m },  // Tabaco (variable)
            { "07", 13.00m }, // IVA cálculo especial
            { "08", 13.00m }, // IVA Bienes Usados
            { "12", 0.00m },  // Cemento asfáltico (variable)
            { "99", 0.00m }   // Otros
        };

        if (!_context.Impuestos.Any())
        {
            var impuestos = new List<Impuesto>
            {
                new() { Codigo = "01", Descripcion = "Impuesto al Valor Agregado", Porcentaje = 13.00m, Activo = true },
                new() { Codigo = "02", Descripcion = "Impuesto Selectivo de Consumo", Porcentaje = 0.00m, Activo = true },
                new() { Codigo = "03", Descripcion = "Impuesto Único a los Combustibles", Porcentaje = 0.00m, Activo = true },
                new() { Codigo = "04", Descripcion = "Impuesto específico de Bebidas Alcohólicas", Porcentaje = 0.00m, Activo = true },
                new() { Codigo = "05", Descripcion = "Impuesto Específico sobre las bebidas envasadas sin contenido alcohólico y jabones de tocador", Porcentaje = 0.00m, Activo = true },
                new() { Codigo = "06", Descripcion = "Impuesto a los Productos de Tabaco", Porcentaje = 0.00m, Activo = true },
                new() { Codigo = "07", Descripcion = "IVA (cálculo especial)", Porcentaje = 13.00m, Activo = true },
                new() { Codigo = "08", Descripcion = "IVA Régimen de Bienes Usados (Factor)", Porcentaje = 13.00m, Activo = true },
                new() { Codigo = "12", Descripcion = "Impuesto específico al cemento asfáltico", Porcentaje = 0.00m, Activo = true },
                new() { Codigo = "99", Descripcion = "Otros", Porcentaje = 0.00m, Activo = true }
            };

            _context.Impuestos.AddRange(impuestos);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Actualizar porcentajes si están en 0 para IVA (códigos 01, 07, 08)
            var impuestosConIVA = await _context.Impuestos
                .Where(i => (i.Codigo == "01" || i.Codigo == "07" || i.Codigo == "08") && i.Porcentaje == 0)
                .ToListAsync();

            foreach (var impuesto in impuestosConIVA)
            {
                if (porcentajesPorCodigo.TryGetValue(impuesto.Codigo, out var porcentaje))
                {
                    impuesto.Porcentaje = porcentaje;
                }
            }

            if (impuestosConIVA.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task CheckCodigosExoneracionAsync()
    {
        // Catálogo oficial Hacienda v4.4 - Códigos de Exoneración
        var codigosOficiales = new List<(string Codigo, string Descripcion)>
        {
            ("01", "Compras autorizadas"),
            ("02", "Ventas exentas a diplomáticos"),
            ("03", "Autorizado por Ley especial"),
            ("04", "Exenciones de la DGT"),
            ("05", "Zonas Francas"),
            ("06", "Régimen de Perfeccionamiento Activo"),
            ("07", "Régimen de Perfeccionamiento Pasivo"),
            ("08", "Bienes de Capital"),
            ("99", "Otros")
        };

        if (!_context.CodigosExoneracion.Any())
        {
            var codigosExoneracion = codigosOficiales.Select(c => new CodigoExoneracion
            {
                Codigo = c.Codigo,
                Descripcion = c.Descripcion,
                Activo = true
            }).ToList();

            _context.CodigosExoneracion.AddRange(codigosExoneracion);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar códigos faltantes
            var existentes = await _context.CodigosExoneracion.Select(c => c.Codigo).ToListAsync();
            var nuevos = codigosOficiales
                .Where(c => !existentes.Contains(c.Codigo))
                .Select(c => new CodigoExoneracion
                {
                    Codigo = c.Codigo,
                    Descripcion = c.Descripcion,
                    Activo = true
                }).ToList();

            if (nuevos.Any())
            {
                _context.CodigosExoneracion.AddRange(nuevos);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task CheckCondicionesVentaAsync()
    {
        // Catálogo oficial Hacienda v4.4
        var condicionesOficiales = new List<(string Codigo, string Descripcion)>
        {
            ("01", "Contado"),
            ("02", "Crédito"),
            ("03", "Consignación"),
            ("04", "Apartado"),
            ("05", "Arrendamiento con opción de compra"),
            ("06", "Arrendamiento en función financiera"),
            ("07", "Cobro a favor de terceros"),
            ("08", "Intercambio / Permuta"),            // NUEVO v4.4
            ("09", "Donación"),                         // NUEVO v4.4
            ("10", "Otros"),                            // NUEVO v4.4
            ("12", "Venta de mercancía no nacionalizada"),    // NUEVO v4.4
            ("13", "Venta de bienes usados a no contribuyente"), // NUEVO v4.4
            ("14", "Arrendamiento operativo"),          // NUEVO v4.4
            ("15", "Arrendamiento financiero")          // NUEVO v4.4
        };

        if (!_context.CondicionesVenta.Any())
        {
            var condicionesVenta = condicionesOficiales.Select(c => new CondicionVenta
            {
                Codigo = c.Codigo,
                Descripcion = c.Descripcion,
                Activo = true
            }).ToList();

            _context.CondicionesVenta.AddRange(condicionesVenta);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar condiciones faltantes (nuevas en v4.4)
            var existentes = await _context.CondicionesVenta.Select(c => c.Codigo).ToListAsync();
            var nuevas = condicionesOficiales
                .Where(c => !existentes.Contains(c.Codigo))
                .Select(c => new CondicionVenta
                {
                    Codigo = c.Codigo,
                    Descripcion = c.Descripcion,
                    Activo = true
                }).ToList();

            if (nuevas.Any())
            {
                _context.CondicionesVenta.AddRange(nuevas);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task CheckMediosPagoAsync()
    {
        if (!_context.MediosPago.Any())
        {
            // Catálogo oficial Hacienda v4.4
            var mediosPago = new List<MedioPago>
            {
                new() { Codigo = "01", Descripcion = "Efectivo", Activo = true },
                new() { Codigo = "02", Descripcion = "Tarjeta", Activo = true },
                new() { Codigo = "03", Descripcion = "Cheque", Activo = true },
                new() { Codigo = "04", Descripcion = "Transferencia - depósito bancario", Activo = true },
                new() { Codigo = "05", Descripcion = "Recaudado por terceros", Activo = true },
                new() { Codigo = "06", Descripcion = "SINPE Móvil", Activo = true },              // NUEVO v4.4
                new() { Codigo = "07", Descripcion = "Plataforma digital", Activo = true },       // NUEVO v4.4 (PayPal, etc.)
                new() { Codigo = "99", Descripcion = "Otros", Activo = true }
            };

            _context.MediosPago.AddRange(mediosPago);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar nuevos medios de pago v4.4 si no existen
            var existentes = await _context.MediosPago.Select(m => m.Codigo).ToListAsync();
            var nuevos = new List<MedioPago>();

            if (!existentes.Contains("06"))
            {
                nuevos.Add(new MedioPago { Codigo = "06", Descripcion = "SINPE Móvil", Activo = true });
            }
            if (!existentes.Contains("07"))
            {
                nuevos.Add(new MedioPago { Codigo = "07", Descripcion = "Plataforma digital", Activo = true });
            }

            if (nuevos.Any())
            {
                _context.MediosPago.AddRange(nuevos);
                await _context.SaveChangesAsync();
            }
        }
    }

    // ===========================================
    // CATEGORÍAS DE EJEMPLO PARA EMPRESAS
    // ===========================================

    private async Task CheckCategoriasEjemploAsync()
    {
        // Obtener todas las empresas que no tienen categorías
        var empresasSinCategorias = await _context.Empresas
            .Where(e => !e.IsDeleted && !_context.Categorias.Any(c => c.EmpresaId == e.Id))
            .ToListAsync();

        if (!empresasSinCategorias.Any())
        {
            return;
        }

        var categoriasEjemplo = new List<string>
        {
            "Productos Generales",
            "Servicios Profesionales",
            "Alimentos y Bebidas",
            "Electrónica",
            "Ropa y Accesorios",
            "Hogar y Jardín",
            "Salud y Belleza",
            "Suministros de Oficina",
            "Transporte y Logística",
            "Otros"
        };

        foreach (var empresa in empresasSinCategorias)
        {
            var categorias = categoriasEjemplo.Select(nombre => new Categoria
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresa.Id,
                Nombre = nombre,
                Descripcion = $"Categoría de {nombre.ToLower()}",
                Activo = true,
                FechaCreacion = FechaCostaRicaHelper.Ahora
            }).ToList();

            _context.Categorias.AddRange(categorias);
        }

        await _context.SaveChangesAsync();
    }

    // ===========================================
    // CATÁLOGOS HACIENDA V4.4 (NUEVOS)
    // ===========================================

    private async Task CheckTarifasIVAAsync()
    {
        // Catálogo oficial Hacienda v4.4 - Tarifas de IVA (CodigoTarifa)
        var tarifasOficiales = new List<(string Codigo, string Descripcion, decimal Porcentaje)>
        {
            ("01", "Tarifa 0% (Exento)", 0.00m),
            ("02", "Tarifa reducida 1%", 1.00m),
            ("03", "Tarifa reducida 2%", 2.00m),
            ("04", "Tarifa reducida 4%", 4.00m),
            ("05", "Transitorio 0%", 0.00m),
            ("06", "Transitorio 4%", 4.00m),
            ("07", "Transitorio 8%", 8.00m),
            ("08", "Tarifa general 13%", 13.00m),
            ("11", "Tarifa 0% sin derecho a crédito", 0.00m)  // NUEVO v4.4
        };

        if (!_context.TarifasIVA.Any())
        {
            var tarifas = tarifasOficiales.Select(t => new TarifaIVA
            {
                Codigo = t.Codigo,
                Descripcion = t.Descripcion,
                Porcentaje = t.Porcentaje,
                Activo = true
            }).ToList();

            _context.TarifasIVA.AddRange(tarifas);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar tarifas faltantes
            var existentes = await _context.TarifasIVA.Select(t => t.Codigo).ToListAsync();
            var nuevas = tarifasOficiales
                .Where(t => !existentes.Contains(t.Codigo))
                .Select(t => new TarifaIVA
                {
                    Codigo = t.Codigo,
                    Descripcion = t.Descripcion,
                    Porcentaje = t.Porcentaje,
                    Activo = true
                }).ToList();

            if (nuevas.Any())
            {
                _context.TarifasIVA.AddRange(nuevas);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task CheckTiposDescuentoHaciendaAsync()
    {
        // Catálogo oficial Hacienda v4.4 - Tipos de Descuento
        var tiposOficiales = new List<(string Codigo, string Descripcion)>
        {
            ("01", "Descuento por Regalía"),
            ("02", "Descuento por Regalía (IVA a cargo del cliente)"),
            ("03", "Descuento por Bonificación"),
            ("04", "Descuento por Volumen"),
            ("05", "Descuento Estacional"),
            ("06", "Descuento Promocional"),
            ("07", "Descuento Comercial"),
            ("08", "Descuento por Frecuencia"),
            ("09", "Descuento Sostenido"),
            ("99", "Otros Descuentos")
        };

        if (!_context.TiposDescuentoHacienda.Any())
        {
            var tipos = tiposOficiales.Select(t => new TipoDescuentoHacienda
            {
                Codigo = t.Codigo,
                Descripcion = t.Descripcion,
                Activo = true
            }).ToList();

            _context.TiposDescuentoHacienda.AddRange(tipos);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar tipos faltantes
            var existentes = await _context.TiposDescuentoHacienda.Select(t => t.Codigo).ToListAsync();
            var nuevos = tiposOficiales
                .Where(t => !existentes.Contains(t.Codigo))
                .Select(t => new TipoDescuentoHacienda
                {
                    Codigo = t.Codigo,
                    Descripcion = t.Descripcion,
                    Activo = true
                }).ToList();

            if (nuevos.Any())
            {
                _context.TiposDescuentoHacienda.AddRange(nuevos);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task CheckTiposDocumentoReferenciaAsync()
    {
        // Catálogo oficial Hacienda v4.4 - Tipos de Documento de Referencia
        var tiposOficiales = new List<(string Codigo, string Descripcion)>
        {
            ("01", "Factura electrónica"),
            ("02", "Nota de débito electrónica"),
            ("03", "Nota de crédito electrónica"),
            ("04", "Tiquete electrónico"),
            ("05", "Nota de despacho"),
            ("06", "Contrato"),
            ("07", "Procedimiento"),
            ("08", "Comprobante emitido en contingencia"),
            ("09", "Devolución de mercadería"),
            ("10", "Sustituye factura rechazada por Hacienda"),
            ("11", "Sustituye factura rechazada por receptor"),
            ("12", "Sustituye factura de exportación"),
            ("13", "Facturación mes vencido"),
            ("99", "Otros")
        };

        if (!_context.TiposDocumentoReferencia.Any())
        {
            var tipos = tiposOficiales.Select(t => new TipoDocumentoReferencia
            {
                Codigo = t.Codigo,
                Descripcion = t.Descripcion,
                Activo = true
            }).ToList();

            _context.TiposDocumentoReferencia.AddRange(tipos);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar tipos faltantes
            var existentes = await _context.TiposDocumentoReferencia.Select(t => t.Codigo).ToListAsync();
            var nuevos = tiposOficiales
                .Where(t => !existentes.Contains(t.Codigo))
                .Select(t => new TipoDocumentoReferencia
                {
                    Codigo = t.Codigo,
                    Descripcion = t.Descripcion,
                    Activo = true
                }).ToList();

            if (nuevos.Any())
            {
                _context.TiposDocumentoReferencia.AddRange(nuevos);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task CheckCodigosReferenciaAsync()
    {
        // Catálogo oficial Hacienda v4.4 - Códigos de Referencia (Razón de referencia)
        var codigosOficiales = new List<(string Codigo, string Descripcion)>
        {
            ("01", "Anula documento de referencia"),
            ("02", "Corrige texto documento de referencia"),
            ("03", "Corrige monto"),
            ("04", "Referencia a otro documento"),
            ("05", "Sustituye comprobante provisional por contingencia"),
            ("99", "Otros")
        };

        if (!_context.CodigosReferencia.Any())
        {
            var codigos = codigosOficiales.Select(c => new CodigoReferencia
            {
                Codigo = c.Codigo,
                Descripcion = c.Descripcion,
                Activo = true
            }).ToList();

            _context.CodigosReferencia.AddRange(codigos);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar códigos faltantes
            var existentes = await _context.CodigosReferencia.Select(c => c.Codigo).ToListAsync();
            var nuevos = codigosOficiales
                .Where(c => !existentes.Contains(c.Codigo))
                .Select(c => new CodigoReferencia
                {
                    Codigo = c.Codigo,
                    Descripcion = c.Descripcion,
                    Activo = true
                }).ToList();

            if (nuevos.Any())
            {
                _context.CodigosReferencia.AddRange(nuevos);
                await _context.SaveChangesAsync();
            }
        }
    }

    // ===========================================
    // NUEVO v4.4 - M7: FORMAS FARMACÉUTICAS
    // ===========================================

    private async Task CheckFormasFarmaceuticasAsync()
    {
        // Catálogo oficial Hacienda v4.4 - Formas Farmacéuticas
        // Obligatorio desde 01/12/2024 para productos farmacéuticos
        var formasOficiales = new List<(string Codigo, string Descripcion)>
        {
            ("01", "Tableta"),
            ("02", "Cápsula"),
            ("03", "Jarabe"),
            ("04", "Solución inyectable"),
            ("05", "Crema/Ungüento"),
            ("06", "Suspensión"),
            ("07", "Gotas"),
            ("08", "Parche transdérmico"),
            ("09", "Supositorio"),
            ("10", "Aerosol/Inhalador"),
            ("99", "Otros")
        };

        if (!_context.FormasFarmaceuticas.Any())
        {
            var formas = formasOficiales.Select(f => new FormaFarmaceutica
            {
                Codigo = f.Codigo,
                Descripcion = f.Descripcion,
                Activo = true
            }).ToList();

            _context.FormasFarmaceuticas.AddRange(formas);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Agregar formas faltantes
            var existentes = await _context.FormasFarmaceuticas.Select(f => f.Codigo).ToListAsync();
            var nuevas = formasOficiales
                .Where(f => !existentes.Contains(f.Codigo))
                .Select(f => new FormaFarmaceutica
                {
                    Codigo = f.Codigo,
                    Descripcion = f.Descripcion,
                    Activo = true
                }).ToList();

            if (nuevas.Any())
            {
                _context.FormasFarmaceuticas.AddRange(nuevas);
                await _context.SaveChangesAsync();
            }
        }
    }
}
