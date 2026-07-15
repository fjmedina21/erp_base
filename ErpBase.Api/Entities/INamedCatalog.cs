namespace ErpBase.Api.Entities;

public interface INamedCatalog
{
    string Nombre { get; set; }
    DateTime? DeletedAt { get; set; }
}
