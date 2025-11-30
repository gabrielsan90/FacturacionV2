# Diagrama de Flujo: Generación de Clave Numérica de Hacienda

## Flujo Completo de Generación y Uso de la Clave

```
┌─────────────────────────────────────────────────────────────────────┐
│                     CREACIÓN DE DOCUMENTO                           │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Usuario completa formulario de documento                          │
│  - Tipo: FE, TE, NC, ND, FEC, FEE                                  │
│  - Cliente, Productos, Montos, etc.                                │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Documento guardado en BD                                           │
│  Estado: BORRADOR                                                   │
│  Clave: NULL (aún no se genera)                                     │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   USUARIO PRESIONA "ENVIAR"                         │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  DocumentoHaciendaService.ProcesarYEnviarAsync(documentoId)        │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  PASO 1: Validar Documento                                          │
│  - Estado debe ser Borrador o Pendiente                            │
│  - Validaciones de negocio (cliente, totales, etc.)                │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  PASO 2: Verificar Configuración de Empresa                         │
│  - Certificado digital configurado                                  │
│  - Credenciales ATV configuradas                                    │
│  - Ambiente (Pruebas/Producción)                                    │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  PASO 3: Determinar Situación del Documento                         │
│  - Normal (1): Envío regular                                        │
│  - Contingencia (2): Problemas con Hacienda                         │
│  - Sin Internet (3): Sin conexión                                   │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  PASO 4: ¿Documento ya tiene clave válida?                          │
└─────────────────────────────────────────────────────────────────────┘
                    │                              │
                    │ NO                           │ SÍ
                    ▼                              ▼
    ┌──────────────────────────────┐   ┌─────────────────────────┐
    │ ClaveGeneradorService        │   │ Usar clave existente    │
    │ .GenerarClaveAsync()         │   └─────────────────────────┘
    └──────────────────────────────┘              │
                    │                              │
                    └──────────────┬───────────────┘
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│           GENERACIÓN DE CLAVE (50 DÍGITOS)                          │
│                                                                     │
│  1. País (3): "506"                                                │
│  2. Día (2): documento.FechaEmision.ToString("dd")                 │
│  3. Mes (2): documento.FechaEmision.ToString("MM")                 │
│  4. Año (2): documento.FechaEmision.ToString("yy")                 │
│  5. Cédula (12): empresa.NumeroIdentificacion.PadLeft(12, '0')    │
│  6. Consecutivo (20): NumeroConsecutivo.Replace("-","").PadLeft()  │
│  7. Situación (1): "1" o "2" o "3"                                 │
│  8. Código Seg. (8): GenerarCodigoSeguridad() - aleatorio          │
│                                                                     │
│  Ejemplo: 506291125000003101234567001000010010000000112345678      │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Validar que clave tenga 50 dígitos                                 │
│  Si no cumple → Exception                                           │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Guardar Clave en BD                                                │
│  documento.Clave = "506291125..."                                   │
│  Estado: PENDIENTE                                                  │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  PASO 5: Generar XML del Documento                                  │
│  XmlGeneradorService.GenerarXmlAsync()                             │
│  - Usa la Clave generada en el tag <Clave>                         │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  PASO 6: Firmar XML Digitalmente                                    │
│  FirmaDigitalService.FirmarXmlAsync()                              │
│  - Firma XAdES-BES con certificado digital                         │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  PASO 7: Enviar a Hacienda                                          │
│  HaciendaApiService.EnviarDocumentoAsync()                         │
│  - POST a API de Hacienda                                          │
│  - Header: Authorization (credenciales ATV)                         │
│  - Body: { clave: "506...", xml: "<?xml..." }                     │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│  RESPUESTA DE HACIENDA                                              │
└─────────────────────────────────────────────────────────────────────┘
                    │                              │
        ┌───────────┴───────────┐                 │
        ▼                       ▼                 ▼
┌─────────────┐    ┌──────────────────┐   ┌──────────────┐
│  ACEPTADO   │    │   RECHAZADO      │   │  PROCESANDO  │
│             │    │                  │   │              │
│  Estado: OK │    │  Estado: Error   │   │ Estado: Pend │
│             │    │  Mensaje: "..."  │   │ Consultar +  │
└─────────────┘    └──────────────────┘   └──────────────┘
       │                    │                     │
       ▼                    ▼                     ▼
┌─────────────────────────────────────────────────────────────┐
│  Actualizar Estado en BD                                    │
│  - Aceptado: EstadoDocumento.Aceptado                       │
│  - Rechazado: EstadoDocumento.Rechazado                     │
│  - Procesando: EstadoDocumento.Procesando                   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  ENVÍO DE EMAIL AL CLIENTE                                  │
│  - Adjunto: PDF del documento                               │
│  - Adjunto: XML firmado                                     │
│  - Clave visible en el PDF                                  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    FIN DEL PROCESO                          │
│  Documento con Clave asignada y estado final                │
└─────────────────────────────────────────────────────────────┘
```

## Componentes de la Clave: Diagrama Visual

```
CLAVE DE 50 DÍGITOS: 50629112500000310123456700100001001000000001112345678
                     └─┬─┘└┬┘└┬┘└┬┘└─────┬──────┘└────────┬────────┘└┬┘└───┬───┘
                       │   │  │  │       │                │          │     │
                       │   │  │  │       │                │          │     └─ Código Seguridad (8)
                       │   │  │  │       │                │          │        Aleatorio: 12345678
                       │   │  │  │       │                │          │
                       │   │  │  │       │                │          └─ Situación (1)
                       │   │  │  │       │                │             1=Normal, 2=Contingencia, 3=Sin Internet
                       │   │  │  │       │                │
                       │   │  │  │       │                └─ Consecutivo (20)
                       │   │  │  │       │                   001-00001-001-0000000001 → 00100001001000000001
                       │   │  │  │       │
                       │   │  │  │       └─ Cédula Emisor (12)
                       │   │  │  │          3-101-234567 → 000003101234567
                       │   │  │  │
                       │   │  │  └─ Año (2): 25 (2025)
                       │   │  │
                       │   │  └─ Mes (2): 11 (Noviembre)
                       │   │
                       │   └─ Día (2): 29
                       │
                       └─ País (3): 506 (Costa Rica)
```

