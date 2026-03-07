# 📋 ESPECIFICACIÓN DETALLADA
## Sistema de Facturación Electrónica v4.4 - Costa Rica

**Fecha de creación:** 21 de noviembre de 2025
**Versión:** 1.0
**Framework:** .NET 9.0
**Base de datos:** SQL Server

---

## 🎯 Descripción General

Sistema web de facturación electrónica multi-empresa para Costa Rica, cumpliendo con la normativa de Hacienda versión 4.4. Diseñado para un SuperUser que administra múltiples empresas, cada una con sus propios usuarios, clientes, documentos e inventario.

---

## 1️⃣ ARQUITECTURA Y TECNOLOGÍA

### Stack Tecnológico

- **Backend**: .NET 9 Web API
- **Frontend**: ASP.NET Core Razor Pages (.NET 9)
- **Base de Datos**: SQL Server remoto
  - Connection String: `Server=www.smarttechcr.com;Database=ATFE;User Id=sanchez;Password=Granados1990*;TrustServerCertificate=True;`
- **Autenticación Backend**: JWT
- **Autenticación Frontend**: Cookies
- **Templates UI**:
  - Light: https://ajoydas.net/laxom/side-menu-white/index.html
  - Dark: https://ajoydas.net/laxom/dark-version/index.html (con cambio en barra superior)

### Características Técnicas

- **Responsive**: Funciona perfectamente en escritorio, tablets y móviles
- **Idioma**: Solo Español
- **DataTables**: jQuery DataTables para todas las listas
- **API Externa**: Sin API pública para terceros
- **Respaldos**: Manuales por administrador del servidor

---

## 2️⃣ MODELO DE USUARIOS Y EMPRESAS

### Tipos de Usuarios

1. **SuperUser**
   - Control total del sistema
   - Puede crear, modificar y eliminar empresas
   - Puede crear usuarios y asignarlos a múltiples empresas
   - Puede ver y gestionar todas las empresas
   - Puede actualizar catálogos de Hacienda

2. **Administrador de Empresa**
   - Gestión completa de su empresa
   - Puede crear usuarios dentro de su empresa
   - Puede asignar roles a usuarios (excepto crear otros Admins)
   - Solo puede modificar la empresa en la que está logueado
   - No puede crear nuevas empresas

3. **Otros Roles** (Contador, Facturador, Vendedor, Inventarista, Consultor)

### Reglas de Usuarios y Empresas

- **Usuario en múltiples empresas**: Tiene el MISMO rol en todas ellas
- **Admin crea usuarios**: Puede crear usuarios Y asignarles roles disponibles
- **Usuarios ilimitados**: Cada empresa puede crear cuantos usuarios necesite sin límites
- **Cambio de empresa**:
  - Selector en barra superior (si usuario tiene más de una empresa)
  - Cambio instantáneo sin perder navegación actual
  - SuperUser puede ver todas las empresas

---

## 3️⃣ ROLES Y PRIVILEGIOS

### Roles Globales Predefinidos

1. **SuperUser** - Control total del sistema
2. **Administrador de Empresa** - Gestión completa de su empresa
3. **Contador** - Acceso a reportes financieros, documentos, gastos
4. **Facturador** - Crear y gestionar documentos electrónicos
5. **Vendedor** - Crear facturas, gestionar clientes
6. **Inventarista** - Gestión de productos, inventario, traslados
7. **Consultor** - Solo lectura de reportes y documentos

### Sistema de Privilegios

- **Granularidad**: Por acciones CRUD (Crear, Ver, Editar, Eliminar)
- **Alcance**: Los roles son globales (mismo catálogo para todas las empresas)
- **Asignación**: SuperUser define los privilegios de cada rol

### Módulos con Control de Privilegios CRUD

