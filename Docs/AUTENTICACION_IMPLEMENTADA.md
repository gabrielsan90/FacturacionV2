# SISTEMA DE AUTENTICACION IMPLEMENTADO
## Sistema de Facturacion Electronica v4.4 - Costa Rica

**Fecha de implementacion:** 21 de noviembre de 2025
**Version:** 1.0
**Estado:** Completado y listo para pruebas

---

## RESUMEN EJECUTIVO

Se ha configurado exitosamente el sistema completo de autenticacion para el proyecto de Facturacion Electronica, cumpliendo con todos los requisitos especificados en ESPECIFICACION_SISTEMA.md (seccion 2️⃣6️⃣).

### Caracteristicas implementadas:

- ✅ Backend con JWT (8 horas de expiracion)
- ✅ Frontend con Cookies seguras (8 horas de expiracion)
- ✅ Control de intentos fallidos (5 intentos, 15 minutos de bloqueo)
- ✅ Politica de contraseñas (minimo 6 caracteres, sin requisitos especiales)
- ✅ Auditoria completa de intentos de login
- ✅ CORS configurado para comunicacion segura
- ✅ HTTPS obligatorio
- ✅ Cookies HttpOnly, Secure y SameSite

---

## 1. BACKEND - CONFIGURACION JWT

### Archivos modificados:

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Program.cs`

**Cambios realizados:**

1. **Configuracion de ASP.NET Core Identity:**
   ```csharp
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
   ```

2. **Configuracion CORS mejorada:**
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowFrontend", policy =>
       {
           var frontendUrl = builder.Configuration["FrontendUrl"] ?? "https://localhost:7031";

           policy.WithOrigins(frontendUrl)
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials(); // Required for cookie-based authentication
       });
   });
   ```

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/appsettings.json`

**Cambios realizados:**

```json
{
  "FrontendUrl": "https://localhost:7031",
  "Jwt": {
    "Issuer": "https://localhost:7030",
    "Audience": "https://localhost:7030",
    "Key": "clave-secreta-jwt-facturacion-minimo-32-caracteres-requeridos-aqui",
    "ExpirationHours": 8
  }
}
```

**IMPORTANTE - PRODUCCION:**
- La clave JWT debe cambiarse en produccion
- Usar variables de entorno o Azure Key Vault
- NUNCA hacer commit de la clave de produccion

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Controllers/AccountsController.cs`

**Implementacion completa del endpoint de Login:**

- Verifica si el usuario existe
- Comprueba si la cuenta esta bloqueada
- Maneja intentos fallidos con contador
- Resetea intentos al login exitoso
- Genera JWT con roles incluidos
- Retorna respuesta completa (token + usuario + roles + expiracion)
- Registra en logs todos los eventos

**Endpoint:** `POST /api/accounts/login`

**Request:**
```json
{
  "email": "usuario@ejemplo.com",
  "password": "password123"
}
```

**Response exitoso (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "guid-del-usuario",
    "email": "usuario@ejemplo.com",
    "fullName": "Nombre Completo",
    "document": "123456789",
    "roles": ["Administrador de Empresa", "Facturador"]
  },
  "expiresAt": "2025-11-21T20:00:00Z"
}
```

**Response error credenciales (400 Bad Request):**
```json
"Email o contraseña incorrectos. Intentos restantes: 3"
```

**Response cuenta bloqueada (400 Bad Request):**
```json
"Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intente nuevamente en 12 minuto(s)."
```

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Helpers/UserHelper.cs`

**Metodos agregados:**

```csharp
Task<User?> GetUserByEmailAsync(string email)
Task<bool> IsLockedOutAsync(User user)
Task<int> GetAccessFailedCountAsync(User user)
Task<IdentityResult> ResetAccessFailedCountAsync(User user)
Task<DateTimeOffset?> GetLockoutEndDateAsync(User user)
```

