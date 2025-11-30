using Facturacion.Backend.Services.Interfaces;
using System.Text.RegularExpressions;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de manejo de errores de Hacienda
/// Proporciona mensajes amigables y acciones recomendadas
/// </summary>
public class HaciendaErrorService : IHaciendaErrorService
{
    // Diccionario de códigos de error conocidos
    private static readonly Dictionary<string, (string Mensaje, string Accion, bool Recuperable)> _erroresConocidos = new()
    {
        // Errores de estructura XML (300s)
        ["300"] = (
            "Error en la estructura del documento XML.",
            "Verifique que todos los campos obligatorios estén presentes y tengan el formato correcto.",
            false),

        ["301"] = (
            "Falta un campo obligatorio en el documento.",
            "Revise que todos los datos requeridos estén completos antes de enviar.",
            false),

        ["302"] = (
            "El formato de un campo es incorrecto.",
            "Verifique los tipos de datos, longitudes y formatos de fecha.",
            false),

        ["303"] = (
            "Un valor está fuera del rango permitido.",
            "Revise que los montos, cantidades y porcentajes estén dentro de los límites aceptados.",
            false),

        // Errores de clave/consecutivo (400s)
        ["400"] = (
            "La clave numérica ya fue utilizada.",
            "Esto indica un problema interno. Contacte al administrador del sistema.",
            false),

        ["401"] = (
            "El número consecutivo es inválido.",
            "Verifique que el consecutivo siga el formato correcto y no esté repetido.",
            false),

        ["402"] = (
            "El consecutivo no sigue la secuencia correcta.",
            "Revise la configuración de consecutivos de la terminal.",
            false),

        ["403"] = (
            "El tipo de documento en el consecutivo no corresponde.",
            "Verifique que el tipo de documento coincida con el XML.",
            false),

        // Errores de firma digital (500s)
        ["500"] = (
            "Error en la firma digital del documento.",
            "Verifique que el certificado digital sea válido y el PIN sea correcto.",
            false),

        ["501"] = (
            "La firma digital no se pudo validar.",
            "El certificado puede estar corrupto. Intente cargarlo nuevamente.",
            false),

        ["502"] = (
            "El certificado no corresponde al emisor.",
            "Asegúrese de usar el certificado de la empresa correcta.",
            false),

        // Errores de certificado (600s)
        ["600"] = (
            "El certificado digital es inválido.",
            "Verifique que el archivo .p12 sea correcto y no esté dañado.",
            false),

        ["601"] = (
            "El certificado digital ha expirado.",
            "Renueve el certificado digital con el proveedor autorizado.",
            false),

        ["602"] = (
            "El certificado digital está revocado.",
            "Contacte al emisor del certificado para más información.",
            false),

        ["603"] = (
            "El certificado no es válido para firma electrónica.",
            "Obtenga un certificado autorizado para facturación electrónica.",
            false),

        // Errores de validación de datos (700s)
        ["700"] = (
            "El emisor no está registrado en Hacienda.",
            "Verifique que la empresa esté inscrita en el sistema ATV.",
            false),

        ["701"] = (
            "El receptor no existe en el registro tributario.",
            "Verifique el número de identificación del cliente.",
            false),

        ["702"] = (
            "El código de actividad económica es inválido.",
            "Seleccione una actividad económica registrada para la empresa.",
            false),

        ["703"] = (
            "El código CAByS no existe en el catálogo.",
            "Actualice el catálogo CAByS y seleccione un código válido.",
            false),

        // Errores de impuestos (-50 a -59)
        ["-50"] = (
            "Error en el cálculo de impuestos.",
            "Revise las tarifas de IVA y los montos calculados.",
            false),

        ["-51"] = (
            "La tarifa de impuesto no es válida.",
            "Verifique que la tarifa de IVA corresponda al tipo de producto.",
            false),

        ["-52"] = (
            "Error en el cálculo de exoneración.",
            "Verifique los datos del documento de exoneración.",
            false),

        // Errores de comunicación (800s)
        ["800"] = (
            "Error de comunicación con el servidor de Hacienda.",
            "Intente nuevamente en unos minutos.",
            true),

        ["801"] = (
            "Tiempo de espera agotado.",
            "El servidor de Hacienda no respondió. Reintente más tarde.",
            true),

        ["802"] = (
            "El servidor de Hacienda está en mantenimiento.",
            "Espere a que finalice el mantenimiento programado.",
            true),

        // Errores genéricos
        ["999"] = (
            "Error no especificado.",
            "Revise el detalle del error y contacte soporte si persiste.",
            false),
    };

