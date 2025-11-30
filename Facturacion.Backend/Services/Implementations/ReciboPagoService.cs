using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio para gestión de Recibos Electrónicos de Pago (REP)
/// </summary>
public class ReciboPagoService : IReciboPagoService
{
    private readonly DataContext _context;
    private readonly IReciboPagoRepository _reciboRepository;
    private readonly IClaveGeneradorService _claveGenerador;
    private readonly IXmlGeneradorService _xmlGenerador;
    private readonly IFirmaDigitalService _firmaDigital;
    private readonly IHaciendaApiService _haciendaApi;
    private readonly ILogger<ReciboPagoService> _logger;

    public ReciboPagoService(
        DataContext context,
        IReciboPagoRepository reciboRepository,
        IClaveGeneradorService claveGenerador,
        IXmlGeneradorService xmlGenerador,
        IFirmaDigitalService firmaDigital,
        IHaciendaApiService haciendaApi,
        ILogger<ReciboPagoService> logger)
    {
        _context = context;
        _reciboRepository = reciboRepository;
        _claveGenerador = claveGenerador;
        _xmlGenerador = xmlGenerador;
        _firmaDigital = firmaDigital;
        _haciendaApi = haciendaApi;
        _logger = logger;
    }

    public async Task<ResultadoREP> GenerarREPAsync(ReciboPagoDTO dto, Guid empresaId, Guid terminalId, string userId)
    {
        var resultado = new ResultadoREP
        {
            Exitoso = false,
            Mensaje = "Procesando recibo de pago...",
            Errores = new List<string>()
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Iniciando generación de REP para documento {DocumentoId}", dto.DocumentoOriginalId);

            // 1. Obtener y validar documento original
            var documentoOriginal = await _context.Documentos
                .Include(d => d.Empresa)
                .Include(d => d.Sucursal)
                .Include(d => d.Terminal)
                .Include(d => d.Cliente)
                .FirstOrDefaultAsync(d => d.Id == dto.DocumentoOriginalId && d.EmpresaId == empresaId);

            if (documentoOriginal == null)
            {
                resultado.Errores.Add("No se encontró el documento original");
                resultado.Mensaje = "Documento original no existe";
                return resultado;
            }

            // 2. Validar que sea documento a crédito
            if (documentoOriginal.CondicionVenta != "02")
            {
                resultado.Errores.Add("El documento original no es una venta a crédito (CondicionVenta debe ser 02)");
                resultado.Mensaje = "Solo se pueden generar REPs para ventas a crédito";
                return resultado;
            }

            // 3. Validar estado del documento original
            if (documentoOriginal.Estado == EstadoDocumento.Borrador || documentoOriginal.Estado == EstadoDocumento.Rechazado)
            {
                resultado.Errores.Add($"El documento original está en estado {documentoOriginal.Estado}");
                resultado.Mensaje = "El documento original debe estar Aceptado por Hacienda";
                return resultado;
            }

            // 4. Calcular saldo pendiente
            var totalPagado = await _reciboRepository.GetTotalPagadoAsync(dto.DocumentoOriginalId);
            var saldoPendiente = documentoOriginal.TotalVenta - totalPagado;

            if (saldoPendiente <= 0)
            {
                resultado.Errores.Add("El documento ya está totalmente pagado");
                resultado.Mensaje = "No hay saldo pendiente";
                resultado.SaldoPendiente = 0;
                return resultado;
            }

            // 5. Validar que el monto no exceda el saldo
            if (dto.MontoPagado > saldoPendiente)
            {
                resultado.Errores.Add($"El monto pagado ({dto.MontoPagado:N2}) excede el saldo pendiente ({saldoPendiente:N2})");
                resultado.Mensaje = "Monto inválido";
                resultado.SaldoPendiente = saldoPendiente;
                return resultado;
            }

            // 6. Validar moneda
            if (dto.Moneda != documentoOriginal.Moneda)
            {
                resultado.Errores.Add($"La moneda del pago ({dto.Moneda}) debe coincidir con la del documento original ({documentoOriginal.Moneda})");
                resultado.Mensaje = "Moneda inválida";
                return resultado;
            }

            // 7. Validar medios de pago
            var totalMediosPago = dto.MediosPago.Sum(m => m.Monto);
            if (Math.Abs(totalMediosPago - dto.MontoPagado) > 0.01m)
            {
                resultado.Errores.Add($"La suma de los medios de pago ({totalMediosPago:N2}) no coincide con el monto pagado ({dto.MontoPagado:N2})");
                resultado.Mensaje = "Error en medios de pago";
                return resultado;
            }

            // 8. Obtener terminal
            var terminal = await _context.Terminales
                .Include(t => t.Sucursal)
                .FirstOrDefaultAsync(t => t.Id == terminalId);

            if (terminal == null)
            {
                resultado.Errores.Add("No se encontró el terminal");
                resultado.Mensaje = "Terminal no existe";
                return resultado;
            }

            // 9. Generar consecutivo para REP
            var consecutivo = await GenerarConsecutivoREPAsync(terminal);

            // 10. Crear documento REP
            var documentoREP = new Documento
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                SucursalId = terminal.SucursalId,
                TerminalId = terminalId,
                TipoDocumento = DocumentoTipo.ReciboElectronicoPago,
                Estado = EstadoDocumento.Borrador,
                NumeroConsecutivo = consecutivo,
                FechaEmision = dto.FechaPago,
                ActividadEconomica = documentoOriginal.ActividadEconomica,

                // Copiar datos del receptor desde documento original
                ClienteId = documentoOriginal.ClienteId,
                ReceptorTipoIdentificacion = documentoOriginal.ReceptorTipoIdentificacion,
                ReceptorNumeroIdentificacion = documentoOriginal.ReceptorNumeroIdentificacion,
                ReceptorNombre = documentoOriginal.ReceptorNombre,
                ReceptorNombreComercial = documentoOriginal.ReceptorNombreComercial,
                ReceptorActividadEconomica = documentoOriginal.ReceptorActividadEconomica,
                ReceptorProvincia = documentoOriginal.ReceptorProvincia,
                ReceptorCanton = documentoOriginal.ReceptorCanton,
                ReceptorDistrito = documentoOriginal.ReceptorDistrito,
                ReceptorBarrio = documentoOriginal.ReceptorBarrio,
                ReceptorOtrasSenas = documentoOriginal.ReceptorOtrasSenas,
                ReceptorEmails = documentoOriginal.ReceptorEmails,
                ReceptorTelefono = documentoOriginal.ReceptorTelefono,

                // Condición y medio de pago (se usa el primero como principal)
                CondicionVenta = "01", // Contado (el REP siempre es contado)
                MedioPago = dto.MediosPago.First().CodigoMedioPago,

                // Moneda
                Moneda = dto.Moneda,
                TipoCambio = dto.TipoCambio,

                // Totales (para REP, TotalVenta es el monto pagado)
                Subtotal = dto.MontoPagado,
                TotalVenta = dto.MontoPagado,
                TotalImpuestos = 0,
                TotalDescuentos = 0,
                TotalOtrosCargos = 0,
                TotalGravado = 0,
                TotalExento = dto.MontoPagado,
                TotalExonerado = 0,
                TotalServiciosGravados = 0,
                TotalServiciosExentos = 0,
                TotalServiciosExonerados = 0,
                TotalMercanciasGravadas = 0,
                TotalMercanciasExentas = 0,
                TotalMercanciasExoneradas = 0,

                // Observaciones
                Observaciones = dto.Observaciones,

                // Audit
                FechaCreacion = DateTime.Now,
                UsuarioCreacionId = userId,
                IsDeleted = false
            };

            // 11. Generar clave para REP
            documentoREP.Clave = await _claveGenerador.GenerarClaveAsync(documentoREP, 1); // 1=Normal

            await _context.Documentos.AddAsync(documentoREP);
            await _context.SaveChangesAsync();

            // 12. Crear registro ReciboPago
            var nuevoSaldo = saldoPendiente - dto.MontoPagado;

            var reciboPago = new ReciboPago
            {
                Id = Guid.NewGuid(),
                DocumentoId = documentoREP.Id,
                DocumentoOriginalId = documentoOriginal.Id,
                ClaveDocumentoOriginal = documentoOriginal.Clave,
                NumeroConsecutivoOriginal = documentoOriginal.NumeroConsecutivo,
                TipoDocumentoOriginal = ((int)documentoOriginal.TipoDocumento).ToString("D2"),
                FechaPago = dto.FechaPago,
                MontoPagado = dto.MontoPagado,
                SaldoPendiente = nuevoSaldo,
                Moneda = dto.Moneda,
                TipoCambio = dto.TipoCambio,
                Observaciones = dto.Observaciones,
                FechaCreacion = DateTime.Now,
                UsuarioCreacionId = userId
            };

            await _reciboRepository.AddAsync(reciboPago);

            // 13. Crear DocumentoReferencia (enlace al documento original)
            var referencia = new DocumentoReferencia
            {
                Id = Guid.NewGuid(),
                DocumentoId = documentoREP.Id,
                DocumentoReferenciadoId = documentoOriginal.Id,
                TipoDocumentoReferenciado = ((int)documentoOriginal.TipoDocumento).ToString("D2"),
                NumeroDocumentoReferenciado = documentoOriginal.NumeroConsecutivo,
                FechaEmisionDocumentoReferenciado = documentoOriginal.FechaEmision,
                ClaveDocumentoReferenciado = documentoOriginal.Clave,
                CodigoReferencia = TipoReferenciaDocumento.ReferenciaOtroDocumento, // 04
                RazonReferencia = $"Pago parcial por {dto.MontoPagado:N2} - Saldo restante: {nuevoSaldo:N2}",
                FechaCreacion = DateTime.Now,
                UsuarioCreacionId = userId
            };

            await _context.DocumentoReferencias.AddAsync(referencia);
            await _context.SaveChangesAsync();

            // 14. Crear DocumentoMedioPago (todos los medios de pago)
            foreach (var medioPago in dto.MediosPago)
            {
                var docMedioPago = new DocumentoMedioPago
                {
                    Id = Guid.NewGuid(),
                    DocumentoId = documentoREP.Id,
                    CodigoMedioPago = medioPago.CodigoMedioPago,
                    Monto = medioPago.Monto,
                    NumeroReferencia = medioPago.NumeroReferencia,
                    Descripcion = medioPago.Descripcion,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacionId = userId
                };

                await _context.DocumentoMediosPago.AddAsync(docMedioPago);
            }

            await _context.SaveChangesAsync();

            // 15. Generar XML del REP
            var xml = await _xmlGenerador.GenerarREPAsync(documentoREP, reciboPago);
            documentoREP.XmlGenerado = xml;

            // 16. Firmar XML
            var empresa = documentoOriginal.Empresa;
            if (empresa.CertificadoDigital == null || string.IsNullOrWhiteSpace(empresa.PinCertificado))
            {
                resultado.Errores.Add("La empresa no tiene configurado el certificado digital");
                resultado.Mensaje = "Falta configuración de certificado";
                await transaction.RollbackAsync();
                return resultado;
            }

            try
            {
                var certificado = await _firmaDigital.ObtenerCertificadoAsync(empresa.Id);
                var xmlFirmado = await _firmaDigital.FirmarXmlAsync(xml, certificado);
                documentoREP.XmlFirmado = xmlFirmado;
                documentoREP.FechaFirma = DateTime.Now;
                documentoREP.Estado = EstadoDocumento.Pendiente;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al firmar REP");
                resultado.Errores.Add($"Error al firmar XML: {ex.Message}");
                resultado.Mensaje = "Error al firmar el REP";
                await transaction.RollbackAsync();
                return resultado;
            }

            // 17. Enviar a Hacienda
            try
            {
                string ambiente = empresa.Ambiente == Ambiente.Produccion ? "prod" : "stag";

                var respuestaHacienda = await _haciendaApi.EnviarDocumentoAsync(
                    documentoREP.Clave,
                    documentoREP.XmlFirmado!,
                    empresa.UsuarioHacienda!,
                    empresa.ClaveHacienda!,
                    ambiente
                );

                documentoREP.FechaEnvioHacienda = DateTime.Now;
                documentoREP.FechaRespuestaHacienda = DateTime.Now;
                documentoREP.XmlRespuestaHacienda = respuestaHacienda.RespuestaXml;

                if (respuestaHacienda.IndEstado.ToLower() == "aceptado")
                {
                    documentoREP.Estado = EstadoDocumento.Aceptado;
                    documentoREP.MensajeHacienda = "Documento aceptado por Hacienda";
                }
                else if (respuestaHacienda.IndEstado.ToLower() == "rechazado")
                {
                    documentoREP.Estado = EstadoDocumento.Rechazado;
                    var mensajes = string.Join("; ", respuestaHacienda.Mensajes.Select(m => m.Mensaje));
                    documentoREP.MensajeHacienda = $"Rechazado: {mensajes}";
                    resultado.Errores.Add($"Hacienda rechazó el documento: {mensajes}");
                }
                else // procesando
                {
                    documentoREP.Estado = EstadoDocumento.Procesando;
                    documentoREP.MensajeHacienda = "Documento en procesamiento";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar REP a Hacienda");
                documentoREP.Estado = EstadoDocumento.Rechazado;
                documentoREP.MensajeHacienda = ex.Message;
                await _context.SaveChangesAsync();

                resultado.Errores.Add($"Error al comunicarse con Hacienda: {ex.Message}");
            }

            await transaction.CommitAsync();

            // 18. Preparar resultado
            resultado.Exitoso = documentoREP.Estado == EstadoDocumento.Aceptado;
            resultado.Mensaje = documentoREP.Estado == EstadoDocumento.Aceptado
                ? "REP generado y aceptado por Hacienda exitosamente"
                : $"REP generado pero con estado: {documentoREP.Estado}";
            resultado.ReciboId = reciboPago.Id;
            resultado.DocumentoREPId = documentoREP.Id;
            resultado.ClaveREP = documentoREP.Clave;
            resultado.MontoPagado = dto.MontoPagado;
            resultado.SaldoPendiente = nuevoSaldo;
            resultado.Estado = documentoREP.Estado.ToString();

            _logger.LogInformation("REP generado exitosamente: {ClaveREP}", documentoREP.Clave);

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar REP");
            await transaction.RollbackAsync();

            resultado.Exitoso = false;
            resultado.Mensaje = "Error al generar el REP";
            resultado.Errores.Add(ex.Message);
            return resultado;
        }
    }

