# Analisis Exhaustivo de Ejemplos XML - Facturacion Electronica Costa Rica v4.4

## Resumen Ejecutivo

Este documento presenta un analisis detallado de los archivos XML de ejemplo para el sistema de facturacion electronica de Costa Rica, version 4.4. El analisis cubre todos los tipos de documentos electronicos soportados por el Ministerio de Hacienda, incluyendo sus estructuras, campos obligatorios/opcionales, reglas de negocio y validaciones importantes.

### Tipos de Documentos Analizados

| Tipo | Codigo | Namespace | Descripcion |
|------|--------|-----------|-------------|
| Factura Electronica | 01 | `facturaElectronica` | Documento principal de venta |
| Nota de Debito | 02 | `notaDebitoElectronica` | Ajuste a favor del emisor |
| Nota de Credito | 03 | `notaCreditoElectronica` | Ajuste a favor del receptor |
| Tiquete Electronico | 04 | `tiqueteElectronico` | Venta a consumidor final |
| Factura Electronica de Compra | 08 | `facturaElectronicaCompra` | Compras a regimen simplificado o no domiciliados |
| Factura Electronica de Exportacion | 09 | `facturaElectronicaExportacion` | Ventas al exterior |

### Estructura del NumeroConsecutivo (20 digitos)

```
SSSTTTCCCCCCCCCCCCCC
```
- **SSS** (3 digitos): Sucursal (001-999)
- **TTT** (3 digitos): Terminal/Caja (001-999)
- **TT** (2 digitos): Tipo de documento (01-09)
- **CCCCCCCCCC** (10 digitos): Consecutivo

Ejemplo: `00100002090000000025`
- Sucursal: 001
- Terminal: 000
- Tipo: 02 (Nota Debito) - NOTA: En este ejemplo es 09 (Exportacion)
- Consecutivo: 0000000025

---

## 1. Factura Electronica (Tipo 01)

### Namespace
```
https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica
```

### Estructura General

```xml
<?xml version="1.0" encoding="utf-8"?>
<FacturaElectronica xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica">
  <Clave>50615072500310108860006600066010000001131105236601</Clave>
  <ProveedorSistemas>311088600</ProveedorSistemas>
  <CodigoActividadEmisor>474110</CodigoActividadEmisor>
  <NumeroConsecutivo>00100066010000001131</NumeroConsecutivo>
  <FechaEmision>2025-07-15T09:26:42-06:00</FechaEmision>
  <Emisor>...</Emisor>
  <Receptor>...</Receptor>
  <CondicionVenta>01</CondicionVenta>
  <PlazoCredito>0</PlazoCredito>
  <MedioPago>...</MedioPago>
  <DetalleServicio>...</DetalleServicio>
  <OtrosCargos>...</OtrosCargos>
  <ResumenFactura>...</ResumenFactura>
  <ds:Signature>...</ds:Signature>
</FacturaElectronica>
```

### Campos Principales

| Campo | Obligatorio | Descripcion | Formato/Valores |
|-------|-------------|-------------|-----------------|
| Clave | SI | Clave numerica unica | 50 digitos |
| ProveedorSistemas | NO | Cedula del proveedor del sistema | 9-12 digitos |
| CodigoActividadEmisor | SI | Codigo de actividad economica | 6 digitos |
| NumeroConsecutivo | SI | Numero consecutivo del documento | 20 digitos |
| FechaEmision | SI | Fecha y hora de emision | ISO 8601 con zona horaria |
| Emisor | SI | Informacion del emisor | Estructura compleja |
| Receptor | SI | Informacion del receptor | Estructura compleja |
| CondicionVenta | SI | Condicion de la venta | 01-04 |
| PlazoCredito | Condicional | Dias de credito | Numerico (si CondicionVenta=02) |
| MedioPago | SI | Medio(s) de pago | Puede ser multiple |
| DetalleServicio | SI | Lineas de detalle | Minimo 1 linea |
| ResumenFactura | SI | Resumen de totales | Estructura compleja |
| ds:Signature | SI | Firma digital XAdES | Estructura XML Signature |

---

## 2. Tiquete Electronico (Tipo 04)

### Namespace
```
https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico
```

### Diferencias con Factura Electronica

1. **Receptor Opcional**: En tiquetes el receptor es opcional (venta a consumidor final anonimo)
2. **No genera credito fiscal**: El receptor no puede usar el tiquete para credito fiscal
3. **Montos maximos**: Existen limites de monto para emitir tiquetes

### Ejemplo con Surtidos y Descuentos

```xml
<TiqueteElectronico xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico">
  <Clave>50613082500011184077300100003040000000265100238802</Clave>
  <NumeroConsecutivo>00100003040000000265</NumeroConsecutivo>
  <DetalleServicio>
    <LineaDetalle>
      <NumeroLinea>1</NumeroLinea>
      <CodigoCABYS>2349900000000</CodigoCABYS>
      <Cantidad>1.000</Cantidad>
      <UnidadMedida>Unid</UnidadMedida>
      <Detalle>PRODUCTO SURTIDO CON DESCUENTOS</Detalle>
      <PrecioUnitario>1000.00000</PrecioUnitario>
      <MontoTotal>1000.00000</MontoTotal>
      <Descuento>
        <MontoDescuento>50.00000</MontoDescuento>
        <NaturalezaDescuento>Descuento promocional</NaturalezaDescuento>
        <CodigoDescuento>06</CodigoDescuento>
      </Descuento>
      <SubTotal>950.00000</SubTotal>
      <DetalleSurtido>
        <LineaDetalleSurtido>
          <NumeroLineaSurtido>1</NumeroLineaSurtido>
          <CodigoCABYS>2349900000100</CodigoCABYS>
          <Cantidad>2.000</Cantidad>
          <UnidadMedida>Unid</UnidadMedida>
          <Detalle>Componente A del surtido</Detalle>
          <PrecioUnitario>300.00000</PrecioUnitario>
          <MontoTotal>600.00000</MontoTotal>
          <SubTotal>600.00000</SubTotal>
          <Impuesto>
            <Codigo>01</Codigo>
            <CodigoTarifaIVA>08</CodigoTarifaIVA>
            <Tarifa>13.00000</Tarifa>
            <Monto>78.00000</Monto>
          </Impuesto>
        </LineaDetalleSurtido>
        <LineaDetalleSurtido>
          <NumeroLineaSurtido>2</NumeroLineaSurtido>
          <CodigoCABYS>2141100000100</CodigoCABYS>
          <Cantidad>1.000</Cantidad>
          <UnidadMedida>Unid</UnidadMedida>
          <Detalle>Componente B del surtido (canasta basica)</Detalle>
          <PrecioUnitario>350.00000</PrecioUnitario>
          <MontoTotal>350.00000</MontoTotal>
          <SubTotal>350.00000</SubTotal>
          <Impuesto>
            <Codigo>01</Codigo>
            <CodigoTarifaIVA>02</CodigoTarifaIVA>
            <Tarifa>1.00000</Tarifa>
            <Monto>3.50000</Monto>
          </Impuesto>
        </LineaDetalleSurtido>
      </DetalleSurtido>
      <MontoTotalLinea>1031.50000</MontoTotalLinea>
    </LineaDetalle>
  </DetalleServicio>
</TiqueteElectronico>
```

