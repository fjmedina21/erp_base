namespace ErpBase.Api.Entities;

public sealed class Cliente
{
    public Guid Id { get; set; }
    public Guid TipoClienteId { get; set; }
    public TipoCliente TipoCliente { get; set; } = null!;
    public Guid TipoDocumentoIdentidadId { get; set; }
    public TipoDocumentoIdentidad TipoDocumentoIdentidad { get; set; } = null!;
    public string DocumentoIdentidad { get; set; } = string.Empty;
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string? RazonSocial { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
