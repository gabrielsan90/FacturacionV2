using ClosedXML.Excel;
using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio para importación de datos desde archivos Excel
/// Utiliza ClosedXML (MIT License - completamente gratuito)
/// </summary>
public class ExcelImportService : IExcelImportService
{
    private readonly DataContext _context;
    private readonly ILogger<ExcelImportService> _logger;

    public ExcelImportService(DataContext context, ILogger<ExcelImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Importar Clientes

    public async Task<ImportResultDTO> ImportarClientesAsync(Stream fileStream, Guid empresaId, string userId)
    {
        var result = new ImportResultDTO { WasSuccess = true };

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene hojas de trabajo";
                return result;
            }

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow < 2)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene datos para importar";
                return result;
            }

            result.TotalRows = lastRow - 1; // Excluye encabezado

            // Obtener identificaciones existentes para validar duplicados
            var identificacionesExistentes = await _context.Clientes
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
                .Select(c => c.NumeroIdentificacion)
                .ToListAsync();

            var clientesNuevos = new List<Cliente>();

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var cliente = ParseClienteRow(worksheet, row, empresaId, userId, identificacionesExistentes, result);
                    if (cliente != null)
                    {
                        clientesNuevos.Add(cliente);
                        identificacionesExistentes.Add(cliente.NumeroIdentificacion);
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportErrorDTO
                    {
                        Row = row,
                        Message = $"Error procesando fila: {ex.Message}"
                    });
                }
            }

            if (clientesNuevos.Any())
            {
                await _context.Clientes.AddRangeAsync(clientesNuevos);
                await _context.SaveChangesAsync();
            }

            result.Message = $"Importación completada: {result.SuccessCount} clientes importados, {result.ErrorCount} errores";
            _logger.LogInformation("Importación de clientes completada para empresa {EmpresaId}: {Success} exitosos, {Errors} errores",
                empresaId, result.SuccessCount, result.ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar clientes para empresa {EmpresaId}", empresaId);
            result.WasSuccess = false;
            result.Message = $"Error al procesar el archivo: {ex.Message}";
        }

        return result;
    }

    private Cliente? ParseClienteRow(IXLWorksheet worksheet, int row, Guid empresaId, string userId,
        List<string> identificacionesExistentes, ImportResultDTO result)
    {
        // Columnas esperadas:
        // A: TipoIdentificacion (1=Fisica, 2=Juridica, 3=DIMEX, 4=NITE, 5=Pasaporte, 6=Extranjera, 7=NoContribuyente)
        // B: NumeroIdentificacion (requerido)
        // C: Nombre (requerido)
        // D: NombreComercial
        // E: EmailPrincipal
        // F: TelefonoPrincipal
        // G: Provincia (1-7)
        // H: Canton
        // I: Distrito
        // J: OtrasSenas
        // K: DiasCredito
        // L: LimiteCredito

        var tipoIdStr = GetCellValue(worksheet, row, 1);
        var numeroId = GetCellValue(worksheet, row, 2);
        var nombre = GetCellValue(worksheet, row, 3);

        // Validar campos requeridos
        if (string.IsNullOrWhiteSpace(numeroId))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "NumeroIdentificacion",
                Message = "El número de identificación es requerido"
            });
            return null;
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "Nombre",
                Message = "El nombre es requerido"
            });
            return null;
        }

        // Validar duplicados
        if (identificacionesExistentes.Contains(numeroId))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "NumeroIdentificacion",
                Value = numeroId,
                Message = "Ya existe un cliente con este número de identificación"
            });
            return null;
        }

        // Parsear tipo de identificación
        var tipoIdentificacion = TipoIdentificacion.Fisica;
        if (!string.IsNullOrWhiteSpace(tipoIdStr) && int.TryParse(tipoIdStr, out int tipoIdVal))
        {
            if (Enum.IsDefined(typeof(TipoIdentificacion), tipoIdVal))
            {
                tipoIdentificacion = (TipoIdentificacion)tipoIdVal;
            }
        }

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            TipoIdentificacion = tipoIdentificacion,
            NumeroIdentificacion = numeroId,
            Nombre = nombre,
            NombreComercial = GetCellValue(worksheet, row, 4),
            EmailPrincipal = GetCellValue(worksheet, row, 5),
            TelefonoPrincipal = GetCellValue(worksheet, row, 6),
            Provincia = GetCellInt(worksheet, row, 7),
            Canton = GetCellInt(worksheet, row, 8),
            Distrito = GetCellInt(worksheet, row, 9),
            OtrasSenas = GetCellValue(worksheet, row, 10),
            DiasCredito = GetCellInt(worksheet, row, 11),
            LimiteCredito = GetCellDecimal(worksheet, row, 12),
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            FechaRegistro = DateTime.UtcNow,
            UsuarioCreacionId = userId,
            Moneda = TipoMoneda.CRC
        };

        return cliente;
    }

    #endregion

    #region Importar Productos

    public async Task<ImportResultDTO> ImportarProductosAsync(Stream fileStream, Guid empresaId, string userId)
    {
        var result = new ImportResultDTO { WasSuccess = true };

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene hojas de trabajo";
                return result;
            }

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow < 2)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene datos para importar";
                return result;
            }

            result.TotalRows = lastRow - 1;

            // Obtener códigos existentes para validar duplicados
            var codigosExistentes = await _context.Productos
                .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
                .Select(p => p.Codigo)
                .ToListAsync();

            // Obtener unidades de medida e impuestos válidos
            var unidadesMedida = await _context.UnidadesMedida.ToListAsync();
            var impuestos = await _context.Impuestos.ToListAsync();
            var categorias = await _context.Categorias
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
                .ToListAsync();

            var productosNuevos = new List<Producto>();

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var producto = ParseProductoRow(worksheet, row, empresaId, userId,
                        codigosExistentes, unidadesMedida, impuestos, categorias, result);
                    if (producto != null)
                    {
                        productosNuevos.Add(producto);
                        codigosExistentes.Add(producto.Codigo);
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportErrorDTO
                    {
                        Row = row,
                        Message = $"Error procesando fila: {ex.Message}"
                    });
                }
            }

            if (productosNuevos.Any())
            {
                await _context.Productos.AddRangeAsync(productosNuevos);
                await _context.SaveChangesAsync();
            }

            result.Message = $"Importación completada: {result.SuccessCount} productos importados, {result.ErrorCount} errores";
            _logger.LogInformation("Importación de productos completada para empresa {EmpresaId}: {Success} exitosos, {Errors} errores",
                empresaId, result.SuccessCount, result.ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar productos para empresa {EmpresaId}", empresaId);
            result.WasSuccess = false;
            result.Message = $"Error al procesar el archivo: {ex.Message}";
        }

        return result;
    }

    private Producto? ParseProductoRow(IXLWorksheet worksheet, int row, Guid empresaId, string userId,
        List<string> codigosExistentes, List<Facturacion.Shared.Entities.Catalogos.UnidadMedida> unidadesMedida,
        List<Facturacion.Shared.Entities.Catalogos.Impuesto> impuestos,
        List<Categoria> categorias, ImportResultDTO result)
    {
        // Columnas esperadas:
        // A: Tipo (0=Producto, 1=Servicio)
        // B: Codigo (requerido)
        // C: Nombre (requerido)
        // D: Descripcion
        // E: UnidadMedidaCodigo (ej: "Unid", "Kg", "Sp")
        // F: PrecioVenta (requerido)
        // G: Costo
        // H: ImpuestoCodigo (ej: "01", "02", "08")
        // I: CategoriaNombre
        // J: ControlarInventario (1=Si, 0=No)
        // K: StockMinimo
        // L: CabysId

        var tipoStr = GetCellValue(worksheet, row, 1);
        var codigo = GetCellValue(worksheet, row, 2);
        var nombre = GetCellValue(worksheet, row, 3);

        // Validar campos requeridos
        if (string.IsNullOrWhiteSpace(codigo))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "Codigo",
                Message = "El código es requerido"
            });
            return null;
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "Nombre",
                Message = "El nombre es requerido"
            });
            return null;
        }

        // Validar duplicados
        if (codigosExistentes.Contains(codigo))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "Codigo",
                Value = codigo,
                Message = "Ya existe un producto con este código"
            });
            return null;
        }

        // Parsear tipo de producto
        var tipo = TipoProducto.Producto;
        if (!string.IsNullOrWhiteSpace(tipoStr) && int.TryParse(tipoStr, out int tipoVal))
        {
            if (Enum.IsDefined(typeof(TipoProducto), tipoVal))
            {
                tipo = (TipoProducto)tipoVal;
            }
        }

        // Buscar unidad de medida (por código o descripción)
        var unidadMedidaCodigo = GetCellValue(worksheet, row, 5) ?? "Unid";
        var unidadMedida = unidadesMedida.FirstOrDefault(u =>
            u.Codigo.Equals(unidadMedidaCodigo, StringComparison.OrdinalIgnoreCase) ||
            u.Descripcion.Contains(unidadMedidaCodigo, StringComparison.OrdinalIgnoreCase));
        var unidadMedidaId = unidadMedida?.Id ?? 1; // Default: Unidad

        // Buscar impuesto (por código)
        var impuestoCodigo = GetCellValue(worksheet, row, 8) ?? "08";
        var impuesto = impuestos.FirstOrDefault(i =>
            i.Codigo.Equals(impuestoCodigo, StringComparison.OrdinalIgnoreCase));
        var impuestoId = impuesto?.Id ?? 8; // Default: IVA 13%

        // Buscar categoría (por nombre)
        Guid? categoriaId = null;
        var categoriaNombre = GetCellValue(worksheet, row, 9);
        if (!string.IsNullOrWhiteSpace(categoriaNombre))
        {
            var categoria = categorias.FirstOrDefault(c =>
                c.Nombre.Equals(categoriaNombre, StringComparison.OrdinalIgnoreCase));
            categoriaId = categoria?.Id;
        }

        var precioVenta = GetCellDecimal(worksheet, row, 6);
        if (precioVenta <= 0)
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "PrecioVenta",
                Value = precioVenta.ToString(),
                Message = "El precio de venta debe ser mayor a 0"
            });
            return null;
        }

        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Tipo = tipo,
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = GetCellValue(worksheet, row, 4),
            UnidadMedidaId = unidadMedidaId,
            PrecioVenta = precioVenta,
            Costo = GetCellDecimalNullable(worksheet, row, 7),
            ImpuestoId = impuestoId,
            CategoriaId = categoriaId,
            ControlarInventario = GetCellInt(worksheet, row, 10) == 1,
            StockMinimo = GetCellDecimalNullable(worksheet, row, 11),
            CabysId = GetCellIntNullable(worksheet, row, 12),
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacionId = userId
        };

        return producto;
    }

    #endregion

    #region Importar Proveedores

    public async Task<ImportResultDTO> ImportarProveedoresAsync(Stream fileStream, Guid empresaId, string userId)
    {
        var result = new ImportResultDTO { WasSuccess = true };

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene hojas de trabajo";
                return result;
            }

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow < 2)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene datos para importar";
                return result;
            }

            result.TotalRows = lastRow - 1;

            // Obtener identificaciones existentes para validar duplicados
            var identificacionesExistentes = await _context.Proveedores
                .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
                .Select(p => p.NumeroIdentificacion)
                .ToListAsync();

            var proveedoresNuevos = new List<Proveedor>();

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var proveedor = ParseProveedorRow(worksheet, row, empresaId, userId, identificacionesExistentes, result);
                    if (proveedor != null)
                    {
                        proveedoresNuevos.Add(proveedor);
                        identificacionesExistentes.Add(proveedor.NumeroIdentificacion);
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportErrorDTO
                    {
                        Row = row,
                        Message = $"Error procesando fila: {ex.Message}"
                    });
                }
            }

            if (proveedoresNuevos.Any())
            {
                await _context.Proveedores.AddRangeAsync(proveedoresNuevos);
                await _context.SaveChangesAsync();
            }

            result.Message = $"Importación completada: {result.SuccessCount} proveedores importados, {result.ErrorCount} errores";
            _logger.LogInformation("Importación de proveedores completada para empresa {EmpresaId}: {Success} exitosos, {Errors} errores",
                empresaId, result.SuccessCount, result.ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar proveedores para empresa {EmpresaId}", empresaId);
            result.WasSuccess = false;
            result.Message = $"Error al procesar el archivo: {ex.Message}";
        }

        return result;
    }

    private Proveedor? ParseProveedorRow(IXLWorksheet worksheet, int row, Guid empresaId, string userId,
        List<string> identificacionesExistentes, ImportResultDTO result)
    {
        // Columnas esperadas:
        // A: TipoIdentificacion (1=Fisica, 2=Juridica, 3=DIMEX, 4=NITE, 5=Pasaporte, 6=Extranjera, 7=NoContribuyente)
        // B: NumeroIdentificacion (requerido)
        // C: Nombre (requerido)
        // D: NombreComercial
        // E: EmailPrincipal
        // F: TelefonoPrincipal
        // G: Provincia (1-7)
        // H: Canton
        // I: Distrito
        // J: OtrasSenas
        // K: DiasCredito
        // L: LimiteCredito
        // M: Banco
        // N: CuentaBancaria
        // O: IBAN

        var tipoIdStr = GetCellValue(worksheet, row, 1);
        var numeroId = GetCellValue(worksheet, row, 2);
        var nombre = GetCellValue(worksheet, row, 3);

        // Validar campos requeridos
        if (string.IsNullOrWhiteSpace(numeroId))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "NumeroIdentificacion",
                Message = "El número de identificación es requerido"
            });
            return null;
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "Nombre",
                Message = "El nombre es requerido"
            });
            return null;
        }

        // Validar duplicados
        if (identificacionesExistentes.Contains(numeroId))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO
            {
                Row = row,
                Column = "NumeroIdentificacion",
                Value = numeroId,
                Message = "Ya existe un proveedor con este número de identificación"
            });
            return null;
        }

        // Parsear tipo de identificación
        var tipoIdentificacion = TipoIdentificacion.Juridica;
        if (!string.IsNullOrWhiteSpace(tipoIdStr) && int.TryParse(tipoIdStr, out int tipoIdVal))
        {
            if (Enum.IsDefined(typeof(TipoIdentificacion), tipoIdVal))
            {
                tipoIdentificacion = (TipoIdentificacion)tipoIdVal;
            }
        }

        var proveedor = new Proveedor
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            TipoIdentificacion = tipoIdentificacion,
            NumeroIdentificacion = numeroId,
            Nombre = nombre,
            NombreComercial = GetCellValue(worksheet, row, 4),
            EmailPrincipal = GetCellValue(worksheet, row, 5),
            TelefonoPrincipal = GetCellValue(worksheet, row, 6),
            Provincia = GetCellInt(worksheet, row, 7),
            Canton = GetCellInt(worksheet, row, 8),
            Distrito = GetCellInt(worksheet, row, 9),
            OtrasSenas = GetCellValue(worksheet, row, 10),
            DiasCredito = GetCellInt(worksheet, row, 11),
            LimiteCredito = GetCellDecimal(worksheet, row, 12),
            Banco = GetCellValue(worksheet, row, 13),
            CuentaBancaria = GetCellValue(worksheet, row, 14),
            IBAN = GetCellValue(worksheet, row, 15),
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacionId = userId
        };

        return proveedor;
    }

    #endregion

    #region Generar Plantillas

    public byte[] GenerarPlantillaClientes()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Clientes");

        // Encabezados
        var headers = new[]
        {
            "TipoIdentificacion", "NumeroIdentificacion", "Nombre", "NombreComercial",
            "EmailPrincipal", "TelefonoPrincipal", "Provincia", "Canton", "Distrito",
            "OtrasSenas", "DiasCredito", "LimiteCredito"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        // Fila de ejemplo
        worksheet.Cell(2, 1).Value = 2; // Jurídica
        worksheet.Cell(2, 2).Value = "3101234567";
        worksheet.Cell(2, 3).Value = "Empresa Ejemplo S.A.";
        worksheet.Cell(2, 4).Value = "Empresa Ejemplo";
        worksheet.Cell(2, 5).Value = "ejemplo@email.com";
        worksheet.Cell(2, 6).Value = "22221111";
        worksheet.Cell(2, 7).Value = 1; // San José
        worksheet.Cell(2, 8).Value = 1; // Central
        worksheet.Cell(2, 9).Value = 1; // Carmen
        worksheet.Cell(2, 10).Value = "100 metros norte del parque";
        worksheet.Cell(2, 11).Value = 30;
        worksheet.Cell(2, 12).Value = 1000000;

        // Hoja de instrucciones
        var instructionsSheet = workbook.Worksheets.Add("Instrucciones");
        instructionsSheet.Cell(1, 1).Value = "INSTRUCCIONES DE USO";
        instructionsSheet.Cell(1, 1).Style.Font.Bold = true;

        instructionsSheet.Cell(3, 1).Value = "Columna";
        instructionsSheet.Cell(3, 2).Value = "Descripción";
        instructionsSheet.Cell(3, 3).Value = "Valores permitidos";

        var instructions = new[]
        {
            new[] { "TipoIdentificacion", "Tipo de cédula", "1=Física, 2=Jurídica, 3=DIMEX, 4=NITE, 5=Pasaporte, 6=Extranjera, 7=NoContribuyente" },
            new[] { "NumeroIdentificacion", "Número de cédula (Requerido)", "Texto sin guiones ni espacios" },
            new[] { "Nombre", "Nombre completo (Requerido)", "Hasta 200 caracteres" },
            new[] { "NombreComercial", "Nombre comercial", "Hasta 200 caracteres" },
            new[] { "EmailPrincipal", "Correo electrónico", "Email válido" },
            new[] { "TelefonoPrincipal", "Teléfono", "Hasta 20 caracteres" },
            new[] { "Provincia", "Código de provincia", "1=SanJosé, 2=Alajuela, 3=Cartago, 4=Heredia, 5=Guanacaste, 6=Puntarenas, 7=Limón" },
            new[] { "Canton", "Código de cantón", "Número según provincia" },
            new[] { "Distrito", "Código de distrito", "Número según cantón" },
            new[] { "OtrasSenas", "Dirección", "Hasta 250 caracteres" },
            new[] { "DiasCredito", "Días de crédito", "Número entero" },
            new[] { "LimiteCredito", "Límite de crédito", "Número decimal" }
        };

        for (int i = 0; i < instructions.Length; i++)
        {
            instructionsSheet.Cell(4 + i, 1).Value = instructions[i][0];
            instructionsSheet.Cell(4 + i, 2).Value = instructions[i][1];
            instructionsSheet.Cell(4 + i, 3).Value = instructions[i][2];
        }

        worksheet.Columns().AdjustToContents();
        instructionsSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerarPlantillaProductos()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Productos");

        // Encabezados
        var headers = new[]
        {
            "Tipo", "Codigo", "Nombre", "Descripcion", "UnidadMedida",
            "PrecioVenta", "Costo", "ImpuestoCodigo", "Categoria",
            "ControlarInventario", "StockMinimo", "CabysId"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
        }

        // Fila de ejemplo
        worksheet.Cell(2, 1).Value = 0; // Producto
        worksheet.Cell(2, 2).Value = "PROD001";
        worksheet.Cell(2, 3).Value = "Producto de Ejemplo";
        worksheet.Cell(2, 4).Value = "Descripción del producto";
        worksheet.Cell(2, 5).Value = "Unid";
        worksheet.Cell(2, 6).Value = 10000;
        worksheet.Cell(2, 7).Value = 5000;
        worksheet.Cell(2, 8).Value = "08";
        worksheet.Cell(2, 9).Value = "";
        worksheet.Cell(2, 10).Value = 1;
        worksheet.Cell(2, 11).Value = 10;
        worksheet.Cell(2, 12).Value = "";

        // Hoja de instrucciones
        var instructionsSheet = workbook.Worksheets.Add("Instrucciones");
        instructionsSheet.Cell(1, 1).Value = "INSTRUCCIONES DE USO";
        instructionsSheet.Cell(1, 1).Style.Font.Bold = true;

        instructionsSheet.Cell(3, 1).Value = "Columna";
        instructionsSheet.Cell(3, 2).Value = "Descripción";
        instructionsSheet.Cell(3, 3).Value = "Valores permitidos";

        var instructions = new[]
        {
            new[] { "Tipo", "Tipo de producto", "0=Producto, 1=Servicio" },
            new[] { "Codigo", "Código único (Requerido)", "Hasta 50 caracteres" },
            new[] { "Nombre", "Nombre del producto (Requerido)", "Hasta 200 caracteres" },
            new[] { "Descripcion", "Descripción", "Hasta 500 caracteres" },
            new[] { "UnidadMedida", "Código unidad de medida", "Unid, Kg, m, L, Sp (servicio profesional), etc." },
            new[] { "PrecioVenta", "Precio de venta (Requerido)", "Número decimal mayor a 0" },
            new[] { "Costo", "Costo del producto", "Número decimal" },
            new[] { "ImpuestoCodigo", "Código de impuesto", "01=Exento, 02=Tarifa0, 07=CalcEspecial, 08=IVA13%" },
            new[] { "Categoria", "Nombre de categoría", "Debe existir previamente" },
            new[] { "ControlarInventario", "Controlar stock", "1=Sí, 0=No" },
            new[] { "StockMinimo", "Stock mínimo para alertas", "Número decimal" },
            new[] { "CabysId", "ID del código CAByS", "Número entero (para facturación electrónica)" }
        };

        for (int i = 0; i < instructions.Length; i++)
        {
            instructionsSheet.Cell(4 + i, 1).Value = instructions[i][0];
            instructionsSheet.Cell(4 + i, 2).Value = instructions[i][1];
            instructionsSheet.Cell(4 + i, 3).Value = instructions[i][2];
        }

        worksheet.Columns().AdjustToContents();
        instructionsSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerarPlantillaProveedores()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Proveedores");

        // Encabezados
        var headers = new[]
        {
            "TipoIdentificacion", "NumeroIdentificacion", "Nombre", "NombreComercial",
            "EmailPrincipal", "TelefonoPrincipal", "Provincia", "Canton", "Distrito",
            "OtrasSenas", "DiasCredito", "LimiteCredito", "Banco", "CuentaBancaria", "IBAN"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightYellow;
        }

        // Fila de ejemplo
        worksheet.Cell(2, 1).Value = 2; // Jurídica
        worksheet.Cell(2, 2).Value = "3101234567";
        worksheet.Cell(2, 3).Value = "Proveedor Ejemplo S.A.";
        worksheet.Cell(2, 4).Value = "Proveedor Ejemplo";
        worksheet.Cell(2, 5).Value = "proveedor@email.com";
        worksheet.Cell(2, 6).Value = "22221111";
        worksheet.Cell(2, 7).Value = 1;
        worksheet.Cell(2, 8).Value = 1;
        worksheet.Cell(2, 9).Value = 1;
        worksheet.Cell(2, 10).Value = "100 metros sur del parque";
        worksheet.Cell(2, 11).Value = 30;
        worksheet.Cell(2, 12).Value = 5000000;
        worksheet.Cell(2, 13).Value = "BCR";
        worksheet.Cell(2, 14).Value = "001-0123456-7";
        worksheet.Cell(2, 15).Value = "CR12345678901234567890";

        // Hoja de instrucciones
        var instructionsSheet = workbook.Worksheets.Add("Instrucciones");
        instructionsSheet.Cell(1, 1).Value = "INSTRUCCIONES DE USO";
        instructionsSheet.Cell(1, 1).Style.Font.Bold = true;

        instructionsSheet.Cell(3, 1).Value = "Columna";
        instructionsSheet.Cell(3, 2).Value = "Descripción";
        instructionsSheet.Cell(3, 3).Value = "Valores permitidos";

        var instructions = new[]
        {
            new[] { "TipoIdentificacion", "Tipo de cédula", "1=Física, 2=Jurídica, 3=DIMEX, 4=NITE, 5=Pasaporte, 6=Extranjera, 7=NoContribuyente" },
            new[] { "NumeroIdentificacion", "Número de cédula (Requerido)", "Texto sin guiones ni espacios" },
            new[] { "Nombre", "Nombre completo (Requerido)", "Hasta 200 caracteres" },
            new[] { "NombreComercial", "Nombre comercial", "Hasta 200 caracteres" },
            new[] { "EmailPrincipal", "Correo electrónico", "Email válido" },
            new[] { "TelefonoPrincipal", "Teléfono", "Hasta 20 caracteres" },
            new[] { "Provincia", "Código de provincia", "1=SanJosé, 2=Alajuela, 3=Cartago, 4=Heredia, 5=Guanacaste, 6=Puntarenas, 7=Limón" },
            new[] { "Canton", "Código de cantón", "Número según provincia" },
            new[] { "Distrito", "Código de distrito", "Número según cantón" },
            new[] { "OtrasSenas", "Dirección", "Hasta 500 caracteres" },
            new[] { "DiasCredito", "Días de crédito", "Número entero" },
            new[] { "LimiteCredito", "Límite de crédito", "Número decimal" },
            new[] { "Banco", "Nombre del banco", "Hasta 100 caracteres" },
            new[] { "CuentaBancaria", "Número de cuenta", "Hasta 30 caracteres" },
            new[] { "IBAN", "Código IBAN", "Hasta 30 caracteres" }
        };

        for (int i = 0; i < instructions.Length; i++)
        {
            instructionsSheet.Cell(4 + i, 1).Value = instructions[i][0];
            instructionsSheet.Cell(4 + i, 2).Value = instructions[i][1];
            instructionsSheet.Cell(4 + i, 3).Value = instructions[i][2];
        }

        worksheet.Columns().AdjustToContents();
        instructionsSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Importar Activos Fijos

    public async Task<ImportResultDTO> ImportarActivosFijosAsync(Stream fileStream, Guid empresaId, string userId)
    {
        var result = new ImportResultDTO { WasSuccess = true };

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene hojas de trabajo";
                return result;
            }

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow < 2)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene datos para importar";
                return result;
            }

            result.TotalRows = lastRow - 1;

            // Obtener códigos existentes
            var codigosExistentes = await _context.ActivosFijos
                .Where(a => a.EmpresaId == empresaId && !a.IsDeleted)
                .Select(a => a.Codigo)
                .ToListAsync();

            // Obtener categorías y sucursales válidas
            var categorias = await _context.CategoriasActivo
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.Activo)
                .ToListAsync();

            var sucursales = await _context.Sucursales
                .Where(s => s.EmpresaId == empresaId && !s.IsDeleted)
                .ToListAsync();

            var activosNuevos = new List<ActivoFijo>();

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var activo = ParseActivoFijoRow(worksheet, row, empresaId, userId,
                        codigosExistentes, categorias, sucursales, result);
                    if (activo != null)
                    {
                        activosNuevos.Add(activo);
                        codigosExistentes.Add(activo.Codigo);
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportErrorDTO
                    {
                        Row = row,
                        Message = $"Error procesando fila: {ex.Message}"
                    });
                }
            }

            if (activosNuevos.Any())
            {
                await _context.ActivosFijos.AddRangeAsync(activosNuevos);
                await _context.SaveChangesAsync();
            }

            result.Message = $"Importación completada: {result.SuccessCount} activos importados, {result.ErrorCount} errores";
            _logger.LogInformation("Importación de activos fijos completada para empresa {EmpresaId}: {Success} exitosos, {Errors} errores",
                empresaId, result.SuccessCount, result.ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar activos fijos para empresa {EmpresaId}", empresaId);
            result.WasSuccess = false;
            result.Message = $"Error al procesar el archivo: {ex.Message}";
        }

        return result;
    }

    private ActivoFijo? ParseActivoFijoRow(IXLWorksheet worksheet, int row, Guid empresaId, string userId,
        List<string> codigosExistentes, List<CategoriaActivo> categorias, List<Sucursal> sucursales, ImportResultDTO result)
    {
        var codigo = GetCellValue(worksheet, row, 1);
        var nombre = GetCellValue(worksheet, row, 2);
        var categoriaCodigoNombre = GetCellValue(worksheet, row, 3);
        var sucursalNombre = GetCellValue(worksheet, row, 4);

        if (string.IsNullOrWhiteSpace(codigo))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Codigo", Message = "El código es requerido" });
            return null;
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Nombre", Message = "El nombre es requerido" });
            return null;
        }

        if (codigosExistentes.Contains(codigo))
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Codigo", Value = codigo, Message = "Ya existe un activo con este código" });
            return null;
        }

        // Buscar categoría
        var categoria = categorias.FirstOrDefault(c =>
            c.Codigo.Equals(categoriaCodigoNombre, StringComparison.OrdinalIgnoreCase) ||
            c.Nombre.Contains(categoriaCodigoNombre ?? "", StringComparison.OrdinalIgnoreCase));

        if (categoria == null)
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Categoria", Value = categoriaCodigoNombre, Message = "Categoría no encontrada" });
            return null;
        }

        // Buscar sucursal
        var sucursal = sucursales.FirstOrDefault(s =>
            s.Nombre.Contains(sucursalNombre ?? "", StringComparison.OrdinalIgnoreCase));

        if (sucursal == null && sucursales.Count > 0)
        {
            sucursal = sucursales.First(); // Usar primera sucursal por defecto
        }

        if (sucursal == null)
        {
            result.ErrorCount++;
            result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Sucursal", Message = "No hay sucursales disponibles" });
            return null;
        }

        var fechaCompraStr = GetCellValue(worksheet, row, 5);
        if (!DateTime.TryParse(fechaCompraStr, out DateTime fechaCompra))
        {
            fechaCompra = DateTime.Today;
        }

        return new ActivoFijo
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = GetCellValue(worksheet, row, 6),
            CategoriaActivoId = categoria.Id,
            SucursalId = sucursal.Id,
            FechaCompra = fechaCompra,
            ValorOriginal = GetCellDecimal(worksheet, row, 7),
            ValorResidual = GetCellDecimal(worksheet, row, 8),
            VidaUtilAnios = GetCellInt(worksheet, row, 9) > 0 ? GetCellInt(worksheet, row, 9) : categoria.VidaUtilAnios,
            PorcentajeDepreciacionAnual = GetCellDecimal(worksheet, row, 10) > 0 ? GetCellDecimal(worksheet, row, 10) : categoria.PorcentajeDepreciacionAnual,
            MetodoDepreciacion = GetCellValue(worksheet, row, 11) ?? "LR",
            Marca = GetCellValue(worksheet, row, 12),
            Modelo = GetCellValue(worksheet, row, 13),
            NumeroSerie = GetCellValue(worksheet, row, 14),
            Estado = "ACT",
            FrecuenciaDepreciacion = "MEN",
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacionId = userId
        };
    }

    public byte[] GenerarPlantillaActivosFijos()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ActivosFijos");

        var headers = new[] { "Codigo", "Nombre", "Categoria", "Sucursal", "FechaCompra", "Descripcion",
            "ValorOriginal", "ValorResidual", "VidaUtilAnios", "PorcentajeDepAnual", "MetodoDepreciacion",
            "Marca", "Modelo", "NumeroSerie" };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightCoral;
        }

        // Ejemplo
        worksheet.Cell(2, 1).Value = "ACT001";
        worksheet.Cell(2, 2).Value = "Computadora Dell";
        worksheet.Cell(2, 3).Value = "EQUIPOS";
        worksheet.Cell(2, 4).Value = "Principal";
        worksheet.Cell(2, 5).Value = DateTime.Today.ToString("yyyy-MM-dd");
        worksheet.Cell(2, 6).Value = "Computadora de escritorio";
        worksheet.Cell(2, 7).Value = 500000;
        worksheet.Cell(2, 8).Value = 50000;
        worksheet.Cell(2, 9).Value = 5;
        worksheet.Cell(2, 10).Value = 20;
        worksheet.Cell(2, 11).Value = "LR";
        worksheet.Cell(2, 12).Value = "Dell";
        worksheet.Cell(2, 13).Value = "OptiPlex 3080";
        worksheet.Cell(2, 14).Value = "ABC123456";

        // Instrucciones
        var inst = workbook.Worksheets.Add("Instrucciones");
        inst.Cell(1, 1).Value = "INSTRUCCIONES - Activos Fijos";
        inst.Cell(1, 1).Style.Font.Bold = true;
        inst.Cell(3, 1).Value = "MetodoDepreciacion: LR=Línea Recta, SAD=Suma Años Dígitos, SDD=Saldo Doble Decreciente";
        inst.Cell(4, 1).Value = "Categoria: Debe coincidir con código o nombre de una categoría existente";
        inst.Cell(5, 1).Value = "Sucursal: Nombre de la sucursal (si no existe, se usa la primera)";

        worksheet.Columns().AdjustToContents();
        inst.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Importar Categorías de Activo

    public async Task<ImportResultDTO> ImportarCategoriasActivoAsync(Stream fileStream, Guid empresaId, string userId)
    {
        var result = new ImportResultDTO { WasSuccess = true };

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene hojas de trabajo";
                return result;
            }

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow < 2)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene datos para importar";
                return result;
            }

            result.TotalRows = lastRow - 1;

            var codigosExistentes = await _context.CategoriasActivo
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
                .Select(c => c.Codigo)
                .ToListAsync();

            var categoriasNuevas = new List<CategoriaActivo>();

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var codigo = GetCellValue(worksheet, row, 1);
                    var nombre = GetCellValue(worksheet, row, 2);

                    if (string.IsNullOrWhiteSpace(codigo))
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Codigo", Message = "El código es requerido" });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Nombre", Message = "El nombre es requerido" });
                        continue;
                    }

                    if (codigosExistentes.Contains(codigo))
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Codigo", Value = codigo, Message = "Ya existe una categoría con este código" });
                        continue;
                    }

                    var categoria = new CategoriaActivo
                    {
                        Id = Guid.NewGuid(),
                        EmpresaId = empresaId,
                        Codigo = codigo,
                        Nombre = nombre,
                        Descripcion = GetCellValue(worksheet, row, 3),
                        VidaUtilAnios = GetCellInt(worksheet, row, 4) > 0 ? GetCellInt(worksheet, row, 4) : 5,
                        PorcentajeDepreciacionAnual = GetCellDecimal(worksheet, row, 5) > 0 ? GetCellDecimal(worksheet, row, 5) : 20,
                        CuentaActivo = GetCellValue(worksheet, row, 6),
                        CuentaDepreciacionAcumulada = GetCellValue(worksheet, row, 7),
                        CuentaGastoDepreciacion = GetCellValue(worksheet, row, 8),
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow,
                        UsuarioCreacionId = userId
                    };

                    categoriasNuevas.Add(categoria);
                    codigosExistentes.Add(codigo);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportErrorDTO { Row = row, Message = $"Error: {ex.Message}" });
                }
            }

            if (categoriasNuevas.Any())
            {
                await _context.CategoriasActivo.AddRangeAsync(categoriasNuevas);
                await _context.SaveChangesAsync();
            }

            result.Message = $"Importación completada: {result.SuccessCount} categorías importadas, {result.ErrorCount} errores";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar categorías de activo para empresa {EmpresaId}", empresaId);
            result.WasSuccess = false;
            result.Message = $"Error al procesar el archivo: {ex.Message}";
        }

        return result;
    }

    public byte[] GenerarPlantillaCategoriasActivo()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("CategoriasActivo");

        var headers = new[] { "Codigo", "Nombre", "Descripcion", "VidaUtilAnios", "PorcentajeDepAnual",
            "CuentaActivo", "CuentaDepreciacionAcum", "CuentaGastoDepreciacion" };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightSalmon;
        }

        // Ejemplos predefinidos
        var ejemplos = new[]
        {
            new[] { "EQUIPOS", "Equipo de Cómputo", "Computadoras, servidores, impresoras", "5", "20", "", "", "" },
            new[] { "MOBILIARIO", "Mobiliario y Equipo", "Escritorios, sillas, archivos", "10", "10", "", "", "" },
            new[] { "VEHICULOS", "Vehículos", "Automóviles, motocicletas", "5", "20", "", "", "" },
            new[] { "MAQUIN", "Maquinaria", "Maquinaria industrial y equipos", "10", "10", "", "", "" },
            new[] { "HERRAM", "Herramientas", "Herramientas manuales y eléctricas", "5", "20", "", "", "" }
        };

        for (int i = 0; i < ejemplos.Length; i++)
        {
            for (int j = 0; j < ejemplos[i].Length; j++)
            {
                worksheet.Cell(i + 2, j + 1).Value = ejemplos[i][j];
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Importar Inventario (Ajuste de Stock)

    public async Task<ImportResultDTO> ImportarInventarioAsync(Stream fileStream, Guid empresaId, Guid sucursalId, string userId)
    {
        var result = new ImportResultDTO { WasSuccess = true };

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene hojas de trabajo";
                return result;
            }

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow < 2)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene datos para importar";
                return result;
            }

            result.TotalRows = lastRow - 1;

            // Obtener productos de la empresa que controlan inventario
            var productos = await _context.Productos
                .Where(p => p.EmpresaId == empresaId && !p.IsDeleted && p.ControlarInventario)
                .ToListAsync();

            // Obtener inventarios existentes para esta sucursal
            var inventariosExistentes = await _context.Inventarios
                .Where(i => i.SucursalId == sucursalId && !i.IsDeleted)
                .ToListAsync();

            var inventariosNuevos = new List<Inventario>();
            var inventariosActualizar = new List<Inventario>();

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var codigoProducto = GetCellValue(worksheet, row, 1);
                    var cantidadStr = GetCellValue(worksheet, row, 2);
                    var costoStr = GetCellValue(worksheet, row, 3);

                    if (string.IsNullOrWhiteSpace(codigoProducto))
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportErrorDTO { Row = row, Column = "CodigoProducto", Message = "El código del producto es requerido" });
                        continue;
                    }

                    var producto = productos.FirstOrDefault(p =>
                        p.Codigo.Equals(codigoProducto, StringComparison.OrdinalIgnoreCase));

                    if (producto == null)
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportErrorDTO { Row = row, Column = "CodigoProducto", Value = codigoProducto, Message = "Producto no encontrado o no controla inventario" });
                        continue;
                    }

                    if (!decimal.TryParse(cantidadStr, out decimal cantidad))
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Cantidad", Value = cantidadStr, Message = "Cantidad inválida" });
                        continue;
                    }

                    decimal.TryParse(costoStr, out decimal costoPromedio);

                    var inventarioExistente = inventariosExistentes.FirstOrDefault(i => i.ProductoId == producto.Id);

                    if (inventarioExistente != null)
                    {
                        // Actualizar existente
                        inventarioExistente.CantidadActual = cantidad;
                        if (costoPromedio > 0)
                            inventarioExistente.CostoPromedio = costoPromedio;
                        inventarioExistente.UltimaActualizacion = DateTime.UtcNow;
                        inventarioExistente.FechaModificacion = DateTime.UtcNow;
                        inventarioExistente.UsuarioModificacionId = userId;
                        inventariosActualizar.Add(inventarioExistente);
                    }
                    else
                    {
                        // Crear nuevo
                        var nuevoInventario = new Inventario
                        {
                            Id = Guid.NewGuid(),
                            ProductoId = producto.Id,
                            SucursalId = sucursalId,
                            CantidadActual = cantidad,
                            CostoPromedio = costoPromedio > 0 ? costoPromedio : producto.Costo ?? 0,
                            UltimaActualizacion = DateTime.UtcNow,
                            FechaCreacion = DateTime.UtcNow,
                            UsuarioCreacionId = userId
                        };
                        inventariosNuevos.Add(nuevoInventario);
                    }

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportErrorDTO { Row = row, Message = $"Error: {ex.Message}" });
                }
            }

            if (inventariosNuevos.Any())
            {
                await _context.Inventarios.AddRangeAsync(inventariosNuevos);
            }

            if (inventariosActualizar.Any())
            {
                _context.Inventarios.UpdateRange(inventariosActualizar);
            }

            await _context.SaveChangesAsync();

            result.Message = $"Importación completada: {result.SuccessCount} registros procesados ({inventariosNuevos.Count} nuevos, {inventariosActualizar.Count} actualizados), {result.ErrorCount} errores";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar inventario para empresa {EmpresaId}", empresaId);
            result.WasSuccess = false;
            result.Message = $"Error al procesar el archivo: {ex.Message}";
        }

        return result;
    }

    public byte[] GenerarPlantillaInventario()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Inventario");

        var headers = new[] { "CodigoProducto", "Cantidad", "CostoPromedio" };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightCyan;
        }

        // Ejemplos
        worksheet.Cell(2, 1).Value = "PROD001";
        worksheet.Cell(2, 2).Value = 100;
        worksheet.Cell(2, 3).Value = 5000;

        worksheet.Cell(3, 1).Value = "PROD002";
        worksheet.Cell(3, 2).Value = 50;
        worksheet.Cell(3, 3).Value = 2500;

        // Instrucciones
        var inst = workbook.Worksheets.Add("Instrucciones");
        inst.Cell(1, 1).Value = "INSTRUCCIONES - Ajuste de Inventario";
        inst.Cell(1, 1).Style.Font.Bold = true;
        inst.Cell(3, 1).Value = "CodigoProducto: Código del producto (debe existir y tener 'Controlar Inventario' activo)";
        inst.Cell(4, 1).Value = "Cantidad: Nueva cantidad en stock (reemplaza la cantidad actual)";
        inst.Cell(5, 1).Value = "CostoPromedio: Costo promedio por unidad (opcional, si está vacío mantiene el actual)";
        inst.Cell(7, 1).Value = "NOTA: Si el producto ya tiene inventario en la sucursal, se actualiza. Si no existe, se crea.";

        worksheet.Columns().AdjustToContents();
        inst.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Importar Categorías de Gasto

    public async Task<ImportResultDTO> ImportarCategoriasGastoAsync(Stream fileStream, string userId)
    {
        var result = new ImportResultDTO { WasSuccess = true };

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene hojas de trabajo";
                return result;
            }

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow < 2)
            {
                result.WasSuccess = false;
                result.Message = "El archivo no contiene datos para importar";
                return result;
            }

            result.TotalRows = lastRow - 1;

            var nombresExistentes = await _context.CategoriasGasto
                .Select(c => c.Nombre.ToLower())
                .ToListAsync();

            var categoriasNuevas = new List<CategoriaGasto>();

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var nombre = GetCellValue(worksheet, row, 1);

                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Nombre", Message = "El nombre es requerido" });
                        continue;
                    }

                    if (nombresExistentes.Contains(nombre.ToLower()))
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportErrorDTO { Row = row, Column = "Nombre", Value = nombre, Message = "Ya existe una categoría con este nombre" });
                        continue;
                    }

                    var categoria = new CategoriaGasto
                    {
                        Nombre = nombre,
                        Descripcion = GetCellValue(worksheet, row, 2),
                        Activa = true
                    };

                    categoriasNuevas.Add(categoria);
                    nombresExistentes.Add(nombre.ToLower());
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportErrorDTO { Row = row, Message = $"Error: {ex.Message}" });
                }
            }

            if (categoriasNuevas.Any())
            {
                await _context.CategoriasGasto.AddRangeAsync(categoriasNuevas);
                await _context.SaveChangesAsync();
            }

            result.Message = $"Importación completada: {result.SuccessCount} categorías importadas, {result.ErrorCount} errores";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar categorías de gasto");
            result.WasSuccess = false;
            result.Message = $"Error al procesar el archivo: {ex.Message}";
        }

        return result;
    }

    public byte[] GenerarPlantillaCategoriasGasto()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("CategoriasGasto");

        var headers = new[] { "Nombre", "Descripcion" };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGoldenrodYellow;
        }

        // Ejemplos predefinidos
        var ejemplos = new[]
        {
            new[] { "Servicios Públicos", "Electricidad, agua, internet, teléfono" },
            new[] { "Alquiler", "Alquiler de local o edificio" },
            new[] { "Salarios", "Sueldos y salarios del personal" },
            new[] { "Suministros de Oficina", "Papelería, artículos de oficina" },
            new[] { "Combustible", "Gasolina, diesel para vehículos" },
            new[] { "Mantenimiento", "Reparaciones y mantenimiento" },
            new[] { "Publicidad", "Publicidad y mercadeo" },
            new[] { "Seguros", "Pólizas de seguro" },
            new[] { "Impuestos", "Impuestos varios" },
            new[] { "Viáticos", "Gastos de viaje y alimentación" }
        };

        for (int i = 0; i < ejemplos.Length; i++)
        {
            for (int j = 0; j < ejemplos[i].Length; j++)
            {
                worksheet.Cell(i + 2, j + 1).Value = ejemplos[i][j];
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Helpers

    private static string? GetCellValue(IXLWorksheet worksheet, int row, int col)
    {
        var cell = worksheet.Cell(row, col);
        var value = cell.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int GetCellInt(IXLWorksheet worksheet, int row, int col)
    {
        var value = GetCellValue(worksheet, row, col);
        return int.TryParse(value, out int result) ? result : 0;
    }

    private static int? GetCellIntNullable(IXLWorksheet worksheet, int row, int col)
    {
        var value = GetCellValue(worksheet, row, col);
        return int.TryParse(value, out int result) ? result : null;
    }

    private static decimal GetCellDecimal(IXLWorksheet worksheet, int row, int col)
    {
        var value = GetCellValue(worksheet, row, col);
        return decimal.TryParse(value, out decimal result) ? result : 0;
    }

    private static decimal? GetCellDecimalNullable(IXLWorksheet worksheet, int row, int col)
    {
        var value = GetCellValue(worksheet, row, col);
        return decimal.TryParse(value, out decimal result) ? result : null;
    }

    #endregion
}
