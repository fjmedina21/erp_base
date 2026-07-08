namespace ErpBase.Api.Entities;

public sealed class Propiedad
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public Guid TipoPropiedadId { get; set; }
    public TipoPropiedad TipoPropiedad { get; set; } = null!;
    public Guid EstadoPropiedadId { get; set; }
    public EstadoPropiedad EstadoPropiedad { get; set; } = null!;
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public decimal MetrosCuadradosTerreno { get; set; }
    public decimal? MetrosCuadradosConstruccion { get; set; }
    public int? Habitaciones { get; set; }
    public int? Banos { get; set; }
    public decimal? PrecioVenta { get; set; }
    public decimal? PrecioAlquiler { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
