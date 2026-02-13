# Sistema de Notificaciones - Guía de Implementación

## Descripción General

El sistema de notificaciones permite enviar alertas en tiempo real a los usuarios dentro de la aplicación. Las notificaciones aparecen en el icono de campana en la barra de navegación y se actualizan automáticamente cada 30 segundos.

## Características Implementadas

### Frontend
- **Icono de campana** con badge que muestra el número de notificaciones no leídas
- **Dropdown** con lista de notificaciones recientes (últimas 10)
- **Actualización automática** cada 30 segundos
- **Indicador visual** para notificaciones no leídas (fondo azul claro + punto azul)
- **Click en notificación** marca como leída y navega a la página relevante
- **Botón "Marcar todas"** para marcar todas las notificaciones como leídas
- **Estilos diferenciados** por tipo de notificación (success, warning, danger, info, secondary)

### Backend
- **API REST completa** con endpoints para crear, leer, marcar como leída y eliminar notificaciones
- **Filtrado por empresa** (multi-tenant)
- **Seguridad JWT** con verificación de acceso por empresa
- **20 tipos de notificaciones** predefinidas con iconos y colores automáticos
- **Helper class** para crear notificaciones fácilmente desde cualquier módulo

## Tipos de Notificaciones Disponibles

| Tipo | Icono | Color | Uso |
|------|-------|-------|-----|
| DocumentoAceptado | fa-check-circle | success | Documento aceptado por Hacienda |
| DocumentoRechazado | fa-times-circle | danger | Documento rechazado por Hacienda |
| DocumentoPendiente | fa-clock | warning | Documento pendiente de envío |
| PagoRecibido | fa-money-bill-wave | success | Pago recibido de cliente |
| PagoPendiente | fa-exclamation-circle | warning | Pago pendiente de recibir |
| InventarioBajo | fa-boxes | danger | Stock bajo en inventario |
| GastoAprobado | fa-check-double | success | Gasto aprobado |
| GastoRechazado | fa-ban | danger | Gasto rechazado |
| GastoRequiereAprobacion | fa-clipboard-check | info | Gasto requiere aprobación |
| NuevoUsuario | fa-user-plus | info | Nuevo usuario registrado |
| Sistema | fa-cog | secondary | Notificación del sistema |
| Advertencia | fa-exclamation-triangle | warning | Advertencia general |
| Error | fa-exclamation-circle | danger | Error del sistema |
| CertificadoPorVencer | fa-certificate | warning | Certificado próximo a vencer |
| CertificadoVencido | fa-certificate | danger | Certificado vencido |
| ErrorEnvioHacienda | fa-times-circle | danger | Error en envío a Hacienda |
| DocumentoContingencia | fa-file-medical | warning | Documento en contingencia |
| ConsecutivoPorAgotar | fa-sort-numeric-up | warning | Consecutivo próximo a agotarse |
| ActualizacionDisponible | fa-download | info | Nueva versión disponible |
| REPPendiente | fa-file-invoice-dollar | warning | REP pendiente de emitir |

## Cómo Crear Notificaciones

### Método 1: Usar NotificacionHelper (Recomendado)

```csharp
using Facturacion.Backend.Helpers;

// Inyectar INotificacionUnitOfWork en el constructor
private readonly INotificacionUnitOfWork _notificacionUnitOfWork;

// Ejemplo: Notificar documento aceptado
await NotificacionHelper.CrearNotificacionDocumentoAceptado(
    _notificacionUnitOfWork,
    empresaId: documento.EmpresaId,
    usuarioId: documento.UsuarioCreacionId,
    numeroConsecutivo: documento.NumeroConsecutivo,
    documentoId: documento.Id
);

// Ejemplo: Notificar inventario bajo
await NotificacionHelper.CrearNotificacionInventarioBajo(
    _notificacionUnitOfWork,
    empresaId: producto.EmpresaId,
    usuarioId: adminUserId,
    nombreProducto: producto.Nombre,
    cantidadActual: inventario.CantidadDisponible,
    stockMinimo: producto.StockMinimo,
    inventarioId: inventario.Id
);

// Ejemplo: Notificar certificado próximo a vencer
await NotificacionHelper.CrearNotificacionCertificadoPorVencer(
    _notificacionUnitOfWork,
    empresaId: empresa.Id,
    usuarioId: adminUserId,
    nombreEmpresa: empresa.NombreComercial,
    fechaVencimiento: empresa.FechaVencimientoCertificado,
    diasRestantes: (empresa.FechaVencimientoCertificado - DateTime.Now).Days
);
```

