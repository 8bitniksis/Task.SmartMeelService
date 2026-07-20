using Sms.WpfEnvManager.Models;

namespace Sms.WpfEnvManager.Services;

public interface IEnvironmentVariableService
{
    IReadOnlyCollection<EnvVariableItem> Load();

    int Save(IEnumerable<EnvVariableItem> variables);
}