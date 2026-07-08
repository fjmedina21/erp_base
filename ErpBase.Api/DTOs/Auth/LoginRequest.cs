using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required] string Password);
