namespace ErpBase.Api.DTOs.Auth;

public sealed record ForgotPasswordResponse(
    string Message,
    string? ResetToken = null,
    string? ResetUrl = null,
    string? ExpiresAt = null);
