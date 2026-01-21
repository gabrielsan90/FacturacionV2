using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementacion del generador de Clave numerica de 50 digitos segun Hacienda v4.4
/// Formato: PPP-DD-MM-AA-EEEEEEEEEEEE-SSSSTTTNNNNNNNNNNN-S-SSSSSSS-V
/// Estructura (50 digitos total):
/// - PPP: Pais (3 digitos - 506 = Costa Rica)
/// - DD: Dia (2 digitos)
/// - MM: Mes (2 digitos)
/// - AA: Anio (2 digitos - ultimos 2 digitos)
/// - EEEEEEEEEEEE: Cedula emisor (12 digitos, padding izquierdo con ceros)
/// - SSSSTTTNNNNNNNNNNN: Consecutivo (20 digitos)
/// - S: Situacion (1 digito: 1=Normal, 2=Contingencia, 3=Sin internet)
/// - SSSSSSS: Codigo de seguridad (7 digitos aleatorios)
/// - V: Digito verificador Modulo 11 (1 digito)
///
/// Clave Base = 3+2+2+2+12+20+1+7 = 49 digitos (sin verificador)
/// Clave Completa = 49 + 1 = 50 digitos
/// </summary>
public class ClaveGeneradorService : IClaveGeneradorService
{
    private readonly DataContext _context;

    public ClaveGeneradorService(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Genera la clave numerica de 50 digitos segun formato Hacienda v4.4
    /// Formato: PPP-DD-MM-AA-EEEEEEEEEEEE-SSSSTTTNNNNNNNNNNN-S-SSSSSSS-V
    /// </summary>
    /// <param name="documento">Documento para el cual generar la clave</param>
    /// <param name="situacion">Situacion del comprobante: 1=Normal, 2=Contingencia, 3=Sin internet</param>
    /// <returns>Clave numerica de 50 digitos</returns>
    public async Task<string> GenerarClaveAsync(Documento documento, int situacion = 1)
    {
        // Cargar entidades relacionadas si no estan cargadas
        if (documento.Empresa == null)
        {
            documento.Empresa = await _context.Set<Empresa>()
                .FirstOrDefaultAsync(e => e.Id == documento.EmpresaId)
                ?? throw new InvalidOperationException("No se encontro la empresa del documento");
        }

        if (documento.Sucursal == null)
        {
            documento.Sucursal = await _context.Set<Sucursal>()
                .FirstOrDefaultAsync(s => s.Id == documento.SucursalId)
                ?? throw new InvalidOperationException("No se encontro la sucursal del documento");
        }

        if (documento.Terminal == null)
        {
            documento.Terminal = await _context.Set<Terminal>()
                .FirstOrDefaultAsync(t => t.Id == documento.TerminalId)
                ?? throw new InvalidOperationException("No se encontro la terminal del documento");
        }

        // 1. Pais (3 digitos) - Siempre 506 para Costa Rica
        string pais = "506";

        // 2. Fecha (6 digitos: DD/MM/YY)
        string dia = documento.FechaEmision.ToString("dd");
        string mes = documento.FechaEmision.ToString("MM");
        string anio = documento.FechaEmision.ToString("yy");

        // 3. Cedula del emisor (12 digitos, rellenar con ceros a la izquierda)
        // Solo tomar digitos y asegurar exactamente 12 caracteres
        string cedulaLimpia = new string(documento.Empresa.NumeroIdentificacion
            .Where(char.IsDigit).ToArray());
        string cedulaEmisor = cedulaLimpia.Length > 12
            ? cedulaLimpia.Substring(cedulaLimpia.Length - 12) // Tomar ultimos 12 si es mas largo
            : cedulaLimpia.PadLeft(12, '0');

        // 4. Consecutivo (20 digitos)
        // Formato esperado: SSSS-TTT-TT-NNNNNNNNNN (sucursal-terminal-tipo-numero)
        // Sin guiones: SSSSTTTTTNNNNNNNNNNN = 20 digitos
        string consecutivoLimpio = new string(documento.NumeroConsecutivo
            .Where(char.IsDigit).ToArray());
        string consecutivo = consecutivoLimpio.Length > 20
            ? consecutivoLimpio.Substring(consecutivoLimpio.Length - 20) // Tomar ultimos 20 si es mas largo
            : consecutivoLimpio.PadLeft(20, '0');

        // 5. Situacion (1 digito) - Hacienda v4.4
        // 1 = Normal
        // 2 = Contingencia
        // 3 = Sin internet
        if (situacion < 1 || situacion > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(situacion), "La situacion debe ser 1, 2 o 3");
        }
        string situacionStr = situacion.ToString("D1"); // 1 digito segun v4.4

        // 6. Codigo de seguridad (7 digitos aleatorios) - Usando RandomNumberGenerator (criptograficamente seguro)
        string codigoSeguridad = GenerarCodigoSeguridadCriptografico();

        // 7. Construir clave base (49 digitos, sin digito verificador)
        // Verificar longitudes antes de construir
        if (pais.Length != 3) throw new InvalidOperationException($"Pais debe tener 3 digitos, tiene {pais.Length}");
        if (dia.Length != 2) throw new InvalidOperationException($"Dia debe tener 2 digitos, tiene {dia.Length}");
        if (mes.Length != 2) throw new InvalidOperationException($"Mes debe tener 2 digitos, tiene {mes.Length}");
        if (anio.Length != 2) throw new InvalidOperationException($"Anio debe tener 2 digitos, tiene {anio.Length}");
        if (cedulaEmisor.Length != 12) throw new InvalidOperationException($"Cedula debe tener 12 digitos, tiene {cedulaEmisor.Length}: '{cedulaEmisor}'");
        if (consecutivo.Length != 20) throw new InvalidOperationException($"Consecutivo debe tener 20 digitos, tiene {consecutivo.Length}: '{consecutivo}'");
        if (situacionStr.Length != 1) throw new InvalidOperationException($"Situacion debe tener 1 digito, tiene {situacionStr.Length}");
        if (codigoSeguridad.Length != 7) throw new InvalidOperationException($"Codigo seguridad debe tener 7 digitos, tiene {codigoSeguridad.Length}");

        string claveBase = $"{pais}{dia}{mes}{anio}{cedulaEmisor}{consecutivo}{situacionStr}{codigoSeguridad}";

        // 8. Calcular digito verificador Modulo 11
        string digitoVerificador = CalcularDigitoVerificadorModulo11(claveBase);

        // 9. Clave completa (50 digitos)
        string clave = claveBase + digitoVerificador;

        // Validar que tenga exactamente 50 caracteres
        if (clave.Length != 50)
        {
            throw new InvalidOperationException(
                $"La clave generada no tiene 50 digitos. Longitud: {clave.Length}. Clave: {clave}");
        }

        return clave;
    }

