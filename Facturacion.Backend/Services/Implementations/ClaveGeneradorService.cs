using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del generador de Clave numérica de 50 dígitos según Hacienda v4.4
/// Formato: CCPPPPDDDDDDDDDDSSSTTTNNNNNNNNNNNNNNNNNNNNSSSSSSSSC
/// </summary>
public class ClaveGeneradorService : IClaveGeneradorService
{
    private readonly DataContext _context;
    private readonly Random _random;

    public ClaveGeneradorService(DataContext context)
    {
        _context = context;
        _random = new Random();
    }

    /// <summary>
    /// Genera la clave numérica de 50 dígitos según formato Hacienda
    /// </summary>
    public async Task<string> GenerarClaveAsync(Documento documento, int situacion = 1)
    {
        // Cargar entidades relacionadas si no están cargadas
        if (documento.Empresa == null)
        {
            documento.Empresa = await _context.Set<Empresa>()
                .FirstOrDefaultAsync(e => e.Id == documento.EmpresaId)
                ?? throw new InvalidOperationException("No se encontró la empresa del documento");
        }

        if (documento.Sucursal == null)
        {
            documento.Sucursal = await _context.Set<Sucursal>()
                .FirstOrDefaultAsync(s => s.Id == documento.SucursalId)
                ?? throw new InvalidOperationException("No se encontró la sucursal del documento");
        }

        if (documento.Terminal == null)
        {
            documento.Terminal = await _context.Set<Terminal>()
                .FirstOrDefaultAsync(t => t.Id == documento.TerminalId)
                ?? throw new InvalidOperationException("No se encontró la terminal del documento");
        }

        // 1. País (3 dígitos) - Siempre 506 para Costa Rica
        string pais = "506";

        // 2. Fecha (6 dígitos: DD/MM/YY)
        string dia = documento.FechaEmision.ToString("dd");
        string mes = documento.FechaEmision.ToString("MM");
        string anio = documento.FechaEmision.ToString("yy");

        // 3. Cédula del emisor (12 dígitos, rellenar con ceros a la izquierda)
        string cedulaEmisor = documento.Empresa.NumeroIdentificacion.PadLeft(12, '0');

        // 4. Consecutivo (20 dígitos - tomar del NumeroConsecutivo sin guiones)
        string consecutivo = documento.NumeroConsecutivo.Replace("-", "").PadLeft(20, '0');

        // 5. Situación (1 dígito)
        // 1 = Normal
        // 2 = Contingencia
        // 3 = Sin internet
        string situacionStr = situacion.ToString();

        // 6. Código de seguridad (8 dígitos aleatorios)
        string codigoSeguridad = GenerarCodigoSeguridad();

        // Construir la clave completa (50 dígitos)
        string clave = $"{pais}{dia}{mes}{anio}{cedulaEmisor}{consecutivo}{situacionStr}{codigoSeguridad}";

        // Validar que tenga exactamente 50 caracteres
        if (clave.Length != 50)
        {
            throw new InvalidOperationException(
                $"La clave generada no tiene 50 dígitos. Longitud: {clave.Length}. Clave: {clave}");
        }

        return clave;
    }

    /// <summary>
    /// Valida que una clave tenga el formato correcto
    /// </summary>
    public bool ValidarClave(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return false;

        // Debe tener exactamente 50 caracteres
        if (clave.Length != 50)
            return false;

        // Debe ser numérica
        if (!clave.All(char.IsDigit))
            return false;

        // Validar país (primeros 3 dígitos deben ser 506)
        if (!clave.StartsWith("506"))
            return false;

        // Validar día (posiciones 4-5, debe ser 01-31)
        if (!int.TryParse(clave.Substring(3, 2), out int dia) || dia < 1 || dia > 31)
            return false;

        // Validar mes (posiciones 6-7, debe ser 01-12)
        if (!int.TryParse(clave.Substring(5, 2), out int mes) || mes < 1 || mes > 12)
            return false;

        // Validar situación (posición 42, debe ser 1, 2 o 3)
        if (!int.TryParse(clave.Substring(41, 1), out int situacion) || situacion < 1 || situacion > 3)
            return false;

        return true;
    }

    /// <summary>
    /// Genera un código de seguridad aleatorio de 8 dígitos
    /// Según especificación de Hacienda, debe ser un número aleatorio de 8 dígitos
    /// </summary>
    public string GenerarCodigoSeguridad()
    {
        // Generar 8 dígitos aleatorios uno por uno para asegurar 8 dígitos exactos
        var codigo = string.Empty;
        for (int i = 0; i < 8; i++)
        {
            codigo += _random.Next(0, 10).ToString();
        }
        return codigo;
    }
}
