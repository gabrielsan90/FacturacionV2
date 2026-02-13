using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories;
using Facturacion.Backend.UnitsOfWork;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure QuestPDF License (Community License for open source projects)
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Add services to the container
builder.Services.AddMemoryCache(); // Required for TipoCambioBCCRService
builder.Services.AddHttpClient(); // Required for IHttpClientFactory (Hacienda API, TipoCambio BCCR, etc.)

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Facturacion API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database configuration
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

// Identity configuration
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;

    // Password settings - according to ESPECIFICACION_SISTEMA.md
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings - 5 failed attempts, 15 minutes lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// JWT Authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Get frontend URL from configuration
        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "https://localhost:7031";

        policy.WithOrigins(frontendUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for cookie-based authentication
    });
});

// Dependency Injection - Repositories and UnitOfWork
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericUnitOfWork<>), typeof(GenericUnitOfWork<>));

// Dependency Injection - Empresa Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IEmpresaRepository, Facturacion.Backend.Repositories.Implementations.EmpresaRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IEmpresaUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.EmpresaUnitOfWork>();

// Dependency Injection - Cliente Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IClienteRepository, Facturacion.Backend.Repositories.Implementations.ClienteRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IClienteUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.ClienteUnitOfWork>();

// Dependency Injection - Proveedor Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IProveedorRepository, Facturacion.Backend.Repositories.Implementations.ProveedorRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IProveedorUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.ProveedorUnitOfWork>();

// Dependency Injection - Producto Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IProductoRepository, Facturacion.Backend.Repositories.Implementations.ProductoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICategoriaRepository, Facturacion.Backend.Repositories.Implementations.CategoriaRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IProductoUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.ProductoUnitOfWork>();

// Dependency Injection - Sucursal Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ISucursalRepository, Facturacion.Backend.Repositories.Implementations.SucursalRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ITerminalRepository, Facturacion.Backend.Repositories.Implementations.TerminalRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IConsecutivoRepository, Facturacion.Backend.Repositories.Implementations.ConsecutivoRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.ISucursalUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.SucursalUnitOfWork>();

// Dependency Injection - Inventario Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IInventarioRepository, Facturacion.Backend.Repositories.Implementations.InventarioRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IMovimientoInventarioRepository, Facturacion.Backend.Repositories.Implementations.MovimientoInventarioRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IAjusteInventarioRepository, Facturacion.Backend.Repositories.Implementations.AjusteInventarioRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IInventarioUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.InventarioUnitOfWork>();

// Dependency Injection - Documento Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IDocumentoRepository, Facturacion.Backend.Repositories.Implementations.DocumentoRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IDocumentoUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.DocumentoUnitOfWork>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IDocumentoService, Facturacion.Backend.Services.Implementations.DocumentoService>();

// Dependency Injection - Consecutivos Service
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IConsecutivoService, Facturacion.Backend.Services.Implementations.ConsecutivoService>();

// Dependency Injection - Hacienda Services
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IClaveGeneradorService, Facturacion.Backend.Services.Implementations.ClaveGeneradorService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IXmlGeneradorService, Facturacion.Backend.Services.Implementations.XmlGeneradorService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IFirmaDigitalService, Facturacion.Backend.Services.Implementations.FirmaDigitalService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IDocumentoHaciendaService, Facturacion.Backend.Services.Implementations.DocumentoHaciendaService>();

// Dependency Injection - Reception and MR Services (NEW)
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IXmlParserService, Facturacion.Backend.Services.Implementations.XmlParserService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IDocumentoRecepcionService, Facturacion.Backend.Services.Implementations.DocumentoRecepcionService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IMensajeReceptorService, Facturacion.Backend.Services.Implementations.MensajeReceptorService>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IDocumentoReceptorMensajeRepository, Facturacion.Backend.Repositories.Implementations.DocumentoReceptorMensajeRepository>();

// Dependency Injection - REP (Recibo Electrónico de Pago) Module - NUEVO v4.4
builder.Services.AddScoped<Facturacion.Backend.Repositories.IReciboPagoRepository, Facturacion.Backend.Repositories.ReciboPagoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IReciboPagoService, Facturacion.Backend.Services.Implementations.ReciboPagoService>();

