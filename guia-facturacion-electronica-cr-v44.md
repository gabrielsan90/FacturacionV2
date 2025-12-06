# Guía Técnica de Facturación Electrónica Costa Rica v4.4
## Manual de Implementación para Desarrolladores

> **Versión:** 4.4 (Obligatoria desde 1 de septiembre 2025)  
> **Última actualización:** Noviembre 2025  
> **Objetivo:** Guía de referencia para implementar o auditar un sistema de facturación electrónica como emisor directo con Hacienda CR

---

## 📋 Tabla de Contenidos

1. [Checklist de Implementación](#1-checklist-de-implementación)
2. [Arquitectura del Sistema](#2-arquitectura-del-sistema)
3. [Configuración de Ambientes](#3-configuración-de-ambientes)
4. [Autenticación OAuth 2.0](#4-autenticación-oauth-20)
5. [Tipos de Documentos Electrónicos](#5-tipos-de-documentos-electrónicos)
6. [Estructura de la Clave Numérica](#6-estructura-de-la-clave-numérica)
7. [Estructura XML por Tipo de Documento](#7-estructura-xml-por-tipo-de-documento)
8. [Firma Digital XAdES-EPES](#8-firma-digital-xades-epes)
9. [API de Recepción de Comprobantes](#9-api-de-recepción-de-comprobantes)
10. [Catálogos y Códigos Oficiales](#10-catálogos-y-códigos-oficiales)
11. [Estados y Respuestas de Hacienda](#11-estados-y-respuestas-de-hacienda)
12. [Manejo de Errores Comunes](#12-manejo-de-errores-comunes)
13. [Contingencias](#13-contingencias)
14. [Testing y Validación](#14-testing-y-validación)
15. [Recursos y Enlaces Oficiales](#15-recursos-y-enlaces-oficiales)

---

## 1. Checklist de Implementación

### ✅ Funcionalidades Core (Obligatorias)

| # | Funcionalidad | Estado | Notas |
|---|---------------|--------|-------|
| 1.1 | Generación de Clave Numérica (50 dígitos) | ⬜ | Ver sección 6 |
| 1.2 | Generación de Consecutivo (20 dígitos) | ⬜ | Formato: SSSPPPPTTNNNNNNNNNN |
| 1.3 | Emisión de Factura Electrónica (01) | ⬜ | |
| 1.4 | Emisión de Nota de Débito (02) | ⬜ | Requiere referencia a doc original |
| 1.5 | Emisión de Nota de Crédito (03) | ⬜ | Requiere referencia a doc original |
| 1.6 | Emisión de Tiquete Electrónico (04) | ⬜ | Sin crédito fiscal |
| 1.7 | Emisión de Factura de Exportación (05) | ⬜ | Partida arancelaria obligatoria |
| 1.8 | Emisión de Factura de Compra (09) | ⬜ | Autofacturación |
| 1.9 | Emisión de Recibo Electrónico de Pago (10) | ⬜ | **NUEVO v4.4** |
| 1.10 | Firma Digital XAdES-EPES | ⬜ | RSA-2048 + SHA-256 |
| 1.11 | Autenticación OAuth 2.0 | ⬜ | Token refresh automático |
| 1.12 | Envío a API de Hacienda | ⬜ | POST /recepcion |
| 1.13 | Consulta de Estado | ⬜ | GET /recepcion/{clave} |
| 1.14 | Procesamiento de Respuestas XML | ⬜ | Aceptación/Rechazo |
| 1.15 | Almacenamiento de Comprobantes (5 años) | ⬜ | XML firmado + respuesta |

### ✅ Campos Nuevos Versión 4.4 (Obligatorios)

| # | Campo | Ubicación XML | Estado | Notas |
|---|-------|---------------|--------|-------|
| 2.1 | ProveedorSistemas | Raíz documento | ⬜ | Cédula del desarrollador |
| 2.2 | CodigoActividadEmisor | Emisor | ⬜ | Código CIIU 6 dígitos |
| 2.3 | CodigoActividadReceptor | Receptor | ⬜ | Condicional según tipo |
| 2.4 | TipoTransaccion | LineaDetalle | ⬜ | Códigos 01-13 |
| 2.5 | DetalleSurtido | LineaDetalle | ⬜ | Para combos/paquetes |
| 2.6 | RegistroMedicamento | LineaDetalle | ⬜ | Sector farmacéutico |
| 2.7 | FormaFarmaceutica | LineaDetalle | ⬜ | Sector farmacéutico |
| 2.8 | NumeroVINoSerie | LineaDetalle | ⬜ | Vehículos (hasta 1000) |
| 2.9 | PlazoCredito | ResumenFactura | ⬜ | Días de crédito |
| 2.10 | FechaPago | Medios de Pago | ⬜ | Para REP |

### ✅ Integraciones con APIs de Hacienda

| # | API | Endpoint | Estado | Uso |
|---|-----|----------|--------|-----|
| 3.1 | Consulta CABYS | api.hacienda.go.cr/fe/cabys | ⬜ | Validar códigos productos |
| 3.2 | Actividades Económicas | api.hacienda.go.cr/fe/ae | ⬜ | Obtener códigos CIIU |
| 3.3 | Exoneraciones | api.hacienda.go.cr/fe/ex | ⬜ | Validar autorizaciones |
| 3.4 | Tipo de Cambio | api.hacienda.go.cr/indicadores/tc | ⬜ | Conversión monedas |

### ✅ Validaciones Previas al Envío

| # | Validación | Estado | Notas |
|---|------------|--------|-------|
| 4.1 | Esquema XSD | ⬜ | Validar contra XSD oficial |
| 4.2 | Cálculos de impuestos | ⬜ | IVA, ISC por línea y totales |
| 4.3 | Consistencia de totales | ⬜ | SubTotal + Impuestos = Total |
| 4.4 | Código CABYS válido | ⬜ | 13 dígitos, existente |
| 4.5 | Cédula receptor válida | ⬜ | Formato según tipo |
| 4.6 | Consecutivo único | ⬜ | No duplicados |
| 4.7 | Fecha dentro de rango | ⬜ | ±24 horas de fecha actual |

---

## 2. Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        SISTEMA DE FACTURACIÓN                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐              │
│  │   UI/API     │───▶│  Generador   │───▶│   Firmador   │              │
│  │   Cliente    │    │     XML      │    │   XAdES      │              │
│  └──────────────┘    └──────────────┘    └──────────────┘              │
│         │                   │                   │                       │
│         ▼                   ▼                   ▼                       │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐              │
│  │  Validador   │    │  Catálogos   │    │ Certificados │              │
│  │    XSD       │    │   (CABYS)    │    │    (.p12)    │              │
│  └──────────────┘    └──────────────┘    └──────────────┘              │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                    CAPA DE COMUNICACIÓN                          │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐  │  │
│  │  │   OAuth    │  │   Sender   │  │  Callback  │  │   Polling  │  │  │
│  │  │   Client   │  │   HTTP     │  │  Handler   │  │   Service  │  │  │
│  │  └────────────┘  └────────────┘  └────────────┘  └────────────┘  │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      MINISTERIO DE HACIENDA (ATV)                       │
├─────────────────────────────────────────────────────────────────────────┤
│  ┌────────────────┐    ┌────────────────┐    ┌────────────────┐        │
│  │  OAuth Server  │    │ API Recepción  │    │  Validadores   │        │
│  │  (IDP)         │    │  /recepcion    │    │  (3 niveles)   │        │
│  └────────────────┘    └────────────────┘    └────────────────┘        │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Configuración de Ambientes

### 3.1 Ambiente de Producción

```json
{
  "ambiente": "produccion",
  "oauth": {
    "tokenUrl": "https://idp.comprobanteselectronicos.go.cr/auth/realms/rut/protocol/openid-connect/token",
    "clientId": "api-prod"
  },
  "api": {
    "baseUrl": "https://api.comprobanteselectronicos.go.cr/recepcion/v1",
    "endpoints": {
      "recepcion": "/recepcion",
      "consulta": "/recepcion/{clave}"
    }
  },
  "tokenExpiration": 300,
  "refreshTokenExpiration": 1800
}
```

### 3.2 Ambiente de Sandbox (Pruebas)

```json
{
  "ambiente": "sandbox",
  "oauth": {
    "tokenUrl": "https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token",
    "clientId": "api-stag"
  },
  "api": {
    "baseUrl": "https://api.comprobanteselectronicos.go.cr/recepcion-sandbox/v1",
    "endpoints": {
      "recepcion": "/recepcion",
      "consulta": "/recepcion/{clave}"
    }
  },
  "tokenExpiration": 300,
  "refreshTokenExpiration": 1800
}
```

### 3.3 Credenciales Requeridas

```csharp
public class HaciendaCredentials
{
    // Usuario: identificación@comprobanteselectronicos.go.cr
    public string Username { get; set; }
    
    // Contraseña generada en portal ATV
    public string Password { get; set; }
    
    // Ruta al archivo .p12 (llave criptográfica)
    public string CertificatePath { get; set; }
    
    // PIN de 4 dígitos del certificado
    public string CertificatePin { get; set; }
}
```

---

## 4. Autenticación OAuth 2.0

### 4.1 Obtener Token de Acceso

```http
POST /auth/realms/rut/protocol/openid-connect/token HTTP/1.1
Host: idp.comprobanteselectronicos.go.cr
Content-Type: application/x-www-form-urlencoded

grant_type=password&
client_id=api-prod&
username=3101234567@comprobanteselectronicos.go.cr&
password=TU_PASSWORD_ATV
```

### 4.2 Respuesta Exitosa

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires_in": 300,
  "refresh_expires_in": 1800,
  "refresh_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "not-before-policy": 0,
  "session_state": "abc123..."
}
```

### 4.3 Implementación C# (.NET 6+)

```csharp
public class OAuthService
{
    private readonly HttpClient _httpClient;
    private readonly HaciendaConfig _config;
    private TokenResponse _currentToken;
    private DateTime _tokenExpiry;

    public async Task<string> GetAccessTokenAsync()
    {
        if (_currentToken != null && DateTime.UtcNow < _tokenExpiry.AddSeconds(-30))
        {
            return _currentToken.AccessToken;
        }

        var request = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _config.ClientId,
            ["username"] = _config.Username,
            ["password"] = _config.Password
        };

        var content = new FormUrlEncodedContent(request);
        var response = await _httpClient.PostAsync(_config.TokenUrl, content);
        
        response.EnsureSuccessStatusCode();
        
        _currentToken = await response.Content.ReadFromJsonAsync<TokenResponse>();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(_currentToken.ExpiresIn);
        
        return _currentToken.AccessToken;
    }

    public async Task<string> RefreshTokenAsync()
    {
        var request = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _config.ClientId,
            ["refresh_token"] = _currentToken.RefreshToken
        };

        var content = new FormUrlEncodedContent(request);
        var response = await _httpClient.PostAsync(_config.TokenUrl, content);
        
        response.EnsureSuccessStatusCode();
        
        _currentToken = await response.Content.ReadFromJsonAsync<TokenResponse>();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(_currentToken.ExpiresIn);
        
        return _currentToken.AccessToken;
    }
}
```

---

## 5. Tipos de Documentos Electrónicos

### 5.1 Tabla de Referencia Rápida

| Código | Tipo | Namespace | Receptor Obligatorio | Crédito Fiscal | Referencia Requerida |
|--------|------|-----------|---------------------|----------------|---------------------|
| 01 | Factura Electrónica | FacturaElectronica | ✅ Sí | ✅ Sí | ❌ No |
| 02 | Nota de Débito | NotaDebitoElectronica | ✅ Sí | ✅ Sí | ✅ Sí |
| 03 | Nota de Crédito | NotaCreditoElectronica | ✅ Sí | ✅ Sí | ✅ Sí |
| 04 | Tiquete Electrónico | TiqueteElectronico | ❌ Opcional | ❌ No | ❌ No |
| 05 | Factura Exportación | FacturaElectronicaExportacion | ✅ Sí | ✅ Sí | ❌ No |
| 09 | Factura de Compra | FacturaElectronicaCompra | ✅ Sí* | ✅ Sí | ❌ No |
| 10 | Recibo Electrónico Pago | ReciboElectronicoPago | ✅ Sí | N/A | ✅ Sí |

> *En Factura de Compra, el "Receptor" es quien emite (autofacturación)

### 5.2 Cuándo Usar Cada Tipo

```
FACTURA ELECTRÓNICA (01)
├── Ventas B2B (empresa a empresa)
├── Ventas B2C donde cliente requiere crédito fiscal
├── Servicios profesionales
└── Cualquier transacción con respaldo tributario

NOTA DE DÉBITO (02)
├── Ajuste al alza de precio
├── Intereses por mora
├── Cargos adicionales posteriores
└── Corrección de errores a favor del emisor

NOTA DE CRÉDITO (03)
├── Anulación total de factura
├── Devolución de mercadería
├── Descuentos posteriores
├── Corrección de errores a favor del receptor
└── Ajuste por pronto pago

TIQUETE ELECTRÓNICO (04)
├── Ventas a consumidor final
├── Transacciones menores
├── Cliente no requiere crédito fiscal
└── Comercio minorista general

FACTURA DE EXPORTACIÓN (05)
├── Venta de bienes al exterior
├── Servicios prestados a extranjeros no domiciliados
└── Requiere partida arancelaria

FACTURA DE COMPRA (09)
├── Compras a proveedores no emisores electrónicos
├── Importación de servicios (proveedor extranjero)
├── Compras a productores agrícolas pequeños
└── Autofacturación permitida

RECIBO ELECTRÓNICO DE PAGO (10) - NUEVO v4.4
├── Documentar pago de facturas a crédito
├── Diferir IVA hasta 90 días
├── Obligatorio para ventas al Estado a crédito
└── Referencia a factura(s) original(es)
```

### 5.3 Namespace XML por Documento

```xml
<!-- Factura Electrónica -->
<FacturaElectronica xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica">

<!-- Nota de Débito -->
<NotaDebitoElectronica xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaDebitoElectronica">

<!-- Nota de Crédito -->
<NotaCreditoElectronica xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica">

<!-- Tiquete Electrónico -->
<TiqueteElectronico xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico">

<!-- Factura de Exportación -->
<FacturaElectronicaExportacion xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaExportacion">

<!-- Factura de Compra -->
<FacturaElectronicaCompra xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaCompra">

<!-- Recibo Electrónico de Pago -->
<ReciboElectronicoPago xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/reciboElectronicoPago">
```

---

## 6. Estructura de la Clave Numérica

### 6.1 Composición (50 dígitos)

```
506 | 301124 | 3101234567XX | 00100001010000000001 | 1 | 12345678
─┬─   ──┬───   ─────┬──────   ─────────┬──────────   ┬   ────┬───
 │      │           │                  │             │       │
 │      │           │                  │             │       └─ Código Seguridad (8 dígitos aleatorios)
 │      │           │                  │             │
 │      │           │                  │             └─ Situación (1=Normal, 2=Contingencia, 3=Sin Internet)
 │      │           │                  │
 │      │           │                  └─ Consecutivo (20 dígitos)
 │      │           │
 │      │           └─ Identificación Emisor (12 dígitos con padding)
 │      │
 │      └─ Fecha DDMMAA
 │
 └─ Código País (506 = Costa Rica)
```

### 6.2 Estructura del Consecutivo (20 dígitos)

```
001 | 00001 | 01 | 0000000001
─┬─   ──┬──   ┬─   ────┬─────
 │      │     │        │
 │      │     │        └─ Número Secuencial (10 dígitos)
 │      │     │
 │      │     └─ Tipo Documento (01-10)
 │      │
 │      └─ Terminal/Caja (5 dígitos)
 │
 └─ Sucursal (001 = Casa Matriz)
```

### 6.3 Implementación C#

```csharp
public class ClaveNumerica
{
    private const string CODIGO_PAIS = "506";
    private static readonly Random _random = new Random();

    public static string Generar(
        DateTime fecha,
        string identificacionEmisor,
        string sucursal,
        string terminal,
        string tipoDocumento,
        long numeroSecuencial,
        SituacionComprobante situacion = SituacionComprobante.Normal)
    {
        // Validaciones
        if (string.IsNullOrEmpty(identificacionEmisor))
            throw new ArgumentException("Identificación del emisor requerida");

        // Formatear fecha DDMMAA
        string fechaFormato = fecha.ToString("ddMMyy");

        // Padding identificación a 12 dígitos
        string idPadded = identificacionEmisor.PadLeft(12, '0');

        // Construir consecutivo (20 dígitos)
        string consecutivo = $"{sucursal.PadLeft(3, '0')}" +
                            $"{terminal.PadLeft(5, '0')}" +
                            $"{tipoDocumento.PadLeft(2, '0')}" +
                            $"{numeroSecuencial.ToString().PadLeft(10, '0')}";

        // Situación
        string situacionStr = ((int)situacion).ToString();

        // Código de seguridad aleatorio (8 dígitos)
        string codigoSeguridad = _random.Next(10000000, 99999999).ToString();

        // Concatenar todo (50 dígitos)
        return $"{CODIGO_PAIS}{fechaFormato}{idPadded}{consecutivo}{situacionStr}{codigoSeguridad}";
    }

    public static bool Validar(string clave)
    {
        if (string.IsNullOrEmpty(clave) || clave.Length != 50)
            return false;

        if (!clave.All(char.IsDigit))
            return false;

        if (!clave.StartsWith("506"))
            return false;

        // Validar tipo de documento
        string tipoDoc = clave.Substring(29, 2);
        var tiposValidos = new[] { "01", "02", "03", "04", "05", "09", "10" };
        if (!tiposValidos.Contains(tipoDoc))
            return false;

        // Validar situación
        char situacion = clave[41];
        if (situacion != '1' && situacion != '2' && situacion != '3')
            return false;

        return true;
    }
}

public enum SituacionComprobante
{
    Normal = 1,
    Contingencia = 2,
    SinInternet = 3
}
```

---

## 7. Estructura XML por Tipo de Documento

### 7.1 Factura Electrónica (01) - Estructura Completa v4.4

```xml
<?xml version="1.0" encoding="UTF-8"?>
<FacturaElectronica xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica"
                    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                    xsi:schemaLocation="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica 
                    https://www.hacienda.go.cr/docs/FacturaElectronica_V4.4.xsd">
    
    <!-- CLAVE NUMÉRICA (50 dígitos) -->
    <Clave>50630112431012345678000100001010000000001112345678</Clave>
    
    <!-- CÓDIGO ACTIVIDAD ECONÓMICA DEL EMISOR (NUEVO v4.4) -->
    <CodigoActividad>620101</CodigoActividad>
    
    <!-- NÚMERO CONSECUTIVO (20 dígitos) -->
    <NumeroConsecutivo>00100001010000000001</NumeroConsecutivo>
    
    <!-- FECHA DE EMISIÓN (ISO 8601) -->
    <FechaEmision>2025-11-30T10:30:00-06:00</FechaEmision>
    
    <!-- PROVEEDOR DE SISTEMAS (NUEVO v4.4 - Obligatorio) -->
    <ProveedorSistemas>
        <Identificacion>
            <Tipo>02</Tipo>
            <Numero>3101234567</Numero>
        </Identificacion>
    </ProveedorSistemas>
    
    <!-- DATOS DEL EMISOR -->
    <Emisor>
        <Nombre>MI EMPRESA S.A.</Nombre>
        <Identificacion>
            <Tipo>02</Tipo>
            <Numero>3101234567</Numero>
        </Identificacion>
        <NombreComercial>Mi Empresa</NombreComercial>
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
        <CorreoElectronico>facturacion@miempresa.com</CorreoElectronico>
    </Emisor>
    
    <!-- DATOS DEL RECEPTOR -->
    <Receptor>
        <Nombre>CLIENTE EJEMPLO S.A.</Nombre>
        <Identificacion>
            <Tipo>02</Tipo>
            <Numero>3109876543</Numero>
        </Identificacion>
        <!-- CÓDIGO ACTIVIDAD DEL RECEPTOR (NUEVO v4.4 - Condicional) -->
        <CodigoActividad>461000</CodigoActividad>
        <Ubicacion>
            <Provincia>1</Provincia>
            <Canton>01</Canton>
            <Distrito>01</Distrito>
            <OtrasSenas>Edificio principal</OtrasSenas>
        </Ubicacion>
        <Telefono>
            <CodigoPais>506</CodigoPais>
            <NumTelefono>22224444</NumTelefono>
        </Telefono>
        <CorreoElectronico>compras@cliente.com</CorreoElectronico>
    </Receptor>
    
    <!-- CONDICIÓN DE VENTA -->
    <CondicionVenta>01</CondicionVenta>
    
    <!-- PLAZO DE CRÉDITO (días) -->
    <PlazoCredito>30</PlazoCredito>
    
    <!-- MEDIOS DE PAGO -->
    <MedioPago>02</MedioPago>
    
    <!-- DETALLE DE LÍNEAS -->
    <DetalleServicio>
        <LineaDetalle>
            <NumeroLinea>1</NumeroLinea>
            
            <!-- TIPO DE TRANSACCIÓN (NUEVO v4.4) -->
            <TipoTransaccion>01</TipoTransaccion>
            
            <!-- CÓDIGO CABYS (13 dígitos) -->
            <Codigo>4321100000000</Codigo>
            
            <!-- CÓDIGO COMERCIAL (interno) -->
            <CodigoComercial>
                <Tipo>04</Tipo>
                <Codigo>PROD-001</Codigo>
            </CodigoComercial>
            
            <Cantidad>10.000</Cantidad>
            <UnidadMedida>Unid</UnidadMedida>
            <Detalle>Servicio de desarrollo de software</Detalle>
            <PrecioUnitario>100000.00000</PrecioUnitario>
            <MontoTotal>1000000.00000</MontoTotal>
            
            <!-- DESCUENTO (opcional) -->
            <Descuento>
                <MontoDescuento>100000.00000</MontoDescuento>
                <NaturalezaDescuento>Descuento por volumen</NaturalezaDescuento>
            </Descuento>
            
            <SubTotal>900000.00000</SubTotal>
            <BaseImponible>900000.00000</BaseImponible>
            
            <!-- IMPUESTOS -->
            <Impuesto>
                <Codigo>01</Codigo>
                <CodigoTarifa>08</CodigoTarifa>
                <Tarifa>13.00</Tarifa>
                <FactorIVA>1.0000</FactorIVA>
                <Monto>117000.00000</Monto>
            </Impuesto>
            
            <ImpuestoNeto>117000.00000</ImpuestoNeto>
            <MontoTotalLinea>1017000.00000</MontoTotalLinea>
        </LineaDetalle>
    </DetalleServicio>
    
    <!-- OTROS CARGOS (opcional) -->
    <OtrosCargos>
        <TipoDocumento>06</TipoDocumento>
        <Detalle>Cargo por envío</Detalle>
        <MontoCargo>5000.00000</MontoCargo>
    </OtrosCargos>
    
    <!-- RESUMEN DE FACTURA -->
    <ResumenFactura>
        <CodigoTipoMoneda>
            <CodigoMoneda>CRC</CodigoMoneda>
            <TipoCambio>1.00000</TipoCambio>
        </CodigoTipoMoneda>
        <TotalServGravados>900000.00000</TotalServGravados>
        <TotalServExentos>0.00000</TotalServExentos>
        <TotalServExonerado>0.00000</TotalServExonerado>
        <TotalMercanciasGravadas>0.00000</TotalMercanciasGravadas>
        <TotalMercanciasExentas>0.00000</TotalMercanciasExentas>
        <TotalMercExonerada>0.00000</TotalMercExonerada>
        <TotalGravado>900000.00000</TotalGravado>
        <TotalExento>0.00000</TotalExento>
        <TotalExonerado>0.00000</TotalExonerado>
        <TotalVenta>900000.00000</TotalVenta>
        <TotalDescuentos>100000.00000</TotalDescuentos>
        <TotalVentaNeta>900000.00000</TotalVentaNeta>
        <TotalImpuesto>117000.00000</TotalImpuesto>
        <TotalIVADevuelto>0.00000</TotalIVADevuelto>
        <TotalOtrosCargos>5000.00000</TotalOtrosCargos>
        <TotalComprobante>1022000.00000</TotalComprobante>
    </ResumenFactura>
    
    <!-- INFORMACIÓN DE REFERENCIA (para NC/ND) -->
    <!-- 
    <InformacionReferencia>
        <TipoDoc>01</TipoDoc>
        <Numero>50630112431012345678000100001010000000000112345678</Numero>
        <FechaEmision>2025-11-25T10:00:00-06:00</FechaEmision>
        <Codigo>01</Codigo>
        <Razon>Anulación de factura por error</Razon>
    </InformacionReferencia>
    -->
    
    <!-- OTROS (opcional) -->
    <Otros>
        <OtroTexto codigo="observacion">Factura generada automáticamente</OtroTexto>
    </Otros>

    <!-- FIRMA DIGITAL (se agrega después) -->
    <!-- <ds:Signature>...</ds:Signature> -->
    
</FacturaElectronica>
```

### 7.2 Nota de Crédito (03) - Diferencias Clave

```xml
<NotaCreditoElectronica xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica">
    <!-- ... misma estructura base ... -->
    
    <!-- OBLIGATORIO: Referencia al documento original -->
    <InformacionReferencia>
        <TipoDoc>01</TipoDoc>
        <Numero>50630112431012345678000100001010000000001112345678</Numero>
        <FechaEmision>2025-11-25T10:00:00-06:00</FechaEmision>
        <Codigo>01</Codigo> <!-- 01=Anula, 02=Corrige, 04=Referencia, 05=Sustituye contingencia -->
        <Razon>Anulación total por error en facturación</Razon>
    </InformacionReferencia>
    
    <!-- ... resto igual ... -->
</NotaCreditoElectronica>
```

### 7.3 Tiquete Electrónico (04) - Sin Receptor Obligatorio

```xml
<TiqueteElectronico xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico">
    <Clave>50630112431012345678000100001040000000001112345678</Clave>
    <CodigoActividad>471110</CodigoActividad>
    <NumeroConsecutivo>00100001040000000001</NumeroConsecutivo>
    <FechaEmision>2025-11-30T10:30:00-06:00</FechaEmision>
    
    <ProveedorSistemas>
        <Identificacion>
            <Tipo>02</Tipo>
            <Numero>3101234567</Numero>
        </Identificacion>
    </ProveedorSistemas>
    
    <Emisor>
        <!-- ... datos del emisor ... -->
    </Emisor>
    
    <!-- RECEPTOR OPCIONAL -->
    <!-- Si se incluye, solo nombre e identificación -->
    
    <CondicionVenta>01</CondicionVenta>
    <MedioPago>01</MedioPago>
    
    <DetalleServicio>
        <LineaDetalle>
            <!-- NO incluye TipoTransaccion en Tiquete -->
            <NumeroLinea>1</NumeroLinea>
            <Codigo>2399100000000</Codigo>
            <Cantidad>1.000</Cantidad>
            <UnidadMedida>Unid</UnidadMedida>
            <Detalle>Producto de consumo</Detalle>
            <PrecioUnitario>5000.00000</PrecioUnitario>
            <MontoTotal>5000.00000</MontoTotal>
            <SubTotal>5000.00000</SubTotal>
            <Impuesto>
                <Codigo>01</Codigo>
                <CodigoTarifa>08</CodigoTarifa>
                <Tarifa>13.00</Tarifa>
                <Monto>650.00000</Monto>
            </Impuesto>
            <ImpuestoNeto>650.00000</ImpuestoNeto>
            <MontoTotalLinea>5650.00000</MontoTotalLinea>
        </LineaDetalle>
    </DetalleServicio>
    
    <ResumenFactura>
        <!-- ... totales ... -->
    </ResumenFactura>
</TiqueteElectronico>
```

### 7.4 Factura de Exportación (05) - Con Partida Arancelaria

```xml
<FacturaElectronicaExportacion xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaExportacion">
    <!-- ... estructura base ... -->
    
    <DetalleServicio>
        <LineaDetalle>
            <NumeroLinea>1</NumeroLinea>
            <TipoTransaccion>01</TipoTransaccion>
            
            <!-- PARTIDA ARANCELARIA (Obligatoria para exportación) -->
            <PartidaArancelaria>8471300000</PartidaArancelaria>
            
            <Codigo>4321100000000</Codigo>
            <Cantidad>100.000</Cantidad>
            <UnidadMedida>Unid</UnidadMedida>
            <Detalle>Equipos de cómputo para exportación</Detalle>
            <PrecioUnitario>500.00000</PrecioUnitario>
            <MontoTotal>50000.00000</MontoTotal>
            <SubTotal>50000.00000</SubTotal>
            <!-- Exportaciones generalmente exentas de IVA -->
            <MontoTotalLinea>50000.00000</MontoTotalLinea>
        </LineaDetalle>
    </DetalleServicio>
    
    <!-- ... resumen ... -->
</FacturaElectronicaExportacion>
```

### 7.5 Recibo Electrónico de Pago (10) - NUEVO v4.4

```xml
<ReciboElectronicoPago xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/reciboElectronicoPago">
    <Clave>50630112431012345678000100001100000000001112345678</Clave>
    <CodigoActividad>620101</CodigoActividad>
    <NumeroConsecutivo>00100001100000000001</NumeroConsecutivo>
    <FechaEmision>2025-11-30T10:30:00-06:00</FechaEmision>
    
    <ProveedorSistemas>
        <Identificacion>
            <Tipo>02</Tipo>
            <Numero>3101234567</Numero>
        </Identificacion>
    </ProveedorSistemas>
    
    <Emisor>
        <!-- ... datos del emisor ... -->
    </Emisor>
    
    <Receptor>
        <!-- ... datos del receptor/pagador ... -->
    </Receptor>
    
    <!-- MEDIOS DE PAGO CON FECHA (específico de REP) -->
    <MedioPago>
        <Codigo>02</Codigo>
        <FechaPago>2025-11-30</FechaPago>
    </MedioPago>
    
    <!-- REFERENCIA A FACTURA(S) PAGADA(S) -->
    <InformacionReferencia>
        <TipoDoc>01</TipoDoc>
        <Numero>50630112431012345678000100001010000000001112345678</Numero>
        <FechaEmision>2025-11-01T10:00:00-06:00</FechaEmision>
        <Codigo>06</Codigo> <!-- 06 = Pago de factura -->
        <Razon>Pago de factura a crédito</Razon>
        <MontoPago>1022000.00000</MontoPago>
    </InformacionReferencia>
    
    <ResumenFactura>
        <CodigoTipoMoneda>
            <CodigoMoneda>CRC</CodigoMoneda>
            <TipoCambio>1.00000</TipoCambio>
        </CodigoTipoMoneda>
        <TotalComprobante>1022000.00000</TotalComprobante>
    </ResumenFactura>
</ReciboElectronicoPago>
```

---

## 8. Firma Digital XAdES-EPES

### 8.1 Especificaciones Técnicas

| Parámetro | Valor |
|-----------|-------|
| Estándar | XAdES-EPES (XML Advanced Electronic Signatures) |
| Tipo | Enveloped (dentro del XML) |
| Algoritmo Hash | SHA-256 |
| Algoritmo Firma | RSA-2048 |
| Canonicalización | C14N (http://www.w3.org/TR/2001/REC-xml-c14n-20010315) |
| Política de Firma | https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/Resolucion_DGT-R-019-2022.pdf |

### 8.2 Estructura de la Firma

```xml
<ds:Signature xmlns:ds="http://www.w3.org/2000/09/xmldsig#" Id="xmldsig-signature">
    <ds:SignedInfo>
        <ds:CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315"/>
        <ds:SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"/>
        
        <!-- Referencia al documento completo -->
        <ds:Reference Id="xmldsig-ref0" URI="">
            <ds:Transforms>
                <ds:Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/>
            </ds:Transforms>
            <ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
            <ds:DigestValue>BASE64_HASH_DEL_DOCUMENTO</ds:DigestValue>
        </ds:Reference>
        
        <!-- Referencia a KeyInfo -->
        <ds:Reference URI="#xmldsig-keyinfo" Type="http://www.w3.org/2000/09/xmldsig#Object">
            <ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
            <ds:DigestValue>BASE64_HASH_KEYINFO</ds:DigestValue>
        </ds:Reference>
        
        <!-- Referencia a SignedProperties (XAdES) -->
        <ds:Reference URI="#xmldsig-signed-props" Type="http://uri.etsi.org/01903#SignedProperties">
            <ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
            <ds:DigestValue>BASE64_HASH_SIGNED_PROPS</ds:DigestValue>
        </ds:Reference>
    </ds:SignedInfo>
    
    <ds:SignatureValue>BASE64_FIRMA_RSA</ds:SignatureValue>
    
    <ds:KeyInfo Id="xmldsig-keyinfo">
        <ds:X509Data>
            <ds:X509Certificate>BASE64_CERTIFICADO_X509</ds:X509Certificate>
        </ds:X509Data>
        <ds:KeyValue>
            <ds:RSAKeyValue>
                <ds:Modulus>BASE64_MODULO</ds:Modulus>
                <ds:Exponent>BASE64_EXPONENTE</ds:Exponent>
            </ds:RSAKeyValue>
        </ds:KeyValue>
    </ds:KeyInfo>
    
    <ds:Object>
        <xades:QualifyingProperties xmlns:xades="http://uri.etsi.org/01903/v1.3.2#" Target="#xmldsig-signature">
            <xades:SignedProperties Id="xmldsig-signed-props">
                <xades:SignedSignatureProperties>
                    <xades:SigningTime>2025-11-30T10:30:00-06:00</xades:SigningTime>
                    <xades:SigningCertificate>
                        <xades:Cert>
                            <xades:CertDigest>
                                <ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
                                <ds:DigestValue>BASE64_HASH_CERT</ds:DigestValue>
                            </xades:CertDigest>
                            <xades:IssuerSerial>
                                <ds:X509IssuerName>ISSUER_DEL_CERT</ds:X509IssuerName>
                                <ds:X509SerialNumber>SERIAL_NUMBER</ds:X509SerialNumber>
                            </xades:IssuerSerial>
                        </xades:Cert>
                    </xades:SigningCertificate>
                    <xades:SignaturePolicyIdentifier>
                        <xades:SignaturePolicyId>
                            <xades:SigPolicyId>
                                <xades:Identifier>https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/Resolucion_DGT-R-019-2022.pdf</xades:Identifier>
                            </xades:SigPolicyId>
                            <xades:SigPolicyHash>
                                <ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
                                <ds:DigestValue>HASH_POLITICA_FIRMA</ds:DigestValue>
                            </xades:SigPolicyHash>
                        </xades:SignaturePolicyId>
                    </xades:SignaturePolicyIdentifier>
                </xades:SignedSignatureProperties>
            </xades:SignedProperties>
        </xades:QualifyingProperties>
    </ds:Object>
</ds:Signature>
```

### 8.3 Implementación C# (.NET 6+)

```csharp
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

public class XadesEpesSigner
{
    private readonly X509Certificate2 _certificate;
    private const string POLICY_URL = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/Resolucion_DGT-R-019-2022.pdf";
    
    public XadesEpesSigner(string certificatePath, string password)
    {
        // Cargar certificado .p12
        _certificate = new X509Certificate2(
            certificatePath, 
            password,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet
        );
    }
    
    public string FirmarDocumento(string xmlContent)
    {
        var xmlDoc = new XmlDocument { PreserveWhitespace = true };
        xmlDoc.LoadXml(xmlContent);
        
        // Crear firma
        var signedXml = new SignedXml(xmlDoc)
        {
            SigningKey = _certificate.GetRSAPrivateKey()
        };
        
        signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
        signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
        
        // Referencia al documento completo
        var reference = new Reference("")
        {
            DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256"
        };
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        signedXml.AddReference(reference);
        
        // KeyInfo con certificado
        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(_certificate));
        signedXml.KeyInfo = keyInfo;
        
        // Calcular firma
        signedXml.ComputeSignature();
        
        // Agregar propiedades XAdES
        var signatureElement = signedXml.GetXml();
        AgregarPropiedadesXades(signatureElement, xmlDoc);
        
        // Insertar firma en documento
        var root = xmlDoc.DocumentElement;
        root?.AppendChild(xmlDoc.ImportNode(signatureElement, true));
        
        return xmlDoc.OuterXml;
    }
    
    private void AgregarPropiedadesXades(XmlElement signatureElement, XmlDocument xmlDoc)
    {
        var nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsMgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
        nsMgr.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");
        
        // Crear elemento Object para QualifyingProperties
        var objectElement = xmlDoc.CreateElement("ds", "Object", "http://www.w3.org/2000/09/xmldsig#");
        
        var qualifyingProps = xmlDoc.CreateElement("xades", "QualifyingProperties", "http://uri.etsi.org/01903/v1.3.2#");
        qualifyingProps.SetAttribute("Target", "#xmldsig-signature");
        
        var signedProps = xmlDoc.CreateElement("xades", "SignedProperties", "http://uri.etsi.org/01903/v1.3.2#");
        signedProps.SetAttribute("Id", "xmldsig-signed-props");
        
        var signedSigProps = xmlDoc.CreateElement("xades", "SignedSignatureProperties", "http://uri.etsi.org/01903/v1.3.2#");
        
        // SigningTime
        var signingTime = xmlDoc.CreateElement("xades", "SigningTime", "http://uri.etsi.org/01903/v1.3.2#");
        signingTime.InnerText = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
        signedSigProps.AppendChild(signingTime);
        
        // SigningCertificate
        var signingCert = CrearSigningCertificate(xmlDoc);
        signedSigProps.AppendChild(signingCert);
        
        // SignaturePolicyIdentifier
        var policyId = CrearSignaturePolicyIdentifier(xmlDoc);
        signedSigProps.AppendChild(policyId);
        
        signedProps.AppendChild(signedSigProps);
        qualifyingProps.AppendChild(signedProps);
        objectElement.AppendChild(qualifyingProps);
        
        signatureElement.AppendChild(xmlDoc.ImportNode(objectElement, true));
    }
    
    private XmlElement CrearSigningCertificate(XmlDocument xmlDoc)
    {
        var signingCert = xmlDoc.CreateElement("xades", "SigningCertificate", "http://uri.etsi.org/01903/v1.3.2#");
        var cert = xmlDoc.CreateElement("xades", "Cert", "http://uri.etsi.org/01903/v1.3.2#");
        
        // CertDigest
        var certDigest = xmlDoc.CreateElement("xades", "CertDigest", "http://uri.etsi.org/01903/v1.3.2#");
        var digestMethod = xmlDoc.CreateElement("ds", "DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
        digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
        var digestValue = xmlDoc.CreateElement("ds", "DigestValue", "http://www.w3.org/2000/09/xmldsig#");
        
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(_certificate.RawData);
            digestValue.InnerText = Convert.ToBase64String(hash);
        }
        
        certDigest.AppendChild(digestMethod);
        certDigest.AppendChild(digestValue);
        
        // IssuerSerial
        var issuerSerial = xmlDoc.CreateElement("xades", "IssuerSerial", "http://uri.etsi.org/01903/v1.3.2#");
        var issuerName = xmlDoc.CreateElement("ds", "X509IssuerName", "http://www.w3.org/2000/09/xmldsig#");
        issuerName.InnerText = _certificate.IssuerName.Name;
        var serialNumber = xmlDoc.CreateElement("ds", "X509SerialNumber", "http://www.w3.org/2000/09/xmldsig#");
        serialNumber.InnerText = _certificate.SerialNumber;
        
        issuerSerial.AppendChild(issuerName);
        issuerSerial.AppendChild(serialNumber);
        
        cert.AppendChild(certDigest);
        cert.AppendChild(issuerSerial);
        signingCert.AppendChild(cert);
        
        return signingCert;
    }
    
    private XmlElement CrearSignaturePolicyIdentifier(XmlDocument xmlDoc)
    {
        var policyIdentifier = xmlDoc.CreateElement("xades", "SignaturePolicyIdentifier", "http://uri.etsi.org/01903/v1.3.2#");
        var policyId = xmlDoc.CreateElement("xades", "SignaturePolicyId", "http://uri.etsi.org/01903/v1.3.2#");
        var sigPolicyId = xmlDoc.CreateElement("xades", "SigPolicyId", "http://uri.etsi.org/01903/v1.3.2#");
        var identifier = xmlDoc.CreateElement("xades", "Identifier", "http://uri.etsi.org/01903/v1.3.2#");
        identifier.InnerText = POLICY_URL;
        
        sigPolicyId.AppendChild(identifier);
        
        var sigPolicyHash = xmlDoc.CreateElement("xades", "SigPolicyHash", "http://uri.etsi.org/01903/v1.3.2#");
        var digestMethod = xmlDoc.CreateElement("ds", "DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
        digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
        var digestValue = xmlDoc.CreateElement("ds", "DigestValue", "http://www.w3.org/2000/09/xmldsig#");
        digestValue.InnerText = "HASH_DE_LA_POLITICA"; // Calcular hash del PDF de política
        
        sigPolicyHash.AppendChild(digestMethod);
        sigPolicyHash.AppendChild(digestValue);
        
        policyId.AppendChild(sigPolicyId);
        policyId.AppendChild(sigPolicyHash);
        policyIdentifier.AppendChild(policyId);
        
        return policyIdentifier;
    }
}
```

### 8.4 Notas Importantes sobre Firma

```
⚠️ PROBLEMAS COMUNES EN .NET 6+:

1. RSACng vs RSACryptoServiceProvider
   - En .NET 6+, usar GetRSAPrivateKey() en lugar de PrivateKey
   - RSACng maneja mejor los certificados modernos

2. Preservar Whitespace
   - CRÍTICO: XmlDocument.PreserveWhitespace = true
   - Cualquier cambio en espacios invalida la firma

3. Canonicalización
   - Usar C14N estricto, no C14N exclusive
   - Verificar que no haya transformaciones adicionales

4. Orden de elementos
   - El orden de elementos en SignedInfo es estricto
   - References deben estar antes de calcular SignatureValue

5. Certificados .p12 de Hacienda
   - Requieren flag X509KeyStorageFlags.Exportable
   - PIN sensible a mayúsculas/minúsculas
```

---

## 9. API de Recepción de Comprobantes

### 9.1 Enviar Comprobante

**Endpoint:** `POST /recepcion`

**Headers:**
```http
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "clave": "50630112431012345678000100001010000000001112345678",
  "fecha": "2025-11-30T10:30:00-06:00",
  "emisor": {
    "tipoIdentificacion": "02",
    "numeroIdentificacion": "3101234567"
  },
  "receptor": {
    "tipoIdentificacion": "02",
    "numeroIdentificacion": "3109876543"
  },
  "callbackUrl": "https://miempresa.com/api/hacienda/callback",
  "comprobanteXml": "BASE64_DEL_XML_FIRMADO"
}
```

**Response (202 Accepted):**
```json
{
  "clave": "50630112431012345678000100001010000000001112345678",
  "fecha": "2025-11-30T10:30:00-06:00",
  "ind-estado": "recibido"
}
```

### 9.2 Consultar Estado

**Endpoint:** `GET /recepcion/{clave}`

**Headers:**
```http
Authorization: Bearer {access_token}
```

**Response (200 OK) - En proceso:**
```json
{
  "clave": "50630112431012345678000100001010000000001112345678",
  "fecha": "2025-11-30T10:30:00-06:00",
  "ind-estado": "procesando"
}
```

**Response (200 OK) - Aceptado:**
```json
{
  "clave": "50630112431012345678000100001010000000001112345678",
  "fecha": "2025-11-30T10:30:00-06:00",
  "ind-estado": "aceptado",
  "respuesta-xml": "BASE64_DEL_XML_RESPUESTA"
}
```

**Response (200 OK) - Rechazado:**
```json
{
  "clave": "50630112431012345678000100001010000000001112345678",
  "fecha": "2025-11-30T10:30:00-06:00",
  "ind-estado": "rechazado",
  "respuesta-xml": "BASE64_DEL_XML_RESPUESTA"
}
```

### 9.3 Implementación del Cliente HTTP

```csharp
public class HaciendaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly OAuthService _oauthService;
    private readonly HaciendaConfig _config;
    private readonly ILogger<HaciendaApiClient> _logger;

    public async Task<EnvioResponse> EnviarComprobanteAsync(ComprobanteRequest request)
    {
        var token = await _oauthService.GetAccessTokenAsync();
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            $"{_config.BaseUrl}/recepcion", 
            content
        );
        
        if (response.StatusCode == HttpStatusCode.Accepted)
        {
            var result = await response.Content.ReadFromJsonAsync<EnvioResponse>();
            return result;
        }
        
        var error = await response.Content.ReadAsStringAsync();
        _logger.LogError("Error enviando comprobante: {StatusCode} - {Error}", 
            response.StatusCode, error);
        
        throw new HaciendaApiException(response.StatusCode, error);
    }
    
    public async Task<ConsultaResponse> ConsultarEstadoAsync(string clave)
    {
        var token = await _oauthService.GetAccessTokenAsync();
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.GetAsync(
            $"{_config.BaseUrl}/recepcion/{clave}"
        );
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ConsultaResponse>();
        }
        
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null; // Comprobante no encontrado
        }
        
        throw new HaciendaApiException(response.StatusCode, 
            await response.Content.ReadAsStringAsync());
    }
    
    public async Task<ConsultaResponse> EsperarRespuestaAsync(
        string clave, 
        int maxIntentos = 36, // 3 minutos con intervalos de 5 segundos
        int intervaloMs = 5000)
    {
        for (int i = 0; i < maxIntentos; i++)
        {
            var resultado = await ConsultarEstadoAsync(clave);
            
            if (resultado?.IndEstado == "aceptado" || 
                resultado?.IndEstado == "rechazado")
            {
                return resultado;
            }
            
            await Task.Delay(intervaloMs);
        }
        
        throw new TimeoutException(
            $"Timeout esperando respuesta para clave: {clave}"
        );
    }
}
```

### 9.4 Manejo de Callback

```csharp
[ApiController]
[Route("api/hacienda")]
public class HaciendaCallbackController : ControllerBase
{
    private readonly IComprobanteService _comprobanteService;
    
    [HttpPost("callback")]
    public async Task<IActionResult> RecibirCallback([FromBody] CallbackPayload payload)
    {
        // Validar que viene de Hacienda (verificar origen)
        
        // Decodificar respuesta XML
        var xmlRespuesta = Encoding.UTF8.GetString(
            Convert.FromBase64String(payload.RespuestaXml)
        );
        
        // Procesar respuesta
        var respuesta = ParsearRespuestaHacienda(xmlRespuesta);
        
        // Actualizar estado en base de datos
        await _comprobanteService.ActualizarEstadoAsync(
            payload.Clave,
            respuesta.Estado,
            xmlRespuesta
        );
        
        // Notificar si fue rechazado
        if (respuesta.Estado == "rechazado")
        {
            await _comprobanteService.NotificarRechazoAsync(
                payload.Clave, 
                respuesta.MensajeError
            );
        }
        
        return Ok();
    }
}
```

---

## 10. Catálogos y Códigos Oficiales

### 10.1 API de Consulta CABYS

```http
GET https://api.hacienda.go.cr/fe/cabys?codigo=4321100000000
GET https://api.hacienda.go.cr/fe/cabys?descripcion=software
GET https://api.hacienda.go.cr/fe/cabys?q=desarrollo&top=10
```

**Response:**
```json
{
  "cabys": [
    {
      "codigo": "4321100000000",
      "descripcion": "Servicios de desarrollo de software",
      "impuesto": 13.00,
      "categoria": "Servicios de tecnología"
    }
  ]
}
```

### 10.2 Tipos de Identificación

| Código | Tipo | Formato | Longitud |
|--------|------|---------|----------|
| 01 | Cédula Física | 9 dígitos | 9 |
| 02 | Cédula Jurídica | 10 dígitos | 10 |
| 03 | DIMEX | 11-12 dígitos | 11-12 |
| 04 | NITE | 10 dígitos | 10 |
| 05 | Extranjero No Domiciliado | Alfanumérico | Hasta 20 |
| 06 | No Contribuyente | Variable | Variable |

### 10.3 Códigos de Impuesto

| Código | Descripción |
|--------|-------------|
| 01 | Impuesto al Valor Agregado (IVA) |
| 02 | Impuesto Selectivo de Consumo |
| 03 | Impuesto Único a los Combustibles |
| 04 | Impuesto Específico de Bebidas Alcohólicas |
| 05 | Impuesto Específico Bebidas Sin Alcohol y Jabones |
| 06 | Impuesto al Tabaco |
| 07 | IVA (Cálculo especial) |
| 08 | IVA Bienes Usados (Factor) |
| 12 | Impuesto Específico al Cemite Portland |
| 98 | Otros |
| 99 | Sin Impuesto |

### 10.4 Tarifas de IVA

| Código | Tarifa | Descripción |
|--------|--------|-------------|
| 01 | 0% | Tarifa 0% |
| 02 | 1% | Tarifa reducida especial |
| 03 | 2% | Tarifa reducida |
| 04 | 4% | Tarifa reducida |
| 05 | Transitorio 0% | Transitorio |
| 06 | Transitorio 4% | Transitorio |
| 07 | Transitorio 8% | Transitorio |
| 08 | 13% | Tarifa general |

### 10.5 Códigos de Referencia (para NC/ND)

| Código | Descripción | Uso |
|--------|-------------|-----|
| 01 | Anula documento de referencia | Anulación total |
| 02 | Corrige texto del documento | Corrección menor |
| 03 | Corrige monto | Ajuste de valores |
| 04 | Referencia a otro documento | Solo referencia |
| 05 | Sustituye comprobante provisional | Contingencia |
| 06 | Pago de factura | Para REP |
| 99 | Otros | Casos especiales |

### 10.6 Condiciones de Venta

| Código | Descripción |
|--------|-------------|
| 01 | Contado |
| 02 | Crédito |
| 03 | Consignación |
| 04 | Apartado |
| 05 | Arrendamiento con opción de compra |
| 06 | Arrendamiento en función financiera |
| 07 | Cobro a favor de terceros |
| 08 | Servicios prestados al Estado a crédito |
| 09 | Pago del servicio prestado al Estado |
| 99 | Otros |

### 10.7 Medios de Pago (Actualizado v4.4)

| Código | Descripción |
|--------|-------------|
| 01 | Efectivo |
| 02 | Tarjeta |
| 03 | Cheque |
| 04 | Transferencia - Depósito Bancario |
| 05 | Recaudado por Terceros |
| 06 | SINPE Móvil |
| 07 | Plataformas Digitales |
| 99 | Otros |

### 10.8 Tipos de Transacción (NUEVO v4.4)

| Código | Descripción |
|--------|-------------|
| 01 | Venta de bienes muebles e inmuebles |
| 02 | Venta de servicios |
| 03 | Importación de bienes |
| 04 | Importación de servicios |
| 05 | Exportación de bienes |
| 06 | Exportación de servicios |
| 07 | Compras locales de bienes |
| 08 | Compras locales de servicios |
| 09 | Devolución de mercancías |
| 10 | Descuentos y bonificaciones |
| 11 | Anticipos |
| 12 | Arrendamiento |
| 13 | Otros |

### 10.9 Unidades de Medida Comunes

| Código | Descripción |
|--------|-------------|
| Unid | Unidad |
| m | Metro |
| kg | Kilogramo |
| s | Segundo |
| A | Ampere |
| K | Kelvin |
| mol | Mol |
| cd | Candela |
| m² | Metro cuadrado |
| m³ | Metro cúbico |
| L | Litro |
| g | Gramo |
| Sp | Servicio Profesional |
| Spe | Servicio Personalizado |
| Cm | Comisión |
| I | Instalación |
| Os | Otros servicios |
| Al | Alquiler |
| d | Día |
| h | Hora |
| min | Minuto |

### 10.10 Códigos de Moneda (ISO 4217)

| Código | Moneda |
|--------|--------|
| CRC | Colón Costarricense |
| USD | Dólar Estadounidense |
| EUR | Euro |
| GBP | Libra Esterlina |
| JPY | Yen Japonés |
| CAD | Dólar Canadiense |
| CHF | Franco Suizo |
| CNY | Yuan Chino |
| MXN | Peso Mexicano |

---

## 11. Estados y Respuestas de Hacienda

### 11.1 Estados Posibles

| Estado | Descripción | Acción Requerida |
|--------|-------------|------------------|
| `recibido` | Comprobante recibido, pendiente de procesar | Esperar y consultar |
| `procesando` | En proceso de validación | Polling cada 5-10 segundos |
| `aceptado` | Válido tributariamente | Almacenar respuesta |
| `rechazado` | Contiene errores | Corregir y reemitir |

### 11.2 Estructura XML de Respuesta (Mensaje Hacienda)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<MensajeHacienda xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeHacienda">
    <Clave>50630112431012345678000100001010000000001112345678</Clave>
    <NombreEmisor>MI EMPRESA S.A.</NombreEmisor>
    <TipoIdentificacionEmisor>02</TipoIdentificacionEmisor>
    <NumeroCedulaEmisor>3101234567</NumeroCedulaEmisor>
    <NombreReceptor>CLIENTE EJEMPLO S.A.</NombreReceptor>
    <TipoIdentificacionReceptor>02</TipoIdentificacionReceptor>
    <NumeroCedulaReceptor>3109876543</NumeroCedulaReceptor>
    <Mensaje>1</Mensaje> <!-- 1=Aceptado, 2=Aceptado Parcial, 3=Rechazado -->
    <DetalleMensaje>Comprobante electrónico aceptado</DetalleMensaje>
    <MontoTotalImpuesto>117000.00</MontoTotalImpuesto>
    <TotalFactura>1022000.00</TotalFactura>
    <!-- Firma digital de Hacienda -->
    <ds:Signature>...</ds:Signature>
</MensajeHacienda>
```

### 11.3 Códigos de Mensaje

| Código | Significado | Descripción |
|--------|-------------|-------------|
| 1 | Aceptado | Comprobante válido y registrado |
| 2 | Aceptación Parcial | Válido con observaciones |
| 3 | Rechazado | Contiene errores, no válido |

### 11.4 Parser de Respuesta

```csharp
public class RespuestaHacienda
{
    public string Clave { get; set; }
    public int CodigoMensaje { get; set; }
    public string DetalleMensaje { get; set; }
    public List<ErrorHacienda> Errores { get; set; }
    public decimal MontoTotalImpuesto { get; set; }
    public decimal TotalFactura { get; set; }
    public string XmlOriginal { get; set; }
    
    public bool EsAceptado => CodigoMensaje == 1;
    public bool EsAceptadoParcial => CodigoMensaje == 2;
    public bool EsRechazado => CodigoMensaje == 3;
    
    public static RespuestaHacienda Parsear(string xmlBase64)
    {
        var xml = Encoding.UTF8.GetString(Convert.FromBase64String(xmlBase64));
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        
        var nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("mh", "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeHacienda");
        
        return new RespuestaHacienda
        {
            Clave = doc.SelectSingleNode("//mh:Clave", nsMgr)?.InnerText,
            CodigoMensaje = int.Parse(doc.SelectSingleNode("//mh:Mensaje", nsMgr)?.InnerText ?? "0"),
            DetalleMensaje = doc.SelectSingleNode("//mh:DetalleMensaje", nsMgr)?.InnerText,
            MontoTotalImpuesto = decimal.Parse(
                doc.SelectSingleNode("//mh:MontoTotalImpuesto", nsMgr)?.InnerText ?? "0"
            ),
            TotalFactura = decimal.Parse(
                doc.SelectSingleNode("//mh:TotalFactura", nsMgr)?.InnerText ?? "0"
            ),
            XmlOriginal = xml
        };
    }
}
```

---

## 12. Manejo de Errores Comunes

### 12.1 Errores de Validación de Hacienda

| Código | Mensaje | Causa | Solución |
|--------|---------|-------|----------|
| -400 | Código CABYS no existe | Código producto inválido | Verificar código en API CABYS |
| 5 | Identificación inválida | Formato de cédula incorrecto | Validar tipo y longitud |
| 6 | Montos incorrectos | Cálculos no cuadran | Verificar subtotales e impuestos |
| 10 | Consecutivo duplicado | Número ya usado | Verificar secuencia |
| 15 | Contribuyente no inscrito | Cédula no registrada en ATV | Verificar inscripción |
| 20 | Llave criptográfica inválida | Certificado expirado o incorrecto | Renovar llave en ATV |
| 25 | Firma digital inválida | Error en proceso de firma | Verificar implementación XAdES |
| 30 | Fecha fuera de rango | Más de 24 horas de diferencia | Ajustar fecha emisión |
| 35 | Código actividad inválido | CIIU no registrado para emisor | Verificar en ATV |

### 12.2 Errores HTTP de la API

| Código HTTP | Significado | Acción |
|-------------|-------------|--------|
| 400 | Bad Request | Revisar estructura JSON |
| 401 | Unauthorized | Token expirado, renovar |
| 403 | Forbidden | Sin permisos, verificar credenciales |
| 404 | Not Found | Clave no existe |
| 429 | Too Many Requests | Rate limit, esperar |
| 500 | Internal Server Error | Error de Hacienda, reintentar |
| 503 | Service Unavailable | Sistema caído, usar contingencia |

### 12.3 Implementación de Reintentos

```csharp
public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly int _baseDelayMs;
    
    public RetryPolicy(int maxRetries = 3, int baseDelayMs = 1000)
    {
        _maxRetries = maxRetries;
        _baseDelayMs = baseDelayMs;
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        int attempt = 0;
        
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex) when (IsRetryable(ex) && attempt < _maxRetries)
            {
                attempt++;
                var delay = _baseDelayMs * Math.Pow(2, attempt); // Exponential backoff
                await Task.Delay((int)delay);
            }
            catch (HaciendaApiException ex) when (IsRetryable(ex.StatusCode) && attempt < _maxRetries)
            {
                attempt++;
                var delay = _baseDelayMs * Math.Pow(2, attempt);
                await Task.Delay((int)delay);
            }
        }
    }
    
    private bool IsRetryable(HttpRequestException ex)
    {
        return ex.Message.Contains("503") || 
               ex.Message.Contains("429") ||
               ex.Message.Contains("timeout");
    }
    
    private bool IsRetryable(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.ServiceUnavailable ||
               statusCode == HttpStatusCode.TooManyRequests ||
               statusCode == HttpStatusCode.GatewayTimeout;
    }
}
```

### 12.4 Validador Pre-envío

```csharp
public class ComprobanteValidator
{
    public ValidationResult Validar(ComprobanteElectronico comprobante)
    {
        var errores = new List<string>();
        
        // Validar clave numérica
        if (!ClaveNumerica.Validar(comprobante.Clave))
            errores.Add("Clave numérica inválida");
        
        // Validar longitud de clave
        if (comprobante.Clave?.Length != 50)
            errores.Add("La clave debe tener exactamente 50 dígitos");
        
        // Validar consecutivo
        if (comprobante.NumeroConsecutivo?.Length != 20)
            errores.Add("El consecutivo debe tener exactamente 20 dígitos");
        
        // Validar fecha
        if (comprobante.FechaEmision > DateTime.Now.AddHours(1) ||
            comprobante.FechaEmision < DateTime.Now.AddHours(-24))
            errores.Add("Fecha de emisión fuera del rango permitido (±24 horas)");
        
        // Validar emisor
        if (!ValidarIdentificacion(comprobante.Emisor.Identificacion))
            errores.Add("Identificación del emisor inválida");
        
        // Validar receptor (si aplica)
        if (comprobante.RequiereReceptor && comprobante.Receptor == null)
            errores.Add("El receptor es obligatorio para este tipo de documento");
        
        // Validar líneas de detalle
        if (comprobante.DetalleServicio?.LineaDetalle == null || 
            !comprobante.DetalleServicio.LineaDetalle.Any())
            errores.Add("Debe incluir al menos una línea de detalle");
        
        foreach (var linea in comprobante.DetalleServicio?.LineaDetalle ?? new List<LineaDetalle>())
        {
            // Validar CABYS
            if (string.IsNullOrEmpty(linea.Codigo) || linea.Codigo.Length != 13)
                errores.Add($"Línea {linea.NumeroLinea}: Código CABYS inválido");
            
            // Validar cálculos
            var montoEsperado = linea.Cantidad * linea.PrecioUnitario;
            if (Math.Abs(linea.MontoTotal - montoEsperado) > 0.01m)
                errores.Add($"Línea {linea.NumeroLinea}: Monto total no coincide con cantidad × precio");
            
            // Validar TipoTransaccion (requerido en v4.4 excepto TE y REP)
            if (comprobante.RequiereTipoTransaccion && string.IsNullOrEmpty(linea.TipoTransaccion))
                errores.Add($"Línea {linea.NumeroLinea}: TipoTransaccion es obligatorio en v4.4");
        }
        
        // Validar totales
        var totalCalculado = comprobante.DetalleServicio?.LineaDetalle?.Sum(l => l.MontoTotalLinea) ?? 0;
        if (Math.Abs(comprobante.ResumenFactura.TotalComprobante - totalCalculado - 
            (comprobante.OtrosCargos?.Sum(c => c.MontoCargo) ?? 0)) > 0.01m)
            errores.Add("El total del comprobante no coincide con la suma de líneas");
        
        // Validar ProveedorSistemas (obligatorio en v4.4)
        if (comprobante.ProveedorSistemas?.Identificacion == null)
            errores.Add("ProveedorSistemas es obligatorio en versión 4.4");
        
        // Validar CodigoActividad del emisor (obligatorio en v4.4)
        if (string.IsNullOrEmpty(comprobante.CodigoActividad))
            errores.Add("CodigoActividad del emisor es obligatorio en versión 4.4");
        
        return new ValidationResult
        {
            EsValido = !errores.Any(),
            Errores = errores
        };
    }
    
    private bool ValidarIdentificacion(Identificacion id)
    {
        if (id == null) return false;
        
        return id.Tipo switch
        {
            "01" => id.Numero.Length == 9 && id.Numero.All(char.IsDigit),
            "02" => id.Numero.Length == 10 && id.Numero.All(char.IsDigit),
            "03" => (id.Numero.Length == 11 || id.Numero.Length == 12) && id.Numero.All(char.IsDigit),
            "04" => id.Numero.Length == 10 && id.Numero.All(char.IsDigit),
            "05" => id.Numero.Length <= 20,
            "06" => true, // No contribuyente, flexible
            _ => false
        };
    }
}
```

---

## 13. Contingencias

### 13.1 Situaciones de Contingencia

| Situación | Código | Descripción |
|-----------|--------|-------------|
| Normal | 1 | Sistema funcionando correctamente |
| Contingencia | 2 | Fallo del sistema de Hacienda |
| Sin Internet | 3 | Sin conexión a internet |

### 13.2 Procedimiento de Contingencia

```
1. DETECTAR INDISPONIBILIDAD
   ├── Timeout en conexión (>30 segundos)
   ├── Error HTTP 503 (Service Unavailable)
   ├── Error HTTP 500 persistente
   └── Sin conectividad de red

2. ACTIVAR MODO CONTINGENCIA
   ├── Cambiar situación en clave numérica a "2" o "3"
   ├── Generar comprobantes con consecutivo de contingencia
   ├── Almacenar localmente XML firmados
   └── Opcional: Usar comprobantes pre-impresos

3. DURANTE LA CONTINGENCIA
   ├── Seguir generando comprobantes normalmente
   ├── Mantener consecutivos únicos
   ├── Registrar todas las operaciones
   └── Informar a clientes si es necesario

4. RECUPERACIÓN
   ├── Detectar restablecimiento del servicio
   ├── Enviar comprobantes pendientes (FIFO)
   ├── Plazo máximo: 2 días hábiles
   └── Verificar aceptación de cada uno

5. SI SE USARON COMPROBANTES PRE-IMPRESOS
   ├── Emitir comprobante electrónico sustituto
   ├── Usar código de referencia "05" (Sustituye provisional)
   └── Referenciar número del comprobante pre-impreso
```

### 13.3 Implementación de Cola de Contingencia

```csharp
public class ContingencyQueue
{
    private readonly IDbContext _dbContext;
    private readonly IHaciendaApiClient _apiClient;
    private readonly ILogger<ContingencyQueue> _logger;
    
    public async Task EncolarComprobanteAsync(ComprobanteElectronico comprobante)
    {
        var pendiente = new ComprobantePendiente
        {
            Id = Guid.NewGuid(),
            Clave = comprobante.Clave,
            XmlFirmado = comprobante.XmlFirmado,
            FechaCreacion = DateTime.UtcNow,
            Intentos = 0,
            Estado = EstadoPendiente.Pendiente
        };
        
        await _dbContext.ComprobantesPendientes.AddAsync(pendiente);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task ProcesarColaAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var pendientes = await _dbContext.ComprobantesPendientes
                .Where(c => c.Estado == EstadoPendiente.Pendiente)
                .Where(c => c.Intentos < 5)
                .OrderBy(c => c.FechaCreacion)
                .Take(10)
                .ToListAsync(cancellationToken);
            
            if (!pendientes.Any())
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                continue;
            }
            
            foreach (var pendiente in pendientes)
            {
                try
                {
                    // Verificar si ya está procesado
                    var estado = await _apiClient.ConsultarEstadoAsync(pendiente.Clave);
                    
                    if (estado?.IndEstado == "aceptado" || estado?.IndEstado == "rechazado")
                    {
                        pendiente.Estado = estado.IndEstado == "aceptado" 
                            ? EstadoPendiente.Aceptado 
                            : EstadoPendiente.Rechazado;
                        pendiente.RespuestaXml = estado.RespuestaXml;
                    }
                    else
                    {
                        // Intentar enviar
                        var request = CrearRequest(pendiente);
                        var resultado = await _apiClient.EnviarComprobanteAsync(request);
                        
                        pendiente.Intentos++;
                        
                        if (resultado.IndEstado == "recibido")
                        {
                            pendiente.Estado = EstadoPendiente.Enviado;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error procesando pendiente {Clave}", pendiente.Clave);
                    pendiente.Intentos++;
                    pendiente.UltimoError = ex.Message;
                }
                
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                // Pequeña pausa entre envíos
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}
```

---

## 14. Testing y Validación

### 14.1 Ambiente de Sandbox

```
URL Base: https://api.comprobanteselectronicos.go.cr/recepcion-sandbox/v1
OAuth URL: https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token
Client ID: api-stag
```

### 14.2 Datos de Prueba Sugeridos

```json
{
  "emisor": {
    "tipo": "02",
    "numero": "3101234567",
    "nombre": "EMPRESA PRUEBA S.A.",
    "actividad": "620101"
  },
  "receptores_prueba": [
    {
      "tipo": "02",
      "numero": "3102000000",
      "nombre": "RECEPTOR JURÍDICO PRUEBA"
    },
    {
      "tipo": "01",
      "numero": "101230456",
      "nombre": "RECEPTOR FÍSICO PRUEBA"
    }
  ],
  "productos_cabys": [
    {
      "codigo": "4321100000000",
      "descripcion": "Servicios de desarrollo de software",
      "iva": 13
    },
    {
      "codigo": "2399100000000",
      "descripcion": "Otros productos manufacturados n.c.p.",
      "iva": 13
    }
  ]
}
```

### 14.3 Validador de XML Local

```csharp
public class XmlSchemaValidator
{
    private readonly XmlSchemaSet _schemaSet;
    
    public XmlSchemaValidator()
    {
        _schemaSet = new XmlSchemaSet();
        
        // Cargar XSDs oficiales
        _schemaSet.Add(null, "schemas/FacturaElectronica_V4.4.xsd");
        _schemaSet.Add(null, "schemas/NotaCreditoElectronica_V4.4.xsd");
        _schemaSet.Add(null, "schemas/NotaDebitoElectronica_V4.4.xsd");
        _schemaSet.Add(null, "schemas/TiqueteElectronico_V4.4.xsd");
        _schemaSet.Add(null, "schemas/FacturaElectronicaExportacion_V4.4.xsd");
        _schemaSet.Add(null, "schemas/FacturaElectronicaCompra_V4.4.xsd");
        _schemaSet.Add(null, "schemas/ReciboElectronicoPago_V4.4.xsd");
        
        _schemaSet.Compile();
    }
    
    public ValidationResult Validar(string xml)
    {
        var errores = new List<string>();
        
        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = _schemaSet
        };
        
        settings.ValidationEventHandler += (sender, args) =>
        {
            errores.Add($"[{args.Severity}] Línea {args.Exception.LineNumber}: {args.Message}");
        };
        
        using (var reader = XmlReader.Create(new StringReader(xml), settings))
        {
            try
            {
                while (reader.Read()) { }
            }
            catch (XmlException ex)
            {
                errores.Add($"Error XML: {ex.Message}");
            }
        }
        
        return new ValidationResult
        {
            EsValido = !errores.Any(),
            Errores = errores
        };
    }
}
```

### 14.4 Tests Unitarios Esenciales

```csharp
[TestClass]
public class FacturacionElectronicaTests
{
    [TestMethod]
    public void ClaveNumerica_DebeGenerar50Digitos()
    {
        var clave = ClaveNumerica.Generar(
            fecha: DateTime.Now,
            identificacionEmisor: "3101234567",
            sucursal: "001",
            terminal: "00001",
            tipoDocumento: "01",
            numeroSecuencial: 1
        );
        
        Assert.AreEqual(50, clave.Length);
        Assert.IsTrue(clave.All(char.IsDigit));
        Assert.IsTrue(clave.StartsWith("506"));
    }
    
    [TestMethod]
    public void ClaveNumerica_DebeIncluirFechaCorrecta()
    {
        var fecha = new DateTime(2025, 11, 30);
        var clave = ClaveNumerica.Generar(
            fecha: fecha,
            identificacionEmisor: "3101234567",
            sucursal: "001",
            terminal: "00001",
            tipoDocumento: "01",
            numeroSecuencial: 1
        );
        
        var fechaEnClave = clave.Substring(3, 6);
        Assert.AreEqual("301125", fechaEnClave); // DDMMAA
    }
    
    [TestMethod]
    public void Consecutivo_DebeSerUnicoPorTipo()
    {
        var consecutivoFE = "00100001010000000001";
        var consecutivoNC = "00100001030000000001";
        
        Assert.AreEqual("01", consecutivoFE.Substring(10, 2)); // Factura
        Assert.AreEqual("03", consecutivoNC.Substring(10, 2)); // Nota Crédito
    }
    
    [TestMethod]
    public void XmlFactura_DebeValidarContraXSD()
    {
        var xml = GenerarXmlFacturaPrueba();
        var validator = new XmlSchemaValidator();
        
        var resultado = validator.Validar(xml);
        
        Assert.IsTrue(resultado.EsValido, string.Join("\n", resultado.Errores));
    }
    
    [TestMethod]
    public void Firma_DebeSerValidaXAdES()
    {
        var xml = GenerarXmlFacturaPrueba();
        var signer = new XadesEpesSigner("test.p12", "1234");
        
        var xmlFirmado = signer.FirmarDocumento(xml);
        
        Assert.IsTrue(xmlFirmado.Contains("<ds:Signature"));
        Assert.IsTrue(xmlFirmado.Contains("xades:SignedProperties"));
    }
    
    [TestMethod]
    public void Calculos_DebenCuadrar()
    {
        var linea = new LineaDetalle
        {
            Cantidad = 10,
            PrecioUnitario = 100,
            TarifaIva = 13
        };
        
        var montoTotal = linea.Cantidad * linea.PrecioUnitario;
        var impuesto = montoTotal * (linea.TarifaIva / 100);
        var total = montoTotal + impuesto;
        
        Assert.AreEqual(1000, montoTotal);
        Assert.AreEqual(130, impuesto);
        Assert.AreEqual(1130, total);
    }
    
    [TestMethod]
    public void Identificacion_DebeValidarFormato()
    {
        Assert.IsTrue(ValidarIdentificacion("01", "101230456")); // Física 9 dígitos
        Assert.IsTrue(ValidarIdentificacion("02", "3101234567")); // Jurídica 10 dígitos
        Assert.IsTrue(ValidarIdentificacion("03", "12345678901")); // DIMEX 11 dígitos
        
        Assert.IsFalse(ValidarIdentificacion("01", "1012304567")); // Física con 10 dígitos
        Assert.IsFalse(ValidarIdentificacion("02", "310123456")); // Jurídica con 9 dígitos
    }
}
```

---

## 15. Recursos y Enlaces Oficiales

### 15.1 Documentación Oficial de Hacienda

| Recurso | URL |
|---------|-----|
| Portal ATV | https://atv.hacienda.go.cr |
| Anexos y Estructuras v4.4 | https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/frmAnexosyEstructuras.aspx |
| Documentación API | https://atv.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2024/v4.4/ |
| Resolución DGT-R-019-2022 | https://www.hacienda.go.cr/docs/ |

### 15.2 XSD Oficiales v4.4

| Documento | URL |
|-----------|-----|
| Factura Electrónica | https://www.hacienda.go.cr/docs/FacturaElectronica_V4.4.xsd |
| Nota de Crédito | https://www.hacienda.go.cr/docs/NotaCreditoElectronica_V4.4.xsd |
| Nota de Débito | https://www.hacienda.go.cr/docs/NotaDebitoElectronica_V4.4.xsd |
| Tiquete Electrónico | https://www.hacienda.go.cr/docs/TiqueteElectronico_V4.4.xsd |
| Factura Exportación | https://www.hacienda.go.cr/docs/FacturaElectronicaExportacion_V4.4.xsd |
| Factura de Compra | https://www.hacienda.go.cr/docs/FacturaElectronicaCompra_V4.4.xsd |
| Recibo Electrónico Pago | https://www.hacienda.go.cr/docs/ReciboElectronicoPago_V4.4.xsd |
| Mensaje Hacienda | https://www.hacienda.go.cr/docs/MensajeHacienda_V4.4.xsd |

### 15.3 APIs Públicas de Hacienda

| API | Endpoint | Descripción |
|-----|----------|-------------|
| CABYS | https://api.hacienda.go.cr/fe/cabys | Catálogo de bienes y servicios |
| Actividades | https://api.hacienda.go.cr/fe/ae | Actividades económicas |
| Exoneraciones | https://api.hacienda.go.cr/fe/ex | Autorizaciones de exoneración |
| Tipo de Cambio | https://api.hacienda.go.cr/indicadores/tc | Tipo de cambio oficial |

### 15.4 Herramientas de la Comunidad

| Herramienta | URL | Descripción |
|-------------|-----|-------------|
| CRLibre API | https://github.com/CRLibre/API_Hacienda | API open source para FE |
| Validador GoMeta | https://apis.gometa.org/validar/ | Validador de XML |
| FE Costa Rica FB | https://www.facebook.com/groups/facaborja | Comunidad de desarrolladores |

### 15.5 Librerías Recomendadas

| Lenguaje | Librería | Uso |
|----------|----------|-----|
| .NET | FirmaXadesNet | Firma XAdES-EPES |
| Java | xades4j | Firma XAdES |
| Ruby | facturacr | Cliente completo |
| PHP | CRLibre | Cliente API |
| Node.js | cr-factura-electronica | Generador XML |

---

## 📝 Notas de Versión

### Cambios de v4.3 a v4.4 (Septiembre 2025)

1. **ProveedorSistemas**: Obligatorio - Identificación del desarrollador del software
2. **CodigoActividadEmisor**: Obligatorio - Código CIIU del emisor
3. **CodigoActividadReceptor**: Condicional - Según tipo de documento
4. **TipoTransaccion**: Obligatorio en líneas (excepto TE y REP)
5. **Recibo Electrónico de Pago (10)**: Nuevo tipo de documento
6. **DetalleSurtido**: Nuevo campo para combos/paquetes
7. **Nuevos campos farmacéuticos**: RegistroMedicamento, FormaFarmaceutica
8. **NumeroVINoSerie**: Para vehículos, hasta 1000 repeticiones
9. **Nuevos medios de pago**: SINPE Móvil (06), Plataformas Digitales (07)
10. **146 cambios técnicos** en total respecto a v4.3

---

## 🔄 Historial del Documento

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | 2025-11-30 | Versión inicial completa para v4.4 |

---

**Autor:** Generado con Claude AI para Infinitech  
**Licencia:** Uso libre para implementación de facturación electrónica en Costa Rica