    public async Task<List<DocumentoPendientePagoDTO>> GetDocumentosPendientesPagoAsync(
        Guid empresaId,
        Guid? clienteId = null,
        bool soloVencidos = false)
    {
        var query = _context.Documentos
            .Include(d => d.Cliente)
            .Where(d => d.EmpresaId == empresaId
                     && d.CondicionVenta == "02" // Solo crédito
                     && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Procesando)
                     && !d.IsDeleted);

        if (clienteId.HasValue)
        {
            query = query.Where(d => d.ClienteId == clienteId.Value);
        }

        var documentos = await query
            .OrderBy(d => d.FechaEmision)
            .ToListAsync();

        var resultado = new List<DocumentoPendientePagoDTO>();

        foreach (var doc in documentos)
        {
            var totalPagado = await _reciboRepository.GetTotalPagadoAsync(doc.Id);
            var saldoPendiente = doc.TotalVenta - totalPagado;

            if (saldoPendiente <= 0)
                continue; // Ya está totalmente pagado

            var fechaVencimiento = doc.PlazoCreditoDias.HasValue
                ? doc.FechaEmision.AddDays(doc.PlazoCreditoDias.Value)
                : (DateTime?)null;

            var diasVencido = fechaVencimiento.HasValue && fechaVencimiento.Value < DateTime.Today
                ? (DateTime.Today - fechaVencimiento.Value).Days
                : 0;

            if (soloVencidos && diasVencido <= 0)
                continue;

            var cantidadPagos = await _context.RecibosPago
                .CountAsync(r => r.DocumentoOriginalId == doc.Id);

            resultado.Add(new DocumentoPendientePagoDTO
            {
                DocumentoId = doc.Id,
                Clave = doc.Clave,
                NumeroConsecutivo = doc.NumeroConsecutivo,
                TipoDocumento = doc.TipoDocumento.ToString(),
                ClienteNombre = doc.ReceptorNombre ?? "Desconocido",
                ClienteIdentificacion = doc.ReceptorNumeroIdentificacion,
                FechaEmision = doc.FechaEmision,
                FechaVencimiento = fechaVencimiento,
                TotalVenta = doc.TotalVenta,
                TotalPagado = totalPagado,
                SaldoPendiente = saldoPendiente,
                DiasVencido = diasVencido,
                PlazoDias = doc.PlazoCreditoDias ?? 0,
                Moneda = doc.Moneda,
                Estado = doc.Estado.ToString(),
                CantidadPagos = cantidadPagos
            });
        }