// Dependency Injection - Cuentas Por Cobrar Module (CxC)
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICuentaPorCobrarRepository, Facturacion.Backend.Repositories.Implementations.CuentaPorCobrarRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IAbonoCobroRepository, Facturacion.Backend.Repositories.Implementations.AbonoCobroRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICotizacionRepository, Facturacion.Backend.Repositories.Implementations.CotizacionRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.ICuentaPorCobrarUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.CuentaPorCobrarUnitOfWork>();

// Dependency Injection - Cuentas Por Pagar Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICuentaPorPagarRepository, Facturacion.Backend.Repositories.Implementations.CuentaPorPagarRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IAbonoPagoRepository, Facturacion.Backend.Repositories.Implementations.AbonoPagoRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.ICuentaPorPagarUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.CuentaPorPagarUnitOfWork>();

// Dependency Injection - Bancos Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IMovimientoBancarioRepository, Facturacion.Backend.Repositories.Implementations.MovimientoBancarioRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICuentaBancariaRepository, Facturacion.Backend.Repositories.Implementations.CuentaBancariaRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IConciliacionBancariaRepository, Facturacion.Backend.Repositories.Implementations.ConciliacionBancariaRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IExtractoBancarioRepository, Facturacion.Backend.Repositories.Implementations.ExtractoBancarioRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IReglaConciliacionRepository, Facturacion.Backend.Repositories.Implementations.ReglaConciliacionRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IMovimientoBancarioUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.MovimientoBancarioUnitOfWork>();

// Dependency Injection - Gastos Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IGastoRepository, Facturacion.Backend.Repositories.Implementations.GastoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICategoriaGastoRepository, Facturacion.Backend.Repositories.Implementations.CategoriaGastoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IGastoService, Facturacion.Backend.Services.Implementations.GastoService>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IGastoUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.GastoUnitOfWork>();

// Dependency Injection - Activos Fijos Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IActivoFijoRepository, Facturacion.Backend.Repositories.Implementations.ActivoFijoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IDepreciacionActivoRepository, Facturacion.Backend.Repositories.Implementations.DepreciacionActivoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICategoriaActivoRepository, Facturacion.Backend.Repositories.Implementations.CategoriaActivoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ITrasladoActivoRepository, Facturacion.Backend.Repositories.Implementations.TrasladoActivoRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IActivoFijoUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.ActivoFijoUnitOfWork>();

// Dependency Injection - Compras Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IOrdenCompraRepository, Facturacion.Backend.Repositories.Implementations.OrdenCompraRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IRecepcionCompraRepository, Facturacion.Backend.Repositories.Implementations.RecepcionCompraRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IRequisicionRepository, Facturacion.Backend.Repositories.Implementations.RequisicionRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICotizacionProveedorRepository, Facturacion.Backend.Repositories.Implementations.CotizacionProveedorRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IOrdenCompraUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.OrdenCompraUnitOfWork>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IRecepcionCompraUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.RecepcionCompraUnitOfWork>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IRequisicionUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.RequisicionUnitOfWork>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.ICotizacionProveedorUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.CotizacionProveedorUnitOfWork>();

// Dependency Injection - RRHH Module (Recursos Humanos)
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IEmpleadoRepository, Facturacion.Backend.Repositories.Implementations.EmpleadoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IDepartamentoRepository, Facturacion.Backend.Repositories.Implementations.DepartamentoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IPuestoRepository, Facturacion.Backend.Repositories.Implementations.PuestoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IVacacionRepository, Facturacion.Backend.Repositories.Implementations.VacacionRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IIncapacidadRepository, Facturacion.Backend.Repositories.Implementations.IncapacidadRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IPlanillaRepository, Facturacion.Backend.Repositories.Implementations.PlanillaRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IEmpleadoUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.EmpleadoUnitOfWork>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IPlanillaUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.PlanillaUnitOfWork>();

// Dependency Injection - Workflow Module (Aprobaciones)
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ITipoWorkflowRepository, Facturacion.Backend.Repositories.Implementations.TipoWorkflowRepository>();

// Dependency Injection - Dashboard Module
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IDashboardService, Facturacion.Backend.Services.Implementations.DashboardService>();

// Dependency Injection - Reportes Module
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IReportesService, Facturacion.Backend.Services.Implementations.ReportesService>();

// Dependency Injection - Email Module
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IEmailService, Facturacion.Backend.Services.Implementations.EmailService>();

