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

// Dependency Injection - Gastos Module
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.IGastoRepository, Facturacion.Backend.Repositories.Implementations.GastoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Repositories.Interfaces.ICategoriaGastoRepository, Facturacion.Backend.Repositories.Implementations.CategoriaGastoRepository>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IGastoService, Facturacion.Backend.Services.Implementations.GastoService>();
builder.Services.AddScoped<Facturacion.Backend.UnitsOfWork.Interfaces.IGastoUnitOfWork, Facturacion.Backend.UnitsOfWork.Implementations.GastoUnitOfWork>();

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

// Dependency Injection - Servicios v4.4 (Validación, Tipo de Cambio, Encriptación, Errores)
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IValidacionDocumentoService, Facturacion.Backend.Services.Implementations.ValidacionDocumentoService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IValidacionCalculosService, Facturacion.Backend.Services.Implementations.ValidacionCalculosService>(); // NUEVO v4.4 - M8
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.ITipoCambioBCCRService, Facturacion.Backend.Services.Implementations.TipoCambioBCCRService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IEncryptionService, Facturacion.Backend.Services.Implementations.EncryptionService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IHaciendaErrorService, Facturacion.Backend.Services.Implementations.HaciendaErrorService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IXsdValidacionService, Facturacion.Backend.Services.Implementations.XsdValidacionService>();

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
