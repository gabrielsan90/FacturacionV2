# Páginas de Autenticación

## Descripción General

Módulo de autenticación para el Sistema de Facturación Electrónica v4.4 de Costa Rica. Incluye las páginas de Login, Logout y Access Denied, completamente integradas con el servicio `IAuthService`.

---

## Archivos Creados

### 1. Login.cshtml
**Ubicación**: `/Pages/Auth/Login.cshtml`

**Descripción**: Página de inicio de sesión con diseño moderno y responsive.

**Características**:
- Diseño responsive (mobile-first)
- Gradiente corporativo en el fondo
- Validación client-side con jQuery Validation
- Validación server-side en el PageModel
- Mensajes de error con SweetAlert2
- Soporte para ReturnUrl (redirección después del login)
- Anti-forgery token automático
- Focus automático en el campo de email
- Spinner durante el proceso de autenticación
- Prevención de doble submit
- Sin layout (página independiente)

**Campos del Formulario**:
- Email (requerido, validación de formato)
- Contraseña (requerido, mínimo 6 caracteres)

**Validaciones Client-Side**:
- Formato de email válido
- Contraseña mínimo 6 caracteres
- Campos requeridos

**Mensajes de Error**:
- Se muestran mediante TempData["ErrorMessage"]
- Mensajes genéricos por seguridad (no revela si el email existe o la contraseña está incorrecta)

**Tecnologías Utilizadas**:
- Bootstrap 5
- Bootstrap Icons
- jQuery
- jQuery Validation
- jQuery Validation Unobtrusive
- SweetAlert2

---

### 2. Login.cshtml.cs
**Ubicación**: `/Pages/Auth/Login.cshtml.cs`

**Descripción**: PageModel para el manejo del inicio de sesión.

**Atributos**:
- `[AllowAnonymous]`: Permite acceso sin autenticación

**Propiedades**:
- `Input`: Modelo de entrada con Email y Password
- `ReturnUrl`: URL para redirección después del login exitoso

**Métodos**:

#### OnGet(string? returnUrl = null)
- Verifica si el usuario ya está autenticado
- Si está autenticado, redirige a /Index
- Guarda el returnUrl para redirección posterior

#### OnPostAsync(string? returnUrl = null)
- Valida el ModelState
- Llama a `AuthService.LoginAsync()` con las credenciales
- Si el login es exitoso: redirige al returnUrl o a la página principal
- Si el login falla: muestra mensaje de error genérico
- Registra todos los intentos en el logger

**Validaciones Server-Side**:
- Email requerido y formato válido
- Contraseña requerida y mínimo 6 caracteres

**Seguridad**:
- Mensajes genéricos (no revela si el email existe)
- Logging de todos los intentos de autenticación
- Prevención de información sensible en logs

---

### 3. Logout.cshtml.cs
**Ubicación**: `/Pages/Auth/Logout.cshtml.cs`

**Descripción**: PageModel para el cierre de sesión.

**Nota**: No tiene archivo .cshtml (solo PageModel)

**Atributos**:
- `[Authorize]`: Requiere usuario autenticado

**Métodos**:

#### OnGet()
- Obtiene información del usuario actual (para logging)
- Llama a `AuthService.LogoutAsync()`
- Registra el cierre de sesión en el logger
- Muestra mensaje de éxito mediante TempData
- Redirige a `/Auth/Login`

#### OnPost()
- Llama internamente a OnGet()
- Permite cierre de sesión por POST (para prevenir CSRF)

**Flujo**:
1. Usuario accede a /Auth/Logout
2. Sistema obtiene datos del usuario
3. Cierra la sesión (elimina cookie de autenticación)
4. Registra el evento en logs
5. Redirige a la página de login con mensaje de éxito

---

### 4. AccessDenied.cshtml
**Ubicación**: `/Pages/Auth/AccessDenied.cshtml`

**Descripción**: Página mostrada cuando un usuario autenticado intenta acceder a un recurso para el cual no tiene permisos.

**Características**:
- Diseño responsive y amigable
- Muestra información del usuario actual (si está autenticado)
- Muestra roles del usuario
- Icono visual llamativo
- Botones de acción claros
- Sin layout (página independiente)

**Información Mostrada**:
- Nombre completo del usuario
- Email del usuario
- Roles asignados
- Mensaje explicativo claro