1. **Documentos Electrónicos** (Crear, Ver, Editar borradores, Anular)
2. **Clientes** (CRUD completo)
3. **Proveedores** (CRUD completo)
4. **Productos** (CRUD completo)
5. **Gastos** (CRUD completo)
6. **Inventario** (Ver, Crear movimientos, Ajustar)
7. **Reportes** (Ver, Exportar)
8. **Usuarios** (CRUD completo - solo Admin y SuperUser)
9. **Empresas** (CRUD completo - solo SuperUser)
10. **Configuración** (Ver, Editar - solo Admin y SuperUser)
11. **Catálogos Hacienda** (Ver, Editar - solo SuperUser)
12. **Recepción de Documentos** (Ver, Procesar)
13. **REP** (CRUD completo)
14. **Sucursales/Terminales** (CRUD completo - solo Admin y SuperUser)

---

## 4️⃣ DOCUMENTOS ELECTRÓNICOS

### Tipos de Documentos Soportados

1. **FE** - Factura Electrónica
2. **TE** - Tiquete Electrónico
3. **NC** - Nota de Crédito Electrónica
4. **ND** - Nota de Débito Electrónica
5. **FEC** - Factura Electrónica de Compra
6. **FEE** - Factura Electrónica de Exportación
7. **MR** - Mensaje Receptor (confirmación/rechazo)
8. **REP** - Recibo Electrónico de Pago

### Flujo de Creación de Documentos

1. Usuario completa el documento en una sola pantalla
2. Vista previa del documento
3. Usuario confirma
4. Sistema firma digitalmente y envía a Hacienda automáticamente
5. Se muestra la respuesta de Hacienda
6. Se envía correo automático al cliente con XML y PDF

### Interfaz de Creación

- **Diseño**: Una sola pantalla con todo visible (scrolleable)
- **Selector de tipo**: Arriba de la página
- **Campos dinámicos**: Cambian según el tipo de documento seleccionado
- **Estructura**:
  - Selector de tipo de documento
  - Sección de encabezado (cliente, fecha, moneda, condición de venta, etc.)
  - Tabla de líneas/productos
  - Sección de totales
  - Botones: Vista previa, Guardar borrador, Cancelar

### Agregar Productos/Líneas

- Campo de búsqueda de producto (por código SKU o nombre)
- Seleccionar producto → se agrega a tabla con datos pre-cargados
- Editable en tabla: cantidad, precio, descuento
- Botón para agregar línea manual (sin producto del catálogo)
- Se pueden eliminar líneas

### Estados de Documentos

1. **Borrador** - guardado pero no enviado a Hacienda
2. **Pendiente** - listo para enviar pero no firmado
3. **Procesando** - en proceso de firma y envío
4. **Aceptado** - aceptado por Hacienda exitosamente
5. **Rechazado** - rechazado por Hacienda (queda como fallido, crear nuevo)
6. **Anulado** - documento anulado (con nota de crédito)

### Manejo de Errores de Conexión

- **Cola de procesamiento**: Documentos van a una cola
- **Proceso background**: Envía documentos automáticamente
- **Reintentos automáticos**: Si falla, reintenta automáticamente
- **Estado en tiempo real**: Usuario ve el estado actualizado

### Consecutivos

- **Formato**: Sucursal(3)-Terminal(5)-Secuencial(8)-Tipo(2)
- **Configuración**: Manual inicial por el Admin (útil para migraciones)
- **Incremento**: Automático después del inicial
- **Rechazados**: El consecutivo se pierde ("quema"), posibles saltos en numeración

### Documentos sin Cliente

- **Facturas (FE, FEC, FEE)**: Requieren cliente obligatorio
- **Tiquetes (TE)**: Pueden ir sin cliente específico
- **Validación**: Según tipo de documento

### Observaciones

- **Públicas**: Aparecen en PDF y XML, se envían a Hacienda
- **Internas**: Solo para uso interno, no aparecen en el documento

---

## 5️⃣ NOTAS DE CRÉDITO Y DÉBITO

### Creación de NC/ND

- **Opción 1**: Desde el documento original (datos pre-cargados, referencia automática)
- **Opción 2**: Creación independiente con vinculación manual
- **Ambas opciones disponibles**

### Anulación de Documentos

- Usuario selecciona "Anular" en documento aceptado
- Sistema crea y envía NC automáticamente por el total
- Documento original queda en estado "Anulado"
- Si era mercadería, regresa al stock automáticamente
- Proceso automático sin pasos adicionales

---

## 6️⃣ CLIENTES

### Tipos de Identificación Soportados

