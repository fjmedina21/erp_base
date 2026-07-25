using Microsoft.AspNetCore.Authorization;

namespace ErpBase.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public HasPermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = PermissionPolicy;
    }

    public string Permission { get; }
    public string PermissionPolicy => $"{PolicyPrefix}{Permission}";
}
