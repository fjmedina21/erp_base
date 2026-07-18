using System.ComponentModel.DataAnnotations;
using ErpBase.Api.DTOs.Catalogos;

namespace ErpBase.Api.DTOs.Proyectos;

public sealed record CreateProyectoRequest(
    Guid? ClienteId,
    [Required, MaxLength(255)] string Nombre,
    string? Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    DateOnly? FechaProrroga,
    decimal? PorcentajeAvance);

public sealed record UpdateProyectoRequest(
    Guid? ClienteId,
    Guid? EstadoProyectoId,
    [MaxLength(255)] string? Nombre,
    string? Descripcion,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    DateOnly? FechaProrroga,
    decimal? PorcentajeAvance);

public sealed record ProyectoResponse(
    Guid Id,
    string Codigo,
    Guid? ClienteId,
    string? Cliente,
    CatalogoBasicoResponse EstadoProyecto,
    string Nombre,
    string? Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    DateOnly? FechaProrroga,
    decimal PorcentajeAvance,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateFaseRequest(
    [Required, MaxLength(150)] string Nombre,
    string? Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    DateOnly? FechaProrroga,
    decimal? PorcentajeAvance);

public sealed record UpdateFaseRequest(
    Guid? EstadoFaseId,
    [MaxLength(150)] string? Nombre,
    string? Descripcion,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    DateOnly? FechaProrroga,
    decimal? PorcentajeAvance);

public sealed record FaseResponse(
    Guid Id,
    Guid ProyectoId,
    CatalogoBasicoResponse EstadoFase,
    string Nombre,
    string? Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    DateOnly? FechaProrroga,
    decimal PorcentajeAvance,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateTareaRequest(
    [Required] Guid PrioridadId,
    [Required, MaxLength(255)] string Nombre,
    string? Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    DateOnly? FechaProrroga,
    decimal? PorcentajeAvance);

public sealed record UpdateTareaRequest(
    Guid? EstadoTareaId,
    Guid? PrioridadId,
    [MaxLength(255)] string? Nombre,
    string? Descripcion,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    DateOnly? FechaProrroga,
    decimal? PorcentajeAvance);

public sealed record TareaResponse(
    Guid Id,
    Guid ProyectoFaseId,
    CatalogoBasicoResponse EstadoTarea,
    CatalogoBasicoResponse Prioridad,
    string Nombre,
    string? Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    DateOnly? FechaProrroga,
    decimal PorcentajeAvance,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record AsignarResponsablesRequest([Required, MinLength(1)] IReadOnlyCollection<Guid> EmpleadoIds);

public sealed record TareaResponsableResponse(Guid EmpleadoId, string NombreCompleto, string Departamento, string Cargo);
