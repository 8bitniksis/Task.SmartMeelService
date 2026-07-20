using System.Net.Http.Headers;
using System.Text;

using Microsoft.Extensions.Options;

using Sms.Client.Configuration;
using Sms.Client.Contracts;
using Sms.Client.Models;

namespace Sms.Client.Http;

internal sealed class HttpSmsClient : ISmsClient
{
    private readonly HttpClient _httpClient;

    public HttpSmsClient(
        HttpClient httpClient,
        IOptions<SmsClientOptions> options)
    {
        _httpClient = httpClient;

        var settings = options.Value;

        _httpClient.BaseAddress = new Uri(settings.BaseUrl);

        var credentials =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{settings.Login}:{settings.Password}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    public Task SendOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    Task<IReadOnlyCollection<MenuItem>> ISmsClient.GetMenuAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}