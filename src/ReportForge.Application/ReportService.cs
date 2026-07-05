using ReportForge.Domain.Exceptions;
using ReportForge.Domain.Interfaces;
using ReportForge.Domain.Models;

namespace ReportForge.Application;

public sealed class ReportService : IReportGenerator
{
    public ReportResult Generate(IReadOnlyList<TransactionRecord> records, int topCategoryCount = 3)
    {
        if (records is null || records.Count == 0)
        {
            throw new DataParsingException("Cannot generate a report from an empty dataset.");
        }

        if (topCategoryCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(topCategoryCount), topCategoryCount, "Top category count must be greater than zero.");
        }

        var amounts = records.Select(r => r.Amount).ToList();

        var categorySummaries = records
            .GroupBy(r => r.Category)
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                Total = g.Sum(r => r.Amount),
                Count = g.Count()
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        return new ReportResult
        {
            RecordCount = records.Count,
            TotalAmount = amounts.Sum(),
            AverageAmount = amounts.Average(),
            MinAmount = amounts.Min(),
            MaxAmount = amounts.Max(),
            StartDate = records.Min(r => r.Date),
            EndDate = records.Max(r => r.Date),
            CategorySummaries = categorySummaries,
            TopCategories = categorySummaries.Take(topCategoryCount).ToList()
        };
    }
}
