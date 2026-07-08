namespace ErpBase.Api.Services.PasswordReset;

public interface IPasswordResetTokenService
{
    string GenerateToken();
    string HashToken(string token);
}
