# Ejemplos de Uso - Documentos Electrónicos v4.4

## Ejemplo 1: Crear una Factura Electrónica (FE)

```csharp
using Facturacion.Shared.Entities.DocumentosElectronicos.V44;
using Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

// Generar clave numérica (50 dígitos)
string clave = GenerarClaveNumerica(
    pais: "506",
    dia: "01",
    mes: "12",
    year: "2025",
    tipoIdentificacion: "02",
    numeroIdentificacion: "003101234567",
    consecutivo: "00100001010000000001",
    situacion: "1",
    codigoSeguridad: "12345678"
);

var factura = new FacturaElectronica
{
    Clave = clave,
    CodigoActividad = "522100", // Código CIIU4 (6 dígitos)
    NumeroConsecutivo = "00100001010000000001", // 20 dígitos
    FechaEmision = DateTime.Now,

    Emisor = new EmisorType
    {
        Nombre = "EMPRESA DEMO SOCIEDAD ANONIMA",
        Identificacion = new IdentificacionType
        {
            Tipo = "02", // Cédula Jurídica
            Numero = "3101234567"
        },
        NombreComercial = "Empresa Demo",
        Ubicacion = new UbicacionType
        {
            Provincia = "1", // San José
            Canton = "01", // San José
            Distrito = "01", // Carmen
            Barrio = "01",
            OtrasSenas = "200 metros norte de la iglesia"
        },
        Telefono = new TelefonoType
        {
            CodigoPais = "506",
            NumTelefono = "22223333"
        },
        CorreoElectronico = new List<string>
        {
            "facturacion@empresademo.cr",
            "contabilidad@empresademo.cr"
        }
    },

    Receptor = new ReceptorType
    {
        Nombre = "CLIENTE EJEMPLO SA",
        Identificacion = new IdentificacionType
        {
            Tipo = "02",
            Numero = "3102345678"
        },
        ActividadEconomica = "522100", // OBLIGATORIO en v4.4
        Ubicacion = new UbicacionType
        {
            Provincia = "1",
            Canton = "01",
            Distrito = "02",
            OtrasSenas = "Edificio Central, Piso 5"
        },
        CorreoElectronico = new List<string> { "compras@cliente.cr" }
    },

    CondicionVenta = "01", // Contado

    MedioPago = new List<MedioPagoType>
    {
        new MedioPagoType
        {
            TipoMedioPago = "06", // SINPE Móvil (NUEVO en v4.4)
            TotalMedioPago = 56500.00m
        }
    },

    DetalleServicio = new DetalleServicioType
    {
        LineaDetalle = new List<LineaDetalleType>
        {
            new LineaDetalleType
            {
                NumeroLinea = 1,
                CodigoCaByS = "8517120100000", // 13 dígitos (obligatorio desde 01/06/2025)
                Cantidad = 2,
                UnidadMedida = "Unid",
                Detalle = "Teléfono celular inteligente",
                PrecioUnitario = 25000.00m,
                MontoTotal = 50000.00m,
                SubTotal = 50000.00m,
                Impuesto = new List<ImpuestoType>
                {
                    new ImpuestoType
                    {
                        Codigo = "01", // IVA
                        CodigoTarifaIVA = "08", // 13%
                        Tarifa = 13.00m,
                        Monto = 6500.00m
                    }
                },
                ImpuestoNeto = 6500.00m,
                MontoTotalLinea = 56500.00m
            }
        }
    },

    ResumenFactura = new ResumenFacturaType
    {
        CodigoTipoMoneda = new CodigoMonedaType
        {
            CodigoMoneda = "CRC",
            TipoCambio = 1.00000m
        },
        TotalMercanciasGravadas13 = 50000.00m,
        TotalVenta = 50000.00m,
        TotalVentaNeta = 50000.00m,
        TotalDesgloseImpuesto = new List<TotalDesgloseImpuestoType>
        {
            new TotalDesgloseImpuestoType
            {
                Codigo = "01",
                CodigoTarifaIVA = "08",
                TotalMontoImpuesto = 6500.00m
            }
        },
        TotalImpuesto = 6500.00m,
        TotalComprobante = 56500.00m
    }
};

// Serializar a XML
string xml = SerializarDocumento(factura);
```

## Ejemplo 2: Crear un Recibo Electrónico de Pago (REP) - NUEVO v4.4

