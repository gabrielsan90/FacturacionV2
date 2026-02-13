# Estado Actual de Base de Datos - FacturacionV2

**Fecha**: 2026-02-07
**Base de datos**: ATFE (SQL Server)
**Servidor**: www.smarttechcr.com
**Última migración**: `IntegracionERP_Fase7_Contabilidad`

---

## Entidades Existentes (120 clases)

### Core / Multi-Tenancy
| Entidad | Descripción | PK |
|---------|-------------|-----|
| User | Usuario del sistema (ASP.NET Identity) | string |
| Empresa | Compañía multi-tenant | Guid |
| Sucursal | Sucursales de empresa | Guid |
| Terminal | Puntos de venta | Guid |
| UsuarioEmpresa | Relación Usuario-Empresa | Guid |

### Maestros de Negocio
| Entidad | Descripción | PK | Notas |
|---------|-------------|-----|-------|
| Cliente | Clientes/Receptores | Guid | **EXTENDIDO Fase 1**: +18 campos CRM, exoneración |
| Proveedor | Proveedores | Guid | **EXTENDIDO Fase 1**: +26 campos bancarios, comercial |
| Producto | Productos y servicios | Guid | |
| Categoria | Categorías de productos | Guid | |
| ActividadEconomica | Actividades CIIU4 | Guid | |
| EmpresaActividadEconomica | Relación Empresa-Actividad | Guid | |
| Banco | Catálogo de bancos | Guid | **NUEVO Fase 1** |

### Documentos Electrónicos (Hacienda v4.4)
| Entidad | Descripción | PK |
|---------|-------------|-----|
| Documento | Documento electrónico principal | Guid |
| DocumentoDetalle | Líneas del documento | Guid |
| DocumentoDetalleImpuesto | Impuestos por línea (FASE 2) | Guid |
| DocumentoDetalleDescuento | Descuentos por línea (FASE 2) | Guid |
| DocumentoDetalleVIN | VINs por línea (v4.4) | Guid |
| DocumentoDescuento | Descuentos globales | Guid |
| DocumentoOtroCargo | Otros cargos (FASE 2) | Guid |
| DocumentoMedioPago | Métodos de pago | Guid |
| DocumentoReferencia | Referencias a otros docs | Guid |
| DocumentoExportacion | Datos de exportación | Guid |
| DocumentoReceptorMensaje | Mensajes de Hacienda | Guid |
| DocumentoOtraInformacion | Info adicional | Guid |
| ReciboPago | Recibos electrónicos de pago | Guid |
| Consecutivo | Numeración secuencial | Guid |
| HaciendaToken | Tokens OAuth2 | Guid |

### Catálogos de Costa Rica (Hacienda)
| Entidad | Descripción | PK |
|---------|-------------|-----|
| Provincia | Provincias CR | string |
| Canton | Cantones CR | string |
| Distrito | Distritos CR | string |
| Barrio | Barrios CR | string |
| CAByS | Clasificador bienes/servicios | string |
| TipoDocumento | Tipos de documentos | string |
| TipoCodigo | Tipos de identificación | string |
| UnidadMedida | Unidades de medida | string |
| Impuesto | Impuestos | string |
| TarifaIVA | Tarifas de IVA | string |
| CodigoExoneracion | Códigos exoneración | string |
| CodigoReferencia | Códigos referencia | string |
| TipoDocumentoReferencia | Tipos doc referencia | string |
| TipoDescuentoHacienda | Tipos de descuento | string |
| CondicionVenta | Condiciones de venta | string |
| MedioPago | Formas de pago | string |
| FormaFarmaceutica | Formas farmacéuticas | string |

### Inventario y Gastos
| Entidad | Descripción | PK | Notas |
|---------|-------------|-----|-------|
| Bodega | Almacenes por sucursal | Guid | **NUEVO Fase 1** |
| Inventario | Stock por producto/sucursal/bodega | Guid | **EXTENDIDO Fase 2**: +BodegaId, LoteId, CostoPromedio |
| MovimientoInventario | Movimientos de stock | Guid | |
| Lote | Control de lotes y vencimientos | Guid | **NUEVO Fase 2** |
| TrasladoInventario | Traslados entre bodegas | Guid | **NUEVO Fase 2** |
| TrasladoInventarioDetalle | Detalle de traslados | Guid | **NUEVO Fase 2** |
| AjusteInventario | Ajustes de inventario | Guid | **NUEVO Fase 2** |
| AjusteInventarioDetalle | Detalle de ajustes | Guid | **NUEVO Fase 2** |
| Gasto | Registro de gastos | Guid | |
| CategoriaGasto | Categorías de gastos | Guid | |

