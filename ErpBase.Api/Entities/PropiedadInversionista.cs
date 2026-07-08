namespace ErpBase.Api.Entities;

public sealed class PropiedadInversionista
{
    public Guid PropiedadId { get; set; }
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public decimal PorcentajeParticipacion { get; set; }
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
