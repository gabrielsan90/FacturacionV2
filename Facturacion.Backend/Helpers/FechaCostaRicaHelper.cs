using System;

namespace Facturacion.Backend.Helpers;

/// <summary>
/// Helper para obtener fechas en zona horaria de Costa Rica (UTC-6)
/// </summary>
public static class FechaCostaRicaHelper
{
    private const string ZonaHorariaWindows = "Central America Standard Time";
    private const string ZonaHorariaIana = "America/Costa_Rica";

    /// <summary>
    /// Obtiene la fecha y hora actual en Costa Rica
    /// </summary>
    public static DateTime Ahora => ObtenerAhoraCostaRica();

    /// <summary>
    /// Obtiene solo la fecha actual en Costa Rica (sin hora)
    /// </summary>
    public static DateTime Hoy => ObtenerAhoraCostaRica().Date;

    public static DateTime ObtenerAhoraCostaRica()
    {
        var zona = ObtenerZonaHorariaCostaRica();
        return TimeZoneInfo.ConvertTime(DateTime.UtcNow, zona);
    }

    public static DateTimeOffset AsignarOffsetCostaRica(DateTime fechaCostaRica)
    {
        var zona = ObtenerZonaHorariaCostaRica();
        var offset = zona.GetUtcOffset(fechaCostaRica);
        return new DateTimeOffset(fechaCostaRica, offset);
    }

    private static TimeZoneInfo ObtenerZonaHorariaCostaRica()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(ZonaHorariaWindows);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(ZonaHorariaIana);
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(ZonaHorariaIana);
        }
    }
}
