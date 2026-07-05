using ReportForge.Domain.Models;

namespace ReportForge.Domain.Interfaces;

public interface IReportGenerator
{
    ReportResult Generate(IReadOnlyList<TransactionRecord> records, int topCategoryCount = 3);
}
