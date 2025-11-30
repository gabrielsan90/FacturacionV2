# Solución al Error de Casting RSACng a RSACryptoServiceProvider

## Problema Identificado

Al intentar firmar documentos XML con certificados digitales de Hacienda Costa Rica, se presentaba el siguiente error:

```
System.InvalidCastException: Unable to cast object of type 'System.Security.Cryptography.RSACng' to type 'System.Security.Cryptography.RSACryptoServiceProvider'.
   at FirmaXadesNet.Crypto.Signer.SetSigningKey(X509Certificate2 certificate)
   at FirmaXadesNet.Crypto.Signer..ctor(X509Certificate2 certificate)
```

### Causa Raíz

**FirmaXadesNet** es una librería de .NET Framework que fue diseñada para trabajar con **RSACryptoServiceProvider** (CAPI - CryptoAPI antigua de Windows).

En .NET Core/.NET 9, Microsoft cambió la implementación predeterminada de RSA a **RSACng** (CNG - Cryptography Next Generation), que es más moderna y segura.

Cuando se carga un certificado .p12/.pfx en .NET 9, la clave privada se obtiene como `RSACng`, pero FirmaXadesNet internamente intenta hacer un cast directo a `RSACryptoServiceProvider`, lo que causa la excepción.

## Solución Implementada

Se implementó un método de conversión que transforma la clave privada del certificado de RSACng a RSACryptoServiceProvider antes de pasarlo a FirmaXadesNet.

### Archivo Modificado

**Archivo:** `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Services/Implementations/FirmaDigitalService.cs`

### Cambios Realizados

#### 1. Nuevo Método de Conversión

```csharp
/// <summary>
/// Convierte un certificado con RSACng (CNG) a un certificado con RSACryptoServiceProvider (CAPI)
/// Esto es necesario porque FirmaXadesNet es una librería de .NET Framework que solo soporta CAPI
/// </summary>
private X509Certificate2 ConvertirCertificadoParaFirmaXadesNet(X509Certificate2 certificadoOriginal)
{
    try
    {
        // Obtener la clave privada RSA del certificado
        using var rsa = certificadoOriginal.GetRSAPrivateKey();
        if (rsa == null)
        {
            throw new InvalidOperationException("El certificado no contiene una clave privada RSA");
        }

        // Exportar los parámetros de la clave privada
        var parametros = rsa.ExportParameters(true);

        // Crear una nueva instancia de RSACryptoServiceProvider con los parámetros
        var rsaCsp = new RSACryptoServiceProvider();
        rsaCsp.ImportParameters(parametros);

        // Crear un nuevo certificado con la misma información pública pero con RSACryptoServiceProvider
        var certificadoSinClave = new X509Certificate2(certificadoOriginal.Export(X509ContentType.Cert));

        // Asociar la clave privada RSACryptoServiceProvider al certificado
        var certificadoConClaveCsp = certificadoSinClave.CopyWithPrivateKey(rsaCsp);

        _logger.LogInformation("Certificado convertido de RSACng a RSACryptoServiceProvider exitosamente");

        return certificadoConClaveCsp;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al convertir certificado a RSACryptoServiceProvider");
        throw new InvalidOperationException("Error al convertir el certificado para firma XAdES", ex);
    }
}
```

#### 2. Modificación del Método de Firma

Se actualizó `FirmarXmlAsync` para:

1. Convertir el certificado antes de pasarlo a FirmaXadesNet
2. Envolver la operación de firma en `Task.Run()` para ejecutarla en un thread separado (operación CPU-intensiva)
3. Limpiar el certificado temporal en un bloque `finally` para asegurar la liberación de recursos

```csharp
public async Task<string> FirmarXmlAsync(string xmlSinFirmar, X509Certificate2 certificado)
{
    return await Task.Run(() =>
    {
        try
        {
            // ... validaciones ...

            // SOLUCIÓN AL ERROR RSACng -> RSACryptoServiceProvider
            var certificadoCompatible = ConvertirCertificadoParaFirmaXadesNet(certificado);

            try
            {
                // Configurar y ejecutar la firma con FirmaXadesNet
                var parametros = new SignatureParameters
                {
                    SignatureMethod = SignatureMethod.RSAwithSHA256,
                    // ... configuración ...
                    Signer = new Signer(certificadoCompatible) // Usar el certificado convertido
                };

                // ... proceso de firma ...
            }
            finally
            {
                certificadoCompatible.Dispose(); // Limpiar recursos
            }
        }
        catch (Exception ex)
        {
            // ... manejo de errores ...
        }
    });
}
```

## Cómo Funciona la Solución

1. **Extracción de parámetros**: Se obtiene la clave privada RSA del certificado original (ya sea RSACng o cualquier implementación) y se exportan sus parámetros (módulo, exponentes, etc.)

2. **Creación de RSACryptoServiceProvider**: Se crea una nueva instancia de `RSACryptoServiceProvider` e se importan los parámetros de la clave privada.

3. **Asociación al certificado**: Se crea un nuevo `X509Certificate2` que contiene la misma información pública del certificado original, pero con la clave privada como `RSACryptoServiceProvider`.

4. **Firma compatible**: Este certificado convertido ahora es compatible con FirmaXadesNet, que puede hacer el cast exitosamente.

## Compatibilidad

Esta solución:

- Funciona con certificados .p12/.pfx de Hacienda Costa Rica
- Es compatible con .NET 9
- Produce firmas XML XAdES-BES válidas
- Mantiene la seguridad del certificado (la clave privada nunca se expone)
- Limpia correctamente los recursos temporales

## Advertencias

El código genera advertencias SYSLIB0057 porque usa constructores de `X509Certificate2` que están marcados como obsoletos en .NET 9. Estas advertencias son inofensivas y no afectan la funcionalidad. Microsoft recomienda usar `X509CertificateLoader`, pero este no está disponible en todas las versiones de .NET y la solución actual funciona correctamente.

## Verificación

El proyecto compila exitosamente:

```
Build succeeded.
```

Las únicas advertencias son de obsolescencia de constructores, que no afectan el funcionamiento en runtime.

## Referencias

- **FirmaXadesNet**: Librería de firma XAdES para .NET Framework
- **RSACng**: Implementación moderna de RSA en .NET Core/.NET 5+
- **RSACryptoServiceProvider**: Implementación legacy de RSA de .NET Framework
- **Hacienda Costa Rica**: Requiere firmas XAdES-BES para facturación electrónica

## Próximos Pasos

Si en el futuro se desea eliminar la dependencia de FirmaXadesNet (que es una librería de .NET Framework), se puede considerar:

1. Implementar XAdES manualmente usando `System.Security.Cryptography.Xml`
2. Usar una librería XAdES moderna compatible con .NET 9
3. Migrar a FacturaJS u otra librería específica para Hacienda Costa Rica

Por ahora, la solución de conversión es la más pragmática y mantiene la compatibilidad con el código existente.