### Método 2: Usar DTO directamente

```csharp
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Enums;

var notificacion = new CrearNotificacionDTO
{
    EmpresaId = empresaId,
    UsuarioId = usuarioId,
    TipoNotificacion = TipoNotificacion.DocumentoAceptado,
    Titulo = "Documento Aceptado",
    Mensaje = $"El documento {numeroConsecutivo} fue aceptado por Hacienda",
    Icono = "fa-check-circle",        // Opcional (asignado automáticamente)
    Color = "success",                // Opcional (asignado automáticamente)
    EntidadRelacionadaId = documentoId,
    TipoEntidad = "Documento",
    UrlAccion = $"/DocumentosElectronicos/Documentos?id={documentoId}",
    Importante = false,
    FechaExpiracion = DateTime.Now.AddDays(30) // Opcional
};

await _notificacionUnitOfWork.CreateAsync(notificacion);
```

### Método 3: Notificaciones masivas para múltiples usuarios

```csharp
// Obtener todos los administradores de la empresa
var adminIds = await _context.Users
    .Where(u => u.UsuariosEmpresas.Any(ue => ue.EmpresaId == empresaId))
    .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Administrador"))
    .Select(u => u.Id)
    .ToListAsync();

await NotificacionHelper.CrearNotificacionParaMultiplesUsuarios(
    _notificacionUnitOfWork,
    empresaId: empresaId,
    usuarioIds: adminIds,
    tipo: TipoNotificacion.Advertencia,
    titulo: "Mantenimiento Programado",
    mensaje: "El sistema tendrá mantenimiento el próximo domingo de 2:00 AM a 6:00 AM",
    urlAccion: null,
    importante: true
);
```

## Integración en Módulos Existentes

### Ejemplo: DocumentoHaciendaService

```csharp
public class DocumentoHaciendaService
{
    private readonly IDocumentoUnitOfWork _documentoUnitOfWork;
    private readonly INotificacionUnitOfWork _notificacionUnitOfWork;

    public async Task<ActionResponse<bool>> EnviarDocumentoHacienda(Guid documentoId)
    {
        var documento = await _documentoUnitOfWork.GetByIdAsync(documentoId);

        if (!documento.WasSuccess)
            return new ActionResponse<bool> { WasSuccess = false, Message = "Documento no encontrado" };

        try
        {
            // Enviar a Hacienda
            var resultado = await EnviarAHacienda(documento.Result);

            if (resultado.Aceptado)
            {
                // Crear notificación de éxito
                await NotificacionHelper.CrearNotificacionDocumentoAceptado(
                    _notificacionUnitOfWork,
                    documento.Result.EmpresaId,
                    documento.Result.UsuarioCreacionId,
                    documento.Result.NumeroConsecutivo,
                    documento.Result.Id
                );
            }
            else
            {
                // Crear notificación de rechazo
                await NotificacionHelper.CrearNotificacionDocumentoRechazado(
                    _notificacionUnitOfWork,
                    documento.Result.EmpresaId,
                    documento.Result.UsuarioCreacionId,
                    documento.Result.NumeroConsecutivo,
                    resultado.MensajeRechazo,
                    documento.Result.Id
                );
            }

            return new ActionResponse<bool> { WasSuccess = true };
        }
        catch (Exception ex)
        {
            // Crear notificación de error
            await NotificacionHelper.CrearNotificacionErrorEnvioHacienda(
                _notificacionUnitOfWork,
                documento.Result.EmpresaId,
                documento.Result.UsuarioCreacionId,
                documento.Result.NumeroConsecutivo,
                ex.Message,
                documento.Result.Id
            );

            return new ActionResponse<bool> { WasSuccess = false, Message = ex.Message };
        }
    }
}
```

### Ejemplo: InventarioRepository

