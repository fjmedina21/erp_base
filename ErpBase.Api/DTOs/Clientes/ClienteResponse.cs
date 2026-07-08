namespace ErpBase.Api.DTOs.Clientes;

public sealed record ClienteResponse(
    Guid Id,
    Guid TipoClienteId,
    string TipoCliente,
    Guid TipoDocumentoIdentidadId,
    string TipoDocumento,
    string Documento,
    string? Nombres,
    string? Apellidos,
    string? RazonSocial,
    string NombreCompleto,
    string? Email,
    string? Telefono,
    string? Direccion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
