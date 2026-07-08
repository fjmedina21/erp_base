using System.Security.Cryptography;
using System.Text;

namespace ErpBase.Api.Services.PasswordReset;

public sealed class PasswordResetTokenService : IPasswordResetTokenService
{
    public string GenerateToken()
    {
        return Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
