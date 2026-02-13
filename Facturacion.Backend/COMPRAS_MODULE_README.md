# Módulo de Compras - Repositorios y Unit of Work

## Resumen
Este documento describe los repositorios y Unit of Work implementados para el módulo de Compras en FacturacionV2.

---

## Archivos Creados

### 1. Interfaces de Repositorios (Repositories/Interfaces/)

#### IOrdenCompraRepository.cs
**Ubicación**: `/Repositories/Interfaces/IOrdenCompraRepository.cs`

Gestiona órdenes de compra a proveedores.

**Métodos principales**:
- `GetAsync(Guid id)` - Obtiene orden básica
- `GetWithDetallesAsync(Guid id)` - Obtiene orden con detalles, productos y recepciones
- `GetByEmpresaAsync(Guid empresaId)` - Todas las órdenes de una empresa
- `GetByProveedorAsync(Guid proveedorId)` - Órdenes de un proveedor
- `GetByEstadoAsync(Guid empresaId, string estado)` - Filtra por estado
- `GetPendientesRecepcionAsync(Guid empresaId)` - Órdenes APR o PAR (pendientes de recibir)
- `AddAsync(OrdenCompra orden)` - Crear nueva orden
- `UpdateAsync(OrdenCompra orden)` - Actualizar orden (solo BOR o PEN)
- `DeleteAsync(Guid id, string usuarioId)` - Soft delete (solo BOR)
- `AprobarAsync(Guid id, string usuarioId)` - Cambiar estado a APR
- `AnularAsync(Guid id, string usuarioId, string motivo)` - Cambiar estado a ANU
- `GenerarNumeroAsync(Guid empresaId)` - Formato: OC-YYYYMMDD-####

**Estados manejados**:
- BOR (Borrador)
- PEN (Pendiente)
- APR (Aprobada)
- PAR (Parcial - recepción parcial)
- COM (Completada - recepción completa)
- ANU (Anulada)

---

#### IRecepcionCompraRepository.cs
**Ubicación**: `/Repositories/Interfaces/IRecepcionCompraRepository.cs`

Registra recepciones de productos de órdenes de compra.

**Métodos principales**:
- `GetAsync(Guid id)` - Obtiene recepción básica
- `GetWithDetallesAsync(Guid id)` - Obtiene recepción con detalles y productos
- `GetByEmpresaAsync(Guid empresaId)` - Todas las recepciones de una empresa
- `GetByOrdenCompraAsync(Guid ordenCompraId)` - Recepciones de una orden específica
- `GetPendientesAsync(Guid empresaId)` - Recepciones en estado APL de órdenes PAR
- `ValidarCantidadesAsync(Guid ordenCompraId, List<RecepcionCompraDetalle> detalles)` - Valida que las cantidades no excedan lo ordenado
- `AddAsync(RecepcionCompra recepcion)` - Crear nueva recepción (actualiza cantidades en OC)
- `UpdateAsync(RecepcionCompra recepcion)` - Actualizar recepción (solo APL)
- `DeleteAsync(Guid id, string usuarioId)` - Soft delete (solo ANU)
- `AnularAsync(Guid id, string usuarioId, string motivo)` - Anular y revertir cantidades en OC
- `GenerarNumeroAsync(Guid empresaId)` - Formato: RC-YYYYMMDD-####

**Estados manejados**:
- APL (Aplicada)
- ANU (Anulada)

**Lógica especial**:
- Al crear recepción: actualiza `CantidadRecibida` en `OrdenCompraDetalle`
- Calcula estado de OC: COM si todas las líneas están completas, PAR si alguna tiene cantidad recibida
- Al anular: revierte las cantidades recibidas y recalcula estado de OC

---

#### IRequisicionRepository.cs
**Ubicación**: `/Repositories/Interfaces/IRequisicionRepository.cs`

Solicitudes internas de compra (inicia el ciclo de compras).

**Métodos principales**:
- `GetAsync(Guid id)` - Obtiene requisición básica
- `GetWithDetallesAsync(Guid id)` - Obtiene requisición con detalles y cotizaciones
- `GetByEmpresaAsync(Guid empresaId)` - Todas las requisiciones de una empresa
- `GetBySolicitanteAsync(string solicitanteId)` - Requisiciones de un usuario
- `GetByEstadoAsync(Guid empresaId, string estado)` - Filtra por estado
- `GetPendientesAprobacionAsync(Guid empresaId)` - Requisiciones en estado ENV
- `GetAprobadasSinOCAsync(Guid empresaId)` - Requisiciones APR o COT (listas para OC)
- `AddAsync(Requisicion requisicion)` - Crear nueva requisición
- `UpdateAsync(Requisicion requisicion)` - Actualizar (solo BOR)
- `DeleteAsync(Guid id, string usuarioId)` - Soft delete (solo BOR)
- `AprobarAsync(Guid id, string usuarioId)` - Cambiar estado a APR
- `RechazarAsync(Guid id, string usuarioId, string motivo)` - Cambiar estado a REC
- `AnularAsync(Guid id, string usuarioId, string motivo)` - Cambiar estado a ANU
- `GenerarNumeroAsync(Guid empresaId)` - Formato: REQ-YYYYMMDD-####

