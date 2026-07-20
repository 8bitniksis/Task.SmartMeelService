using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Sms.Client.Configuration;
using Sms.Client.Contracts;
using Sms.Client.Http;

namespace Sms.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddSmsClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SmsClientOptions>(
            configuration.GetSection(SmsClientOptions.SectionName));

        services.AddHttpClient<ISmsClient, HttpSmsClient>();

        return services;
    }
}