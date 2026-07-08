namespace ErpBase.Api.DTOs.Clientes;

public sealed record ClienteHistorialResponse(
    ClienteResponse Cliente,
    IReadOnlyCollection<object> Propiedades,
    IReadOnlyCollection<object> Contratos,
    IReadOnlyCollection<object> Pagos,
    IReadOnlyCollection<string> Observaciones,
    IReadOnlyCollection<ClienteHistorialItemResponse> HistorialModificaciones);
