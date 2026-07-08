namespace ErpBase.Api.Entities;

public sealed class Auditoria
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public Guid? RegistroId { get; set; }
    public DateTime Fecha { get; set; }
}
