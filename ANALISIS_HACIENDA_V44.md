# Análisis de Facturación Electrónica v4.4 - Hacienda Costa Rica

## Fecha de Vigencia
- **Obligatorio desde:** 1 de septiembre de 2025
- **Resolución:** MH-DGT-RES-0027-2024

## Fuentes Oficiales
- [Portal ATV - Anexos y Estructuras](https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/frmAnexosyEstructuras.aspx)
- [Documento Anexos y Estructuras v4.4](https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/ANEXOS%20Y%20ESTRUCTURAS_V4.4.pdf)

---

## CATÁLOGOS OFICIALES

### 1. Medios de Pago (MedioPago)

| Código | Descripción |
|--------|-------------|
| 01 | Efectivo |
| 02 | Tarjeta |
| 03 | Cheque |
| 04 | Transferencia - depósito bancario |
| 05 | Recaudado por terceros |
| 06 | SINPE Móvil (**NUEVO v4.4**) |
| 07 | Plataforma digital (PayPal, etc.) (**NUEVO v4.4**) |
| 99 | Otros |

### 2. Condiciones de Venta (CondicionVenta)

| Código | Descripción |
|--------|-------------|
| 01 | Contado |
| 02 | Crédito |
| 03 | Consignación |
| 04 | Apartado |
| 05 | Arrendamiento con opción de compra |
| 06 | Arrendamiento en función financiera |
| 07 | Cobro a favor de terceros |
| 08 | Intercambio / Permuta (**NUEVO v4.4**) |
| 09 | Donación (**NUEVO v4.4**) |
| 10 | Otros (**NUEVO v4.4**) |
| 12 | Venta de mercancía no nacionalizada (**NUEVO v4.4**) |
| 13 | Venta de bienes usados a no contribuyente (**NUEVO v4.4**) |
| 14 | Arrendamiento operativo (**NUEVO v4.4**) |
| 15 | Arrendamiento financiero (**NUEVO v4.4**) |

### 3. Tipos de Impuesto (Impuesto)

| Código | Descripción | Porcentaje |
|--------|-------------|------------|
| 01 | Impuesto al Valor Agregado | Variable (ver tarifas) |
| 02 | Impuesto Selectivo de Consumo | Variable |
| 03 | Impuesto Único a los Combustibles | Variable |
| 04 | Impuesto específico de Bebidas Alcohólicas | Variable |
| 05 | Impuesto sobre bebidas sin alcohol y jabones | Variable |
| 06 | Impuesto a los Productos de Tabaco | Variable |
| 07 | IVA (cálculo especial) | 13% |
| 08 | IVA Régimen de Bienes Usados (Factor) | 13% |
| 12 | Impuesto específico al cemento asfáltico | Variable |
| 99 | Otros | Variable |

### 4. Tarifas de IVA (CodigoTarifa)

| Código | Descripción | Porcentaje |
|--------|-------------|------------|
| 01 | Tarifa 0% (Exento) | 0% |
| 02 | Tarifa reducida 1% | 1% |
| 03 | Tarifa reducida 2% | 2% |
| 04 | Tarifa reducida 4% | 4% |
| 05 | Transitorio 0% | 0% |
| 06 | Transitorio 4% | 4% |
| 07 | Transitorio 8% | 8% |
| 08 | Tarifa general 13% | 13% |
| 11 | Tarifa 0% sin derecho a crédito (**NUEVO v4.4**) | 0% |

### 5. Tipos de Documento Electrónico

| Código | Descripción |
|--------|-------------|
| 01 | Factura Electrónica (FE) |
| 02 | Nota de Débito Electrónica (ND) |
| 03 | Nota de Crédito Electrónica (NC) |
| 04 | Tiquete Electrónico (TE) |
| 05 | Confirmación de aceptación del comprobante |
| 06 | Confirmación de aceptación parcial del comprobante |
| 07 | Confirmación de rechazo del comprobante |
| 08 | Factura Electrónica de Compra (FEC) |
| 09 | Factura Electrónica de Exportación (FEE) |
| 10 | Recibo Electrónico de Pago (REP) (**NUEVO v4.4**) |

