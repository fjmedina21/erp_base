namespace ErpBase.Api.DTOs.Roles;

public sealed record PermissionResponse(
    Guid Id,
    string Codigo,
    string Modulo,
    string Nombre,
    string? Descripcion,
    DateTime CreatedAt);
