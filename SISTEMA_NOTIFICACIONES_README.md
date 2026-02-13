# Sistema de Notificaciones - Resumen de Implementación

## Estado: Implementado y Funcional

El sistema de notificaciones está completamente implementado y listo para usar. Incluye backend API, frontend interactivo, y helpers para facilitar la creación de notificaciones desde cualquier módulo.

## Componentes Implementados

### Backend (API)

**Entidades y DTOs:**
- `Notificacion.cs` - Entidad principal con 20+ propiedades
- `NotificacionDTO.cs` - DTO para transferencia de datos
- `CrearNotificacionDTO.cs` - DTO para crear notificaciones
- `ResumenNotificacionesDTO.cs` - DTO con estadísticas
- `TipoNotificacion.cs` - Enum con 20 tipos predefinidos

**Capa de Datos:**
- `NotificacionRepository.cs` - Repositorio con queries optimizadas
- `NotificacionUnitOfWork.cs` - Unit of Work para transacciones
- `NotificacionService.cs` - Lógica de negocio (asignación automática de iconos y colores)

**API:**
- `NotificacionesController.cs` - 10 endpoints RESTful
  - GET `/api/Notificaciones/usuario/{empresaId}` - Todas las notificaciones
  - GET `/api/Notificaciones/no-leidas/{empresaId}` - No leídas
  - GET `/api/Notificaciones/count-no-leidas/{empresaId}` - Contador
  - GET `/api/Notificaciones/resumen/{empresaId}` - Resumen con estadísticas
  - POST `/api/Notificaciones` - Crear
  - PUT `/api/Notificaciones/marcar-leida/{id}` - Marcar como leída
  - PUT `/api/Notificaciones/marcar-todas-leidas/{empresaId}` - Marcar todas
  - DELETE `/api/Notificaciones/{id}` - Eliminar
  - DELETE `/api/Notificaciones/expiradas/{empresaId}` - Limpiar expiradas

**Helpers:**
- `NotificacionHelper.cs` - Métodos estáticos para crear notificaciones comunes
  - CrearNotificacionDocumentoAceptado
  - CrearNotificacionDocumentoRechazado
  - CrearNotificacionInventarioBajo
  - CrearNotificacionCertificadoPorVencer
  - CrearNotificacionErrorEnvioHacienda
  - Y más...

### Frontend (UI)

**Layout:**
- Icono de campana en barra de navegación
- Badge con número de notificaciones no leídas
- Dropdown con lista de notificaciones (últimas 10)
- Botón "Marcar todas como leídas"
- Link a página de todas las notificaciones

**JavaScript:**
- `notifications.js` - Sistema completo de notificaciones
  - Auto-inicialización al cargar la página
  - Actualización automática cada 30 segundos
  - Comunicación con API vía fetch
  - Manejo de eventos (click, marcar como leída)
  - Navegación a páginas relevantes

**CSS:**
- `modern-theme.css` - Estilos para notificaciones
  - `.notification-item` - Item de notificación
  - `.notification-icon` - Icono circular con colores por tipo
  - `.notification-item.unread` - Indicador visual de no leída
  - `.unread-indicator` - Punto azul para notificaciones no leídas

## Tipos de Notificaciones

| ID | Tipo | Icono | Color | Uso |
|----|------|-------|-------|-----|
| 1 | DocumentoAceptado | fa-check-circle | success | Hacienda aceptó documento |
| 2 | DocumentoRechazado | fa-times-circle | danger | Hacienda rechazó documento |
| 3 | DocumentoPendiente | fa-clock | warning | Documento pendiente de envío |
| 4 | PagoRecibido | fa-money-bill-wave | success | Pago recibido |
| 5 | PagoPendiente | fa-exclamation-circle | warning | Pago pendiente |
| 6 | InventarioBajo | fa-boxes | danger | Stock bajo |
| 7 | GastoAprobado | fa-check-double | success | Gasto aprobado |
| 8 | GastoRechazado | fa-ban | danger | Gasto rechazado |
| 9 | GastoRequiereAprobacion | fa-clipboard-check | info | Gasto requiere aprobación |
| 10 | NuevoUsuario | fa-user-plus | info | Nuevo usuario |
| 11 | Sistema | fa-cog | secondary | Notificación del sistema |
| 12 | Advertencia | fa-exclamation-triangle | warning | Advertencia general |
| 13 | Error | fa-exclamation-circle | danger | Error del sistema |
| 14 | CertificadoPorVencer | fa-certificate | warning | Certificado próximo a vencer |
| 15 | CertificadoVencido | fa-certificate | danger | Certificado vencido |
| 16 | ErrorEnvioHacienda | fa-times-circle | danger | Error enviando a Hacienda |
| 17 | DocumentoContingencia | fa-file-medical | warning | Documento en contingencia |
| 18 | ConsecutivoPorAgotar | fa-sort-numeric-up | warning | Consecutivo próximo a agotarse |
| 19 | ActualizacionDisponible | fa-download | info | Nueva versión disponible |
| 20 | REPPendiente | fa-file-invoice-dollar | warning | REP pendiente de emitir |

