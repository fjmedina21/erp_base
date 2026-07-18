using System.Net;
using System.Net.Mail;
using ErpBase.Api.Options;
using Microsoft.Extensions.Options;

namespace ErpBase.Api.Services.Email;

public sealed class SmtpPasswordResetEmailSender(IOptions<SmtpEmailOptions> options) : IPasswordResetEmailSender
{
    public async Task SendPasswordResetAsync(string email, string resetUrl, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var smtpOptions = GetConfiguredOptions();

        using var message = new MailMessage
        {
            From = new MailAddress(smtpOptions.FromEmail, smtpOptions.FromName),
            Subject = "Recuperación de contraseña - ERP Base",
            Body = BuildBody(resetUrl, expiresAt),
            IsBodyHtml = true
        };
        message.To.Add(email);

        using var client = new SmtpClient(smtpOptions.Host, smtpOptions.Port)
        {
            EnableSsl = smtpOptions.EnableSsl,
            Credentials = new NetworkCredential(smtpOptions.UserName, smtpOptions.Password)
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    private SmtpEmailOptions GetConfiguredOptions()
    {
        var smtpOptions = options.Value;
        if (string.IsNullOrWhiteSpace(smtpOptions.Host) ||
            string.IsNullOrWhiteSpace(smtpOptions.UserName) ||
            string.IsNullOrWhiteSpace(smtpOptions.Password) ||
            string.IsNullOrWhiteSpace(smtpOptions.FromEmail))
        {
            throw new InvalidOperationException("SMTP no está configurado. Configure Email:Smtp:Host, UserName, Password y FromEmail.");
        }

        return smtpOptions;
    }

    private static string BuildBody(string resetUrl, DateTime expiresAt) =>
        $$"""
        <p>Hola,</p>
        <p>Recibimos una solicitud para restablecer tu contraseña en ERP Base.</p>
        <p><a href="{{WebUtility.HtmlEncode(resetUrl)}}">Restablecer contraseña</a></p>
        <p>Este enlace expira el {{expiresAt:yyyy-MM-dd HH:mm}}.</p>
        <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
        """;
}