- Cédula física (9 dígitos)
- Cédula jurídica (10 dígitos)
- DIMEX (11-12 dígitos)
- NITE (10 dígitos)
- Pasaporte
- Identificación extranjera

### Validación

- **Formato**: Validación automática de formato
- **Servicios externos**: No se consulta Registro Civil/Hacienda

### Información de Cliente

- Tipo y número de identificación
- Nombre / Razón social
- Dirección estructurada (Provincia → Cantón → Distrito → Otras señas)
- Múltiples teléfonos (uno marcado como principal)
- Múltiples correos (uno marcado como principal)
- Condición de venta por defecto (pre-carga al seleccionar cliente)
- Activo/Inactivo

### Direcciones

- **Catálogo estructurado**: Selectores en cascada
- **División territorial**: Provincia → Cantón → Distrito
- **Catálogo pre-cargado** de Costa Rica
- **Otras señas**: Campo adicional de texto libre

### Historial

- **No hay historial en ficha**: Se usa el reporte de documentos con filtro por cliente

---

## 7️⃣ PROVEEDORES

### Información de Proveedor

- Todo lo de clientes, MÁS:
- Días de crédito
- Límite de crédito
- Cuenta bancaria
- Información de contacto comercial
- Categoría de proveedor

---

## 8️⃣ PRODUCTOS Y SERVICIOS

### Tipos de Productos

- **Mercadería**: Afecta stock
- **Servicios**: No afecta stock

### Información de Producto

- **Código SKU**: Único y obligatorio
  - Generación automática por defecto (ej: PROD0001, PROD0002)
  - Usuario puede modificarlo manualmente
  - Validación de unicidad
- **Nombre/Descripción**
- **Código CAByS**:
  - Búsqueda en catálogo integrado de Hacienda
  - O ingreso manual con validación de formato (13 dígitos)
  - Híbrido (ambas opciones)
- **Unidad de medida**: Del catálogo oficial de Hacienda
- **Precio de venta**: Único (5 decimales)
  - Modificable manualmente al facturar si es necesario
- **Impuestos múltiples configurables**:
  - IVA (13%, 4%, 2%, 1%, Exento)
  - Impuesto sobre consumo (% configurable)
  - Otros impuestos específicos
  - Cada producto puede tener múltiples impuestos simultáneos
- **Stock actual** (solo para mercadería)
- **Tipo**: Mercadería o Servicio
- **Activo/Inactivo**

### Organización

- **Sin categorías**: Lista simple de productos sin clasificación

### Códigos de Barras

- **No soportado**: Solo búsqueda por código SKU o nombre

---

## 9️⃣ INVENTARIO

### Modelo de Inventario

- **Stock centralizado**: Un inventario general de la empresa
- **Sin stock por sucursal**: No hay separación por ubicación
- **Sin traslados**: No se necesitan movimientos entre sucursales

### Control de Stock Insuficiente

- **Advertencia pero permite**: Muestra alerta de stock insuficiente
- Permite continuar si usuario confirma
- Stock puede quedar negativo
- Flexible para casos especiales

### Movimientos de Inventario

1. **Entrada por Compra** - vinculado a documentos recibidos
2. **Salida por Venta** - automático al facturar mercadería
3. **Ajuste de Inventario** - correcciones manuales (+ o -)
4. **Inventario Inicial** - al dar de alta productos
5. **Merma/Pérdida** - productos dañados/perdidos
6. **Devolución de Cliente** - vinculado a NC, regresa al stock
7. **Devolución a Proveedor** - vinculado a ND, reduce stock

### Precisión Decimal

- **Cantidades**: 3 decimales (para productos fraccionables)

---

## 🔟 GASTOS

### Modelo de Gastos

- **Vinculados a documentos**: Cuando se recibe documento electrónico de proveedor, automáticamente se crea gasto
- **Gastos manuales**: También se pueden crear gastos sin documento asociado
- **Información**: Fecha, proveedor, monto, concepto, categoría, documento asociado (si aplica)

---

## 1️⃣1️⃣ RECEPCIÓN DE DOCUMENTOS

### Mensaje Receptor (MR)

