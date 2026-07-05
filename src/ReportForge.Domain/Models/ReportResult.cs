namespace ReportForge.Domain.Models;

public sealed record ReportResult
{
    public required int RecordCount { get; init; }
    public required decimal TotalAmount { get; init; }
    public required decimal AverageAmount { get; init; }
    public required decimal MinAmount { get; init; }
    public required decimal MaxAmount { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required IReadOnlyList<CategorySummary> CategorySummaries { get; init; }
    public required IReadOnlyList<CategorySummary> TopCategories { get; init; }
}
