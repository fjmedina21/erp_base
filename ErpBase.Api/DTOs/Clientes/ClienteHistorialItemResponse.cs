namespace ErpBase.Api.DTOs.Clientes;

public sealed record ClienteHistorialItemResponse(
    Guid Id,
    string Accion,
    Guid? UserId,
    string Entidad,
    Guid? RegistroId,
    DateTime CreatedAt);