---

## 3. Nota de Credito (Tipo 03)

### Namespace
```
https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica
```

### Campo Obligatorio Adicional: InformacionReferencia

```xml
<NotaCreditoElectronica xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica">
  <Clave>50601011900107090036006500065030000000018182820201</Clave>
  <NumeroConsecutivo>00100065030000000018</NumeroConsecutivo>
  <!-- ... otros campos ... -->
  <InformacionReferencia>
    <TipoDoc>01</TipoDoc>
    <Numero>50601011900107090036006500065010000000017112820201</Numero>
    <FechaEmision>2019-01-01T10:22:45-06:00</FechaEmision>
    <Codigo>01</Codigo>
    <Razon>Devolucion de mercaderia</Razon>
  </InformacionReferencia>
</NotaCreditoElectronica>
```

### Codigos de Referencia (Codigo en InformacionReferencia)

| Codigo | Descripcion |
|--------|-------------|
| 01 | Anula documento de referencia |
| 02 | Corrige texto del documento de referencia |
| 03 | Corrige monto del documento de referencia |
| 04 | Comprobante aportado por Regimen Simplificado |
| 05 | Sustituye comprobante provisional por contingencia |
| 11 | Comprobante de Proveedor No Domiciliado |
| 99 | Otros |

---

## 4. Nota de Debito (Tipo 02)

### Namespace
```
https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaDebitoElectronica
```

### Estructura Similar a Nota de Credito

```xml
<NotaDebitoElectronica xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaDebitoElectronica">
  <Clave>50601011900107090036006500065020000000019182820201</Clave>
  <NumeroConsecutivo>00100065020000000019</NumeroConsecutivo>
  <!-- ... otros campos ... -->
  <InformacionReferencia>
    <TipoDoc>01</TipoDoc>
    <Numero>50601011900107090036006500065010000000017112820201</Numero>
    <FechaEmision>2019-01-01T10:22:45-06:00</FechaEmision>
    <Codigo>03</Codigo>
    <Razon>Ajuste por diferencia en precio</Razon>
  </InformacionReferencia>
</NotaDebitoElectronica>
```

---

## 5. Factura Electronica de Compra (Tipo 08)

### Namespace
```
https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaCompra
```

### Casos de Uso

1. **Compras a Regimen Simplificado**: Cuando el proveedor esta en regimen simplificado
2. **Compras a No Domiciliados**: Cuando el proveedor es extranjero sin domicilio en CR

### Ejemplo: Compra a Regimen Simplificado

```xml
<FacturaElectronicaCompra xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaCompra">
  <Clave>50627072500011184077300100004580000000014107054413</Clave>
  <NumeroConsecutivo>00100004580000000014</NumeroConsecutivo>
  <Emisor>
    <Nombre>EMPRESA COMPRADORA S.A.</Nombre>
    <Identificacion>
      <Tipo>02</Tipo>
      <Numero>3101234567</Numero>
    </Identificacion>
    <!-- El emisor es quien COMPRA -->
  </Emisor>
  <Receptor>
    <Nombre>PROVEEDOR REGIMEN SIMPLIFICADO</Nombre>
    <Identificacion>
      <Tipo>01</Tipo>
      <Numero>123456789</Numero>
    </Identificacion>
    <!-- El receptor es el PROVEEDOR -->
  </Receptor>
  <InformacionReferencia>
    <TipoDocIR>14</TipoDocIR>
    <Numero>FACT-001-2025</Numero>
    <FechaEmision>2025-07-27T10:00:00-06:00</FechaEmision>
    <Codigo>04</Codigo>
    <Razon>Comprobante aportado por Regimen Simplificado</Razon>
  </InformacionReferencia>
</FacturaElectronicaCompra>
```

### Ejemplo: Compra a No Domiciliado

```xml
<FacturaElectronicaCompra xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaCompra">
  <Clave>50627072500011184077300100004580000000017107054413</Clave>
  <NumeroConsecutivo>00100004580000000017</NumeroConsecutivo>
  <Receptor>
    <Nombre>PROVEEDOR EXTRANJERO LLC</Nombre>
    <Identificacion>
      <Tipo>05</Tipo>
      <Numero>000000000</Numero>
    </Identificacion>
    <IdentificacionExtranjero>US-TAX-123456</IdentificacionExtranjero>
    <Ubicacion>
      <Provincia>1</Provincia>
      <Canton>01</Canton>
      <Distrito>01</Distrito>
      <OtrasSenas>N/A</OtrasSenas>
      <OtrasSenasExtranjero>123 Main Street, Miami, FL 33101, USA</OtrasSenasExtranjero>
    </Ubicacion>
    <CorreoElectronico>foreign@provider.com</CorreoElectronico>
  </Receptor>
  <InformacionReferencia>
    <TipoDocIR>16</TipoDocIR>
    <Numero>INV-2025-001234</Numero>
    <FechaEmision>2025-07-20T00:00:00-06:00</FechaEmision>
    <Codigo>11</Codigo>
    <Razon>Comprobante de Proveedor No Domiciliado</Razon>
  </InformacionReferencia>
</FacturaElectronicaCompra>
```