**Estados manejados**:
- BOR (Borrador)
- ENV (Enviada - pendiente aprobación)
- APR (Aprobada)
- REC (Rechazada)
- COT (En Cotización)
- OC (Orden Compra Generada)
- ANU (Anulada)

**Prioridades**:
- URG (Urgente)
- ALT (Alta)
- MED (Media)
- BAJ (Baja)

---

#### ICotizacionProveedorRepository.cs
**Ubicación**: `/Repositories/Interfaces/ICotizacionProveedorRepository.cs`

Solicitudes de cotización enviadas a proveedores.

**Métodos principales**:
- `GetAsync(Guid id)` - Obtiene cotización básica
- `GetWithDetallesAsync(Guid id)` - Obtiene cotización con detalles
- `GetByRequisicionAsync(Guid requisicionId)` - Cotizaciones de una requisición
- `GetByProveedorAsync(Guid proveedorId)` - Cotizaciones de un proveedor
- `GetByEstadoAsync(Guid empresaId, string estado)` - Filtra por estado
- `GetVencidasAsync(Guid empresaId)` - Cotizaciones ENV vencidas
- `AddAsync(CotizacionProveedor cotizacion)` - Crear nueva cotización (cambia requisición a COT)
- `UpdateAsync(CotizacionProveedor cotizacion)` - Actualizar (solo ENV o REC)
- `SeleccionarAsync(Guid id, string usuarioId)` - Marcar como seleccionada (desmarca otras)
- `RechazarAsync(Guid id, string usuarioId, string motivo)` - Cambiar estado a REJ
- `GenerarNumeroAsync(Guid empresaId)` - Formato: COT-YYYYMMDD-####

**Estados manejados**:
- ENV (Enviada)
- REC (Recibida)
- APR (Aprobada)
- REJ (Rechazada)
- VEN (Vencida)
- SEL (Seleccionada)

---

### 2. Implementaciones de Repositorios (Repositories/Implementations/)

Cada repositorio incluye:
- `ILogger` para registro de operaciones y errores
- `ActionResponse<T>` para respuestas consistentes
- Validaciones de estado antes de operaciones críticas
- Soft delete con auditoría completa
- Generación automática de números consecutivos

**Archivos**:
- `OrdenCompraRepository.cs` (20.5 KB)
- `RecepcionCompraRepository.cs` (22.8 KB)
- `RequisicionRepository.cs` (23.6 KB)
- `CotizacionProveedorRepository.cs` (20.5 KB)

**Características comunes**:
- Uso de `Include()` para eager loading de relaciones
- `AsNoTracking()` en queries de solo lectura
- Transacciones en operaciones complejas (RecepcionCompra)
- Validaciones de negocio (estados, cantidades, existencia de entidades)
- Logging estructurado de todas las operaciones

---

### 3. Interfaces de Unit of Work (UnitsOfWork/Interfaces/)

**Archivos**:
- `IOrdenCompraUnitOfWork.cs`
- `IRecepcionCompraUnitOfWork.cs`
- `IRequisicionUnitOfWork.cs`
- `ICotizacionProveedorUnitOfWork.cs`

Cada interfaz expone los mismos métodos que su repositorio correspondiente.

---

### 4. Implementaciones de Unit of Work (UnitsOfWork/Implementations/)

**Archivos**:
- `OrdenCompraUnitOfWork.cs`
- `RecepcionCompraUnitOfWork.cs`
- `RequisicionUnitOfWork.cs`
- `CotizacionProveedorUnitOfWork.cs`

Cada implementación delega directamente al repositorio correspondiente usando arrow notation para métodos de una línea.

---

## Flujo de Procesos de Compras

### 1. Flujo de Requisición → Orden de Compra

```
1. Usuario crea Requisición (estado BOR)
2. Usuario envía a aprobación (estado ENV)
3. Aprobador aprueba (estado APR) o rechaza (estado REC)
4. Si aprobada, se pueden solicitar cotizaciones (estado COT)
5. Se selecciona mejor cotización (estado SEL)
6. Se genera Orden de Compra (requisición pasa a estado OC)
```

### 2. Flujo de Orden de Compra → Recepción

```
1. Se crea Orden de Compra (estado BOR)
2. Se aprueba (estado APR)
3. Se recibe mercadería (RecepcionCompra)
   - Si recepción parcial: OC estado PAR
   - Si recepción completa: OC estado COM
4. Cada recepción actualiza CantidadRecibida en OrdenCompraDetalle
```

### 3. Máquina de Estados - Orden de Compra

```
BOR → PEN → APR → PAR → COM
                      ↓
                    ANU
```

### 4. Máquina de Estados - Requisición

```
BOR → ENV → APR → COT → SEL → OC
         ↓     ↓
       REC   ANU
```

---

## Validaciones Implementadas