    /// <summary>
    /// Valida que una clave tenga el formato correcto segun v4.4
    /// Incluye validacion del digito verificador Modulo 11
    /// </summary>
    public bool ValidarClave(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return false;

        // Debe tener exactamente 50 caracteres
        if (clave.Length != 50)
            return false;

        // Debe ser numerica
        if (!clave.All(char.IsDigit))
            return false;

        // Validar pais (primeros 3 digitos deben ser 506)
        if (!clave.StartsWith("506"))
            return false;

        // Validar dia (posiciones 4-5, debe ser 01-31)
        if (!int.TryParse(clave.Substring(3, 2), out int dia) || dia < 1 || dia > 31)
            return false;

        // Validar mes (posiciones 6-7, debe ser 01-12)
        if (!int.TryParse(clave.Substring(5, 2), out int mes) || mes < 1 || mes > 12)
            return false;

        // Validar situacion (posicion 42, 1 digito, debe ser 1, 2 o 3) - Hacienda v4.4
        // Posicion: 3+2+2+2+12+20 = 41 (indice base 0)
        if (!int.TryParse(clave.Substring(41, 1), out int situacion) || situacion < 1 || situacion > 3)
            return false;

        // Validar digito verificador Modulo 11
        string claveBase = clave.Substring(0, 49);
        string digitoVerificadorCalculado = CalcularDigitoVerificadorModulo11(claveBase);
        string digitoVerificadorEnClave = clave.Substring(49, 1);

        if (digitoVerificadorCalculado != digitoVerificadorEnClave)
            return false;

        return true;
    }

    /// <summary>
    /// Genera un codigo de seguridad criptograficamente seguro de 7 digitos
    /// Usa RandomNumberGenerator en lugar de Random para mayor seguridad
    /// </summary>
    public string GenerarCodigoSeguridadCriptografico()
    {
        var bytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        // Convertir a numero y tomar 7 digitos
        var numero = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 10000000;
        return numero.ToString("D7");
    }

    /// <summary>
    /// Calcula el digito verificador usando Modulo 11
    /// Algoritmo: Multiplicar cada digito por pesos (2,3,4,5,6,7) ciclicamente de derecha a izquierda
    /// </summary>
    public string CalcularDigitoVerificadorModulo11(string claveBase)
    {
        if (string.IsNullOrWhiteSpace(claveBase) || claveBase.Length != 49)
        {
            throw new ArgumentException("La clave base debe tener exactamente 49 digitos", nameof(claveBase));
        }

        int[] pesos = { 2, 3, 4, 5, 6, 7 };
        int suma = 0;
        int pesoIndex = 0;

        // Recorrer de derecha a izquierda
        for (int i = claveBase.Length - 1; i >= 0; i--)
        {
            int digito = int.Parse(claveBase[i].ToString());
            suma += digito * pesos[pesoIndex];
            pesoIndex = (pesoIndex + 1) % pesos.Length;
        }

        int resto = suma % 11;
        int digitoVerificador = 11 - resto;

        // Si el resultado es 10 u 11, usar 0
        if (digitoVerificador >= 10)
            digitoVerificador = 0;

        return digitoVerificador.ToString();
    }

    /// <summary>
    /// Metodo legacy para compatibilidad - usa el nuevo metodo criptografico
    /// </summary>
    [Obsolete("Use GenerarCodigoSeguridadCriptografico() para mejor seguridad")]
    public string GenerarCodigoSeguridad()
    {
        return GenerarCodigoSeguridadCriptografico();
    }
}
