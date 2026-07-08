namespace ErpBase.Api.DTOs.Auth;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string Token,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
