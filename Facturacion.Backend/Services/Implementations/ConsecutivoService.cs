using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio para gestión de consecutivos de documentos electrónicos
/// Implementa la lógica de generación de consecutivos según formato de Hacienda CR
/// </summary>
public class ConsecutivoService : IConsecutivoService
{
    private readonly DataContext _context;
    private readonly ILogger<ConsecutivoService> _logger;

    public ConsecutivoService(
        DataContext context,
        ILogger<ConsecutivoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Genera y asigna el siguiente número consecutivo de forma atómica
    /// Formato SQL equivalente:
    /// SELECT @Consecutivo = (C.IdTienda + C.Servidor + C.TipoDocumento + RIGHT('0000000000' + convert(varchar(max),(C.Consecutivo +1)),10))
    /// </summary>
    public async Task<string> GenerarYAsignarConsecutivoAsync(Guid terminalId, DocumentoTipo tipoDocumento, Ambiente ambiente)
    {
        var codigoTipoDoc = ObtenerCodigoTipoDocumento(tipoDocumento);

        // Usar una transacción para garantizar atomicidad
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Obtener terminal con sucursal y empresa (con bloqueo para evitar concurrencia)
            var terminal = await _context.Set<Terminal>()
                .Include(t => t.Sucursal)
                    .ThenInclude(s => s!.Empresa)
                .FirstOrDefaultAsync(t => t.Id == terminalId && !t.IsDeleted && t.Activo);

            if (terminal == null)
            {
                throw new InvalidOperationException(
                    $"Terminal con ID {terminalId} no encontrado o está inactivo");
            }

            if (terminal.Sucursal == null)
            {
                throw new InvalidOperationException(
                    $"Terminal {terminal.Nombre} no tiene una sucursal asignada");
            }

            // 2. Buscar el consecutivo activo para este terminal, tipo de documento Y AMBIENTE
            // Usar FromSqlRaw para bloqueo a nivel de fila (evitar duplicados en concurrencia)
            var consecutivoEntity = await _context.Consecutivos
                .FromSqlRaw(@"
                    SELECT * FROM Consecutivos WITH (UPDLOCK, ROWLOCK)
                    WHERE TerminalId = {0}
                    AND TipoDocumento = {1}
                    AND Ambiente = {2}
                    AND Activo = 1
                    AND IsDeleted = 0",
                    terminalId, codigoTipoDoc, (int)ambiente)
                .FirstOrDefaultAsync();

            if (consecutivoEntity == null)
            {
                var ambienteNombre = ambiente == Ambiente.Pruebas ? "Pruebas (stag)" : "Producción (prod)";
                throw new InvalidOperationException(
                    $"No hay consecutivo activo configurado para el terminal '{terminal.Nombre}', " +
                    $"tipo de documento '{tipoDocumento}' ({codigoTipoDoc}) y ambiente '{ambienteNombre}'");
            }

            // 3. Verificar que no haya alcanzado el límite
            if (consecutivoEntity.NumeroActual >= consecutivoEntity.NumeroFin)
            {
                throw new InvalidOperationException(
                    $"El consecutivo del terminal '{terminal.Nombre}' ha alcanzado el límite. " +
                    $"Actual: {consecutivoEntity.NumeroActual}, Límite: {consecutivoEntity.NumeroFin}. " +
                    $"Por favor configure un nuevo rango de consecutivos.");
            }

            // 4. Incrementar el consecutivo de forma atómica
            consecutivoEntity.NumeroActual++;
            var numeroAsignado = consecutivoEntity.NumeroActual;
            consecutivoEntity.FechaModificacion = FechaCostaRicaHelper.Ahora;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 5. Formatear el número consecutivo según especificación de Hacienda
            // IdTienda (3) + Servidor (5) + TipoDocumento (2) + Consecutivo (10)
            var consecutivoFormateado = FormatearConsecutivo(
                terminal.Sucursal.Codigo,
                terminal.Codigo,
                codigoTipoDoc,
                numeroAsignado);

            var ambienteLog = ambiente == Ambiente.Pruebas ? "Pruebas" : "Producción";
            _logger.LogInformation(
                "Consecutivo generado exitosamente: {Consecutivo} para Terminal: {Terminal}, Tipo: {Tipo}, Ambiente: {Ambiente}",
                consecutivoFormateado, terminal.Nombre, tipoDocumento, ambienteLog);

            return consecutivoFormateado;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,
                "Error al generar consecutivo para Terminal: {TerminalId}, Tipo: {TipoDocumento}",
                terminalId, tipoDocumento);
            throw;
        }
    }

    /// <summary>
    /// Obtiene el siguiente número consecutivo sin incrementarlo (solo consulta)
    /// </summary>
    public async Task<string> ObtenerSiguienteConsecutivoAsync(Guid terminalId, DocumentoTipo tipoDocumento, Ambiente ambiente)
    {
        var codigoTipoDoc = ObtenerCodigoTipoDocumento(tipoDocumento);

        var terminal = await _context.Set<Terminal>()
            .Include(t => t.Sucursal)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == terminalId && !t.IsDeleted && t.Activo);

        if (terminal == null)
        {
            throw new InvalidOperationException(
                $"Terminal con ID {terminalId} no encontrado o está inactivo");
        }

        if (terminal.Sucursal == null)
        {
            throw new InvalidOperationException(
                $"Terminal {terminal.Nombre} no tiene una sucursal asignada");
        }

        var consecutivoEntity = await _context.Consecutivos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TerminalId == terminalId &&
                                     c.TipoDocumento == codigoTipoDoc &&
                                     c.Ambiente == ambiente &&
                                     c.Activo &&
                                     !c.IsDeleted);

        if (consecutivoEntity == null)
        {
            var ambienteNombre = ambiente == Ambiente.Pruebas ? "Pruebas (stag)" : "Producción (prod)";
            throw new InvalidOperationException(
                $"No hay consecutivo activo configurado para el terminal '{terminal.Nombre}', " +
                $"tipo de documento '{tipoDocumento}' ({codigoTipoDoc}) y ambiente '{ambienteNombre}'");
        }

        var siguienteNumero = consecutivoEntity.NumeroActual + 1;

        if (siguienteNumero > consecutivoEntity.NumeroFin)
        {
            throw new InvalidOperationException(
                $"El terminal '{terminal.Nombre}' alcanzará el límite de consecutivos. " +
                $"Siguiente: {siguienteNumero}, Límite: {consecutivoEntity.NumeroFin}");
        }

        return FormatearConsecutivo(
            terminal.Sucursal.Codigo,
            terminal.Codigo,
            codigoTipoDoc,
            siguienteNumero);
    }

    /// <summary>
    /// Convierte el número consecutivo con guiones al formato numérico de 20 dígitos
    /// </summary>
    public string ConvertirAFormatoHacienda(string consecutivoConGuiones)
    {
        if (string.IsNullOrWhiteSpace(consecutivoConGuiones))
        {
            throw new ArgumentException("El consecutivo no puede estar vacío", nameof(consecutivoConGuiones));
        }

        // Remover guiones: XXX-YYYYY-ZZ-AAAAAAAAAA -> XXXYYYYYZZAAAAAAAAAA
        var consecutivoSinGuiones = consecutivoConGuiones.Replace("-", "");

        if (consecutivoSinGuiones.Length != 20)
        {
            throw new ArgumentException(
                $"El consecutivo debe tener 20 dígitos después de remover guiones. " +
                $"Recibido: {consecutivoSinGuiones} ({consecutivoSinGuiones.Length} dígitos)",
                nameof(consecutivoConGuiones));
        }

        if (!long.TryParse(consecutivoSinGuiones, out _))
        {
            throw new ArgumentException(
                "El consecutivo debe contener solo dígitos numéricos",
                nameof(consecutivoConGuiones));
        }

        return consecutivoSinGuiones;
    }

    /// <summary>
    /// Obtiene el código de tipo de documento en formato de 2 dígitos
    /// Mapeo según especificación:
    /// FE -> 01, ND -> 02, NC -> 03, TE -> 04, FEC -> 08, FEE -> 09
    /// </summary>
    public string ObtenerCodigoTipoDocumento(DocumentoTipo tipoDocumento)
    {
        // El enum ya está numerado correctamente según Hacienda
        // FacturaElectronica = 1 -> "01"
        // NotaDebitoElectronica = 2 -> "02"
        // NotaCreditoElectronica = 3 -> "03"
        // TiqueteElectronico = 4 -> "04"
        // etc.
        return ((int)tipoDocumento).ToString("D2");
    }

    /// <summary>
    /// Verifica si un terminal tiene consecutivos disponibles
    /// </summary>
    public async Task<bool> TieneConsecutivosDisponiblesAsync(Guid terminalId, DocumentoTipo tipoDocumento, Ambiente ambiente)
    {
        var codigoTipoDoc = ObtenerCodigoTipoDocumento(tipoDocumento);

        var consecutivoEntity = await _context.Consecutivos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TerminalId == terminalId &&
                                     c.TipoDocumento == codigoTipoDoc &&
                                     c.Ambiente == ambiente &&
                                     c.Activo &&
                                     !c.IsDeleted);

        if (consecutivoEntity == null)
        {
            return false; // No existe consecutivo configurado
        }

        return consecutivoEntity.NumeroActual < consecutivoEntity.NumeroFin;
    }

    /// <summary>
    /// Formatea el consecutivo según el formato de Hacienda
    /// Formato con guiones: XXX-YYYYY-ZZ-AAAAAAAAAA
    /// Formato sin guiones (para XML): XXXYYYYYZZAAAAAAAAAA (20 dígitos)
    /// </summary>
    private string FormatearConsecutivo(
        string codigoSucursal,
        string codigoTerminal,
        string tipoDocumento,
        long numeroConsecutivo)
    {
        // Asegurar formato correcto con ceros a la izquierda
        // IdTienda: 3 dígitos
        var sucursal = FormatearConCeros(codigoSucursal, 3);

        // Servidor: 5 dígitos
        var terminal = FormatearConCeros(codigoTerminal, 5);

        // TipoDocumento: ya viene en formato 2 dígitos
        var tipo = tipoDocumento.PadLeft(2, '0').Substring(0, 2);

        // Consecutivo: 10 dígitos (equivalente a RIGHT('0000000000' + convert(varchar(max), Consecutivo), 10))
        var consecutivo = numeroConsecutivo.ToString().PadLeft(10, '0');

        // Formato con guiones para BD y visualización: XXX-YYYYY-ZZ-AAAAAAAAAA
        return $"{sucursal}{terminal}{tipo}{consecutivo}";
    }

    /// <summary>
    /// Formatea un código con ceros a la izquierda
    /// Si el código es numérico, rellena con ceros
    /// Si es alfanumérico, rellena con espacios
    /// </summary>
    private string FormatearConCeros(string codigo, int longitudRequerida)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return new string('0', longitudRequerida);
        }

        // Si es numérico, rellenar con ceros a la izquierda
        if (long.TryParse(codigo, out var numeroSucursal))
        {
            return numeroSucursal.ToString().PadLeft(longitudRequerida, '0');
        }

        // Si es alfanumérico, tomar solo los dígitos necesarios o rellenar
        if (codigo.Length >= longitudRequerida)
        {
            return codigo.Substring(0, longitudRequerida);
        }

        return codigo.PadLeft(longitudRequerida, '0');
    }
}
