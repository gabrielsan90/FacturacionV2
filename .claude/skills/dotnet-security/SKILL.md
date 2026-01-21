---
name: dotnet-security
description: Configure JWT authentication for Backend API and Cookie authentication for Frontend. Use when setting up login/logout, creating AuthController, implementing TokenService, protecting endpoints with roles, or configuring Identity.
---

# .NET Security Specialist

Implement JWT (Backend) and Cookie (Frontend) authentication with role-based authorization.

## Security Model

| Layer | Authentication | Purpose |
|-------|---------------|---------|
| Backend | JWT Bearer | Stateless REST API |
| Frontend | Cookies | Stateful web session |

## JWT Authentication in Backend (Program.cs)

```csharp
// Identity
builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 8;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireLowercase = true;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    opt.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// JWT
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
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
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
});
```

## TokenService

```csharp
public interface ITokenService
{
    Task<string> GenerateTokenAsync(User user);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;

    public TokenService(IConfiguration configuration, UserManager<User> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("UserId", user.Id)
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

## AuthController (Backend)

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized(new { message = result.IsLockedOut ? "Account locked" : "Invalid credentials" });

        var token = await _tokenService.GenerateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new TokenDto
        {
            Token = token,
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}",
            Roles = roles.ToList(),
            Expiration = DateTime.UtcNow.AddHours(24)
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, "User");
        return Ok(new { message = "User registered successfully" });
    }
}
```

## Cookie Authentication in Frontend (Program.cs)

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Auth/Login";
        opt.LogoutPath = "/Auth/Logout";
        opt.AccessDeniedPath = "/Auth/AccessDenied";
        opt.ExpireTimeSpan = TimeSpan.FromHours(24);
        opt.SlidingExpiration = true;
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opt.Cookie.SameSite = SameSiteMode.Strict;
    });
```

## Login PageModel (Frontend)

```csharp
public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty]
    public LoginDto Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var client = _httpClientFactory.CreateClient("BackendApi");
        var response = await client.PostAsJsonAsync("api/auth/login", Input);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials");
            return Page();
        }

        var tokenDto = await response.Content.ReadFromJsonAsync<TokenDto>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, tokenDto!.FullName),
            new(ClaimTypes.Email, tokenDto.Email),
            new("Token", tokenDto.Token)
        };
        foreach (var role in tokenDto.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = tokenDto.Expiration });

        return RedirectToPage("/Dashboard");
    }
}
```

## HttpClient with JWT Token

```csharp
public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticatedHttpClientHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = _httpContextAccessor.HttpContext?.User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }
}

// Program.cs
builder.Services.AddTransient<AuthenticatedHttpClientHandler>();
builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
}).AddHttpMessageHandler<AuthenticatedHttpClientHandler>();
```

## Role-Based Authorization

```csharp
// Backend Controller
[Authorize]                          // Requires authentication
[Authorize(Roles = "Admin")]         // Admin only
[Authorize(Roles = "Admin,Manager")] // Admin or Manager
[AllowAnonymous]                     // No authentication

// Frontend PageModel
[Authorize]
[Authorize(Roles = "Admin")]
```

```html
<!-- Razor View -->
@if (User.IsInRole("Admin"))
{
    <button class="btn btn-danger">Delete</button>
}
```

## System Roles

| Role | Permissions |
|------|-------------|
| Admin | Full access |
| Manager | Full CRUD, no config |
| Employee | Create, Read, Update |
| User | Read only own data |

## Unbreakable Rules

1. **NEVER** store passwords in plain text
2. **ALWAYS** use HTTPS in production
3. **ALWAYS** validate tokens on each request
4. **NEVER** expose sensitive info in errors
5. **ALWAYS** use claims for authorization
6. **ALWAYS** implement lockout after failed attempts
7. **NEVER** put JWT key in source code
8. **ALWAYS** use User Secrets in development
9. **ALWAYS** rotate keys periodically
10. **ALWAYS** validate on server even if validated on client

## Configuration (appsettings.json)

```json
{
  "Jwt": {
    "Issuer": "https://[domain]/",
    "Audience": "https://[domain]/",
    "Key": "[USE USER SECRETS]",
    "ExpirationHours": 24
  }
}
```
