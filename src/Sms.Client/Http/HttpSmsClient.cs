using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sms.Client.Configuration;
using Sms.Client.Contracts;
using Sms.Client.Exceptions;
using Sms.Client.Http.Requests;
using Sms.Client.Http.Responses;
using Sms.Client.Models;

namespace Sms.Client.Http;

internal sealed class HttpSmsClient : ISmsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpSmsClient> _logger;

    public HttpSmsClient(
        HttpClient httpClient,
        IOptions<SmsClientOptions> options,
        ILogger<HttpSmsClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var settings = options.Value;

        _httpClient.BaseAddress = new Uri(settings.BaseUrl);

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{settings.Login}:{settings.Password}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    private async Task<T> SendAsync<T>(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "HTTP {Method} {Uri}",
            request.Method,
            request.RequestUri);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        var apiResponse =
            await JsonSerializer.DeserializeAsync<ApiResponse<T>>(
                stream,
                JsonSettings.Default,
                cancellationToken);

        if (apiResponse is null)
            throw new SmsApiException("Server returned an empty response.");

        if (!apiResponse.Success)
            throw new SmsApiException(
                apiResponse.ErrorMessage ?? "Unknown server error.");

        if (apiResponse.Data is null)
            throw new SmsApiException("Response does not contain data.");

        return apiResponse.Data;
    }

    private static MenuItem Map(MenuItemResponse dto)
    {
        return new MenuItem
        {
            Id = dto.Id,
            Article = dto.Article,
            Name = dto.Name,
            Price = dto.Price,
            IsWeighted = dto.IsWeighted,
            FullPath = dto.FullPath,
            Barcodes = dto.Barcodes
        };
    }

    public async Task SendOrderAsync(
    Order order,
    CancellationToken cancellationToken = default)
    {
        var requestModel = new OrderRequest
        {
            Id = order.Id,
            Items = order.Items
                .Select(OrderItemRequest.From)
                .ToList()
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "order");

        request.Content = JsonContent.Create(requestModel);

        await SendAsync<object>(
            request,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<MenuItem>> GetMenuAsync(
    CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "menu");

        var menu =
            await SendAsync<List<MenuItemResponse>>(
                request,
                cancellationToken);

        return menu
            .Select(Map)
            .ToList();
    }
}