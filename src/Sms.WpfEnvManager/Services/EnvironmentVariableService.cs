using Serilog;
using Sms.WpfEnvManager.Models;

namespace Sms.WpfEnvManager.Services;

public sealed class EnvironmentVariableService : IEnvironmentVariableService
{
    private readonly EnvironmentConfiguration _configuration;
    private readonly ILogger _logger;

    public EnvironmentVariableService(
        EnvironmentConfiguration configuration,
        ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public IReadOnlyCollection<EnvVariableItem> Load()
    {
        var result = new List<EnvVariableItem>();

        foreach (var variableName in _configuration.VariableNames)
        {
            var value = Environment.GetEnvironmentVariable(
                variableName,
                EnvironmentVariableTarget.User);

            if (value is null)
            {
                value = string.Empty;

                Environment.SetEnvironmentVariable(
                    variableName,
                    value,
                    EnvironmentVariableTarget.User);

                _logger.Information(
                    "Environment variable '{Variable}' created.",
                    variableName);
            }

            result.Add(new EnvVariableItem
            {
                Name = variableName,
                Value = value,
                Comment = "User Environment"
            });
        }

        _logger.Information(
            "Loaded {Count} variables.",
            result.Count);

        return result;
    }

    public int Save(IEnumerable<EnvVariableItem> variables)
    {
        var changed = 0;

        foreach (var item in variables)
        {
            var current = Environment.GetEnvironmentVariable(
                item.Name,
                EnvironmentVariableTarget.User) ?? string.Empty;

            if (current == item.Value)
                continue;

            Environment.SetEnvironmentVariable(
                item.Name,
                item.Value,
                EnvironmentVariableTarget.User);

            item.Comment = $"Updated {DateTime.Now:G}";

            _logger.Information(
                "Variable '{Variable}' changed from '{Old}' to '{New}'",
                item.Name,
                current,
                item.Value);

            changed++;
        }

        return changed;
    }
}