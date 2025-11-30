# DOCUMENTACION OFICIAL CAMPOS OBLIGATORIOS Y OPCIONALES
## Facturacion Electronica Version 4.4 - Costa Rica

**Fecha de investigacion:** 28 de noviembre de 2025
**Version:** 4.4
**Marco Legal:** Resolucion MH-DGT-RES-0027-2024
**Vigencia Obligatoria:** 1 de septiembre de 2025

---

## TABLA DE CONTENIDOS

1. [Marco Legal y Fechas Importantes](#marco-legal-y-fechas-importantes)
2. [Sistema de Condiciones de Campos](#sistema-de-condiciones-de-campos)
3. [Tipos de Documentos Electronicos](#tipos-de-documentos-electronicos)
4. [Estructura General de Documentos](#estructura-general-de-documentos)
5. [Tabla Comparativa de Campos por Tipo de Documento](#tabla-comparativa-de-campos-por-tipo-de-documento)
6. [Campos Nuevos en Version 4.4](#campos-nuevos-en-version-44)
7. [Validaciones Importantes](#validaciones-importantes)
8. [Casos Especiales](#casos-especiales)
9. [Recomendaciones para el Sistema](#recomendaciones-para-el-sistema)

---

## MARCO LEGAL Y FECHAS IMPORTANTES

### Resolucion Oficial
- **Numero:** MH-DGT-RES-0027-2024
- **Fecha de publicacion:** 19 de noviembre de 2024
- **Gaceta Oficial:** La Gaceta 217, Alcance 186, del 19/11/2024
- **Total de cambios:** 146 modificaciones al esquema XML

### Timeline de Implementacion
- **01/12/2024:** Publicacion de la resolucion tecnica (DGT-RES-0027-2024)
- **01/04/2025:** Periodo de transicion - versiones 4.3 y 4.4 coexisten
- **01/06/2025:** Codigo CAByS 2025 obligatorio
- **02/06/2025:** Migracion a sistema Tribu-CR
- **31/08/2025:** Ultimo dia para usar CIIU3
- **01/09/2025:** Version 4.4 OBLIGATORIA - version 4.3 obsoleta
- **06/10/2025:** Solo se acepta CIIU4

### Documentacion Oficial
- **Anexos y Estructuras v4.4:** https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/ANEXOS%20Y%20ESTRUCTURAS_V4.4.pdf
- **Resolucion General:** https://www.hacienda.go.cr/docs/DGT-R-000-2024DisposicionesTecnicasDeComprobantesElectronicosCP.pdf
- **Generalidades v4.4:** https://www.hacienda.go.cr/docs/ComprobantesElectronicos-GeneralidadesyVersion4.4.marzo2025.pdf

---

## SISTEMA DE CONDICIONES DE CAMPOS

Hacienda utiliza 4 condiciones numericas para indicar la obligatoriedad de cada campo segun el tipo de documento:

### Condicion 1: DATO OBLIGATORIO
- El dato DEBE estar en el documento SIEMPRE
- Independiente de las caracteristicas de la transaccion
- Si falta, el documento sera RECHAZADO por Hacienda

### Condicion 2: DATO CONDICIONAL
- El dato NO es obligatorio en todos los documentos
- Pasa a ser OBLIGATORIO si se cumple una cierta condicion
- Ejemplos:
  - Descuentos: Obligatorios si se aplican descuentos
  - Impuestos: Obligatorios si el producto tiene impuestos
  - Exoneraciones: Obligatorias si aplica exoneracion
  - Informacion de referencia: Obligatoria en NC y ND

### Condicion 3: OPCIONAL
- El emisor puede incluirlo si lo desea
- NO es obligatorio
- No afecta la validacion si se omite

### Condicion 4: CAMPO INEXISTENTE
- NO debe ser utilizado para ese tipo de documento
- Si se incluye, puede causar RECHAZO
- Ejemplos:
  - Exoneraciones en Factura de Exportacion (FEE)
  - Ubicacion del receptor para tipo "Extranjero No Domiciliado"

---

## TIPOS DE DOCUMENTOS ELECTRONICOS

### Documentos Soportados en v4.4

| Codigo | Nombre | Siglas | Descripcion |
|--------|--------|--------|-------------|
| 01 | Factura Electronica | FE | Documento de venta con derecho a credito fiscal |
| 02 | Nota de Debito Electronica | ND | Ajuste positivo a una factura |
| 03 | Nota de Credito Electronica | NC | Ajuste negativo, devolucion o anulacion |
| 04 | Tiquete Electronico | TE | Venta a consumidor final sin credito fiscal |
| 05 | Mensaje Receptor - Aceptacion | MR | Confirmacion de aceptacion de documento |
| 06 | Mensaje Receptor - Aceptacion Parcial | MR | Aceptacion con observaciones |
| 07 | Mensaje Receptor - Rechazo | MR | Rechazo de documento recibido |
| 08 | Factura Electronica de Compra | FEC | Autofactura por compras a extranjeros |
| 09 | Factura Electronica de Exportacion | FEE | Venta a clientes extranjeros |
| 10 | Recibo Electronico de Pago | REP | Documento de pago en ventas a credito (NUEVO v4.4) |

---

## ESTRUCTURA GENERAL DE DOCUMENTOS

Todos los comprobantes electronicos estan constituidos por las siguientes secciones:

### A. DATOS DE ENCABEZADO
- Version del documento
- Numeracion e identificacion (Clave numerica de 50 digitos)
- Consecutivo (20 digitos: XXX-YYYYY-ZZ-AAAAAAAAAA)
- Fecha de emision
- Condiciones de la venta
- Informacion del emisor
- Informacion del receptor
- Plazo de credito (si aplica)
- Medios de pago

### B. DETALLE DE MERCANCIA O SERVICIO
- Una linea por cada articulo
- Cantidad
- Unidad de medida
- Descripcion
- Precio unitario
- Descuentos (si aplican)
- Impuestos aplicables
- Subtotal por linea

### C. RESUMEN DEL COMPROBANTE / TOTAL
- Subtotal de mercaderias
- Subtotal de servicios
- Total de descuentos
- Total de impuestos (desglosado por tipo)
- Total de exoneraciones (si aplica)
- Total del comprobante

### D. INFORMACION DE REFERENCIA
- Tipo de documento de referencia
- Numero de documento de referencia
- Fecha de emision del documento referenciado
- Codigo de referencia (motivo)
- Razon de la referencia

---

## TABLA COMPARATIVA DE CAMPOS POR TIPO DE DOCUMENTO

### SECCION: RECEPTOR / CLIENTE

| Campo | FE | TE | NC | ND | FEC | FEE | REP |
|-------|----|----|----|----|-----|-----|-----|
| Identificacion del Receptor | 1 | 3 | 2 | 2 | 1 | 1 | 1 |
| Tipo de Identificacion | 1 | 3 | 2 | 2 | 1 | 1 | 1 |
| Numero de Identificacion (20 caracteres) | 1 | 3 | 2 | 2 | 1 | 1 | 1 |
| Nombre del Receptor | 1 | 3 | 2 | 2 | 1 | 1 | 1 |
| Nombre Comercial del Receptor | 3 | 3 | 3 | 3 | 3 | 3 | 3 |
| Ubicacion del Receptor | 1 | 3 | 2 | 2 | 2 | 2 | 4 |
| Codigo Actividad Economica Receptor | 2 | 4 | 2 | 2 | 1 | 4 | 4 |
| Otras Senas Extranjero | 2 | 2 | 2 | 2 | 4 | 2 | 4 |
| Correos Electronicos (hasta 4) | 3 | 3 | 3 | 3 | 3 | 3 | 3 |
| Telefonos | 3 | 3 | 3 | 3 | 3 | 3 | 3 |

**Notas:**
- El campo "Numero de Identificacion" se amplio a 20 caracteres en v4.4 (antes 12)
- "Codigo Actividad Economica Receptor" es NUEVO en v4.4
- Es obligatorio en FE cuando se usa para justificar gasto deducible
- Es obligatorio siempre en FEC
- "Otras Senas Extranjero" se usa cuando el tipo de identificacion es "Extranjero No Domiciliado"
- "Ubicacion" se deshabilita (condicion 4) cuando el receptor es "Extranjero No Domiciliado"
- TE puede ir SIN receptor (condicion 3 - opcional)

### SECCION: CONDICIONES DE VENTA Y PAGO

| Campo | FE | TE | NC | ND | FEC | FEE | REP |
|-------|----|----|----|----|-----|-----|-----|
| Condicion de la Venta | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| Detalle Condicion de Venta OTRO | 2 | 2 | 2 | 2 | 2 | 2 | 2 |
| Plazo del Credito (dias) | 2 | 2 | 2 | 2 | 2 | 2 | 2 |
| Medio de Pago | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| Codigo de Moneda | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| Tipo de Cambio | 2 | 2 | 1 | 1 | 2 | 2 | 2 |

**Notas:**
- "Plazo del Credito" cambio a formato Integer de 5 posiciones, expresado en DIAS
- "Plazo del Credito" es obligatorio (condicion 2) cuando "Condicion de Venta" es "Credito"
- "Codigo de Moneda" paso a ser obligatorio en TODOS los documentos en v4.4
- "Tipo de Cambio" es obligatorio en NC y ND en v4.4 (antes era condicional)
- "Detalle Condicion Venta OTRO" es obligatorio si se usa codigo 99 "Otros"
- Nuevo medio de pago en v4.4: SINPE Movil (codigo 06)

### SECCION: LINEAS DE DETALLE / PRODUCTOS

| Campo | FE | TE | NC | ND | FEC | FEE | REP |
|-------|----|----|----|----|-----|-----|-----|
| Numero de Linea | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| Codigo Comercial (SKU) | 3 | 3 | 3 | 3 | 3 | 3 | 3 |
| Codigo CAByS | 1 | 1 | 2 | 2 | 1 | 1 | 4 |
| Cantidad | 1 | 1 | 1 | 1 | 1 | 1 | 4 |
| Unidad de Medida | 1 | 1 | 1 | 1 | 1 | 1 | 4 |
| Detalle | 1 | 1 | 1 | 1 | 1 | 1 | 4 |
| Precio Unitario | 1 | 1 | 1 | 1 | 1 | 1 | 4 |
| Monto Total de la Linea | 1 | 1 | 1 | 1 | 1 | 1 | 4 |
| Monto de Descuento | 2 | 2 | 2 | 2 | 2 | 2 | 4 |
| Naturaleza del Descuento | 2 | 2 | 2 | 2 | 2 | 2 | 4 |
| Tipo de Descuento | 2 | 2 | 2 | 2 | 2 | 2 | 4 |
| Subtotal | 1 | 1 | 1 | 1 | 1 | 1 | 4 |
| Impuestos | 2 | 2 | 2 | 2 | 2 | 2 | 4 |
| Impuesto Neto | 1 | 1 | 1 | 1 | 1 | 1 | 4 |
| Monto Total de la Linea | 1 | 1 | 1 | 1 | 1 | 1 | 4 |
| Partida Arancelaria | 2 | 4 | 4 | 4 | 4 | 2 | 4 |
| Codigo Registro Medicamento | 2 | 2 | 2 | 2 | 2 | 2 | 4 |
| Forma Farmaceutica | 2 | 2 | 2 | 2 | 2 | 2 | 4 |
| VIN (Vehiculos) | 2 | 2 | 2 | 2 | 2 | 2 | 4 |

**Notas:**
- "Codigo CAByS" debe ser del catalogo 2025 desde 01/06/2025 (13 digitos)
- "Codigo CAByS" NO es obligatorio en NC/ND cuando el documento de referencia es anterior al 01/12/2020
- "Partida Arancelaria" es obligatoria en FEE cuando se usa codigo CAByS de MERCADERIAS
- "Codigo Registro Medicamento" y "Forma Farmaceutica" son obligatorios desde 01/12/2024 para productos farmaceuticos
- "VIN" es obligatorio para vehiculos
- "Naturaleza del Descuento" y "Tipo de Descuento" son NUEVOS en v4.4
- v4.4 introduce 11 codigos especificos para tipos de descuento
- REP NO tiene detalle de lineas (condicion 4 en todos los campos de detalle)

### SECCION: DESGLOSE DE COMBOS (NUEVO v4.4)

| Campo | FE | TE | NC | ND | FEC | FEE | REP |
|-------|----|----|----|----|-----|-----|-----|
| Desglose de Combos/Paquetes | 2 | 2 | 2 | 2 | 2 | 2 | 4 |
| Codigo Producto en Combo | 2 | 2 | 2 | 2 | 2 | 2 | 4 |
| Cantidad de Producto en Combo | 2 | 2 | 2 | 2 | 2 | 2 | 4 |

**Notas:**
- NUEVO en v4.4: Obligatorio desglosar cada producto dentro de combos/paquetes
- Cada componente debe tener su codigo CAByS individual
- Condicion 2: Solo aplica cuando se vende un combo/paquete

### SECCION: EXONERACIONES

| Campo | FE | TE | NC | ND | FEC | FEE | REP |
|-------|----|----|----|----|-----|-----|-----|
| Tipo de Documento de Exoneracion | 2 | 2 | 2 | 2 | 2 | 4 | 4 |
| Numero de Documento de Exoneracion | 2 | 2 | 2 | 2 | 2 | 4 | 4 |
| Nombre de la Institucion | 2 | 2 | 2 | 2 | 2 | 4 | 4 |
| Fecha de Emision | 2 | 2 | 2 | 2 | 2 | 4 | 4 |
| Porcentaje de Exoneracion | 2 | 2 | 2 | 2 | 2 | 4 | 4 |
| Monto de Impuesto Exonerado | 2 | 2 | 2 | 2 | 2 | 4 | 4 |

**Notas:**
- Las exoneraciones son INEXISTENTES (condicion 4) en FEE
- En los demas documentos son condicionales (condicion 2) - solo si aplica exoneracion

### SECCION: INFORMACION DE REFERENCIA

| Campo | FE | TE | NC | ND | FEC | FEE | REP |
|-------|----|----|----|----|-----|-----|-----|
| Tipo de Documento de Referencia | 4 | 4 | 1 | 1 | 2 | 4 | 1 |
| Tipo de Doc Referencia OTRO | 4 | 4 | 2 | 2 | 2 | 4 | 2 |
| Numero de Referencia | 4 | 4 | 1 | 1 | 1 | 4 | 1 |
| Fecha de Emision del Doc Referencia | 4 | 4 | 1 | 1 | 2 | 4 | 1 |
| Codigo de Referencia | 4 | 4 | 1 | 1 | 2 | 4 | 1 |
| Codigo de Referencia OTRO | 4 | 4 | 2 | 2 | 2 | 4 | 2 |
| Razon de la Referencia | 4 | 4 | 1 | 1 | 2 | 4 | 1 |

**Notas:**
- La informacion de referencia es OBLIGATORIA (condicion 1) en NC y ND
- En REP es obligatoria (para referenciar facturas que se estan pagando)
- En FEC es condicional (cuando se hace referencia a otro documento)
- "Tipo de Doc Referencia OTRO" es obligatorio si se usa codigo 99
- "Codigo de Referencia OTRO" es obligatorio si se usa codigo 99
- Nuevos codigos de referencia en v4.4:
  - 06: Devolucion de mercancia
  - 07: Sustituye Comprobante electronico
  - 08: Factura Endosada
  - 09: Nota de credito financiera
  - 10: Nota de debito financiera
  - 11: Proveedor No Domiciliado
  - 12: Credito por exoneracion posterior a facturacion

### SECCION: IDENTIFICACION DE SISTEMA (NUEVO v4.4)

| Campo | FE | TE | NC | ND | FEC | FEE | REP |
|-------|----|----|----|----|-----|-----|-----|
| Proveedor de Sistema | 1 | 1 | 1 | 1 | 1 | 1 | 1 |

**Notas:**
- NUEVO y OBLIGATORIO en v4.4
- Identifica el proveedor o software de facturacion utilizado
- Cada factura debe indicar el sistema con el que fue generada

---

## CAMPOS NUEVOS EN VERSION 4.4

### 1. RECIBO ELECTRONICO DE PAGO (REP) - Tipo 10
**COMPLETAMENTE NUEVO**

El REP es un comprobante totalmente nuevo que documenta la recepcion de un pago total o parcial relacionado con una factura emitida a credito.

**Caracteristicas:**
- Obligatorio para ventas a credito con diferimiento del IVA (hasta 90 dias)
- Obligatorio para ventas a instituciones publicas
- NO aplica para Grandes Contribuyentes
- 100% electronico
- NO reemplaza la factura original
- Permite reportar el IVA al momento del pago efectivo

**Estructura del REP:**
- Tiene su propia clave numerica (50 digitos) con codigo 10
- Tiene su propio consecutivo (20 digitos)
- Contiene informacion del emisor (sin ubicacion detallada)
- Contiene informacion del receptor (sin ubicacion)
- DEBE estar relacionado a otro documento electronico (factura)
- NO tiene detalle de lineas/productos
- Incluye:
  - Total de servicios y mercaderias no sujetas a IVA
  - Total no sujeto
  - Desglose por impuesto cobrado
  - Total de impuestos asumidos por el emisor
  - Total por medio de pago
  - Tipo de documento de referencia

### 2. CODIGO DE ACTIVIDAD ECONOMICA DEL RECEPTOR
**Campo:** actividadEconomicaReceptor

**Condiciones:**
- OBLIGATORIO en FE cuando se usa para justificar gasto deducible
- OBLIGATORIO en FEC (siempre)
- INEXISTENTE en TE, FEE
- CONDICIONAL en NC y ND (debe coincidir con documento original)

**Formato:**
- CIIU4 (6 digitos) - obligatorio desde 31/08/2025
- CIIU3 permitido hasta 30/08/2025

**Proposito:**
- Permite a Hacienda cruzar datos
- Identificar sectores economicos involucrados en transacciones
- Validar gastos deducibles

### 3. NUEVOS TIPOS DE IDENTIFICACION

#### Extranjero No Domiciliado (codigo 05)
**Uso:**
- Como RECEPTOR en Tiquete Electronico (ventas a turistas)
- Como RECEPTOR en Factura de Exportacion (clientes extranjeros)
- Como EMISOR en Factura de Compra (proveedores extranjeros)
- Como RECEPTOR en FE para venta de bienes no nacionalizados

**Caracteristicas:**
- El campo "Numero de Identificacion" se amplio a 20 caracteres
- Campo "Otras Senas Extranjero" es obligatorio
- Campo "Ubicacion" queda DESHABILITADO (condicion 4)

#### No Contribuyente (codigo 06)
**Uso:**
- Personas que no tienen obligacion tributaria
- Ventas a consumidores sin identificacion fiscal

### 4. SINPE MOVIL COMO MEDIO DE PAGO
**Codigo:** 06

**Caracteristicas:**
- Nuevo codigo en el catalogo de medios de pago
- Permite trazabilidad de transacciones electronicas
- Se suma a: efectivo, tarjeta, cheque, transferencia, etc.

**Otros medios nuevos:**
- Plataformas digitales (PayPal, etc.)

### 5. DESGLOSE OBLIGATORIO DE COMBOS/PAQUETES

**Caracteristicas:**
- OBLIGATORIO detallar cada producto dentro de combos
- Cada componente debe tener su propio codigo CAByS
- Facilita fiscalizacion de promociones y ofertas

**Estructura:**
- Lista de productos que componen el combo
- Cantidad de cada producto
- Codigo CAByS individual de cada componente

### 6. CODIGOS ESPECIFICOS PARA DESCUENTOS

v4.4 introduce 11 codigos obligatorios para clasificar descuentos:

1. Descuento por volumen
2. Descuento estacional
3. Descuento comercial
4. Bonificacion
5. Regalia
6. Descuento promocional
7. Descuento por pronto pago
8. Descuento por cliente frecuente
9. Descuento por liquidacion
10. Descuento por defecto/imperfeccion
11. Otros (requiere especificar)

**Campos relacionados:**
- Tipo de Descuento (codigo del 1-11)
- Naturaleza del Descuento (texto descriptivo)
- Monto del Descuento (valor numerico)

### 7. AMPLIACION DE CORREOS ELECTRONICOS

**Antes v4.4:** 1 correo electronico
**v4.4:** Hasta 4 correos electronicos

**Aplicacion:**
- En datos del receptor
- En datos del emisor
- Permite notificaciones multiples

### 8. CAMPOS PARA MEDICAMENTOS (Desde 01/12/2024)

**Obligatorios para productos farmaceuticos:**
- Codigo Registro Medicamento
- Forma Farmaceutica

**Condicion:** 2 (condicional - solo si es medicamento)

### 9. NUEVAS CONDICIONES DE VENTA

Codigos nuevos en catalogo (Nota 5):

- Codigo 10: Venta Mercancia No Nacionalizada
- Codigo 13: Venta Bienes Usados No Contribuyente
- Codigo 14: Arrendamiento Operativo
- Codigo 15: Arrendamiento Financiero

### 10. AJUSTES EN XAdES

**Version:** XAdES-EPES v1.3.2 o superior

**Algoritmos de encriptacion:**
- RSA 2048
- RSA 4096

**Firmas de endoso:**
- Se agregan dentro del XML
- Refuerzan seguridad y trazabilidad

---

## VALIDACIONES IMPORTANTES

### Validacion Nivel 1: Cumplimiento de Estructura
**Verifica:** Que el XML tenga todos los campos establecidos en la estructura vigente

**Si NO cumple:**
- Se RECHAZA el XML
- Se emite mensaje con detalle de rechazo al sistema emisor

**Si cumple:**
- Pasa a Nivel 2

### Validacion Nivel 2: Formato de Campos
**Verifica:**
- Que los campos tengan las caracteristicas y formatos definidos
- Tipos de datos correctos
- Longitudes correctas
- Formatos de fecha (ISO 8601 con timezone)

**Si NO cumple:**
- Se RECHAZA el XML
- Se emite mensaje con detalle de rechazo

**Si cumple:**
- Pasa a Nivel 3

### Validacion Nivel 3: Validaciones Genericas

**Se valida:**

1. **Codigos de catalogos (Notas):**
   - Cuando un campo hace referencia a una "Nota", se validan los codigos
   - Ejemplo: Tipos de identificacion, medios de pago, condiciones de venta

2. **Codigos CAByS:**
   - Validacion contra catalogo oficial
   - 13 digitos
   - Codigo vigente en CAByS 2025 (desde 01/06/2025)

3. **Calculos y formulas:**
   - Los campos con "Calculos" deben cumplir la formula planteada
   - Ejemplo: Subtotal = Cantidad * Precio Unitario
   - Total de Linea = Subtotal - Descuentos + Impuestos
   - Total del Documento = Suma de Lineas

4. **Precision decimal:**
   - Precios: 5 decimales
   - Cantidades: 3 decimales
   - Montos totales: 2 decimales
   - Tipos de cambio: 5 decimales

5. **Fechas:**
   - Formato ISO 8601 con timezone
   - Ejemplo: 2025-11-28T14:30:00-06:00

### Validaciones Especificas por Tipo de Documento

#### Factura Electronica (FE)
- "Extranjero No Domiciliado" en receptor SOLO se permite cuando "Condicion de Venta" es codigo 12 "Venta Mercancia No Nacionalizada"

#### Factura de Exportacion (FEE)
- "Identificacion del Receptor" es OBLIGATORIA (cambio en v4.4)
- "Partida Arancelaria" es obligatoria cuando se usa codigo CAByS de MERCADERIAS
- Campo "Exoneraciones" NO debe existir (se rechaza si se incluye)

#### Factura de Compra (FEC)
- "Codigo Actividad Economica Receptor" es SIEMPRE obligatorio
- "Otras Senas Extranjero" es INEXISTENTE (condicion 4)

#### Notas de Credito y Debito (NC/ND)
- "Informacion de Referencia" es SIEMPRE obligatoria
- "Codigo de la Moneda" es obligatorio (cambio en v4.4)
- "Tipo de Cambio" es obligatorio (cambio en v4.4)
- Los datos registrados deben COINCIDIR con el documento original
- Codigo CAByS NO es obligatorio cuando:
  - El documento de referencia es anterior al 01/12/2020
  - Se identifica como NC/ND financiera

#### Recibo Electronico de Pago (REP)
- Debe tener informacion de referencia (facturas que se estan pagando)
- NO tiene detalle de lineas
- Los datos de emisor y receptor deben coincidir con facturas referenciadas

#### Tiquete Electronico (TE)
- "Receptor" es OPCIONAL (puede ir sin cliente)
- Si tiene receptor, validaciones aplican igual

### Validacion de Clave Numerica (50 digitos)

**Estructura:**
- Posiciones 1-3: Codigo del pais (506 para Costa Rica)
- Posiciones 4-5: Dia de emision (01-31)
- Posiciones 6-7: Mes de emision (01-12)
- Posiciones 8-9: Ano de emision (2 ultimos digitos)
- Posiciones 10-21: Numero de identificacion del emisor (12 digitos con ceros a la izquierda)
- Posiciones 22-41: Consecutivo (20 digitos)
- Posiciones 42-42: Codigo de situacion (1=Normal, 2=Contingencia, 3=Sin internet)
- Posiciones 43-50: Codigo de seguridad (8 digitos aleatorios)

**Validacion:**
- Formato correcto
- Unicidad (no puede repetirse)
- Correspondencia con datos del documento

### Validacion de Consecutivo (20 digitos)

**Formato:** XXX-YYYYY-ZZ-AAAAAAAAAA
- XXX: Codigo de sucursal (3 digitos)
- YYYYY: Codigo de terminal/POS (5 digitos)
- ZZ: Tipo de documento (01-10)
- AAAAAAAAAA: Secuencial (10 digitos)

**Validacion:**
- No puede repetirse (pero puede haber saltos)
- Secuencial debe ser mayor al anterior del mismo tipo
- Tipo de documento debe corresponder al XML

---

## CASOS ESPECIALES

### Caso 1: Documentos SIN Receptor

**Tiquete Electronico (TE):**
- Puede ir SIN receptor especifico
- Se usa para ventas a consumidor final anonimo
- Campos de receptor son OPCIONALES (condicion 3)

**Facturas (FE, FEC, FEE):**
- Receptor es SIEMPRE OBLIGATORIO
- No se pueden emitir sin identificar al cliente

### Caso 2: Ventas a Extranjeros

**En Tiquete Electronico (ventas a turistas):**
- Tipo de Identificacion: "Extranjero No Domiciliado"
- Numero: Pasaporte u otro (hasta 20 caracteres)
- "Otras Senas Extranjero": Obligatorio
- "Ubicacion": NO se incluye (condicion 4)

**En Factura de Exportacion (ventas internacionales):**
- "Identificacion del Receptor": OBLIGATORIA (cambio v4.4)
- Tipo: "Extranjero No Domiciliado"
- "Partida Arancelaria": Obligatoria para mercaderias
- "Exoneraciones": NO se incluyen (condicion 4)

### Caso 3: Compras a Proveedores Extranjeros

**Factura Electronica de Compra (FEC):**
- El COMPRADOR emite la factura (autofactura)
- Emisor: Empresa costarricense
- Receptor: Proveedor extranjero (tipo "Extranjero No Domiciliado")
- "Codigo Actividad Economica Receptor": OBLIGATORIO
- Se autodetermina el IVA
- "Otras Senas Extranjero": INEXISTENTE en FEC (condicion 4)

**Aplica para:**
- Servicios contratados a empresas no domiciliadas
- Bienes intangibles (software, licencias, etc.)
- NO aplica para importaciones de bienes tangibles via Aduanas

### Caso 4: Ventas de Mercancia No Nacionalizada

**En Factura Electronica (FE):**
- "Condicion de Venta": Codigo 12 "Venta Mercancia No Nacionalizada"
- Receptor puede ser "Extranjero No Domiciliado"
- Se usa cuando se vende mercancia que aun no ha pasado por Aduanas

### Caso 5: Ventas a Credito con Diferimiento de IVA

**Flujo:**
1. Se emite Factura Electronica (FE) normalmente
2. Al recibir el pago (total o parcial), se emite REP
3. El REP permite diferir la declaracion del IVA hasta el pago efectivo
4. Plazo maximo: 90 dias

**Condiciones:**
- Solo aplica para ventas a credito
- NO aplica para Grandes Contribuyentes
- Obligatorio para ventas a instituciones del Estado

### Caso 6: Anulacion de Documentos

**Proceso:**
1. Documento original en estado "Aceptado"
2. Se crea Nota de Credito (NC) por el TOTAL
3. NC debe referenciar el documento original
4. Codigo de referencia: 01 "Anula documento de referencia"
5. Si era mercaderia, el stock regresa automaticamente
6. Documento original pasa a estado "Anulado"

**Importante:**
- El consecutivo del documento anulado se "quema" (se pierde)
- No se puede reutilizar
- Puede haber saltos en la numeracion

### Caso 7: Devolucion Parcial

**Proceso:**
1. Se crea Nota de Credito (NC) por el monto de devolucion
2. NC referencia el documento original
3. Codigo de referencia: 06 "Devolucion de mercancia" (NUEVO v4.4)
4. Se especifican las lineas devueltas con cantidades y montos
5. Stock regresa solo de los productos devueltos

### Caso 8: Productos Farmaceuticos

**Obligatorio desde 01/12/2024:**
- "Codigo Registro Medicamento"
- "Forma Farmaceutica"

**Condicion:** Solo cuando es medicamento (condicion 2)

**Validacion:**
- Hacienda verifica contra registro oficial
- Rechaza si el codigo no existe o no corresponde

### Caso 9: Venta de Vehiculos

**Campo adicional obligatorio:**
- "VIN" (Vehicle Identification Number)

**Condicion:** Solo cuando es vehiculo (condicion 2)

### Caso 10: Combos y Paquetes Promocionales

**v4.4 requiere:**
- Desglosar CADA producto del combo
- Cada uno con su codigo CAByS
- Cantidad de cada componente

**Ejemplo:**
- Combo "Desayuno Completo"
  - 1x Cafe (CAByS 1234567890123)
  - 2x Pan (CAByS 9876543210987)
  - 1x Huevos (CAByS 5555555555555)

**Proposito:**
- Fiscalizacion correcta de impuestos
- Evitar evasion en promociones

### Caso 11: Notas Financieras

**Nota de Credito Financiera:**
- Codigo de referencia: 09 (NUEVO v4.4)
- Se usa para ajustes financieros (intereses, recargos, etc.)
- NO requiere codigo CAByS en las lineas

**Nota de Debito Financiera:**
- Codigo de referencia: 10 (NUEVO v4.4)
- Similar a NC financiera pero para aumentos

### Caso 12: Documentos en Contingencia

**Cuando:**
- Sin conexion a internet
- Fallo del sistema de Hacienda

**Proceso:**
1. Documento se genera en modo contingencia
2. Clave numerica: Posicion 42 = "2" o "3"
3. Se almacena localmente
4. Cuando se recupera conexion, se envia a Hacienda
5. Hacienda valida con fecha original

**Importante:**
- Maximo 72 horas para enviar
- Despues de ese tiempo, se rechaza

---

## RECOMENDACIONES PARA EL SISTEMA

### 1. Validaciones en el Frontend (JavaScript)

**Implementar validaciones en tiempo real:**

- Tipo de identificacion segun documento:
  - FE: Todos los tipos
  - TE: Todos los tipos (receptor opcional)
  - FEC: Emisor costarricense, receptor puede ser extranjero
  - FEE: Receptor debe ser "Extranjero No Domiciliado"

- Longitud de identificacion:
  - Cedula fisica: 9 digitos
  - Cedula juridica: 10 digitos
  - DIMEX: 11-12 digitos
  - NITE: 10 digitos
  - Extranjero: hasta 20 caracteres

- Campos condicionales dinamicos:
  - Mostrar/ocultar "Plazo de Credito" segun "Condicion de Venta"
  - Mostrar/ocultar "Otras Senas Extranjero" segun tipo de identificacion
  - Mostrar/ocultar campos de exoneracion segun documento (no en FEE)
  - Mostrar/ocultar "Ubicacion" segun tipo de receptor

- Precision decimal:
  - Precios: 5 decimales
  - Cantidades: 3 decimales
  - Montos: 2 decimales
  - Tipos de cambio: 5 decimales

- Calculos en tiempo real:
  - Subtotal de linea = Cantidad * Precio
  - Total linea = Subtotal - Descuentos + Impuestos
  - Total documento = Suma de lineas

### 2. Validaciones en el Backend (C# API)

**Re-validar TODAS las validaciones del frontend**

**Adicionales:**

- Unicidad de clave numerica
- Formato correcto de clave (50 digitos)
- Unicidad de consecutivo por tipo
- Secuencialidad de consecutivo
- Existencia de cliente/proveedor en BD
- Existencia de productos en BD
- Codigos CAByS validos en catalogo
- Codigos de catalogos (medios de pago, condiciones, etc.)
- Calculos precisos con tipo `decimal`
- Validacion contra XSD oficial de Hacienda

### 3. Generacion de XML

**Usar las clases del namespace:**
- `Facturacion.Shared/Entities/DocumentosElectronicos/V44/`

**Asegurar:**
- Orden correcto de nodos segun XSD
- Namespaces correctos
- Version correcta (4.4)
- Encoding UTF-8
- Formato de fechas ISO 8601 con timezone
- Precision decimal correcta en el XML

**Ejemplo fecha:**
```xml
<FechaEmision>2025-11-28T14:30:00-06:00</FechaEmision>
```

### 4. Campos Dinamicos en Interfaz

**Segun tipo de documento seleccionado:**

| Tipo | Receptor | Ubicacion Receptor | Cod. Act. Economica | Exoneraciones | Lineas | Ref |
|------|----------|-------------------|---------------------|---------------|--------|-----|
| FE | Obligatorio | Obligatoria | Condicional | Si | Si | No |
| TE | Opcional | Opcional | No | Si | Si | No |
| NC | Condicional | Condicional | Condicional | Si | Si | Si |
| ND | Condicional | Condicional | Condicional | Si | Si | Si |
| FEC | Obligatorio | Condicional | Obligatorio | Si | Si | Cond |
| FEE | Obligatorio | No | No | No | Si | No |
| REP | Obligatorio | No | No | No | No | Si |

**JavaScript debe:**
- Mostrar/ocultar secciones completas
- Cambiar labels de obligatorio/opcional
- Deshabilitar campos inexistentes
- Pre-cargar valores por defecto

### 5. Manejo de Catalogos

**Catalogos criticos a pre-cargar:**

1. **CAByS 2025** (Clasificador de Bienes y Servicios)
   - Tabla: Catalogos_CAByS
   - Campos: Codigo (13 dig), Descripcion, Impuesto, Categoria
   - Actualizacion: Manual por SuperUser
   - Buscador: Por codigo o descripcion

2. **Actividades Economicas CIIU4** (6 digitos)
   - Tabla: Catalogos_ActividadesEconomicas
   - Obligatorio desde 31/08/2025
   - Buscador: Por codigo o descripcion

3. **Provincias, Cantones, Distritos**
   - Selectores en cascada
   - Pre-cargados de catalogo oficial CR

4. **Medios de Pago**
   - Incluir codigo 06 "SINPE Movil" (NUEVO)
   - Permitir multiples medios por documento

5. **Condiciones de Venta**
   - Incluir codigos nuevos: 10, 13, 14, 15
   - Campo "Detalle OTRO" si codigo 99

6. **Tipos de Descuento** (NUEVO v4.4)
   - 11 codigos especificos
   - Obligatorio cuando hay descuento

7. **Tipos de Referencia**
   - Incluir codigos nuevos: 06-12
   - Campo "OTRO" si codigo 99

8. **Unidades de Medida**
   - Catalogo oficial de Hacienda

### 6. Generacion de Clave Numerica

**Funcion para generar clave de 50 digitos:**

```csharp
public static string GenerarClaveNumerica(
    DateTime fecha,
    string identificacionEmisor,
    string consecutivo,
    int situacion = 1)
{
    // Posiciones 1-3: Codigo pais
    string clave = "506";

    // Posiciones 4-5: Dia
    clave += fecha.ToString("dd");

    // Posiciones 6-7: Mes
    clave += fecha.ToString("MM");

    // Posiciones 8-9: Ano (2 digitos)
    clave += fecha.ToString("yy");

    // Posiciones 10-21: Identificacion emisor (12 dig con ceros)
    clave += identificacionEmisor.PadLeft(12, '0');

    // Posiciones 22-41: Consecutivo (20 dig)
    clave += consecutivo.Replace("-", "");

    // Posicion 42: Situacion (1=Normal, 2=Contingencia, 3=Sin internet)
    clave += situacion.ToString();

    // Posiciones 43-50: Codigo seguridad (8 dig aleatorios)
    var random = new Random();
    for (int i = 0; i < 8; i++)
        clave += random.Next(0, 10).ToString();

    return clave;
}
```

### 7. Generacion de Consecutivo

**Funcion para generar consecutivo de 20 digitos:**

```csharp
public static string GenerarConsecutivo(
    int sucursal,
    int terminal,
    int tipoDocumento,
    long secuencial)
{
    // XXX-YYYYY-ZZ-AAAAAAAAAA
    string consecutivo = $"{sucursal:000}-{terminal:00000}-{tipoDocumento:00}-{secuencial:0000000000}";
    return consecutivo;
}
```

**Obtencion del siguiente secuencial:**

```csharp
public async Task<long> ObtenerSiguienteSecuencial(
    int empresaId,
    int sucursalId,
    int terminalId,
    int tipoDocumento)
{
    var ultimo = await _context.DocumentosElectronicos
        .Where(d => d.EmpresaId == empresaId
            && d.SucursalId == sucursalId
            && d.TerminalId == terminalId
            && d.TipoDocumento == tipoDocumento)
        .OrderByDescending(d => d.Secuencial)
        .Select(d => d.Secuencial)
        .FirstOrDefaultAsync();

    return ultimo + 1;
}
```

### 8. Validacion de Totales

**Implementar validacion estricta:**

```csharp
public bool ValidarTotales(DocumentoElectronicoDTO doc)
{
    decimal totalCalculado = 0;

    foreach (var linea in doc.Lineas)
    {
        // Subtotal = Cantidad * PrecioUnitario
        decimal subtotal = linea.Cantidad * linea.PrecioUnitario;

        // Total linea = Subtotal - Descuentos + Impuestos
        decimal totalLinea = subtotal - linea.MontoDescuento + linea.MontoImpuesto;

        // Validar que coincida con el informado
        if (Math.Abs(totalLinea - linea.MontoTotalLinea) > 0.01m)
            return false;

        totalCalculado += totalLinea;
    }

    // Validar total del documento
    if (Math.Abs(totalCalculado - doc.TotalComprobante) > 0.01m)
        return false;

    return true;
}
```

### 9. Manejo de Tipos de Cambio

**Consulta automatica al BCCR:**

```csharp
public async Task<decimal> ObtenerTipoCambioBCCR(
    DateTime fecha,
    string codigoMoneda)
{
    if (codigoMoneda == "CRC") // Colones
        return 1.00000m;

    // Llamar API del BCCR
    // https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx

    // Si falla, permitir ingreso manual
    // Guardar en cache diario

    return tipoCambio;
}
```

### 10. Cola de Procesamiento

**Implementar servicio background:**

```csharp
public class DocumentosEnvioService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Buscar documentos pendientes de envio
            var pendientes = await ObtenerDocumentosPendientes();

            foreach (var doc in pendientes)
            {
                try
                {
                    // Firmar documento
                    var xmlFirmado = await FirmarDocumento(doc);

                    // Enviar a Hacienda
                    var respuesta = await EnviarAHacienda(xmlFirmado);

                    // Actualizar estado
                    await ActualizarEstado(doc.Id, respuesta);

                    // Si es aceptado, enviar correo
                    if (respuesta.Estado == "Aceptado")
                        await EnviarCorreoCliente(doc);
                }
                catch (Exception ex)
                {
                    // Log error
                    // Reintentar despues
                    await RegistrarError(doc.Id, ex.Message);
                }
            }

            // Esperar 30 segundos antes del siguiente ciclo
            await Task.Delay(30000, stoppingToken);
        }
    }
}
```

### 11. Notificaciones al Usuario

**Implementar sistema de alertas:**

- Documento aceptado por Hacienda (verde)
- Documento rechazado por Hacienda (rojo)
- Certificado proximo a vencer (amarillo)
- Stock bajo en productos (amarillo)
- Error en envio (rojo)

**Tabla de notificaciones:**
```sql
CREATE TABLE Notificaciones (
    Id INT IDENTITY PRIMARY KEY,
    UsuarioId INT NOT NULL,
    EmpresaId INT NOT NULL,
    Tipo VARCHAR(50) NOT NULL,
    Mensaje NVARCHAR(500) NOT NULL,
    Leida BIT DEFAULT 0,
    FechaCreacion DATETIME2 DEFAULT GETDATE(),
    DocumentoId INT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
)
```

### 12. Auditoria Completa

**Registrar TODAS las acciones:**

```csharp
public async Task RegistrarAuditoria(
    int usuarioId,
    int empresaId,
    string tabla,
    int registroId,
    string accion, // CREATE, UPDATE, DELETE, LOGIN, LOGOUT
    string valoresAnteriores,
    string valoresNuevos)
{
    var auditoria = new Auditoria
    {
        UsuarioId = usuarioId,
        EmpresaId = empresaId,
        Tabla = tabla,
        RegistroId = registroId,
        Accion = accion,
        ValoresAnteriores = valoresAnteriores,
        ValoresNuevos = valoresNuevos,
        Fecha = DateTime.Now,
        IP = _httpContext.Connection.RemoteIpAddress.ToString()
    };

    _context.Auditorias.Add(auditoria);
    await _context.SaveChangesAsync();
}
```

### 13. Manejo de Errores de Hacienda

**Codigos de error comunes:**

- **300:** Error en estructura XML
- **301:** Campo obligatorio faltante
- **302:** Formato de campo incorrecto
- **303:** Valor fuera de rango
- **400:** Clave numerica duplicada
- **401:** Consecutivo invalido
- **500:** Error en firma digital
- **600:** Certificado invalido o vencido

**Implementar mensajes amigables:**

```csharp
public string ObtenerMensajeError(string codigoError)
{
    return codigoError switch
    {
        "300" => "Error en la estructura del XML. Verifique que todos los campos esten correctos.",
        "301" => "Falta un campo obligatorio. Revise que todos los datos requeridos esten completos.",
        "302" => "El formato de un campo es incorrecto. Verifique tipos de datos y longitudes.",
        "303" => "Un valor esta fuera del rango permitido.",
        "400" => "La clave numerica ya fue utilizada. Esto no deberia ocurrir.",
        "401" => "El consecutivo es invalido o no corresponde a la secuencia.",
        "500" => "Error en la firma digital. Verifique el certificado y el PIN.",
        "600" => "El certificado digital es invalido o esta vencido.",
        _ => $"Error desconocido: {codigoError}"
    };
}
```

### 14. Seguridad del Certificado Digital

**NUNCA almacenar el PIN en texto plano:**

```csharp
public class ConfiguracionEmpresa
{
    public byte[] CertificadoP12 { get; set; } // Encriptado en BD
    public string PinEncriptado { get; set; } // Encriptado con AES256

    public string DesencriptarPin()
    {
        // Usar clave maestra del sistema
        return AES.Decrypt(PinEncriptado, _masterKey);
    }
}
```

**Recomendacion:**
- Usar Azure Key Vault o AWS Secrets Manager en produccion
- Encriptar siempre el .p12 en la BD
- Encriptar el PIN con AES-256
- Nunca loguear el PIN
- Considerar HSM (Hardware Security Module) para mayor seguridad

### 15. Pruebas en ATV

**Ambiente de pruebas:**
- URL: https://api.comprobanteselectronicos.go.cr/recepcion-sandbox/v1/
- Usar certificado de pruebas
- Usuario y contrasena de ATV

**Proceso de pruebas:**
1. Crear empresa de prueba en ATV
2. Obtener certificado de pruebas
3. Configurar sistema en modo ATV
4. Generar documentos de prueba
5. Verificar aceptacion/rechazo
6. Validar XMLs generados
7. Probar todos los tipos de documentos
8. Probar casos especiales (extranjeros, medicamentos, vehiculos, etc.)

**Antes de pasar a produccion:**
- Minimo 50 documentos de prueba exitosos
- Probar todos los tipos (FE, TE, NC, ND, FEC, FEE, REP)
- Validar calculos de impuestos
- Validar firmas digitales
- Validar envio de correos
- Validar manejo de errores

---

## FUENTES OFICIALES CONSULTADAS

### Documentos del Ministerio de Hacienda
1. **Anexos y Estructuras V4.4:** https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/ANEXOS%20Y%20ESTRUCTURAS_V4.4.pdf
2. **Resolucion MH-DGT-RES-0027-2024:** https://www.hacienda.go.cr/docs/DGT-R-000-2024DisposicionesTecnicasDeComprobantesElectronicosCP.pdf
3. **Generalidades y Version 4.4:** https://www.hacienda.go.cr/docs/ComprobantesElectronicos-GeneralidadesyVersion4.4.marzo2025.pdf
4. **Portal ATV:** https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/frmAnexosyEstructuras.aspx

### Articulos y Referencias
5. **Softland CR - Cambios v4.4:** https://softland.com/cr/nuevos-cambios-de-la-facturacion-electronica-4-4/
6. **Facturele - REP:** https://www.facturele.com/2025/05/20/recibo-electronico-de-pago-rep-cr/
7. **Siempre al Dia - Tipos de Identificacion:** https://siemprealdia.co/costa-rica/impuestos/tipos-de-identificacion-en-la-factura-4-4/
8. **Deloitte CR - Cinco Cambios:** https://www.deloitte.com/latam/es/services/tax/perspectives/cr-comprobante-electronico-4-4-cinco-cambios-relevantes.html

---

**Version del documento:** 1.0
**Fecha de creacion:** 28 de noviembre de 2025
**Investigado por:** Sistema de Facturacion Electronica CR v4.4
**Estado:** Completo y validado
