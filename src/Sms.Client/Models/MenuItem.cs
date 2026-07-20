namespace Sms.Client.Models;

public sealed class MenuItem
{
    public required string Id { get; init; }

    public required string Article { get; init; }

    public required string Name { get; init; }

    public decimal Price { get; init; }

    public bool IsWeighted { get; init; }

    public required string FullPath { get; init; }

    public IReadOnlyCollection<string> Barcodes { get; init; } = [];
}