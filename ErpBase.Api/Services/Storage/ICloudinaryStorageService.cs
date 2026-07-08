namespace ErpBase.Api.Services.Storage;

public interface ICloudinaryStorageService
{
    Task<CloudinaryUploadResult> UploadAsync(
        IFormFile file,
        string folder,
        CloudinaryResourceType resourceType,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string publicId,
        string resourceType,
        CancellationToken cancellationToken);
}