    /// <summary>
    /// Obtiene un mensaje amigable para un código de error
    /// </summary>
    public string ObtenerMensajeAmigable(string codigoError)
    {
        if (string.IsNullOrWhiteSpace(codigoError))
            return "Error desconocido.";

        // Normalizar código (quitar espacios, guiones)
        var codigo = codigoError.Trim().Replace("-", "");

        // Buscar por código exacto
        if (_erroresConocidos.TryGetValue(codigoError, out var errorExacto))
            return errorExacto.Mensaje;

        // Buscar por grupo (primeros 2 dígitos)
        if (codigo.Length >= 2)
        {
            var grupo = codigo.Substring(0, 1) + "00";
            if (_erroresConocidos.TryGetValue(grupo, out var errorGrupo))
                return errorGrupo.Mensaje;
        }

        // Intentar identificar el error por patrones comunes
        if (codigoError.Contains("certificado", StringComparison.OrdinalIgnoreCase) ||
            codigoError.Contains("certificate", StringComparison.OrdinalIgnoreCase))
            return "Error relacionado con el certificado digital. Verifique que esté vigente y el PIN sea correcto.";

        if (codigoError.Contains("firma", StringComparison.OrdinalIgnoreCase) ||
            codigoError.Contains("signature", StringComparison.OrdinalIgnoreCase))
            return "Error en la firma digital del documento.";

        if (codigoError.Contains("xml", StringComparison.OrdinalIgnoreCase))
            return "Error en la estructura del documento XML.";

        if (codigoError.Contains("consecutivo", StringComparison.OrdinalIgnoreCase))
            return "Error con el número consecutivo del documento.";

        return $"Error: {codigoError}. Contacte al administrador si el problema persiste.";
    }

    /// <summary>
    /// Obtiene la acción recomendada para un código de error
    /// </summary>
    public string ObtenerAccionRecomendada(string codigoError)
    {
        if (string.IsNullOrWhiteSpace(codigoError))
            return "Revise los datos del documento e intente nuevamente.";

        if (_erroresConocidos.TryGetValue(codigoError, out var error))
            return error.Accion;

        // Acciones genéricas por tipo de error
        if (codigoError.StartsWith("3"))
            return "Verifique que todos los campos estén completos y con el formato correcto.";

        if (codigoError.StartsWith("4"))
            return "Revise la configuración de consecutivos y claves numéricas.";

        if (codigoError.StartsWith("5") || codigoError.StartsWith("6"))
            return "Verifique el certificado digital y el PIN de la empresa.";

        if (codigoError.StartsWith("7"))
            return "Valide los datos de emisor, receptor y productos.";

        if (codigoError.StartsWith("8"))
            return "Intente nuevamente en unos minutos o verifique la conexión a internet.";

        return "Revise el detalle del error y contacte soporte técnico si persiste.";
    }

    /// <summary>
    /// Determina si el error es recuperable
    /// </summary>
    public bool EsErrorRecuperable(string codigoError)
    {
        if (string.IsNullOrWhiteSpace(codigoError))
            return false;

        if (_erroresConocidos.TryGetValue(codigoError, out var error))
            return error.Recuperable;

        // Errores de comunicación suelen ser recuperables
        if (codigoError.StartsWith("8"))
            return true;

        // Errores con "timeout" o "conexión" son recuperables
        if (codigoError.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
            codigoError.Contains("conexión", StringComparison.OrdinalIgnoreCase) ||
            codigoError.Contains("connection", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Parsea un mensaje de error de Hacienda
    /// </summary>
    public ErrorHaciendaInfo ParsearMensajeHacienda(string mensajeHacienda)
    {
        var info = new ErrorHaciendaInfo
        {
            Mensaje = mensajeHacienda ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(mensajeHacienda))
        {
            info.CodigoError = "999";
            info.MensajeAmigable = "No se recibió información del error.";
            info.AccionRecomendada = "Intente nuevamente o contacte soporte.";
            return info;
        }

        // Intentar extraer código de error del mensaje
        // Patrones comunes: "Error 301:", "[301]", "Código: 301"
        var patronesCodigo = new[]
        {
            @"Error[:\s]+(\d+)",
            @"\[(\d+)\]",
            @"Código[:\s]+(\d+)",
            @"Code[:\s]+(\d+)",
            @"^(\d+)[:\s]",
            @"-(\d+)[:\s]"
        };

        foreach (var patron in patronesCodigo)
        {
            var match = Regex.Match(mensajeHacienda, patron, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                info.CodigoError = match.Groups[1].Value;
                break;
            }
        }

        // Si no se encontró código, usar 999
        if (string.IsNullOrEmpty(info.CodigoError))
        {
            // Intentar identificar por contenido
            if (mensajeHacienda.Contains("certificado", StringComparison.OrdinalIgnoreCase))
                info.CodigoError = "600";
            else if (mensajeHacienda.Contains("firma", StringComparison.OrdinalIgnoreCase))
                info.CodigoError = "500";
            else if (mensajeHacienda.Contains("consecutivo", StringComparison.OrdinalIgnoreCase))
                info.CodigoError = "401";
            else if (mensajeHacienda.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                info.CodigoError = "801";
            else
                info.CodigoError = "999";
        }

        info.MensajeAmigable = ObtenerMensajeAmigable(info.CodigoError);
        info.AccionRecomendada = ObtenerAccionRecomendada(info.CodigoError);
        info.EsRecuperable = EsErrorRecuperable(info.CodigoError);
        info.DetallesTecnicos = mensajeHacienda;

        // Intentar extraer campo afectado
        var matchCampo = Regex.Match(mensajeHacienda, "campo[:\\s]+['\"]?(\\w+)['\"]?", RegexOptions.IgnoreCase);
        if (matchCampo.Success)
            info.CampoAfectado = matchCampo.Groups[1].Value;

        return info;
    }
}