### Tipos de Documento de Referencia (TipoDocIR)

| Codigo | Descripcion |
|--------|-------------|
| 14 | Comprobante Regimen Simplificado |
| 16 | Comprobante Proveedor No Domiciliado |

---

## 6. Factura Electronica de Exportacion (Tipo 09)

### Namespace
```
https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaExportacion
```

### Campos Especificos de Exportacion

```xml
<FacturaElectronicaExportacion xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaExportacion">
  <Clave>50603072500020546033500100002090000000025128839839</Clave>
  <NumeroConsecutivo>00100002090000000025</NumeroConsecutivo>
  <DetalleServicio>
    <LineaDetalle>
      <NumeroLinea>1</NumeroLinea>
      <PartidaArancelaria>012345678912</PartidaArancelaria>
      <CodigoCABYS>3532303000100</CodigoCABYS>
      <Cantidad>1.00</Cantidad>
      <UnidadMedida>Unid</UnidadMedida>
      <Detalle>Producto para exportacion</Detalle>
      <PrecioUnitario>132.80000</PrecioUnitario>
      <MontoTotal>132.80000</MontoTotal>
      <SubTotal>132.80000</SubTotal>
      <Impuesto>
        <Codigo>01</Codigo>
        <CodigoTarifaIVA>08</CodigoTarifaIVA>
        <Tarifa>13.00000</Tarifa>
        <Monto>17.26400</Monto>
        <MontoExportacion>17.26400</MontoExportacion>
      </Impuesto>
      <MontoTotalLinea>150.06400</MontoTotalLinea>
    </LineaDetalle>
  </DetalleServicio>
</FacturaElectronicaExportacion>
```

### Campos Adicionales para Exportacion

| Campo | Obligatorio | Descripcion |
|-------|-------------|-------------|
| PartidaArancelaria | SI | Codigo arancelario del producto (12 digitos) |
| MontoExportacion | SI | Monto del impuesto para exportacion |

---

## 7. Mensaje Hacienda (Respuesta)

### Namespace
```
https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeHacienda
```

### Estructura de Respuesta

```xml
<?xml version="1.0" encoding="utf-8"?>
<MensajeHacienda xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeHacienda">
  <Clave>50613082500011184077300100003010000000410147440128</Clave>
  <Fecha>2025-08-13T16:36:18-06:00</Fecha>
  <IndEstado>procesando</IndEstado>
  <NombreEmisor>EMPRESA EJEMPLO S.A.</NombreEmisor>
  <TipoIdentificacionEmisor>02</TipoIdentificacionEmisor>
  <NumeroCedulaEmisor>3101234567</NumeroCedulaEmisor>
  <Mensaje>1</Mensaje>
  <DetalleMensaje>El comprobante fue aceptado con advertencias:
    El codigo CABYS 2349900000000 no tiene impuesto asociado.</DetalleMensaje>
  <MontoTotalImpuesto>130.00000</MontoTotalImpuesto>
  <TotalFactura>1130.00000</TotalFactura>
  <NombreReceptor>CLIENTE FINAL</NombreReceptor>
  <TipoIdentificacionReceptor>01</TipoIdentificacionReceptor>
  <NumeroCedulaReceptor>123456789</NumeroCedulaReceptor>
</MensajeHacienda>
```

### Estados de Respuesta (Mensaje)

| Codigo | Descripcion |
|--------|-------------|
| 1 | Aceptado |
| 2 | Aceptado parcialmente (con advertencias) |
| 3 | Rechazado |

### Estados de Procesamiento (IndEstado)

| Estado | Descripcion |
|--------|-------------|
| recibido | Documento recibido, pendiente de procesar |
| procesando | Documento en proceso de validacion |
| aceptado | Documento aceptado |
| rechazado | Documento rechazado |

---

## 8. Estructura del Emisor

### Campos del Emisor

```xml
<Emisor>
  <Nombre>EMPRESA DE PRUEBA S.A.</Nombre>
  <Identificacion>
    <Tipo>02</Tipo>
    <Numero>3101234567</Numero>
  </Identificacion>
  <NombreComercial>NOMBRE COMERCIAL</NombreComercial>
  <Ubicacion>
    <Provincia>1</Provincia>
    <Canton>01</Canton>
    <Distrito>01</Distrito>
    <Barrio>01</Barrio>
    <OtrasSenas>100 metros norte del parque central</OtrasSenas>
  </Ubicacion>
  <Telefono>
    <CodigoPais>506</CodigoPais>
    <NumTelefono>22223333</NumTelefono>
  </Telefono>
  <Fax>
    <CodigoPais>506</CodigoPais>
    <NumTelefono>22224444</NumTelefono>
  </Fax>
  <CorreoElectronico>empresa@ejemplo.com</CorreoElectronico>
</Emisor>
```

### Tipos de Identificacion

| Tipo | Descripcion | Formato |
|------|-------------|---------|
| 01 | Cedula Fisica | 9 digitos |
| 02 | Cedula Juridica | 10 digitos |
| 03 | DIMEX | 11-12 digitos |
| 04 | NITE | 10 digitos |
| 05 | No Domiciliado | Variable (para extranjeros) |

---

## 9. Estructura del Receptor

### Campos del Receptor