### CRM (Fase 3)
| Entidad | Descripción | PK | Notas |
|---------|-------------|-----|-------|
| EtapaPipeline | Etapas del pipeline de ventas | Guid | **NUEVO Fase 3** |
| Competidor | Catálogo de competidores | Guid | **NUEVO Fase 3** |
| Oportunidad | Oportunidades de venta | Guid | **NUEVO Fase 3** |
| ActividadCRM | Actividades CRM (llamadas, reuniones, etc.) | Guid | **NUEVO Fase 3** |
| NotaOportunidad | Notas asociadas a oportunidades | Guid | **NUEVO Fase 3** |
| HistorialEtapaOportunidad | Historial de cambios de etapa | Guid | **NUEVO Fase 3** |

### RRHH & Nómina (Fase 4)
| Entidad | Descripción | PK | Notas |
|---------|-------------|-----|-------|
| Departamento | Departamentos organizacionales con jerarquía | Guid | **NUEVO Fase 4** |
| Puesto | Puestos de trabajo con rango salarial | Guid | **NUEVO Fase 4** |
| Empleado | Empleados con datos personales, laborales, pago | Guid | **NUEVO Fase 4** |
| ContactoEmergencia | Contactos de emergencia por empleado | Guid | **NUEVO Fase 4** |
| ExpedienteDigital | Documentos digitales del empleado | Guid | **NUEVO Fase 4** |
| Vacacion | Control de vacaciones con aprobación | Guid | **NUEVO Fase 4** |
| Incapacidad | Incapacidades médicas (CCSS Costa Rica) | Guid | **NUEVO Fase 4** |
| AccionPersonal | Acciones de personal (promociones, aumentos, etc.) | Guid | **NUEVO Fase 4** |
| Planilla | Encabezado de planilla de pagos | Guid | **NUEVO Fase 4** |
| DetallePlanilla | Detalle de planilla por empleado (nómina CR) | Guid | **NUEVO Fase 4** |

### Activos Fijos (Fase 5)
| Entidad | Descripción | PK | Notas |
|---------|-------------|-----|-------|
| CategoriaActivo | Categorías de activos con vida útil y % depreciación | Guid | **NUEVO Fase 5** |
| ActivoFijo | Activos fijos con valores, depreciación y ubicación | Guid | **NUEVO Fase 5** |
| DepreciacionActivo | Historial de depreciaciones aplicadas | Guid | **NUEVO Fase 5** |
| TrasladoActivo | Traslados de activos entre sucursales/responsables | Guid | **NUEVO Fase 5** |

### Compras Avanzadas (Fase 6)
| Entidad | Descripción | PK | Notas |
|---------|-------------|-----|-------|
| Requisicion | Requisiciones de compra con flujo de aprobación | Guid | **NUEVO Fase 6** |
| RequisicionDetalle | Líneas de requisición | Guid | **NUEVO Fase 6** |
| CotizacionProveedor | Cotizaciones recibidas de proveedores | Guid | **NUEVO Fase 6** |
| CotizacionProveedorDetalle | Líneas de cotización | Guid | **NUEVO Fase 6** |
| ComparativoCotizacion | Comparativo de cotizaciones por requisición | Guid | **NUEVO Fase 6** |
| ComparativoCotizacionDetalle | Evaluación por cotización | Guid | **NUEVO Fase 6** |
| EvaluacionProveedor | Evaluaciones periódicas de proveedores | Guid | **NUEVO Fase 6** |
| OrdenCompra | Órdenes de compra a proveedores | Guid | **NUEVO Fase 6** |
| OrdenCompraDetalle | Líneas de orden de compra | Guid | **NUEVO Fase 6** |
| RecepcionCompra | Recepciones de mercadería | Guid | **NUEVO Fase 6** |
| RecepcionCompraDetalle | Líneas de recepción | Guid | **NUEVO Fase 6** |

