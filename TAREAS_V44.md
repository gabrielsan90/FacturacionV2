# Análisis Comparativo: Sistema Actual vs Guía v4.4

## 🎉 ESTADO: IMPLEMENTACIÓN v4.4 100% COMPLETADA

**Fecha de actualización:** 2025-12-01

### Resumen de Implementación

| Categoría | Total | Completadas | Pendientes |
|-----------|-------|-------------|------------|
| 🔴 Críticas | 4 | 4 ✅ | 0 |
| 🟠 Importantes | 8 | 8 ✅ | 0 |
| 🟢 Mejoras | 9 | 9 ✅ | 0 |
| **TOTAL** | **21** | **21** | **0** |

### Principales Logros
- ✅ Namespaces XML actualizados a v4.4
- ✅ Campo ProveedorSistemas implementado (obligatorio v4.4)
- ✅ TipoTransaccion en líneas de detalle
- ✅ Validación XSD integrada al flujo de envío
- ✅ APIs externas: CABYS, Actividades Económicas, Tipo de Cambio, Exoneraciones
- ✅ UI Frontend actualizada con campos v4.4
- ✅ Migraciones de base de datos creadas y aplicadas

---

## Resumen Ejecutivo (Original)

El sistema actual tiene una buena base para facturación electrónica v4.4, pero existen campos y funcionalidades que requieren implementación o ajustes para cumplir completamente con la resolución MH-DGT-RES-0027-2024.

---

## 1. CAMPOS NUEVOS v4.4

### 1.1 Campos Implementados ✅

| Campo | Ubicación | Estado |
|-------|-----------|--------|
| ActividadEconomica Emisor | Documento.ActividadEconomica | ✅ Implementado |
| ActividadEconomica Receptor | Documento.ReceptorActividadEconomica | ✅ Implementado |
| PlazoCredito | Documento.PlazoCreditoDias | ✅ Implementado |
| NumeroRegistroMedicamento | DocumentoDetalle.NumeroRegistroMedicamento | ✅ Implementado |
| FormaFarmaceutica | DocumentoDetalle.FormaFarmaceutica | ✅ Implementado |
| NumeroVIN | DocumentoDetalle.NumeroVIN | ✅ Implementado |
| CodigoCabys | DocumentoDetalle.CodigoCabys | ✅ Implementado |
| PartidaArancelaria | DocumentoDetalle.NumeroPartidaArancelaria | ✅ Implementado |

### 1.2 Campos v4.4 - Estado Actualizado

| Campo | Ubicación XML | Estado | Descripción |
|-------|---------------|--------|-------------|
| **ProveedorSistemas** | Raíz del documento | ✅ IMPLEMENTADO | Empresa.ProveedorSistemas* - 3 campos agregados |
| **TipoTransaccion** | LineaDetalle | ✅ IMPLEMENTADO | DocumentoDetalle.TipoTransaccion + Enum TipoTransaccion |
| **FechaPago** | MedioPago (REP) | ✅ IMPLEMENTADO | DocumentoMedioPago.FechaPago |
| **FactorIVA** | Impuesto | ✅ IMPLEMENTADO | DocumentoDetalleImpuesto.FactorIVA |
| **DetalleSurtido** | LineaDetalle | ⏳ PENDIENTE | Para combos/paquetes de productos |
| NumeroVINoSerie (hasta 1000) | LineaDetalle | ⏳ PENDIENTE | Múltiples VINs por línea (vehículos) |

---

## 2. TIPOS DE DOCUMENTOS

### 2.1 Implementados ✅

| Código | Tipo | Estado |
|--------|------|--------|
| 01 | Factura Electrónica | ✅ |
| 02 | Nota de Débito | ✅ |
| 03 | Nota de Crédito | ✅ |
| 04 | Tiquete Electrónico | ✅ |
| 09 | Factura de Exportación | ✅ |
| 10 | Recibo Electrónico Pago (REP) | ✅ Parcial |

### 2.2 Por Verificar/Completar

| Código | Tipo | Estado | Notas |
|--------|------|--------|-------|
| 05 | Nota Débito Compra | ⚠️ Verificar enum | El enum dice 05=NotaDebitoElectronicaCompra pero guía dice 05=FacturaExportación |
| 08 | Factura de Compra | ✅ | Autofacturación |
| 10 | REP | ⚠️ Revisar | Existe entidad ReciboPago pero falta FechaPago en MedioPago |

