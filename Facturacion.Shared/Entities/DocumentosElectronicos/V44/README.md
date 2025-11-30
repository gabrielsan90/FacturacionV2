# Clases C# para Documentos Electrónicos v4.4 - Costa Rica

Este directorio contiene las clases C# generadas a partir de los esquemas XSD v4.4 del Ministerio de Hacienda de Costa Rica para la facturación electrónica.

## Fecha de Generación
21 de noviembre de 2025

## Versión
4.4 - Obligatoria desde el 1 de septiembre de 2025

## Estructura de Archivos

### Tipos de Documentos Principales

#### 1. **FacturaElectronica.cs** (Tipo 01)
- Factura Electrónica estándar
- Namespace: `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica`
- Uso: Ventas regulares a contribuyentes inscritos

#### 2. **NotaDebitoElectronica.cs** (Tipo 02)
- Nota de Débito Electrónica
- Namespace: `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaDebitoElectronica`
- Uso: Ajustes positivos a facturas previamente emitidas

#### 3. **NotaCreditoElectronica.cs** (Tipo 03)
- Nota de Crédito Electrónica
- Namespace: `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica`
- Uso: Ajustes negativos, devoluciones, descuentos posteriores

#### 4. **TiqueteElectronico.cs** (Tipo 04)
- Tiquete Electrónico
- Namespace: `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico`
- Uso: Ventas al consumidor final, ventas menores

#### 5. **MensajeReceptor.cs** (Tipos 05, 06, 07)
- Mensaje de aceptación/rechazo por parte del receptor
- Namespace: `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeReceptor`
- Uso: Respuesta del receptor a documentos recibidos

#### 6. **MensajeHacienda.cs** (Tipos 05, 06, 07)
- Mensaje de Hacienda (uso exclusivo DGT)
- Namespace: `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeHacienda`
- Uso: Comunicaciones oficiales del Ministerio de Hacienda

#### 7. **FacturaElectronicaCompra.cs** (Tipo 08)
- Factura Electrónica de Compra
- Namespace: `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaCompra`
- Uso: Auto-facturación para compras a no obligados

#### 8. **FacturaElectronicaExportacion.cs** (Tipo 09)
- Factura Electrónica de Exportación
- Namespace: `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaExportacion`
- Uso: Ventas de exportación fuera de Costa Rica

#### 9. **ReciboElectronicoPago.cs** (Tipo 10) - NUEVO EN v4.4
- Recibo Electrónico de Pago
- Namespace: `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/reciboElectronicoPago`
- Uso: **OBLIGATORIO** para ventas a crédito con IVA hasta 90 días
- Condiciones: 09 (Pago servicios al Estado), 11 (Pago venta a crédito IVA)

### Tipos Comunes (Carpeta TiposComunes/)

#### Tipos de Identificación y Ubicación
- **IdentificacionType.cs**: Tipo e identificación de personas
  - Tipos: 01 Física, 02 Jurídica, 03 DIMEX, 04 NITE, 05 Extranjero, 06 No Contribuyente

- **UbicacionType.cs**: Ubicación geográfica según división territorial
  - Provincia, Cantón, Distrito, Barrio, Otras Señas

- **TelefonoType.cs**: Teléfono con código de país (ITU-T E.164)

#### Tipos de Emisor y Receptor
- **EmisorType.cs**: Información completa del emisor
  - Nombre, Identificación, Ubicación, Teléfono, Correos (hasta 4 en v4.4)

- **ReceptorType.cs**: Información del receptor
  - **NUEVO v4.4**: Campo `ActividadEconomica` (CIIU4, 6 dígitos) **OBLIGATORIO**

#### Tipos de Línea de Detalle
- **LineaDetalleType.cs**: Detalle de productos/servicios
  - CAByS (13 dígitos) - obligatorio desde 01/06/2025
  - VIN para vehículos (17 caracteres)
  - Campos farmacéuticos (NumeroRegistro, FormaFarmaceutica) - obligatorios desde 01/12/2024
  - Descuentos, impuestos, exoneraciones