### Contabilidad (Fase 7)
| Entidad | Descripción | PK | Notas |
|---------|-------------|-----|-------|
| CuentaContable | Plan de cuentas con estructura jerárquica | Guid | **NUEVO Fase 7** |
| PeriodoFiscal | Períodos fiscales anuales (Oct-Sep Costa Rica) | Guid | **NUEVO Fase 7** |
| PeriodoContable | Períodos contables mensuales | Guid | **NUEVO Fase 7** |
| AsientoContable | Asientos contables/partidas de diario | Guid | **NUEVO Fase 7** |
| MovimientoContable | Líneas de movimiento contable | Guid | **NUEVO Fase 7** |
| ConfiguracionContable | Configuración del módulo contable por empresa | Guid | **NUEVO Fase 7** |
| CuentaIntegracion | Mapeo de cuentas para integración automática | Guid | **NUEVO Fase 7** |

### Seguridad y Auditoría
| Entidad | Descripción | PK |
|---------|-------------|-----|
| Rol | Roles del sistema | Guid |
| Modulo | Módulos del sistema | Guid |
| Privilegio | Permisos CRUD | Guid |
| RolPrivilegio | Relación Rol-Privilegio | Guid |
| Auditoria | Log de cambios | Guid |
| Notificacion | Notificaciones | Guid |

### Contactos
| Entidad | Descripción | PK |
|---------|-------------|-----|
| Telefono | Teléfonos | Guid |
| Email | Correos electrónicos | Guid |

### Migración
| Entidad | Descripción | PK |
|---------|-------------|-----|
| MigracionIdMapping | Mapeo IDs int→Guid | Guid |

---

## Campos Nuevos - Fase 1

### Cliente (18 campos nuevos)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| DiasCredito | int | Días de crédito permitidos |
| ExentoIVA | bool | Si está exento de IVA |
| TipoExoneracion | string(2) | Código tipo exoneración Hacienda |
| NumeroExoneracion | string(40) | Número documento exoneración |
| NombreInstitucionExoneracion | string(160) | Institución que otorga |
| FechaExoneracion | DateTime? | Fecha del documento |
| PorcentajeExoneracion | decimal | Porcentaje de exoneración |
| DescuentoGeneral | decimal | Descuento general aplicable |
| FechaUltimoPago | DateTime? | Último pago recibido |
| FechaUltimaCompra | DateTime? | Última compra realizada |
| Categoria | string(50) | Categoría del cliente |
| RequiereOrdenCompra | bool | Si requiere OC para facturar |
| Contacto | string(100) | Nombre de contacto |
| TelefonoContacto | string(20) | Teléfono del contacto |
| Zona | string(50) | Zona geográfica |
| EnMora | bool | Si está en mora |
| Bloqueado | bool | Si está bloqueado |
| MotivoBloqueo | string(200) | Razón del bloqueo |
| Notas | string(500) | Notas adicionales |

### Proveedor (26 campos nuevos)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| DiasCredito | int | Días de crédito otorgados |
| Celular | string(20) | Celular de contacto |
| SitioWeb | string(200) | Sitio web |
| Banco | string(100) | Nombre del banco |
| CuentaBancaria | string(30) | Número de cuenta |
| TipoCuentaBancaria | string(20) | Ahorro/Corriente |
| IBAN | string(30) | Código IBAN |
| LimiteCredito | decimal | Límite de crédito |
| SaldoPendiente | decimal | Saldo por pagar |
| FechaUltimoPago | DateTime? | Último pago realizado |
| FechaUltimaCompra | DateTime? | Última compra |
| Categoria | string(50) | Categoría del proveedor |
| ProductosServicios | string(500) | Productos que ofrece |
| TiempoEntrega | int | Días promedio de entrega |
| PedidoMinimo | decimal | Monto mínimo de pedido |
| DescuentoGeneral | decimal | Descuento general |
| EsExtranjero | bool | Si es proveedor extranjero |
| Pais | string(50) | País de origen |
| ContactoCompras | string(100) | Nombre contacto compras |
| TelefonoContacto | string(20) | Teléfono contacto |
| EmailContacto | string(100) | Email contacto |
| RetencionIVA | decimal | % retención IVA |
| RetencionRenta | decimal | % retención renta |
| Bloqueado | bool | Si está bloqueado |
| MotivoBloqueo | string(200) | Razón del bloqueo |
| Notas | string(500) | Notas adicionales |

---

## Módulos del ERP a Integrar

Los siguientes módulos serán integrados desde el proyecto ERP:

### Fase 1: Catálogos y Maestros (COMPLETADA)
- [x] Extender Cliente con campos CRM y exoneración
- [x] Extender Proveedor con campos bancarios y comerciales
- [x] Agregar Bodega (almacenes por sucursal)
- [x] Agregar Banco (catálogo de bancos)
- [ ] Agregar Moneda con tipo de cambio (pendiente)

### Fase 2: Inventario Avanzado (COMPLETADA)
- [x] Stock por bodega (extender Inventario con BodegaId)
- [x] Lote (control de lotes y vencimientos)
- [x] TrasladoInventario + TrasladoInventarioDetalle
- [x] AjusteInventario + AjusteInventarioDetalle
- [ ] ConfiguracionCosteo (pendiente)

### Fase 3: CRM (COMPLETADA)
- [x] EtapaPipeline (etapas del pipeline de ventas)
- [x] Competidor (catálogo de competidores)
- [x] Oportunidad (oportunidades de venta con cliente, vendedor, monto, probabilidad)
- [x] ActividadCRM (llamadas, reuniones, emails, tareas, visitas, demostraciones)
- [x] NotaOportunidad (notas asociadas a oportunidades)
- [x] HistorialEtapaOportunidad (historial de cambios de etapa)

### Fase 4: RRHH & Nómina (COMPLETADA)
- [x] Departamento (departamentos organizacionales con jerarquía)
- [x] Puesto (puestos de trabajo con rangos salariales y requisitos)
- [x] Empleado (empleados con datos personales, laborales, bancarios)
- [x] ContactoEmergencia (contactos de emergencia por empleado)
- [x] ExpedienteDigital (documentos digitales del expediente)
- [x] Vacacion (control de vacaciones con flujo de aprobación)
- [x] Incapacidad (incapacidades médicas según CCSS)
- [x] AccionPersonal (promociones, aumentos, traslados, amonestaciones)
- [x] Planilla (encabezado de planilla por período)
- [x] DetallePlanilla (detalle de nómina con cálculos Costa Rica)

### Fase 5: Activos Fijos (COMPLETADA)
- [x] CategoriaActivo (categorías con vida útil, % depreciación, cuentas contables)
- [x] ActivoFijo (activos con valores, depreciación línea recta, ubicación, responsable)
- [x] DepreciacionActivo (historial de depreciaciones por período)
- [x] TrasladoActivo (traslados entre sucursales con flujo de aprobación)

### Fase 6: Compras Avanzadas (COMPLETADA)
- [x] Requisicion + RequisicionDetalle (requisiciones de compra con aprobación)
- [x] CotizacionProveedor + CotizacionProveedorDetalle (cotizaciones de proveedores)
- [x] ComparativoCotizacion + ComparativoCotizacionDetalle (comparativo de cotizaciones)
- [x] EvaluacionProveedor (evaluaciones periódicas de proveedores)
- [x] OrdenCompra + OrdenCompraDetalle (órdenes de compra)
- [x] RecepcionCompra + RecepcionCompraDetalle (recepciones de mercadería)

### Fase 7: Contabilidad (COMPLETADA)
- [x] CuentaContable (plan de cuentas jerárquico con niveles y naturaleza)
- [x] PeriodoFiscal (años fiscales Oct-Sep para Costa Rica)
- [x] PeriodoContable (períodos mensuales con control de apertura/cierre)
- [x] AsientoContable (partidas de diario con aprobación)
- [x] MovimientoContable (líneas con debe/haber, centro costo, tercero)
- [x] ConfiguracionContable (parámetros del módulo y cuentas principales)
- [x] CuentaIntegracion (mapeo automático por módulo/operación)

### Fase 8: Workflow
- [ ] TipoWorkflow
- [ ] NivelAprobacion
- [ ] SolicitudAprobacion

### Fase 9+: Otros módulos
- [ ] Presupuestos
- [ ] Conciliación Bancaria
- [ ] Dashboard personalizable
- [ ] Permisos granulares

---

## Índices Importantes

### Existentes
- `IX_Documento_Clave` - Búsqueda por clave 50 dígitos
- `IX_Consecutivo_Unique` - Unicidad de consecutivos
- `IX_Cliente_NumeroIdentificacion` - Búsqueda por cédula
- `IX_Producto_Codigo` - Búsqueda por código
- `IX_MigracionIdMapping_NombreEntidad_IdAnterior` - Mapeo de IDs