- **Aceptación automática**: Al recibir documento XML se procesa automáticamente
- Se envía MR de aceptación a Hacienda automáticamente
- Se crea el gasto/compra en el sistema automáticamente
- Proceso completamente automático

---

## 1️⃣2️⃣ RECIBO ELECTRÓNICO DE PAGO (REP)

### Creación de REP

- **Manual**: Usuario crea REP manualmente
- Selecciona cliente y facturas que está pagando
- Indica forma de pago y monto
- Se envía a Hacienda
- No hay generación automática

---

## 1️⃣3️⃣ FORMAS DE PAGO Y MEDIOS

### Formas de Pago

- **Múltiples formas por documento**: Un documento puede tener varias formas de pago
- Distribución de montos entre ellas
- Ejemplo: ₡50,000 efectivo + ₡50,000 tarjeta

### Información Capturada

- **Solo medio y monto**: Se selecciona medio de pago y monto
- Sin campos adicionales (referencia, últimos 4 dígitos, etc.)

### Condiciones de Venta

- **Según cliente**: Cada cliente tiene condición de venta por defecto
- Se pre-carga al seleccionar cliente
- Modificable en el documento si es necesario
- Catálogo de Hacienda (01-Contado, 02-Crédito, 03-Consignación, etc.)

---

## 1️⃣4️⃣ DESCUENTOS

### Aplicación de Descuentos

- **Solo por línea**: Cada producto/servicio puede tener su descuento individual
- En porcentaje (%) o monto fijo
- No hay descuento general al documento

---

## 1️⃣5️⃣ MONEDAS

### Soporte Multi-Moneda

- **Documentos en cualquier moneda**: Del catálogo de Hacienda
- **Tipo de cambio**: Consulta automática del Banco Central de Costa Rica (BCCR)
  - Actualización diaria automática
  - Modificable manualmente si es necesario
  - Precisión: 5 decimales
- **Reportes**: Pueden mostrar en moneda original o convertido a colones
- **Inventario**: Valorado en colones

---

## 1️⃣6️⃣ SUCURSALES Y TERMINALES

### Sucursales

- **Información básica**:
  - Código de sucursal (3 dígitos para consecutivos)
  - Nombre de la sucursal
  - Dirección estructurada (Provincia, Cantón, Distrito, Otras señas)
  - Activa/Inactiva

### Terminales/POS

- **Información mínima**:
  - Código de terminal (5 dígitos para consecutivos)
  - Sucursal a la que pertenece
- **Relación**: Cada terminal pertenece a una sucursal
- **Consecutivos**: Formato Sucursal-Terminal-Secuencial-Tipo

---

## 1️⃣7️⃣ CONFIGURACIÓN DE EMPRESA

### Información Requerida

1. **Identificación**: Tipo y número (física/jurídica)
2. **Nombre comercial** y **Razón social**
3. **Dirección** estructurada (Provincia, Cantón, Distrito, Otras señas)
4. **Múltiples teléfonos** (uno marcado como principal)
5. **Múltiples emails** (uno marcado como principal)
6. **Logo** (imagen para documentos PDF)
7. **Actividades económicas**:
   - Múltiples actividades (una principal, otras secundarias)
   - Seleccionable por documento
8. **Certificado digital** (.p12) y PIN:
   - Se almacena encriptado en la base de datos
   - Se ingresa el PIN una vez al configurar
   - Compatible con todos los proveedores (validación básica .p12)
9. **Usuario y contraseña** de Hacienda (ATV/Producción)
10. **Ambiente**: Configurable por empresa (Pruebas ATV / Producción)
    - URLs de API diferentes según ambiente
11. **Configuración de correo SMTP**: Servidor, puerto, usuario, contraseña

---

## 1️⃣8️⃣ CATÁLOGOS DE HACIENDA

### Catálogos Incluidos

- Monedas
- Formas de pago
- Tipos de documento
- Tipos de referencia
- Condiciones de venta
- Unidades de medida
- Códigos de impuesto
- Provincias, Cantones, Distritos
- Códigos CAByS (Clasificador de Bienes y Servicios)

### Gestión

- **Pre-cargados en el sistema**
- **Actualizables por SuperUser**: Puede agregar/editar valores
- **No descarga automática** de Hacienda

