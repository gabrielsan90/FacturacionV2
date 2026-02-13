-- Script para crear notificaciones de prueba
-- Ejecutar este script para probar el sistema de notificaciones

-- Variables (ajustar según tu base de datos)
DECLARE @EmpresaId UNIQUEIDENTIFIER
DECLARE @UsuarioId NVARCHAR(450)
DECLARE @DocumentoId UNIQUEIDENTIFIER

-- Obtener una empresa activa
SELECT TOP 1 @EmpresaId = Id FROM Empresas WHERE Activa = 1

-- Obtener un usuario administrador
SELECT TOP 1 @UsuarioId = Id FROM AspNetUsers
WHERE Id IN (
    SELECT UserId FROM UsuariosEmpresas WHERE EmpresaId = @EmpresaId
)

-- Obtener un documento de prueba
SELECT TOP 1 @DocumentoId = Id FROM Documentos WHERE EmpresaId = @EmpresaId

PRINT 'EmpresaId: ' + CAST(@EmpresaId AS NVARCHAR(50))
PRINT 'UsuarioId: ' + @UsuarioId
PRINT 'DocumentoId: ' + CAST(@DocumentoId AS NVARCHAR(50))

-- Crear notificaciones de prueba

-- 1. Documento Aceptado (no leída)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaCreacion, Importante, EntidadRelacionadaId, TipoEntidad, UrlAccion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    1, -- DocumentoAceptado
    'Documento Aceptado',
    'La factura FE-001-001-00000123 fue aceptada por Hacienda',
    'fa-check-circle',
    'success',
    0, -- No leída
    DATEADD(MINUTE, -5, GETDATE()),
    0,
    @DocumentoId,
    'Documento',
    '/DocumentosElectronicos/Documentos'
)

-- 2. Stock Bajo (no leída, importante)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaCreacion, Importante, TipoEntidad, UrlAccion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    6, -- InventarioBajo
    'Stock Bajo',
    'El producto "Laptop Dell XPS 15" tiene stock bajo (5 unidades, mínimo: 10)',
    'fa-boxes',
    'danger',
    0, -- No leída
    DATEADD(HOUR, -1, GETDATE()),
    1, -- Importante
    'Inventario',
    '/Stock/Inventario'
)

-- 3. Documento Rechazado (no leída, importante)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaCreacion, Importante, EntidadRelacionadaId, TipoEntidad, UrlAccion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    2, -- DocumentoRechazado
    'Documento Rechazado',
    'La factura FE-001-001-00000122 fue rechazada por Hacienda: Cédula del receptor no es válida',
    'fa-times-circle',
    'danger',
    0, -- No leída
    DATEADD(HOUR, -2, GETDATE()),
    1, -- Importante
    @DocumentoId,
    'Documento',
    '/DocumentosElectronicos/Documentos'
)

-- 4. Pago Recibido (leída)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaLeida, FechaCreacion, Importante, TipoEntidad, UrlAccion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    4, -- PagoRecibido
    'Pago Recibido',
    'Se recibió un pago de ₡500,000.00 del cliente ABC S.A.',
    'fa-money-bill-wave',
    'success',
    1, -- Leída
    DATEADD(HOUR, -2, GETDATE()),
    DATEADD(HOUR, -3, GETDATE()),
    0,
    'Pago',
    '/CxC/CuentasPorCobrar'
)

-- 5. Certificado Próximo a Vencer (no leída, importante)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaCreacion, Importante, TipoEntidad, UrlAccion, FechaExpiracion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    14, -- CertificadoPorVencer
    'Certificado Digital Próximo a Vencer',
    'El certificado digital vence en 15 días (31/03/2026). Renuévelo pronto para continuar facturando.',
    'fa-certificate',
    'warning',
    0, -- No leída
    DATEADD(DAY, -1, GETDATE()),
    1, -- Importante
    'Empresa',
    '/Configuracion/Empresas',
    DATEADD(DAY, 22, GETDATE()) -- Expira 22 días después
)

