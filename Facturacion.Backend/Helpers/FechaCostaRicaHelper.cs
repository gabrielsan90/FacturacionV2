using System;

namespace Facturacion.Backend.Helpers;

internal static class FechaCostaRicaHelper
{
    private const string ZonaHorariaWindows = "Central America Standard Time";
    private const string ZonaHorariaIana = "America/Costa_Rica";

    internal static DateTime ObtenerAhoraCostaRica()
    {
        var zona = ObtenerZonaHorariaCostaRica();
        return TimeZoneInfo.ConvertTime(DateTime.UtcNow, zona);
    }

    internal static DateTimeOffset AsignarOffsetCostaRica(DateTime fechaCostaRica)
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