## Características

### Seguridad
- Autenticación JWT requerida
- Filtrado por empresa (multi-tenant)
- Verificación de acceso vía tabla `UsuariosEmpresas`
- Solo el usuario destinatario puede ver/modificar sus notificaciones

### Performance
- Índices en base de datos para queries optimizadas
- Carga solo las últimas 10 notificaciones en el dropdown
- Endpoint de resumen optimizado (1 sola query)
- Auto-refresh configurable (default: 30 segundos)

### UX
- Indicador visual para notificaciones no leídas
- Click en notificación marca como leída automáticamente
- Navegación directa a la página relevante
- Tiempo transcurrido humanizado ("Hace 5 minutos")
- Badge desaparece cuando no hay notificaciones

## Cómo Usar

### Crear una notificación simple

```csharp
// Inyectar INotificacionUnitOfWork
private readonly INotificacionUnitOfWork _notificacionUnitOfWork;

// Usar el helper
await NotificacionHelper.CrearNotificacionDocumentoAceptado(
    _notificacionUnitOfWork,
    empresaId: documento.EmpresaId,
    usuarioId: documento.UsuarioCreacionId,
    numeroConsecutivo: documento.NumeroConsecutivo,
    documentoId: documento.Id
);
```

### Crear una notificación personalizada

```csharp
var notificacion = new CrearNotificacionDTO
{
    EmpresaId = empresaId,
    UsuarioId = usuarioId,
    TipoNotificacion = TipoNotificacion.Sistema,
    Titulo = "Título de la notificación",
    Mensaje = "Mensaje detallado",
    UrlAccion = "/Ruta/Pagina",
    Importante = true,
    FechaExpiracion = DateTime.Now.AddDays(7)
};

await _notificacionUnitOfWork.CreateAsync(notificacion);
```

## Integración en Módulos Existentes

### Sugerencias de integración:

1. **DocumentosElectronicos**:
   - Notificar cuando Hacienda acepta/rechaza un documento
   - Alertar sobre errores de envío
   - Recordar documentos pendientes de envío

2. **Inventario**:
   - Alertar cuando stock llega al mínimo
   - Notificar movimientos importantes
   - Informar sobre productos próximos a vencer

3. **Gastos**:
   - Notificar aprobaciones/rechazos
   - Alertar gastos que requieren aprobación
   - Informar sobre límites de presupuesto

4. **Certificados Digitales**:
   - Alertar 30, 15 y 7 días antes del vencimiento
   - Notificar certificados vencidos

5. **Consecutivos**:
   - Alertar cuando quedan pocos números
   - Notificar cuando se agotan

## Testing

### Script de prueba
Ejecutar `/Scripts/crear-notificaciones-prueba.sql` para crear 12 notificaciones de ejemplo que cubren todos los escenarios.

### Verificación manual
1. Iniciar sesión en el sistema
2. Navegar a cualquier página
3. Observar el icono de campana en la barra de navegación
4. Click en el icono para ver el dropdown
5. Click en una notificación para navegar
6. Click en "Marcar todas" para marcar como leídas

## Configuración

### Cambiar intervalo de actualización

En `/wwwroot/js/notifications.js`:

```javascript
const config = {
    refreshInterval: 30000,  // milisegundos (30 segundos)
    maxNotificationsShown: 10,
    apiBaseUrl: '/api/Notificaciones'
};
```