### Nuevos (Fase 1)
- `IX_Banco_Codigo` - Código único de banco
- `IX_Banco_CodigoSINPE` - Búsqueda por código SINPE
- `IX_Bodega_Empresa_Sucursal_Codigo` - Código único por empresa/sucursal

### Nuevos (Fase 2)
- `IX_Inventario_BodegaId` - Búsqueda por bodega
- `IX_Lote_Empresa_NumeroLote` - Número de lote único por empresa
- `IX_TrasladoInventario_Empresa_Numero` - Número traslado único por empresa
- `IX_AjusteInventario_Empresa_Numero` - Número ajuste único por empresa

### Nuevos (Fase 3 - CRM)
- `IX_EtapaPipeline_Empresa_Codigo` - Código único de etapa por empresa
- `IX_Competidor_Empresa_Nombre` - Nombre único de competidor por empresa
- `IX_Oportunidad_Empresa_Numero` - Número único de oportunidad por empresa
- `IX_Oportunidad_ClienteId` - Búsqueda por cliente
- `IX_Oportunidad_CodigoEtapa` - Búsqueda por etapa
- `IX_Oportunidad_VendedorId` - Búsqueda por vendedor
- `IX_ActividadCRM_Empresa_FechaProgramada` - Actividades por fecha programada
- `IX_ActividadCRM_OportunidadId` - Actividades por oportunidad
- `IX_ActividadCRM_ClienteId` - Actividades por cliente
- `IX_ActividadCRM_AsignadoAId` - Actividades por usuario asignado
- `IX_ActividadCRM_Estado` - Actividades por estado
- `IX_NotaOportunidad_OportunidadId` - Notas por oportunidad
- `IX_HistorialEtapaOportunidad_OportunidadId` - Historial por oportunidad
- `IX_HistorialEtapaOportunidad_FechaCambio` - Historial por fecha de cambio

### Nuevos (Fase 4 - RRHH & Nómina)
- `IX_Departamento_Empresa_Codigo` - Código único de departamento por empresa
- `IX_Puesto_Empresa_Codigo` - Código único de puesto por empresa
- `IX_Empleado_Empresa_Codigo` - Código único de empleado por empresa
- `IX_Empleado_Identificacion` - Búsqueda por identificación
- `IX_Empleado_DepartamentoId` - Empleados por departamento
- `IX_Empleado_PuestoId` - Empleados por puesto
- `IX_Empleado_Estado` - Empleados por estado (ACT, INA, VAC, INC, LIC)
- `IX_ContactoEmergencia_EmpleadoId` - Contactos por empleado
- `IX_ExpedienteDigital_EmpleadoId` - Documentos por empleado
- `IX_Vacacion_EmpleadoId` - Vacaciones por empleado
- `IX_Vacacion_Estado` - Vacaciones por estado (SOL, APR, REC, DIS, CAN)
- `IX_Incapacidad_EmpleadoId` - Incapacidades por empleado
- `IX_AccionPersonal_EmpleadoId` - Acciones por empleado
- `IX_AccionPersonal_FechaAccion` - Acciones por fecha
- `IX_Planilla_Empresa_Codigo` - Código único de planilla por empresa
- `IX_Planilla_Empresa_Periodo` - Planillas por empresa/año/mes/período
- `IX_DetallePlanilla_PlanillaId` - Detalles por planilla
- `IX_DetallePlanilla_EmpleadoId` - Detalles por empleado

### Nuevos (Fase 5 - Activos Fijos)
- `IX_CategoriaActivo_Empresa_Codigo` - Código único de categoría por empresa
- `IX_ActivoFijo_Empresa_Codigo` - Código único de activo por empresa
- `IX_ActivoFijo_CategoriaId` - Activos por categoría
- `IX_ActivoFijo_SucursalId` - Activos por sucursal
- `IX_ActivoFijo_ResponsableId` - Activos por responsable
- `IX_ActivoFijo_Estado` - Activos por estado (ACT, BAJ, VEN, ROB, DEP)
- `IX_ActivoFijo_NumeroSerie` - Búsqueda por número de serie
- `IX_DepreciacionActivo_ActivoFijoId` - Depreciaciones por activo
- `IX_DepreciacionActivo_Activo_Periodo` - Depreciación única por activo/período
- `IX_DepreciacionActivo_Fecha` - Depreciaciones por fecha
- `IX_TrasladoActivo_Empresa_Numero` - Número único de traslado por empresa
- `IX_TrasladoActivo_ActivoFijoId` - Traslados por activo
- `IX_TrasladoActivo_Fecha` - Traslados por fecha
- `IX_TrasladoActivo_Estado` - Traslados por estado (PEN, APR, REC, CAN)

