namespace ReportForge.Domain.Models;

public sealed record TransactionRecord
{
    public required DateOnly Date { get; init; }
    public required string Category { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
}