// Dependency Injection - Notificaciones Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.INotificacionRepository, Facturacion.Backend.Repositories.Implementations.NotificacionRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.INotificacionUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.NotificacionUnitOfWork>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.INotificacionService, Facturacion.Backend.Services.Implementations.NotificacionService>();

// Dependency Injection - Auditoría Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IAuditoriaRepository, Facturacion.Backend.Repositories.Implementations.AuditoriaRepository>();

// Dependency Injection - Contabilidad Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IAsientoContableRepository, Facturacion.Backend.Repositories.Implementations.AsientoContableRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IPeriodoContableRepository, Facturacion.Backend.Repositories.Implementations.PeriodoContableRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IPlantillaAsientoRepository, Facturacion.Backend.Repositories.Implementations.PlantillaAsientoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICuentaContableRepository, Facturacion.Backend.Repositories.Implementations.CuentaContableRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICentroCostoRepository, Facturacion.Backend.Repositories.Implementations.CentroCostoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICuentaIntegracionRepository, Facturacion.Backend.Repositories.Implementations.CuentaIntegracionRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IConfiguracionContableRepository, Facturacion.Backend.Repositories.Implementations.ConfiguracionContableRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IContabilidadUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.ContabilidadUnitOfWork>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IContabilidadIntegracionService, Facturacion.Backend.Services.Implementations.ContabilidadIntegracionService>();

// Dependency Injection - Servicios v4.4 (Validación, Tipo de Cambio, Encriptación, Errores)
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IValidacionDocumentoService, Facturacion.Backend.Services.Implementations.ValidacionDocumentoService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IValidacionCalculosService, Facturacion.Backend.Services.Implementations.ValidacionCalculosService>(); // NUEVO v4.4 - M8
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.ITipoCambioBCCRService, Facturacion.Backend.Services.Implementations.TipoCambioBCCRService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IEncryptionService, Facturacion.Backend.Services.Implementations.EncryptionService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IHaciendaErrorService, Facturacion.Backend.Services.Implementations.HaciendaErrorService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IXsdValidacionService, Facturacion.Backend.Services.Implementations.XsdValidacionService>();

// Dependency Injection - PDF Generation Service
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IPdfGeneradorService, Facturacion.Backend.Services.Implementations.PdfGeneradorService>();

// Dependency Injection - Excel Import Service
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IExcelImportService, Facturacion.Backend.Services.Implementations.ExcelImportService>();

// Dependency Injection - Servicios de APIs de Hacienda (CABYS, Actividades Económicas, Exoneraciones)
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.ICabysService, Facturacion.Backend.Services.Implementations.CabysService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IActividadEconomicaService, Facturacion.Backend.Services.Implementations.ActividadEconomicaService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IExoneracionService, Facturacion.Backend.Services.Implementations.ExoneracionService>();

// Dependency Injection - Hacienda Token Module (OAuth2)
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IHaciendaTokenRepository, Facturacion.Backend.Repositories.Implementations.HaciendaTokenRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IHaciendaTokenUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.HaciendaTokenUnitOfWork>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IHaciendaTokenService, Facturacion.Backend.Services.Implementations.HaciendaTokenService>();

// Background Services - Cola de envío a Hacienda
builder.Services.AddHostedService<Facturacion.Backend.Services.BackgroundServices.DocumentoEnvioBackgroundService>();

// Dependency Injection - Catalogos Module (Geographic Divisions)
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IProvinciaRepository, Facturacion.Backend.Repositories.Implementations.ProvinciaRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICantonRepository, Facturacion.Backend.Repositories.Implementations.CantonRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IDistritoRepository, Facturacion.Backend.Repositories.Implementations.DistritoRepository>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.ICatalogoUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.CatalogoUnitOfWork>();

// Hacienda API Service - Usa IHttpClientFactory para flexibilidad
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IHaciendaApiService, Facturacion.Backend.Services.Implementations.HaciendaApiService>();

// Dependency Injection - Helpers
builder.Services.AddScoped<IUserHelper, UserHelper>();

// Seed Database
builder.Services.AddTransient<SeedDb>();

var app = builder.Build();

// Seed data
SeedData(app);

void SeedData(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var seedDb = scope.ServiceProvider.GetRequiredService<SeedDb>();
    seedDb.SeedAsync().Wait();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
