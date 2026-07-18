namespace ErpBase.Api.Entities;

public sealed class ProyectoTarea
{
    public Guid Id { get; set; }
    public Guid ProyectoFaseId { get; set; }
    public ProyectoFase ProyectoFase { get; set; } = null!;
    public Guid EstadoTareaId { get; set; }
    public EstadoTarea EstadoTarea { get; set; } = null!;
    public Guid PrioridadId { get; set; }
    public PrioridadTarea Prioridad { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public DateOnly? FechaProrroga { get; set; }
    public decimal PorcentajeAvance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