### Nuevos (Fase 6 - Compras Avanzadas)
- `IX_Requisicion_Empresa_Numero` - Número único de requisición por empresa
- `IX_Requisicion_SolicitanteId` - Requisiciones por solicitante
- `IX_Requisicion_DepartamentoId` - Requisiciones por departamento
- `IX_Requisicion_Estado` - Requisiciones por estado (BOR, ENV, APR, REC, COM, ANU)
- `IX_RequisicionDetalle_RequisicionId` - Detalles por requisición
- `IX_CotizacionProveedor_Empresa_Numero` - Número único de cotización por empresa
- `IX_CotizacionProveedor_RequisicionId` - Cotizaciones por requisición
- `IX_CotizacionProveedor_ProveedorId` - Cotizaciones por proveedor
- `IX_CotizacionProveedor_Estado` - Cotizaciones por estado (REC, EVA, SEL, RCH, VEN)
- `IX_CotizacionProveedorDetalle_CotizacionId` - Detalles por cotización
- `IX_ComparativoCotizacion_Empresa_Numero` - Número único de comparativo por empresa
- `IX_ComparativoCotizacion_RequisicionId` - Comparativos por requisición
- `IX_ComparativoCotizacionDetalle_ComparativoId` - Detalles por comparativo
- `IX_EvaluacionProveedor_Empresa_ProveedorId` - Evaluaciones por empresa/proveedor
- `IX_EvaluacionProveedor_Periodo` - Evaluaciones por año/mes
- `IX_OrdenCompra_Empresa_Numero` - Número único de orden por empresa
- `IX_OrdenCompra_ProveedorId` - Órdenes por proveedor
- `IX_OrdenCompra_Estado` - Órdenes por estado (BOR, PEN, APR, PAR, COM, ANU)
- `IX_OrdenCompraDetalle_OrdenCompraId` - Detalles por orden
- `IX_RecepcionCompra_Empresa_Numero` - Número único de recepción por empresa
- `IX_RecepcionCompra_OrdenCompraId` - Recepciones por orden de compra
- `IX_RecepcionCompra_BodegaId` - Recepciones por bodega
- `IX_RecepcionCompraDetalle_RecepcionId` - Detalles por recepción

### Nuevos (Fase 7 - Contabilidad)
- `IX_CuentaContable_Empresa_Codigo` - Código único de cuenta por empresa
- `IX_CuentaContable_TipoCuenta` - Cuentas por tipo (ACT, PAS, CAP, ING, GAS, COS)
- `IX_CuentaContable_Nivel` - Cuentas por nivel jerárquico
- `IX_CuentaContable_CuentaPadreId` - Estructura de árbol
- `IX_PeriodoFiscal_Empresa_Anio` - Período fiscal único por empresa/año
- `IX_PeriodoFiscal_Estado` - Períodos fiscales por estado
- `IX_PeriodoContable_Empresa_AnioMes` - Período contable único por empresa/año/mes
- `IX_PeriodoContable_PeriodoFiscalId` - Períodos contables por período fiscal
- `IX_PeriodoContable_Estado` - Períodos contables por estado
- `IX_AsientoContable_Empresa_Periodo_Numero` - Número único de asiento por empresa/período
- `IX_AsientoContable_Fecha` - Asientos por fecha
- `IX_AsientoContable_Estado` - Asientos por estado (BOR, APR, ANU)
- `IX_AsientoContable_TipoAsiento` - Asientos por tipo (DIA, AJU, CIE, APE)
- `IX_AsientoContable_ModuloOrigen` - Asientos por módulo origen
- `IX_AsientoContable_DocumentoOrigenId` - Asientos por documento origen
- `IX_MovimientoContable_AsientoContableId` - Movimientos por asiento
- `IX_MovimientoContable_CuentaContableId` - Movimientos por cuenta
- `IX_MovimientoContable_ClienteId` - Movimientos por cliente
- `IX_MovimientoContable_ProveedorId` - Movimientos por proveedor
- `IX_ConfiguracionContable_Empresa` - Configuración única por empresa
- `IX_CuentaIntegracion_Empresa_Modulo_TipoOperacion_Concepto` - Mapeo único

---

## Notas de Migración

