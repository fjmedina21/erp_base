namespace ErpBase.Api.Entities;

public sealed class TipoPropiedad
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
