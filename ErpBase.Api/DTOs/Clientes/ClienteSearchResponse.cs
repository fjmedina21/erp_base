namespace ErpBase.Api.DTOs.Clientes;

public sealed record ClienteSearchResponse(
    IReadOnlyCollection<ClienteResponse> Items,
    int Page,
    int PageSize,
    int Total);
