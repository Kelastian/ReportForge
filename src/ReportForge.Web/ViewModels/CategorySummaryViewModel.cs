namespace ReportForge.Web.ViewModels;

public sealed class CategorySummaryViewModel
{
    public required string Category { get; init; }
    public required decimal Total { get; init; }
    public required int Count { get; init; }
    public required decimal PercentageOfTotal { get; init; }
}
