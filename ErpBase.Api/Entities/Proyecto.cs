namespace ErpBase.Api.Entities;

public sealed class Proyecto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid EstadoProyectoId { get; set; }
    public EstadoProyecto EstadoProyecto { get; set; } = null!;
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
