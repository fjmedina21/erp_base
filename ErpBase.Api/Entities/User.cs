namespace ErpBase.Api.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public bool Bloqueado { get; set; }
    public string? MotivoBloqueo { get; set; }
    public DateTime? BloqueadoAt { get; set; }
    public DateTime? ReactivadoAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
