using Microsoft.Extensions.Configuration;

namespace Sms.WpfEnvManager.Services;

public sealed class EnvironmentConfiguration
{
    public EnvironmentConfiguration(IConfiguration configuration)
    {
        VariableNames = configuration
            .GetSection("EnvironmentVariables")
            .Get<string[]>() ?? [];
    }

    public IReadOnlyList<string> VariableNames { get; }
}