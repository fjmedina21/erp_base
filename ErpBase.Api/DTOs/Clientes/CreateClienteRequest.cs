using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Clientes;

public sealed record CreateClienteRequest(
    [Required, MaxLength(50)] string TipoCliente,
    [Required, MaxLength(20)] string TipoDocumento,
    [Required, MaxLength(20)] string Documento,
    [MaxLength(150)] string? Nombres,
    [MaxLength(150)] string? Apellidos,
    [MaxLength(250)] string? RazonSocial,
    [EmailAddress, MaxLength(255)] string? Email,
    [MaxLength(50)] string? Telefono,
    string? Direccion);
