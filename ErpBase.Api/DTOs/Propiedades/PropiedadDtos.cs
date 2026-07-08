using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ErpBase.Api.DTOs.Propiedades;

public sealed record CreatePropiedadRequest(
    [Required, MaxLength(50)] string Codigo,
    [Required] Guid TipoPropiedadId,
    [Required] Guid EstadoPropiedadId,
    [Required, MaxLength(255)] string Titulo,
    string? Descripcion,
    decimal Latitud,
    decimal Longitud,
    [Required, MaxLength(1000)] string Direccion,
    decimal MetrosCuadradosTerreno,
    decimal? MetrosCuadradosConstruccion,
    int? Habitaciones,
    int? Banos,
    decimal? PrecioVenta,
    decimal? PrecioAlquiler);

public sealed record UpdatePropiedadRequest(
    Guid? TipoPropiedadId,
    Guid? EstadoPropiedadId,
    [MaxLength(255)] string? Titulo,
    string? Descripcion,
    decimal? Latitud,
    decimal? Longitud,
    [MaxLength(1000)] string? Direccion,
    decimal? MetrosCuadradosTerreno,
    decimal? MetrosCuadradosConstruccion,
    int? Habitaciones,
    int? Banos,
    decimal? PrecioVenta,
    decimal? PrecioAlquiler,
    string? ObservacionCambio);

public sealed record PropiedadResponse(
    Guid Id,
    string Codigo,
    Guid TipoPropiedadId,
    string TipoPropiedad,
    Guid EstadoPropiedadId,
    string EstadoPropiedad,
    string Titulo,
    string? Descripcion,
    decimal Latitud,
    decimal Longitud,
    string Direccion,
    decimal MetrosCuadradosTerreno,
    decimal? MetrosCuadradosConstruccion,
    int? Habitaciones,
    int? Banos,
    decimal? PrecioVenta,
    decimal? PrecioAlquiler,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed class PropiedadDocumentoUploadRequest
{
    [Required]
    public IFormFile Archivo { get; set; } = null!;
}

public sealed class PropiedadFotografiaUploadRequest
{
    [Required]
    public IFormFile Archivo { get; set; } = null!;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public bool EsPrincipal { get; set; }
}

public sealed record PropiedadDocumentoResponse(
    Guid Id,
    Guid PropiedadId,
    string NombreArchivo,
    string RutaArchivo,
    string TipoContenido,
    string? CloudinaryPublicId,
    string? CloudinaryResourceType,
    DateTime CreatedAt);

public sealed record PropiedadFotografiaResponse(
    Guid Id,
    Guid PropiedadId,
    string NombreArchivo,
    string RutaArchivo,
    string? TipoContenido,
    string? Descripcion,
    bool? EsPrincipal,
    string? CloudinaryPublicId,
    string? CloudinaryResourceType,
    DateTime CreatedAt);

public sealed record InsertPropiedadInversionistaRequest(
    [Required] Guid ClienteId,
    decimal PorcentajeParticipacion,
    string? Observaciones);

public sealed record UpdatePropiedadInversionistaRequest(
    decimal PorcentajeParticipacion,
    string? Observaciones);

public sealed record PropiedadInversionistaResponse(
    Guid PropiedadId,
    Guid ClienteId,
    string NombreCliente,
    decimal PorcentajeParticipacion,
    string? Observaciones,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