```csharp
public async Task<ActionResponse<bool>> ActualizarInventario(Guid inventarioId, decimal cantidad)
{
    var inventario = await _context.Inventarios
        .Include(i => i.Producto)
        .FirstOrDefaultAsync(i => i.Id == inventarioId);

    if (inventario == null)
        return new ActionResponse<bool> { WasSuccess = false, Message = "Inventario no encontrado" };

    inventario.CantidadDisponible -= cantidad;

    // Verificar stock mínimo
    if (inventario.CantidadDisponible < inventario.Producto.StockMinimo)
    {
        // Obtener usuario administrador de la empresa
        var adminUser = await _context.Users
            .Where(u => u.UsuariosEmpresas.Any(ue => ue.EmpresaId == inventario.Producto.EmpresaId))
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Administrador"))
            .FirstOrDefaultAsync();

        if (adminUser != null)
        {
            await NotificacionHelper.CrearNotificacionInventarioBajo(
                _notificacionUnitOfWork,
                inventario.Producto.EmpresaId,
                adminUser.Id,
                inventario.Producto.Nombre,
                inventario.CantidadDisponible,
                inventario.Producto.StockMinimo,
                inventario.Id
            );
        }
    }

    await _context.SaveChangesAsync();
    return new ActionResponse<bool> { WasSuccess = true };
}
```

## Background Jobs para Notificaciones Periódicas

### Verificar certificados próximos a vencer (diariamente)

```csharp
public class CertificadoVerificationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CertificadoVerificationJob> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var notificacionUnitOfWork = scope.ServiceProvider.GetRequiredService<INotificacionUnitOfWork>();

                var empresas = await context.Empresas
                    .Where(e => e.Activa && e.FechaVencimientoCertificado.HasValue)
                    .ToListAsync(stoppingToken);

                foreach (var empresa in empresas)
                {
                    var diasRestantes = (empresa.FechaVencimientoCertificado.Value - DateTime.Now).Days;

                    // Notificar si quedan 30, 15 o 7 días
                    if (diasRestantes == 30 || diasRestantes == 15 || diasRestantes == 7)
                    {
                        var adminUsers = await context.Users
                            .Where(u => u.UsuariosEmpresas.Any(ue => ue.EmpresaId == empresa.Id))
                            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Administrador"))
                            .Select(u => u.Id)
                            .ToListAsync(stoppingToken);

                        foreach (var userId in adminUsers)
                        {
                            await NotificacionHelper.CrearNotificacionCertificadoPorVencer(
                                notificacionUnitOfWork,
                                empresa.Id,
                                userId,
                                empresa.NombreComercial,
                                empresa.FechaVencimientoCertificado.Value,
                                diasRestantes
                            );
                        }
                    }
                    else if (diasRestantes < 0)
                    {
                        // Certificado vencido
                        var adminUsers = await context.Users
                            .Where(u => u.UsuariosEmpresas.Any(ue => ue.EmpresaId == empresa.Id))
                            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Administrador"))
                            .Select(u => u.Id)
                            .ToListAsync(stoppingToken);

                        foreach (var userId in adminUsers)
                        {
                            await NotificacionHelper.CrearNotificacionCertificadoVencido(
                                notificacionUnitOfWork,
                                empresa.Id,
                                userId,
                                empresa.NombreComercial,
                                empresa.FechaVencimientoCertificado.Value
                            );
                        }
                    }
                }
            }

            // Ejecutar cada 24 horas
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

### Limpiar notificaciones expiradas (semanalmente)

```csharp
public class NotificacionCleanupJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                // Eliminar notificaciones expiradas
                var notificacionesExpiradas = await context.Notificaciones
                    .Where(n => n.FechaExpiracion.HasValue && n.FechaExpiracion.Value < DateTime.Now)
                    .ToListAsync(stoppingToken);

                context.Notificaciones.RemoveRange(notificacionesExpiradas);
                await context.SaveChangesAsync(stoppingToken);

                // Eliminar notificaciones leídas con más de 30 días
                var fechaLimite = DateTime.Now.AddDays(-30);
                var notificacionesAntiguas = await context.Notificaciones
                    .Where(n => n.Leida && n.FechaLeida.HasValue && n.FechaLeida.Value < fechaLimite)
                    .ToListAsync(stoppingToken);

                context.Notificaciones.RemoveRange(notificacionesAntiguas);
                await context.SaveChangesAsync(stoppingToken);
            }

            // Ejecutar cada 7 días
            await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
        }
    }
}
```

## API Endpoints Disponibles

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/Notificaciones/usuario/{empresaId}` | Obtener todas las notificaciones del usuario |
| GET | `/api/Notificaciones/no-leidas/{empresaId}` | Obtener notificaciones no leídas |
| GET | `/api/Notificaciones/count-no-leidas/{empresaId}` | Obtener conteo de no leídas |
| GET | `/api/Notificaciones/resumen/{empresaId}` | Obtener resumen con estadísticas |
| GET | `/api/Notificaciones/{id}` | Obtener notificación por ID |
| POST | `/api/Notificaciones` | Crear nueva notificación |
| PUT | `/api/Notificaciones/marcar-leida/{id}` | Marcar como leída |
| PUT | `/api/Notificaciones/marcar-todas-leidas/{empresaId}` | Marcar todas como leídas |
| DELETE | `/api/Notificaciones/{id}` | Eliminar notificación |
| DELETE | `/api/Notificaciones/expiradas/{empresaId}` | Eliminar notificaciones expiradas |