```xml
<Receptor>
  <Nombre>CLIENTE EJEMPLO</Nombre>
  <Identificacion>
    <Tipo>01</Tipo>
    <Numero>123456789</Numero>
  </Identificacion>
  <IdentificacionExtranjero>TAX-ID-EXTRANJERO</IdentificacionExtranjero>
  <NombreComercial>NOMBRE COMERCIAL CLIENTE</NombreComercial>
  <Ubicacion>
    <Provincia>2</Provincia>
    <Canton>01</Canton>
    <Distrito>12</Distrito>
    <OtrasSenas>Direccion del cliente</OtrasSenas>
    <OtrasSenasExtranjero>Direccion en el extranjero</OtrasSenasExtranjero>
  </Ubicacion>
  <Telefono>
    <CodigoPais>506</CodigoPais>
    <NumTelefono>88887777</NumTelefono>
  </Telefono>
  <CorreoElectronico>cliente@ejemplo.com</CorreoElectronico>
</Receptor>
```

---

## 10. Estructura de Impuestos

### Codigos de Impuesto (Codigo)

| Codigo | Descripcion | Notas |
|--------|-------------|-------|
| 01 | Impuesto al Valor Agregado (IVA) | Impuesto principal |
| 02 | Impuesto Selectivo de Consumo | Productos especificos |
| 03 | Impuesto Unico a los Combustibles | Gasolina, diesel, etc. |
| 04 | Impuesto Especifico de Bebidas Alcoholicas | |
| 05 | Impuesto Especifico de Bebidas sin Alcohol | |
| 06 | Impuesto al Tabaco | Cigarrillos, puros |
| 07 | IVA Calculo Especial | |
| 08 | IVA Bienes Usados | |
| 12 | Impuesto Especifico al Cemento | |
| 98 | Otros | Requiere especificar |
| 99 | Impuesto de Servicio | 10% propinas |

### Codigos de Tarifa IVA (CodigoTarifaIVA)

| Codigo | Tarifa | Descripcion | Uso |
|--------|--------|-------------|-----|
| 01 | 0% | Tarifa 0% | Productos no sujetos |
| 02 | 1% | Tarifa reducida | Canasta basica, insumos agropecuarios |
| 03 | 2% | Tarifa reducida | Medicamentos, educacion privada |
| 04 | 4% | Tarifa reducida | Boletos aereos, seguros |
| 05 | 0% | Transitorio 0% | Periodo transitorio |
| 06 | 1% | Transitorio 1% | Periodo transitorio |
| 07 | 2% | Transitorio 2% | Periodo transitorio |
| 08 | 13% | Tarifa general | Tarifa estandar |
| 10 | 0% | Exento | Productos exentos de IVA |

### Estructura del Impuesto en LineaDetalle

```xml
<Impuesto>
  <Codigo>01</Codigo>
  <CodigoTarifaIVA>08</CodigoTarifaIVA>
  <Tarifa>13.00000</Tarifa>
  <FactorIVA>1.00000</FactorIVA>
  <Monto>130.00000</Monto>
  <MontoExportacion>0.00000</MontoExportacion>
  <Exoneracion>
    <TipoDocumentoEX1>05</TipoDocumentoEX1>
    <NumeroDocumento>AL-001-2025</NumeroDocumento>
    <NombreInstitucion>MINISTERIO DE HACIENDA</NombreInstitucion>
    <FechaEmision>2025-01-01T00:00:00-06:00</FechaEmision>
    <PorcentajeExoneracion>100</PorcentajeExoneracion>
    <MontoExoneracion>130.00000</MontoExoneracion>
  </Exoneracion>
</Impuesto>
```

### Impuestos Especificos (Tabaco, Combustible, etc.)

```xml
<Impuesto>
  <Codigo>06</Codigo>
  <Tarifa>0.00000</Tarifa>
  <Monto>1320.00000</Monto>
  <DatosImpuestoEspecifico>
    <Tipo>C</Tipo>
    <CodigoCABYS>2410201010115</CodigoCABYS>
    <CodigoPartida>240210</CodigoPartida>
    <CantidadBaseImponible>10.00000</CantidadBaseImponible>
    <UnidadMedidaBaseImponible>Unid</UnidadMedidaBaseImponible>
    <MontoBaseImponible>132.00000</MontoBaseImponible>
    <PorcentajeMaximoDescuento>0.00000</PorcentajeMaximoDescuento>
    <TarifaEspecifica>132.00000</TarifaEspecifica>
  </DatosImpuestoEspecifico>
</Impuesto>
```

### Impuesto Asumido por Emisor/Fabrica

```xml
<Impuesto>
  <Codigo>03</Codigo>
  <Tarifa>0.00000</Tarifa>
  <Monto>5000.00000</Monto>
  <ImpuestoAsumidoEmisorFabrica>5000.00000</ImpuestoAsumidoEmisorFabrica>
</Impuesto>
```

---

## 11. Estructura de Descuentos

### Codigos de Descuento (CodigoDescuento)

| Codigo | Descripcion |
|--------|-------------|
| 01 | Regalia |
| 02 | Cortesia |
| 03 | Bonificacion |
| 04 | Volumen |
| 05 | Convenio |
| 06 | Promocion |
| 07 | Ajuste |
| 08 | Frecuencia |
| 09 | Sostenido |
| 99 | Otro (requiere CodigoDescuentoOTRO) |

### Estructura del Descuento

```xml
<Descuento>
  <MontoDescuento>100.00000</MontoDescuento>
  <NaturalezaDescuento>Descuento por volumen de compra</NaturalezaDescuento>
  <CodigoDescuento>04</CodigoDescuento>
</Descuento>
```

### Descuento con Codigo Otro (99)

```xml
<Descuento>
  <MontoDescuento>50.00000</MontoDescuento>
  <NaturalezaDescuento>Descuento especial cliente VIP</NaturalezaDescuento>
  <CodigoDescuento>99</CodigoDescuento>
  <CodigoDescuentoOTRO>DESC-VIP-001</CodigoDescuentoOTRO>
</Descuento>
```

---

## 12. Condiciones de Venta y Medios de Pago

### Condiciones de Venta (CondicionVenta)

