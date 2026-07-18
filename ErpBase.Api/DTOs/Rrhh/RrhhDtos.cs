using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Rrhh;

public sealed record CreateCatalogoRequest([Required, MaxLength(100)] string Nombre);

public sealed record UpdateCatalogoRequest([Required, MaxLength(100)] string Nombre);

public sealed record CatalogoAdminResponse(Guid Id, string Nombre, DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record CreateEmpleadoRequest(
    [Required] Guid UsuarioId,
    [Required] Guid DepartamentoId,
    [Required] Guid CargoId,
    [Required, MaxLength(150)] string Nombres,
    [Required, MaxLength(150)] string Apellidos,
    [MaxLength(50)] string? Telefono,
    DateOnly FechaIngreso,
    DateOnly? FechaSalida);

public sealed record UpdateEmpleadoRequest(
    Guid? DepartamentoId,
    Guid? CargoId,
    [MaxLength(150)] string? Nombres,
    [MaxLength(150)] string? Apellidos,
    [MaxLength(50)] string? Telefono,
    DateOnly? FechaIngreso,
    DateOnly? FechaSalida);

public sealed record EmpleadoResponse(
    Guid Id,
    Guid UsuarioId,
    string UsuarioEmail,
    Guid DepartamentoId,
    string Departamento,
    Guid CargoId,
    string Cargo,
    string Nombres,
    string Apellidos,
    string? Telefono,
    DateOnly FechaIngreso,
    DateOnly? FechaSalida,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
