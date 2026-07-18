namespace ErpBase.Api.DTOs.Catalogos;

public sealed record CatalogoBasicoResponse(
    Guid Id,
    string Nombre);

public sealed record CatalogosResponse(
    IReadOnlyCollection<CatalogoBasicoResponse> TiposCliente,
    IReadOnlyCollection<CatalogoBasicoResponse> TiposDocumentoIdentidad,
    IReadOnlyCollection<CatalogoBasicoResponse> TiposPropiedad,
    IReadOnlyCollection<CatalogoBasicoResponse> EstadosPropiedad,
    IReadOnlyCollection<CatalogoBasicoResponse> TiposContrato,
    IReadOnlyCollection<CatalogoBasicoResponse> EstadosContrato);
