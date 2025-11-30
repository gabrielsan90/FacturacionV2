# GUÍA DE CONFIGURACIÓN DE SEGURIDAD

## PROPÓSITO
Este documento define TODA la configuración de seguridad que DEBE implementarse en los proyectos Backend y Frontend.

## 1. JWT AUTHENTICATION (BACKEND)

### appsettings.json (Backend)

```json
{
  "ConnectionStrings": {
    "LocalConnection": "Server=localhost;Database=MJL;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "https://localhost:7030",
    "Audience": "https://localhost:7030",
    "Key": "esta-es-una-clave-secreta-muy-larga-de-al-menos-32-caracteres-para-jwt"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@mjlogistics.com",
    "SenderName": "MJ Logistics",
    "Username": "tu-email@gmail.com",
    "Password": "tu-app-password"
  }
}
```

### REGLAS PARA JWT Key
1. **MÍNIMO** 32 caracteres
2. **NUNCA** compartir en repositorio público
3. **SIEMPRE** usar diferentes keys para Development y Production
4. **RECOMENDADO**: Generar con: `openssl rand -base64 32`

### Program.cs - Configuración JWT (Backend)

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MJL.Backend.Data;
using MJL.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configurar Controllers
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.MaxDepth = 128;
});

// Configurar DbContext
builder.Services.AddDbContext<DataContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

// PATRÓN: Configurar Identity (OBLIGATORIO)
builder.Services.AddIdentity<User, IdentityRole>(x =>
{
    // Requerir email confirmado para login
    x.SignIn.RequireConfirmedEmail = true;
    x.User.RequireUniqueEmail = true;

    // Configuración de contraseñas (ajustar según requisitos)
    x.Password.RequireDigit = false;
    x.Password.RequiredUniqueChars = 0;
    x.Password.RequireLowercase = false;
    x.Password.RequireNonAlphanumeric = false;
    x.Password.RequireUppercase = false;
    x.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// PATRÓN: Configurar JWT Authentication (OBLIGATORIO)
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
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

var app = builder.Build();

// IMPORTANTE: Orden correcto de middleware
app.UseHttpsRedirection();
app.UseAuthentication();  // DEBE ir antes de Authorization
app.UseAuthorization();

app.MapControllers();
app.Run();
```

### AccountsController.cs - Generar JWT

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MJL.Shared.DTO;
using MJL.Shared.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MJL.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountsController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Email o contraseña incorrectos");
            }

            // Verificar si el email está confirmado
            if (!user.EmailConfirmed)
            {
                return BadRequest("Debe confirmar su email antes de iniciar sesión");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return BadRequest("Email o contraseña incorrectos");
            }

            // Generar JWT Token
            var token = await GenerateTokenAsync(user);

            return Ok(new TokenDTO
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(12)
            });
        }

        // PATRÓN: Método para generar JWT (OBLIGATORIO)
        private async Task<string> GenerateTokenAsync(User user)
        {
            // Obtener roles del usuario
            var roles = await _userManager.GetRolesAsync(user);

            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            // Agregar roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Crear clave de seguridad
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Crear token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                UserType = Enums.UserType.User
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "User");

            // Generar token de confirmación de email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Aquí enviar email con el token
            // await _mailHelper.SendConfirmationEmailAsync(user, token);

            return Ok("Usuario registrado exitosamente. Revise su email para confirmar su cuenta.");
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok("Email confirmado exitosamente");
            }

            return BadRequest("Error al confirmar email");
        }
    }
}
```

### Proteger Endpoints con [Authorize]

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MJL.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaqueteController : Controller
    {
        // Endpoint público (sin autorización)
        [HttpGet("tracking/{numeroTracking}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByTrackingAsync(string numeroTracking)
        {
            // ...
        }

        // Endpoint protegido (requiere autenticación)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAsync()
        {
            // ...
        }

        // Endpoint solo para Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostAsync(Paquete model)
        {
            // ...
        }

        // Endpoint para Admin o Employee
        [HttpPut]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> PutAsync(Paquete model)
        {
            // ...
        }
    }
}
```

## 2. COOKIE AUTHENTICATION (FRONTEND)

### appsettings.json (Frontend)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApiBaseUrl": "https://localhost:7030/"
}
```

### Program.cs - Configuración Cookies (Frontend)

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// PATRÓN: Configurar HttpClient (OBLIGATORIO)
builder.Services.AddHttpClient("MJLApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
    client.Timeout = TimeSpan.FromMinutes(5);
});

// PATRÓN: Configurar Cookie Authentication (OBLIGATORIO)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANTE: Orden correcto
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
```

### Login.cshtml.cs - Autenticación Frontend

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MJL.Shared.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MJL.FrontendAdmin.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public LoginDTO Login { get; set; } = new();

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("MJLApi");
                var apiUrl = $"{_configuration["ApiBaseUrl"]}api/accounts/Login";

                var json = JsonSerializer.Serialize(Login);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenDTO>();

                    // PATRÓN: Guardar JWT en cookie HttpOnly (OBLIGATORIO)
                    Response.Cookies.Append("jwtAdmin", tokenResponse!.Token, new CookieOptions
                    {
                        HttpOnly = true,        // No accesible desde JavaScript
                        Secure = true,          // Solo HTTPS
                        SameSite = SameSiteMode.Strict,  // Protección CSRF
                        Expires = tokenResponse.Expiration
                    });

                    // PATRÓN: Leer claims del JWT y crear Cookie Authentication
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(tokenResponse.Token);
                    var claims = jwtToken.Claims.ToList();

                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = tokenResponse.Expiration
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToPage("/Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return Page();
            }
        }
    }
}
```

### Logout.cshtml.cs

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MJL.FrontendAdmin.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            // PATRÓN: Eliminar cookie JWT
            Response.Cookies.Delete("jwtAdmin");

            // PATRÓN: Sign out de Cookie Authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToPage("/Auth/Login");
        }
    }
}
```

### Proteger Páginas con [Authorize]

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MJL.FrontendAdmin.Pages
{
    // Página solo para usuarios autenticados
    [Authorize]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }

    // Página solo para Admin
    [Authorize(Roles = "Admin")]
    public class UsuariosModel : PageModel
    {
        public void OnGet()
        {
        }
    }

    // Página para Admin o Employee
    [Authorize(Roles = "Admin,Employee")]
    public class PaquetesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
```

