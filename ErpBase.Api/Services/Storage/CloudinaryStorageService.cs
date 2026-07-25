using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ErpBase.Api.Options;
using Microsoft.Extensions.Options;

namespace ErpBase.Api.Services.Storage;

public sealed class CloudinaryStorageService(HttpClient httpClient, IOptions<CloudinaryOptions> options) : ICloudinaryStorageService
{
    public async Task<CloudinaryUploadResult> UploadAsync(
        IFormFile file,
        string folder,
        CloudinaryResourceType resourceType,
        CancellationToken cancellationToken)
    {
        var cloudinaryOptions = GetConfiguredOptions();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var effectiveFolder = CombineFolder(cloudinaryOptions.Folder, folder);
        var publicId = BuildPublicId(file.FileName, resourceType);
        var parameters = new SortedDictionary<string, string>
        {
            ["folder"] = effectiveFolder,
            ["public_id"] = publicId,
            ["timestamp"] = timestamp
        };

        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        }

        var resourcePath = ToCloudinaryResourceType(resourceType);
        var query = BuildQueryString(new Dictionary<string, string>
        {
            ["api_key"] = cloudinaryOptions.ApiKey,
            ["timestamp"] = timestamp,
            ["folder"] = effectiveFolder,
            ["public_id"] = publicId,
            ["signature"] = Sign(parameters, cloudinaryOptions.ApiSecret)
        });
        form.Add(fileContent, "file", file.FileName);

        var endpoint = $"https://api.cloudinary.com/v1_1/{cloudinaryOptions.CloudName}/{resourcePath}/upload?{query}";
        using var response = await httpClient.PostAsync(endpoint, form, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Cloudinary no pudo subir el archivo. Respuesta: {body}");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        return new CloudinaryUploadResult(
            root.GetProperty("secure_url").GetString() ?? string.Empty,
            root.GetProperty("public_id").GetString() ?? string.Empty,
            root.GetProperty("resource_type").GetString() ?? resourcePath);
    }

    public async Task DeleteAsync(string publicId, string resourceType, CancellationToken cancellationToken)
    {
        var cloudinaryOptions = GetConfiguredOptions();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var parameters = new SortedDictionary<string, string>
        {
            ["public_id"] = publicId,
            ["timestamp"] = timestamp
        };

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["public_id"] = publicId,
            ["api_key"] = cloudinaryOptions.ApiKey,
            ["timestamp"] = timestamp,
            ["signature"] = Sign(parameters, cloudinaryOptions.ApiSecret)
        });

        var endpoint = $"https://api.cloudinary.com/v1_1/{cloudinaryOptions.CloudName}/{resourceType}/destroy";
        using var response = await httpClient.PostAsync(endpoint, form, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Cloudinary no pudo eliminar el archivo. Respuesta: {body}");
        }
    }

    private CloudinaryOptions GetConfiguredOptions()
    {
        var cloudinaryOptions = options.Value;
        if (string.IsNullOrWhiteSpace(cloudinaryOptions.CloudName) ||
            string.IsNullOrWhiteSpace(cloudinaryOptions.ApiKey) ||
            string.IsNullOrWhiteSpace(cloudinaryOptions.ApiSecret))
        {
            throw new InvalidOperationException("Cloudinary no esta configurado. Configure Cloudinary:CloudName, Cloudinary:ApiKey y Cloudinary:ApiSecret.");
        }

        return cloudinaryOptions;
    }

    private static string ToCloudinaryResourceType(CloudinaryResourceType resourceType) =>
        resourceType == CloudinaryResourceType.Image ? "image" : "raw";

    private static string BuildPublicId(string fileName, CloudinaryResourceType resourceType)
    {
        var extension = resourceType == CloudinaryResourceType.Raw
            ? Path.GetExtension(fileName).Trim().ToLowerInvariant()
            : string.Empty;

        var name = Path.GetFileNameWithoutExtension(fileName).Trim().ToLowerInvariant();
        name = Regex.Replace(name, @"[^a-z0-9_-]+", "-", RegexOptions.CultureInvariant);
        name = name.Trim('-');

        if (string.IsNullOrWhiteSpace(name))
        {
            name = "archivo";
        }

        return $"{name}-{Guid.NewGuid():N}{extension}";
    }

    private static string CombineFolder(string rootFolder, string folder)
    {
        var root = rootFolder.Trim().Trim('/');
        var child = folder.Trim().Trim('/');
        if (string.IsNullOrWhiteSpace(root)) return child;
        if (string.IsNullOrWhiteSpace(child)) return root;
        return $"{root}/{child}";
    }

    private static string Sign(SortedDictionary<string, string> parameters, string apiSecret)
    {
        var signatureBase = string.Join('&', parameters.Select(x => $"{x.Key}={x.Value}")) + apiSecret;
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(signatureBase));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string BuildQueryString(IReadOnlyDictionary<string, string> parameters) =>
        string.Join('&', parameters.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
}