| Codigo | Descripcion | Nota |
|--------|-------------|------|
| 01 | Contado | Pago inmediato |
| 02 | Credito | Requiere PlazoCredito |
| 03 | Consignacion | |
| 04 | Apartado | |
| 05 | Arrendamiento con opcion de compra | |
| 06 | Arrendamiento en funcion financiera | |
| 99 | Otros | |

### Medios de Pago (MedioPago)

| Codigo | Descripcion |
|--------|-------------|
| 01 | Efectivo |
| 02 | Tarjeta |
| 03 | Cheque |
| 04 | Transferencia - Deposito bancario |
| 05 | Recaudado por terceros |
| 99 | Otros |

### Estructura de Medio de Pago

```xml
<MedioPago>
  <TipoMedioPago>01</TipoMedioPago>
  <Descripcion>Pago en efectivo</Descripcion>
  <MontoPago>500.00000</MontoPago>
</MedioPago>
<MedioPago>
  <TipoMedioPago>02</TipoMedioPago>
  <Descripcion>Tarjeta de credito VISA</Descripcion>
  <MontoPago>630.00000</MontoPago>
</MedioPago>
```

---

## 13. Linea de Detalle

### Estructura Completa de LineaDetalle

```xml
<LineaDetalle>
  <NumeroLinea>1</NumeroLinea>
  <PartidaArancelaria>012345678912</PartidaArancelaria>
  <CodigoCABYS>4111101010100</CodigoCABYS>
  <CodigoComercial>
    <Tipo>01</Tipo>
    <Codigo>7501234567890</Codigo>
  </CodigoComercial>
  <Cantidad>10.000</Cantidad>
  <UnidadMedida>Unid</UnidadMedida>
  <UnidadMedidaComercial>Caja</UnidadMedidaComercial>
  <Detalle>Descripcion del producto o servicio</Detalle>
  <PrecioUnitario>100.00000</PrecioUnitario>
  <MontoTotal>1000.00000</MontoTotal>
  <Descuento>
    <MontoDescuento>50.00000</MontoDescuento>
    <NaturalezaDescuento>Descuento promocional</NaturalezaDescuento>
    <CodigoDescuento>06</CodigoDescuento>
  </Descuento>
  <SubTotal>950.00000</SubTotal>
  <BaseImponible>950.00000</BaseImponible>
  <Impuesto>
    <Codigo>01</Codigo>
    <CodigoTarifaIVA>08</CodigoTarifaIVA>
    <Tarifa>13.00000</Tarifa>
    <Monto>123.50000</Monto>
  </Impuesto>
  <ImpuestoNeto>123.50000</ImpuestoNeto>
  <MontoTotalLinea>1073.50000</MontoTotalLinea>
</LineaDetalle>
```

### Tipos de Codigo Comercial

| Tipo | Descripcion |
|------|-------------|
| 01 | Codigo del producto del vendedor |
| 02 | Codigo del producto del comprador |
| 03 | Codigo del producto asignado por la industria |
| 04 | Codigo de uso interno |
| 99 | Otros |

### Unidades de Medida Comunes

| Codigo | Descripcion |
|--------|-------------|
| Unid | Unidad |
| Kg | Kilogramo |
| g | Gramo |
| L | Litro |
| mL | Mililitro |
| m | Metro |
| cm | Centimetro |
| m2 | Metro cuadrado |
| m3 | Metro cubico |
| Sp | Servicios profesionales |
| Spe | Servicios personales |
| St | Servicios tecnicos |
| Os | Otros servicios |

---

## 14. Productos con Registro de Medicamento

### Estructura para Productos Farmaceuticos

```xml
<LineaDetalle>
  <NumeroLinea>1</NumeroLinea>
  <CodigoCABYS>4941100019900</CodigoCABYS>
  <RegistroMedicamento>M-12345-2024</RegistroMedicamento>
  <FormaFarmaceutica>01</FormaFarmaceutica>
  <Cantidad>100.000</Cantidad>
  <UnidadMedida>Unid</UnidadMedida>
  <Detalle>ACETAMINOFEN 500MG TABLETAS</Detalle>
  <PrecioUnitario>50.00000</PrecioUnitario>
  <MontoTotal>5000.00000</MontoTotal>
  <SubTotal>5000.00000</SubTotal>
  <Impuesto>
    <Codigo>01</Codigo>
    <CodigoTarifaIVA>01</CodigoTarifaIVA>
    <Tarifa>0.00000</Tarifa>
    <Monto>0.00000</Monto>
  </Impuesto>
  <MontoTotalLinea>5000.00000</MontoTotalLinea>
</LineaDetalle>
```

---

## 15. Exoneraciones

### Tipos de Documento de Exoneracion (TipoDocumentoEX1)

| Codigo | Descripcion |
|--------|-------------|
| 01 | Compras autorizadas |
| 02 | Ventas exentas a diplomáticos |
| 03 | Orden de Compra (instituciones públicas) |
| 04 | Exenciones Ministerio Hacienda |
| 05 | Zonas Francas |
| 06 | Otros |
| 99 | Otros (especificar) |

### Estructura de Exoneracion

```xml
<Exoneracion>
  <TipoDocumentoEX1>05</TipoDocumentoEX1>
  <NumeroDocumento>ZF-2025-001234</NumeroDocumento>
  <NombreInstitucion>PROCOMER</NombreInstitucion>
  <FechaEmision>2025-01-15T00:00:00-06:00</FechaEmision>
  <PorcentajeExoneracion>100</PorcentajeExoneracion>
  <MontoExoneracion>130.00000</MontoExoneracion>
</Exoneracion>
```

---

## 16. Resumen de Factura

### Estructura Completa del ResumenFactura

