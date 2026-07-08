using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Users;

public sealed record BlockUserRequest([Required, MaxLength(500)] string Motivo);