### OrdenCompra
- Solo se pueden editar órdenes en estado BOR o PEN
- Solo se pueden eliminar órdenes en estado BOR
- Solo se pueden aprobar órdenes en estado PEN o BOR
- No se pueden anular órdenes completadas o con recepciones aplicadas

### RecepcionCompra
- Solo se pueden recibir órdenes en estado APR o PAR
- Las cantidades recibidas no pueden exceder las cantidades pendientes
- Al anular, revierte cantidades en la orden de compra
- Solo se pueden eliminar recepciones anuladas

### Requisicion
- Solo se pueden editar requisiciones en estado BOR
- Solo se pueden eliminar requisiciones en estado BOR
- Solo se pueden aprobar/rechazar requisiciones en estado ENV
- No se pueden anular requisiciones con OC generada o cotizaciones seleccionadas

### CotizacionProveedor
- Solo se pueden crear cotizaciones para requisiciones aprobadas (APR o COT)
- Solo se pueden editar cotizaciones en estado ENV o REC
- Al seleccionar, desmarca automáticamente otras cotizaciones de la misma requisición

---

## Próximos Pasos

### Pendiente de Implementación

1. **Configuración en DataContext.cs**:
   - Agregar DbSets para las nuevas entidades
   - Configurar relaciones en OnModelCreating
   - Configurar índices para números (únicos)

2. **Registro en Program.cs**:
   ```csharp
   // Repositories
   builder.Services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
   builder.Services.AddScoped<IRecepcionCompraRepository, RecepcionCompraRepository>();
   builder.Services.AddScoped<IRequisicionRepository, RequisicionRepository>();
   builder.Services.AddScoped<ICotizacionProveedorRepository, CotizacionProveedorRepository>();

   // Unit of Work
   builder.Services.AddScoped<IOrdenCompraUnitOfWork, OrdenCompraUnitOfWork>();
   builder.Services.AddScoped<IRecepcionCompraUnitOfWork, RecepcionCompraUnitOfWork>();
   builder.Services.AddScoped<IRequisicionUnitOfWork, RequisicionUnitOfWork>();
   builder.Services.AddScoped<ICotizacionProveedorUnitOfWork, CotizacionProveedorUnitOfWork>();
   ```

3. **Crear Migraciones**:
   ```bash
   dotnet ef migrations add AddComprasModule
   dotnet ef database update
   ```

4. **Implementar Controllers**:
   - OrdenCompraController
   - RecepcionCompraController
   - RequisicionController
   - CotizacionProveedorController

5. **Crear Razor Pages en Frontend**:
   - OrdenesCompra.cshtml
   - RecepcionesCompra.cshtml
   - Requisiciones.cshtml
   - CotizacionesProveedor.cshtml

---

## Patrones Aplicados

### ActionResponse<T>
Todas las operaciones retornan `ActionResponse<T>` con:
- `WasSuccess` (bool)
- `Message` (string)
- `Result` (T)

### Soft Delete
Todas las entidades implementan:
- `IsDeleted` (bool)
- `FechaEliminacion` (DateTime?)
- `UsuarioEliminacionId` (string?)

### Audit Trail
Todas las entidades rastrean:
- Creación: `FechaCreacion`, `CreadoPorId`
- Modificación: `FechaModificacion`, `ModificadoPorId`
- Aprobación: `FechaAprobacion`, `AprobadoPorId` (OrdenCompra, Requisicion)

### Logging
Todos los repositorios incluyen:
- Logs de información para operaciones exitosas
- Logs de error para excepciones con contexto completo

---

## Dependencias entre Entidades

```
Empresa
  ├── Requisicion
  │     ├── RequisicionDetalle (Producto)
  │     └── CotizacionProveedor
  │           └── CotizacionProveedorDetalle (Producto)
  │
  └── OrdenCompra (Proveedor, Sucursal, Bodega)
        ├── OrdenCompraDetalle (Producto)
        └── RecepcionCompra (Bodega)
              └── RecepcionCompraDetalle (Producto, OrdenCompraDetalle)
```

---

## Generación de Números Consecutivos

Formato unificado: `{PREFIJO}-{YYYYMMDD}-{####}`

- OrdenCompra: `OC-20260209-0001`
- RecepcionCompra: `RC-20260209-0001`
- Requisicion: `REQ-20260209-0001`
- CotizacionProveedor: `COT-20260209-0001`

Los números se resetean diariamente y son secuenciales por empresa.

---

## Notas de Implementación

### RecepcionCompraRepository
- Utiliza transacciones para garantizar consistencia al actualizar cantidades en OrdenCompra
- Implementa lógica compleja para calcular estados de OC (APR → PAR → COM)
- Revierte cantidades al anular recepciones

### CotizacionProveedorRepository
- Actualiza automáticamente el estado de la requisición a COT al crear primera cotización
- Desmarca automáticamente otras cotizaciones al seleccionar una

### RequisicionRepository
- Ordena pendientes de aprobación por prioridad y fecha requerida
- Valida que no tenga cotizaciones seleccionadas antes de anular

### OrdenCompraRepository
- Valida que no tenga recepciones aplicadas antes de anular
- Soporta cambio de estado de BOR/PEN directamente a APR
