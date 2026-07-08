namespace ErpBase.Api.Options;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    public int ExpirationMinutes { get; set; } = 30;
    public string ResetUrlBase { get; set; } = "http://localhost:5080/reset-password";
}
