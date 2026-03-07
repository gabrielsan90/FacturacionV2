# GUÍA DE PATRONES DEL FRONTEND

## PROPÓSITO
Este documento define TODOS los patrones de código que DEBEN usarse en los proyectos Frontend (FrontendAdmin, FrontendUser, Frontend).

## ESTRUCTURA DE CARPETAS

```
[NOMBREPROYECTO].Frontend/
├── Pages/
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   ├── Login.cshtml.cs
│   │   ├── ForgotPassword.cshtml
│   │   └── ForgotPassword.cshtml.cs
│   ├── [Entidad].cshtml          ← NUNCA usar "Index.cshtml"
│   ├── [Entidad].cshtml.cs
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _NavBar.cshtml
│   │   └── _SideBar.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── lib/                      ← Bootstrap, jQuery, DataTables, etc.
│   └── img/
├── Program.cs
└── appsettings.json
```

## 1. RAZOR PAGE MODEL PATTERN

### NOMBRAR PÁGINAS

```
❌ MAL:
  Pages/Index.cshtml           ← NUNCA
  Pages/List.cshtml            ← NUNCA
  Pages/Manage.cshtml          ← NUNCA

✅ BIEN:
  Pages/Paquetes.cshtml        ← Nombre de entidad en plural
  Pages/Usuarios.cshtml
  Pages/Productos.cshtml
  Pages/TicketsPago.cshtml
```

### ESTRUCTURA BÁSICA: [Entidad].cshtml