**Botones de Acción**:
- "Ir al Dashboard": Redirige a /Index
- "Cerrar Sesión": Redirige a /Auth/Logout

**Diseño**:
- Icono de escudo con X
- Gradiente en el fondo
- Card centrada con información
- Mensaje amable y profesional

---

### 5. AccessDenied.cshtml.cs
**Ubicación**: `/Pages/Auth/AccessDenied.cshtml.cs`

**Descripción**: PageModel para la página de acceso denegado.

**Atributos**:
- `[AllowAnonymous]`: Permite acceso sin autenticación (para mostrar el mensaje incluso si la sesión expiró)

**Propiedades**:
- `CurrentUser`: Información del usuario actual (si está autenticado)

**Métodos**:

#### OnGetAsync()
- Obtiene información del usuario actual mediante `AuthService.GetCurrentUserAsync()`
- Registra en logs el intento de acceso denegado
- Registra el email y roles del usuario (si está autenticado)
- Maneja errores silenciosamente (no bloquea la página)

**Logging**:
- Registra todos los intentos de acceso denegado
- Incluye información del usuario y sus roles
- Permite auditoría de intentos de acceso no autorizados

---

## Integración con el Sistema

### Configuración en Program.cs

El sistema ya tiene configurada la autenticación por cookies:

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "FacturacionAuth";
    });