1. **IDs**: FacturacionV2 usa `Guid`, ERP usa `int`
2. **Tabla de mapeo**: `MigracionesIdMapping` registra conversión int→Guid
3. **Entidades extendidas Fase 1**: Cliente (+18 campos), Proveedor (+26 campos)
4. **Nuevas entidades Fase 1**: Banco, Bodega
5. **Entidades extendidas Fase 2**: Inventario (+BodegaId, LoteId, CostoPromedio)
6. **Nuevas entidades Fase 2**: Lote, TrasladoInventario, TrasladoInventarioDetalle, AjusteInventario, AjusteInventarioDetalle
7. **Nuevas entidades Fase 3**: EtapaPipeline, Competidor, Oportunidad, ActividadCRM, NotaOportunidad, HistorialEtapaOportunidad
8. **Nuevas entidades Fase 4**: Departamento, Puesto, Empleado, ContactoEmergencia, ExpedienteDigital, Vacacion, Incapacidad, AccionPersonal, Planilla, DetallePlanilla
9. **Características RRHH Costa Rica**: CCSS 9.67% empleado / 14.50% patrono, Banco Popular 1%, Renta escalonada, Aguinaldo 8.33%, Vacaciones 4.16%, Cesantía, INS
10. **Nuevas entidades Fase 5**: CategoriaActivo, ActivoFijo, DepreciacionActivo, TrasladoActivo
11. **Características Activos Fijos**: Depreciación línea recta, traslados con aprobación, historial por período
12. **Nuevas entidades Fase 6**: Requisicion, RequisicionDetalle, CotizacionProveedor, CotizacionProveedorDetalle, ComparativoCotizacion, ComparativoCotizacionDetalle, EvaluacionProveedor, OrdenCompra, OrdenCompraDetalle, RecepcionCompra, RecepcionCompraDetalle
13. **Características Compras Avanzadas**: Flujo completo de compras (requisición → cotización → comparativo → orden → recepción), evaluación de proveedores con escala 1-5, control de lotes en recepciones
14. **Nuevas entidades Fase 7**: CuentaContable, PeriodoFiscal, PeriodoContable, AsientoContable, MovimientoContable, ConfiguracionContable, CuentaIntegracion
15. **Características Contabilidad**: Plan de cuentas jerárquico (4 niveles), períodos fiscales Oct-Sep (Costa Rica), asientos con flujo de aprobación, integración automática con módulos VEN/COM/INV/CXC/CXP/BAN/NOM/ACT
16. **Preservar datos**: Hay datos en producción que deben mantenerse

---

## Scripts Disponibles

- `Scripts/backup-database.ps1` - Backup PowerShell
- `Scripts/backup-database.sh` - Backup Linux/WSL

---

## Historial de Migraciones ERP

| Migración | Fecha | Descripción |
|-----------|-------|-------------|
| `IntegracionERP_Fase1_CatalogosYMaestros` | 2026-02-07 | Cliente, Proveedor extendidos + Banco, Bodega |
| `IntegracionERP_Fase2_InventarioAvanzado` | 2026-02-07 | Lote, TrasladoInventario, AjusteInventario + Inventario extendido |
| `IntegracionERP_Fase3_CRM` | 2026-02-07 | EtapaPipeline, Competidor, Oportunidad, ActividadCRM, NotaOportunidad, HistorialEtapaOportunidad |
| `IntegracionERP_Fase4_RRHHNomina` | 2026-02-07 | Departamento, Puesto, Empleado, ContactoEmergencia, ExpedienteDigital, Vacacion, Incapacidad, AccionPersonal, Planilla, DetallePlanilla |
| `IntegracionERP_Fase5_ActivosFijos` | 2026-02-07 | CategoriaActivo, ActivoFijo, DepreciacionActivo, TrasladoActivo |
| `IntegracionERP_Fase6_ComprasAvanzadas` | 2026-02-07 | Requisicion, RequisicionDetalle, CotizacionProveedor, CotizacionProveedorDetalle, ComparativoCotizacion, ComparativoCotizacionDetalle, EvaluacionProveedor, OrdenCompra, OrdenCompraDetalle, RecepcionCompra, RecepcionCompraDetalle |
| `IntegracionERP_Fase7_Contabilidad` | 2026-02-07 | CuentaContable, PeriodoFiscal, PeriodoContable, AsientoContable, MovimientoContable, ConfiguracionContable, CuentaIntegracion |
