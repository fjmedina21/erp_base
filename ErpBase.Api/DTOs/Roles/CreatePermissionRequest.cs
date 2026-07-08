using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Roles;

public sealed record CreatePermissionRequest(
    [Required, MaxLength(50)] string Codigo,
    [Required, MaxLength(100)] string Nombre,
    string? Descripcion);