```

### Rutas Configuradas

- **LoginPath**: `/Auth/Login` - Redirige aquí cuando el usuario no está autenticado
- **AccessDeniedPath**: `/Auth/AccessDenied` - Redirige aquí cuando el usuario no tiene permisos
- **Logout**: `/Auth/Logout` - Cierra la sesión del usuario

### Servicio de Autenticación

Las páginas utilizan `IAuthService` que ya está implementado en:
- `/Services/IAuthService.cs`
- `/Services/AuthService.cs`

**Métodos Utilizados**:
- `LoginAsync(LoginDto model)`: Autentica al usuario
- `LogoutAsync()`: Cierra la sesión del usuario
- `GetCurrentUserAsync()`: Obtiene información del usuario actual
- `IsAuthenticated()`: Verifica si el usuario está autenticado

---

## Flujos de Usuario

### Flujo de Login Exitoso

1. Usuario navega a `/Auth/Login`
2. Ingresa email y contraseña
3. Sistema valida formato (client-side)
4. Usuario hace submit
5. Sistema valida datos (server-side)
6. `AuthService.LoginAsync()` autentica con el backend
7. Backend devuelve JWT token y datos del usuario
8. `AuthService` crea cookie de autenticación con claims
9. Usuario es redirigido a la página solicitada o al Dashboard

### Flujo de Login Fallido

1. Usuario navega a `/Auth/Login`
2. Ingresa credenciales incorrectas
3. Sistema valida formato (client-side)
4. Usuario hace submit
5. `AuthService.LoginAsync()` intenta autenticar
6. Backend rechaza las credenciales
7. Se muestra mensaje genérico de error
8. Usuario permanece en la página de login
9. Evento se registra en logs para auditoría

### Flujo de Logout

1. Usuario hace clic en "Cerrar Sesión"
2. Sistema redirige a `/Auth/Logout`
3. `AuthService.LogoutAsync()` elimina la cookie de autenticación
4. Usuario es redirigido a `/Auth/Login`
5. Se muestra mensaje de éxito
6. Evento se registra en logs

### Flujo de Access Denied

1. Usuario autenticado intenta acceder a una página protegida
2. Sistema verifica roles/permisos
3. Usuario no tiene los permisos requeridos
4. Sistema redirige automáticamente a `/Auth/AccessDenied`
5. Se muestra página con información del usuario
6. Usuario puede ir al Dashboard o cerrar sesión
7. Intento se registra en logs para auditoría

---

## Seguridad Implementada

### Protección contra Ataques

1. **CSRF Protection**: Anti-forgery tokens en formularios
2. **XSS Prevention**: Validación y sanitización de inputs
3. **Información Genérica**: No revela si el email existe o la contraseña está incorrecta
4. **Logging Completo**: Auditoría de todos los intentos de autenticación
5. **HTTPS Only**: Cookies solo por HTTPS (SecurePolicy.Always)
6. **HttpOnly Cookies**: Cookies no accesibles por JavaScript
7. **SameSite**: Protección contra CSRF (SameSite.Lax)

### Validaciones Implementadas

**Client-Side**:
- Formato de email
- Longitud mínima de contraseña
- Campos requeridos
- Prevención de doble submit

**Server-Side**:
- ModelState validation
- Data annotations
- Validación de credenciales con el backend
- Manejo de excepciones

---

## Responsive Design

Todas las páginas son completamente responsive y se adaptan a:

- **Móviles** (< 576px)
  - Formularios adaptados a pantallas pequeñas
  - Botones de tamaño táctil (44px mínimo)
  - Tipografía ajustada
  - Padding reducido para optimizar espacio

- **Tablets** (576px - 991px)
  - Layout optimizado para tablets
  - Formularios centrados
  - Botones de tamaño adecuado

- **Escritorio** (> 992px)
  - Layout completo
  - Formularios con espaciado amplio
  - Diseño optimizado para mouse

---

## Compatibilidad

### Navegadores Soportados

- Chrome (últimas 2 versiones)
- Firefox (últimas 2 versiones)
- Safari (últimas 2 versiones)
- Edge (últimas 2 versiones)

### Tecnologías Requeridas

- JavaScript habilitado (para validación client-side)
- Cookies habilitadas (para autenticación)
- HTTPS (para cookies seguras)

---

## Pruebas Recomendadas

### Pruebas Funcionales

1. **Login Exitoso**
   - Verificar redirección correcta
   - Verificar cookie de autenticación
   - Verificar claims en la cookie

2. **Login Fallido**
   - Verificar mensaje de error
   - Verificar que no se crea cookie
   - Verificar logging del intento

3. **Logout**
   - Verificar eliminación de cookie
   - Verificar redirección a login
   - Verificar mensaje de éxito

4. **Access Denied**
   - Verificar redirección automática
   - Verificar información del usuario
   - Verificar logging del intento

### Pruebas de Seguridad

1. **CSRF Protection**
   - Intentar submit sin anti-forgery token
   - Verificar rechazo de la petición

2. **XSS Prevention**
   - Ingresar scripts en campos de texto
   - Verificar sanitización

3. **Información Sensible**
   - Verificar mensajes genéricos
   - Verificar que no se revela información del sistema

### Pruebas de UI/UX

1. **Responsive Design**
   - Probar en móvil (320px, 375px, 414px)
   - Probar en tablet (768px, 1024px)
   - Probar en escritorio (1920px)

2. **Validación**
   - Probar validación client-side
   - Probar validación server-side
   - Verificar mensajes de error claros

3. **Usabilidad**
   - Verificar focus automático
   - Verificar soporte de Enter key
   - Verificar navegación por tabulador

---

## Mantenimiento

### Logs

Todos los eventos de autenticación se registran en:
- `ILogger<LoginModel>`
- `ILogger<LogoutModel>`
- `ILogger<AccessDeniedModel>`

**Eventos Registrados**:
- Intentos de login (exitosos y fallidos)
- Cierres de sesión
- Accesos denegados con roles del usuario

### Personalización

Para personalizar el diseño:

1. **Colores**: Modificar variables CSS en `:root`
2. **Logo**: Cambiar icono de Bootstrap Icons
3. **Textos**: Editar directamente en las vistas
4. **Validaciones**: Modificar Data Annotations en InputModel

---

## Próximos Pasos

Después de implementar las páginas de autenticación:

1. **Proteger Páginas**: Agregar `[Authorize]` a páginas que requieren autenticación
2. **Roles**: Agregar `[Authorize(Roles = "Admin")]` según sea necesario
3. **Layout Principal**: Actualizar `_Layout.cshtml` para mostrar usuario logueado y opción de logout
4. **Dashboard**: Crear página de Dashboard (/Index) como landing page después del login
5. **Recordar Contraseña**: Implementar flujo de recuperación de contraseña (fase futura)

---

## Soporte

Para dudas o problemas:
- Revisar logs en `ILogger`
- Verificar configuración en `Program.cs`
- Verificar que `IAuthService` está registrado en DI
- Verificar que el backend API está funcionando

---

**Versión**: 1.0
**Fecha**: 21 de noviembre de 2025
**Framework**: .NET 9.0
**Sistema**: Facturación Electrónica v4.4 - Costa Rica
