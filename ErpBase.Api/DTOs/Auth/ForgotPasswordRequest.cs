using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Auth;

public sealed record ForgotPasswordRequest([Required, EmailAddress, MaxLength(255)] string Email);