```cshtml
@page
@model MJL.FrontendAdmin.Pages.PaquetesModel
@{
    ViewData["Title"] = "Gestión de Paquetes";
}

<div class="container-fluid">
    <h1>Paquetes</h1>

    <!-- Botón para abrir modal -->
    <button type="button" class="btn btn-primary mb-3" onclick="openModal()">
        <i class="fa fa-plus"></i> Nuevo Paquete
    </button>

    <!-- DataTable -->
    <table id="tablePaquetes" class="table table-striped table-bordered">
        <thead>
            <tr>
                <th>Tracking</th>
                <th>Descripción</th>
                <th>Usuario</th>
                <th>Estado</th>
                <th>Acciones</th>
            </tr>
        </thead>
    </table>
</div>

<!-- Modal para crear/editar -->
<div class="modal fade" id="modalPaquete" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <form id="formPaquete">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalTitle">Nuevo Paquete</h5>
                    <button type="button" class="close" data-dismiss="modal">
                        <span>&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="Id" name="Paquete.Id" value="0" />

                    <div class="form-group">
                        <label for="NumeroTracking">Número de Tracking *</label>
                        <input type="text" class="form-control" id="NumeroTracking"
                               name="Paquete.NumeroTracking" required />
                    </div>

                    <div class="form-group">
                        <label for="Descripcion">Descripción</label>
                        <textarea class="form-control" id="Descripcion"
                                  name="Paquete.Descripcion" rows="3"></textarea>
                    </div>

                    <div class="form-group">
                        <label for="UserId">Usuario *</label>
                        <select class="form-control" id="UserId" name="Paquete.UserId" required>
                            <option value="">Seleccione...</option>
                            @foreach (var user in Model.Usuarios)
                            {
                                <option value="@user.Value">@user.Text</option>
                            }
                        </select>
                    </div>

                    <div class="form-group">
                        <label for="EstadoId">Estado *</label>
                        <select class="form-control" id="EstadoId" name="Paquete.EstadoId" required>
                            <option value="">Seleccione...</option>
                            @foreach (var estado in Model.Estados)
                            {
                                <option value="@estado.Value">@estado.Text</option>
                            }
                        </select>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                    <button type="submit" class="btn btn-primary">Guardar</button>
                </div>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        // PATRÓN: Variables globales
        var table;
        var apiBaseUrl = '@Model.Configuration["ApiBaseUrl"]';

        // PATRÓN: Document ready
        $(document).ready(function () {
            loadDataTable();
            setupFormSubmit();
        });

        // PATRÓN: Cargar DataTable
        function loadDataTable() {
            table = $('#tablePaquetes').DataTable({
                ajax: {
                    url: '?handler=Data',
                    type: 'GET',
                    headers: {
                        "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                    },
                    dataSrc: 'data'
                },
                columns: [
                    { data: 'numeroTracking' },
                    { data: 'descripcion' },
                    { data: 'user.fullName' },
                    { data: 'estado.nombre' },
                    {
                        data: 'id',
                        render: function (data) {
                            return `
                                <button class="btn btn-sm btn-info" onclick="edit(${data})">
                                    <i class="fa fa-edit"></i>
                                </button>
                                <button class="btn btn-sm btn-danger" onclick="deleteRecord(${data})">
                                    <i class="fa fa-trash"></i>
                                </button>
                            `;
                        }
                    }
                ],
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
                },
                responsive: true,
                order: [[0, 'desc']]
            });
        }

        // PATRÓN: Configurar submit del formulario
        function setupFormSubmit() {
            $('#formPaquete').on('submit', function (e) {
                e.preventDefault();
                save();
            });
        }

        // PATRÓN: Abrir modal para crear
        function openModal() {
            $('#modalTitle').text('Nuevo Paquete');
            $('#formPaquete')[0].reset();
            $('#Id').val('0');
            $('#modalPaquete').modal('show');
        }

        // PATRÓN: Editar registro
        function edit(id) {
            $.ajax({
                url: `?handler=Details&id=${id}`,
                type: 'GET',
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        $('#modalTitle').text('Editar Paquete');
                        $('#Id').val(response.data.id);
                        $('#NumeroTracking').val(response.data.numeroTracking);
                        $('#Descripcion').val(response.data.descripcion);
                        $('#UserId').val(response.data.userId);
                        $('#EstadoId').val(response.data.estadoId);
                        $('#modalPaquete').modal('show');
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error', 'No se pudo cargar el registro', 'error');
                }
            });
        }

        // PATRÓN: Guardar (crear/actualizar)
        function save() {
            var formData = new FormData($('#formPaquete')[0]);

            $.ajax({
                url: '?handler=Save',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Éxito', response.message, 'success');
                        $('#modalPaquete').modal('hide');
                        table.ajax.reload();
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error', 'No se pudo guardar el registro', 'error');
                }
            });
        }

        // PATRÓN: Eliminar registro
        function deleteRecord(id) {
            Swal.fire({
                title: '¿Está seguro?',
                text: "Esta acción no se puede revertir",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: `?handler=Delete&id=${id}`,
                        type: 'POST',
                        headers: {
                            "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (response) {
                            if (response.success) {
                                Swal.fire('Eliminado', response.message, 'success');
                                table.ajax.reload();
                            } else {
                                Swal.fire('Error', response.message, 'error');
                            }
                        },
                        error: function () {
                            Swal.fire('Error', 'No se pudo eliminar el registro', 'error');
                        }
                    });
                }
            });
        }
    </script>
}
```

### ESTRUCTURA BÁSICA: [Entidad].cshtml.cs (PageModel)

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MJL.Shared.Entities;
using System.Text;
using System.Text.Json;

namespace MJL.FrontendAdmin.Pages
{
    [Authorize(Roles = "Admin,Employee")]
    public class PaquetesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // PATRÓN: Propiedades para SelectLists
        public IEnumerable<SelectListItem> Estados { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Usuarios { get; set; } = new List<SelectListItem>();

        // PATRÓN: BindProperty para formularios
        [BindProperty]
        public Paquete Paquete { get; set; } = new();

        public PaquetesModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // PATRÓN: OnGetAsync para cargar página inicial
        public async Task OnGetAsync()
        {
            ViewData["BreadcrumbTitle"] = "Paquetería";
            ViewData["BreadcrumbSubtitle"] = "Paquetes";
            await LoadSelectListsAsync();
        }

        // PATRÓN: Handler para DataTable (devuelve JSON)
        public async Task<IActionResult> OnGetDataAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MJLApi");

                // PATRÓN: Obtener JWT desde cookie
                if (Request.Cookies.TryGetValue("jwtAdmin", out var jwt))
                    client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);

