using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO que representa el resultado del envío de un correo electrónico
/// Incluye información de éxito, errores y destinatarios
/// </summary>
public class ResultadoEnvioEmailDTO
{
    [Display(Name = "Exitoso")]
    public bool Exitoso { get; set; }

    [Display(Name = "Mensaje")]
    public string Mensaje { get; set; } = null!;

    [Display(Name = "Fecha de Envío")]
    public DateTime? FechaEnvio { get; set; }

    [Display(Name = "Emails Enviados")]
    public List<string> EmailsEnviados { get; set; } = new List<string>();

    [Display(Name = "Errores")]
    public List<string> Errores { get; set; } = new List<string>();
}