```csharp
// El REP es OBLIGATORIO para ventas a crédito con IVA (hasta 90 días)
var reciboPago = new ReciboElectronicoPago
{
    Clave = GenerarClaveNumerica(...), // Clave de 50 dígitos
    ProveedorSistemas = "3101234567", // Cédula del proveedor de software
    NumeroConsecutivo = "00100001100000000001", // Tipo 10 (REP)
    FechaEmision = DateTime.Now,

    Emisor = new EmisorREPType
    {
        Nombre = "EMPRESA DEMO SA",
        Identificacion = new IdentificacionType
        {
            Tipo = "02",
            Numero = "3101234567"
        },
        CorreoElectronico = new List<string> { "facturacion@empresademo.cr" }
    },

    Receptor = new ReceptorREPType
    {
        Nombre = "CLIENTE QUE PAGA SA",
        Identificacion = new IdentificacionType
        {
            Tipo = "02",
            Numero = "3102345678"
        },
        CorreoElectronico = "pagos@cliente.cr"
    },

    // Solo códigos 09 u 11 permitidos
    CondicionVenta = "11", // Pago de venta a crédito en IVA hasta 90 días

    DetalleServicio = new DetalleServicioREPType
    {
        LineaDetalle = new List<LineaDetalleREPType>
        {
            new LineaDetalleREPType
            {
                NumeroLinea = 1,
                Detalle = "Pago parcial factura 001-00001-01-0000000123",
                MontoTotal = 100000.00m,
                SubTotal = 100000.00m, // Monto del pago para cálculo de IVA
                Impuesto = new List<ImpuestoType>
                {
                    new ImpuestoType
                    {
                        Codigo = "01",
                        CodigoTarifaIVA = "08",
                        Tarifa = 13.00m,
                        Monto = 13000.00m
                    }
                },
                ImpuestoNeto = 13000.00m,
                MontoTotalLinea = 113000.00m
            }
        }
    },

    ResumenFactura = new ResumenFacturaREPType
    {
        CodigoTipoMoneda = new CodigoMonedaType
        {
            CodigoMoneda = "CRC",
            TipoCambio = 1.00000m
        },
        TotalVenta = 100000.00m,
        TotalVentaNeta = 100000.00m,
        TotalDesgloseImpuesto = new List<TotalDesgloseImpuestoType>
        {
            new TotalDesgloseImpuestoType
            {
                Codigo = "01",
                CodigoTarifaIVA = "08",
                TotalMontoImpuesto = 13000.00m
            }
        },
        TotalImpuesto = 13000.00m,
        MedioPago = new List<MedioPagoType>
        {
            new MedioPagoType
            {
                TipoMedioPago = "04", // Transferencia
                TotalMedioPago = 113000.00m
            }
        },
        TotalComprobante = 113000.00m
    },

    // Referencia a la factura original (OBLIGATORIO)
    InformacionReferencia = new List<InformacionReferenciaType>
    {
        new InformacionReferenciaType
        {
            TipoDocIR = "01", // Factura electrónica
            Numero = "50612012500031012345670010000101000000012300000001",
            FechaEmisionIR = DateTime.Parse("2025-12-01"),
            Codigo = "04", // Referencia a otro documento
            Razon = "Pago parcial de factura a crédito"
        }
    }
};
```

## Ejemplo 3: Crear una Nota de Crédito

```csharp
var notaCredito = new NotaCreditoElectronica
{
    Clave = GenerarClaveNumerica(...),
    CodigoActividad = "522100",
    NumeroConsecutivo = "00100001030000000001", // Tipo 03 (NC)
    FechaEmision = DateTime.Now,

    Emisor = emisorCompleto, // Reutilizar del ejemplo anterior
    Receptor = receptorCompleto,

    CondicionVenta = "01",
    MedioPago = new List<MedioPagoType>
    {
        new MedioPagoType { TipoMedioPago = "01" } // Efectivo
    },

    DetalleServicio = new DetalleServicioType
    {
        LineaDetalle = new List<LineaDetalleType>
        {
            new LineaDetalleType
            {
                NumeroLinea = 1,
                CodigoCaByS = "8517120100000",
                Cantidad = 1,
                UnidadMedida = "Unid",
                Detalle = "Devolución - Teléfono defectuoso",
                PrecioUnitario = 25000.00m,
                MontoTotal = 25000.00m,
                SubTotal = 25000.00m,
                Impuesto = new List<ImpuestoType>
                {
                    new ImpuestoType
                    {
                        Codigo = "01",
                        CodigoTarifaIVA = "08",
                        Tarifa = 13.00m,
                        Monto = 3250.00m
                    }
                },
                ImpuestoNeto = 3250.00m,
                MontoTotalLinea = 28250.00m
            }
        }
    },

    ResumenFactura = new ResumenFacturaType
    {
        CodigoTipoMoneda = new CodigoMonedaType { CodigoMoneda = "CRC", TipoCambio = 1.00000m },
        TotalMercanciasGravadas13 = 25000.00m,
        TotalVenta = 25000.00m,
        TotalVentaNeta = 25000.00m,
        TotalImpuesto = 3250.00m,
        TotalComprobante = 28250.00m
    },

    // Referencia OBLIGATORIA a la factura que se está afectando
    InformacionReferencia = new List<InformacionReferenciaType>
    {
        new InformacionReferenciaType
        {
            TipoDocIR = "01", // Factura electrónica
            Numero = "50612012500031012345670010000101000000012300000001",
            FechaEmisionIR = DateTime.Parse("2025-12-01"),
            Codigo = "06", // Devolución de mercancía
            Razon = "Producto defectuoso, se procede con devolución completa"
        }
    }
};
```