                var response = await client.GetAsync("/api/paquete");

                if (response.IsSuccessStatusCode)
                {
                    var paquetes = await response.Content.ReadFromJsonAsync<List<Paquete>>(
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return new JsonResult(new { data = paquetes });
                }

                return new JsonResult(new { data = new List<Paquete>() });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        // PATRÓN: Handler para obtener detalles de un registro
        public async Task<IActionResult> OnGetDetailsAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MJLApi");
                if (Request.Cookies.TryGetValue("jwtAdmin", out var jwt))
                    client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);

                var response = await client.GetAsync($"/api/paquete/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var paquete = await response.Content.ReadFromJsonAsync<Paquete>(
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var result = new
                    {
                        success = true,
                        data = new
                        {
                            paquete.Id,
                            paquete.NumeroTracking,
                            paquete.Descripcion,
                            paquete.UserId,
                            paquete.EstadoId,
                            paquete.Peso,
                            paquete.Valor
                        }
                    };

                    return new JsonResult(result);
                }

                return new JsonResult(new { success = false, message = "Paquete no encontrado" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // PATRÓN: Handler para guardar (crear/actualizar)
        public async Task<IActionResult> OnPostSaveAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MJLApi");
                if (Request.Cookies.TryGetValue("jwtAdmin", out var jwt))
                    client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);

                // PATRÓN: Validar ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value!.Errors.Count > 0)
                        .Select(x => new {
                            Field = x.Key,
                            Message = x.Value!.Errors.First().ErrorMessage
                        })
                        .ToList();

                    return new JsonResult(new { success = false, message = "Datos inválidos", errors });
                }

                HttpResponseMessage response;
                var json = JsonSerializer.Serialize(Paquete);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // PATRÓN: Decidir entre POST (crear) o PUT (actualizar)
                if (Paquete.Id == 0)
                {
                    response = await client.PostAsync("/api/paquete", content);
                }
                else
                {
                    response = await client.PutAsync("/api/paquete", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    string mensaje = Paquete.Id == 0
                        ? "Paquete creado exitosamente"
                        : "Paquete actualizado exitosamente";

                    return new JsonResult(new { success = true, message = mensaje });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new JsonResult(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // PATRÓN: Handler para eliminar
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MJLApi");
                if (Request.Cookies.TryGetValue("jwtAdmin", out var jwt))
                    client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);

                var response = await client.DeleteAsync($"/api/paquete/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = true, message = "Paquete eliminado exitosamente" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Error al eliminar" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // PATRÓN: Método privado para cargar SelectLists
        private async Task LoadSelectListsAsync()
        {
            var client = _httpClientFactory.CreateClient("MJLApi");
            if (Request.Cookies.TryGetValue("jwtAdmin", out var jwt))
                client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);

            // Cargar Estados
            var estadosResponse = await client.GetAsync("/api/estados");
            if (estadosResponse.IsSuccessStatusCode)
            {
                var estadosList = await estadosResponse.Content.ReadFromJsonAsync<List<EstadoPaquete>>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Estados = estadosList!.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre
                }).ToList();
            }

            // Cargar Usuarios
            var usersResponse = await client.GetAsync("/api/Accounts/GetUsers");
            if (usersResponse.IsSuccessStatusCode)
            {
                var usersList = await usersResponse.Content.ReadFromJsonAsync<List<User>>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Usuarios = usersList!.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FullName} / {u.Email}"
                }).ToList();
            }
        }
    }
}
```

## 2. PATRÓN DE AUTENTICACIÓN

### Login.cshtml.cs

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

                    // PATRÓN: Guardar JWT en cookie HttpOnly
                    Response.Cookies.Append("jwtAdmin", tokenResponse!.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(12)
                    });

                    // PATRÓN: Crear claims desde JWT
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(tokenResponse.Token);

                    var claims = jwtToken.Claims.ToList();

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
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

    public class LoginDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class TokenDTO
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
}
```

## 3. PATRÓN DE CONFIGURACIÓN

### Program.cs (Frontend)

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// PATRÓN: Configurar Razor Pages
builder.Services.AddRazorPages();

