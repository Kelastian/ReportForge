namespace ReportForge.Domain.Models;

public sealed record CategorySummary
{
    public required string Category { get; init; }
    public required decimal Total { get; init; }
    public required int Count { get; init; }
}
