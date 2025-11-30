# Servicio de Gestión de Tokens OAuth2 de Hacienda

## Descripción

Este documento explica el servicio de gestión de tokens OAuth2 implementado para la autenticación con el sistema de Hacienda de Costa Rica.

## Componentes Implementados

### 1. Entidad HaciendaToken
Ubicación: `/Facturacion.Shared/Entities/HaciendaToken.cs`

Almacena los tokens OAuth2 en la base de datos para reutilizarlos y evitar solicitar tokens innecesarios.

**Campos principales:**
- `AccessToken`: Token de acceso (JWT) - válido por 5 minutos
- `RefreshToken`: Token para refrescar - válido por 30 minutos
- `FechaExpiracionToken`: Cuándo expira el access token
- `FechaExpiracionRefreshToken`: Cuándo expira el refresh token
- `Ambiente`: "stag" (sandbox) o "prod" (producción)
- `Activo`: Si el token está activo o fue invalidado

### 2. DTO HaciendaTokenResponse
Ubicación: `/Facturacion.Shared/DTOs/HaciendaTokenResponse.cs`

Estructura de la respuesta del IDP de Hacienda:
```json
{
  "access_token": "eyJ...",
  "expires_in": 300,
  "refresh_expires_in": 1800,
  "refresh_token": "eyJ...",
  "token_type": "bearer"
}
```

### 3. Servicio HaciendaTokenService
Ubicación: `/Facturacion.Backend/Services/Implementations/HaciendaTokenService.cs`

**Métodos principales:**

#### ObtenerTokenValidoAsync(Guid empresaId, string ambiente)
Método inteligente que:
1. Verifica si existe un token guardado en BD
2. Si no existe → obtiene un nuevo token
3. Si el refresh token expiró → obtiene un nuevo token
4. Si solo el access token expiró → refresca el token
5. Si el token es válido → lo retorna directamente

**Uso:**
```csharp
var tokenService = // inyectado por DI
string token = await tokenService.ObtenerTokenValidoAsync(empresaId, "stag");
// Usar el token en peticiones HTTP
```

#### ObtenerNuevoTokenAsync(Guid empresaId, string ambiente)
Obtiene un nuevo token usando las credenciales de ATV (usuario y contraseña) con `grant_type=password`.

#### RefrescarTokenAsync(Guid empresaId, string ambiente)
Refresca un token existente usando el refresh token con `grant_type=refresh_token`.

#### InvalidarTokenAsync(Guid empresaId, string ambiente)
Invalida el token actual forzando la obtención de uno nuevo en la próxima llamada.

## Configuración en appsettings.json

```json
{
  "HaciendaIdp": {
    "UrlStaging": "https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token",
    "UrlProduction": "https://idp.comprobanteselectronicos.go.cr/auth/realms/rut/protocol/openid-connect/token",
    "ClientIdStaging": "api-stag",
    "ClientIdProduction": "api-prod",
    "Timeout": 30
  }
}
```

## Repositorio y UnitOfWork

### IHaciendaTokenRepository / HaciendaTokenRepository
Métodos CRUD para gestionar tokens en BD:
- `GetTokenActivoAsync`: Obtiene el token activo más reciente
- `AddAsync`: Guarda un nuevo token
- `UpdateAsync`: Actualiza un token
- `InvalidarTokenAsync`: Marca un token como inactivo
- `LimpiarTokensExpiradosAsync`: Elimina tokens antiguos expirados

### IHaciendaTokenUnitOfWork / HaciendaTokenUnitOfWork
Capa de abstracción sobre el repositorio siguiendo el patrón Unit of Work del sistema.

## Flujo de Autenticación OAuth2

### 1. Obtener Nuevo Token (grant_type=password)
```http
POST https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

client_id=api-stag
grant_type=password
username=<usuario_atv>
password=<clave_atv>
```

### 2. Refrescar Token (grant_type=refresh_token)
```http
POST https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

client_id=api-stag
grant_type=refresh_token
refresh_token=<refresh_token_anterior>
```

## Lógica de Decisión del Servicio

```
┌─────────────────────────────────────┐
│ ObtenerTokenValidoAsync(empresaId)  │
└─────────────────┬───────────────────┘
                  │
                  ▼
        ¿Existe token en BD?
                  │
         ┌────────┴────────┐
         NO                YES
         │                  │
         │                  ▼
         │       ¿RefreshToken expiró?
         │                  │
         │         ┌────────┴────────┐
         │        YES                NO
         │         │                  │
         │         │                  ▼
         │         │       ¿AccessToken expiró?
         │         │                  │
         │         │         ┌────────┴────────┐
         │         │        YES                NO
         │         │         │                  │
         ▼         ▼         ▼                  ▼
  Obtener Nuevo  Obtener   Refrescar       Retornar
     Token        Nuevo      Token           Token
   (password)   (password)  (refresh)       Existente
         │         │         │                  │
         └─────────┴─────────┴──────────────────┘
                          │
                          ▼
                  Guardar en BD
                          │
                          ▼
                  Retornar Token
```

## Ejemplo de Uso en DocumentoHaciendaService

