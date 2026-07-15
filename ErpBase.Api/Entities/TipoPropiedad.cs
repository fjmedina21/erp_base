namespace ErpBase.Api.Entities;

public sealed class TipoPropiedad : INamedCatalog
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
