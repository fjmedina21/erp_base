namespace ErpBase.Api.Entities;

public sealed class ProyectoTareaResponsable
{
    public Guid ProyectoTareaId { get; set; }
    public ProyectoTarea ProyectoTarea { get; set; } = null!;
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
}