**IMPORTANTE:** Hay discrepancia en el enum `DocumentoTipo`:
- El enum tiene códigos 5,6,7,8 que no coinciden exactamente con la guía v4.4
- Guía v4.4: 01, 02, 03, 04, 05 (exportación), 09 (compra), 10 (REP)

---

## 3. GENERACIÓN XML

### 3.1 Estado Actual

- Servicio de generación XML existe
- Firma XAdES-EPES implementada con FirmaXadesNet

### 3.2 Verificar/Actualizar

| Elemento | Estado | Acción |
|----------|--------|--------|
| Namespace v4.4 | ⚠️ Verificar | Debe usar `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/` |
| ProveedorSistemas | ❌ Falta | Agregar elemento obligatorio al XML |
| TipoTransaccion | ❌ Falta | Agregar en LineaDetalle |
| Política firma XAdES | ⚠️ Verificar | Debe ser `Resolucion_DGT-R-019-2022.pdf` |

---

## 4. CATÁLOGOS

### 4.1 Catálogos Existentes
- Provincias, Cantones, Distritos
- Unidades de Medida
- Actividades Económicas

### 4.2 Catálogos a Verificar/Agregar

| Catálogo | Estado | Notas |
|----------|--------|-------|
| TipoTransaccion | ❌ Falta | Nuevo catálogo v4.4 (01-13) |
| FormaFarmaceutica | ⚠️ Verificar | 01-Tableta, 02-Cápsula, etc. |
| MediosPago | ⚠️ Actualizar | Agregar 06-SINPE Móvil, 07-Plataformas Digitales |
| CondicionVenta | ⚠️ Actualizar | Agregar 10-Mercancía no nacionalizada |

---

## 5. LISTA DE TAREAS PRIORIZADAS

### 🔴 CRÍTICAS (Bloquean funcionamiento v4.4) - ✅ COMPLETADAS

| # | Tarea | Estado | Archivos Modificados |
|---|-------|--------|---------------------|
| C1 | **Agregar campo ProveedorSistemas a Empresa** | ✅ HECHO | `Empresa.cs` |
| C2 | **Incluir ProveedorSistemas en XML** | ✅ HECHO | `XmlGeneradorService.cs` |
| C3 | **Actualizar namespaces XML a v4.4** | ✅ HECHO | `XmlGeneradorService.cs` |
| C4 | **Corregir enum DocumentoTipo** | ✅ HECHO | `DocumentoTipo.cs`, `DashboardService.cs`, `ReportesService.cs` |

### 🟠 IMPORTANTES (Requeridas para cumplimiento) - ✅ COMPLETADAS

| # | Tarea | Estado | Archivos Modificados |
|---|-------|--------|---------------------|
| I1 | **Agregar TipoTransaccion a DocumentoDetalle** | ✅ HECHO | `DocumentoDetalle.cs` |
| I2 | **Crear enum TipoTransaccion** | ✅ HECHO | `Enums/TipoTransaccion.cs` (nuevo) |
| I3 | **Incluir TipoTransaccion en XML** | ✅ HECHO | `XmlGeneradorService.cs` |
| I4 | **Agregar FechaPago a DocumentoMedioPago** | ✅ HECHO | `DocumentoMedioPago.cs` |
| I5 | **Actualizar catálogo MediosPago** | ✅ YA EXISTÍA | `SeedDb.cs` ya tiene 06, 07 |
| I6 | **Actualizar catálogo CondicionVenta** | ✅ YA EXISTÍA | `SeedDb.cs` ya tiene 08-15 |
| I7 | **Validar actividad económica receptor en FE** | ✅ HECHO | `ValidacionDocumentoService.cs` |
| I8 | **Implementar validación XSD contra esquemas v4.4** | ✅ HECHO | `XsdValidacionService.cs` + DTO |

### Migraciones EF Core: ✅ CREADAS
- Archivo: `Migrations/20251201061235_CamposV44.cs`
  - Campos: ProveedorSistemas* (3), TipoTransaccion, FechaPago, FactorIVA
- Archivo: `Migrations/AgregarDetalleSurtidoYCamposV44.cs`
  - Campo adicional: DetalleSurtido
- Archivo: `Migrations/20251201133721_TareasV44_M6M7M8M9.cs`
  - Tabla: FormasFarmaceuticas (catálogo M7)
  - Tabla: DocumentoDetalleVINs (múltiples VINs M6)
  - Campo: FechaVencimientoRetencion en Documentos (M9)
  - FK: DocumentoDetalle -> FormaFarmaceutica (M7)

### 🟢 MEJORAS (Optimizaciones) - ✅ COMPLETADAS

