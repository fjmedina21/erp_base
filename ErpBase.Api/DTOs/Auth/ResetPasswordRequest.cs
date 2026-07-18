using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Auth;

public sealed record ResetPasswordRequest(
    [Required, MinLength(8), MaxLength(100)] string NewPassword,
    [Required, MinLength(8), MaxLength(100)] string ConfirmPassword);
