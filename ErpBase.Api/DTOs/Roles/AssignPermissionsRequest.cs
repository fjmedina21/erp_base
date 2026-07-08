using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Roles;

public sealed record AssignPermissionsRequest([Required] IReadOnlyCollection<Guid> PermissionIds);
