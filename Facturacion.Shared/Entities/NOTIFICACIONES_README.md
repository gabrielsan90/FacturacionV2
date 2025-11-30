# Sistema de Notificaciones In-App

## Descripción General

El sistema de notificaciones in-app permite enviar alertas y mensajes a los usuarios dentro de la aplicación. Las notificaciones aparecen en el icono de campana con badge y pueden ser consultadas, marcadas como leídas, y eliminadas.

## Características Principales

- **Multi-tenant**: Cada notificación está asociada a una empresa específica
- **Seguridad**: Autenticación JWT y verificación de acceso por empresa
- **Tipos de Notificación**: 13 tipos predefinidos con iconos y colores automáticos
- **Gestión de Estado**: Leídas/No leídas, importantes, expiradas
- **Deep Links**: URLs para navegación directa desde la notificación
- **Tiempo Transcurrido**: Cálculo automático ("Hace 5 minutos", "Hace 2 horas", etc.)
- **Entidades Relacionadas**: Vinculación con documentos, gastos, inventarios, etc.

## Tipos de Notificación

```csharp
public enum TipoNotificacion
{
    DocumentoAceptado = 1,        // fa-check-circle, success
    DocumentoRechazado = 2,       // fa-times-circle, danger
    DocumentoPendiente = 3,       // fa-clock, warning
    PagoRecibido = 4,             // fa-money-bill-wave, success
    PagoPendiente = 5,            // fa-exclamation-circle, warning
    InventarioBajo = 6,           // fa-boxes, danger
    GastoAprobado = 7,            // fa-check-double, success
    GastoRechazado = 8,           // fa-ban, danger
    GastoRequiereAprobacion = 9,  // fa-clipboard-check, info
    NuevoUsuario = 10,            // fa-user-plus, info
    Sistema = 11,                 // fa-cog, secondary
    Advertencia = 12,             // fa-exclamation-triangle, warning
    Error = 13                    // fa-exclamation-circle, danger
}
```

## API Endpoints

### GET /api/Notificaciones/usuario/{empresaId}
Obtiene todas las notificaciones del usuario para una empresa.

**Respuesta**: `ActionResponse<IEnumerable<NotificacionDTO>>`

### GET /api/Notificaciones/no-leidas/{empresaId}
Obtiene las notificaciones no leídas del usuario.

**Respuesta**: `ActionResponse<IEnumerable<NotificacionDTO>>`

### GET /api/Notificaciones/count-no-leidas/{empresaId}
Obtiene el conteo de notificaciones no leídas (para el badge).

**Respuesta**: `ActionResponse<int>`

### GET /api/Notificaciones/resumen/{empresaId}
Obtiene un resumen con estadísticas y notificaciones recientes.

**Respuesta**: `ActionResponse<ResumenNotificacionesDTO>`

```csharp
public class ResumenNotificacionesDTO
{
    public int TotalNoLeidas { get; set; }
    public int TotalImportantes { get; set; }
    public int TotalHoy { get; set; }
    public int TotalSemana { get; set; }
    public List<NotificacionDTO> NotificacionesRecientes { get; set; }
}
```

### GET /api/Notificaciones/{id}
Obtiene una notificación específica por ID.

**Respuesta**: `ActionResponse<NotificacionDTO>`

### POST /api/Notificaciones
Crea una nueva notificación.

**Body**: `CrearNotificacionDTO`

```csharp
{
    "empresaId": "guid",
    "usuarioId": "string",
    "tipoNotificacion": 1,
    "titulo": "Documento Aceptado",
    "mensaje": "El documento FE-001-001-000000123 fue aceptado por Hacienda",
    "icono": "fa-check-circle",        // Opcional (asignado automáticamente)
    "color": "success",                // Opcional (asignado automáticamente)
    "entidadRelacionadaId": "guid",    // Opcional
    "tipoEntidad": "Documento",        // Opcional
    "urlAccion": "/documentos/123",    // Opcional
    "importante": false,               // Opcional
    "fechaExpiracion": "2024-12-31"    // Opcional
}
```

