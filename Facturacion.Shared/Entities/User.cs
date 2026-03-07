using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Facturacion.Shared.Entities;

public class User : IdentityUser
{
    [Display(Name = "Nombre Completo")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string FullName { get; set; } = null!;

    [Display(Name = "Documento")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Document { get; set; } = null!;

    [Display(Name = "Última Conexión")]
    public DateTime? UltimaConexion { get; set; }

    // =========================================================================
    // Security: Override inherited IdentityUser properties with [JsonIgnore]
    // to prevent serialization of sensitive data when User is included as a
    // navigation property (e.g., CreadoPor, ModificadoPor, AprobadoPor).
    // Without these, the JSON serializer exposes password hashes, security
    // stamps, and other sensitive fields to API consumers.
    // =========================================================================

    [JsonIgnore]
    public override string? PasswordHash
    {
        get => base.PasswordHash;
        set => base.PasswordHash = value;
    }

    [JsonIgnore]
    public override string? SecurityStamp
    {
        get => base.SecurityStamp;
        set => base.SecurityStamp = value;
    }

    [JsonIgnore]
    public override string? ConcurrencyStamp
    {
        get => base.ConcurrencyStamp;
        set => base.ConcurrencyStamp = value;
    }

    [JsonIgnore]
    public override bool TwoFactorEnabled
    {
        get => base.TwoFactorEnabled;
        set => base.TwoFactorEnabled = value;
    }

    [JsonIgnore]
    public override bool PhoneNumberConfirmed
    {
        get => base.PhoneNumberConfirmed;
        set => base.PhoneNumberConfirmed = value;
    }

    [JsonIgnore]
    public override bool EmailConfirmed
    {
        get => base.EmailConfirmed;
        set => base.EmailConfirmed = value;
    }

    [JsonIgnore]
    public override int AccessFailedCount
    {
        get => base.AccessFailedCount;
        set => base.AccessFailedCount = value;
    }

    [JsonIgnore]
    public override DateTimeOffset? LockoutEnd
    {
        get => base.LockoutEnd;
        set => base.LockoutEnd = value;
    }

    [JsonIgnore]
    public override bool LockoutEnabled
    {
        get => base.LockoutEnabled;
        set => base.LockoutEnabled = value;
    }

    [JsonIgnore]
    public override string? NormalizedEmail
    {
        get => base.NormalizedEmail;
        set => base.NormalizedEmail = value;
    }

    [JsonIgnore]
    public override string? NormalizedUserName
    {
        get => base.NormalizedUserName;
        set => base.NormalizedUserName = value;
    }
}