### Usar JWT en llamadas al Backend

```csharp
public async Task<IActionResult> OnGetDataAsync()
{
    var client = _httpClientFactory.CreateClient("MJLApi");

    // PATRÓN: Obtener JWT desde cookie y agregarlo al header (OBLIGATORIO)
    if (Request.Cookies.TryGetValue("jwtAdmin", out var jwt))
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);
    }

    var response = await client.GetAsync("/api/paquete");

    if (response.IsSuccessStatusCode)
    {
        var data = await response.Content.ReadFromJsonAsync<List<Paquete>>();
        return new JsonResult(new { data });
    }

    return new JsonResult(new { data = new List<Paquete>() });
}
```

## 3. CORS (SI SE NECESITA)

### Program.cs (Backend) - Configurar CORS

```csharp
var builder = WebApplication.CreateBuilder(args);

// PATRÓN: Configurar CORS (solo si Frontend está en dominio diferente)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins(
                "https://localhost:7107",  // Frontend
                "https://localhost:7159",  // FrontendAdmin
                "https://localhost:7032"   // FrontendUser
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// IMPORTANTE: UseCors DEBE ir antes de UseAuthentication
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

## 4. PROTECCIÓN CONTRA ATAQUES

### CSRF Protection (Anti-Forgery Token)

```cshtml
<!-- En formularios Razor Pages -->
<form method="post">
    @* Token anti-forgery automático *@
    <input type="text" name="campo" />
    <button type="submit">Enviar</button>
</form>

<!-- En AJAX calls -->
<script>
    $.ajax({
        url: '?handler=Save',
        type: 'POST',
        data: formData,
        headers: {
            // OBLIGATORIO: Incluir RequestVerificationToken
            "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            // ...
        }
    });
</script>
```

### SQL Injection Protection

```csharp
// ✅ BIEN: EF Core automáticamente protege contra SQL Injection
var paquetes = await _context.Paquetes
    .Where(p => p.NumeroTracking == numeroTracking)
    .ToListAsync();

// ✅ BIEN: Usar parámetros en queries raw
var paquetes = await _context.Paquetes
    .FromSqlRaw("SELECT * FROM Paquetes WHERE NumeroTracking = {0}", numeroTracking)
    .ToListAsync();

// ❌ MAL: NUNCA concatenar strings en SQL
var query = $"SELECT * FROM Paquetes WHERE NumeroTracking = '{numeroTracking}'";
```

### XSS Protection

```cshtml
<!-- ✅ BIEN: Razor automáticamente escapa HTML -->
<p>@Model.Descripcion</p>

<!-- ❌ MAL: Raw HTML sin sanitizar -->
<p>@Html.Raw(Model.Descripcion)</p>

<!-- ✅ BIEN: Si necesitas HTML, sanitiza primero -->
@using Ganss.XSS
@{
    var sanitizer = new HtmlSanitizer();
    var safeHtml = sanitizer.Sanitize(Model.Descripcion);
}
<p>@Html.Raw(safeHtml)</p>
```

## 5. VARIABLES DE ENTORNO (PRODUCCIÓN)

### NUNCA hacer commit de:
- Connection strings de producción
- JWT Keys de producción
- Passwords de email
- API Keys de terceros

### USAR en Producción:
```bash
# Variables de entorno en servidor
export ConnectionStrings__LocalConnection="Server=prod;Database=MJL;..."
export Jwt__Key="clave-super-secreta-de-produccion"
export EmailSettings__Password="password-real"
```

### O usar User Secrets en Development:
```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "mi-clave-secreta"
dotnet user-secrets set "ConnectionStrings:LocalConnection" "Server=..."
```

## CHECKLIST DE SEGURIDAD

- [ ] JWT configurado en Backend con clave de 32+ caracteres
- [ ] Cookie Authentication configurado en Frontend
- [ ] JWT guardado en cookie HttpOnly, Secure, SameSite=Strict
- [ ] Endpoints protegidos con [Authorize]
- [ ] Páginas protegidas con [Authorize]
- [ ] JWT incluido en headers de llamadas HTTP al Backend
- [ ] CORS configurado correctamente (si aplica)
- [ ] Anti-Forgery Token en todos los POST
- [ ] Connection strings NO en código fuente
- [ ] Passwords de Identity con requisitos mínimos
- [ ] Email confirmation habilitado
- [ ] HTTPS habilitado en producción
- [ ] User Secrets o Variables de Entorno en producción