## Frontend - JavaScript

El sistema de notificaciones se auto-inicializa al cargar la página. El archivo `/wwwroot/js/notifications.js` contiene toda la lógica:

```javascript
// Auto-inicializa al cargar la página
// No requiere código adicional en las páginas

// Funciones disponibles globalmente:
NotificationSystem.refresh();           // Recargar notificaciones manualmente
NotificationSystem.markAllAsRead();     // Marcar todas como leídas
NotificationSystem.setEmpresa(id);      // Cambiar empresa
NotificationSystem.stopAutoRefresh();   // Detener actualización automática
```

## Personalización

### Cambiar intervalo de actualización

Editar `/wwwroot/js/notifications.js`:

```javascript
const config = {
    refreshInterval: 30000,  // Cambiar a 60000 para 1 minuto
    maxNotificationsShown: 10,
    apiBaseUrl: '/api/Notificaciones'
};
```

### Agregar nuevos tipos de notificaciones

1. Agregar enum en `Facturacion.Shared/Enums/TipoNotificacion.cs`
2. Actualizar `NotificacionService.cs` para asignar icono y color automáticamente
3. Crear método helper en `NotificacionHelper.cs` (opcional)

## Mejores Prácticas

1. **Usa NotificacionHelper**: Simplifica el código y asegura consistencia
2. **Define fecha de expiración**: Para notificaciones temporales o que pierden relevancia
3. **Marca como importante solo lo crítico**: No abusar de las notificaciones importantes
4. **Incluye URL de acción siempre que sea posible**: Mejora la experiencia del usuario
5. **Vincula a la entidad relacionada**: Permite rastrear el origen de la notificación
6. **Notifica solo a usuarios relevantes**: Evita spam de notificaciones
7. **Limpia periódicamente**: Implementa jobs para eliminar notificaciones antiguas

## Troubleshooting

### Las notificaciones no aparecen

1. Verificar que el usuario esté autenticado y tenga JWT válido
2. Verificar que el usuario tenga acceso a la empresa (tabla `UsuariosEmpresas`)
3. Revisar la consola del navegador para errores de JavaScript
4. Verificar que el endpoint `/api/Notificaciones/resumen/{empresaId}` responda correctamente

### El badge no se actualiza

1. Verificar que `NotificationSystem` se esté inicializando correctamente
2. Revisar que el `data-empresa-id` esté presente en el `companySelector`
3. Verificar que no haya errores en la consola del navegador

### Las notificaciones se duplican

1. Asegurarse de que `NotificationSystem.init()` se llame solo una vez
2. Verificar que no haya múltiples instancias del script `notifications.js`

### Error 403 Forbidden

1. El usuario no tiene acceso a la empresa especificada
2. Verificar relación en tabla `UsuariosEmpresas`
3. Verificar que el JWT sea válido y contenga los claims correctos

## Ejemplo Completo: Módulo de Gastos