-- 6. Error en Envío a Hacienda (no leída, importante)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaCreacion, Importante, EntidadRelacionadaId, TipoEntidad, UrlAccion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    16, -- ErrorEnvioHacienda
    'Error en Envío a Hacienda',
    'Error al enviar FE-001-001-00000124: Timeout al conectar con el servicio de Hacienda',
    'fa-times-circle',
    'danger',
    0, -- No leída
    DATEADD(MINUTE, -30, GETDATE()),
    1, -- Importante
    @DocumentoId,
    'Documento',
    '/DocumentosElectronicos/Documentos'
)

-- 7. Gasto Requiere Aprobación (no leída)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaCreacion, Importante, TipoEntidad, UrlAccion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    9, -- GastoRequiereAprobacion
    'Gasto Requiere Aprobación',
    'El gasto "Compra de equipos de oficina" por ₡1,250,000.00 requiere su aprobación',
    'fa-clipboard-check',
    'info',
    0, -- No leída
    DATEADD(MINUTE, -45, GETDATE()),
    0,
    'Gasto',
    '/Gastos/Gasto'
)

-- 8. Consecutivo Próximo a Agotarse (no leída, importante)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaCreacion, Importante, TipoEntidad, UrlAccion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    18, -- ConsecutivoPorAgotar
    'Consecutivo Próximo a Agotarse',
    'El consecutivo de Facturas Electrónicas se está agotando. Quedan 50 números disponibles.',
    'fa-sort-numeric-up',
    'warning',
    0, -- No leída
    DATEADD(DAY, -2, GETDATE()),
    1, -- Importante
    'Consecutivo',
    '/Configuracion/Empresas'
)

-- 9. Nuevo Usuario (leída)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaLeida, FechaCreacion, Importante, TipoEntidad, UrlAccion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    10, -- NuevoUsuario
    'Nuevo Usuario Registrado',
    'El usuario "María González" se ha registrado en el sistema',
    'fa-user-plus',
    'info',
    1, -- Leída
    DATEADD(DAY, -2, GETDATE()),
    DATEADD(DAY, -3, GETDATE()),
    0,
    'Usuario',
    '/Seguridad/Usuarios'
)

-- 10. REP Pendiente (no leída)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaCreacion, Importante, EntidadRelacionadaId, TipoEntidad, UrlAccion, FechaExpiracion)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    20, -- REPPendiente
    'REP Pendiente',
    'Recuerde emitir el REP para la factura FE-001-001-00000120 antes del 28/02/2026',
    'fa-file-invoice-dollar',
    'warning',
    0, -- No leída
    DATEADD(DAY, -1, GETDATE()),
    0,
    @DocumentoId,
    'Documento',
    '/Pagos/RecibosPago',
    DATEADD(DAY, 14, GETDATE())
)

-- 11. Actualización Disponible (leída)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaLeida, FechaCreacion, Importante)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    19, -- ActualizacionDisponible
    'Nueva Versión Disponible',
    'La versión 4.5 del sistema ya está disponible con mejoras en rendimiento y nuevas funcionalidades',
    'fa-download',
    'info',
    1, -- Leída
    DATEADD(DAY, -4, GETDATE()),
    DATEADD(DAY, -5, GETDATE()),
    0
)

-- 12. Sistema (leída)
INSERT INTO Notificaciones (Id, EmpresaId, UsuarioId, TipoNotificacion, Titulo, Mensaje, Icono, Color, Leida, FechaLeida, FechaCreacion, Importante)
VALUES (
    NEWID(),
    @EmpresaId,
    @UsuarioId,
    11, -- Sistema
    'Mantenimiento Programado',
    'El sistema tendrá mantenimiento el próximo domingo de 2:00 AM a 6:00 AM',
    'fa-cog',
    'secondary',
    1, -- Leída
    DATEADD(DAY, -3, GETDATE()),
    DATEADD(DAY, -4, GETDATE()),
    1 -- Importante
)

PRINT 'Se crearon 12 notificaciones de prueba exitosamente'

-- Verificar las notificaciones creadas
SELECT
    TipoNotificacion,
    Titulo,
    Mensaje,
    Color,
    Leida,
    Importante,
    FechaCreacion,
    DATEDIFF(MINUTE, FechaCreacion, GETDATE()) as MinutosTranscurridos
FROM Notificaciones
WHERE UsuarioId = @UsuarioId
ORDER BY FechaCreacion DESC
