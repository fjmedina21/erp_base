namespace ErpBase.Api.DTOs.Roles;

public sealed record RoleResponse(
    Guid Id,
    string Nombre,
    string? Descripcion,
    IReadOnlyCollection<string> Permisos,
    DateTime CreatedAt);
