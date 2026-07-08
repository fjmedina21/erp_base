namespace ErpBase.Api.Entities;

public sealed class PropiedadImagen
{
    public Guid Id { get; set; }
    public Guid PropiedadId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CloudinaryPublicId { get; set; }
    public string? CloudinaryResourceType { get; set; }
    public string? Descripcion { get; set; }
    public bool EsPrincipal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
