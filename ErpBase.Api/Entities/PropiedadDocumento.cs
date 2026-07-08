namespace ErpBase.Api.Entities;

public sealed class PropiedadDocumento
{
    public Guid Id { get; set; }
    public Guid PropiedadId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string TipoContenido { get; set; } = string.Empty;
    public string? CloudinaryPublicId { get; set; }
    public string? CloudinaryResourceType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