**Modificacion en LoginAsync:**
- Ahora incluye `lockoutOnFailure: true` para habilitar bloqueo automatico

---

## 2. SHARED - DTOs CREADOS

### Archivos creados:

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/DTOs/LoginResponseDto.cs`

```csharp
public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
```

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/DTOs/UserDto.cs`

```csharp
public class UserDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Document { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}
```

---

## 3. FRONTEND - CONFIGURACION COOKIES

### Archivos modificados:

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Program.cs`

**Cambios realizados:**

1. **Configuracion de Cookies seguras:**
   ```csharp
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
   ```

2. **Registro de servicios:**
   ```csharp
   builder.Services.AddHttpContextAccessor();
   builder.Services.AddScoped<IApiService, ApiService>();
   builder.Services.AddScoped<IAuthService, AuthService>();
   ```

### Archivos creados:

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Services/IAuthService.cs`

```csharp
public interface IAuthService
{
    Task<bool> LoginAsync(LoginDto model);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    bool IsAuthenticated();
}
```

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Services/AuthService.cs`

**Funcionalidades implementadas:**

1. **LoginAsync:**
   - Llama al endpoint del Backend
   - Recibe token JWT y datos del usuario
   - Crea claims a partir de la respuesta
   - Almacena el token en claims (para llamadas HTTP)
   - Crea cookie de autenticacion con todos los claims
   - Configura expiracion sincronizada con JWT

2. **LogoutAsync:**
   - Elimina la sesion de autenticacion
   - Limpia las cookies

3. **GetCurrentUserAsync:**
   - Extrae informacion del usuario de los claims
   - Retorna DTO con datos completos

4. **IsAuthenticated:**
   - Verifica si hay sesion activa

**Claims almacenados en cookie:**
- ClaimTypes.NameIdentifier (ID usuario)
- ClaimTypes.Name (Email)
- ClaimTypes.Email (Email)
- FullName
- Document
- Token (JWT para llamadas API)
- TokenExpiration
- ClaimTypes.Role (multiples roles)

---

## 4. FLUJO COMPLETO DE AUTENTICACION

### Login - Flujo paso a paso:

1. **Usuario ingresa credenciales en Frontend**
   - Email y contraseña en formulario

2. **Frontend llama a AuthService.LoginAsync()**
   - AuthService llama a API Backend: `POST /api/accounts/login`

3. **Backend verifica usuario**
   - Busca usuario por email
   - Si no existe: error generico (NO revela que no existe)

4. **Backend verifica bloqueo**
   - Si bloqueado: retorna tiempo restante de bloqueo
   - Si no bloqueado: continua

5. **Backend verifica credenciales**
   - Llama a `UserHelper.LoginAsync()` con `lockoutOnFailure: true`
   - Si credenciales correctas: continua al paso 6
   - Si incorrectas: incrementa contador y retorna intentos restantes

6. **Backend genera token JWT**
   - Obtiene roles del usuario
   - Crea claims (ID, Email, FullName, Document, Roles)
   - Genera JWT con expiracion de 8 horas
   - Resetea contador de intentos fallidos

7. **Backend retorna respuesta**
   - Token JWT
   - Datos completos del usuario
   - Roles
   - Fecha de expiracion

8. **Frontend procesa respuesta**
   - Extrae token y datos de usuario
   - Crea claims para cookie
   - Crea cookie de autenticacion con:
     - HttpOnly: true
     - Secure: true
     - SameSite: Lax
     - Expiracion: 8 horas

9. **Usuario autenticado**
   - Puede acceder a paginas protegidas
   - JWT incluido en claims para llamadas HTTP

### Logout - Flujo:

1. Usuario selecciona "Cerrar sesion"
2. Frontend llama a `AuthService.LogoutAsync()`
3. Se elimina la cookie de autenticacion
4. Usuario redirigido a pagina de login

---

## 5. SEGURIDAD IMPLEMENTADA

### Proteccion contra ataques:

#### 1. Fuerza bruta
- ✅ Bloqueo automatico despues de 5 intentos
- ✅ Bloqueo temporal de 15 minutos
- ✅ Contador de intentos visible para el usuario

#### 2. XSS (Cross-Site Scripting)
- ✅ Cookies HttpOnly (no accesibles desde JavaScript)
- ✅ Razor Pages escapa HTML automaticamente

#### 3. CSRF (Cross-Site Request Forgery)
- ✅ SameSite cookies (Lax)
- ✅ CORS restrictivo (solo dominio especifico)

#### 4. Man-in-the-Middle
- ✅ HTTPS obligatorio (UseHttpsRedirection)
- ✅ Cookies Secure (solo HTTPS)

#### 5. Session Hijacking
- ✅ Sesion fija de 8 horas
- ✅ No renovacion automatica (SlidingExpiration: false)
- ✅ Token incluye claims de validacion

### Auditoria y logging:

Todos los siguientes eventos se registran en logs:

- ✅ Login exitoso
- ✅ Login fallido con contador
- ✅ Cuenta bloqueada
- ✅ Intento de login en cuenta bloqueada
- ✅ Email no existente (sin revelar al usuario)

**Ubicacion de logs:** Logs de ASP.NET Core (consola en desarrollo)

---

## 6. CONFIGURACION DE POLITICAS

### Contraseñas:

Segun ESPECIFICACION_SISTEMA.md, seccion 2️⃣6️⃣:

- Longitud minima: 6 caracteres
- Sin requisitos especiales (digitos, mayusculas, simbolos)
- No expiran
- Cumplimiento: ✅ IMPLEMENTADO

### Bloqueo de cuenta:

Segun ESPECIFICACION_SISTEMA.md, seccion 2️⃣6️⃣:

- Intentos maximos: 5
- Duracion: 15 minutos
- Reset al login exitoso
- Cumplimiento: ✅ IMPLEMENTADO

### Sesion:

Segun ESPECIFICACION_SISTEMA.md, seccion 2️⃣6️⃣:

- Duracion: 8 horas desde login
- Tipo: Fija (no renovable)
- Cumplimiento: ✅ IMPLEMENTADO

---

## 7. CHECKLIST DE VERIFICACION

### Backend:

- [x] JWT configurado con validacion completa
- [x] Clave JWT de 32+ caracteres
- [x] Issuer y Audience validados
- [x] Expiracion: 8 horas
- [x] RoleClaimType: ClaimTypes.Role
- [x] NameClaimType: ClaimTypes.NameIdentifier
- [x] Politica de contraseñas: minimo 6 caracteres
- [x] Bloqueo: 5 intentos, 15 minutos
- [x] Auditoria de intentos (logs)
- [x] CORS especifico para Frontend
- [x] HTTPS habilitado
- [x] Endpoint /api/accounts/login implementado
- [x] Response completa con token + usuario + roles

### Frontend:

- [x] Cookies HttpOnly
- [x] Cookies Secure (Always)
- [x] SameSite: Lax
- [x] Expiracion: 8 horas
- [x] SlidingExpiration: false
- [x] AuthService implementado
- [x] HttpContextAccessor registrado
- [x] HTTPS habilitado
- [x] Claims completos en cookie

### Pendientes (proximas fases):

- [ ] Crear paginas de Login/Logout en Frontend
- [ ] Proteger paginas con [Authorize]
- [ ] Implementar anti-forgery tokens
- [ ] Rate limiting global
- [ ] Migracion de secretos a Azure Key Vault (produccion)

---

## 8. COMO PROBAR

### 1. Probar login exitoso:

**Backend:**
```bash
curl -X POST https://localhost:7030/api/accounts/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@facturacion.com","password":"Admin123"}'
```

**Respuesta esperada:**
- Status: 200 OK
- Body: JSON con token, usuario, roles y expiracion

### 2. Probar intentos fallidos:

**Backend:**
```bash
# Intento 1 (contraseña incorrecta)
curl -X POST https://localhost:7030/api/accounts/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@facturacion.com","password":"incorrecta"}'