### PUT /api/Notificaciones/marcar-leida/{id}
Marca una notificación como leída.

**Respuesta**: `ActionResponse<bool>`

### PUT /api/Notificaciones/marcar-todas-leidas/{empresaId}
Marca todas las notificaciones del usuario como leídas.

**Respuesta**: `ActionResponse<bool>`

### DELETE /api/Notificaciones/{id}
Elimina una notificación específica.

**Respuesta**: `ActionResponse<bool>`

### DELETE /api/Notificaciones/expiradas/{empresaId}
Elimina todas las notificaciones expiradas de una empresa.

**Respuesta**: `ActionResponse<bool>`

## Ejemplo de Uso

### Crear notificación cuando un documento es aceptado

```csharp
// En DocumentoHaciendaService.cs
var notificacion = new CrearNotificacionDTO
{
    EmpresaId = empresaId,
    UsuarioId = documento.UsuarioCreacionId,
    TipoNotificacion = TipoNotificacion.DocumentoAceptado,
    Titulo = "Documento Aceptado",
    Mensaje = $"El documento {documento.NumeroConsecutivo} fue aceptado por Hacienda",
    EntidadRelacionadaId = documento.Id,
    TipoEntidad = "Documento",
    UrlAccion = $"/documentos/{documento.Id}",
    Importante = false
};

await _notificacionUnitOfWork.CreateAsync(notificacion);
```

### Crear notificación de inventario bajo

```csharp
// En InventarioRepository.cs
if (inventario.CantidadDisponible < producto.StockMinimo)
{
    var notificacion = new CrearNotificacionDTO
    {
        EmpresaId = inventario.Producto!.EmpresaId,
        UsuarioId = adminUserId, // Usuario administrador
        TipoNotificacion = TipoNotificacion.InventarioBajo,
        Titulo = "Stock Bajo",
        Mensaje = $"El producto {inventario.Producto.Nombre} tiene stock bajo en {sucursal.Nombre}",
        EntidadRelacionadaId = inventario.Id,
        TipoEntidad = "Inventario",
        UrlAccion = $"/inventarios/{inventario.Id}",
        Importante = true
    };

    await _notificacionUnitOfWork.CreateAsync(notificacion);
}
```

### Frontend - Blazor Component

```razor
@inject INotificacionUnitOfWork NotificacionUnitOfWork
@inject NavigationManager NavigationManager

<div class="notification-bell" @onclick="ToggleNotifications">
    <i class="fas fa-bell"></i>
    @if (countNoLeidas > 0)
    {
        <span class="badge badge-danger">@countNoLeidas</span>
    }
</div>

@if (showNotifications)
{
    <div class="notification-dropdown">
        <div class="notification-header">
            <h5>Notificaciones</h5>
            @if (countNoLeidas > 0)
            {
                <button @onclick="MarcarTodasLeidas">Marcar todas como leídas</button>
            }
        </div>
        <div class="notification-list">
            @foreach (var notif in notificaciones)
            {
                <div class="notification-item @(notif.Leida ? "read" : "unread") @(notif.Importante ? "important" : "")"
                     @onclick="() => OnNotificationClick(notif)">
                    <i class="@notif.Icono text-@notif.Color"></i>
                    <div class="notification-content">
                        <strong>@notif.Titulo</strong>
                        <p>@notif.Mensaje</p>
                        <small class="text-muted">@notif.TiempoTranscurrido</small>
                    </div>
                </div>
            }
        </div>
    </div>
}

@code {
    private List<NotificacionDTO> notificaciones = new();
    private int countNoLeidas = 0;
    private bool showNotifications = false;

    protected override async Task OnInitializedAsync()
    {
        await CargarNotificaciones();

        // Actualizar cada 30 segundos
        var timer = new System.Threading.Timer(async _ =>
        {
            await CargarNotificaciones();
            await InvokeAsync(StateHasChanged);
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private async Task CargarNotificaciones()
    {
        var empresaId = GetCurrentEmpresaId(); // Obtener empresa actual

        var actionCount = await NotificacionUnitOfWork.GetCountNoLeidasAsync(User.Id, empresaId);
        if (actionCount.WasSuccess)
        {
            countNoLeidas = actionCount.Result;
        }

        var action = await NotificacionUnitOfWork.GetByUsuarioAsync(User.Id, empresaId);
        if (action.WasSuccess)
        {
            notificaciones = action.Result.ToList();
        }
    }

    private async Task OnNotificationClick(NotificacionDTO notif)
    {
        if (!notif.Leida)
        {
            await NotificacionUnitOfWork.MarcarComoLeidaAsync(notif.Id);
            await CargarNotificaciones();
        }

        if (!string.IsNullOrEmpty(notif.UrlAccion))
        {
            NavigationManager.NavigateTo(notif.UrlAccion);
        }

        showNotifications = false;
    }

    private async Task MarcarTodasLeidas()
    {
        var empresaId = GetCurrentEmpresaId();
        await NotificacionUnitOfWork.MarcarTodasComoLeidasAsync(User.Id, empresaId);
        await CargarNotificaciones();
    }

    private void ToggleNotifications()
    {
        showNotifications = !showNotifications;
    }
}
```

