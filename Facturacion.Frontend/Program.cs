using Facturacion.Frontend.Helpers;
using Facturacion.Frontend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Configure Data Protection to persist keys (fixes antiforgery token issues on restart)
try
{
    var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "keys");
    Directory.CreateDirectory(keysFolder);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
        .SetApplicationName("FacturacionFrontend");
}
catch (Exception ex)
{
    // Si no puede crear la carpeta, usar Data Protection en memoria
    Console.WriteLine($"Warning: Could not configure persistent Data Protection: {ex.Message}");
    builder.Services.AddDataProtection()
        .SetApplicationName("FacturacionFrontend");
}

// Add services to the container
builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        // Match backend JSON serialization so JsonResult responses use camelCase (matching JS expectations)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Prevent circular reference crashes when serializing entities with navigation properties
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        // Omit null navigation properties to reduce response size
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        // Match backend enum handling so [FromBody] correctly deserializes enums sent as strings (e.g., "CRC" → TipoMoneda.CRC)
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// HttpClient configuration with SSL certificate bypass for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient().ConfigureHttpClientDefaults(http =>
    {
        http.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
    });
}
else
{
    builder.Services.AddHttpClient();
}

// Named HttpClient for API
builder.Services.AddHttpClient("FacturacionApi", client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
    client.BaseAddress = new Uri(apiBaseUrl!);
    client.Timeout = TimeSpan.FromMinutes(5); // Increased for PDF generation and long operations
});

// HttpContextAccessor for accessing HttpContext in services
builder.Services.AddHttpContextAccessor();

// Dependency Injection - Services
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Authentication with Cookies - according to ESPECIFICACION_SISTEMA.md
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";

        // Session expires after 8 hours (fixed, not sliding)
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;

        // Security settings
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "FacturacionAuth";
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://code.jquery.com https://cdn.jsdelivr.net https://cdn.datatables.net https://cdnjs.cloudflare.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdn.datatables.net https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.gstatic.com; " +
        "connect-src 'self' https://cdn.datatables.net https://api.hacienda.go.cr https://tipodecambio.paginasweb.cr ws: wss:; " +
        "frame-ancestors 'none';";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
