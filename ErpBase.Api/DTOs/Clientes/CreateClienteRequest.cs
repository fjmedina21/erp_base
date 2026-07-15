using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Clientes;

public sealed record CreateClienteRequest(
    [Required] Guid TipoClienteId,
    [Required] Guid TipoDocumentoIdentidadId,
    [Required, MaxLength(20)] string Documento,
    [MaxLength(150)] string? Nombres,
    [MaxLength(150)] string? Apellidos,
    [MaxLength(250)] string? RazonSocial,
    [EmailAddress, MaxLength(255)] string? Email,
    [MaxLength(50)] string? Telefono,
    string? Direccion);