## Índices de Base de Datos

El sistema incluye los siguientes índices para optimizar el rendimiento:

1. `IX_Notificacion_EmpresaId` - Consultas por empresa
2. `IX_Notificacion_UsuarioId` - Consultas por usuario
3. `IX_Notificacion_Usuario_Empresa_Leida` - Consultas de no leídas (compuesto)
4. `IX_Notificacion_FechaCreacion` - Ordenamiento cronológico
5. `IX_Notificacion_FechaExpiracion` - Limpieza de notificaciones expiradas
6. `IX_Notificacion_Tipo_Leida` - Filtrado por tipo y estado

## Limpieza de Notificaciones Expiradas

Se recomienda ejecutar periódicamente el endpoint de limpieza de notificaciones expiradas:

```csharp
// En un servicio programado (background service)
await _notificacionUnitOfWork.DeleteExpiradasAsync(empresaId);
```

## Mejores Prácticas

1. **Usa tipos específicos**: Aprovecha los 13 tipos predefinidos para mejor organización
2. **Marca como importante**: Solo para notificaciones críticas que requieren atención inmediata
3. **Incluye deep links**: Siempre que sea posible, incluye la URL de acción para mejor UX
4. **Vincula entidades**: Usa EntidadRelacionadaId y TipoEntidad para rastrear el origen
5. **Establece expiración**: Para notificaciones temporales, usa FechaExpiracion
6. **Limpieza periódica**: Implementa un job para eliminar notificaciones antiguas

## Integración con Otros Módulos

El sistema de notificaciones puede integrarse fácilmente con:

- **Documentos**: Notificar sobre aceptación/rechazo de Hacienda
- **Gastos**: Notificar aprobaciones/rechazos de gastos
- **Inventarios**: Alertar sobre stock bajo
- **Pagos**: Notificar recepción de pagos
- **Usuarios**: Dar bienvenida a nuevos usuarios
- **Sistema**: Alertas de mantenimiento o actualizaciones

## Troubleshooting

### Las notificaciones no aparecen
- Verificar que el usuario tiene acceso a la empresa (tabla UsuariosEmpresas)
- Verificar que las notificaciones no hayan expirado
- Verificar que la autenticación JWT es válida

### El contador no se actualiza
- Asegurarse de llamar a GetCountNoLeidasAsync después de marcar como leída
- Verificar que el componente se está refrescando correctamente

### Error 403 Forbidden
- El usuario no tiene acceso a la empresa especificada
- Verificar relación en tabla UsuariosEmpresas
