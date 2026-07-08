namespace ErpBase.Api.Services.Storage;

public sealed record CloudinaryUploadResult(
    string Url,
    string PublicId,
    string ResourceType);