// PATRÓN: Configurar HttpClient con nombre
builder.Services.AddHttpClient("MJLApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
    client.Timeout = TimeSpan.FromMinutes(5);
});

// PATRÓN: Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
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

## 4. COMPONENTES COMUNES

### DataTables Pattern

```javascript
// PATRÓN: Configuración estándar de DataTable
$('#tableId').DataTable({
    ajax: {
        url: '?handler=Data',
        type: 'GET',
        headers: {
            "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
        },
        dataSrc: 'data'
    },
    columns: [
        { data: 'campo1' },
        { data: 'campo2' },
        { data: 'relacion.campo' }, // Para propiedades de navegación
        {
            data: 'id',
            render: function (data, type, row) {
                return `<button onclick="edit(${data})">Editar</button>`;
            }
        }
    ],
    language: {
        url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
    },
    responsive: true,
    order: [[0, 'desc']],
    pageLength: 25
});
```

### Select2 Pattern

```javascript
// PATRÓN: Configuración estándar de Select2
$('#selectId').select2({
    theme: 'bootstrap4',
    placeholder: 'Seleccione...',
    allowClear: true,
    language: 'es'
});
```

### SweetAlert2 Pattern

```javascript
// PATRÓN: Confirmación antes de eliminar
Swal.fire({
    title: '¿Está seguro?',
    text: "Esta acción no se puede revertir",
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Sí, eliminar',
    cancelButtonText: 'Cancelar',
    confirmButtonColor: '#d33',
    cancelButtonColor: '#3085d6'
}).then((result) => {
    if (result.isConfirmed) {
        // Ejecutar eliminación
    }
});

// PATRÓN: Mensaje de éxito
Swal.fire('Éxito', 'Operación completada', 'success');

// PATRÓN: Mensaje de error
Swal.fire('Error', 'No se pudo completar la operación', 'error');
```

## 5. REGLAS OBLIGATORIAS

### Nomenclatura
1. **NUNCA** usar "Index.cshtml" para páginas de entidades
2. **SIEMPRE** usar nombre de entidad en plural: `Paquetes.cshtml`, `Usuarios.cshtml`
3. **SIEMPRE** usar PascalCase para archivos y clases

### Handlers
4. **SIEMPRE** usar sufijo "Async" en handlers: `OnPostSaveAsync()`, `OnGetDataAsync()`
5. **SIEMPRE** decorar handlers con `public async Task<IActionResult>`
6. **SIEMPRE** usar atributo `[BindProperty]` para propiedades de formulario

### HTTP Calls
7. **SIEMPRE** usar `IHttpClientFactory` en lugar de `new HttpClient()`
8. **SIEMPRE** obtener JWT desde cookie con `Request.Cookies.TryGetValue("jwtAdmin", out var jwt)`
9. **SIEMPRE** agregar header Authorization: `client.DefaultRequestHeaders.Authorization = new("Bearer", jwt)`
10. **SIEMPRE** usar `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`

### JavaScript/AJAX
11. **SIEMPRE** incluir RequestVerificationToken en AJAX calls
12. **SIEMPRE** usar `?handler=NombreHandler` para llamar handlers específicos
13. **SIEMPRE** usar SweetAlert2 para mensajes al usuario
14. **SIEMPRE** recargar DataTable después de crear/editar/eliminar: `table.ajax.reload()`

### Formularios
15. **SIEMPRE** usar modal de Bootstrap para crear/editar
16. **SIEMPRE** resetear formulario al abrir modal para crear: `$('#formId')[0].reset()`
17. **SIEMPRE** validar `ModelState.IsValid` en handlers POST

### Respuestas JSON
18. **SIEMPRE** devolver JSON con estructura: `{ success: true/false, message: "...", data: {...} }`
19. **SIEMPRE** manejar errores en bloque catch y devolver JSON con error

### Performance
20. **SIEMPRE** usar lazy loading cuando sea posible (cargar datos bajo demanda)
21. **NUNCA** cargar datos pesados en OnGetAsync, usar handlers específicos
