using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para importación de datos desde archivos Excel
/// </summary>
public interface IExcelImportService
{
    /// <summary>
    /// Importa clientes desde un archivo Excel
    /// </summary>
    /// <param name="fileStream">Stream del archivo Excel</param>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="userId">ID del usuario que realiza la importación</param>
    /// <returns>Resultado de la importación</returns>
    Task<ImportResultDTO> ImportarClientesAsync(Stream fileStream, Guid empresaId, string userId);

    /// <summary>
    /// Importa productos desde un archivo Excel
    /// </summary>
    /// <param name="fileStream">Stream del archivo Excel</param>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="userId">ID del usuario que realiza la importación</param>
    /// <returns>Resultado de la importación</returns>
    Task<ImportResultDTO> ImportarProductosAsync(Stream fileStream, Guid empresaId, string userId);

    /// <summary>
    /// Importa proveedores desde un archivo Excel
    /// </summary>
    /// <param name="fileStream">Stream del archivo Excel</param>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="userId">ID del usuario que realiza la importación</param>
    /// <returns>Resultado de la importación</returns>
    Task<ImportResultDTO> ImportarProveedoresAsync(Stream fileStream, Guid empresaId, string userId);

    /// <summary>
    /// Genera una plantilla Excel para importación de clientes
    /// </summary>
    /// <returns>Bytes del archivo Excel</returns>
    byte[] GenerarPlantillaClientes();

    /// <summary>
    /// Genera una plantilla Excel para importación de productos
    /// </summary>
    /// <returns>Bytes del archivo Excel</returns>
    byte[] GenerarPlantillaProductos();

    /// <summary>
    /// Genera una plantilla Excel para importación de proveedores
    /// </summary>
    /// <returns>Bytes del archivo Excel</returns>
    byte[] GenerarPlantillaProveedores();

    // =====================================================
    // Activos Fijos
    // =====================================================
    Task<ImportResultDTO> ImportarActivosFijosAsync(Stream fileStream, Guid empresaId, string userId);
    byte[] GenerarPlantillaActivosFijos();

    // =====================================================
    // Categorías de Activo
    // =====================================================
    Task<ImportResultDTO> ImportarCategoriasActivoAsync(Stream fileStream, Guid empresaId, string userId);
    byte[] GenerarPlantillaCategoriasActivo();

    // =====================================================
    // Inventario (ajuste de stock)
    // =====================================================
    Task<ImportResultDTO> ImportarInventarioAsync(Stream fileStream, Guid empresaId, Guid sucursalId, string userId);
    byte[] GenerarPlantillaInventario();

    // =====================================================
    // Categorías de Gasto
    // =====================================================
    Task<ImportResultDTO> ImportarCategoriasGastoAsync(Stream fileStream, string userId);
    byte[] GenerarPlantillaCategoriasGasto();
}