| # | Tarea | Estado | Archivos Creados |
|---|-------|--------|------------------|
| M1 | **DetalleSurtido en DocumentoDetalle** | ✅ HECHO | `DocumentoDetalle.cs` |
| M2 | **Integración API CABYS** | ✅ HECHO | `ICabysService.cs`, `CabysService.cs`, `CabysController.cs`, `CabysDTO.cs` |
| M3 | **Integración API Actividades Económicas** | ✅ HECHO | `IActividadEconomicaService.cs`, `ActividadEconomicaService.cs`, `ActividadesEconomicasController.cs`, `ActividadEconomicaDTO.cs` |
| M4 | **Integración API Tipo de Cambio BCCR** | ✅ HECHO | `ITipoCambioBCCRService.cs`, `TipoCambioBCCRService.cs`, `TipoCambioController.cs` |
| M5 | **Integración API Exoneraciones** | ✅ HECHO | `IExoneracionService.cs`, `ExoneracionService.cs`, `ExoneracionesController.cs`, `ExoneracionDTO.cs` |
| M6 | Soporte múltiples VINs por línea (hasta 1000) | ✅ HECHO | `DocumentoDetalleVIN.cs`, `DocumentoDetalle.cs`, `XmlGeneradorService.cs`, `DataContext.cs` |
| M7 | Crear catálogo FormaFarmaceutica completo | ✅ HECHO | `FormaFarmaceutica.cs`, `SeedDb.cs`, `DocumentoDetalle.cs`, `DataContext.cs` |
| M8 | Validación de cálculos previo al envío | ✅ HECHO | `IValidacionCalculosService.cs`, `ValidacionCalculosService.cs`, `DocumentoHaciendaService.cs`, `Program.cs` |
| M9 | Almacenamiento de comprobantes 5 años | ✅ HECHO | `Documento.cs` (campo FechaVencimientoRetencion) |

### 📋 Archivos de Documentación Generados
- `IMPLEMENTACION_VALIDACION_XSD.md` - Guía de validación XSD
- `API_EXTERNA_INTEGRACIONES.md` - Documentación de APIs externas
- `IMPLEMENTACION_TAREAS_V44_M6-M9.md` - Documentación completa de implementación M6-M9 (NUEVO 2025-12-01)

---

## 6. PLAN DE IMPLEMENTACIÓN SUGERIDO

### Fase 1: Críticos (1-2 días)
1. Agregar ProveedorSistemas a Empresa
2. Actualizar generación XML con ProveedorSistemas
3. Verificar namespaces v4.4
4. Revisar y corregir enum DocumentoTipo

### Fase 2: Importantes (3-5 días)
1. Agregar TipoTransaccion a DocumentoDetalle
2. Crear enum TipoTransaccion
3. Actualizar generación XML
4. Agregar FechaPago a medios de pago
5. Actualizar catálogos

### Fase 3: Mejoras (Continuo)
1. Integraciones con APIs de Hacienda
2. Validaciones adicionales
3. Campos opcionales nuevos

---

## 7. ARCHIVOS PRINCIPALES A MODIFICAR

### Entidades (Shared)
- `Facturacion.Shared/Entities/Empresa.cs` - Agregar ProveedorSistemas
- `Facturacion.Shared/Entities/DocumentoDetalle.cs` - Agregar TipoTransaccion
- `Facturacion.Shared/Entities/DocumentoMedioPago.cs` - Agregar FechaPago
- `Facturacion.Shared/Enums/DocumentoTipo.cs` - Verificar códigos

### Enums (Shared)
- Crear: `Facturacion.Shared/Enums/TipoTransaccion.cs`

### Servicios (Backend)
- Servicio generación XML - Incluir nuevos campos
- Servicio validación - Agregar validaciones v4.4
- Servicio firma - Verificar política

### Base de Datos
- Nueva migración para campos agregados

---

## 8. NOTAS ADICIONALES

### Fechas Límite v4.4
- **01/09/2025**: Obligatoria versión 4.4
- **01/06/2025**: CABYS obligatorio
- **01/04/2025**: ActividadEconomica receptor obligatoria en FE

### URLs de Referencia
- Producción: `https://api.comprobanteselectronicos.go.cr/recepcion/v1`
- Sandbox: `https://api.comprobanteselectronicos.go.cr/recepcion-sandbox/v1`
- Token Prod: `https://idp.comprobanteselectronicos.go.cr/auth/realms/rut/protocol/openid-connect/token`
- Token Stag: `https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token`

---

*Documento generado: 2025-11-30*
*Basado en: guia-facturacion-electronica-cr-v44.md*