```csharp
public class GastoService : IGastoService
{
    private readonly IGastoUnitOfWork _gastoUnitOfWork;
    private readonly INotificacionUnitOfWork _notificacionUnitOfWork;
    private readonly DataContext _context;

    public async Task<ActionResponse<bool>> AprobarGasto(Guid gastoId, string aprobadorId)
    {
        var gasto = await _gastoUnitOfWork.GetByIdAsync(gastoId);

        if (!gasto.WasSuccess)
            return new ActionResponse<bool> { WasSuccess = false, Message = "Gasto no encontrado" };

        // Aprobar gasto
        gasto.Result.Aprobado = true;
        gasto.Result.AprobadorId = aprobadorId;
        gasto.Result.FechaAprobacion = DateTime.Now;

        await _gastoUnitOfWork.UpdateAsync(gasto.Result);

        // Crear notificación para el creador del gasto
        var notificacion = new CrearNotificacionDTO
        {
            EmpresaId = gasto.Result.EmpresaId,
            UsuarioId = gasto.Result.UsuarioCreacionId,
            TipoNotificacion = TipoNotificacion.GastoAprobado,
            Titulo = "Gasto Aprobado",
            Mensaje = $"Su gasto de {gasto.Result.Monto:C2} ha sido aprobado",
            EntidadRelacionadaId = gasto.Result.Id,
            TipoEntidad = "Gasto",
            UrlAccion = $"/Gastos/Gasto?id={gasto.Result.Id}",
            Importante = false
        };

        await _notificacionUnitOfWork.CreateAsync(notificacion);

        return new ActionResponse<bool> { WasSuccess = true };
    }

    public async Task<ActionResponse<bool>> RechazarGasto(Guid gastoId, string aprobadorId, string motivo)
    {
        var gasto = await _gastoUnitOfWork.GetByIdAsync(gastoId);

        if (!gasto.WasSuccess)
            return new ActionResponse<bool> { WasSuccess = false, Message = "Gasto no encontrado" };

        // Rechazar gasto
        gasto.Result.Aprobado = false;
        gasto.Result.AprobadorId = aprobadorId;
        gasto.Result.FechaAprobacion = DateTime.Now;
        gasto.Result.MotivoRechazo = motivo;

        await _gastoUnitOfWork.UpdateAsync(gasto.Result);

        // Crear notificación para el creador del gasto
        var notificacion = new CrearNotificacionDTO
        {
            EmpresaId = gasto.Result.EmpresaId,
            UsuarioId = gasto.Result.UsuarioCreacionId,
            TipoNotificacion = TipoNotificacion.GastoRechazado,
            Titulo = "Gasto Rechazado",
            Mensaje = $"Su gasto de {gasto.Result.Monto:C2} fue rechazado: {motivo}",
            EntidadRelacionadaId = gasto.Result.Id,
            TipoEntidad = "Gasto",
            UrlAccion = $"/Gastos/Gasto?id={gasto.Result.Id}",
            Importante = true
        };

        await _notificacionUnitOfWork.CreateAsync(notificacion);

        return new ActionResponse<bool> { WasSuccess = true };
    }
}
```

## Referencias

- Entidad: `Facturacion.Shared/Entities/Notificacion.cs`
- DTOs: `Facturacion.Shared/DTOs/NotificacionDTO.cs`, `CrearNotificacionDTO.cs`, `ResumenNotificacionesDTO.cs`
- Enums: `Facturacion.Shared/Enums/TipoNotificacion.cs`
- Controller: `Facturacion.Backend/Controllers/NotificacionesController.cs`
- Repository: `Facturacion.Backend/Repositories/Implementations/NotificacionRepository.cs`
- UnitOfWork: `Facturacion.Backend/UnitsOfWork/Implementations/NotificacionUnitOfWork.cs`
- Service: `Facturacion.Backend/Services/Implementations/NotificacionService.cs`
- Helper: `Facturacion.Backend/Helpers/NotificacionHelper.cs`
- Frontend JS: `Facturacion.Frontend/wwwroot/js/notifications.js`
- Frontend CSS: `Facturacion.Frontend/wwwroot/css/modern-theme.css`
- Layout: `Facturacion.Frontend/Pages/Shared/_Layout.cshtml`
