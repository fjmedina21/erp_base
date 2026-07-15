using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Contratos;

public sealed class CreateContratoRequest
{
    [Required]
    public Guid ClienteId { get; set; }

    [Required]
    public Guid PropiedadId { get; set; }

    [Required]
    public Guid TipoContratoId { get; set; }

    public Guid? EstadoContratoId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    [Required]
    public IFormFile Archivo { get; set; } = null!;

    public string? Observaciones { get; set; }
}

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