#### Tipos de Impuestos
- **ImpuestoType.cs**: Impuestos aplicados
  - Códigos: 01 IVA, 02 ISC, 03 Combustibles, 04 Alcohólicas, 05 Bebidas sin alcohol, 06 Tabaco, 07 IVA especial, 08 Bienes Usados, 12 Cemento, 99 Otros
  - CodigoTarifaIVA: 01-11 (diferentes tarifas de IVA)
  - Exoneraciones con documentación completa

#### Otros Tipos
- **CodigoMonedaType.cs**: Moneda según ISO 4217 con tipo de cambio
- **MedioPagoType.cs**: Medios de pago
  - **NUEVO v4.4**: 06 SINPE MÓVIL
  - Hasta 4 medios de pago por documento

- **InformacionReferenciaType.cs**: Referencias a otros documentos
  - Hasta 10 referencias por documento

- **DescuentoType.cs**: Descuentos aplicados a líneas

## Cambios Importantes en v4.4

### 1. Campo ActividadEconomicaReceptor
- **OBLIGATORIO** en facturas desde v4.4
- Código CIIU4 de 6 dígitos
- Reemplaza CIIU3 desde 01/09/2025

### 2. Recibo Electrónico de Pago (REP)
- **NUEVO** documento tipo 10
- **OBLIGATORIO** para ventas a crédito con IVA hasta 90 días
- Condiciones: 09 o 11 solamente

### 3. SINPE Móvil
- Nuevo medio de pago: código 06
- Disponible en todos los documentos

### 4. Correos Electrónicos
- Hasta **4 direcciones** de email permitidas (antes solo 1)
- Aplicable a emisor y receptor

### 5. CAByS 2025
- Obligatorio desde 01/06/2025
- 13 dígitos (formato actualizado)

### 6. Productos Farmacéuticos
- Campos obligatorios desde 01/12/2024:
  - NumeroRegistro
  - FormaFarmaceutica

### 7. Nueva Condición de Venta
- Código 10: "Mercancía no nacionalizada"

## Validaciones Implementadas

Todas las clases incluyen:

1. **DataAnnotations** para validación:
   - `[Required]` para campos obligatorios
   - `[StringLength]` para límites de caracteres
   - `[RegularExpression]` para patrones específicos
   - `[Range]` para valores numéricos

2. **Atributos XML** para serialización:
   - `[XmlRoot]` con namespace correcto
   - `[XmlElement]` para mapeo de elementos
   - `[XmlAttribute]` para atributos XML

