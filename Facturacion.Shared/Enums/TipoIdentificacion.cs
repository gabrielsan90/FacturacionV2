namespace Facturacion.Shared.Enums;

/// <summary>
/// Tipos de Identificación según Hacienda v4.4
/// </summary>
public enum TipoIdentificacion
{
    /// <summary>01 - Cédula Física</summary>
    Fisica = 1,

    /// <summary>02 - Cédula Jurídica</summary>
    Juridica = 2,

    /// <summary>03 - DIMEX (Documento de Identificación Migratorio para Extranjeros)</summary>
    DIMEX = 3,

    /// <summary>04 - NITE (Número de Identificación Tributario Especial)</summary>
    NITE = 4,

    /// <summary>05 - Pasaporte (NUEVO v4.4)</summary>
    Pasaporte = 5,

    /// <summary>06 - Extranjero no domiciliado (NUEVO v4.4)</summary>
    Extranjera = 6,

    /// <summary>07 - No contribuyente (NUEVO v4.4)</summary>
    NoContribuyente = 7
}
