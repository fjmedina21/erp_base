using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Auth;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password);
