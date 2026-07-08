using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Users;

public sealed record UpdateUserStatusRequest([Required] bool Activo);
