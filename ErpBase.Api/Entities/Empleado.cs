namespace ErpBase.Api.Entities;

public sealed class Empleado
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public User Usuario { get; set; } = null!;
    public Guid DepartamentoId { get; set; }
    public Departamento Departamento { get; set; } = null!;
    public Guid CargoId { get; set; }
    public Cargo Cargo { get; set; } = null!;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public DateOnly? FechaSalida { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