## Clase ClaveGeneradorService: Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────────────┐
│          ClaveGeneradorService                                  │
├─────────────────────────────────────────────────────────────────┤
│  - _context: DataContext                                        │
│  - _random: Random                                              │
├─────────────────────────────────────────────────────────────────┤
│  + GenerarClaveAsync(documento, situacion) : Task<string>       │
│    ┌─────────────────────────────────────────────────────┐    │
│    │ 1. Verificar entidades relacionadas                 │    │
│    │    - Empresa (para cédula)                          │    │
│    │    - Sucursal (para consecutivo)                    │    │
│    │    - Terminal (para consecutivo)                    │    │
│    │                                                      │    │
│    │ 2. Extraer componentes:                             │    │
│    │    País = "506"                                     │    │
│    │    Día = FechaEmision.ToString("dd")                │    │
│    │    Mes = FechaEmision.ToString("MM")                │    │
│    │    Año = FechaEmision.ToString("yy")                │    │
│    │                                                      │    │
│    │ 3. Formatear cédula:                                │    │
│    │    NumeroIdentificacion.PadLeft(12, '0')           │    │
│    │                                                      │    │
│    │ 4. Formatear consecutivo:                           │    │
│    │    NumeroConsecutivo.Replace("-", "").PadLeft(20)  │    │
│    │                                                      │    │
│    │ 5. Convertir situación a string                     │    │
│    │                                                      │    │
│    │ 6. Generar código de seguridad aleatorio           │    │
│    │                                                      │    │
│    │ 7. Concatenar todos los componentes                │    │
│    │                                                      │    │
│    │ 8. Validar longitud = 50                            │    │
│    │                                                      │    │
│    │ 9. Retornar clave                                   │    │
│    └─────────────────────────────────────────────────────┘    │
│                                                                 │
│  + ValidarClave(clave) : bool                                   │
│    ┌─────────────────────────────────────────────────────┐    │
│    │ - Verificar longitud = 50                           │    │
│    │ - Verificar solo dígitos numéricos                  │    │
│    │ - Verificar país = "506"                            │    │
│    │ - Verificar día entre 01-31                         │    │
│    │ - Verificar mes entre 01-12                         │    │
│    │ - Verificar situación entre 1-3                     │    │
│    └─────────────────────────────────────────────────────┘    │
│                                                                 │
│  + GenerarCodigoSeguridad() : string                            │
│    ┌─────────────────────────────────────────────────────┐    │
│    │ - Generar 8 dígitos aleatorios                      │    │
│    │ - for (i = 0; i < 8; i++)                          │    │
│    │     codigo += _random.Next(0, 10).ToString()       │    │
│    └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

## Ejemplo Paso a Paso

```
ENTRADA:
────────────────────────────────────────────────────────
Documento:
  - FechaEmision: 29/11/2025
  - NumeroConsecutivo: "001-00001-01-0000000001"
  - EmpresaId: {guid}

Empresa:
  - NumeroIdentificacion: "3101234567"

Situación: 1 (Normal)

PROCESO:
────────────────────────────────────────────────────────
Paso 1: País
  "506"

Paso 2: Fecha
  Día: "29"
  Mes: "11"
  Año: "25"

Paso 3: Cédula del Emisor
  Input:  "3101234567"
  Output: "000003101234567" (12 dígitos)

Paso 4: Consecutivo
  Input:  "001-00001-01-0000000001"
  Remove: "00100001010000000001" (sin guiones)
  Output: "00100001010000000001" (20 dígitos)

Paso 5: Situación
  "1"

Paso 6: Código de Seguridad
  GenerarCodigoSeguridad()
  Output: "87654321" (ejemplo - aleatorio)

Paso 7: Concatenar
  "506" + "29" + "11" + "25" + "000003101234567" +
  "00100001010000000001" + "1" + "87654321"

SALIDA:
────────────────────────────────────────────────────────
Clave: "50629112500000310123456700100001010000000001187654321"

Validación:
  Longitud: 50 ✓
  Solo dígitos: ✓
  País = 506: ✓
  Día = 29 (01-31): ✓
  Mes = 11 (01-12): ✓
  Situación = 1 (1-3): ✓

GUARDADO EN BD:
────────────────────────────────────────────────────────
UPDATE Documentos
SET Clave = '50629112500000310123456700100001010000000001187654321'
WHERE Id = {documentoId}
```

## Integración con Otros Servicios

```
┌─────────────────────────────────────────────────────────────────┐
│                  DocumentoHaciendaService                       │
│                  (Orquestador Principal)                        │
└────────────┬────────────────────────────────────────────────────┘
             │
             ├─► ClaveGeneradorService
             │   └─► Genera clave de 50 dígitos
             │
             ├─► XmlGeneradorService
             │   └─► Genera XML v4.4 (usa la clave)
             │
             ├─► FirmaDigitalService
             │   └─► Firma XML con certificado
             │
             ├─► HaciendaApiService
             │   └─► Envía XML firmado a Hacienda
             │
             └─► EmailService
                 └─► Envía PDF y XML al cliente
```

---

**Fecha**: 29 de noviembre de 2025
**Versión**: 1.0
**Estado**: Documentación completa