```xml
<ResumenFactura>
  <CodigoTipoMoneda>
    <CodigoMoneda>CRC</CodigoMoneda>
    <TipoCambio>1.00000</TipoCambio>
  </CodigoTipoMoneda>
  <TotalServGravados>0.00000</TotalServGravados>
  <TotalServExentos>0.00000</TotalServExentos>
  <TotalServExonerado>0.00000</TotalServExonerado>
  <TotalServNoSujeto>0.00000</TotalServNoSujeto>
  <TotalMercanciasGravadas>1000.00000</TotalMercanciasGravadas>
  <TotalMercanciasExentas>0.00000</TotalMercanciasExentas>
  <TotalMercExonerada>0.00000</TotalMercExonerada>
  <TotalMercNoSujeta>0.00000</TotalMercNoSujeta>
  <TotalGravado>1000.00000</TotalGravado>
  <TotalExento>0.00000</TotalExento>
  <TotalExonerado>0.00000</TotalExonerado>
  <TotalNoSujeto>0.00000</TotalNoSujeto>
  <TotalVenta>1000.00000</TotalVenta>
  <TotalDescuentos>50.00000</TotalDescuentos>
  <TotalVentaNeta>950.00000</TotalVentaNeta>
  <TotalDesgloseImpuesto>
    <Codigo>01</Codigo>
    <CodigoTarifaIVA>08</CodigoTarifaIVA>
    <TotalMontoImpuesto>123.50000</TotalMontoImpuesto>
  </TotalDesgloseImpuesto>
  <TotalImpuesto>123.50000</TotalImpuesto>
  <TotalIVADevuelto>0.00000</TotalIVADevuelto>
  <TotalImpAsumEmisorFabrica>0.00000</TotalImpAsumEmisorFabrica>
  <TotalOtrosCargos>0.00000</TotalOtrosCargos>
  <TotalComprobante>1073.50000</TotalComprobante>
</ResumenFactura>
```

### Codigos de Moneda Comunes

| Codigo | Descripcion |
|--------|-------------|
| CRC | Colon Costarricense |
| USD | Dolar Estadounidense |
| EUR | Euro |

---

## 17. Otros Cargos

### Tipos de Otros Cargos

| Tipo | Descripcion |
|------|-------------|
| 01 | Contribucion parafiscal |
| 02 | Timbre de la Cruz Roja |
| 03 | Timbre de Benemérito Cuerpo de Bomberos de Costa Rica |
| 04 | Cobro de un tercero |
| 05 | Costos de exportación |
| 06 | Impuesto de servicio 10% |
| 07 | Timbre de Colegios Profesionales |
| 99 | Otros |

### Estructura de OtrosCargos

```xml
<OtrosCargos>
  <TipoDocumento>06</TipoDocumento>
  <Detalle>Impuesto de servicio 10%</Detalle>
  <Porcentaje>10.00000</Porcentaje>
  <MontoCargo>100.00000</MontoCargo>
</OtrosCargos>
```

---

## 18. Estructura de la Clave Numerica (50 digitos)

### Formato Detallado

```
PPPDDMMAAEEEEEEEEEEEECSSSTTTCCCCCCCCCCCCCCCCCSSCCCCCCCCV
```

| Posicion | Longitud | Campo | Descripcion |
|----------|----------|-------|-------------|
| 1-3 | 3 | Pais | 506 (Costa Rica) |
| 4-5 | 2 | Dia | Dia de emision (01-31) |
| 6-7 | 2 | Mes | Mes de emision (01-12) |
| 8-9 | 2 | Ano | Ultimos 2 digitos del ano |
| 10-21 | 12 | Cedula | Cedula del emisor (padded left with 0) |
| 22-41 | 20 | Consecutivo | Numero consecutivo completo |
| 42-43 | 2 | Situacion | 01=Normal, 02=Contingencia, 03=Sin internet |
| 44-51 | 8 | Seguridad | Codigo aleatorio |
| 52 | 1 | Verificador | Digito verificador (Modulo 11) |

### Ejemplo Desglosado

Clave: `50615072500310108860006600066010000001131105236601`

| Campo | Valor | Significado |
|-------|-------|-------------|
| Pais | 506 | Costa Rica |
| Dia | 15 | Dia 15 |
| Mes | 07 | Julio |
| Ano | 25 | 2025 |
| Cedula | 003101088600 | Cedula 3101088600 (juridica) |
| Consecutivo | 00100066010000001131 | Suc 001, Term 000, Tipo 66, Num 10000001131 |
| Situacion | 01 | Situacion Normal |
| Seguridad | 05236601 | Codigo aleatorio |

---

## 19. Reglas de Negocio Identificadas

### Validaciones de Totales

1. **MontoTotal** = Cantidad * PrecioUnitario
2. **SubTotal** = MontoTotal - MontoDescuento
3. **MontoTotalLinea** = SubTotal + Impuesto.Monto - MontoExoneracion
4. **TotalVenta** = TotalGravado + TotalExento + TotalExonerado + TotalNoSujeto
5. **TotalVentaNeta** = TotalVenta - TotalDescuentos
6. **TotalComprobante** = TotalVentaNeta + TotalImpuesto + TotalOtrosCargos - TotalIVADevuelto

### Reglas de Impuestos

1. Si CodigoTarifaIVA = 01 o 10, entonces Tarifa = 0 y Monto = 0
2. Si hay Exoneracion, el PorcentajeExoneracion debe ser entre 0 y 100
3. MontoExoneracion = Monto * (PorcentajeExoneracion / 100)
4. ImpuestoNeto = Monto - MontoExoneracion

### Reglas de Documentos de Referencia

1. NC y ND **requieren** InformacionReferencia
2. FEC **requiere** InformacionReferencia con TipoDocIR
3. El TipoDoc debe corresponder a un documento valido existente
4. La FechaEmision de referencia no puede ser futura

### Reglas de Identificacion

1. Cedula Fisica (Tipo 01): Exactamente 9 digitos
2. Cedula Juridica (Tipo 02): Exactamente 10 digitos
3. DIMEX (Tipo 03): 11-12 digitos
4. NITE (Tipo 04): 10 digitos
5. No Domiciliado (Tipo 05): Requiere IdentificacionExtranjero

---