### 6. Tipos de Identificación

| Código | Descripción |
|--------|-------------|
| 01 | Cédula Física |
| 02 | Cédula Jurídica |
| 03 | DIMEX |
| 04 | NITE |
| 05 | Pasaporte (**NUEVO v4.4**) |
| 06 | Extranjero no domiciliado (**NUEVO v4.4**) |
| 07 | No contribuyente (**NUEVO v4.4**) |

### 7. Códigos de Descuento (**NUEVO v4.4**)

| Código | Descripción |
|--------|-------------|
| 01 | Descuento por Regalía |
| 02 | Descuento por Regalía (IVA a cargo del cliente) |
| 03 | Descuento por Bonificación |
| 04 | Descuento por Volumen |
| 05 | Descuento Estacional |
| 06 | Descuento Promocional |
| 07 | Descuento Comercial |
| 08 | Descuento por Frecuencia |
| 09 | Descuento Sostenido |
| 99 | Otros Descuentos |

### 8. Unidades de Medida

| Código | Descripción |
|--------|-------------|
| Al | Alquiler |
| Alc | Alcance cuartilla (resma) |
| Cm | Centímetro |
| Cj | Caja |
| Cn | Caneca |
| Ct | Cartucho |
| d | Día |
| dm | Decímetro |
| Gal | Galón |
| g | Gramo |
| h | Hora |
| Kg | Kilogramo |
| Km | Kilómetro |
| L | Litro |
| m | Metro |
| m² | Metro cuadrado |
| m³ | Metro cúbico |
| min | Minuto |
| mL | Mililitro |
| mm | Milímetro |
| Mn | Mensualidad |
| Oz | Onza |
| Otro | Otros (por especificar) |
| Paq | Paquete |
| Pl | Pliego |
| Qd | Quintal métrico |
| Qm | Quintal métrico |
| Rac | Ración |
| s | Segundo |
| Sb | Sobre |
| Sp | Servicios Profesionales |
| St | Set |
| Tam | Tanda |
| Tm | Tonelada |
| Unid | Unidad |
| Yd | Yarda |

### 9. Códigos de Exoneración

| Código | Descripción |
|--------|-------------|
| 01 | Compras autorizadas |
| 02 | Ventas exentas a diplomáticos |
| 03 | Autorizado por Ley especial |
| 04 | Exenciones de la DGT |
| 05 | Zonas Francas |
| 06 | Régimen de Perfeccionamiento Activo |
| 07 | Régimen de Perfeccionamiento Pasivo |
| 08 | Bienes de Capital |
| 99 | Otros |

### 10. Tipos de Documento de Referencia

| Código | Descripción |
|--------|-------------|
| 01 | Factura electrónica |
| 02 | Nota de débito electrónica |
| 03 | Nota de crédito electrónica |
| 04 | Tiquete electrónico |
| 05 | Nota de despacho |
| 06 | Contrato |
| 07 | Procedimiento |
| 08 | Comprobante emitido en contingencia |
| 09 | Devolución de mercadería |
| 10 | Sustituye factura rechazada por Hacienda |
| 11 | Sustituye factura rechazada por receptor |
| 12 | Sustituye factura de exportación |
| 13 | Facturacion mes vencido |
| 99 | Otros |

---

## CAMBIOS NECESARIOS EN EL SISTEMA

### 1. Entidades a Crear/Modificar

#### Nuevas Entidades:
- [x] `TipoDescuentoHacienda` - Catálogo de códigos de descuento (10 códigos) ✅ COMPLETADO
- [x] `TarifaIVA` - Catálogo de tarifas de IVA con porcentajes ✅ COMPLETADO
- [x] `TipoDocumentoReferencia` - Ya existe, datos actualizados ✅ COMPLETADO
- [x] `CodigoReferencia` - Códigos de razón de referencia ✅ COMPLETADO