        return resultado;
    }

    public async Task<DetallePagosDocumentoDTO> GetDetallePagosDocumentoAsync(Guid documentoId)
    {
        var documento = await _context.Documentos
            .FirstOrDefaultAsync(d => d.Id == documentoId);

        if (documento == null)
            throw new Exception("Documento no encontrado");

        var recibos = await _reciboRepository.GetByDocumentoOriginalAsync(documentoId);
        var totalPagado = recibos.Sum(r => r.MontoPagado);

        var detalle = new DetallePagosDocumentoDTO
        {
            DocumentoId = documentoId,
            NumeroConsecutivo = documento.NumeroConsecutivo,
            TotalVenta = documento.TotalVenta,
            TotalPagado = totalPagado,
            SaldoPendiente = documento.TotalVenta - totalPagado,
            Pagos = new List<ReciboPagoResumenDTO>()
        };

        foreach (var recibo in recibos)
        {
            var mediosPago = await _context.DocumentoMediosPago
                .Where(m => m.DocumentoId == recibo.DocumentoId)
                .ToListAsync();

            var resumen = new ReciboPagoResumenDTO
            {
                ReciboId = recibo.Id,
                DocumentoREPId = recibo.DocumentoId,
                ClaveREP = recibo.Documento?.Clave ?? "",
                NumeroConsecutivoREP = recibo.Documento?.NumeroConsecutivo ?? "",
                FechaPago = recibo.FechaPago,
                MontoPagado = recibo.MontoPagado,
                SaldoRestante = recibo.SaldoPendiente,
                Estado = recibo.Documento?.Estado.ToString() ?? "Desconocido",
                MediosPago = mediosPago.Select(m => $"{m.CodigoMedioPago}: {m.Monto:N2}").ToList(),
                Observaciones = recibo.Observaciones
            };

            detalle.Pagos.Add(resumen);
        }

        return detalle;
    }

    public async Task<decimal> CalcularSaldoPendienteAsync(Guid documentoId)
    {
        return await _reciboRepository.CalcularSaldoPendienteAsync(documentoId);
    }

    public async Task<(bool esValido, string mensaje)> ValidarPagoAsync(Guid documentoId, decimal montoPago)
    {
        var documento = await _context.Documentos
            .FirstOrDefaultAsync(d => d.Id == documentoId);

        if (documento == null)
            return (false, "Documento no encontrado");

        if (documento.CondicionVenta != "02")
            return (false, "El documento no es una venta a crédito");

        if (documento.Estado != EstadoDocumento.Aceptado && documento.Estado != EstadoDocumento.Procesando)
            return (false, $"El documento está en estado {documento.Estado}");

        var saldoPendiente = await _reciboRepository.CalcularSaldoPendienteAsync(documentoId);

        if (saldoPendiente <= 0)
            return (false, "El documento ya está totalmente pagado");

        if (montoPago <= 0)
            return (false, "El monto del pago debe ser mayor a cero");

        if (montoPago > saldoPendiente)
            return (false, $"El monto ({montoPago:N2}) excede el saldo pendiente ({saldoPendiente:N2})");

        return (true, "Pago válido");
    }

    public async Task<List<ReciboPago>> GetRecibosPorDocumentoAsync(Guid documentoId)
    {
        var recibos = await _reciboRepository.GetByDocumentoOriginalAsync(documentoId);
        return recibos.ToList();
    }

    public async Task<ReciboPago?> GetReciboAsync(Guid reciboId)
    {
        return await _reciboRepository.GetByIdAsync(reciboId);
    }

    public async Task<(bool exitoso, string mensaje)> AnularREPAsync(Guid reciboId, string userId, string razon)
    {
        var recibo = await _reciboRepository.GetByIdAsync(reciboId);
        if (recibo == null)
            return (false, "Recibo no encontrado");

        var documentoREP = await _context.Documentos
            .FirstOrDefaultAsync(d => d.Id == recibo.DocumentoId);

        if (documentoREP == null)
            return (false, "Documento REP no encontrado");

        if (documentoREP.Estado == EstadoDocumento.Anulado)
            return (false, "El REP ya está anulado");

        // Marcar como anulado
        documentoREP.Estado = EstadoDocumento.Anulado;
        documentoREP.Observaciones = $"ANULADO: {razon}";
        documentoREP.FechaModificacion = DateTime.Now;
        documentoREP.UsuarioModificacionId = userId;

        await _context.SaveChangesAsync();

        _logger.LogWarning("REP anulado: {ClaveREP} - Razón: {Razon}", documentoREP.Clave, razon);

        return (true, "REP anulado exitosamente");
    }

    public async Task<EstadisticasCobranzaDTO> GetEstadisticasCobranzaAsync(Guid empresaId)
    {
        var documentosPendientes = await GetDocumentosPendientesPagoAsync(empresaId);

        var stats = new EstadisticasCobranzaDTO
        {
            TotalDocumentosPendientes = documentosPendientes.Count,
            TotalPorCobrar = documentosPendientes.Sum(d => d.SaldoPendiente),
            DocumentosVencidos = documentosPendientes.Count(d => d.DiasVencido > 0),
            MontoVencido = documentosPendientes.Where(d => d.DiasVencido > 0).Sum(d => d.SaldoPendiente),
            DocumentosAlDia = documentosPendientes.Count(d => d.DiasVencido <= 0),
            MontoAlDia = documentosPendientes.Where(d => d.DiasVencido <= 0).Sum(d => d.SaldoPendiente),
            PromedioVencimiento = documentosPendientes.Any()
                ? (decimal)documentosPendientes.Average(d => d.DiasVencido)
                : 0
        };

        // Top clientes deudores
        var clientesDeudores = documentosPendientes
            .GroupBy(d => new { d.ClienteNombre, d.ClienteIdentificacion })
            .Select(g => new CobranzaPorClienteDTO
            {
                ClienteNombre = g.Key.ClienteNombre,
                ClienteIdentificacion = g.Key.ClienteIdentificacion,
                CantidadDocumentos = g.Count(),
                TotalPendiente = g.Sum(d => d.SaldoPendiente),
                DiasPromedioVencido = (int)g.Average(d => d.DiasVencido)
            })
            .OrderByDescending(c => c.TotalPendiente)
            .Take(10)
            .ToList();

        stats.TopClientesDeudores = clientesDeudores;

        return stats;
    }

    /// <summary>
    /// Genera el número consecutivo para un REP
    /// </summary>
    private async Task<string> GenerarConsecutivoREPAsync(Terminal terminal)
    {
        const string tipoDocumentoREP = "10";

        // Buscar consecutivo activo para tipo REP
        var consecutivoEntity = await _context.Set<Consecutivo>()
            .FirstOrDefaultAsync(c => c.TerminalId == terminal.Id &&
                                     c.TipoDocumento == tipoDocumentoREP &&
                                     c.Activo &&
                                     !c.IsDeleted);

        if (consecutivoEntity == null)
        {
            throw new InvalidOperationException(
                $"No hay consecutivo activo configurado para REP en el terminal {terminal.Nombre}");
        }

        // Incrementar contador del consecutivo para tipo REP (10)
        consecutivoEntity.NumeroActual++;
        consecutivoEntity.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Formato: XXX-YYYYY-ZZ-AAAAAAAAAA
        // XXX = Código sucursal (3 dígitos)
        // YYYYY = Código terminal (5 dígitos)
        // ZZ = Tipo documento (10 para REP)
        // AAAAAAAAAA = Consecutivo (10 dígitos)

        var codigoSucursal = terminal.Sucursal!.Codigo.PadLeft(3, '0');
        var codigoTerminal = terminal.Codigo.PadLeft(5, '0');
        var consecutivo = consecutivoEntity.NumeroActual.ToString().PadLeft(10, '0');

        return $"{codigoSucursal}-{codigoTerminal}-{tipoDocumentoREP}-{consecutivo}";
    }
}