```csharp
public class DocumentoHaciendaService : IDocumentoHaciendaService
{
    private readonly IHaciendaTokenService _tokenService;
    private readonly IHaciendaApiService _haciendaApi;

    public DocumentoHaciendaService(
        IHaciendaTokenService tokenService,
        IHaciendaApiService haciendaApi)
    {
        _tokenService = tokenService;
        _haciendaApi = haciendaApi;
    }

    public async Task<ResultadoEnvio> EnviarDocumentoAsync(Guid documentoId)
    {
        // Obtener documento...
        var documento = await ObtenerDocumentoAsync(documentoId);

        // Obtener token válido automáticamente
        string ambiente = documento.Empresa.Ambiente == Ambiente.Produccion ? "prod" : "stag";
        string token = await _tokenService.ObtenerTokenValidoAsync(documento.EmpresaId, ambiente);

        // Usar el token en la petición a Hacienda
        var resultado = await _haciendaApi.EnviarDocumentoConTokenAsync(
            documento.Clave,
            documento.XmlFirmado,
            token,
            ambiente
        );

        return resultado;
    }
}
```

## Tiempos de Expiración

- **Access Token**: 300 segundos (5 minutos)
- **Refresh Token**: 1800 segundos (30 minutos)

El servicio gestiona automáticamente estos tiempos y decide cuándo:
- Usar el token guardado
- Refrescar el token
- Obtener un nuevo token

## Ventajas de esta Implementación

1. **Eficiencia**: Reutiliza tokens válidos evitando peticiones innecesarias al IDP
2. **Automático**: El desarrollador solo llama `ObtenerTokenValidoAsync()` sin preocuparse de la lógica
3. **Persistente**: Los tokens se guardan en BD y sobreviven reinicios de la aplicación
4. **Multi-empresa**: Cada empresa tiene sus propios tokens independientes
5. **Multi-ambiente**: Gestiona tokens separados para sandbox y producción
6. **Robusto**: Maneja automáticamente la expiración y refresco de tokens

## Migración de Base de Datos

La tabla `HaciendaTokens` fue creada con la migración `AddHaciendaTokenTable`:

```sql
CREATE TABLE [HaciendaTokens] (
    [Id] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [AccessToken] nvarchar(4000) NOT NULL,
    [RefreshToken] nvarchar(4000) NOT NULL,
    [FechaExpiracionToken] datetime2 NOT NULL,
    [FechaExpiracionRefreshToken] datetime2 NOT NULL,
    [Ambiente] nvarchar(10) NOT NULL,
    [FechaCreacion] datetime2 NOT NULL DEFAULT (GETDATE()),
    [FechaActualizacion] datetime2 NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_HaciendaTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HaciendaTokens_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId])
        REFERENCES [Empresas] ([Id]) ON DELETE CASCADE
);

-- Índices para búsquedas eficientes
CREATE INDEX [IX_HaciendaToken_Empresa_Ambiente_Activo]
    ON [HaciendaTokens] ([EmpresaId], [Ambiente], [Activo]);

CREATE INDEX [IX_HaciendaToken_FechaExpiracion]
    ON [HaciendaTokens] ([FechaExpiracionToken]);

CREATE INDEX [IX_HaciendaToken_FechaExpiracionRefresh]
    ON [HaciendaTokens] ([FechaExpiracionRefreshToken]);
```

## Registro de Servicios (Program.cs)

```csharp
// Dependency Injection - Hacienda Token Module (OAuth2)
builder.Services.AddScoped<IHaciendaTokenRepository, HaciendaTokenRepository>();
builder.Services.AddScoped<IHaciendaTokenUnitOfWork, HaciendaTokenUnitOfWork>();
builder.Services.AddScoped<IHaciendaTokenService, HaciendaTokenService>();
```

## Mantenimiento

### Limpieza de Tokens Antiguos
Para evitar acumulación de tokens expirados en la BD:

```csharp
// Eliminar tokens inactivos con más de 30 días
var resultado = await _haciendaTokenUnitOfWork.LimpiarTokensExpiradosAsync(diasAntiguedad: 30);
Console.WriteLine($"Tokens eliminados: {resultado.Result}");
```

Se puede crear un Background Service que ejecute esto periódicamente.

## Manejo de Errores

El servicio maneja automáticamente:
- Errores de conexión con el IDP
- Tokens expirados
- Credenciales inválidas
- Respuestas inesperadas del servidor

Todos los errores lanzan excepciones `InvalidOperationException` con mensajes descriptivos.

## Seguridad

- Los tokens se almacenan en la BD (considerar encriptación adicional si es necesario)
- El refresh token permite obtener nuevos access tokens sin requerir credenciales
- Los tokens antiguos se invalidan automáticamente
- Cada empresa tiene aislamiento de sus tokens

## URLs de Producción vs Sandbox

**Sandbox (Pruebas)**
- IDP: `https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token`
- Client ID: `api-stag`

**Producción**
- IDP: `https://idp.comprobanteselectronicos.go.cr/auth/realms/rut/protocol/openid-connect/token`
- Client ID: `api-prod`

El servicio selecciona automáticamente la URL correcta basándose en el parámetro `ambiente`.
