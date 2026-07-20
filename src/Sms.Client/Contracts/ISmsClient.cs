namespace Sms.Client.Contracts;

using Sms.Client.Models;

public interface ISmsClient
{
    Task<IReadOnlyCollection<MenuItem>> GetMenuAsync(
        CancellationToken cancellationToken = default);

    Task SendOrderAsync(
        Order order,
        CancellationToken cancellationToken = default);
}