## 20. Validaciones Importantes

### Antes de Enviar a Hacienda

1. **Validacion XSD**: El XML debe ser valido contra el esquema oficial
2. **Clave Numerica**: Verificar los 50 digitos y el digito verificador
3. **Consecutivo**: Debe ser unico y secuencial
4. **Fecha de Emision**: No puede ser futura, formato ISO 8601
5. **Codigo CABYS**: Debe existir en el catalogo oficial
6. **Codigo Actividad**: Debe estar registrado para el emisor
7. **Calculos**: Verificar todos los totales matematicamente
8. **Firma Digital**: Certificado vigente y firma XAdES-BES valida

### Errores Comunes de Hacienda

| Codigo | Descripcion | Solucion |
|--------|-------------|----------|
| 01 | XML mal formado | Validar contra XSD |
| 02 | Firma digital invalida | Verificar certificado y algoritmo |
| 03 | Clave duplicada | Generar nueva clave |
| 04 | Emisor no autorizado | Verificar registro en ATV |
| 05 | Receptor invalido | Verificar cedula del receptor |
| 06 | Calculos incorrectos | Revisar formulas de totales |
| 07 | Actividad no registrada | Registrar actividad en ATV |

---

## 21. Ejemplos de Casos Especiales

### Producto No Sujeto a IVA

```xml
<LineaDetalle>
  <NumeroLinea>1</NumeroLinea>
  <CodigoCABYS>4941100019900</CodigoCABYS>
  <Detalle>Medicamento exento de IVA</Detalle>
  <Impuesto>
    <Codigo>01</Codigo>
    <CodigoTarifaIVA>01</CodigoTarifaIVA>
    <Tarifa>0.00000</Tarifa>
    <Monto>0.00000</Monto>
  </Impuesto>
</LineaDetalle>
```

### Producto con 100% Exoneracion

```xml
<Impuesto>
  <Codigo>01</Codigo>
  <CodigoTarifaIVA>08</CodigoTarifaIVA>
  <Tarifa>13.00000</Tarifa>
  <Monto>130.00000</Monto>
  <Exoneracion>
    <TipoDocumentoEX1>05</TipoDocumentoEX1>
    <NumeroDocumento>ZF-2025-001</NumeroDocumento>
    <NombreInstitucion>PROCOMER</NombreInstitucion>
    <FechaEmision>2025-01-01T00:00:00-06:00</FechaEmision>
    <PorcentajeExoneracion>100</PorcentajeExoneracion>
    <MontoExoneracion>130.00000</MontoExoneracion>
  </Exoneracion>
</Impuesto>
```

### Multiple Impuestos en una Linea (IVA + Tabaco)

```xml
<LineaDetalle>
  <Impuesto>
    <Codigo>01</Codigo>
    <CodigoTarifaIVA>08</CodigoTarifaIVA>
    <Tarifa>13.00000</Tarifa>
    <Monto>130.00000</Monto>
  </Impuesto>
  <Impuesto>
    <Codigo>06</Codigo>
    <Tarifa>0.00000</Tarifa>
    <Monto>1320.00000</Monto>
    <DatosImpuestoEspecifico>
      <Tipo>C</Tipo>
      <CodigoCABYS>2410201010115</CodigoCABYS>
      <TarifaEspecifica>132.00000</TarifaEspecifica>
    </DatosImpuestoEspecifico>
  </Impuesto>
</LineaDetalle>
```

---

## 22. Namespaces Oficiales v4.4

```csharp
public static class NamespacesV44
{
    public const string FacturaElectronica =
        "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica";
    public const string NotaCredito =
        "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica";
    public const string NotaDebito =
        "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaDebitoElectronica";
    public const string Tiquete =
        "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico";
    public const string FacturaCompra =
        "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaCompra";
    public const string FacturaExportacion =
        "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaExportacion";
    public const string MensajeReceptor =
        "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeReceptor";
    public const string MensajeHacienda =
        "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeHacienda";
    public const string DigitalSignature =
        "http://www.w3.org/2000/09/xmldsig#";
    public const string XAdES =
        "http://uri.etsi.org/01903/v1.3.2#";
}
```

---

## 23. URLs de Referencia Oficiales

### Esquemas XSD v4.4
- Factura Electronica: https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/FacturaElectronica_V4.4.xsd
- Nota Debito: https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/NotaDebitoElectronica_V4.4.xsd
- Nota Credito: https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/NotaCreditoElectronica_V4.4.xsd
- Tiquete Electronico: https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/TiqueteElectronico_V4.4.xsd
- Factura Compra: https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/FacturaElectronicaCompra_V4.4.xsd
- Factura Exportacion: https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/FacturaElectronicaExportacion_V4.4.xsd
- Mensaje Hacienda: https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/MensajeHacienda_V4.4.xsd
- Mensaje Receptor: https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/MensajeReceptor_V4.4.xsd

### APIs de Hacienda
- **Staging IDP**: https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token
- **Staging API**: https://api-sandbox.comprobanteselectronicos.go.cr/recepcion/v1/
- **Production IDP**: https://idp.comprobanteselectronicos.go.cr/auth/realms/rut/protocol/openid-connect/token
- **Production API**: https://api.comprobanteselectronicos.go.cr/recepcion/v1/

### Documentacion Oficial
- ANEXOS Y ESTRUCTURAS v4.4: https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/ANEXOS%20Y%20ESTRUCTURAS_V4.4.pdf

---

## 24. Firma Digital XAdES-BES

### Estructura de la Firma

