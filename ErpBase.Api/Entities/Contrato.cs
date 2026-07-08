namespace ErpBase.Api.Entities;

public sealed class Contrato
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Guid PropiedadId { get; set; }
    public Guid TipoContratoId { get; set; }
    public TipoContrato TipoContrato { get; set; } = null!;
    public Guid EstadoContratoId { get; set; }
    public EstadoContrato EstadoContrato { get; set; } = null!;
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public string? Observaciones { get; set; }
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
