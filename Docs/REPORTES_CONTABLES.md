# Reportes Contables Financieros

## Estado Actual del Módulo Contabilidad

### Páginas existentes (7)
| Página | Descripción |
|--------|-------------|
| Plan de Cuentas | Catálogo con TreeView jerárquico (4 niveles) |
| Períodos Contables | Gestión de períodos (apertura/cierre) |
| Asientos Contables | CRUD de asientos + movimientos |
| Mayores Contables | Libro mayor por cuenta |
| Plantillas de Asientos | Plantillas reutilizables |
| Cuentas de Integración | Mapeo módulo → cuenta contable |
| Configuración Contable | Auto-generación, auto-aprobación |

### Reportes operativos existentes
- Reporte de Ventas, Gastos, Productos, Documentos Emitidos/Recibidos
- Flujo de Caja (en Dashboard)
- Antigüedad de Saldos CxC y CxP
- Estado de Cuenta por Proveedor

## Reportes Financieros a Implementar

| # | Reporte | Prioridad | Descripción |
|---|---------|-----------|-------------|
| 1 | **Balanza de Comprobación** | CRITICA | Saldos deudor/acreedor por cuenta en un período. Base para todos los demás reportes. |
| 2 | **Balance General** | CRITICA | Estado de Situación Financiera: Activos = Pasivos + Capital (NIC 1) |
| 3 | **Estado de Resultados** | CRITICA | Ingresos - Costos - Gastos = Utilidad/Pérdida del período (NIC 1) |
| 4 | **Libro Diario** | ALTA | Todos los asientos contables en orden cronológico con sus movimientos |
| 5 | **Flujo de Efectivo** | MEDIA | Estado de flujos de efectivo método directo/indirecto (NIC 7) |
| 6 | **Balance de Sumas y Saldos** | MEDIA | Variación de la balanza con totales de movimientos deudor/acreedor + saldos |

## Arquitectura de Implementación

### Capa Shared (DTOs)
- `ReportesContablesDTO.cs` — DTOs para todos los reportes

### Capa Backend
- `IReportesContablesService` — Interface del servicio
- `ReportesContablesService` — Lógica de generación de reportes (queries a MovimientosContables + CuentasContables)
- `ReportesContablesController` — Endpoints API

### Capa Frontend
- `Pages/Contabilidad/BalanzaComprobacion.cshtml` — Balanza de Comprobación
- `Pages/Contabilidad/BalanceGeneral.cshtml` — Balance General
- `Pages/Contabilidad/EstadoResultados.cshtml` — Estado de Resultados
- `Pages/Contabilidad/LibroDiario.cshtml` — Libro Diario
- `Pages/Contabilidad/FlujoEfectivo.cshtml` — Flujo de Efectivo
- `Pages/Contabilidad/BalanceSumasySaldos.cshtml` — Balance de Sumas y Saldos

## Notas Técnicas
- Los datos base ya existen: `AsientosContables`, `MovimientosContables`, `CuentasContables` con `SaldoActual`
- Las cuentas tienen `TipoCuenta` (ACT/PAS/CAP/ING/GAS/COS) y `Naturaleza` (D/A)
- Los períodos contables tienen `FechaInicio`/`FechaFin` y `Estado`
- La generación de reportes debe filtrar solo asientos con `Estado = "APR"` (aprobados)
- Todos los reportes deben soportar exportación a Excel y PDF (QuestPDF)