---

## 1️⃣9️⃣ DASHBOARD

### KPIs e Indicadores

1. **Ventas del período** (día/semana/mes)
2. **Documentos emitidos** (cantidad por tipo)
3. **Documentos pendientes de envío** a Hacienda
4. **Top 10 productos más vendidos**
5. **Top 10 clientes** (por monto)
6. **Gastos del período**
7. **Gráficos de ventas** (tendencia mensual)
8. **Estado de inventario** (productos con stock bajo)
9. **Documentos rechazados** por Hacienda

---

## 2️⃣0️⃣ REPORTES

### Reportes Incluidos

1. **Reporte de Ventas** (por período, cliente, producto, usuario)
2. **Reporte de Documentos Emitidos** (filtrado por tipo, estado, fecha)
3. **Reporte de Compras/Gastos** (por período, proveedor, categoría)
4. **Reporte de Inventario** (existencias actuales, valorización)
5. **Reporte de Clientes** (listado, estados de cuenta)
6. **Reporte de Impuestos** (IVA cobrado, impuestos por período)
7. **Estado de cuenta por cliente** (facturas pendientes, pagos)
8. **Libro de ventas** (resumen fiscal)
9. **Libro de compras** (resumen fiscal)

### Exportación

- **Excel (.xlsx)**: Todos los reportes
- **PDF**: Todos los reportes
- Usuario elige el formato al exportar

### Filtros del Reporte de Documentos

1. **Rango de fechas** (desde/hasta)
2. **Tipo de documento** (FE, TE, NC, ND, FEC, FEE, MR, REP)
3. **Estado** (Borrador, Aceptado, Rechazado, Anulado, etc.)
4. **Cliente** (búsqueda/selector)
5. **Número de documento** (clave o consecutivo)
6. **Sucursal/Terminal**
7. **Usuario que creó**
8. **Moneda**
9. **Rango de monto** (desde/hasta)

---

## 2️⃣1️⃣ BÚSQUEDA Y FILTRADO

### DataTables jQuery

- **Búsqueda general** en múltiples campos
- **Filtros específicos** por columna (fecha desde/hasta, estado, tipo, etc.)
- **Ordenar** por cualquier columna
- **Paginación configurable** (10, 25, 50, 100 registros)
- **Exportar resultados** filtrados (Excel/PDF)

---

## 2️⃣2️⃣ CORREO ELECTRÓNICO

### Envío Automático

- **Al aprobar documento**: Se envía automáticamente por correo al cliente
- **Adjuntos**: XML y PDF del documento
- **Copia**: Se envía también a la empresa emisora
- **Plantilla**: Fija del sistema, no personalizable
- **Mensaje estándar**: "Adjunto encontrará el documento [Tipo] [Número]"

---

## 2️⃣3️⃣ NOTIFICACIONES

### Notificaciones Internas

- **Icono de campana** en barra superior
- **Alertas para**:
  - Documentos rechazados por Hacienda
  - Certificado digital próximo a vencer
  - Stock bajo en productos
- **Marcar como leídas**
- No hay notificaciones en tiempo real (SignalR)
- No configurables por usuario

---

## 2️⃣4️⃣ IMPRESIÓN Y DESCARGA

### Formatos Disponibles

- **PDF estándar**: Formato oficial con código QR y logo de empresa
- **XML**: Descarga del archivo XML
- **Botones**: "Descargar PDF", "Descargar XML", "Imprimir"

### Contenido del PDF

- Logo de la empresa
- Información completa del documento
- Código QR (requerido por Hacienda)
- Formato oficial según normativa

---

## 2️⃣5️⃣ AUDITORÍA

### Registro Completo

- **Todas las acciones**: Crear, Editar, Eliminar
- **Historial de cambios de valores**: Qué cambió, de qué valor a qué valor
- **Accesos**: Quién accedió a qué y cuándo
- **Usuario y fecha/hora** en cada operación
- **Máximo control** y trazabilidad

---

## 2️⃣6️⃣ SEGURIDAD

### Autenticación

- **Backend**: JWT tokens
- **Frontend**: Cookies
- **Sesión**: Expira después de tiempo fijo (ej: 8 horas) desde el login

