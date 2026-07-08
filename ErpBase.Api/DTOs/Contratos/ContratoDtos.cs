using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Contratos;

public sealed record CreateContratoRequest(
    [Required] Guid ClienteId,
    [Required] Guid PropiedadId,
    [Required] Guid TipoContratoId,
    Guid? EstadoContratoId,
    DateOnly FechaInicio,
    DateOnly? FechaFin,
    [Required, MaxLength(1000)] string RutaArchivo,
    string? Observaciones);

public sealed record ContratoResponse(
    Guid Id,
    string Codigo,
    Guid ClienteId,
    Guid PropiedadId,
    string TipoContrato,
    string EstadoContrato,
    DateOnly FechaInicio,
    DateOnly? FechaFin,
    string RutaArchivo,
    string? Observaciones,
    DateTime CreatedAt);