## Ejemplo 4: Producto Farmacéutico (Campos obligatorios desde 01/12/2024)

```csharp
var lineaFarmaceutica = new LineaDetalleType
{
    NumeroLinea = 1,
    CodigoCaByS = "9999999999999", // CAByS para farmacéuticos
    Cantidad = 30,
    UnidadMedida = "Unid",
    Detalle = "Paracetamol 500mg",
    PrecioUnitario = 150.00m,
    MontoTotal = 4500.00m,
    SubTotal = 4500.00m,

    // Campos OBLIGATORIOS para productos farmacéuticos
    NumeroRegistro = "REG-2024-12345", // Número de registro sanitario
    FormaFarmaceutica = "Tabletas", // Forma farmacéutica del producto

    Impuesto = new List<ImpuestoType>
    {
        new ImpuestoType
        {
            Codigo = "01",
            CodigoTarifaIVA = "08",
            Tarifa = 13.00m,
            Monto = 585.00m
        }
    },
    ImpuestoNeto = 585.00m,
    MontoTotalLinea = 5085.00m
};
```

## Ejemplo 5: Vehículo con VIN

```csharp
var lineaVehiculo = new LineaDetalleType
{
    NumeroLinea = 1,
    CodigoCaByS = "8703230100000", // CAByS para vehículos
    Cantidad = 1,
    UnidadMedida = "Unid",
    Detalle = "Automóvil sedan 4 puertas",
    PrecioUnitario = 15000000.00m,
    MontoTotal = 15000000.00m,
    SubTotal = 15000000.00m,

    // VIN obligatorio para vehículos (17 caracteres exactos)
    NumeroVin = "1HGBH41JXMN109186",

    Impuesto = new List<ImpuestoType>
    {
        new ImpuestoType
        {
            Codigo = "01",
            CodigoTarifaIVA = "08",
            Tarifa = 13.00m,
            Monto = 1950000.00m
        }
    },
    ImpuestoNeto = 1950000.00m,
    MontoTotalLinea = 16950000.00m
};
```

## Ejemplo 6: Múltiples Medios de Pago

```csharp
// Cuando se usan múltiples medios, TotalMedioPago es OBLIGATORIO
MedioPago = new List<MedioPagoType>
{
    new MedioPagoType
    {
        TipoMedioPago = "01", // Efectivo
        TotalMedioPago = 50000.00m
    },
    new MedioPagoType
    {
        TipoMedioPago = "06", // SINPE Móvil (NUEVO v4.4)
        TotalMedioPago = 30000.00m
    },
    new MedioPagoType
    {
        TipoMedioPago = "02", // Tarjeta
        TotalMedioPago = 20000.00m
    }
}
// Total: 100,000.00 (debe coincidir con TotalComprobante)
```

## Ejemplo 7: Exoneración de Impuestos

```csharp
var lineaExonerada = new LineaDetalleType
{
    NumeroLinea = 1,
    Cantidad = 1,
    UnidadMedida = "Unid",
    Detalle = "Equipo médico para hospital",
    PrecioUnitario = 500000.00m,
    MontoTotal = 500000.00m,
    SubTotal = 500000.00m,

    Impuesto = new List<ImpuestoType>
    {
        new ImpuestoType
        {
            Codigo = "01",
            CodigoTarifaIVA = "08",
            Tarifa = 13.00m,
            Monto = 65000.00m, // Monto calculado antes de exoneración

            // Información de exoneración
            Exoneracion = new ExoneracionType
            {
                TipoDocumento = "04", // Exención DGH
                NumeroDocumento = "EXO-2025-001234",
                NombreInstitucion = "DIRECCION GENERAL DE HACIENDA",
                FechaEmision = DateTime.Parse("2025-01-15"),
                PorcentajeExoneracion = 100, // 100% exonerado
                MontoExoneracion = 65000.00m
            }
        }
    },
    ImpuestoNeto = 0.00m, // 65000 - 65000 = 0
    MontoTotalLinea = 500000.00m
};
```

## Ejemplo 8: Factura de Exportación