### Contraseñas

- **Política básica**: Mínimo 6 caracteres, sin requisitos especiales
- **Sin expiración**: No caducan

### Bloqueo de Cuenta

- **Automático**: Después de 5 intentos fallidos
- **Duración**: 15 minutos
- **Registro**: Se registra en auditoría
- **Prevención**: Ataques de fuerza bruta

### Validación de Emails

- **Solo formato**: Verifica formato correcto (contiene @, dominio)
- **No verifica existencia**: No se envían correos de confirmación

---

## 2️⃣7️⃣ INTERFAZ DE USUARIO

### Temas

- **Light y Dark**: Dos opciones de tema
- **Preferencia por usuario**: Cada usuario elige su tema preferido
- **Independiente de empresa**: No depende de configuración de empresa

### Estructura del Menú

**Principal**
- Dashboard

**Documentos**
- Crear Documento
- Reporte de Documentos
- Documentos Recibidos
- Recibos de Pago (REP)

**Catálogos**
- Clientes
- Proveedores
- Productos
- Gastos

**Inventario**
- Movimientos de Inventario
- Reporte de Inventario

**Reportes**
- Reportes (con submenú)

**Configuración** (solo Admin/SuperUser)
- Empresas (solo SuperUser)
- Sucursales y Terminales
- Configuración de Empresa
- Usuarios
- Roles y Privilegios
- Catálogos de Hacienda

### Barra Superior

- Logo/nombre del sistema
- Selector de empresa (si usuario tiene más de una)
- Icono de notificaciones (campana)
- Selector de tema (Light/Dark)
- Usuario logueado y opción de cerrar sesión

---

## 2️⃣8️⃣ PRECISIÓN DECIMAL

### Estándar Costa Rica

- **Precios**: 5 decimales (permite precisión en cálculos)
- **Cantidades**: 3 decimales (productos fraccionables)
- **Montos totales**: 2 decimales (moneda)
- **Tipos de cambio**: 5 decimales

---

## 2️⃣9️⃣ ELIMINACIÓN DE REGISTROS

### Soft Delete

- **No se borran físicamente**: Los registros se marcan como "Inactivos" o "Eliminados"
- **Recuperables**: Se pueden recuperar si es necesario
- **Integridad referencial**: Mantiene relaciones con documentos históricos
- **Más seguro**: Previene pérdida de datos

---

## 3️⃣0️⃣ IMPORTACIÓN/EXPORTACIÓN DE DATOS

### Carga de Datos

- **Solo manual**: Todos los registros se crean manualmente uno por uno
- **No hay importación masiva**: No se permite cargar desde Excel/CSV
- **Exportación**: Sí, desde reportes a Excel/PDF

---

## 3️⃣1️⃣ AYUDA Y DOCUMENTACIÓN

### Sin Ayuda Integrada

- No hay sistema de ayuda dentro de la aplicación
- Se puede proporcionar manual externo en PDF si es necesario
- Sin tooltips, sin tutoriales integrados

---

## 📊 RESUMEN DE FUNCIONALIDADES PRINCIPALES

✅ **0. Login y Cerrar Sesión**
✅ **1. Dashboard** con 9 KPIs
✅ **2. Creación de Documentos** (8 tipos) en una pantalla dinámica
✅ **3. Reporte de Documentos** con 9 filtros
✅ **4. Clientes** con identificaciones CR completas
✅ **5. Proveedores** con campos adicionales
✅ **6. Productos** (mercadería y servicios)
✅ **7. Recepción de Documentos** automática con MR
✅ **8. Recibo Electrónico de Pago (REP)** manual
✅ **9. Gastos** vinculados a documentos
✅ **10. Catálogos de Hacienda** actualizables
✅ **11. Inventario** centralizado con 7 tipos de movimientos
✅ **12. Reportes** (9 tipos) exportables
✅ **13. Configuración** completa de empresa
✅ **14. Usuarios** ilimitados
✅ **15. Roles** (7 predefinidos)
✅ **16. Privilegios** CRUD por módulo
✅ **17. Correo** automático con documentos

---

## 🎨 EXPERIENCIA DE USUARIO