### Agregar nuevos tipos de notificaciones

1. Agregar enum en `Facturacion.Shared/Enums/TipoNotificacion.cs`
2. Actualizar método `AsignarIconoYColor` en `NotificacionService.cs`
3. Opcionalmente crear método helper en `NotificacionHelper.cs`

## Documentación Adicional

- **Guía completa**: `NOTIFICACIONES_SISTEMA_GUIA.md`
- **Documentación técnica**: `Facturacion.Shared/Entities/NOTIFICACIONES_README.md`
- **Script de prueba**: `Scripts/crear-notificaciones-prueba.sql`

## Mejoras Futuras Sugeridas

1. **WebSockets/SignalR**: Notificaciones en tiempo real sin polling
2. **Notificaciones Push**: Integración con navegadores para notificaciones del sistema
3. **Email**: Enviar notificaciones importantes por correo
4. **SMS**: Alertas críticas por mensaje de texto
5. **Configuración por usuario**: Permitir activar/desactivar tipos de notificaciones
6. **Sonido**: Reproducir sonido al recibir notificación importante
7. **Categorías**: Agrupar notificaciones por categoría/módulo
8. **Filtros**: Filtrar notificaciones por tipo, fecha, leídas/no leídas

## Archivos Modificados/Creados

### Creados
- `Facturacion.Frontend/wwwroot/js/notifications.js`
- `Facturacion.Backend/Helpers/NotificacionHelper.cs`
- `Scripts/crear-notificaciones-prueba.sql`
- `NOTIFICACIONES_SISTEMA_GUIA.md`
- `SISTEMA_NOTIFICACIONES_README.md` (este archivo)

### Modificados
- `Facturacion.Frontend/Pages/Shared/_Layout.cshtml`
  - Dropdown de notificaciones actualizado
  - Script de notificaciones agregado
  - Data attribute `data-empresa-id` agregado
- `Facturacion.Frontend/wwwroot/css/modern-theme.css`
  - Estilos para notificaciones no leídas
  - Indicador visual de no leída

## Dependencias

- **Backend**: Ya existentes (Entity Framework, JWT, etc.)
- **Frontend**: Ya existentes (jQuery, Bootstrap, SweetAlert2)
- **APIs**: Ninguna nueva

## Mantenimiento

### Limpieza de notificaciones antiguas

Ejecutar periódicamente (recomendado: semanalmente):

```sql
-- Eliminar notificaciones expiradas
DELETE FROM Notificaciones
WHERE FechaExpiracion IS NOT NULL AND FechaExpiracion < GETDATE()

-- Eliminar notificaciones leídas con más de 30 días
DELETE FROM Notificaciones
WHERE Leida = 1 AND FechaLeida IS NOT NULL AND FechaLeida < DATEADD(DAY, -30, GETDATE())
```

O usar el endpoint:
```
DELETE /api/Notificaciones/expiradas/{empresaId}
```

### Monitoreo

Consultas útiles:

```sql
-- Notificaciones por tipo
SELECT TipoNotificacion, COUNT(*) as Total
FROM Notificaciones
GROUP BY TipoNotificacion
ORDER BY Total DESC

-- Notificaciones no leídas por usuario
SELECT u.Email, COUNT(*) as NoLeidas
FROM Notificaciones n
INNER JOIN AspNetUsers u ON n.UsuarioId = u.Id
WHERE n.Leida = 0
GROUP BY u.Email
ORDER BY NoLeidas DESC

-- Notificaciones por empresa
SELECT e.NombreComercial, COUNT(*) as Total
FROM Notificaciones n
INNER JOIN Empresas e ON n.EmpresaId = e.Id
GROUP BY e.NombreComercial
ORDER BY Total DESC
```

## Soporte

Para preguntas o problemas:
1. Revisar `NOTIFICACIONES_SISTEMA_GUIA.md` (guía completa)
2. Revisar sección "Troubleshooting" en la guía
3. Verificar logs del navegador (consola JavaScript)
4. Verificar logs del backend (.NET)

## Versión

- **Versión del sistema de notificaciones**: 1.0
- **Fecha de implementación**: Febrero 2026
- **Compatible con**: FacturaciónV2 v4.4
