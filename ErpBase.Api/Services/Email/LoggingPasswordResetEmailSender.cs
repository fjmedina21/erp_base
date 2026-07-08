namespace ErpBase.Api.Services.Email;

public sealed class LoggingPasswordResetEmailSender(ILogger<LoggingPasswordResetEmailSender> logger) : IPasswordResetEmailSender
{
    public Task SendPasswordResetAsync(string email, string resetUrl, DateTime expiresAt, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Password reset link generated for {Email}. ExpiresAt: {ExpiresAt}. ResetUrl: {ResetUrl}",
            email,
            expiresAt,
            resetUrl);

        // implementar send email

        return Task.CompletedTask;
    }
}