```xml
<ds:Signature xmlns:ds="http://www.w3.org/2000/09/xmldsig#" Id="xmldsig-signature">
  <ds:SignedInfo>
    <ds:CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315"/>
    <ds:SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"/>
    <ds:Reference Id="xmldsig-ref0" URI="">
      <ds:Transforms>
        <ds:Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/>
      </ds:Transforms>
      <ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
      <ds:DigestValue>...</ds:DigestValue>
    </ds:Reference>
    <ds:Reference URI="#xmldsig-keyinfo">
      <ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
      <ds:DigestValue>...</ds:DigestValue>
    </ds:Reference>
    <ds:Reference Type="http://uri.etsi.org/01903#SignedProperties" URI="#xmldsig-signedprops">
      <ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
      <ds:DigestValue>...</ds:DigestValue>
    </ds:Reference>
  </ds:SignedInfo>
  <ds:SignatureValue>...</ds:SignatureValue>
  <ds:KeyInfo Id="xmldsig-keyinfo">
    <ds:X509Data>
      <ds:X509Certificate>...</ds:X509Certificate>
    </ds:X509Data>
    <ds:KeyValue>
      <ds:RSAKeyValue>
        <ds:Modulus>...</ds:Modulus>
        <ds:Exponent>...</ds:Exponent>
      </ds:RSAKeyValue>
    </ds:KeyValue>
  </ds:KeyInfo>
  <ds:Object>
    <xades:QualifyingProperties xmlns:xades="http://uri.etsi.org/01903/v1.3.2#" Target="#xmldsig-signature">
      <xades:SignedProperties Id="xmldsig-signedprops">
        <xades:SignedSignatureProperties>
          <xades:SigningTime>2025-07-15T09:26:42-06:00</xades:SigningTime>
          <xades:SigningCertificate>
            <xades:Cert>
              <xades:CertDigest>
                <ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
                <ds:DigestValue>...</ds:DigestValue>
              </xades:CertDigest>
              <xades:IssuerSerial>
                <ds:X509IssuerName>...</ds:X509IssuerName>
                <ds:X509SerialNumber>...</ds:X509SerialNumber>
              </xades:IssuerSerial>
            </xades:Cert>
          </xades:SigningCertificate>
          <xades:SignaturePolicyIdentifier>
            <xades:SignaturePolicyImplied/>
          </xades:SignaturePolicyIdentifier>
        </xades:SignedSignatureProperties>
        <xades:SignedDataObjectProperties>
          <xades:DataObjectFormat ObjectReference="#xmldsig-ref0">
            <xades:MimeType>text/xml</xades:MimeType>
            <xades:Encoding>UTF-8</xades:Encoding>
          </xades:DataObjectFormat>
        </xades:SignedDataObjectProperties>
      </xades:SignedProperties>
    </xades:QualifyingProperties>
  </ds:Object>
</ds:Signature>
```

### Requisitos de la Firma

1. **Algoritmo**: RSA-SHA256
2. **Tipo**: XAdES-BES (enveloped signature)
3. **Certificado**: .p12 emitido por CA autorizada (Firma Digital CR)
4. **Referencias firmadas**: Documento completo + KeyInfo + SignedProperties
5. **SigningTime**: Momento de la firma en formato ISO 8601

---

## 25. Diferencias entre Gravado, Exento, Exonerado y No Sujeto

| Tipo | CodigoTarifaIVA | Descripcion | Ejemplo |
|------|-----------------|-------------|---------|
| **Gravado** | 02, 03, 04, 08 | Producto sujeto a IVA con tarifa > 0% | Electrodomesticos (13%), Canasta basica (1%) |
| **Exento** | 10 | Producto exento por ley (no paga IVA) | Exportaciones, transporte publico |
| **Exonerado** | 08 con Exoneracion | Producto gravado pero con autorizacion de exoneracion | Compras de zonas francas |
| **No Sujeto** | 01 | Producto fuera del ambito del IVA | Intereses financieros, dividendos |

### Campos en ResumenFactura

```xml
<ResumenFactura>
  <!-- Servicios -->
  <TotalServGravados>0.00000</TotalServGravados>
  <TotalServExentos>0.00000</TotalServExentos>
  <TotalServExonerado>0.00000</TotalServExonerado>
  <TotalServNoSujeto>0.00000</TotalServNoSujeto>

  <!-- Mercancias -->
  <TotalMercanciasGravadas>1000.00000</TotalMercanciasGravadas>
  <TotalMercanciasExentas>0.00000</TotalMercanciasExentas>
  <TotalMercExonerada>0.00000</TotalMercExonerada>
  <TotalMercNoSujeta>0.00000</TotalMercNoSujeta>

  <!-- Totales consolidados -->
  <TotalGravado>1000.00000</TotalGravado>
  <TotalExento>0.00000</TotalExento>
  <TotalExonerado>0.00000</TotalExonerado>
  <TotalNoSujeto>0.00000</TotalNoSujeto>
</ResumenFactura>
```

---

## 26. Archivos de Ejemplo Analizados

| Archivo | Tipo | Caracteristicas Especiales |
|---------|------|---------------------------|
| FAC_EXP_*.xml | Exportacion (09) | PartidaArancelaria, MontoExportacion |
| TABACO_IVA13_FE-*.XML | Factura (01) | Impuesto tabaco (06), DatosImpuestoEspecifico |
| COMBUSTIBLE_IVA13_FE-*.XML | Factura (01) | Impuesto combustible (03), ImpuestoAsumidoEmisorFabrica |
| MEDIO_PAGO_FE-*.XML | Factura (01) | Multiples medios de pago, TipoTransaccion |
| FE con surtidos-*.xml | Factura (01) | DetalleSurtido, Exoneracion 100% |
| FACTURA No sujeto.XML | Factura (01) | CodigoTarifaIVA 01, RegistroMedicamento |
| FEC regimen simplificado.XML | Compra (08) | TipoDocIR 14, Codigo 04 |
| FEC No domiciliado.XML | Compra (08) | Tipo 05, OtrasSenasExtranjero, TipoDocIR 16 |
| RESP_*.xml | Respuesta | MensajeHacienda, DetalleMensaje con advertencias |
| surtidos_y_descuentos.XML | Tiquete (04) | DetalleSurtido con IVA mixto (13% y 1%) |

---

*Documento generado para el proyecto de Facturacion Electronica Costa Rica v4.4*
*Ultima actualizacion: Diciembre 2025*
