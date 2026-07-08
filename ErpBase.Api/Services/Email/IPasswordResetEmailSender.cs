namespace ErpBase.Api.Services.Email;

public interface IPasswordResetEmailSender
{
    Task SendPasswordResetAsync(string email, string resetUrl, DateTime expiresAt, CancellationToken cancellationToken);
}