3. **Comentarios XML** (///) para IntelliSense:
   - Descripción de cada clase
   - Descripción de cada propiedad
   - Valores permitidos y restricciones

## Uso Básico

```csharp
using Facturacion.Shared.Entities.DocumentosElectronicos.V44;
using Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

// Crear una factura electrónica
var factura = new FacturaElectronica
{
    Clave = "50601012500031159640000100001010000000001190000001",
    CodigoActividad = "522100", // CIIU4
    NumeroConsecutivo = "00100001010000000001",
    FechaEmision = DateTime.Now,
    Emisor = new EmisorType
    {
        Nombre = "Empresa SA",
        Identificacion = new IdentificacionType
        {
            Tipo = "02", // Jurídica
            Numero = "3101234567"
        },
        // ... más campos
    },
    // ... resto de la factura
};

// Serializar a XML
var serializer = new XmlSerializer(typeof(FacturaElectronica));
using var writer = new StringWriter();
serializer.Serialize(writer, factura);
string xml = writer.ToString();
```

## Serialización XML

Las clases están diseñadas para serialización/deserialización XML directa usando `System.Xml.Serialization.XmlSerializer`.

### Ejemplo de Serialización:
```csharp
public static string SerializarDocumento<T>(T documento)
{
    var serializer = new XmlSerializer(typeof(T));
    var settings = new XmlWriterSettings
    {
        Indent = true,
        Encoding = Encoding.UTF8,
        OmitXmlDeclaration = false
    };

    using var stringWriter = new StringWriter();
    using var xmlWriter = XmlWriter.Create(stringWriter, settings);
    serializer.Serialize(xmlWriter, documento);
    return stringWriter.ToString();
}
```

### Ejemplo de Deserialización:
```csharp
public static T DeserializarDocumento<T>(string xml)
{
    var serializer = new XmlSerializer(typeof(T));
    using var stringReader = new StringReader(xml);
    return (T)serializer.Deserialize(stringReader);
}
```

## Validación de Documentos

```csharp
using System.ComponentModel.DataAnnotations;

public static bool ValidarDocumento<T>(T documento, out List<ValidationResult> errores)
{
    errores = new List<ValidationResult>();
    var contexto = new ValidationContext(documento);
    return Validator.TryValidateObject(documento, contexto, errores, validateAllProperties: true);
}
```

## Consideraciones de Implementación

### 1. Firma Digital
- El campo `Signature` está definido como `object?`
- Debe implementarse usando XAdES-EPES según especificaciones de Hacienda
- Requiere certificado digital válido emitido por autoridad certificadora autorizada

### 2. Clave Numérica (50 dígitos)
- Formato: PPSSSSSSSSSSSSSDTTTNNNNNNNNNNCCCCCCCCCCCSSSSSSSS
- PP: País (50 para Costa Rica)
- DD/MM/YYYY: Fecha de emisión
- T: Tipo de identificación emisor
- NNN: Número de cédula (12 dígitos)
- CCC: Consecutivo (20 dígitos)
- SSS: Situación (01-03) + código de seguridad

### 3. Consecutivo (20 dígitos)
- Formato: XXX-YYYYY-ZZ-AAAAAAAAAA
- XXX: Sucursal (001-999)
- YYYYY: Punto de venta (00001-99999)
- ZZ: Tipo de documento (01-10)
- AAAAAAAAAA: Número secuencial (0000000001-9999999999)

### 4. Códigos CIIU4
- 6 dígitos obligatorios desde 01/09/2025
- Reemplaza CIIU3 (5 dígitos)
- Último día para CIIU3: 31/08/2025

### 5. Precisión de Montos
- Todos los montos: hasta 5 decimales
- Cálculos: mantener precisión, redondear solo al final
- Tipo: `decimal` (no `float` ni `double`)

## Timeline de Obligatoriedad v4.4

- **01/12/2024**: Campos farmacéuticos obligatorios
- **01/04/2025**: v4.4 período voluntario
- **01/06/2025**: CAByS 2025 obligatorio
- **02/06/2025**: Migración a Tribu-CR
- **31/08/2025**: Último día CIIU3
- **01/09/2025**: v4.4 OBLIGATORIO
- **06/10/2025**: Solo CIIU4 aceptado

## Soporte y Documentación Oficial

- **Portal Hacienda**: https://www.hacienda.go.cr/ATV/ComprobanteElectronico/
- **XSD Oficiales**: Carpeta `/4.4/` en este proyecto
- **Resolución**: MH-DGT-RES-0027-2024

## Notas Técnicas

1. **Encoding**: UTF-8 para todos los documentos XML
2. **Formato Fecha**: ISO 8601 con timezone (yyyy-MM-ddTHH:mm:sszzz)
3. **Namespace**: Cada documento tiene su namespace específico v4.4
4. **Validación**: XSD validación + validación de negocio en Hacienda
5. **Retención**: 5 años obligatorios para documentos electrónicos

## Dependencias

- .NET 9.0
- System.ComponentModel.DataAnnotations
- System.Xml.Serialization

## Autores

Generado a partir de los esquemas XSD oficiales v4.4 del Ministerio de Hacienda de Costa Rica.

## Licencia

Las clases generadas son de uso libre para implementaciones del sistema de facturación electrónica de Costa Rica.
