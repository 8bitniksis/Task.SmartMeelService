using System.Text.Json;

namespace Sms.Client.Http;

internal static class JsonSettings
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true
    };
}