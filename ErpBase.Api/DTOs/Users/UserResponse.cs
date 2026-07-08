namespace ErpBase.Api.DTOs.Users;

public sealed record UserResponse(
    Guid Id,
    string Email,
    bool Activo,
    bool Bloqueado,
    string? MotivoBloqueo,
    DateTime? BloqueadoAt,
    IReadOnlyCollection<string> Roles,
    DateTime CreatedAt);
