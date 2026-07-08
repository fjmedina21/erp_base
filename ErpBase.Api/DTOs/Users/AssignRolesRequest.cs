using System.ComponentModel.DataAnnotations;

namespace ErpBase.Api.DTOs.Users;

public sealed record AssignRolesRequest([Required] IReadOnlyCollection<Guid> RoleIds);