#### Modificar Entidades Existentes:
- [x] `MedioPago` - Agregar códigos 06 (SINPE) y 07 (Plataforma digital) ✅ COMPLETADO
- [x] `CondicionVenta` - Agregar códigos 08-15 ✅ COMPLETADO
- [x] `TipoIdentificacion` - Agregar códigos 05, 06, 07 (enum actualizado) ✅ COMPLETADO
- [x] `Impuesto` - Porcentajes correctos (IVA 13%) ✅ COMPLETADO
- [x] `UnidadMedida` - Lista completa según Hacienda ✅ COMPLETADO
- [x] `TipoDocumento` - Agregar código 10 (REP) ✅ COMPLETADO
- [x] `CodigoExoneracion` - Códigos completos ✅ COMPLETADO

### 2. Actualizar SeedDb ✅ COMPLETADO

Los siguientes catálogos fueron actualizados:

1. **MediosPago** - ✅ Agregados SINPE Móvil (06) y Plataforma digital (07)
2. **CondicionesVenta** - ✅ Agregadas 7 nuevas condiciones (08-15)
3. **Impuestos** - ✅ Corregidos porcentajes (IVA 13%, etc.)
4. **TarifasIVA** - ✅ Creado nuevo catálogo con 9 tarifas
5. **TiposDescuentoHacienda** - ✅ Creado nuevo catálogo con 10 códigos
6. **TiposIdentificacion** - ✅ Enum actualizado con código 07 (NoContribuyente)
7. **UnidadesMedida** - ✅ Lista completa según Hacienda v4.4
8. **TiposDocumento** - ✅ Agregado REP (código 10)
9. **CodigosExoneracion** - ✅ Códigos completos (01-08, 99)
10. **TiposDocumentoReferencia** - ✅ Códigos completos (01-13, 99)
11. **CodigosReferencia** - ✅ Códigos de razón de referencia

### 3. Cambios en Documentos Electrónicos

#### Campos Nuevos Obligatorios:
- `actividadEconomicaReceptor` - Obligatorio en facturas
- Código de descuento obligatorio cuando hay descuentos
- Desglose de combos/surtidos con códigos CABYS individuales

#### Nuevo Documento:
- **REP (Recibo Electrónico de Pago)** - Código 10
  - Obligatorio para ventas a crédito al Estado
  - Registra el momento del pago del IVA

### 4. Validaciones Adicionales

- Moneda obligatoria en NC y ND
- Código CABYS 2025 obligatorio desde 01/06/2025
- Actividad económica CIIU4 (automático con TRIBU-CR desde 06/10/2025)

---

## RESUMEN DE TAREAS

### Alta Prioridad: ✅ COMPLETADO
1. [x] Actualizar catálogo MediosPago con códigos 06 y 07 ✅
2. [x] Actualizar catálogo CondicionVenta con códigos 08-15 ✅
3. [x] Crear catálogo TarifaIVA con 9 tarifas ✅
4. [x] Crear catálogo TipoDescuentoHacienda con 10 códigos ✅
5. [x] Corregir porcentajes en Impuestos ✅
6. [x] Actualizar TiposDocumento con REP (código 10) ✅
7. [x] Actualizar TiposDocumentoReferencia ✅
8. [x] Actualizar CodigosReferencia ✅
9. [x] Actualizar CodigosExoneracion ✅

### Media Prioridad: ✅ COMPLETADO
1. [x] Actualizar TiposIdentificacion con códigos 05, 06, 07 ✅
2. [ ] Implementar validación de actividad económica del receptor
3. [x] Actualizar UnidadesMedida con lista completa ✅

### Baja Prioridad: PENDIENTE
1. [x] REP (Recibo Electrónico de Pago) - Catálogo agregado, servicio ya existe ✅
2. [ ] Implementar desglose de combos/surtidos
3. [ ] Integración con CABYS 2025

### Migración Creada
- Migración: `AddTarifasIVA` - Agrega tabla TarifasIVA al esquema de BD

---

## Notas Adicionales

- La versión 4.3 solo podrá usarse para NC/ND que ajusten comprobantes emitidos durante su vigencia
- 146 cambios totales en la estructura de comprobantes electrónicos respecto a v4.3
- CABYS 2025 incluye nuevos códigos para útiles escolares e higiene menstrual
