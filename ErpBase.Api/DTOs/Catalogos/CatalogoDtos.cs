namespace ErpBase.Api.DTOs.Catalogos;

public sealed record CatalogoItemResponse(
    Guid Id,
    string Nombre);

public sealed record TipoDocumentoIdentidadResponse(
    Guid Id,
    string Nombre,
    string? RegexValidacion);

public sealed record CatalogosResponse(
    IReadOnlyCollection<CatalogoItemResponse> TiposCliente,
    IReadOnlyCollection<TipoDocumentoIdentidadResponse> TiposDocumentoIdentidad,
    IReadOnlyCollection<CatalogoItemResponse> TiposPropiedad,
    IReadOnlyCollection<CatalogoItemResponse> EstadosPropiedad,
    IReadOnlyCollection<CatalogoItemResponse> TiposContrato,
    IReadOnlyCollection<CatalogoItemResponse> EstadosContrato);
