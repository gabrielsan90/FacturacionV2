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
builder.Services.AddRazorPages();

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