# Repetir 5 veces
# En el intento 5: cuenta bloqueada
# En el intento 6: mensaje de cuenta bloqueada con tiempo restante
```

### 3. Probar bloqueo temporal:

1. Fallar 5 intentos consecutivos
2. Esperar que se bloquee la cuenta
3. Intentar login nuevamente
4. Verificar mensaje: "Cuenta bloqueada... en X minutos"
5. Esperar 15 minutos
6. Verificar que se puede volver a intentar

### 4. Probar Frontend:

1. Crear pagina de login en Frontend
2. Inyectar `IAuthService`
3. Llamar a `await _authService.LoginAsync(model)`
4. Si retorna `true`: redirigir a dashboard
5. Si retorna `false`: mostrar error
6. Verificar que se crea cookie "FacturacionAuth"
7. Verificar que cookie es HttpOnly y Secure

---

## 9. PROXIMOS PASOS

### Fase 1: Interfaz de Usuario (Frontend)

1. **Crear pagina de Login**
   - `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Pages/Auth/Login.cshtml`
   - Formulario con email y password
   - Mensajes de error
   - Contador de intentos restantes

2. **Crear pagina de Logout**
   - `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Pages/Auth/Logout.cshtml`
   - Redireccionar a login despues de logout

3. **Crear pagina de Access Denied**
   - `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Pages/Auth/AccessDenied.cshtml`
   - Mostrar mensaje de acceso denegado

### Fase 2: Proteccion de paginas

1. **Agregar [Authorize] a paginas**
   - Dashboard: `[Authorize]`
   - Administracion: `[Authorize(Roles = "SuperUser,Administrador de Empresa")]`
   - Facturacion: `[Authorize(Roles = "SuperUser,Administrador de Empresa,Facturador")]`

### Fase 3: Mejoras de seguridad

1. **Implementar anti-forgery tokens**
   - En todos los formularios POST

2. **Rate limiting**
   - Limitar intentos de login por IP

3. **Rotacion de claves JWT**
   - Implementar rotacion periodica

---

## 10. ARCHIVOS MODIFICADOS Y CREADOS

### Backend - Modificados:

1. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Program.cs`
   - Configuracion Identity con lockout
   - CORS mejorado
   - Ya tenia JWT configurado (sin cambios)

2. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/appsettings.json`
   - Agregado FrontendUrl
   - Agregado ExpirationHours: 8

3. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Controllers/AccountsController.cs`
   - Implementacion completa de login
   - Manejo de intentos fallidos
   - Auditoria con ILogger

4. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Helpers/UserHelper.cs`
   - Metodos para lockout
   - LoginAsync con lockoutOnFailure

5. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Helpers/IUserHelper.cs`
   - Nuevas firmas de metodos

### Shared - Creados:

1. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/DTOs/LoginResponseDto.cs`
2. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/DTOs/UserDto.cs`

### Frontend - Modificados:

1. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Program.cs`
   - Configuracion de cookies seguras
   - Registro de AuthService
   - HttpContextAccessor

### Frontend - Creados:

1. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Services/IAuthService.cs`
2. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Services/AuthService.cs`

---

## 11. REFERENCIAS

- **ESPECIFICACION_SISTEMA.md** - Seccion 2️⃣6️⃣ (Seguridad)
- **SECURITY_CONFIG.md** - Guia general de seguridad
- **BACKEND_PATTERNS.md** - Patrones de backend
- **FRONTEND_PATTERNS.md** - Patrones de frontend

---

**Documento creado:** 21 de noviembre de 2025
**Estado:** Implementacion completa - Listo para testing
**Responsable:** Sistema de Facturacion Electronica CR v4.4
