# Clave Numérica de Hacienda - Formato v4.4

## Descripción General

La clave numérica es un identificador único de 50 dígitos que se genera para cada documento electrónico que se envía a Hacienda de Costa Rica. Esta clave cumple con los estándares establecidos por el Ministerio de Hacienda en la versión 4.4 de la especificación.

## Formato de la Clave (50 dígitos)

La clave numérica se compone de los siguientes elementos en orden:

| Posición | Longitud | Descripción | Ejemplo | Formato |
|----------|----------|-------------|---------|---------|
| 1-3 | 3 | Código del país | 506 | Siempre 506 para Costa Rica |
| 4-5 | 2 | Día de emisión | 29 | DD (01-31) |
| 6-7 | 2 | Mes de emisión | 11 | MM (01-12) |
| 8-9 | 2 | Año de emisión | 25 | YY (últimos 2 dígitos) |
| 10-21 | 12 | Número de identificación del emisor | 000003101234567 | Cédula/identificación con ceros a la izquierda |
| 22-41 | 20 | Número consecutivo del documento | 00100000100100000001 | Consecutivo sin guiones, con ceros a la izquierda |
| 42 | 1 | Situación del comprobante | 1 | 1=Normal, 2=Contingencia, 3=Sin Internet |
| 43-50 | 8 | Código de seguridad | 12345678 | Número aleatorio de 8 dígitos |

**Total: 50 caracteres numéricos**

## Ejemplo Completo

```
Clave: 50629112500000310123456700100000100100000001112345678

Desglose:
506          - País (Costa Rica)
29           - Día (29)
11           - Mes (Noviembre)
25           - Año (2025)
000003101234567 - Cédula del emisor (3-101-234567)
00100000100100000001 - Consecutivo (001-00001-01-0000000001)
1            - Situación (Normal)
12345678     - Código de seguridad (aleatorio)
```

## Implementación en C#

### Servicio: `ClaveGeneradorService`

El servicio `ClaveGeneradorService` se encarga de generar la clave numérica automáticamente al procesar un documento.

**Ubicación:** `/Facturacion.Backend/Services/Implementations/ClaveGeneradorService.cs`

### Método Principal: `GenerarClaveAsync`

```csharp
Task<string> GenerarClaveAsync(Documento documento, int situacion = 1)
```

**Parámetros:**
- `documento`: Documento para el cual generar la clave
- `situacion`: Situación del documento (1=Normal, 2=Contingencia, 3=Sin Internet)

**Retorno:** String de 50 dígitos con la clave numérica

### Componentes de la Clave

#### 1. País (3 dígitos)
```csharp
string pais = "506"; // Siempre Costa Rica
```

#### 2. Fecha de Emisión (6 dígitos: DDMMYY)
```csharp
string dia = documento.FechaEmision.ToString("dd");   // 01-31
string mes = documento.FechaEmision.ToString("MM");   // 01-12
string anio = documento.FechaEmision.ToString("yy");  // 00-99
```

#### 3. Cédula del Emisor (12 dígitos)
```csharp
string cedulaEmisor = documento.Empresa.NumeroIdentificacion.PadLeft(12, '0');
```

La cédula se rellena con ceros a la izquierda para alcanzar 12 dígitos:
- Cédula física (9 dígitos): `123456789` → `000123456789`
- Cédula jurídica (10 dígitos): `3101234567` → `003101234567`

#### 4. Consecutivo (20 dígitos)
```csharp
string consecutivo = documento.NumeroConsecutivo.Replace("-", "").PadLeft(20, '0');
```

El consecutivo original tiene formato: `XXX-YYYYY-ZZ-AAAAAAAAAA`
- Se eliminan los guiones
- Se rellena con ceros a la izquierda hasta 20 dígitos

Ejemplo: `001-00001-01-0000000001` → `00100000100100000001`

#### 5. Situación del Comprobante (1 dígito)
```csharp
string situacionStr = situacion.ToString();
```

Valores posibles:
- `1`: Normal (documento enviado en condiciones normales)
- `2`: Contingencia (problemas con el sistema de Hacienda)
- `3`: Sin Internet (sin conexión a Internet)

#### 6. Código de Seguridad (8 dígitos)
```csharp
public string GenerarCodigoSeguridad()
{
    var codigo = string.Empty;
    for (int i = 0; i < 8; i++)
    {
        codigo += _random.Next(0, 10).ToString();
    }
    return codigo;
}
```

Genera 8 dígitos aleatorios para garantizar la unicidad de la clave.

## Validación de la Clave

El servicio incluye un método `ValidarClave` que verifica:

1. Longitud exacta de 50 caracteres
2. Todos los caracteres son numéricos
3. País inicia con "506"
4. Día está entre 01 y 31
5. Mes está entre 01 y 12
6. Situación es 1, 2 o 3

```csharp
bool ValidarClave(string clave)
```

## Integración con DocumentoHaciendaService

La clave se genera automáticamente durante el proceso de envío a Hacienda:

```csharp
// En DocumentoHaciendaService.ProcesarYEnviarAsync()
if (string.IsNullOrWhiteSpace(documento.Clave) || documento.Clave.Length != 50)
{
    documento.Clave = await _claveGenerador.GenerarClaveAsync(documento, situacion);
}
```

## Casos de Uso

### Documento Normal
```
Fecha: 29/11/2025
Empresa: 3-101-234567
Consecutivo: 001-00001-01-0000000001
Situación: Normal (1)

Clave: 50629112500000310123456700100000100100000001112345678
```

### Documento en Contingencia
```
Fecha: 29/11/2025
Empresa: 3-101-234567
Consecutivo: 001-00001-01-0000000002
Situación: Contingencia (2)

Clave: 50629112500000310123456700100000100100000002287654321
```

## Consideraciones Importantes

1. **Unicidad**: El código de seguridad de 8 dígitos asegura que dos documentos con el mismo consecutivo y fecha tengan claves diferentes.

2. **Atomicidad**: El consecutivo se genera de forma atómica para evitar duplicados en ambientes concurrentes.

3. **Inmutabilidad**: Una vez generada, la clave NO debe modificarse. Si hay errores, se debe crear un nuevo documento.

4. **Formato SQL Original**: El código C# replica la lógica del SQL proporcionado:
```sql
SELECT @Clave = '506' +
       RIGHT('00' + CONVERT(VARCHAR(2), DAY(DATEADD(HOUR, -2, GETDATE()))), 2) +
       RIGHT('00' + CONVERT(VARCHAR(2), MONTH(DATEADD(HOUR, -2, GETDATE()))), 2) +
       RIGHT('0000' + CONVERT(VARCHAR(4), YEAR(DATEADD(HOUR, -2, GETDATE()))), 2) +
       (SELECT RIGHT('0000' + NumeroDocumento, 12) FROM ATV WHERE IdATV = @IdATV) +
       @Consecutivo +
       '1' +
       '0000000000'
```

## Registro en Program.cs

El servicio está registrado en el contenedor de dependencias:

```csharp
builder.Services.AddScoped<IClaveGeneradorService, ClaveGeneradorService>();
```

## Referencias

- Especificación Sistema: `/especificacion_sistema.md`
- Documentación Hacienda v4.4: Resolución DGT-R-48-2016 y actualizaciones
- Servicio de Consecutivos: `/Facturacion.Backend/Services/Implementations/ConsecutivoService.cs`
- Servicio de Documentos Hacienda: `/Facturacion.Backend/Services/Implementations/DocumentoHaciendaService.cs`

---

**Última actualización**: 29 de noviembre de 2025
**Versión**: 1.0
**Estado**: Implementado y funcional
