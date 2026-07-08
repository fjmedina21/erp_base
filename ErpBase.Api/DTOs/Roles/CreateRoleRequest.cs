using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Roles;

public sealed record CreateRoleRequest(
    [Required, MaxLength(100)] string Nombre,
    string? Descripcion);
