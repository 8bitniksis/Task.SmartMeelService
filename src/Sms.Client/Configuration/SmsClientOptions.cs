namespace Sms.Client.Configuration;

public sealed class SmsClientOptions
{
    public const string SectionName = "SmsClient";

    public required string BaseUrl { get; init; }

    public required string Login { get; init; }

    public required string Password { get; init; }
}