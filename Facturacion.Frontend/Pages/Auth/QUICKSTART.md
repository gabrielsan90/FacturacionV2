# QUICK START - Páginas de Autenticación

## Inicio Rápido (5 minutos)

### 1. Verificar que el Backend está corriendo
```bash
# El backend debe estar corriendo en la URL configurada
# Por defecto debería ser algo como: https://localhost:7001
```

### 2. Ejecutar el Frontend
```bash
cd /mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend
dotnet run
```

### 3. Navegar a la página de Login
```
https://localhost:5001/Auth/Login
```

### 4. Probar el Login
- Email: `admin@ejemplo.com` (usar un usuario que exista en el backend)
- Password: `123456` (usar la contraseña correcta)
- Click en "Iniciar Sesión"

### 5. Verificar que funciona
- Deberías ser redirigido a `/Index` (o la URL solicitada)
- Deberías ver tu información de usuario en la barra superior (cuando se implemente)

---

## URLs Disponibles

| URL | Descripción | Requiere Auth |
|-----|-------------|---------------|
| `/Auth/Login` | Página de inicio de sesión | No |
| `/Auth/Logout` | Cerrar sesión | Sí |
| `/Auth/AccessDenied` | Acceso denegado | No |

---

## Probar las Páginas

### Probar Login Exitoso
1. Ir a `/Auth/Login`
2. Ingresar credenciales válidas
3. Click en "Iniciar Sesión"
4. ✅ Deberías ser redirigido a `/Index`

### Probar Login Fallido
1. Ir a `/Auth/Login`
2. Ingresar credenciales inválidas
3. Click en "Iniciar Sesión"
4. ✅ Deberías ver mensaje de error

### Probar Logout
1. Estando autenticado, ir a `/Auth/Logout`
2. ✅ Deberías ser redirigido a `/Auth/Login` con mensaje de éxito

### Probar Access Denied
1. Estando autenticado, ir a `/Auth/AccessDenied`
2. ✅ Deberías ver la página con tu información de usuario

---

## Integrar con el resto del sistema

### 1. Proteger páginas que requieren autenticación

Agregar `[Authorize]` a las páginas:

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Facturacion.Frontend.Pages;

[Authorize] // ← Agregar esta línea
public class IndexModel : PageModel
{
    // ...
}
```

### 2. Proteger páginas por rol

```csharp
[Authorize(Roles = "Admin")] // Solo administradores
public class UsuariosModel : PageModel
{
    // ...
}
```

```csharp
[Authorize(Roles = "Admin,SuperUser")] // Admin O SuperUser
public class EmpresasModel : PageModel
{
    // ...
}
```

### 3. Actualizar _Layout.cshtml para mostrar usuario y logout

Agregar en la barra superior:

```html
@using Microsoft.AspNetCore.Authorization
@inject IAuthorizationService AuthorizationService
@inject Facturacion.Frontend.Services.IAuthService AuthService

@{
    var user = await AuthService.GetCurrentUserAsync();
}

@if (user != null)
{
    <div class="user-menu">
        <span>@user.FullName</span>
        <a href="/Auth/Logout" class="btn btn-outline-danger">
            <i class="bi bi-box-arrow-right"></i> Cerrar Sesión
        </a>
    </div>
}
```

### 4. Crear página de Dashboard (/Index)

Si aún no existe, crear `/Pages/Index.cshtml`:

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
```

---

## Verificar Configuración

### Program.cs debe tener:

```csharp
// ✅ Ya está configurado
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
    });

// ✅ Ya está configurado
builder.Services.AddScoped<IAuthService, AuthService>();

// En el pipeline:
app.UseAuthentication(); // ✅ ANTES de UseAuthorization
app.UseAuthorization();
```

---

## Troubleshooting

### Problema: "No se puede conectar al backend"
**Solución**: Verificar que el backend está corriendo y la URL es correcta en appsettings.json

### Problema: "Login no funciona"
**Solución**:
1. Verificar que el usuario existe en el backend
2. Verificar que la contraseña es correcta
3. Revisar logs del backend para ver el error
4. Revisar logs del frontend (ILogger)

### Problema: "Cookies no se crean"
**Solución**:
1. Verificar que estás usando HTTPS
2. Verificar que las cookies están habilitadas en el navegador
3. Revisar la configuración de cookies en Program.cs

### Problema: "Access Denied aparece inmediatamente"
**Solución**:
1. Verificar que el usuario tiene el rol correcto
2. Verificar el atributo [Authorize] en la página
3. Revisar la configuración de roles en el backend

### Problema: "Validación client-side no funciona"
**Solución**:
1. Verificar que JavaScript está habilitado
2. Verificar que jQuery está cargando correctamente
3. Revisar consola del navegador para errores

---

## Logs y Debugging

### Ver logs del frontend
```bash
# Los logs aparecen en la consola donde ejecutaste dotnet run
# Buscar líneas como:
# [Information] Usuario admin@ejemplo.com inició sesión exitosamente
# [Warning] Intento de inicio de sesión fallido para usuario@ejemplo.com
```

### Ver cookies en el navegador
1. Abrir DevTools (F12)
2. Ir a Application → Cookies
3. Buscar cookie "FacturacionAuth"
4. Verificar que existe y tiene un valor

### Ver claims del usuario
Agregar esto temporalmente en cualquier página:

```csharp
public void OnGet()
{
    foreach (var claim in User.Claims)
    {
        _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
    }
}
```

---

## Comandos Útiles

### Compilar el proyecto
```bash
dotnet build
```

### Ejecutar el proyecto
```bash
dotnet run
```

### Ver logs detallados
```bash
dotnet run --verbosity detailed
```

### Limpiar y recompilar
```bash
dotnet clean
dotnet build
```

---

## Próximos Pasos

1. ✅ Probar las 3 páginas de autenticación
2. ✅ Agregar `[Authorize]` a las páginas existentes
3. ✅ Actualizar `_Layout.cshtml` para mostrar usuario y logout
4. ✅ Crear Dashboard (/Index) si no existe
5. ✅ Implementar resto de módulos del sistema

---

## Ayuda Adicional

- **Documentación completa**: Ver `README.md` en esta carpeta
- **Reporte de implementación**: Ver `REPORTE_IMPLEMENTACION.md`
- **Especificación del sistema**: Ver `/ESPECIFICACION_SISTEMA.md` en la raíz

---

**¿Listo para empezar?** 🚀

1. Asegúrate de que el backend está corriendo
2. Ejecuta `dotnet run` en el frontend
3. Navega a `https://localhost:5001/Auth/Login`
4. Ingresa tus credenciales
5. ¡Listo! Ya puedes usar el sistema

---

**Versión**: 1.0
**Fecha**: 21 de noviembre de 2025
**Sistema**: Facturación Electrónica v4.4 - Costa Rica