```csharp
var facturaExportacion = new FacturaElectronicaExportacion
{
    Clave = GenerarClaveNumerica(...),
    CodigoActividad = "522100",
    NumeroConsecutivo = "00100001090000000001", // Tipo 09 (FEE)
    FechaEmision = DateTime.Now,

    Emisor = emisorCompleto,

    // Receptor extranjero (estructura simplificada)
    Receptor = new ReceptorExportacionType
    {
        Nombre = "INTERNATIONAL CUSTOMER INC",
        TipoIdentificacion = "05", // Extranjero no domiciliado
        NumeroIdentificacion = "EXT123456",
        Telefono = new TelefonoType
        {
            CodigoPais = "001", // USA
            NumTelefono = "5551234567"
        },
        CorreoElectronico = new List<string> { "orders@customer.com" }
    },

    CondicionVenta = "02", // Crédito
    PlazoCredito = "30",

    MedioPago = new List<MedioPagoType>
    {
        new MedioPagoType { TipoMedioPago = "04" } // Transferencia
    },

    DetalleServicio = detalleCompleto,

    // Resumen especial para exportación (generalmente sin IVA)
    ResumenFactura = new ResumenFacturaExportacionType
    {
        CodigoTipoMoneda = new CodigoMonedaType
        {
            CodigoMoneda = "USD", // Dólares americanos
            TipoCambio = 515.00m // Tipo de cambio del día
        },
        TotalMercanciasExentas = 10000.00m,
        TotalExento = 10000.00m,
        TotalVenta = 10000.00m,
        TotalVentaNeta = 10000.00m,
        TotalImpuesto = 0.00m, // Exportaciones exentas de IVA
        TotalComprobante = 10000.00m
    }
};
```

## Utilidades de Serialización

```csharp
using System.Xml;
using System.Xml.Serialization;
using System.Text;

public static class DocumentoElectronicoHelper
{
    public static string SerializarDocumento<T>(T documento)
    {
        var serializer = new XmlSerializer(typeof(T));
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("", ""); // Remover prefijos por defecto si es necesario

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = new UTF8Encoding(false), // UTF-8 sin BOM
            OmitXmlDeclaration = false
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        serializer.Serialize(xmlWriter, documento, namespaces);
        return stringWriter.ToString();
    }

    public static T DeserializarDocumento<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stringReader = new StringReader(xml);
        var documento = serializer.Deserialize(stringReader);
        return documento != null ? (T)documento : throw new InvalidOperationException("Error al deserializar");
    }

    public static bool ValidarDocumento<T>(T documento, out List<ValidationResult> errores)
    {
        errores = new List<ValidationResult>();
        var contexto = new ValidationContext(documento, serviceProvider: null, items: null);
        return Validator.TryValidateObject(
            documento,
            contexto,
            errores,
            validateAllProperties: true
        );
    }
}
```

## Generación de Clave Numérica (50 dígitos)

```csharp
public static string GenerarClaveNumerica(
    string pais,
    string dia,
    string mes,
    string year,
    string tipoIdentificacion,
    string numeroIdentificacion,
    string consecutivo,
    string situacion,
    string codigoSeguridad)
{
    // Formato: PPDDMMYYYYTIIIIIIIIIIIICCCCCCCCCCCCCCCCCCCSSSSSSSSSS
    // PP: País (506)
    // DDMMYYYY: Fecha
    // T: Tipo identificación (1 dígito)
    // IIIIIIIIIIII: Número identificación (12 dígitos, rellenar con ceros)
    // CCCCCCCCCCCCCCCCCCCC: Consecutivo (20 dígitos)
    // S: Situación (1 dígito: 1=normal, 2=contingencia, 3=sin internet)
    // SSSSSSSS: Código de seguridad (8 dígitos aleatorios)

    numeroIdentificacion = numeroIdentificacion.PadLeft(12, '0');

    return $"{pais}{dia}{mes}{year}{tipoIdentificacion}{numeroIdentificacion}{consecutivo}{situacion}{codigoSeguridad}";
}
```

## Notas Importantes

1. **Validaciones**: Siempre validar con `ValidarDocumento()` antes de enviar a Hacienda
2. **Firma Digital**: Implementar XAdES-EPES después de serializar
3. **Encoding**: Usar UTF-8 sin BOM
4. **Precisión**: Usar `decimal` para todos los montos, no `float` ni `double`
5. **Fechas**: Formato ISO 8601 con timezone: `yyyy-MM-ddTHH:mm:sszzz`
6. **CIIU4**: Obligatorio desde 01/09/2025 (6 dígitos)
7. **CAByS 2025**: Obligatorio desde 01/06/2025 (13 dígitos)
8. **REP**: Obligatorio para ventas a crédito con IVA hasta 90 días