- ✅ Responsive completo (móvil, tablet, escritorio)
- ✅ Tema Light/Dark por usuario
- ✅ DataTables con búsqueda avanzada
- ✅ Notificaciones en campana
- ✅ Cambio rápido de empresa
- ✅ Cola de procesamiento para documentos
- ✅ Vista previa antes de enviar
- ✅ Envío automático de correos
- ✅ Auditoría completa

---

## 🔧 CONSIDERACIONES TÉCNICAS

### Precisión Numérica

- Usar tipo `decimal` para todos los montos
- Evitar `float` o `double` por problemas de precisión
- Mantener 5 decimales en cálculos intermedios
- Redondear solo al final según normativa

### Validaciones

- Validación en cliente (JavaScript) para UX
- Validación en servidor (C#) para seguridad
- Validación de formato de identificaciones
- Validación de montos y totales

### Integración con Hacienda

- URLs diferentes para ATV y Producción
- Autenticación con certificado digital
- Firma digital XAdES-EPES
- Reintentos automáticos en caso de fallo
- Cola de procesamiento background

### Base de Datos

- Índices en campos de búsqueda frecuente
- Soft delete para todos los registros principales
- Auditoría completa con triggers o EF Core Interceptors
- Backup manual por administrador del servidor

---

## 📅 TIMELINE DE IMPLEMENTACIÓN SUGERIDO

### Fase 1: Infraestructura Base (2-3 semanas)
- Configuración de proyectos (.NET 9)
- Base de datos y migraciones iniciales
- Autenticación (JWT + Cookies)
- Layout base (Light/Dark themes)
- Gestión de usuarios y roles

### Fase 2: Módulos Básicos (3-4 semanas)
- Gestión de empresas
- Catálogos de Hacienda
- Clientes y proveedores
- Productos y servicios
- Sucursales y terminales

### Fase 3: Inventario (2 semanas)
- Movimientos de inventario
- Control de stock
- Reportes de inventario

### Fase 4: Documentos Electrónicos (4-5 semanas)
- Interfaz de creación de documentos
- Generación de XML según v4.4
- Firma digital XAdES
- Integración con API de Hacienda
- Manejo de estados y errores

### Fase 5: Documentos Recibidos y Gastos (2 semanas)
- Recepción de XML
- Mensaje Receptor automático
- Gestión de gastos
- REP (Recibo Electrónico de Pago)

### Fase 6: Reportes y Dashboard (2-3 semanas)
- Dashboard con KPIs
- 9 reportes principales
- Exportación Excel/PDF
- Filtros avanzados con DataTables

### Fase 7: Complementos (2 semanas)
- Envío de correos automáticos
- Notificaciones internas
- Auditoría completa
- Consulta de tipos de cambio BCCR

### Fase 8: Testing y Refinamiento (2-3 semanas)
- Pruebas con ATV de Hacienda
- Corrección de errores
- Optimizaciones de rendimiento
- Documentación de usuario

**TOTAL ESTIMADO: 19-24 semanas (4.5-6 meses)**

---

## 📝 NOTAS FINALES

Este documento representa la especificación completa del sistema desarrollada a través de 81 preguntas detalladas que cubrieron todos los aspectos funcionales, técnicos y de experiencia de usuario del sistema.

La especificación está lista para ser entregada a un equipo de desarrollo para su implementación.

### Documentos Relacionados

- `BACKEND_PATTERNS.md` - Patrones de desarrollo backend
- `FRONTEND_PATTERNS.md` - Patrones de desarrollo frontend
- `SHARED_PATTERNS.md` - Patrones de capa compartida
- `SECURITY_CONFIG.md` - Configuración de seguridad
- `NAMING_CONVENTIONS.md` - Convenciones de nomenclatura
- `ARCHITECTURE_GUIDE.md` - Guía de arquitectura del proyecto
- `Facturacion.Shared/Entities/DocumentosElectronicos/V44/README.md` - Documentación de clases v4.4

---

**Versión del documento:** 1.0
**Última actualización:** 21 de noviembre de 2025
**Responsable:** Sistema de Facturación Electrónica CR v4.4
**Estado:** Aprobado y listo para desarrollo
